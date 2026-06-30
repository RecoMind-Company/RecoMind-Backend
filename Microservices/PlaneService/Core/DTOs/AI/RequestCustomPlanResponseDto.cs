using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class RequestCustomPlanResponseDto
{
    [JsonPropertyName("task_id")]
    public string taskId { get; set; }
    [JsonPropertyName("status")]
    public string status { get; set; }
    [JsonPropertyName("message")]
    public string message { get; set; }
}
