using RecoMind.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IPlanEventPublisher
    {
        Task PublishNotificationAsync(NotificationEventDto notification);
    }
}
