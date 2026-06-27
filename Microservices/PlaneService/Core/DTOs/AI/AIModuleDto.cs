using System.Text.Json.Serialization;

namespace Core.DTOs.AI;
/*
  "modules": [
      {
        "module_id": "mod_1",
        "module_name": "استهداف عملاء جدد وتطوير قاعدة بيانات العملاء",
        "tasks": []
      }
 */
public class AIModuleDto
{
    [JsonPropertyName("module_id")]
    public string module_id { get; set; }

    [JsonPropertyName("module_name")]
    public string module_name { get; set; }

    [JsonPropertyName("tasks")]
    public List<AITasksDto> tasks { get; set; } = [];
}