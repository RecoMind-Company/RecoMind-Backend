using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class DataAssignResponseDto
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = default!;
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; } = default!;
}
