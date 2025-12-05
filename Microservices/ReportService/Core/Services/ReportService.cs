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
    public async Task<AnalysisResponseDto> CreateReport()
    {
        // Call the grpc service to get the TeamName, companyId by {teamId}
        //var teamDetails = grpcTeamService.GetTeamDetails(teamId);``
        //if (teamDetails is null)
        //    return null;
        var analysisRequest = new AnalysisRequestDto
        {
            CompanyId = "fb140d33-7e96-474d-a06d-ab3a6c65d1a9",    //teamDetails.CompanyId,
            TeamName = "HR", //teamDetails.TeamName,
            UserRequest = "Full Employees Report including their Salaries." // getReportDto.UserRequest
        };
        var generateReportInitialResponse = await generateReportService.GenerateReport(analysisRequest);
        return generateReportInitialResponse;
    }
    public async Task<AiReportResponseDto> GetReport(string teamId, string taskId)
    {
        var generatedReportStatus = await generateReportService.GetStatus(taskId);
        if (generatedReportStatus.Status == Status.PENDING || generatedReportStatus.Status == Status.PROGRESS)
        {
            return null;
        }
        // The Status are have only 3 values PENDING, FAILURE, SUCCESS
        // FAILURE CASE HANDELD IN GENERATE REPORT SERVICE
        // PENDING CASE HANDELD ABOVE
        // SUCCESS
        var dynamicPath = await fileStorageService.SaveFileAsync(generatedReportStatus.Result!);
        // create report model 
        // add to database
        // save changes
        // return report model
        var report = new Report
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = teamId,
            FilePath = dynamicPath,
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Periodic.Weekly
        };
        await reportRepository.AddAsync(report);
        await unitOfWork.Save();
        var aiReportResponse = new AiReportResponseDto
        {
            AiResponse = generatedReportStatus.Result!,
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
