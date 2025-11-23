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
        public string Code {  get; set; } 
    }
}
