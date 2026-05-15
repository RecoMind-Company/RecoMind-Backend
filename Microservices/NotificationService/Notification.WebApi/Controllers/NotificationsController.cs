using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _notificationService.GetUserHistoryAsync(UserId);
            return result.Map<IActionResult>(
                Ok, BadRequest);
        }

        [HttpGet("filter")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetByStatus([FromQuery] bool isRead)
        {
            var result = await _notificationService.GetByStatusAsync(UserId, isRead);
            return result.Map<IActionResult>(
                Ok, BadRequest);
        }

        [HttpGet("unread-count")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _notificationService.GetUnreadCountAsync(UserId);
            return result.Map<IActionResult>(
                count => Ok(new { count })
                , BadRequest);
        }

        [HttpPatch("{id}/mark-as-read")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var result = await _notificationService.GetAndMarkAsReadAsync(id);

            return result.Map<IActionResult>(
                onSuccess: notification => Ok(notification),
                onFailure: error => error.Code == "Notification.NotFound"
                    ? NotFound(error)
                    : BadRequest(error)
            );
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);

            return result.Map<IActionResult>(
                onSuccess: _ => NoContent(),
                onFailure: error => error.Code == "Notification.NotFound"
                    ? NotFound(error)
                    : BadRequest(error)
            );
        }

        [HttpPost("register-device")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceTokenDto dto)
        {
            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();

            var result = await _notificationService.RegisterDeviceTokenAsync(UserId, dto);

            return result.Map<IActionResult>(
                onSuccess: _ => Ok(new { message = "Device token registered or updated successfully." }),
                onFailure: error => BadRequest(error)
            );
        }
    }
}
