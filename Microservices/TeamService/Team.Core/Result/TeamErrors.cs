using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.Result
{
    public static class TeamErrors
    {
        public static Error NotFound = new("Team.NotFound", "The requested team was not found.");
        public static Error NameAlreadyExists = new("Team.DuplicateName", "A team with the same name already exists in this company.");
        public static Error EmployeeAlreadyInTeam = new("Team.EmployeeExists", "This employee is already a member of this team.");
        public static Error UnauthorizedAccess = new("Team.Unauthorized", "You do not have permission to access this team.");
    }
}
