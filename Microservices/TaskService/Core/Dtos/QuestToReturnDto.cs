using Core.Models;

namespace Core.Dtos;

public class QuestToReturnDto
{
    public string QuestId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public QuestStatusEnum Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DeadLine { get; set; }
    public TimeSpan Duration { get; set; }
    public string PlanId { get; set; } = default!;
    public List<string> UserAssignedQuests { get; set; } = [];
}
