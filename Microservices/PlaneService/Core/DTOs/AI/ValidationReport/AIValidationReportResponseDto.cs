using System.Text.Json.Serialization;

namespace Core.DTOs.AI.ValidationReport;

public class AIValidationReportResponseDto
{
    [JsonPropertyName("task_id")]
    public string taskId { get; set; }
    [JsonPropertyName("status")]
    public string status { get; set; }
    [JsonPropertyName("message")]
    public string message { get; set; }
}
