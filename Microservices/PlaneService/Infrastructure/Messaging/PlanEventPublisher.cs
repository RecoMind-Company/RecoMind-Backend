using Core.Interfaces;
using MassTransit;
using RecoMind.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging
{
    public class PlanEventPublisher : IPlanEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PlanEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishNotificationAsync(NotificationEventDto notificationEvent)
        {
            // MassTransit هيبعت الـ Event تلقائياً للـ Exchange المشترك
            await _publishEndpoint.Publish(notificationEvent);
        }
    }
}

