namespace DesinstalaPhotoshop.UI
{
    partial class TestModeOptionsForm
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
            rbDetectOnly = new RadioButton();
            rbSimulateUninstall = new RadioButton();
            rbSimulateCleanup = new RadioButton();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(55, 19);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(270, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Seleccione la operación a simular:";
            // 
            // rbDetectOnly
            // 
            rbDetectOnly.AutoSize = true;
            rbDetectOnly.ForeColor = Color.White;
            rbDetectOnly.Location = new Point(40, 60);
            rbDetectOnly.Name = "rbDetectOnly";
            rbDetectOnly.Size = new Size(310, 19);
            rbDetectOnly.TabIndex = 1;
            rbDetectOnly.TabStop = true;
            rbDetectOnly.Text = "Solo detectar instalaciones (sin cambios en el sistema)";
            rbDetectOnly.UseVisualStyleBackColor = true;
            rbDetectOnly.CheckedChanged += RbDetectOnly_CheckedChanged;
            // 
            // rbSimulateUninstall
            // 
            rbSimulateUninstall.AutoSize = true;
            rbSimulateUninstall.ForeColor = Color.White;
            rbSimulateUninstall.Location = new Point(40, 90);
            rbSimulateUninstall.Name = "rbSimulateUninstall";
            rbSimulateUninstall.Size = new Size(300, 19);
            rbSimulateUninstall.TabIndex = 2;
            rbSimulateUninstall.TabStop = true;
            rbSimulateUninstall.Text = "Simular desinstalación (mostrar qué se desinstalaría)";
            rbSimulateUninstall.UseVisualStyleBackColor = true;
            rbSimulateUninstall.CheckedChanged += RbSimulateUninstall_CheckedChanged;
            // 
            // rbSimulateCleanup
            // 
            rbSimulateCleanup.AutoSize = true;
            rbSimulateCleanup.ForeColor = Color.White;
            rbSimulateCleanup.Location = new Point(40, 120);
            rbSimulateCleanup.Name = "rbSimulateCleanup";
            rbSimulateCleanup.Size = new Size(250, 19);
            rbSimulateCleanup.TabIndex = 3;
            rbSimulateCleanup.TabStop = true;
            rbSimulateCleanup.Text = "Simular limpieza (mostrar qué se limpiaría)";
            rbSimulateCleanup.UseVisualStyleBackColor = true;
            rbSimulateCleanup.CheckedChanged += RbSimulateCleanup_CheckedChanged;
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(30, 40, 60);
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(100, 170);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 4;
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
            btnCancel.Location = new Point(210, 170);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // TestModeOptionsForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 30, 45);
            CancelButton = btnCancel;
            ClientSize = new Size(400, 220);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(rbSimulateCleanup);
            Controls.Add(rbSimulateUninstall);
            Controls.Add(rbDetectOnly);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TestModeOptionsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Opciones de Modo de Prueba";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RadioButton rbDetectOnly;
        private System.Windows.Forms.RadioButton rbSimulateUninstall;
        private System.Windows.Forms.RadioButton rbSimulateCleanup;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
