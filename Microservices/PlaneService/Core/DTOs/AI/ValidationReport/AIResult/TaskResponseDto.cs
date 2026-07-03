using System.Text.Json.Serialization;

namespace Core.DTOs.AI.ValidationReport.AIResult;

public class TaskResponseDto
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("result")]
    public TaskResultDto Result { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}
