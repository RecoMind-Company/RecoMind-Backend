using System.Text.Json.Serialization;

namespace Core.DTOs.AI;
/*
  "suggested_owner": {
              "user_id": "Sales-mahmoud.ali-employee",
              "job_title": "B2B Sales Representative",
            },
 */
public class AISuggestedOwnersDto
{
    [JsonPropertyName("user_id")]
    public string user_id { get; set; }

    [JsonPropertyName("job_title")]
    public string job_title { get; set; }
}