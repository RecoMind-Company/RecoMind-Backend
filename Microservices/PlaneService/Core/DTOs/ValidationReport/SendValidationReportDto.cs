using Core.DTOs.AI.ValidationReport.AIResult;
using System.Text.Json.Serialization;

namespace Core.DTOs.ValidationReport;

public class SendValidationReportDto
{
    public ValidationReportDto Content { get; set; }
    public int Status { get; set; }
    public string SendTo { get; set; }
    [JsonIgnore]
    public string? CreatedBy { get; set; }
}
