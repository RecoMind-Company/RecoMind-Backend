using Core.DTOs.PlanDtos.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.PlanDtos
{
    public class UpdatePlanDto : AddPlanDto
    {
        public string PlanId { get; set; }
        public string Status { get; set; }
    }
}
