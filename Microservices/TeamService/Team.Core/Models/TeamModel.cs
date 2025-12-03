using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.Models
{
    public class TeamModel
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string TeamLeadId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<TeamEmployee> TeamEmployees { get; set; } = new List<TeamEmployee>();
    }
}
