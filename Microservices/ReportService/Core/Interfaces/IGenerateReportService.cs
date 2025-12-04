using Core.DTOs.AI;

namespace Core.Interfaces;

public interface IGenerateReportService
{
    Task<AnalysisResponseDto> GenerateReport(AnalysisRequestDto request);
    Task<TaskStatusResponseDto> GetStatus(string taskId);
}
