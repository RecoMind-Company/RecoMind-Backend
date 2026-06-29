namespace Core.Models;

public class PlanComment
{
    public string Id { get; set; } = default!;
    public string UserComment { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string PlanId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
