namespace Core.Dtos.AI;
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
    public string task_id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public int duration_days { get; set; }
    public string start_date { get; set; }
    public string deadline_date { get; set; }
    public AISuggestedOwnersDto suggested_owner { get; set; }
    public string status { get; set; }
    public string priority { get; set; }
    // module 
    public string moduleId { get; set; }
}