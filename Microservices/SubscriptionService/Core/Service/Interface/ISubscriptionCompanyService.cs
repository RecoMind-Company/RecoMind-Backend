using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface ISubscriptionCompanyService
    {
        public Task<IEnumerable<GetSubscriptionCompanyDto>> GetAllSubscriptions();
        public Task<GetSubscriptionCompanyDto> GetSubscriptionById(string subscriptionId);
        public Task<GetSubscriptionCompanyDto> CreateSubscription(CreateSubscriptionCompanyDto subscriptionDto);
        public Task<GetSubscriptionCompanyDto> UpdateSubscription(string subscriptionId, CreateSubscriptionCompanyDto subscriptionDto);
        public Task<DeleteSubscriptionCompanyDto> DeleteSubscription(string subscriptionId);
    }
}
