using Core.Interfaces;
using MassTransit;
using RecoMind.Contracts.Events;

namespace Infrastructure.Notification;

public class NotificationService(IPublishEndpoint publishEndpoint) : INotificationService
{
    public async Task SendNotificationAsync(NotificationEventDto notificationEvent)
    {
        await publishEndpoint.Publish<NotificationEventDto>(notificationEvent);
    }
}
