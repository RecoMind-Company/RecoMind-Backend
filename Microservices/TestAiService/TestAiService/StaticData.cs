namespace TestAiService
{
    public static class StaticData
    {
        public static string server = "tcp:sqlrecomindserver.database.windows.net";
        public static string databaseName = "AdventureWorks2014";
        public static string user = "recomind";
        public static string password = "Server12";
        public static string companyId = "fb140d33-7e96-474d-a06d-ab3a6c65d1a9";

        public static List<string> tableNames = new List<string>
        {
            "Sales",
            "HR",
            "Operations",
            "Production",
            "Purchasing",
            "Shared/General"
        };
        public static string FullConnectionString =>
            $"Server={server};Database={databaseName};User Id={user};Password={password};";
    }
}