using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface ISubscriptionService
    {
        public Task<IEnumerable<GetSubscriptionDto>> GetAllSubscriptions();
        public Task<GetSubscriptionDto> GetSubscriptionById(string subscriptionId);
        public Task<GetSubscriptionDto> CreateSubscription(CreateSubscriptionDto subscriptionDto);
        public Task<UpdateSubscriptionDto> UpdateSubscription(string subscriptionId, CreateSubscriptionDto subscriptionDto);
        public Task<DeleteSubscriptionDto> DeleteSubscription(string subscriptionId);
    }
}
