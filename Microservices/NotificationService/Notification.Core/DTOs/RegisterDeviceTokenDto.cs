using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Core.DTOs
{
    public class RegisterDeviceTokenDto
    {
        public string DeviceToken { get; set; } = string.Empty;
        public string? DeviceType { get; set; } // اختياري (Android / iOS)
    }
}
