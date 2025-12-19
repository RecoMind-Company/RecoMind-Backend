using System.Text.Json.Serialization;

namespace Core.DTOs;

public class AiReportResponseDto
{
    public string AiResponse { get; set; } = default!;
    public DateTime GeneratedDate { get; set; }
    [JsonIgnore]
    public string? message { get; set; }
    [JsonIgnore]
    public bool IsSuccess { get; set; }

}
