using Core.DTOs.AI.ValidationReport.AIResult;
using System.Text.Json.Serialization;

namespace Core.DTOs.ValidationReport;

public class SendValidationReportDto
{
    public string UserRequest { get; set; }
    public ValidationReportDto Content { get; set; }
    [JsonIgnore]
    public string? CreatedBy { get; set; }
}
