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
            chkRemoveUserData = new CheckBox();
            chkRemoveSharedComponents = new CheckBox();
            chkWhatIfMode = new CheckBox();
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
            // chkRemoveUserData
            //
            chkRemoveUserData.AutoSize = true;
            chkRemoveUserData.ForeColor = Color.White;
            chkRemoveUserData.Location = new Point(27, 120);
            chkRemoveUserData.Name = "chkRemoveUserData";
            chkRemoveUserData.Size = new Size(300, 19);
            chkRemoveUserData.TabIndex = 3;
            chkRemoveUserData.Text = "Eliminar datos de usuario (preferencias, presets, etc.)";
            chkRemoveUserData.UseVisualStyleBackColor = true;
            chkRemoveUserData.CheckedChanged += ChkRemoveUserData_CheckedChanged;
            //
            // chkRemoveSharedComponents
            //
            chkRemoveSharedComponents.AutoSize = true;
            chkRemoveSharedComponents.ForeColor = Color.White;
            chkRemoveSharedComponents.Location = new Point(27, 150);
            chkRemoveSharedComponents.Name = "chkRemoveSharedComponents";
            chkRemoveSharedComponents.Size = new Size(350, 19);
            chkRemoveSharedComponents.TabIndex = 4;
            chkRemoveSharedComponents.Text = "Eliminar componentes compartidos (puede afectar otras apps)";
            chkRemoveSharedComponents.UseVisualStyleBackColor = true;
            chkRemoveSharedComponents.CheckedChanged += ChkRemoveSharedComponents_CheckedChanged;
            //
            // chkWhatIfMode
            //
            chkWhatIfMode.AutoSize = true;
            chkWhatIfMode.ForeColor = Color.White;
            chkWhatIfMode.Location = new Point(27, 180);
            chkWhatIfMode.Name = "chkWhatIfMode";
            chkWhatIfMode.Size = new Size(350, 19);
            chkWhatIfMode.TabIndex = 5;
            chkWhatIfMode.Text = "Modo de simulación (no realizar cambios reales)";
            chkWhatIfMode.UseVisualStyleBackColor = true;
            chkWhatIfMode.CheckedChanged += ChkWhatIfMode_CheckedChanged;
            //
            // btnOK
            //
            btnOK.BackColor = Color.FromArgb(30, 40, 60);
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(100, 220);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 6;
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
            btnCancel.Location = new Point(210, 220);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 7;
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
            ClientSize = new Size(400, 270);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkWhatIfMode);
            Controls.Add(chkRemoveSharedComponents);
            Controls.Add(chkRemoveUserData);
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
        private System.Windows.Forms.CheckBox chkRemoveUserData;
        private System.Windows.Forms.CheckBox chkRemoveSharedComponents;
        private System.Windows.Forms.CheckBox chkWhatIfMode;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
