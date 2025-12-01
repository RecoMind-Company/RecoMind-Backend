using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class UpdateCompanyDTO : CreateCompanyDTO
    {
        public string Id { get; set; }        
        public string Massage { get; set; } =  $" Company Updated Successfuly " ;
    }
}
