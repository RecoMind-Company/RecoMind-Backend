using Core.ServicesAbstractions;
using MassTransit;
using RecoMind.Contracts.Events;

namespace Infrastructure.Notification;

internal class NotificationService(IPublishEndpoint publishEndpoint) : INotificationService
{
    public async Task SendNotificationAsync(NotificationEventDto notificationEvent)
    {
        await publishEndpoint.Publish<NotificationEventDto>(notificationEvent);
    }
}
