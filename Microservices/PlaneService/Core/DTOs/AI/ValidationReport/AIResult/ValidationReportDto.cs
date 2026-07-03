using System.Text.Json.Serialization;

namespace Core.DTOs.AI.ValidationReport.AIResult;

public class ValidationReportDto
{
    [JsonPropertyName("executive_summary")]
    public string ExecutiveSummary { get; set; }

    [JsonPropertyName("validation_decision")]
    public string ValidationDecision { get; set; }

    [JsonPropertyName("confidence_score")]
    public int ConfidenceScore { get; set; }

    [JsonPropertyName("key_findings")]
    public KeyFindingsDto KeyFindings { get; set; }

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; }

    [JsonPropertyName("risk_factors")]
    public List<string> RiskFactors { get; set; }

    [JsonPropertyName("next_steps")]
    public List<string> NextSteps { get; set; }
}
