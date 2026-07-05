using Core.DTOs;
using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.DTOs.ValidationReport;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Core.Service.Interface.AI;
using Infrastructure.GrpcClients.Team;
using RecoMind.Contracts.Events;
using System.Text.Json;

namespace Core.Service;

public class ValidationReportService(IValidationReportGeneratorService reportGeneratorService,
                                     IUnitOfWork<ValidationReport> unitOfWork,
                                     ITeamGrpcClient teamGrpcClient,
                                     IFileStorageService fileStorageService,
                                     IBackgroundService backgroundService,
                                     IPlanEventPublisher eventPublisher) : IValidationReportService
{
    private readonly IGenericRepository<ValidationReport> _validationReportRepository = unitOfWork.Entity;
    public async Task<Result<AIValidationReportResponseDto>> RequestValidationReport(UserValidationReportRequestDto requestDto)
    {
        //var TeamId = "0dc1400d-a758-424b-80fb-a8ff89078522"; // FOR TEST ------------------------------

        var checkTeamId = await teamGrpcClient.GetTeamNameById(requestDto.UserId);  //Check if the user is part of a team and get the team id
        if (!checkTeamId.IsSuccess)
            return Result<AIValidationReportResponseDto>.Failure(checkTeamId.Error);

        var aiRequest = new AIValidationReportRequestDto
        {
            CompanyId = requestDto.CompanyId,
            TeamId = checkTeamId.Value,
            UserRequest = requestDto.UserRequest
        };
        var response = await reportGeneratorService.GenerateValidationReport(aiRequest);
        return response;
    }
    public async Task<Result<ValidationReportDto>> GetValidationReport(string taskId)
    {
        var response = await reportGeneratorService.GetValidationReport(taskId);

        if (!response.IsSuccess)
            return Result<ValidationReportDto>.Failure(response.Error);

        var validationReport = response.Value.Result.ValidationReport;
        return Result<ValidationReportDto>.Success(validationReport);
    }

    public async Task<Result<UserValidationReportDto>> AddValidationReport(UserValidationReportAddDto reportAddDto)
    {
        var stringContent = JsonSerializer.Serialize(reportAddDto.Content);
        var fileName = await fileStorageService.SaveFileAsync(stringContent!);
        var report = new ValidationReport
        {
            Id = Guid.NewGuid().ToString(),
            Status = (ValidationReportStatusEnum)reportAddDto.Status,
            FileType = ".txt",
            FileName = fileName,
            CreatedBy = reportAddDto.CreatedBy!,
            CreatedAt = DateTime.UtcNow,
            UserSuggestedPlan = reportAddDto.UserRequest,
        };
        await _validationReportRepository.AddAsync(report);
        unitOfWork.Save();

        var reportToReturn = new UserValidationReportDto
        {
            Id = report.Id,
            Content = reportAddDto.Content!,
            CreatedAt = report.CreatedAt,
            CreatedBy = report.CreatedBy,
            Status = report.Status
        };
        return Result<UserValidationReportDto>.Success(reportToReturn);
    }

    public async Task<Result<UserValidationReportDto>> UpdateValidationReport(UserUpdateReportDto updateReportDto)
    {
        var report = await _validationReportRepository.Find(r => r.Id == updateReportDto.Id);
        if (report is null)
            return Result<UserValidationReportDto>.Failure("There is no report with this Id");

        var content = await fileStorageService.ReadFileAsync(report.FileName);
        var serializedContent = JsonSerializer.Deserialize<ValidationReportDto>(content);
        report.Status = (ValidationReportStatusEnum)updateReportDto.Status;
        unitOfWork.Save();

        var response = new UserValidationReportDto
        {
            Id = report.Id,
            Content = serializedContent,
            CreatedAt = report.CreatedAt,
            CreatedBy = report.CreatedBy,
            Status = report.Status
        };
        return Result<UserValidationReportDto>.Success(response);
    }

    public async Task<Result<UserValidationReportDto>> GetValidationReportById(string reportId)
    {
        var report = await _validationReportRepository.Find(r => r.Id == reportId);
        if (report is null)
            return Result<UserValidationReportDto>.Failure("There is no report with this Id");

        var content = await fileStorageService.ReadFileAsync(report.FileName);
        var serializedContent = JsonSerializer.Deserialize<ValidationReportDto>(content);

        var response = new UserValidationReportDto
        {
            Id = report.Id,
            Content = serializedContent,
            CreatedAt = report.CreatedAt,
            CreatedBy = report.CreatedBy,
            Status = report.Status,
            UserQuestion = report.UserSuggestedPlan ?? null
        };
        return Result<UserValidationReportDto>.Success(response);
    }

    public async Task<Result<BaseToReturnDto>> SendValidationReport(SendValidationReportDto sendValidationReportDto)
    {
        // here we must have a grpc that take userId and companyId then return the teamleader of the team that user belong to.
        var teamLeaderId = await teamGrpcClient.GetTeamLeaderId(sendValidationReportDto.CreatedBy!);
        if (!teamLeaderId.IsSuccess)
            return Result<BaseToReturnDto>.Failure(teamLeaderId.Error);

        var stringContent = JsonSerializer.Serialize(sendValidationReportDto.Content);
        var fileName = await fileStorageService.SaveFileAsync(stringContent!);
        var report = new ValidationReport
        {
            Id = Guid.NewGuid().ToString(),
            Status = ValidationReportStatusEnum.UnderReview,
            FileType = ".txt",
            FileName = fileName,
            CreatedBy = sendValidationReportDto.CreatedBy!,
            CreatedAt = DateTime.UtcNow,
            UserSuggestedPlan = sendValidationReportDto.UserRequest,

            // FOR NOW
            SendTo = teamLeaderId.Value
        };
        await _validationReportRepository.AddAsync(report);
        unitOfWork.Save();

        var notification = new NotificationEventDto
        {
            Title = "validation report ",
            Message = "A new validation report is waiting for your approval!",
            ReceiverId = teamLeaderId.Value,
            SenderId = sendValidationReportDto.CreatedBy,
            PlanId = null
        };
        backgroundService.ExecuteInBackground(() => eventPublisher.PublishNotificationAsync(notification));

        var response = new BaseToReturnDto
        {
            IsSuccess = true,
            message = "validation report send successfully!"
        };
        return Result<BaseToReturnDto>.Success(response);

    }

    public async Task<Result<IEnumerable<UserValidationReportDto>>> GetValidationReportBySendToId(string sendToId, int limit = 3)
    {
        var reports = await _validationReportRepository.FindAllWithLimit(r => r.SendTo == sendToId, x => x.CreatedAt, limit);
        if (!reports.Any())
            return Result<IEnumerable<UserValidationReportDto>>.Success(Enumerable.Empty<UserValidationReportDto>());

        const int maxParallelTasks = 5;

        var semaphore = new SemaphoreSlim(maxParallelTasks);
        var tasks = reports.Select(async r =>
        {
            // wait for a slot from the semaphore
            await semaphore.WaitAsync();
            try
            {
                // Read file execution
                var content = await fileStorageService.ReadFileAsync(r.FileName);
                var serializedContent = JsonSerializer.Deserialize<ValidationReportDto>(content);

                // Create the DTO
                return new UserValidationReportDto
                {
                    Id = r.Id,
                    UserQuestion = r.UserSuggestedPlan ?? null,
                    Content = serializedContent,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    Status = r.Status
                };
            }
            finally
            {
                // Release the slot after completion (in both cases: success or error)
                semaphore.Release();
            }
        }).ToList(); // Convert to list to ensure all Select executions begin

        // Wait for all tasks to complete
        var reportsDto = await Task.WhenAll(tasks);
        return Result<IEnumerable<UserValidationReportDto>>.Success(reportsDto);
    }

    public async Task<Result<IEnumerable<UserValidationReportDto>>> GetValidationReportByStatus(string sendToId, int status, int limit = 3)
    {
        var reports = await _validationReportRepository.FindAllWithLimit(r =>
        r.SendTo == sendToId &&
        r.Status == (ValidationReportStatusEnum)status,
        x => x.CreatedAt, limit);

        if (!reports.Any())
            return Result<IEnumerable<UserValidationReportDto>>.Success(Enumerable.Empty<UserValidationReportDto>());

        const int maxParallelTasks = 5;

        var semaphore = new SemaphoreSlim(maxParallelTasks);
        var tasks = reports.Select(async r =>
        {
            // wait for a slot from the semaphore
            await semaphore.WaitAsync();
            try
            {
                // Read file execution
                var content = await fileStorageService.ReadFileAsync(r.FileName);
                var serializedContent = JsonSerializer.Deserialize<ValidationReportDto>(content);

                // Create the DTO
                return new UserValidationReportDto
                {
                    Id = r.Id,
                    UserQuestion = r.UserSuggestedPlan ?? null,
                    Content = serializedContent,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    Status = r.Status
                };
            }
            finally
            {
                // Release the slot after completion (in both cases: success or error)
                semaphore.Release();
            }
        }).ToList(); // Convert to list to ensure all Select executions begin

        // Wait for all tasks to complete
        var reportsDto = await Task.WhenAll(tasks);
        return Result<IEnumerable<UserValidationReportDto>>.Success(reportsDto);
    }

    public async Task<Result<IEnumerable<UserValidationReportDto>>> GetCreatedByValidationReportByStatus(string createdBy, int status, int limit = 3)
    {
        var reports = await _validationReportRepository.FindAllWithLimit(r =>
        r.CreatedBy == createdBy &&
        r.Status == (ValidationReportStatusEnum)status,
        x => x.CreatedAt, limit);

        if (!reports.Any())
            return Result<IEnumerable<UserValidationReportDto>>.Success(Enumerable.Empty<UserValidationReportDto>());

        const int maxParallelTasks = 5;

        var semaphore = new SemaphoreSlim(maxParallelTasks);
        var tasks = reports.Select(async r =>
        {
            // wait for a slot from the semaphore
            await semaphore.WaitAsync();
            try
            {
                // Read file execution
                var content = await fileStorageService.ReadFileAsync(r.FileName);
                var serializedContent = JsonSerializer.Deserialize<ValidationReportDto>(content);

                // Create the DTO
                return new UserValidationReportDto
                {
                    Id = r.Id,
                    UserQuestion = r.UserSuggestedPlan ?? null,
                    Content = serializedContent,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    Status = r.Status
                };
            }
            finally
            {
                // Release the slot after completion (in both cases: success or error)
                semaphore.Release();
            }
        }).ToList(); // Convert to list to ensure all Select executions begin

        // Wait for all tasks to complete
        var reportsDto = await Task.WhenAll(tasks);
        return Result<IEnumerable<UserValidationReportDto>>.Success(reportsDto);
    }
    public async Task<Result<IEnumerable<UserValidationReportDto>>> GetValidationReportByCreatedById(string userId, int limit)
    {
        var reports = await _validationReportRepository.FindAllWithLimit(r => r.CreatedBy == userId, x => x.CreatedAt, limit);
        if (!reports.Any())
            return Result<IEnumerable<UserValidationReportDto>>.Success(Enumerable.Empty<UserValidationReportDto>());

        const int maxParallelTasks = 5;

        var semaphore = new SemaphoreSlim(maxParallelTasks);
        var tasks = reports.Select(async r =>
        {
            // wait for a slot from the semaphore
            await semaphore.WaitAsync();
            try
            {
                // Read file execution
                var content = await fileStorageService.ReadFileAsync(r.FileName);
                var serializedContent = JsonSerializer.Deserialize<ValidationReportDto>(content);

                // Create the DTO
                return new UserValidationReportDto
                {
                    Id = r.Id,
                    UserQuestion = r.UserSuggestedPlan ?? null,
                    Content = serializedContent,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    Status = r.Status
                };
            }
            finally
            {
                // Release the slot after completion (in both cases: success or error)
                semaphore.Release();
            }
        }).ToList(); // Convert to list to ensure all Select executions begin

        // Wait for all tasks to complete
        var reportsDto = await Task.WhenAll(tasks);
        return Result<IEnumerable<UserValidationReportDto>>.Success(reportsDto);
    }
}
