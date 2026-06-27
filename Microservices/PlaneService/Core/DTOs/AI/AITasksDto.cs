using System.Text.Json.Serialization;

namespace Core.DTOs.AI;
/*
  "tasks": [
          {
            "task_id": "task_101",
            "title": "تحديد أهداف استهداف عملاء جدد لكل ممثل مبيعات",
            "description": "تحديد عدد العملاء المستهدفين لكل ممثل مبيعات بناءً على حجم الفريق وقيمة كل عميل، مع تحديد القطاعات والصناعات المستهدفة.",
            "duration_days": 3,
            "start_date": "2026-06-26",
            "deadline_date": "2026-06-28",
            "suggested_owner": {
              "user_id": "Sales-mahmoud.ali-employee",
              "job_title": "B2B Sales Representative",
            },
            "status": "to_do",
            "priority": "high"
          }
 */
public class AITasksDto
{
    [JsonPropertyName("task_id")]
    public string task_id { get; set; }

    [JsonPropertyName("title")]
    public string title { get; set; }

    [JsonPropertyName("description")]
    public string description { get; set; }

    [JsonPropertyName("duration_days")]
    public int duration_days { get; set; }

    [JsonPropertyName("start_date")]
    public string start_date { get; set; }

    [JsonPropertyName("deadline_date")]
    public string deadline_date { get; set; }

    [JsonPropertyName("suggested_owner")]
    public AISuggestedOwnersDto suggested_owner { get; set; }

    [JsonPropertyName("status")]
    public string status { get; set; }

    [JsonPropertyName("priority")]
    public string priority { get; set; }
}