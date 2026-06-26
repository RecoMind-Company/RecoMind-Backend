using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class AIRequestDto
{
    [JsonPropertyName("company_id")]
    public string company_id { get; set; }
    [JsonPropertyName("plan_text")]
    public string plan_text { get; set; }
    [JsonPropertyName("team_id")]
    public string team_id { get; set; }
}
