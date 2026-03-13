using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.DTOs
{
    public class NotificationResponseDto
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string Message { get; set; }
        public string? SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string? PlanId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
