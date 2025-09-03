using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    
    public class DatabaseSettings
    {
        public string Id { get; set; }

        public string Server { get; set; }

        public string DatabaseName { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        [ForeignKey("CompanyId")]
        public string CompanyId { get; set; }
        public virtual Company Company { get; set; }



    }
}
