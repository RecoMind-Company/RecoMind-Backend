using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.DTOs
{
    public class CreateDbSettingModel
    {
        [Required]
        public string Name { get; set; }
  
        [Required]
        public string DbType { get; set; } // SqlServer, MySql, PostgreSql
        
        [Required]
        public string ConnectionString { get; set; }
    }
}
