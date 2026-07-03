using System.Text.Json.Serialization;

namespace Core.DTOs.AI.ValidationReport;


/*
 * {
  "company_id": "34293b50-0c58-4111-8fcd-b0127dd250ce",
  "team_id": "0dc1400d-a758-424b-80fb-a8ff89078522",
  "user_request": "Increase company sales by 20% in the next quarter by targeting new customers, following up with potential customers, conducting product presentations, negotiating contracts, completing sales, then preparing a final report showing sales results and performance indicators"
}
 */
public class AIValidationReportRequestDto
{
    [JsonPropertyName("company_id")]
    public string? CompanyId { get; set; }
    [JsonPropertyName("team_id")]
    public string? TeamId { get; set; }
    [JsonPropertyName("user_request")]
    public string UserRequest { get; set; } = default!;
}
