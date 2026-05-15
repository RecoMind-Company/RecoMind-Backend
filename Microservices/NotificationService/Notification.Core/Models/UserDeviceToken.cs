using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.Models
{
    public class UserDeviceToken
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public string DeviceToken { get; set; } = string.Empty;

        public string? DeviceType { get; set; } // "Android", "iOS"
        public DateTime? UpdatedAt { get; set; }
    }
}
