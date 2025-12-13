using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.DTOs
{
    public class DbSettingResponseForAiDto
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Server { get; set; }
        public string DbName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
