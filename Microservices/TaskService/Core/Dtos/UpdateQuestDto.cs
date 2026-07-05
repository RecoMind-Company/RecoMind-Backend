namespace Core.Dtos;

public class UpdateQuestDto
{
    public string? Title { get; set; } = default!;
    public string? Description { get; set; }
    public int? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DeadLine { get; set; }
}
