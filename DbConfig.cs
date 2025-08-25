using System;
using System.IO;

namespace Eximp
{
    public static class DbConfig
    {
        public static string GetConnectionString()
        {
            if (!File.Exists("dbconfig.txt"))
                throw new Exception("Database connection not configured.");

            var lines = File.ReadAllLines("dbconfig.txt");
            if (lines.Length < 5)
                throw new Exception("Invalid dbconfig.txt format. Expected 5 lines: Provider, Server, Database, Username, Password.");

            string provider = lines[0].Trim();
            string server = lines[1].Trim();
            string database = lines[2].Trim();
            string username = lines[3].Trim();
            string password = lines[4].Trim();

            return provider switch
            {
                "System.Data.SqlClient" or "Microsoft.Data.SqlClient" =>
                    $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;",

                "MySqlConnector" or "MySql.Data.MySqlClient" =>
                    $"Server={server};Database={database};User Id={username};Password={password};" +
                    "SslMode=None;AllowPublicKeyRetrieval=True",

                "Npgsql" =>
                    $"Host={server};Database={database};Username={username};Password={password};SSL Mode=Disable;",

                "Oracle.ManagedDataAccess.Client" =>
                    $"User Id={username};Password={password};Data Source={server}:1521/{database};",

                _ => throw new Exception($"Unsupported provider: {provider}")

            };
        }
    }
}