using System;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using System.Diagnostics;
// AutoHotkey.Interop removido por incompatibilidad con .NET 9.0

namespace DesinstalaPhotoshop.UI
{
    public partial class AboutForm : Form
    {
        // Ya no usamos AutoHotkey para abrir URLs

        // URLs temporales (staging) que serán reemplazadas posteriormente
        private const string REPO_URL = "https://github.com/FrkL81/DesinstalaPhotoshopUI";
        private const string MANUAL_URL = "https://ejemplo.com/manual-usuario";
        private const string DONATION_URL = "https://paypal.me/ejemplo";

        public AboutForm()
        {
            InitializeComponent();
            
            // Ya no necesitamos inicializar AutoHotkey
            
            // Configurar apariencia del formulario
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 30, 45); // Mismo color de fondo que MainForm
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Acerca de DesinstalaPhotoshop";
            this.Size = new Size(600, 500);
            
            // Crear controles
            CreateControls();
        }

        private void CreateControls()
        {
            // Panel principal
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 30, 45),
                Padding = new Padding(20)
            };

            // Título
            Label lblTitle = new Label
            {
                Text = "DesinstalaPhotoshop",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Versión
            Label lblVersion = new Label
            {
                Text = "Versión 1.0.0",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(20, lblTitle.Bottom + 5)
            };

            // Descripción
            Label lblDescription = new Label
            {
                Text = "Herramienta para desinstalar completamente Adobe Photoshop y limpiar sus residuos del sistema.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(540, 40),
                Location = new Point(20, lblVersion.Bottom + 15)
            };

            // Autores
            Label lblAuthors = new Label
            {
                Text = "Desarrollado por: Equipo DesinstalaPhotoshop",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, lblDescription.Bottom + 15)
            };

            // Licencia - Título
            Label lblLicenseTitle = new Label
            {
                Text = "Licencia:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, lblAuthors.Bottom + 20)
            };

            // Licencia - Texto
            TextBox txtLicense = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(30, 40, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(540, 150),
                Location = new Point(20, lblLicenseTitle.Bottom + 5),
                Text = "Licencia MIT\r\n\r\nCopyright (c) 2025 DesinstalaPhotoshop\r\n\r\n" +
                "Por la presente se concede permiso, sin costo alguno, a cualquier persona que obtenga una copia " +
                "de este software y los archivos de documentación asociados (el \"Software\"), para tratar " +
                "con el Software sin restricción, incluyendo, sin limitación, los derechos " +
                "de uso, copia, modificación, fusión, publicación, distribución, sublicencia y/o venta " +
                "de copias del Software, y para permitir a las personas a quienes se les proporcione el Software " +
                "hacer lo mismo, sujeto a las siguientes condiciones:\r\n\r\n" +
                "El aviso de copyright anterior y este aviso de permiso deberán incluirse en todas " +
                "las copias o porciones sustanciales del Software.\r\n\r\n" +
                "EL SOFTWARE SE PROPORCIONA \"TAL CUAL\", SIN GARANTÍA DE NINGÚN TIPO, EXPRESA O " +
                "IMPLÍCITA, INCLUYENDO PERO NO LIMITADO A LAS GARANTÍAS DE COMERCIABILIDAD, " +
                "IDONEIDAD PARA UN PROPÓSITO PARTICULAR Y NO INFRACCIÓN. EN NINGÚN CASO LOS " +
                "AUTORES O TITULARES DEL COPYRIGHT SERÁN RESPONSABLES DE NINGUNA RECLAMACIÓN, DAÑOS U OTRA " +
                "RESPONSABILIDAD, YA SEA EN UNA ACCIÓN CONTRACTUAL, EXTRACONTRACTUAL O DE OTRO TIPO, DERIVADA DE, " +
                "O RELACIONADA CON EL SOFTWARE O EL USO U OTRAS INTERACCIONES CON EL " +
                "SOFTWARE."
            };


            // Panel para botones de enlaces
            FlowLayoutPanel linksPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Location = new Point(20, txtLicense.Bottom + 20)
            };

            // Botón Repositorio
            IconButton btnRepo = CreateLinkButton("Repositorio Oficial", IconChar.Github, REPO_URL);
            linksPanel.Controls.Add(btnRepo);

            // Botón Manual
            IconButton btnManual = CreateLinkButton("Manual del Usuario", IconChar.Book, MANUAL_URL);
            linksPanel.Controls.Add(btnManual);

            // Botón Donación
            IconButton btnDonation = CreateLinkButton("Invítame Un Café", IconChar.Coffee, DONATION_URL);
            linksPanel.Controls.Add(btnDonation);

            // Botón Cerrar
            Button btnClose = new Button
            {
                Text = "Cerrar",
                BackColor = Color.FromArgb(40, 50, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 35),
                Location = new Point(460, linksPanel.Bottom + 20),
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderColor = Color.Gray;

            // Añadir controles al panel principal
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblVersion);
            mainPanel.Controls.Add(lblDescription);
            mainPanel.Controls.Add(lblAuthors);
            mainPanel.Controls.Add(lblLicenseTitle);
            mainPanel.Controls.Add(txtLicense);
            mainPanel.Controls.Add(linksPanel);
            mainPanel.Controls.Add(btnClose);

            // Añadir panel principal al formulario
            this.Controls.Add(mainPanel);
        }

        private IconButton CreateLinkButton(string text, IconChar icon, string url)
        {
            IconButton button = new IconButton
            {
                Text = text,
                IconChar = icon,
                IconColor = Color.White,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 50, 65),
                FlatStyle = FlatStyle.Flat,
                IconSize = 24,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleRight,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Size = new Size(170, 40),
                Margin = new Padding(5),
                Tag = url
            };
            button.FlatAppearance.BorderColor = Color.Gray;
            button.Click += LinkButton_Click;
            return button;
        }

        private void LinkButton_Click(object sender, EventArgs e)
        {
            if (sender is IconButton button && button.Tag is string url)
            {
                try
                {
                    // Abrir URL usando Process.Start (compatible con .NET 9.0)
                    var psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir el enlace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Método requerido por el diseñador de Windows Forms
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AboutForm
            // 
            this.ClientSize = new System.Drawing.Size(582, 453);
            this.Name = "AboutForm";
            this.ResumeLayout(false);
        }
    }
}