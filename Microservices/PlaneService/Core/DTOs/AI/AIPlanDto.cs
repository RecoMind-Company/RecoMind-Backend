using System.Text.Json.Serialization;

namespace Core.DTOs.AI;
/*
 {
  "result": {
    "plan_id": "plan_202606261452_f3e0ac",
    "plan_title": "زيادة مبيعات الشركة بنسبة 20% خلال الربع القادم",
    "status": "draft",
    "start_date": "2026-06-26",
    "deadline_date": "2026-09-20",
    "total_estimated_days": 87,
    "modules": []
 */
public class AIPlanDto
{
    [JsonPropertyName("plan_id")]
    public string plan_id { get; set; }

    [JsonPropertyName("plan_title")]
    public string plan_title { get; set; }

    [JsonPropertyName("status")]
    public string status { get; set; }

    [JsonPropertyName("start_date")]
    public DateTime start_date { get; set; }

    [JsonPropertyName("deadline_date")]
    public DateTime deadline_date { get; set; }

    [JsonPropertyName("total_estimated_days")]
    public int total_estimated_days { get; set; }

    [JsonPropertyName("modules")]
    public List<AIModuleDto> modules { get; set; } = [];
}
