using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.Core.Interfaces;

namespace Notification.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // جلب كل الإشعارات
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetHistory(string userId)
        {
            var result = await _notificationService.GetUserHistoryAsync(userId);
            return Ok(result);
        }

        // فلترة الإشعارات (مقروءة أو غير مقروءة)
        [HttpGet("user/{userId}/filter")]
        public async Task<IActionResult> GetByStatus(string userId, [FromQuery] bool isRead)
        {
            var result = await _notificationService.GetByStatusAsync(userId, isRead);
            return Ok(result);
        }

        // جلب عدد غير المقروء (للرقم اللي فوق الجرس)
        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(string userId)
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        // فتح إشعار وتعليمه كمقروء
        [HttpPatch("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var result = await _notificationService.GetAndMarkAsReadAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // حذف إشعار
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _notificationService.DeleteNotificationAsync(id);
            if (!success) return BadRequest();
            return NoContent();
        }
    }
}
