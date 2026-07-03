using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.DTOs.ValidationReport;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Core.Service.Interface.AI;
using Infrastructure.GrpcClients.Team;
using System.Text.Json;

namespace Core.Service;

public class ValidationReportService(IValidationReportGeneratorService reportGeneratorService,
                                     IUnitOfWork<ValidationReport> unitOfWork,
                                     ITeamGrpcClient teamGrpcClient,
                                     IFileStorageService fileStorageService) : IValidationReportService
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
            Status = report.Status
        };
        return Result<UserValidationReportDto>.Success(response);
    }
}
