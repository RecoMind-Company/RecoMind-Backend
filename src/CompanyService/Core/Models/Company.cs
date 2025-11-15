using Core.Const;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Company
    {
        [Key]
        public string Id { get; set; }= Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Country { get; set; } 
        public string? Size { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PlanType { get; set; } = CompanyPlanType.Free.ToString();
        public string Billing { get; set; } = BillingCycle.Monthly.ToString();

        //[ForeignKey("Subscription")]
        //public string SubscriptionId {  get; set; }

        // Relationships
        //public CompanySubscription? Subscription { get; set; }               
        //public IEnumerable<CompanySetting> CompanySetting { get; set; }
    }
}
