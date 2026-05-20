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
        Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string receiverId);
        Task<IEnumerable<NotificationModel>> GetByReadStatusAsync(string receiverId, bool isRead);
        Task<int> GetUnreadCountAsync(string receiverId);
        Task MarkAsReadAsync(string id);
        Task MarkAllAsReadAsync(string receiverId);
        Task DeleteAsync(string id);


        Task<UserDeviceToken?> FindDeviceTokenAsync(string userId, string deviceToken);
        Task AddDeviceTokenAsync(UserDeviceToken token);
        Task UpdateDeviceTokenAsync(UserDeviceToken token);
        Task<List<string>> GetUserDeviceTokensAsync(string userId); // اللي بنستخدمها وإحنا بنبعت الفايربيز
    }
}
