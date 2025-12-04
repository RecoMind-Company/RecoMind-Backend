using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class CreateSubscriptionDto
    {
        public string PlanName { get; set; }
        public string BillingCycle { get; set; }
    }
}
