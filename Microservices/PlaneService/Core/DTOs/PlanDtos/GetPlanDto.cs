using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.PlanDtos
{
    public class GetPlanDto
    {        
        public string Id { get; set; }
        public string Description { get; set; }
        public string Goal { get; set; }
        public string PlanType { get; set; }
        public string Status { get; set; }
        public bool IsApproved { get; set; }
        public string Duration { get; set; }
    }
}
