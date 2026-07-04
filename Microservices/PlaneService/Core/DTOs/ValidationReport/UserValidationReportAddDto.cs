using Core.DTOs.AI.ValidationReport.AIResult;
using System.Text.Json.Serialization;

namespace Core.DTOs.ValidationReport;

public class UserValidationReportAddDto
{
    public string UserRequest { get; set; }
    public ValidationReportDto Content { get; set; }
    public int Status { get; set; }
    [JsonIgnore]
    public string? CreatedBy { get; set; }
}
