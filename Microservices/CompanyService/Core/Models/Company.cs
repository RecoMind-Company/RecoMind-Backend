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
        public string Id { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Country { get; set; }
        public string? Size { get; set; }
        public string Code { get; set; }
<<<<<<< Updated upstream
        public string BusinessDescription { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        public string? SubscriptionId { get; set; }
        public string? AdminId { get; set; }

=======
        public DateTime CreatedAt { get; set; }        
        public string? Description { get; set; }

        // Relationships
        public string? SubscriptionId { get; set; }  
        public string? AdminId { get; set; }
>>>>>>> Stashed changes
        //public IEnumerable<string> CompanySettingsId { get; set; }
        //public IEnumerable<string> TeamsId { get; set; }
    }
}
