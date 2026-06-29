namespace Core.Models;

public class QuestComment
{
    public string Id { get; set; } = default!;
    public string UserComment { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string QuestId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
