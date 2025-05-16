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
        /// Indica si se deben eliminar los datos de usuario
        /// </summary>
        private bool _removeUserData = false;

        /// <summary>
        /// Indica si se deben eliminar componentes compartidos
        /// </summary>
        private bool _removeSharedComponents = false;

        /// <summary>
        /// Indica si se debe ejecutar en modo de simulación (sin realizar cambios reales)
        /// </summary>
        private bool _whatIfMode = false;

        /// <summary>
        /// Obtiene si se debe crear una copia de seguridad antes de desinstalar
        /// </summary>
        public bool CreateBackup => _createBackup;

        /// <summary>
        /// Obtiene si se deben limpiar residuos después de la desinstalación
        /// </summary>
        public bool CleanupAfterUninstall => _cleanupAfterUninstall;

        /// <summary>
        /// Obtiene si se deben eliminar los datos de usuario
        /// </summary>
        public bool RemoveUserData => _removeUserData;

        /// <summary>
        /// Obtiene si se deben eliminar componentes compartidos
        /// </summary>
        public bool RemoveSharedComponents => _removeSharedComponents;

        /// <summary>
        /// Obtiene si se debe ejecutar en modo de simulación (sin realizar cambios reales)
        /// </summary>
        public bool WhatIfMode => _whatIfMode;

        public UninstallOptionsForm(bool isSimulationContext = false)
        {
            InitializeComponent();
            SetupForm();
            
            if (isSimulationContext)
            {
                chkWhatIfMode.Checked = true;
                chkWhatIfMode.Enabled = false; // Bloquear el checkbox
                _whatIfMode = true;
            }
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
            this.Size = new Size(400, 350);
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Seleccionar las opciones por defecto
            chkCreateBackup.Checked = true;
            chkCleanupAfterUninstall.Checked = true;
            chkRemoveUserData.Checked = false;
            chkRemoveSharedComponents.Checked = false;
            chkWhatIfMode.Checked = false;
        }

        private void ChkCreateBackup_CheckedChanged(object sender, EventArgs e)
        {
            _createBackup = chkCreateBackup.Checked;
        }

        private void ChkCleanupAfterUninstall_CheckedChanged(object sender, EventArgs e)
        {
            _cleanupAfterUninstall = chkCleanupAfterUninstall.Checked;
        }

        private void ChkRemoveUserData_CheckedChanged(object sender, EventArgs e)
        {
            _removeUserData = chkRemoveUserData.Checked;
        }

        private void ChkRemoveSharedComponents_CheckedChanged(object sender, EventArgs e)
        {
            _removeSharedComponents = chkRemoveSharedComponents.Checked;
        }

        private void ChkWhatIfMode_CheckedChanged(object sender, EventArgs e)
        {
            _whatIfMode = chkWhatIfMode.Checked;
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
