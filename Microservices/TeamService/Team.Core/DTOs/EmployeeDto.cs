using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.DTOs
{
    public class EmployeeDto
    {
        [Required]
        public string EmployeeId { get; set; }

    }
}
