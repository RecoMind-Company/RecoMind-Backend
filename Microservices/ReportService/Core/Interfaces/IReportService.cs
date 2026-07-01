using Core.DTOs;
using Core.DTOs.AI;

namespace Core.Interfaces;

public interface IReportService
{
    Task<ReportDto> CreateTestReport(TestDto testDto);
    Task<IEnumerable<ReportDto>> GetAllReportsByTeamId(string TeamId, int limit);
    Task<AiReportResponseDto> GetReportFromAi(GetReportFromAiDto reportFromAiDto);
    Task<DividedReportDto> GetReportById(string reportId);
    Task<string> DeleteReport(string reportId);
    Task<AnalysisResponseDto> CreateReportByAi(AnalysisRequestDto analysisRequest);
    Task<string> AssignCompanyData(string companyId);
    Task<string> GetAssignCompanyDataStatus(string taskId);
    Task<TeamToReturnDto> GetUserData(string userId);
}
