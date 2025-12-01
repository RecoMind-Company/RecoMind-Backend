using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.Entities
{
    public class DbSettingModel
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string DbType { get; set; } // SqlServer, MySql, PostgreSql
        public string ConnectionString { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
