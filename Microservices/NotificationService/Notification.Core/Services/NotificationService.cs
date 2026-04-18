using AutoMapper;
using Notification.Core.DTOs;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationHubContext _hubContext;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationHubContext hubContext,
            INotificationRepository repository,
            IMapper mapper)
        {
            _hubContext = hubContext;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task SendNotificationAsync(NotificationEventDto dto)
        {
            var notification = _mapper.Map<NotificationModel>(dto);
            notification.Id = Guid.NewGuid().ToString();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            // Save to database
            await _repository.AddAsync(notification);

            var response = _mapper.Map<NotificationResponseDto>(notification);

            // Send real-time notification
            await _hubContext.SendNotificationAsync(notification.ReceiverId, response);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetUserHistoryAsync(string userId)
        {
            var notifications = await _repository.GetUserNotificationsAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetByStatusAsync(string userId, bool isRead)
        {
            var notifications = await _repository.GetByReadStatusAsync(userId, isRead);
            return _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);
        }

        public async Task<NotificationResponseDto?> GetAndMarkAsReadAsync(string id)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification is null) return null;

            if (!notification.IsRead)
            {
                await _repository.MarkAsReadAsync(id);
                notification.IsRead = true;
            }

            return _mapper.Map<NotificationResponseDto>(notification);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            if(string.IsNullOrEmpty(userId)) return false;
            await _repository.MarkAllAsReadAsync(userId);
            return true;
        }

        public async Task<int> GetUnreadCountAsync(string userId) 
            => await _repository.GetUnreadCountAsync(userId);

        public async Task<bool> DeleteNotificationAsync(string id)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification is null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

    }
}
