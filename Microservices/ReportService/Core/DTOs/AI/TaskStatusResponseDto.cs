using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class TaskStatusResponseDto
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = default!;
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
    [JsonPropertyName("result")]
    public string? Result { get; set; } // MAY BE THE STATUS IS PENDING SO THIS CAN BE NULL
}
