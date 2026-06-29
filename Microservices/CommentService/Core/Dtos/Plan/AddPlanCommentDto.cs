using System.Text.Json.Serialization;

namespace Core.Dtos.Plan;

public class AddPlanCommentDto
{
    public string? UserComment { get; set; }
    [JsonIgnore]
    public string? PlanId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
