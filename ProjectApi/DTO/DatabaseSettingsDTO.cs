using Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectApi.DTO
{
    public class DatabaseSettingsDTO
    {
     
        public string Server { get; set; }

        public string DatabaseName { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string CompanyId { get; set; }
        
    }
}
