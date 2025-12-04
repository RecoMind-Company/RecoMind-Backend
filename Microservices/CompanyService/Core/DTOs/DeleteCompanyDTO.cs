using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class DeleteCompanyDTO
    {
        public  string? Id { get; set; } 
        public string? Massage { get; set; } = $" Company Deleted Successfuly ";    
    }
}
