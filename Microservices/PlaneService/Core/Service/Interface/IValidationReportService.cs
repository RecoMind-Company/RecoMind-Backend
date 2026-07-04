using Core.DTOs;
using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.DTOs.ValidationReport;
using Core.Models;

namespace Core.Service.Interface;

public interface IValidationReportService
{
    Task<Result<AIValidationReportResponseDto>> RequestValidationReport(UserValidationReportRequestDto requestDto);
    Task<Result<ValidationReportDto>> GetValidationReport(string taskId);
    Task<Result<UserValidationReportDto>> AddValidationReport(UserValidationReportAddDto reportAddDto);
    Task<Result<UserValidationReportDto>> UpdateValidationReport(UserUpdateReportDto updateReportDto);
    Task<Result<UserValidationReportDto>> GetValidationReportById(string reportId);
    Task<Result<BaseToReturnDto>> SendValidationReport(SendValidationReportDto sendValidationReportDto);
}
