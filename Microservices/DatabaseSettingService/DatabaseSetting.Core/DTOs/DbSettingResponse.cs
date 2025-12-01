using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.DTOs
{
    public class DbSettingResponse
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string DbType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
