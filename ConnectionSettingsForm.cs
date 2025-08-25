using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;

namespace Eximp
{
    public partial class ConnectionSettingsForm : Form
    {
        public ConnectionSettingsForm()
        {
            InitializeComponent();
            this.Load += ConnectionSettingsForm_Load;
        }

        private static string BuildOracleDataSourceUI(string server, string serviceName)
        {
            return server.Contains(":") ? $"{server}/{serviceName}" : $"{server}:1521/{serviceName}";
        }

        #region Build Connection String
        private static string BuildConnectionString(string provider, string server, string database, string username, string password)
        {
            return provider switch
            {
                "System.Data.SqlClient" or "Microsoft.Data.SqlClient" =>
                    $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;",
                "MySqlConnector" or "MySql.Data.MySqlClient" =>
                    $"Server={server};Database={database};User Id={username};Password={password};SslMode=None;AllowPublicKeyRetrieval=True",
                "Npgsql" =>
                    $"Host={server};Database={database};Username={username};Password={password};SSL Mode=Disable;",
                "Oracle.ManagedDataAccess.Client" =>                  // ✅ updated
                    $"User Id={username};Password={password};Data Source={BuildOracleDataSourceUI(server, database)};",
                _ => throw new Exception("Unsupported provider.")
            };
        }
        #endregion
        #region Test Connection
        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                string provider = cboProvider.SelectedItem?.ToString() ?? "";
                string server = txtServer.Text.Trim();
                string database = txtDatabase.Text.Trim();
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                string connStr = BuildConnectionString(provider, server, database, username, password);

                using var conn = DbProviderFactories.GetFactory(provider).CreateConnection();
                conn.ConnectionString = connStr;
                conn.Open();

                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        #region Save Connection Settings
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string provider = cboProvider.SelectedItem?.ToString() ?? "";
                string server = txtServer.Text.Trim();
                string database = txtDatabase.Text.Trim();
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                // Build provider-specific connection string
                string connectionString = provider switch
                {
                    "System.Data.SqlClient" or "Microsoft.Data.SqlClient" =>
                        $"Server={server};Database={database};User Id={username};Password={password};TrustServerCertificate=True;",
                    "MySqlConnector" or "MySql.Data.MySqlClient" =>
                        $"Server={server};Database={database};User Id={username};Password={password};SslMode=None;AllowPublicKeyRetrieval=True",
                    "Npgsql" =>
                        $"Host={server};Database={database};Username={username};Password={password};SSL Mode=Disable;",
                    "Oracle.ManagedDataAccess.Client" =>
                        $"User Id={username};Password={password};Data Source={server}/{database};",
                    _ => throw new Exception("Unsupported provider.")
                };

                // Test connection before saving
                using (var conn = DbProviderFactories.GetFactory(provider).CreateConnection())
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();
                }

                // ✅ Ensure folder exists
                Directory.CreateDirectory(Path.GetDirectoryName(Db.ConfigFilePath)!);

                // ✅ Save to unified config path
                File.WriteAllLines(Db.ConfigFilePath, new[]
                {
            provider,
            server,
            database,
            username,
            password
        });

                MessageBox.Show("Database connection settings saved successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect: " + ex.Message,
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        #region Load Setttings
        private void ConnectionSettingsForm_Load(object sender, EventArgs e)
        {
            // Populate providers dropdown
            cboProvider.Items.Clear();
            cboProvider.Items.Add("System.Data.SqlClient");
            cboProvider.Items.Add("Microsoft.Data.SqlClient");   // only if registered above
            cboProvider.Items.Add("MySqlConnector");
            cboProvider.Items.Add("MySql.Data.MySqlClient");     // only if registered above
            cboProvider.Items.Add("Npgsql");
            cboProvider.Items.Add("Oracle.ManagedDataAccess.Client");


            try
            {
                if (File.Exists(Db.ConfigFilePath))
                {
                    var lines = File.ReadAllLines(Db.ConfigFilePath);
                    if (lines.Length >= 5)
                    {
                        string savedProvider = lines[0].Trim();
                        if (cboProvider.Items.Contains(savedProvider))
                            cboProvider.SelectedItem = savedProvider;
                        else
                            cboProvider.SelectedIndex = 0; // fallback

                        txtServer.Text = lines[1].Trim();
                        txtDatabase.Text = lines[2].Trim();
                        txtUsername.Text = lines[3].Trim();
                        txtPassword.Text = lines[4].Trim();
                    }
                    else
                    {
                        cboProvider.SelectedIndex = 0;
                    }
                }
                else
                {
                    cboProvider.SelectedIndex = 0; // No config file, default to first option
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading saved configuration: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboProvider.SelectedIndex = 0;
            }
        }
        #endregion
        private string BuildConnectionString()
        {
            string provider = cboProvider.SelectedItem?.ToString() ?? "";
            return BuildConnectionString(
                provider,
                txtServer.Text.Trim(),
                txtDatabase.Text.Trim(),
                txtUsername.Text.Trim(),
                txtPassword.Text.Trim());
        }


        #region Notneeded
        private void lblPassword_Click(object sender, EventArgs e)
        {

        }
        #endregion
    }
}

