using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class FullAIResponseDto
{
    [JsonPropertyName("result")]
    public AIPlanDto result { get; set; }
    [JsonPropertyName("status")]
    public string status { get; set; }
}
