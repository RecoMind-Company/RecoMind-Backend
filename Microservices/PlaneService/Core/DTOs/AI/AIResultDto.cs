using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class AIResultDto
{
    [JsonPropertyName("result")]
    public AIPlanDto result { get; set; }
}
