using System.Text.Json.Serialization;

namespace Core.DTOs.AI;

public class AIGetPlanDto
{
    public string TaskId { get; set; }
    [JsonIgnore]
    public string? CompanyId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
