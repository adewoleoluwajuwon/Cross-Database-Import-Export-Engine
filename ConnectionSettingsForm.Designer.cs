namespace Eximp
{
    partial class ConnectionSettingsForm
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
            lblServer = new Label();
            txtServer = new TextBox();
            txtDatabase = new TextBox();
            lblDatabase = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            txtPassword = new TextBox();
            lblPassword = new Label();
            btnTestConnection = new Button();
            btnSave = new Button();
            cboProvider = new ComboBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // lblServer
            // 
            lblServer.AutoSize = true;
            lblServer.Location = new Point(20, 33);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(106, 15);
            lblServer.TabIndex = 0;
            lblServer.Text = "Server Name or IP:";
            // 
            // txtServer
            // 
            txtServer.Location = new Point(150, 30);
            txtServer.Name = "txtServer";
            txtServer.PlaceholderText = "localhost or 192.168.1.10\"";
            txtServer.Size = new Size(220, 23);
            txtServer.TabIndex = 1;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(150, 81);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.PlaceholderText = "Exact db name in Server";
            txtDatabase.Size = new Size(220, 23);
            txtDatabase.TabIndex = 3;
            // 
            // lblDatabase
            // 
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(20, 84);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(94, 15);
            lblDatabase.TabIndex = 2;
            lblDatabase.Text = "Database Name:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(150, 126);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "eg: sa";
            txtUsername.Size = new Size(220, 23);
            txtUsername.TabIndex = 5;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(20, 129);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(63, 15);
            lblUsername.TabIndex = 4;
            lblUsername.Text = "Username:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(150, 174);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "eg: sa";
            txtPassword.Size = new Size(220, 23);
            txtPassword.TabIndex = 7;
            txtPassword.Text = "Enter Password. eg: sa@123";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(20, 177);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "Password:";
            lblPassword.Click += lblPassword_Click;
            // 
            // btnTestConnection
            // 
            btnTestConnection.ForeColor = SystemColors.ActiveCaptionText;
            btnTestConnection.Location = new Point(20, 266);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(128, 23);
            btnTestConnection.TabIndex = 8;
            btnTestConnection.Text = "Test Connection";
            btnTestConnection.UseVisualStyleBackColor = true;
            btnTestConnection.Click += btnTestConnection_Click;
            // 
            // btnSave
            // 
            btnSave.ForeColor = SystemColors.ActiveCaptionText;
            btnSave.Location = new Point(171, 266);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 9;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // cboProvider
            // 
            cboProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProvider.FormattingEnabled = true;
            cboProvider.Location = new Point(150, 214);
            cboProvider.Name = "cboProvider";
            cboProvider.Size = new Size(220, 23);
            cboProvider.TabIndex = 10;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 217);
            label1.Name = "label1";
            label1.Size = new Size(54, 15);
            label1.TabIndex = 11;
            label1.Text = "Provider:";
            // 
            // ConnectionSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.MenuHighlight;
            ClientSize = new Size(404, 311);
            Controls.Add(label1);
            Controls.Add(cboProvider);
            Controls.Add(btnSave);
            Controls.Add(btnTestConnection);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblUsername);
            Controls.Add(txtDatabase);
            Controls.Add(lblDatabase);
            Controls.Add(txtServer);
            Controls.Add(lblServer);
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ForeColor = SystemColors.ControlLightLight;
            Name = "ConnectionSettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Database Connection Setup";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblServer;
        private TextBox txtServer;
        private TextBox txtDatabase;
        private Label lblDatabase;
        private TextBox txtUsername;
        private Label lblUsername;
        private TextBox txtPassword;
        private Label lblPassword;
        private Button btnTestConnection;
        private Button btnSave;
        private ComboBox cboProvider;
        private Label label1;
    }
}