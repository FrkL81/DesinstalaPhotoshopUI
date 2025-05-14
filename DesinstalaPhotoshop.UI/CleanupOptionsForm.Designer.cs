namespace DesinstalaPhotoshop.UI
{
    partial class CleanupOptionsForm
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
            chkWhatIfMode = new CheckBox();
            chkCleanupTempFiles = new CheckBox();
            chkCleanupRegistry = new CheckBox();
            chkCleanupConfigFiles = new CheckBox();
            chkCleanupCacheFiles = new CheckBox();
            btnSelectAll = new Button();
            btnDeselectAll = new Button();
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
            lblTitle.Size = new Size(178, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Opciones de limpieza:";
            //
            // chkCreateBackup
            //
            chkCreateBackup.AutoSize = true;
            chkCreateBackup.Checked = true;
            chkCreateBackup.CheckState = CheckState.Checked;
            chkCreateBackup.ForeColor = Color.White;
            chkCreateBackup.Location = new Point(31, 110);
            chkCreateBackup.Name = "chkCreateBackup";
            chkCreateBackup.Size = new Size(329, 19);
            chkCreateBackup.TabIndex = 1;
            chkCreateBackup.Text = "Crear copia de seguridad antes de limpiar (recomendado)";
            chkCreateBackup.UseVisualStyleBackColor = true;
            chkCreateBackup.CheckedChanged += ChkCreateBackup_CheckedChanged;
            //
            // chkWhatIfMode
            //
            chkWhatIfMode.AutoSize = true;
            chkWhatIfMode.ForeColor = Color.White;
            chkWhatIfMode.Location = new Point(31, 270);
            chkWhatIfMode.Name = "chkWhatIfMode";
            chkWhatIfMode.Size = new Size(150, 19);
            chkWhatIfMode.TabIndex = 10;
            chkWhatIfMode.Text = "Modo simulación (no realizar cambios reales)";
            chkWhatIfMode.UseVisualStyleBackColor = true;
            chkWhatIfMode.CheckedChanged += ChkWhatIfMode_CheckedChanged;
            //
            // chkCleanupTempFiles
            //
            chkCleanupTempFiles.AutoSize = true;
            chkCleanupTempFiles.Checked = true;
            chkCleanupTempFiles.CheckState = CheckState.Checked;
            chkCleanupTempFiles.ForeColor = Color.White;
            chkCleanupTempFiles.Location = new Point(31, 142);
            chkCleanupTempFiles.Name = "chkCleanupTempFiles";
            chkCleanupTempFiles.Size = new Size(175, 19);
            chkCleanupTempFiles.TabIndex = 2;
            chkCleanupTempFiles.Text = "Limpiar archivos temporales";
            chkCleanupTempFiles.UseVisualStyleBackColor = true;
            chkCleanupTempFiles.CheckedChanged += ChkCleanupTempFiles_CheckedChanged;
            //
            // chkCleanupRegistry
            //
            chkCleanupRegistry.AutoSize = true;
            chkCleanupRegistry.Checked = true;
            chkCleanupRegistry.CheckState = CheckState.Checked;
            chkCleanupRegistry.ForeColor = Color.White;
            chkCleanupRegistry.Location = new Point(31, 174);
            chkCleanupRegistry.Name = "chkCleanupRegistry";
            chkCleanupRegistry.Size = new Size(176, 19);
            chkCleanupRegistry.TabIndex = 3;
            chkCleanupRegistry.Text = "Limpiar entradas del registro";
            chkCleanupRegistry.UseVisualStyleBackColor = true;
            chkCleanupRegistry.CheckedChanged += ChkCleanupRegistry_CheckedChanged;
            //
            // chkCleanupConfigFiles
            //
            chkCleanupConfigFiles.AutoSize = true;
            chkCleanupConfigFiles.Checked = true;
            chkCleanupConfigFiles.CheckState = CheckState.Checked;
            chkCleanupConfigFiles.ForeColor = Color.White;
            chkCleanupConfigFiles.Location = new Point(31, 206);
            chkCleanupConfigFiles.Name = "chkCleanupConfigFiles";
            chkCleanupConfigFiles.Size = new Size(206, 19);
            chkCleanupConfigFiles.TabIndex = 4;
            chkCleanupConfigFiles.Text = "Limpiar archivos de configuración";
            chkCleanupConfigFiles.UseVisualStyleBackColor = true;
            chkCleanupConfigFiles.CheckedChanged += ChkCleanupConfigFiles_CheckedChanged;
            //
            // chkCleanupCacheFiles
            //
            chkCleanupCacheFiles.AutoSize = true;
            chkCleanupCacheFiles.Checked = true;
            chkCleanupCacheFiles.CheckState = CheckState.Checked;
            chkCleanupCacheFiles.ForeColor = Color.White;
            chkCleanupCacheFiles.Location = new Point(31, 238);
            chkCleanupCacheFiles.Name = "chkCleanupCacheFiles";
            chkCleanupCacheFiles.Size = new Size(163, 19);
            chkCleanupCacheFiles.TabIndex = 5;
            chkCleanupCacheFiles.Text = "Limpiar archivos de caché";
            chkCleanupCacheFiles.UseVisualStyleBackColor = true;
            chkCleanupCacheFiles.CheckedChanged += ChkCleanupCacheFiles_CheckedChanged;
            //
            // btnSelectAll
            //
            btnSelectAll.BackColor = Color.FromArgb(30, 40, 60);
            btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnSelectAll.FlatStyle = FlatStyle.Flat;
            btnSelectAll.ForeColor = Color.White;
            btnSelectAll.Location = new Point(31, 61);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(120, 30);
            btnSelectAll.TabIndex = 6;
            btnSelectAll.Text = "Seleccionar Todo";
            btnSelectAll.UseVisualStyleBackColor = false;
            btnSelectAll.Click += BtnSelectAll_Click;
            //
            // btnDeselectAll
            //
            btnDeselectAll.BackColor = Color.FromArgb(30, 40, 60);
            btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnDeselectAll.FlatStyle = FlatStyle.Flat;
            btnDeselectAll.ForeColor = Color.White;
            btnDeselectAll.Location = new Point(166, 61);
            btnDeselectAll.Name = "btnDeselectAll";
            btnDeselectAll.Size = new Size(120, 30);
            btnDeselectAll.TabIndex = 7;
            btnDeselectAll.Text = "Deseleccionar Todo";
            btnDeselectAll.UseVisualStyleBackColor = false;
            btnDeselectAll.Click += BtnDeselectAll_Click;
            //
            // btnOK
            //
            btnOK.BackColor = Color.FromArgb(30, 40, 60);
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(289, 277);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 8;
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
            btnCancel.Location = new Point(188, 277);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            //
            // CleanupOptionsForm
            //
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 30, 45);
            CancelButton = btnCancel;
            ClientSize = new Size(400, 319);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnDeselectAll);
            Controls.Add(btnSelectAll);
            Controls.Add(chkCleanupCacheFiles);
            Controls.Add(chkCleanupConfigFiles);
            Controls.Add(chkCleanupRegistry);
            Controls.Add(chkCleanupTempFiles);
            Controls.Add(chkCreateBackup);
            Controls.Add(chkWhatIfMode);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CleanupOptionsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Opciones de Limpieza";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkCreateBackup;
        private System.Windows.Forms.CheckBox chkWhatIfMode;
        private System.Windows.Forms.CheckBox chkCleanupTempFiles;
        private System.Windows.Forms.CheckBox chkCleanupRegistry;
        private System.Windows.Forms.CheckBox chkCleanupConfigFiles;
        private System.Windows.Forms.CheckBox chkCleanupCacheFiles;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnDeselectAll;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
