using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class CreateCompanyDTO
    {
        public string Name { get; set; }
        public string Industry { get; set; } 
        public string Country { get; set; } 
        public string Size { get; set; }
<<<<<<< HEAD
<<<<<<< Updated upstream
        public string Code {  get; set; }
        public string BusinessDescription { get; set; }
=======
        public string Code {  get; set; }         
        public string Description { get; set; }

>>>>>>> Stashed changes
        public string? AdminId { get; set; }
=======
        public string Code {  get; set; }         
>>>>>>> b55309eff502b485ffbac0fff343644a670244ed
        public string? SubscriptionId { get; set; }
    }
}
