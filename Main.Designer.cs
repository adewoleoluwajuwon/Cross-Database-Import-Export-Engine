namespace Eximp
{
    partial class Main
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
            btnTestDb = new Button();
            btnImport = new Button();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            btnFilter = new Button();
            lblStatus = new Label();
            btnExport = new Button();
            btnExportPdf = new Button();
            btnChangeConnection = new Button();
            SuspendLayout();
            // 
            // btnTestDb
            // 
            btnTestDb.Location = new Point(20, 20);
            btnTestDb.Margin = new Padding(3, 2, 3, 2);
            btnTestDb.Name = "btnTestDb";
            btnTestDb.Size = new Size(120, 30);
            btnTestDb.TabIndex = 0;
            btnTestDb.Text = "Test db";
            btnTestDb.UseVisualStyleBackColor = true;
            btnTestDb.Click += btnTestDb_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(160, 20);
            btnImport.Margin = new Padding(3, 2, 3, 2);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(150, 30);
            btnImport.TabIndex = 1;
            btnImport.Text = "Import From Excel";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(330, 20);
            btnFilter.Margin = new Padding(3, 2, 3, 2);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(150, 30);
            btnFilter.TabIndex = 2;
            btnFilter.Text = "Class-wise Filter";
            btnFilter.UseVisualStyleBackColor = true;
            btnFilter.Click += btnFilter_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Ubuntu Mono Medium", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.ForeColor = SystemColors.ButtonHighlight;
            lblStatus.Location = new Point(20, 170);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(470, 78);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Click \"Test db\" to test db";
            // 
            // btnExport
            // 
            btnExport.Location = new Point(160, 56);
            btnExport.Margin = new Padding(3, 2, 3, 2);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(150, 30);
            btnExport.TabIndex = 4;
            btnExport.Text = "Export to Excel";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnExportPdf
            // 
            btnExportPdf.Location = new Point(330, 56);
            btnExportPdf.Margin = new Padding(3, 2, 3, 2);
            btnExportPdf.Name = "btnExportPdf";
            btnExportPdf.Size = new Size(150, 30);
            btnExportPdf.TabIndex = 5;
            btnExportPdf.Text = "Export to PDF";
            btnExportPdf.UseVisualStyleBackColor = true;
            btnExportPdf.Click += btnExportPdf_Click;
            // 
            // btnChangeConnection
            // 
            btnChangeConnection.Location = new Point(314, 116);
            btnChangeConnection.Margin = new Padding(3, 2, 3, 2);
            btnChangeConnection.Name = "btnChangeConnection";
            btnChangeConnection.Size = new Size(166, 30);
            btnChangeConnection.TabIndex = 6;
            btnChangeConnection.Text = "Change db Connection";
            btnChangeConnection.UseVisualStyleBackColor = true;
            btnChangeConnection.Click += btnChangeConnection_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.MenuHighlight;
            ClientSize = new Size(500, 253);
            Controls.Add(btnChangeConnection);
            Controls.Add(btnExportPdf);
            Controls.Add(btnExport);
            Controls.Add(lblStatus);
            Controls.Add(btnFilter);
            Controls.Add(btnImport);
            Controls.Add(btnTestDb);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main";
            Click += btnTestDb_Click;
            ResumeLayout(false);
        }

        #endregion

        private Button btnTestDb;
        private Button btnImport;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Button btnFilter;
        private Label lblStatus;
        private Button btnExport;
        private Button btnExportPdf;
        private Button btnChangeConnection;
    }
}