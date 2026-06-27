using Core.DTOs.AI;
using Core.DTOs.PlanDtos;
using Core.Models;

namespace Core.Service.Interface
{
    public interface IPlanService
    {
        Task<Result<GetPlanDto>> GetPlanById(string planId, string companyId);
        Task<IEnumerable<Result<GetPlanDto>>> GetPlansByStatus(string status, string companyId);
        Task<IEnumerable<Result<GetPlanDto>>> GetAllPlans(string companyId);
        Task<Result<GetPlanDto>> CreatePlan(AddPlanDto createPlanDto, string companyId, string userId);
        Task<Result<GetPlanDto>> UpdatePlan(string companyId, string userId, UpdatePlanDto dto);
        Task<bool> DeletePlan(string planId, string companyId);
        Task<Result<AIPlanDto>> CreateCustomPlan(UserCustomPlanDto userCustomPlanDto, string companyId, string userId);



    }
}
