namespace DesinstalaPhotoshop.UI
{
    partial class RestoreBackupForm
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
            lstBackups = new ListView();
            colFileName = new ColumnHeader();
            colDate = new ColumnHeader();
            colSize = new ColumnHeader();
            lblNoBackups = new Label();
            btnRestore = new Button();
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
            lblTitle.Size = new Size(389, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Seleccione una copia de seguridad para restaurar:";
            // 
            // lstBackups
            // 
            lstBackups.BackColor = Color.FromArgb(20, 30, 45);
            lstBackups.BorderStyle = BorderStyle.FixedSingle;
            lstBackups.Columns.AddRange(new ColumnHeader[] { colFileName, colDate, colSize });
            lstBackups.ForeColor = Color.White;
            lstBackups.FullRowSelect = true;
            lstBackups.Location = new Point(20, 60);
            lstBackups.MultiSelect = false;
            lstBackups.Name = "lstBackups";
            lstBackups.Size = new Size(460, 250);
            lstBackups.TabIndex = 1;
            lstBackups.UseCompatibleStateImageBehavior = false;
            lstBackups.View = View.Details;
            lstBackups.SelectedIndexChanged += LstBackups_SelectedIndexChanged;
            // 
            // colFileName
            // 
            colFileName.Text = "Nombre del archivo";
            colFileName.Width = 200;
            // 
            // colDate
            // 
            colDate.Text = "Fecha";
            colDate.Width = 150;
            // 
            // colSize
            // 
            colSize.Text = "Tamaño";
            colSize.Width = 100;
            // 
            // lblNoBackups
            // 
            lblNoBackups.AutoSize = true;
            lblNoBackups.Font = new Font("Segoe UI", 10F);
            lblNoBackups.ForeColor = Color.White;
            lblNoBackups.Location = new Point(27, 279);
            lblNoBackups.Name = "lblNoBackups";
            lblNoBackups.Size = new Size(323, 19);
            lblNoBackups.TabIndex = 2;
            lblNoBackups.Text = "No se encontraron copias de seguridad disponibles.";
            lblNoBackups.Visible = false;
            // 
            // btnRestore
            // 
            btnRestore.BackColor = Color.FromArgb(30, 40, 60);
            btnRestore.Enabled = false;
            btnRestore.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnRestore.FlatStyle = FlatStyle.Flat;
            btnRestore.ForeColor = Color.White;
            btnRestore.Location = new Point(150, 330);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(90, 30);
            btnRestore.TabIndex = 3;
            btnRestore.Text = "Restaurar";
            btnRestore.UseVisualStyleBackColor = false;
            btnRestore.Click += BtnRestore_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(30, 40, 60);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(260, 330);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // RestoreBackupForm
            // 
            AcceptButton = btnRestore;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 30, 45);
            CancelButton = btnCancel;
            ClientSize = new Size(500, 380);
            Controls.Add(btnCancel);
            Controls.Add(btnRestore);
            Controls.Add(lblNoBackups);
            Controls.Add(lstBackups);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(516, 419);
            Name = "RestoreBackupForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Restaurar Copia de Seguridad";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListView lstBackups;
        private System.Windows.Forms.ColumnHeader colFileName;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.Label lblNoBackups;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnCancel;
    }
}
