using Notification.Core.DTOs;
using Notification.Core.Models;
using Notification.Core.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecoMind.Contracts.Events;

namespace Notification.Core.Interfaces
{
    public interface INotificationService
    {
        Task<Result<bool>> SendNotificationAsync(NotificationEventDto notification);
        Task<Result<IEnumerable<NotificationResponseDto>>> GetUserHistoryAsync(string userId);
        Task<Result<IEnumerable<NotificationResponseDto>>> GetByStatusAsync(string userId, bool isRead);
        Task<Result<NotificationResponseDto>> GetAndMarkAsReadAsync(string id);
        Task<Result<bool>> MarkAllAsReadAsync(string userId);
        Task<Result<bool>> DeleteNotificationAsync(string id);
        Task<Result<int>> GetUnreadCountAsync(string userId);

        Task<Result<bool>> RegisterDeviceTokenAsync(string userId, RegisterDeviceTokenDto dto);
    }
}
