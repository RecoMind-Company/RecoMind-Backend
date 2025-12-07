using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class DataAssignRequestDto
{
    [JsonPropertyName("company_id")]
    public string CompanyId { get; set; } = default!;
}
