using Core.DTOs.PlanDtos;
using Core.DTOs.PlnaTypeDtos;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface IPlanService
    {
         Task<Result<GetPlanDto>> GetPlanById(string planId , string companyId);
         Task<IEnumerable<Result<GetPlanDto>>> GetPlansByStatus(string status , string companyId);
         Task<IEnumerable<Result<GetPlanDto>>> GetAllPlans(string companyId);
         Task<Result<GetPlanDto>> CreatePlan(AddPlanDto createPlanDto, string companyId , string userId);
         Task<Result<GetPlanDto>> UpdatePlan(string companyId , string userId ,UpdatePlanDto dto);
         Task<bool> DeletePlan(string planId , string companyId ); 
    }
}
