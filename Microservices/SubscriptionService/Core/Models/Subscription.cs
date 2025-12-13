using Core.Consts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SubscriptionCompany
    {
        public string Id { get; set; }
        public double Price{ get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }               
        public string BillingCycle { get; set; }

        [ForeignKey("subscriptionId")]
        public string SubscriptionTypeId { get; set; } 
        public SubscriptionType? subscriptionType { get; set; }
    }

    public class SubscriptionType
    {
        public string SubscriptionTypeId { get; set; }
        public string PlanName { get; set; }
        public double Price{ get; set; }
    }
}
