using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.Result
{
    public static class DbSettingErrors
    {
        public static readonly Error NotFound = new("DbSetting.NotFound", "Database settings for this company were not found.");
        public static readonly Error ConnectionFailed = new("DbSetting.ConnectionFailed", "Could not connect to the database with the provided credentials.");
        public static readonly Error AlreadyExists = new("DbSetting.AlreadyExists", "Database settings for this company already exist.");
        public static readonly Error EncryptionError = new("DbSetting.Encryption", "An error occurred while encrypting the database settings.");
    }
}
