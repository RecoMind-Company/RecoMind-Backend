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
    Task<Result<IEnumerable<UserValidationReportDto>>> GetValidationReportBySendToId(string sendToId, int limit = 3);
    Task<Result<BaseToReturnDto>> SendValidationReport(SendValidationReportDto sendValidationReportDto);
    Task<Result<IEnumerable<UserValidationReportDto>>> GetValidationReportByCreatedById(string userId, int limit);
    Task<Result<IEnumerable<UserValidationReportDto>>> GetValidationReportByStatus(string sendToId, int status, int limit = 3);
    Task<Result<IEnumerable<UserValidationReportDto>>> GetCreatedByValidationReportByStatus(string createdBy, int status, int limit = 3);
}
