using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DesinstalaPhotoshop.UI
{
    public partial class RestoreBackupForm : Form
    {
        /// <summary>
        /// La ruta del backup seleccionado
        /// </summary>
        private string _selectedBackupPath = string.Empty;

        /// <summary>
        /// Obtiene la ruta del backup seleccionado
        /// </summary>
        public string SelectedBackupPath => _selectedBackupPath;

        public RestoreBackupForm()
        {
            InitializeComponent();
            SetupForm();
            LoadBackups();
        }

        private void SetupForm()
        {
            // Configurar propiedades del formulario
            this.Text = "Restaurar Copia de Seguridad";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(20, 30, 45);
            this.ForeColor = Color.White;
            this.Size = new Size(500, 400);
            this.AcceptButton = btnRestore;
            this.CancelButton = btnCancel;
        }

        private void LoadBackups()
        {
            try
            {
                lstBackups.Items.Clear();

                // Obtener la carpeta de backups
                string backupFolder = Path.Combine(Application.StartupPath, "backups");

                if (!Directory.Exists(backupFolder))
                {
                    lblNoBackups.Visible = true;
                    lstBackups.Visible = false;
                    btnRestore.Enabled = false;
                    return;
                }

                // Buscar archivos de backup
                string[] backupFiles = Directory.GetFiles(backupFolder, "*.zip");

                if (backupFiles.Length == 0)
                {
                    lblNoBackups.Visible = true;
                    lstBackups.Visible = false;
                    btnRestore.Enabled = false;
                    return;
                }

                // Mostrar los backups encontrados
                lblNoBackups.Visible = false;
                lstBackups.Visible = true;

                foreach (string backupFile in backupFiles)
                {
                    FileInfo fileInfo = new FileInfo(backupFile);
                    string fileName = fileInfo.Name;
                    string fileDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string fileSize = (fileInfo.Length / 1024 / 1024).ToString() + " MB";

                    ListViewItem item = new ListViewItem(fileName);
                    item.SubItems.Add(fileDate);
                    item.SubItems.Add(fileSize);
                    item.Tag = backupFile;

                    lstBackups.Items.Add(item);
                }

                // Seleccionar el primer backup por defecto
                if (lstBackups.Items.Count > 0)
                {
                    lstBackups.Items[0].Selected = true;
                    btnRestore.Enabled = true;
                }
                else
                {
                    btnRestore.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los backups: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LstBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count > 0)
            {
                _selectedBackupPath = lstBackups.SelectedItems[0].Tag?.ToString() ?? string.Empty;
                btnRestore.Enabled = true;
            }
            else
            {
                _selectedBackupPath = string.Empty;
                btnRestore.Enabled = false;
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedBackupPath))
            {
                MessageBox.Show("Por favor, seleccione un backup para restaurar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
