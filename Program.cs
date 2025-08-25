using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace Eximp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RegisterProviders(); // ? before showing any forms

            if (!File.Exists("dbconfig.txt"))
            {
                using var f = new ConnectionSettingsForm();
                if (f.ShowDialog() != DialogResult.OK) return;
            }

            Application.Run(new Main());
        }


        #region Register Providers
        // ? CLASS-SCOPE method
        private static void RegisterProviders()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
            
            DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);
        }
        #endregion
    }
}
