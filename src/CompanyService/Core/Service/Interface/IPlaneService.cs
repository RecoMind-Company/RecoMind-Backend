using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface IPlaneService 
    {
        public IEnumerable<string> GetAllPlans();
        public bool CheckPlanName(string planName);
        public  Task<GetCompanyDTO> AssignPlane(string Id, string PlaneName);
    }
}
