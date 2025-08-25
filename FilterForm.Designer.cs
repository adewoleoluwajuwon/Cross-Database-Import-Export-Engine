namespace Eximp
{
    partial class FilterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cboTable = new ComboBox();
            cboClassColumn = new ComboBox();
            cboClassValue = new ComboBox();
            btnLoad = new Button();
            lblInfo = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // cboTable
            // 
            cboTable.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTable.FormattingEnabled = true;
            cboTable.Location = new Point(20, 20);
            cboTable.Name = "cboTable";
            cboTable.Size = new Size(240, 23);
            cboTable.TabIndex = 0;
            // 
            // cboClassColumn
            // 
            cboClassColumn.DropDownStyle = ComboBoxStyle.DropDownList;
            cboClassColumn.FormattingEnabled = true;
            cboClassColumn.Location = new Point(278, 21);
            cboClassColumn.Name = "cboClassColumn";
            cboClassColumn.Size = new Size(240, 23);
            cboClassColumn.TabIndex = 1;
            // 
            // cboClassValue
            // 
            cboClassValue.DropDownStyle = ComboBoxStyle.DropDownList;
            cboClassValue.FormattingEnabled = true;
            cboClassValue.Location = new Point(540, 20);
            cboClassValue.Name = "cboClassValue";
            cboClassValue.Size = new Size(240, 23);
            cboClassValue.TabIndex = 2;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(800, 20);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(160, 23);
            btnLoad.TabIndex = 3;
            btnLoad.Text = "Load Records";
            btnLoad.UseVisualStyleBackColor = true;
            // 
            // lblInfo
            // 
            lblInfo.AutoEllipsis = true;
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(20, 60);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(71, 15);
            lblInfo.TabIndex = 4;
            lblInfo.Text = " info display";
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Location = new Point(20, 100);
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.Size = new Size(950, 520);
            grid.TabIndex = 5;
            // 
            // FilterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 641);
            Controls.Add(grid);
            Controls.Add(lblInfo);
            Controls.Add(btnLoad);
            Controls.Add(cboClassValue);
            Controls.Add(cboClassColumn);
            Controls.Add(cboTable);
            Name = "FilterForm";
            Text = "Class-wise Filter";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cboTable;
        private ComboBox cboClassColumn;
        private ComboBox cboClassValue;
        private Button btnLoad;
        private Label lblInfo;
        private DataGridView grid;
    }
}