using System.Text.Json.Serialization;

namespace Core.DTOs.AI.ValidationReport.AIResult;

public class TaskResultDto
{
    [JsonPropertyName("validation_report")]
    public ValidationReportDto ValidationReport { get; set; }
}
