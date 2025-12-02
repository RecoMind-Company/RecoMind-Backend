using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class AnalysisRequestDto
{
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; } = default!;
    [JsonPropertyName("user_request")]
    public string UserRequest { get; set; } = default!;
    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }
}
