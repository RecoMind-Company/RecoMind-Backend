namespace Core.Dtos;

public class QuestDto
{
    // All of this are requied except ModuleId
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DeadLine { get; set; }
    public string? ModuleId { get; set; }
    public string? PlanId { get; set; }
}
