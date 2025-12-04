using Core.DTOs;

namespace Core.Interfaces;

public interface IReportService
{
    Task<AiReportResponseDto> GetReport(string teamId, GetReportDto getReportDto);
    Task<AiReportResponseDto> GetReportById(string reportId);
    Task<string> DeleteReport(string reportId);
}
