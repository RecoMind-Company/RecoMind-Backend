using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.DTOs
{
    public class UserTeamInfoDto
    {
        public string CompanyId { get; set; }
        public string TeamId { get; set; }
        public string TeamName { get; set; }
    }
}
