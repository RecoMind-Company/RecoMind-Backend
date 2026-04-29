using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Notification.Core.Result
{
    public static class NotificationErrors
    {
        public static Error NotFound(string id) =>
            new("Notification.NotFound", $"Notification with ID {id} was not found.");

        public static Error InvalidUserId =
            new("Notification.InvalidUser", "User ID cannot be null or empty.");

        public static Error FailureToSend =
            new("Notification.SendFailure", "An error occurred while sending the real-time notification.");

        public static Error InvalidId =
            new Error("Notification.InvalidId", "Notification ID is required.");
    }
}
