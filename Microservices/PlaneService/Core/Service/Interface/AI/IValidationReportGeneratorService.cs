using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.Models;

namespace Core.Service.Interface.AI;

public interface IValidationReportGeneratorService
{
    Task<Result<AIValidationReportResponseDto>> GenerateValidationReport(AIValidationReportRequestDto requestDto);
    Task<Result<TaskResponseDto>> GetValidationReport(string taskId);
}
