using System.Text.Json.Serialization;

namespace Core.Dtos;

public class UpdateCommentDto
{
    public string? UserComment { get; set; }
    [JsonIgnore]
    public string? CommentId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
