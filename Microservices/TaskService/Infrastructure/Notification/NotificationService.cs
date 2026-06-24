using Core.Dtos.Notification;
using Core.ServicesAbstractions;
using MassTransit;

namespace Infrastructure.Notification;

internal class NotificationService(IPublishEndpoint publishEndpoint) : INotificationService
{
    public async Task SendNotificationAsync(NotificationEventDto notificationEvent)
    {
        await publishEndpoint.Publish<NotificationEventDto>(notificationEvent);
    }
}
