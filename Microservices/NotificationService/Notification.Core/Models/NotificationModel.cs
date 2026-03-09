using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.Models
{
    public class NotificationModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? senderId { get; set; }
        public string receiverId { get; set; }
        public string? planId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
