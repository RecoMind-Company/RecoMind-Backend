using Core.DTOs;
using Core.DTOs.AI;
using Core.Interfaces;
using Core.Models;

namespace Core.Services;

public class ReportService(IGenerateReportService generateReportService,
                           IUnitOfWork unitOfWork,
                           IReportRepository reportRepository,
                           IFileStorageService fileStorageService,
                           IGrpcTeamService grpcTeamService) : IReportService
{
    public async Task<AiReportResponseDto> GetReport(GetReportDto getReportDto)
    {
        // Call the grpc service to get the TeamName, companyId by {teamId}
        var teamDetails = grpcTeamService.GetTeamDetails(getReportDto.TeamId);
        if (teamDetails is null)
            return null;
        var analysisRequest = new AnalysisRequestDto
        {
            CompanyId = teamDetails.CompanyId,
            TeamName = teamDetails.TeamName,
            UserRequest = getReportDto.UserRequest
        };
        var generateReportInitialResponse = await generateReportService.GenerateReport(analysisRequest);
        TaskStatusResponseDto taskStatusResponse;

        do
        {
            taskStatusResponse = await generateReportService.GetStatus(generateReportInitialResponse.TaskId);
            await Task.Delay(180000);
        }
        while (taskStatusResponse.Status != Status.SUCCESS);
        var dynamicPath = await fileStorageService.SaveFileAsync(taskStatusResponse.Result!);
        // create report model 
        // add to database
        // save changes
        // return report model
        var report = new Report
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = getReportDto.TeamId,
            FilePath = dynamicPath,
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Enum.Parse<Periodic>(getReportDto.Periodic)
        };
        await reportRepository.AddAsync(report);
        await unitOfWork.Save();
        var aiReportResponse = new AiReportResponseDto
        {
            AiResponse = taskStatusResponse.Result!,
            GeneratedDate = DateTime.UtcNow
        };
        return aiReportResponse;
    }

    public async Task<AiReportResponseDto> GetReportById(string reportId)
    {
        var report = await reportRepository.GetByIdAsync(reportId);
        if (report is null)
            return null;
        var reportContent = await fileStorageService.ReadFileAsync(report.FilePath);
        if (string.IsNullOrEmpty(reportContent))
            return null;
        var aiReportResponse = new AiReportResponseDto
        {
            AiResponse = reportContent,
            GeneratedDate = report.GeneratedDate
        };
        return aiReportResponse;
    }
    public async Task<string> DeleteReport(string reportId)
    {
        var report = await reportRepository.GetByIdAsync(reportId);
        if (report is null)
            return "";
        reportRepository.Delete(report);
        await unitOfWork.Save();
        return "Report deleted successfully";
    }


}
