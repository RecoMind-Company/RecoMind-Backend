using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    public class Company
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Industry { get; set; }

        public string? CompanySize { get; set; }
        public string? Country { get; set; }
        public string? CompanyCode { get; set; }
        [ForeignKey("UserId")]
        public string? UserId { get; set; }

       
        public virtual AppUser? User { get; set; }


    }
}
