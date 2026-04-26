using Core.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class CreateSubscriptionCompanyDto
    {
        public string PlanName { get; set; }
        public BillingCycle BillingCycle { get; set; }
    }
}
