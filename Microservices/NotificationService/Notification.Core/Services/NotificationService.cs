using AutoMapper;
using Microsoft.Extensions.Logging;
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
        private readonly IPushNotificationService _fcmService;
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;

        public NotificationService(
            IPushNotificationService fcmService,
            ILogger<NotificationService> logger,
            INotificationHubContext hubContext,
            INotificationRepository repository,
            IMapper mapper)
        {
            _hubContext = hubContext;
            _repository = repository;
            _fcmService = fcmService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<bool>> SendNotificationAsync(NotificationEventDto dto)
        {
            if (dto == null) return new Error("Notification.BadRequest", "Notification data is required.");

            var notification = _mapper.Map<NotificationModel>(dto);
            notification.Id = Guid.NewGuid().ToString();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            // 1. Save to database
            await _repository.AddAsync(notification);

            var response = _mapper.Map<NotificationResponseDto>(notification);

            // 2. Send real-time notification (SignalR - Web/Active Users)
            await _hubContext.SendNotificationAsync(notification.ReceiverId, response);

            // 3. Send Push Notification (Firebase - Mobile Users)
            try
            {
                var deviceTokens = await _repository.GetUserDeviceTokensAsync(notification.ReceiverId);

                if (deviceTokens != null && deviceTokens.Any())
                {
                    foreach (var token in deviceTokens)
                    {
                        await _fcmService.SendPushNotificationAsync(
                            token,
                            notification.Title,
                            notification.Message,
                            new Dictionary<string, string>
                            {
                                { "notificationId", notification.Id },
                                { "senderId", notification.SenderId ?? string.Empty },
                                { "planId", notification.PlanId ?? string.Empty }
                            }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "An error occurred while sending Firebase Push Notification to Receiver: {ReceiverId}",
                    notification.ReceiverId);
            }

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


        public async Task<Result<bool>> RegisterDeviceTokenAsync(string userId, RegisterDeviceTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return NotificationErrors.InvalidUserId;

            if (string.IsNullOrWhiteSpace(dto.DeviceToken))
                return new Error("Notification.InvalidToken", "Device token cannot be empty.");

            var existingToken = await _repository.FindDeviceTokenAsync(userId, dto.DeviceToken);

            if (existingToken != null)
            {
                existingToken.UpdatedAt = DateTime.UtcNow;
                existingToken.DeviceType = dto.DeviceType;
                await _repository.UpdateDeviceTokenAsync(existingToken);
            }
            else
            {
                var newToken = new UserDeviceToken
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    DeviceToken = dto.DeviceToken,
                    DeviceType = dto.DeviceType,
                    UpdatedAt = DateTime.UtcNow
                };
                await _repository.AddDeviceTokenAsync(newToken);
            }

            return true;
        }
    }
}
