using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.SubscriptionTypeDto
{
    public class GetDto
    {
        public string Id { get; set; }
        public string PlanName { get; set; }
        public double Price { get; set; }
    }
}
