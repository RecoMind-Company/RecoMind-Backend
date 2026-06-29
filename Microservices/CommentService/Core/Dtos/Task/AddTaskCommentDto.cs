using System.Text.Json.Serialization;

namespace Core.Dtos.Task;

public class AddTaskCommentDto
{
    public string? UserComment { get; set; }
    [JsonIgnore]
    public string? QuestId { get; set; }
    [JsonIgnore]
    public string? UserId { get; set; }
}
