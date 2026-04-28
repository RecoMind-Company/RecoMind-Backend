using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team.Core.Models
{
    public class TeamEmployee
    {
        [Key]
        public string Id { get; set; }
         
        [ForeignKey("Team")]
        public string TeamId { get; set; }
        public TeamModel Team { get; set; }
        public string EmployeeId { get; set; }
    }
}
