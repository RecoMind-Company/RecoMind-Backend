using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class DeleteSubscriptionCompanyDto
    {
        public string Id { get; set; }
        public string Message{ get; set; } = "Subscription Deleted Successfully.";
    }
}
