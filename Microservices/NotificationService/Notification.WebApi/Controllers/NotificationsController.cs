using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.Core.DTOs;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using System.Security.Claims;

namespace Notification.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send-test")]
        public async Task<IActionResult> SendTestNotification([FromBody] NotificationEventDto model)
        {
            await _notificationService.SendNotificationAsync(model);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _notificationService.GetUserHistoryAsync(UserId);
            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetByStatus([FromQuery] bool isRead)
        {
            var result = await _notificationService.GetByStatusAsync(UserId, isRead);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(UserId);
            return Ok(new { count });
        }

        [HttpPatch("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var result = await _notificationService.GetAndMarkAsReadAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _notificationService.DeleteNotificationAsync(id);
            if (!success) return BadRequest();
            return NoContent();
        }
    }
}
