using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class GetCompanyDTO 
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Industry { get; set; }
        public string? Country { get; set; }
        public string? AdminId { get; set; }
        public string? SubscriptionId { get; set; }
        public DateTime CreatedAt { get; set; }     
    }
}
