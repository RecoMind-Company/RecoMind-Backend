using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.SubscriptionTypeDto
{
    public class CreateDto
    {
        public string PlanName { get; set; }
        public double Price { get; set; }
    }
}
