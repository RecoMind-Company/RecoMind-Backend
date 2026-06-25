namespace Core.Models;

public class Quest
{
    public string QuestId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public QuestStatusEnum Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DeadLine { get; set; }
    public TimeSpan Duration => DeadLine - StartDate;
    public string? PlanId { get; set; }
    public ICollection<UserQuests> UserAssignedQuests { get; set; } = [];
}
