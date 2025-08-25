using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eximp
{
    public partial class FilterForm : Form
    {
        public FilterForm()
        {
            InitializeComponent();

            // Wire up event handlers
            this.Load += async (_, __) => await InitAsync();
            cboTable.SelectedIndexChanged += async (_, __) => await ReloadColumnsAsync();
            cboClassColumn.SelectedIndexChanged += async (_, __) => await ReloadClassValuesAsync();
            btnLoad.Click += async (_, __) => await LoadDataAsync();
        }

        private async Task InitAsync()
        {
            var tables = await Db.ListTablesAsync();
            cboTable.Items.Clear();
            foreach (DataRow r in tables.Rows)
                cboTable.Items.Add(Convert.ToString(r["FullName"]));
            if (cboTable.Items.Count > 0)
                cboTable.SelectedIndex = 0;
        }

        private async Task ReloadColumnsAsync()
        {
            cboClassColumn.Items.Clear();
            cboClassValue.Items.Clear();
            if (cboTable.SelectedItem == null) return;

            var cols = await Db.ListColumnsAsync(cboTable.SelectedItem.ToString()!);
            foreach (DataRow r in cols.Rows)
            {
                var name = Convert.ToString(r["COLUMN_NAME"])!;
                cboClassColumn.Items.Add(name);

                if (name.Equals("Class", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("ClassName", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("ClassId", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Class_Code", StringComparison.OrdinalIgnoreCase))
                {
                    cboClassColumn.SelectedItem = name;
                }
            }

            if (cboClassColumn.SelectedItem == null && cboClassColumn.Items.Count > 0)
                cboClassColumn.SelectedIndex = 0;
        }

        private async Task ReloadClassValuesAsync()
        {
            cboClassValue.Items.Clear();
            if (cboTable.SelectedItem == null || cboClassColumn.SelectedItem == null) return;

            var vals = await Db.DistinctValuesAsync(
                cboTable.SelectedItem.ToString()!,
                cboClassColumn.SelectedItem.ToString()!
            );

            foreach (DataRow r in vals.Rows)
                cboClassValue.Items.Add(Convert.ToString(r["Val"]));

            if (cboClassValue.Items.Count > 0)
                cboClassValue.SelectedIndex = 0;
        }

        private async Task LoadDataAsync()
        {
            if (cboTable.SelectedItem == null ||
                cboClassColumn.SelectedItem == null ||
                cboClassValue.SelectedItem == null)
                return;

            var dt = await Db.FilterAsync(
                cboTable.SelectedItem.ToString()!,
                cboClassColumn.SelectedItem.ToString()!,
                cboClassValue.SelectedItem
            );

            grid.DataSource = dt;
            lblInfo.Text = $"{dt.Rows.Count} record(s) found in {cboTable.SelectedItem} for {cboClassColumn.SelectedItem} = '{cboClassValue.SelectedItem}'.";
        }
    }
}
