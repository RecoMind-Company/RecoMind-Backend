using Microsoft.AspNetCore.SignalR;
using Notification.Core.DTOs;
using Notification.Core.Interfaces;

namespace Notification.WebApi.Hubs
{
    public class NotificationHubContext : INotificationHubContext
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubContext(IHubContext<NotificationHub> hubContext)
            => _hubContext = hubContext;

        public async Task SendNotificationAsync(string userId, NotificationResponseDto notification)
        {
            // بيبعت على الـ Group الخاص بالـ user
            // لو offline مش هيحصل error، بس مش هيستقبلش حاجة
            await _hubContext.Clients
                .Group(userId)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
