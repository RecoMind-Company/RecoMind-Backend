using Core.Models;

namespace Core.Dtos;
/*


 public string? ModuleId { get; set; }
 public string? PlanId { get; set; }
 */
public class QuestToReturnDto
{
    public string QuestId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public QuestStatusEnum Status { get; set; }
    public QuestPriorityEnum Priority { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DeadLine { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ModuleId { get; set; }
    public string? PlanId { get; set; }
    public List<string> UserAssignedQuests { get; set; } = [];
}
