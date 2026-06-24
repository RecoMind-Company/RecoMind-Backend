using Core.Dtos.Notification;

namespace Core.ServicesAbstractions;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationEventDto notificationEvent);
}
