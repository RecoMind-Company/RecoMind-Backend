using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class ReportValidationError
{
    [JsonPropertyName("detail")]
    public List<ErrorDetail> Details { get; set; } = new List<ErrorDetail>();
}
