using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Core.DTOs
{
    public class MappingRequestDto
    {
        public string CompanyId { get; set; }
        public string DeptName { get; set; }
        public List<int> TableIds { get; set; }
    }
}
