using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class ErrorDetail
{
    [JsonPropertyName("loc")]
    public List<object> Loc { get; set; } = new List<object>();

    [JsonPropertyName("msg")]
    public string Msg { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    public bool IsValid { get; set; }
}
