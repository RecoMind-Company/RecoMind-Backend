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
        //private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMapper _mapper;

        public NotificationService(
            //IHubContext<NotificationHub> hubContext,
            INotificationRepository repository,
            IMapper mapper)
        {
            //_hubContext = hubContext;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<NotificationResponseDto> SendNotificationAsync(NotificationModel notification)
        {
            // 1. الحفظ في الداتا بيز
            await _repository.AddAsync(notification);

            // 2. تحويل لـ DTO للإرسال
            var response = _mapper.Map<NotificationResponseDto>(notification);

            // 3. إرسال Real-time للمستخدم
            //await _hubContext.Clients.User(notification.receiverId)
            //    .SendAsync("ReceiveNotification", response);

            return response;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetUserHistoryAsync(string userId)
        {
            var notifications = await _repository.GetByReceiverIdAsync(userId);
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
            if (notification == null) return null;

            if (!notification.IsRead)
            {
                await _repository.MarkAsReadAsync(id);
                notification.IsRead = true; // تحديث الحالة في الأوبجكت الراجع
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
            // ملحوظة: تأكد من إضافة DeleteAsync في الـ Repository interface بتاعك
            // await _repository.DeleteAsync(id);
            return true;
        }

    }
}
