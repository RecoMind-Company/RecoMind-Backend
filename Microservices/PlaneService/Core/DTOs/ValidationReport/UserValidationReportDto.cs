using Core.DTOs.AI.ValidationReport.AIResult;
using Core.Models;

namespace Core.DTOs.ValidationReport;

public class UserValidationReportDto
{
    public string Id { get; set; } = default!;
    public ValidationReportDto Content { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public ValidationReportStatusEnum Status { get; set; }
}
