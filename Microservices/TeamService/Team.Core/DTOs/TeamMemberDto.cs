using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.DTOs
{
    public class TeamMemberDto
    {
        public string? TeamId { get; set; }
        public List<string> EmployeesId { get; set; } = new List<string>();
    }
}
