using System.Text.Json.Serialization;

namespace Core.Dtos.Plan;

public class UpdatePlanCommentDto
{
    public string? UserComment { get; set; }
    public string? CommentId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
