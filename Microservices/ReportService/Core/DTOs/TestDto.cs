using System.Text.Json.Serialization;

namespace Core.DTOs;

public class TestDto
{
    [JsonIgnore]
    public string? UserId { get; set; }
    public string TeamId { get; set; }
    public string content { get; set; }

}
