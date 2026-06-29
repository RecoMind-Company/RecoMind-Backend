using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.PlanDtos.Approve
{
    public class PostIsApprovedDto
    {
        public string PlanId { get; set; }
        public bool IsAproved { get; set; }
        public string? Feedback { get; set; }
    }
}
