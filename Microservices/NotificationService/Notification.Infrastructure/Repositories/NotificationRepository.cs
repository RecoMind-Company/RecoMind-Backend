using Microsoft.EntityFrameworkCore;
using Notification.Core.Infrastructure.Data;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;
        public NotificationRepository(NotificationDbContext context)
            => _context = context;

        public async Task AddAsync(NotificationModel notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<NotificationModel?> GetByIdAsync(string id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string receiverId)
        {
            return await _context.Notifications
                        .AsNoTracking()
                        .Where(n => n.ReceiverId == receiverId)
                        .OrderByDescending(n => n.CreatedAt)
                        .ToListAsync();
        }

        public async Task<IEnumerable<NotificationModel>> GetByReadStatusAsync(string receiverId, bool isRead)
        {
            return await _context.Notifications
                        .AsNoTracking()
                        .Where(n => n.ReceiverId == receiverId && n.IsRead == isRead)
                        .OrderByDescending(n => n.CreatedAt)
                        .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string receiverId)
        {
            return await _context.Notifications
                        .CountAsync(n => n.ReceiverId == receiverId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(string id)
        {
            await _context.Notifications
                .Where(n => n.Id == id && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task MarkAllAsReadAsync(string receiverId)
        {
            await _context.Notifications
                .Where(n => n.ReceiverId == receiverId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task DeleteAsync(string id)
        {
            await _context.Notifications
                .Where(n => n.Id == id)
                .ExecuteDeleteAsync();
        }


        public async Task<UserDeviceToken?> FindDeviceTokenAsync(string userId, string deviceToken)
        {
            return await _context.UserDeviceTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.DeviceToken == deviceToken);
        }

        public async Task AddDeviceTokenAsync(UserDeviceToken token)
        {
            await _context.UserDeviceTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeviceTokenAsync(UserDeviceToken token)
        {
            _context.UserDeviceTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetUserDeviceTokensAsync(string userId)
        {
            return await _context.UserDeviceTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.DeviceToken)
                .ToListAsync();
        }
    }
}
