using System.Text.Json.Serialization;

namespace Core.DTOs.AiService
{
    /*
     {
      "task_id": "string",
      "status": "string",
      "result": "string"
    }
     */

    public class FinalResponseDto
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("result")]
        public ResponseMessage Result { get; set; }
    }

    public class ResponseMessage
    {
        [JsonPropertyName("sql_query")]
        public string Sql_Query { get; set; }
        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}
