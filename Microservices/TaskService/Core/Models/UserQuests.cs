namespace Core.Models;

public class UserQuests
{
    public string QuestId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public Quest Quest { get; set; } = default!;
}
