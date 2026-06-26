namespace Core.Dtos.AI;
/*
  "suggested_owner": {
              "user_id": "Sales-mahmoud.ali-employee",
              "job_title": "B2B Sales Representative",
            },
 */
public class AISuggestedOwnersDto
{
    public string user_id { get; set; }
    public string job_title { get; set; }
}