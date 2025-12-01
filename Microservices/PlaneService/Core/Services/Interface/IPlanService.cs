using Core.DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Interface
{
    public interface IPlanService
    {
        public Task<GetPlaneDto> CreatePlan( CreatePlanDto Plan );
        public Task<GetPlaneDto> UpdatePlan(string PlanId , CreatePlanDto Plan );
        public Task<DeletePlanDto> DeletePlan( string PlanId );
        public Task<GetPlaneDto> GetPlan( string PlanId );
        public Task<IEnumerable<GetPlaneDto>> GetAllPlansByTeamId(string TeamId);
        public Task<IEnumerable<GetPlaneDto>> GetAllPlans( );
    }
}
