using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesinstalaPhotoshop.UI
{
    public partial class CleanupOptionsForm : Form
    {
        /// <summary>
        /// Indica si se debe crear una copia de seguridad antes de limpiar
        /// </summary>
        private bool _createBackup = true;

        /// <summary>
        /// Indica si se deben limpiar archivos temporales
        /// </summary>
        private bool _cleanupTempFiles = true;

        /// <summary>
        /// Indica si se deben limpiar entradas del registro
        /// </summary>
        private bool _cleanupRegistry = true;

        /// <summary>
        /// Indica si se deben limpiar archivos de configuración
        /// </summary>
        private bool _cleanupConfigFiles = true;

        /// <summary>
        /// Indica si se deben limpiar archivos de caché
        /// </summary>
        private bool _cleanupCacheFiles = true;

        /// <summary>
        /// Obtiene si se debe crear una copia de seguridad antes de limpiar
        /// </summary>
        public bool CreateBackup => _createBackup;

        /// <summary>
        /// Obtiene si se deben limpiar archivos temporales
        /// </summary>
        public bool CleanupTempFiles => _cleanupTempFiles;

        /// <summary>
        /// Obtiene si se deben limpiar entradas del registro
        /// </summary>
        public bool CleanupRegistry => _cleanupRegistry;

        /// <summary>
        /// Obtiene si se deben limpiar archivos de configuración
        /// </summary>
        public bool CleanupConfigFiles => _cleanupConfigFiles;

        /// <summary>
        /// Obtiene si se deben limpiar archivos de caché
        /// </summary>
        public bool CleanupCacheFiles => _cleanupCacheFiles;

        public CleanupOptionsForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Configurar propiedades del formulario
            this.Text = "Opciones de Limpieza";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(20, 30, 45);
            this.ForeColor = Color.White;
            this.Size = new Size(400, 350);
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Seleccionar las opciones por defecto
            chkCreateBackup.Checked = true;
            chkCleanupTempFiles.Checked = true;
            chkCleanupRegistry.Checked = true;
            chkCleanupConfigFiles.Checked = true;
            chkCleanupCacheFiles.Checked = true;
        }

        private void ChkCreateBackup_CheckedChanged(object sender, EventArgs e)
        {
            _createBackup = chkCreateBackup.Checked;
        }

        private void ChkCleanupTempFiles_CheckedChanged(object sender, EventArgs e)
        {
            _cleanupTempFiles = chkCleanupTempFiles.Checked;
        }

        private void ChkCleanupRegistry_CheckedChanged(object sender, EventArgs e)
        {
            _cleanupRegistry = chkCleanupRegistry.Checked;
        }

        private void ChkCleanupConfigFiles_CheckedChanged(object sender, EventArgs e)
        {
            _cleanupConfigFiles = chkCleanupConfigFiles.Checked;
        }

        private void ChkCleanupCacheFiles_CheckedChanged(object sender, EventArgs e)
        {
            _cleanupCacheFiles = chkCleanupCacheFiles.Checked;
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            chkCleanupTempFiles.Checked = true;
            chkCleanupRegistry.Checked = true;
            chkCleanupConfigFiles.Checked = true;
            chkCleanupCacheFiles.Checked = true;
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            chkCleanupTempFiles.Checked = false;
            chkCleanupRegistry.Checked = false;
            chkCleanupConfigFiles.Checked = false;
            chkCleanupCacheFiles.Checked = false;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
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
