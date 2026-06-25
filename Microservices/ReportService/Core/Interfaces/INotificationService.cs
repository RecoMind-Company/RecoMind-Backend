using RecoMind.Contracts.Events;

namespace Core.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationEventDto notificationEvent);
}
