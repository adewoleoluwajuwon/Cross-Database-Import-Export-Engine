using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Linq;

namespace Eximp
{
    public class ImportForm : Form
    {
        private ComboBox cboSheets;
        private ComboBox cboTables;
        private Button btnFile;
        private Button btnPreview;
        private Button btnImport;
        private DataGridView grid;
        private Label lblInfo;
        private CancellationTokenSource? _cts;
        private ProgressBar prg;
        private Button btnCancel;
        private ComboBox cboDup; // Error / Skip / Upsert


        private string? _filePath;

        

        public ImportForm()
        {
            Text = "Import From Excel (OLE DB)";
            Width = 1000;
            Height = 680;
            StartPosition = FormStartPosition.CenterParent;

            btnFile = new Button { Text = "Choose Excel...", Left = 20, Top = 20, Width = 140 };
            cboSheets = new ComboBox { Left = 180, Top = 20, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTables = new ComboBox { Left = 420, Top = 20, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            btnPreview = new Button { Text = "Preview Sheet", Left = 700, Top = 20, Width = 120 };
            btnImport = new Button { Text = "Bulk Import", Left = 840, Top = 20, Width = 120 };
            lblInfo = new Label { Left = 20, Top = 55, Width = 940, Height = 35, AutoEllipsis = true };

            prg = new ProgressBar { Left = 20, Top = 75, Width = 940, Height = 10, Minimum = 0, Step = 1 };
            btnCancel = new Button { Text = "Cancel", Left = 840, Top = 55, Width = 120, Enabled = false };
            cboDup = new ComboBox { Left = 700, Top = 55, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDup.Items.AddRange(new object[] { "Error", "Skip", "Upsert" });
            cboDup.SelectedIndex = 1; // default to Skip

            btnCancel.Click += (_, __) => _cts?.Cancel();

            Controls.AddRange(new Control[] { prg, btnCancel, cboDup });


            grid = new DataGridView
            {
                Left = 20,
                Top = 100,
                Width = 940,
                Height = 520,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
            };

            btnFile.Click += async (_, __) => await PickFileAsync();
            btnPreview.Click += async (_, __) => await PreviewAsync();
            btnImport.Click += async (_, __) => await DoImportAsync();
            Load += async (_, __) => await LoadTablesAsync();

            Controls.AddRange(new Control[] { btnFile, cboSheets, cboTables, btnPreview, btnImport, lblInfo, grid });
        }

        private async Task LoadTablesAsync()
        {
            var dt = await Db.ListTablesAsync();
            cboTables.Items.Clear();
            foreach (DataRow r in dt.Rows)
                cboTables.Items.Add(Convert.ToString(r["FullName"]));
            if (cboTables.Items.Count > 0) cboTables.SelectedIndex = 0;
        }

        private static string BuildExcelOleDbConn(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".xlsx" or ".xlsm" or ".xlsb" =>
                    $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";",
                ".xls" =>
                    $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\";",
                _ => throw new NotSupportedException($"Unsupported file type: {ext}")
            };
        }

        private async Task PickFileAsync()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls;*.xlsm;*.xlsb",
                Title = "Pick an Excel file"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            _filePath = ofd.FileName;
            try
            {
                await LoadSheetsFromExcelAsync(_filePath);
                lblInfo.Text = $"Loaded: {Path.GetFileName(_filePath)} | Sheets: {cboSheets.Items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    ex.Message + "\n\nIf you see 'provider not registered', install 'Microsoft Access Database Engine 2016' and match x86/x64 to your project platform.",
                    "Load Excel failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Task LoadSheetsFromExcelAsync(string filePath)
        {
            cboSheets.Items.Clear();
            using var wb = new XLWorkbook(filePath);
            foreach (var ws in wb.Worksheets)
                cboSheets.Items.Add(ws.Name + "$");   // keep “$” so existing code paths expecting it still work
            if (cboSheets.Items.Count > 0) cboSheets.SelectedIndex = 0;
            return Task.CompletedTask;
        }

        private Task<DataTable> ReadSheetAsync(string filePath, string sheetNameWithDollar)
        {
            var dt = new DataTable();
            using var wb = new XLWorkbook(filePath);
            var name = sheetNameWithDollar.TrimEnd('$', '\'');  // normalize
            var ws = wb.Worksheets.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                     ?? wb.Worksheet(1);

            bool first = true;
            foreach (var row in ws.RangeUsed().RowsUsed())
            {
                if (first)
                {
                    foreach (var cell in row.Cells())
                        dt.Columns.Add(cell.GetString().Trim());  // trim headers
                    first = false;
                }
                else
                {
                    var values = row.Cells(1, dt.Columns.Count).Select(c => c.Value).ToArray();
                    dt.Rows.Add(values);
                }
            }
            return Task.FromResult(dt);
        }

        private async Task PreviewAsync()
        {
            if (_filePath == null || cboSheets.SelectedItem == null) return;
            var dt = await ReadSheetAsync(_filePath, cboSheets.SelectedItem.ToString()!);
            grid.DataSource = dt;
            lblInfo.Text = $"Rows: {dt.Rows.Count} | Columns: {dt.Columns.Count}. Columns will be matched by name to {cboTables.SelectedItem}.";
        }

        private DuplicateStrategy GetDupStrategy() =>
        (cboDup.SelectedItem?.ToString()) switch
        {
            "Skip" => DuplicateStrategy.Skip,
            "Upsert" => DuplicateStrategy.Upsert,
            _ => DuplicateStrategy.Error
        };

        private async Task DoImportAsync()
        {
            if (_filePath == null || cboSheets.SelectedItem == null || cboTables.SelectedItem == null)
            {
                MessageBox.Show(this, "Pick file, sheet and destination table.", "Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                UseWaitCursor = true;
                btnImport.Enabled = btnPreview.Enabled = btnFile.Enabled = false;
                cboSheets.Enabled = cboTables.Enabled = cboDup.Enabled = false;
                btnCancel.Enabled = true;

                // 1) Read the sheet to a DataTable (your existing OLE DB reader)
                var sheet = await ReadSheetAsync(_filePath, cboSheets.SelectedItem.ToString()!);
                if (sheet.Rows.Count == 0)
                {
                    MessageBox.Show(this, "Selected sheet is empty.", "Nothing to import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Optional hygiene: drop completely empty rows
                // sheet = sheet.Rows.Cast<DataRow>().Where(r => r.ItemArray.Any(v => v != null && v != DBNull.Value && v.ToString()!.Trim() != "")).CopyToDataTable();

                // 2) Configure progress + cancel
                _cts = new CancellationTokenSource();
                prg.Minimum = 0;
                prg.Maximum = sheet.Rows.Count;
                prg.Value = 0;

                var progress = new Progress<int>(rowsDone =>
                {
                    prg.Value = Math.Min(prg.Maximum, rowsDone);
                    lblInfo.Text = $"Inserted {rowsDone}/{prg.Maximum} rows…";
                });

                // 3) Call the new bulk insert (batched, duplicate strategy)
                await Db.BulkInsertAsync(
                    fullTableName: cboTables.SelectedItem.ToString()!,
                    data: sheet,
                    batchSize: 1000,
                    duplicateStrategy: GetDupStrategy(),   // Error / Skip / Upsert
                    progress: progress,
                    ct: _cts.Token);

                lblInfo.Text = $"Done. Inserted {prg.Value} rows.";
                MessageBox.Show(this, "Import completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                lblInfo.Text = "Import canceled.";
                MessageBox.Show(this, "Import canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    ex.Message + "\n\nTip: If this is a duplicate key error, try 'Skip' or 'Upsert', or remove PK/identity column from the sheet.",
                    "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnImport.Enabled = btnPreview.Enabled = btnFile.Enabled = true;
                cboSheets.Enabled = cboTables.Enabled = cboDup.Enabled = true;
                btnCancel.Enabled = false;
                _cts = null;
            }
        }


        //#region Dynamic Import

        //private async void btnImport_Click(object sender, EventArgs e)
        //{
        //    // Step 1: Let user pick the table
        //    DataTable tables = await Db.ListTablesAsync();
        //    if (tables.Rows.Count == 0)
        //    {
        //        MessageBox.Show("No tables found in the database.");
        //        return;
        //    }

        //    Form selectTableForm = new Form()
        //    {
        //        Width = 300,
        //        Height = 120,
        //        Text = "Select Table to Import",
        //        FormBorderStyle = FormBorderStyle.FixedDialog,
        //        StartPosition = FormStartPosition.CenterParent
        //    };

        //    ComboBox cboTables = new ComboBox() { Left = 20, Top = 20, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        //    foreach (DataRow row in tables.Rows)
        //        cboTables.Items.Add(row["FullName"].ToString());
        //    cboTables.SelectedIndex = 0;

        //    Button btnOk = new Button() { Text = "OK", Left = 100, Width = 80, Top = 60, DialogResult = DialogResult.OK };
        //    selectTableForm.Controls.Add(cboTables);
        //    selectTableForm.Controls.Add(btnOk);
        //    selectTableForm.AcceptButton = btnOk;

        //    if (selectTableForm.ShowDialog() != DialogResult.OK)
        //        return;

        //    string tableName = cboTables.SelectedItem.ToString();

        //    // Step 2: Let user pick Excel file
        //    using (OpenFileDialog ofd = new OpenFileDialog())
        //    {
        //        ofd.Filter = "Excel Files|*.xlsx";
        //        if (ofd.ShowDialog() != DialogResult.OK)
        //            return;

        //        try
        //        {
        //            // Step 3: Read Excel into DataTable
        //            DataTable dt;
        //            using (var workbook = new ClosedXML.Excel.XLWorkbook(ofd.FileName))
        //            {
        //                var ws = workbook.Worksheets.First();
        //                dt = new DataTable();
        //                bool firstRow = true;
        //                foreach (var row in ws.RowsUsed())
        //                {
        //                    if (firstRow)
        //                    {
        //                        foreach (var cell in row.Cells())
        //                            dt.Columns.Add(cell.GetString());
        //                        firstRow = false;
        //                    }
        //                    else
        //                    {
        //                        dt.Rows.Add(row.Cells().Select(c => c.Value).ToArray());
        //                    }
        //                }
        //            }

        //            // Step 4: Bulk insert into selected table
        //            await Db.BulkInsertAsync(tableName, dt);

        //            MessageBox.Show("Import successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Import failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}


        //#endregion
    }
}
