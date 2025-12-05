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

        // Optional name for the database setting
        public string? Name { get; set; }
        public string CompanyId { get; set; }
        public string? DbType { get; set; } // SqlServer, MySql, PostgreSql

        // Database connection details
        public string Server { get; set; }
        public string DbName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        // End database connection details

        public string ConnectionString { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
