using Core.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Subscription
    {
        public string Id { get; set; }
        public string PlanName { get; set; }
        public double Price{ get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }               // status
        public string BillingCycle { get; set; }

        // public string Companyid { get; set; }
    }
}
