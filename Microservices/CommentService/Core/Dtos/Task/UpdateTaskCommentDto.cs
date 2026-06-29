using System.Text.Json.Serialization;

namespace Core.Dtos.Task;

public class UpdateTaskCommentDto
{
    public string? UserComment { get; set; }
    public string? CommentId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
