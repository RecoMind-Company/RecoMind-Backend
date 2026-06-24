using Core.DTOs.Notification;
using Core.Interfaces;
using MassTransit;

namespace Infrastructure.Notification;

internal class NotificationService(IPublishEndpoint publishEndpoint) : INotificationService
{
    public async Task SendNotificationAsync(NotificationEventDto notificationEvent)
    {
        await publishEndpoint.Publish<NotificationEventDto>(notificationEvent);
    }
}
