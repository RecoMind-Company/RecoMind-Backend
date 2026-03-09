using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Core.Models;

namespace Notification.Core.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(NotificationModel notification);
        Task<NotificationModel?> GetByIdAsync(string id);
        Task<IEnumerable<NotificationModel>> GetByReceiverIdAsync(string receiverId);
        Task<int> GetUnreadCountAsync(string receiverId);
        Task MarkAsReadAsync(string id);
        Task MarkAllAsReadAsync(string receiverId);
    }
}
