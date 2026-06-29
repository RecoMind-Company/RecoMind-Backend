using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.PlanDtos.Plan
{
    public class AddPlanDto
    {
        public string Description { get; set; }
        public string Goal { get; set; }
        public string PlanType { get; set; }
    }
}
