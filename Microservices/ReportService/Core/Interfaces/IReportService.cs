using Core.DTOs;
using Core.DTOs.AI;

namespace Core.Interfaces;

public interface IReportService
{
    Task<IEnumerable<ReportDto>> GetAllReportsByTeamId(string TeamId);
    Task<AiReportResponseDto> GetReportFromAi(GetReportFromAiDto reportFromAiDto);
    Task<AiReportResponseDto> GetReportById(string reportId);
    Task<string> DeleteReport(string reportId);
    Task<AnalysisResponseDto> CreateReportByAi(AnalysisRequestDto analysisRequest);
    Task<string> AssignCompanyData(string companyId);
    Task<string> GetAssignCompanyDataStatus(string taskId);
    Task<TeamToReturnDto> GetUserData(string userId);
}
