using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class GetReportFromAiDto
{
    // periodic // userId  ----------- TO ADD -------------
    public string TeamId { get; set; } = default!;
    [JsonIgnore]
    public string? UserId { get; set; } = default!;
    public string Periodic { get; set; } = default!;
    public string TaskId { get; set; } = default!;
}
