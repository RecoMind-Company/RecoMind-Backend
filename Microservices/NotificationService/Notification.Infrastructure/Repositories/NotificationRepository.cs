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
        {
            _context = context;
        }
        public async Task AddAsync(NotificationModel notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<NotificationModel?> GetByIdAsync(string id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<IEnumerable<NotificationModel>> GetByReceiverIdAsync(string receiverId)
        {
            return await _context.Notifications
                        .Where(n => n.receiverId == receiverId)
                        .OrderByDescending(n => n.CreatedAt) // sort by most recent first
                        .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string receiverId)
        {
            return await _context.Notifications
                        .CountAsync(n => n.receiverId == receiverId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(string id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string receiverId)
        {
            var unreadNotifications = await _context.Notifications
                        .Where(n => n.receiverId == receiverId && !n.IsRead)
                        .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
