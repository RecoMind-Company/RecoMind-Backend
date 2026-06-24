namespace Core.Dtos.Notification;

public class NotificationEventDto
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public string? SenderId { get; set; }
    public string? PlanId { get; set; }
}
