using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesinstalaPhotoshop.UI
{
    public partial class UninstallOptionsForm : Form
    {
        /// <summary>
        /// Indica si se debe crear una copia de seguridad antes de desinstalar
        /// </summary>
        private bool _createBackup = true;

        /// <summary>
        /// Indica si se deben limpiar residuos después de la desinstalación
        /// </summary>
        private bool _cleanupAfterUninstall = true;

        /// <summary>
        /// Obtiene si se debe crear una copia de seguridad antes de desinstalar
        /// </summary>
        public bool CreateBackup => _createBackup;

        /// <summary>
        /// Obtiene si se deben limpiar residuos después de la desinstalación
        /// </summary>
        public bool CleanupAfterUninstall => _cleanupAfterUninstall;

        public UninstallOptionsForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Configurar propiedades del formulario
            this.Text = "Opciones de Desinstalación";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(20, 30, 45);
            this.ForeColor = Color.White;
            this.Size = new Size(400, 250);
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Seleccionar las opciones por defecto
            chkCreateBackup.Checked = true;
            chkCleanupAfterUninstall.Checked = true;
        }

        private void ChkCreateBackup_CheckedChanged(object sender, EventArgs e)
        {
            _createBackup = chkCreateBackup.Checked;
        }

        private void ChkCleanupAfterUninstall_CheckedChanged(object sender, EventArgs e)
        {
            _cleanupAfterUninstall = chkCleanupAfterUninstall.Checked;
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
