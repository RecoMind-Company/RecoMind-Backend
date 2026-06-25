using Core.Models;

namespace Core.Dtos;

public class PersonalQuestToReturnDto
{
    public string QuestId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public QuestStatusEnum Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DeadLine { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> UserAssignedQuests { get; set; } = [];
}
