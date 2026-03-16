namespace Core.Models;

public class Comment
{
    public string Id { get; set; } = default!;
    public string UserComment { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string PlanId { get; set; } = default!;
}
