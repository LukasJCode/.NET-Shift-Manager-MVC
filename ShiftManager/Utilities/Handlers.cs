namespace ShiftManager.Utilities
{
    public static class Handlers
    {
        public static string GetConnectionString()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            //reading configuration settings from json file
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            var connectionString = configuration.GetConnectionString("DbConnection");

            //handling possible issues with connection string
            if (connectionString == null || connectionString == "")
            {
                Console.WriteLine("Invalid Databse Connection String");
                return "";
            }

            return connectionString;
        }
    }
}
