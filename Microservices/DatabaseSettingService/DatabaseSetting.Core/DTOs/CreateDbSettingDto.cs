using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.DTOs
{
    public class CreateDbSettingDto
    {
        public string Name { get; set; }
        public string DbType { get; set; }

        [Required]
        public string Server { get; set; }
        [Required]
        public string DbName { get; set; }
        [Required]
        public string User { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
