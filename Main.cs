using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2013.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Eximp
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private async Task TestDbAsync()
        {
            lblStatus.Text = "Testing connection...";
            try
            {
                var ok = await Db.TestConnectionAsync();
                lblStatus.Text = ok ? "DB connection OK." : "DB connection failed.";
            }
            catch (Exception ex)
            {
                // Take only the first sentence for the label
                string shortMessage = ex.Message.Split('.')[0] + ".";

                // Show short message in lblStatus
                lblStatus.Text = "Error: " + shortMessage;

                // Show full error in a popup
                MessageBox.Show(ex.ToString(), "Detailed Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void btnTestDb_Click(object sender, EventArgs e)
        {
            await TestDbAsync();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using var f = new ImportForm();
            f.ShowDialog(this);
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            using var f = new FilterForm();
            f.ShowDialog();
        }
        #region Manual Export
        //Relates to the method that does NOT render Dynamically
        //private async void btnExport_Click(object sender, EventArgs e)
        //{
        //    using (var sfd = new SaveFileDialog())
        //    {
        //        sfd.Filter = "Excel Files|*.xlsx";
        //        sfd.FileName = "SchoolExport.xlsx";

        //        if (sfd.ShowDialog() == DialogResult.OK)
        //        {
        //            try
        //            {
        //                await Db.ExportToExcelAsync(sfd.FileName);
        //                MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            }
        //        }
        //    }
        //}
        #endregion


        #region Dynamic Export
        //Relates to the that renders Dynamically

        private async void btnExport_Click(object sender, EventArgs e)
        {
            // Step 1: Get all tables from the DB
            DataTable tables = await Db.ListTablesAsync();

            if (tables.Rows.Count == 0)
            {
                MessageBox.Show("No tables found in the database.");
                return;
            }

            // Step 2: Show selection dialog with a ComboBox
            Form selectTableForm = new Form()
            {
                Width = 300,
                Height = 120,
                Text = "Select Table",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };

            ComboBox cboTables = new ComboBox() { Left = 20, Top = 20, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (DataRow row in tables.Rows)
                cboTables.Items.Add(row["FullName"].ToString());

            cboTables.SelectedIndex = 0;

            Button btnOk = new Button() { Text = "OK", Left = 100, Width = 80, Top = 60, DialogResult = DialogResult.OK };
            selectTableForm.Controls.Add(cboTables);
            selectTableForm.Controls.Add(btnOk);
            selectTableForm.AcceptButton = btnOk;

            if (selectTableForm.ShowDialog() == DialogResult.OK)
            {
                string tableName = cboTables.SelectedItem.ToString();

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Files|*.xlsx";
                    sfd.FileName = tableName.Replace(".", "_") + "_Export.xlsx";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            await Db.ExportToExcelAsync(sfd.FileName, tableName);
                            MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        #endregion

        #region Export to PDF

        private async void btnExportPdf_Click(object sender, EventArgs e)
        {
            // 1) Load tables dynamically
            DataTable tables = await Db.ListTablesAsync();
            if (tables.Rows.Count == 0)
            {
                MessageBox.Show("No tables found in the database.");
                return;
            }

            // 2) Mini dialog with a ComboBox to pick the table
            using var selectTableForm = new Form
            {
                Width = 300,
                Height = 120,
                Text = "Select Table",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };

            var cboTables = new ComboBox
            {
                Left = 20,
                Top = 20,
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (DataRow row in tables.Rows)
                cboTables.Items.Add(row["FullName"].ToString());
            cboTables.SelectedIndex = 0;

            var btnOk = new Button { Text = "OK", Left = 100, Width = 80, Top = 60, DialogResult = DialogResult.OK };
            selectTableForm.Controls.Add(cboTables);
            selectTableForm.Controls.Add(btnOk);
            selectTableForm.AcceptButton = btnOk;

            if (selectTableForm.ShowDialog() != DialogResult.OK)
                return;

            string tableName = cboTables.SelectedItem.ToString();

            // 3) Ask where to save the PDF
            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = tableName.Replace(".", "_") + "_Export.pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                await Db.ExportToPdfAsync(sfd.FileName, tableName);
                MessageBox.Show("PDF export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Change Connection
        private void btnChangeConnection_Click(object sender, EventArgs e)
        {
            using (var form = new ConnectionSettingsForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string connStr = DbConfig.GetConnectionString();
                    MessageBox.Show("Connection settings updated.",
                        "Connection Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Optionally reload data with the new connection string
                    // LoadDataFromDatabase(connStr);
                }
            }
        }
        #endregion
    }
}
