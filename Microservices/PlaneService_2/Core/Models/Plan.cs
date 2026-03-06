using Core.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Plan
    {
        public string Id { get; set; }
        public string Goal { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public PlanType PlanType { get; set; }
        public bool IsApproved { get; set; }
        public string Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // **************

        public string Owner_Id { get; set; }   // User who created the plan "from cocky"
        public string Company_Id { get; set; } // Company to which the plan belongs "from cocky"
        public string Team_Id { get; set; }    // Team to which the plan belongs "from cocky"
    }
}
