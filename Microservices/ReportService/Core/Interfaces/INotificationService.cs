using Core.DTOs.Notification;

namespace Core.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationEventDto notificationEvent);
}
