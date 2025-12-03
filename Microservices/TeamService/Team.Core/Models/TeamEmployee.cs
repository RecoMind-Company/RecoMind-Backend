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


        [ForeignKey("TeamId")]
        public TeamModel Team { get; set; }
        public string TeamId { get; set; }

        public string EmployeeId { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
