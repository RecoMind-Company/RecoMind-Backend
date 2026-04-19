using Notification.Core.DTOs;
using Notification.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationEventDto notification);
        Task<IEnumerable<NotificationResponseDto>> GetUserHistoryAsync(string userId);
        Task<IEnumerable<NotificationResponseDto>> GetByStatusAsync(string userId, bool isRead);
        Task<NotificationResponseDto?> GetAndMarkAsReadAsync(string id);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string id);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
