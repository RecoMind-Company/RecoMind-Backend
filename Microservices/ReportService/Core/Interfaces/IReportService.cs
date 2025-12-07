using Core.DTOs;
using Core.DTOs.AI;

namespace Core.Interfaces;

public interface IReportService
{
    Task<AiReportResponseDto> GetReport(string teamId, string taskId);
    Task<AiReportResponseDto> GetReportById(string reportId);
    Task<string> DeleteReport(string reportId);
    Task<AnalysisResponseDto> CreateReport(string userRequest);
    Task<string> AssignCompanyData(string companyId);
    Task<string> GetAssignCompanyDataStatus(string taskId);
}
