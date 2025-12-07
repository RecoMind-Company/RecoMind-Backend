using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class DataAssignStatusDto
{

    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = default!;
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
}
