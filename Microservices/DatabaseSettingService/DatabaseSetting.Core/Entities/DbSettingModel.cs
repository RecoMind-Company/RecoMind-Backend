namespace DatabaseSetting.Core.Entities
{
    public class DbSettingModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string CompanyId { get; set; }
        public string? DbType { get; set; }

        // Database connection details
        public string Server { get; set; }
        public string DbName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
