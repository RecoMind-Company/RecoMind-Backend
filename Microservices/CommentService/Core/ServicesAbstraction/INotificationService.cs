using RecoMind.Contracts.Events;

namespace Core.ServicesAbstractions;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationEventDto notificationEvent);
}
