using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesinstalaPhotoshop.UI
{
    /// <summary>
    /// Enumeración que define los tipos de operaciones en modo de prueba
    /// </summary>
    public enum TestModeOperation
    {
        DetectOnly,
        SimulateUninstall,
        SimulateCleanup
    }

    public partial class TestModeOptionsForm : Form
    {
        /// <summary>
        /// La operación seleccionada por el usuario
        /// </summary>
        private TestModeOperation _selectedOperation = TestModeOperation.DetectOnly;

        /// <summary>
        /// Obtiene la operación seleccionada por el usuario
        /// </summary>
        public TestModeOperation SelectedOperation => _selectedOperation;

        public TestModeOptionsForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Configurar propiedades del formulario
            this.Text = "Opciones de Modo de Prueba";
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

            // Seleccionar la primera opción por defecto
            rbDetectOnly.Checked = true;
        }

        private void RbDetectOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDetectOnly.Checked)
            {
                _selectedOperation = TestModeOperation.DetectOnly;
            }
        }

        private void RbSimulateUninstall_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSimulateUninstall.Checked)
            {
                _selectedOperation = TestModeOperation.SimulateUninstall;
            }
        }

        private void RbSimulateCleanup_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSimulateCleanup.Checked)
            {
                _selectedOperation = TestModeOperation.SimulateCleanup;
            }
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
