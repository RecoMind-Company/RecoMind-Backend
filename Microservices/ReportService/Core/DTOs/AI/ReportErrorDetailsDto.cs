using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class ReportErrorDetailsDto
{
    [JsonPropertyName("detail")]
    public List<ErrorDetail> Details { get; set; } = new List<ErrorDetail>();
}
