using Core.DTOs.SubscriptionTypeDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface ISubscriptionTypeService
    {
        Task<GetDto> AddSubscriptionPlan(CreateDto planType);
        Task<IEnumerable<GetDto>> GetAllSubscriptionPlan();
        Task<DeleteDto> DeleteSubscriptionType(string PlanName);
        Task<GetDto> UpdateSubscriptionType(string Id , CreateDto planType);
        Task<string> GetId(string PlanNeme);
        Task<double> GetPrice(string PlanName);
        Task<bool> CheckPlanName (string PlanName);
    }
}
