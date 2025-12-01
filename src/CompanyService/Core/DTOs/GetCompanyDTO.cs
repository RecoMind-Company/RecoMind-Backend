using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class GetCompanyDTO : CreateCompanyDTO
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }     
    }
}
