using Notification.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.Interfaces
{
    public interface INotificationHubContext
    {
        Task SendNotificationAsync(string userId, NotificationResponseDto notification);
    }
}
