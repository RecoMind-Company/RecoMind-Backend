namespace Core.Models;

public class ValidationReport
{
    public string Id { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public string CreatedBy { get; set; } = default!;
    public string? UserSuggestedPlan { get; set; }
    public string? SendTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public ValidationReportStatusEnum Status { get; set; }
}
