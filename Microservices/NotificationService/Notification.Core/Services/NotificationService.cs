using AutoMapper;
using Notification.Core.DTOs;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using Notification.Core.Result;
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

        public async Task<Result<bool>> SendNotificationAsync(NotificationEventDto dto)
        {
            if (dto == null) return new Error("Notification.BadRequest", "Notification data is required.");

            var notification = _mapper.Map<NotificationModel>(dto);
            notification.Id = Guid.NewGuid().ToString();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            // Save to database
            await _repository.AddAsync(notification);

            var response = _mapper.Map<NotificationResponseDto>(notification);

            // Send real-time notification
            await _hubContext.SendNotificationAsync(notification.ReceiverId, response);
            return true;
        }

        public async Task<Result<IEnumerable<NotificationResponseDto>>> GetUserHistoryAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return NotificationErrors.InvalidUserId;

            var notifications = await _repository.GetUserNotificationsAsync(userId);
            var response = _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);

            return Result<IEnumerable<NotificationResponseDto>>.Success(response);
        }

        public async Task<Result<IEnumerable<NotificationResponseDto>>> GetByStatusAsync(string userId, bool isRead)
        {
            if (string.IsNullOrWhiteSpace(userId)) return NotificationErrors.InvalidUserId;

            var notifications = await _repository.GetByReadStatusAsync(userId, isRead);
            var response = _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);

            return Result<IEnumerable<NotificationResponseDto>>.Success(response);
        }

        public async Task<Result<NotificationResponseDto>> GetAndMarkAsReadAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotificationErrors.InvalidId;

            var notification = await _repository.GetByIdAsync(id);
            if (notification is null) return NotificationErrors.NotFound(id);

            if (!notification.IsRead)
            {
                await _repository.MarkAsReadAsync(id);
                notification.IsRead = true;
            }

            return _mapper.Map<NotificationResponseDto>(notification);
        }

        public async Task<Result<bool>> MarkAllAsReadAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return NotificationErrors.InvalidUserId;

            await _repository.MarkAllAsReadAsync(userId);
            return true;
        }

        public async Task<Result<int>> GetUnreadCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return NotificationErrors.InvalidUserId;

            var count = await _repository.GetUnreadCountAsync(userId);
            return count;
        }

        public async Task<Result<bool>> DeleteNotificationAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotificationErrors.InvalidId;

            var notification = await _repository.GetByIdAsync(id);
            if (notification is null) return NotificationErrors.NotFound(id);

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
