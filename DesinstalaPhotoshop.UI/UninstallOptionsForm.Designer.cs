namespace DesinstalaPhotoshop.UI
{
    partial class UninstallOptionsForm
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
            lblTitle = new Label();
            chkCreateBackup = new CheckBox();
            chkCleanupAfterUninstall = new CheckBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(222, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Opciones de desinstalación:";
            // 
            // chkCreateBackup
            // 
            chkCreateBackup.AutoSize = true;
            chkCreateBackup.Checked = true;
            chkCreateBackup.CheckState = CheckState.Checked;
            chkCreateBackup.ForeColor = Color.White;
            chkCreateBackup.Location = new Point(27, 60);
            chkCreateBackup.Name = "chkCreateBackup";
            chkCreateBackup.Size = new Size(348, 19);
            chkCreateBackup.TabIndex = 1;
            chkCreateBackup.Text = "Crear copia de seguridad antes de desinstalar (recomendado)";
            chkCreateBackup.UseVisualStyleBackColor = true;
            chkCreateBackup.CheckedChanged += ChkCreateBackup_CheckedChanged;
            // 
            // chkCleanupAfterUninstall
            // 
            chkCleanupAfterUninstall.AutoSize = true;
            chkCleanupAfterUninstall.Checked = true;
            chkCleanupAfterUninstall.CheckState = CheckState.Checked;
            chkCleanupAfterUninstall.ForeColor = Color.White;
            chkCleanupAfterUninstall.Location = new Point(27, 90);
            chkCleanupAfterUninstall.Name = "chkCleanupAfterUninstall";
            chkCleanupAfterUninstall.Size = new Size(350, 19);
            chkCleanupAfterUninstall.TabIndex = 2;
            chkCleanupAfterUninstall.Text = "Limpiar residuos después de la desinstalación (recomendado)";
            chkCleanupAfterUninstall.UseVisualStyleBackColor = true;
            chkCleanupAfterUninstall.CheckedChanged += ChkCleanupAfterUninstall_CheckedChanged;
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(30, 40, 60);
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(100, 148);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 3;
            btnOK.Text = "Aceptar";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(30, 40, 60);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(210, 148);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // UninstallOptionsForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 30, 45);
            CancelButton = btnCancel;
            ClientSize = new Size(400, 200);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkCleanupAfterUninstall);
            Controls.Add(chkCreateBackup);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UninstallOptionsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Opciones de Desinstalación";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkCreateBackup;
        private System.Windows.Forms.CheckBox chkCleanupAfterUninstall;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
