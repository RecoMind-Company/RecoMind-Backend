using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.DTOs
{
    public class UsersToReturnDto
    {
        public List<string> Usernames { get; set; } = new List<string>();
        public int EmpCount => Usernames.Count;
    }
}
