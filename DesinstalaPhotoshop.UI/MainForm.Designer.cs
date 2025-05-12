using System.Windows.Forms;
using System.Drawing;
using FontAwesome.Sharp;

namespace DesinstalaPhotoshop.UI
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            panelTop = new Panel();
            btnRestore = new IconButton();
            btnCancel = new IconButton();
            btnTestMode = new IconButton();
            btnCleanup = new IconButton();
            btnUninstall = new IconButton();
            btnDetect = new IconButton();
            panelMain = new Panel();
            splitContainer = new SplitContainer();
            lstInstallations = new ListView();
            colName = new ColumnHeader();
            colVersion = new ColumnHeader();
            colLocation = new ColumnHeader();
            colConfidence = new ColumnHeader();
            panelConsole = new Panel();
            txtConsole = new RichTextBox();
            panelConsoleButtons = new Panel();
            panelLeftB = new Panel();
            lblAnimatedText = new Label();
            panelCentralB = new Panel();
            lblProgress = new Label();
            panelRightB = new Panel();
            progressBar = new ProgressBar();
            btnGenerarScript = new IconButton();
            btnAbrirLog = new IconButton();
            btnCopyOutput = new IconButton();
            lblInfoText = new Label();
            animationTimer = new System.Windows.Forms.Timer(components);
            panelTop.SuspendLayout();
            panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelConsole.SuspendLayout();
            panelConsoleButtons.SuspendLayout();
            panelLeftB.SuspendLayout();
            panelCentralB.SuspendLayout();
            panelRightB.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(30, 40, 60);
            panelTop.Controls.Add(btnRestore);
            panelTop.Controls.Add(btnCancel);
            panelTop.Controls.Add(btnTestMode);
            panelTop.Controls.Add(btnCleanup);
            panelTop.Controls.Add(btnUninstall);
            panelTop.Controls.Add(btnDetect);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1000, 60);
            panelTop.TabIndex = 0;
            // 
            // btnRestore
            // 
            btnRestore.BackColor = Color.FromArgb(30, 40, 60);
            btnRestore.Enabled = false;
            btnRestore.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnRestore.FlatAppearance.BorderSize = 0;
            btnRestore.FlatStyle = FlatStyle.Flat;
            btnRestore.ForeColor = Color.White;
            btnRestore.IconChar = IconChar.RotateBack;
            btnRestore.IconColor = Color.White;
            btnRestore.IconFont = IconFont.Auto;
            btnRestore.IconSize = 24;
            btnRestore.Location = new Point(642, 12);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(120, 36);
            btnRestore.TabIndex = 5;
            btnRestore.Text = "  Restaurar";
            btnRestore.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnRestore.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(30, 40, 60);
            btnCancel.Enabled = false;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.White;
            btnCancel.IconChar = IconChar.XmarkCircle;
            btnCancel.IconColor = Color.White;
            btnCancel.IconFont = IconFont.Auto;
            btnCancel.IconSize = 24;
            btnCancel.Location = new Point(516, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 36);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "  Cancelar";
            btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnTestMode
            // 
            btnTestMode.BackColor = Color.FromArgb(30, 40, 60);
            btnTestMode.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnTestMode.FlatAppearance.BorderSize = 0;
            btnTestMode.FlatStyle = FlatStyle.Flat;
            btnTestMode.ForeColor = Color.White;
            btnTestMode.IconChar = IconChar.VialCircleCheck;
            btnTestMode.IconColor = Color.White;
            btnTestMode.IconFont = IconFont.Auto;
            btnTestMode.IconSize = 24;
            btnTestMode.Location = new Point(390, 12);
            btnTestMode.Name = "btnTestMode";
            btnTestMode.Size = new Size(120, 36);
            btnTestMode.TabIndex = 3;
            btnTestMode.Text = "  Modo Prueba";
            btnTestMode.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnTestMode.UseVisualStyleBackColor = false;
            // 
            // btnCleanup
            // 
            btnCleanup.BackColor = Color.FromArgb(30, 40, 60);
            btnCleanup.Enabled = false;
            btnCleanup.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnCleanup.FlatAppearance.BorderSize = 0;
            btnCleanup.FlatStyle = FlatStyle.Flat;
            btnCleanup.ForeColor = Color.White;
            btnCleanup.IconChar = IconChar.Broom;
            btnCleanup.IconColor = Color.White;
            btnCleanup.IconFont = IconFont.Auto;
            btnCleanup.IconSize = 24;
            btnCleanup.Location = new Point(264, 12);
            btnCleanup.Name = "btnCleanup";
            btnCleanup.Size = new Size(120, 36);
            btnCleanup.TabIndex = 2;
            btnCleanup.Text = "  Limpiar";
            btnCleanup.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCleanup.UseVisualStyleBackColor = false;
            // 
            // btnUninstall
            // 
            btnUninstall.BackColor = Color.FromArgb(30, 40, 60);
            btnUninstall.Enabled = false;
            btnUninstall.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.FlatStyle = FlatStyle.Flat;
            btnUninstall.ForeColor = Color.White;
            btnUninstall.IconChar = IconChar.TrashAlt;
            btnUninstall.IconColor = Color.White;
            btnUninstall.IconFont = IconFont.Auto;
            btnUninstall.IconSize = 24;
            btnUninstall.Location = new Point(138, 12);
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(120, 36);
            btnUninstall.TabIndex = 1;
            btnUninstall.Text = "  Desinstalar";
            btnUninstall.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUninstall.UseVisualStyleBackColor = false;
            // 
            // btnDetect
            // 
            btnDetect.BackColor = Color.FromArgb(30, 40, 60);
            btnDetect.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnDetect.FlatAppearance.BorderSize = 0;
            btnDetect.FlatStyle = FlatStyle.Flat;
            btnDetect.ForeColor = Color.White;
            btnDetect.IconChar = IconChar.Search;
            btnDetect.IconColor = Color.White;
            btnDetect.IconFont = IconFont.Auto;
            btnDetect.IconSize = 24;
            btnDetect.Location = new Point(12, 12);
            btnDetect.Name = "btnDetect";
            btnDetect.Size = new Size(120, 36);
            btnDetect.TabIndex = 0;
            btnDetect.Text = "  Detectar";
            btnDetect.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnDetect.UseVisualStyleBackColor = false;
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.FromArgb(20, 30, 45);
            panelMain.Controls.Add(splitContainer);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 60);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1000, 570);
            panelMain.TabIndex = 1;
            // 
            // splitContainer
            // 
            splitContainer.BackColor = Color.FromArgb(20, 30, 45);
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.BackColor = Color.FromArgb(20, 30, 45);
            splitContainer.Panel1.Controls.Add(lstInstallations);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.BackColor = Color.FromArgb(20, 30, 45);
            splitContainer.Panel2.Controls.Add(panelConsole);
            splitContainer.Size = new Size(1000, 570);
            splitContainer.SplitterDistance = 272;
            splitContainer.TabIndex = 0;
            // 
            // lstInstallations
            // 
            lstInstallations.BackColor = Color.FromArgb(20, 30, 45);
            lstInstallations.BorderStyle = BorderStyle.None;
            lstInstallations.Columns.AddRange(new ColumnHeader[] { colName, colVersion, colLocation, colConfidence });
            lstInstallations.Dock = DockStyle.Fill;
            lstInstallations.ForeColor = Color.White;
            lstInstallations.FullRowSelect = true;
            lstInstallations.Location = new Point(0, 0);
            lstInstallations.Name = "lstInstallations";
            lstInstallations.ShowItemToolTips = true;
            lstInstallations.Size = new Size(1000, 272);
            lstInstallations.TabIndex = 0;
            lstInstallations.UseCompatibleStateImageBehavior = false;
            lstInstallations.View = View.Details;
            // 
            // colName
            // 
            colName.Text = "Nombre";
            colName.Width = 300;
            // 
            // colVersion
            // 
            colVersion.Text = "Versión";
            colVersion.Width = 100;
            // 
            // colLocation
            // 
            colLocation.Text = "Ubicación";
            colLocation.Width = 500;
            // 
            // colConfidence
            // 
            colConfidence.Text = "Confianza";
            colConfidence.Width = 80;
            // 
            // panelConsole
            // 
            panelConsole.BackColor = Color.FromArgb(20, 30, 45);
            panelConsole.Controls.Add(txtConsole);
            panelConsole.Controls.Add(panelConsoleButtons);
            panelConsole.Dock = DockStyle.Fill;
            panelConsole.Location = new Point(0, 0);
            panelConsole.Name = "panelConsole";
            panelConsole.Size = new Size(1000, 294);
            panelConsole.TabIndex = 0;
            // 
            // txtConsole
            // 
            txtConsole.BackColor = Color.FromArgb(20, 30, 45);
            txtConsole.BorderStyle = BorderStyle.None;
            txtConsole.Dock = DockStyle.Fill;
            txtConsole.Font = new Font("Consolas", 9F);
            txtConsole.ForeColor = Color.White;
            txtConsole.Location = new Point(0, 0);
            txtConsole.Name = "txtConsole";
            txtConsole.ReadOnly = true;
            txtConsole.Size = new Size(1000, 224);
            txtConsole.TabIndex = 0;
            txtConsole.Text = "";
            // 
            // panelConsoleButtons
            // 
            panelConsoleButtons.BackColor = Color.FromArgb(30, 40, 60);
            panelConsoleButtons.Controls.Add(panelLeftB);
            panelConsoleButtons.Controls.Add(panelCentralB);
            panelConsoleButtons.Controls.Add(panelRightB);
            panelConsoleButtons.Controls.Add(btnGenerarScript);
            panelConsoleButtons.Controls.Add(btnAbrirLog);
            panelConsoleButtons.Controls.Add(btnCopyOutput);
            panelConsoleButtons.Dock = DockStyle.Bottom;
            panelConsoleButtons.Location = new Point(0, 224);
            panelConsoleButtons.Name = "panelConsoleButtons";
            panelConsoleButtons.Size = new Size(1000, 70);
            panelConsoleButtons.TabIndex = 1;
            // 
            // panelLeftB
            // 
            panelLeftB.Controls.Add(lblAnimatedText);
            panelLeftB.Location = new Point(13, 43);
            panelLeftB.Name = "panelLeftB";
            panelLeftB.Size = new Size(280, 24);
            panelLeftB.TabIndex = 5;
            // 
            // lblAnimatedText
            // 
            lblAnimatedText.BackColor = Color.Transparent;
            lblAnimatedText.Dock = DockStyle.Top;
            lblAnimatedText.ForeColor = Color.White;
            lblAnimatedText.Location = new Point(0, 0);
            lblAnimatedText.Name = "lblAnimatedText";
            lblAnimatedText.Size = new Size(280, 20);
            lblAnimatedText.TabIndex = 1;
            lblAnimatedText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelCentralB
            // 
            panelCentralB.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelCentralB.Controls.Add(lblProgress);
            panelCentralB.Location = new Point(300, 43);
            panelCentralB.Name = "panelCentralB";
            panelCentralB.Size = new Size(265, 24);
            panelCentralB.TabIndex = 4;
            // 
            // lblProgress
            // 
            lblProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblProgress.AutoSize = true;
            lblProgress.ForeColor = Color.White;
            lblProgress.Location = new Point(83, 3);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(137, 15);
            lblProgress.TabIndex = 1;
            lblProgress.Text = "Operación en curso - 0%";
            // 
            // panelRightB
            // 
            panelRightB.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelRightB.Controls.Add(progressBar);
            panelRightB.Location = new Point(571, 43);
            panelRightB.Name = "panelRightB";
            panelRightB.Size = new Size(417, 24);
            panelRightB.TabIndex = 3;
            // 
            // progressBar
            // 
            progressBar.Dock = DockStyle.Top;
            progressBar.Location = new Point(0, 0);
            progressBar.MinimumSize = new Size(100, 20);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(417, 20);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 0;
            // 
            // btnGenerarScript
            // 
            btnGenerarScript.BackColor = Color.FromArgb(30, 40, 60);
            btnGenerarScript.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnGenerarScript.FlatAppearance.BorderSize = 0;
            btnGenerarScript.FlatStyle = FlatStyle.Flat;
            btnGenerarScript.ForeColor = Color.White;
            btnGenerarScript.IconChar = IconChar.FileCode;
            btnGenerarScript.IconColor = Color.White;
            btnGenerarScript.IconFont = IconFont.Auto;
            btnGenerarScript.IconSize = 20;
            btnGenerarScript.Location = new Point(264, 4);
            btnGenerarScript.Name = "btnGenerarScript";
            btnGenerarScript.Size = new Size(120, 32);
            btnGenerarScript.TabIndex = 2;
            btnGenerarScript.Text = "  Generar Script";
            btnGenerarScript.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnGenerarScript.UseVisualStyleBackColor = false;
            // 
            // btnAbrirLog
            // 
            btnAbrirLog.BackColor = Color.FromArgb(30, 40, 60);
            btnAbrirLog.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnAbrirLog.FlatAppearance.BorderSize = 0;
            btnAbrirLog.FlatStyle = FlatStyle.Flat;
            btnAbrirLog.ForeColor = Color.White;
            btnAbrirLog.IconChar = IconChar.FolderOpen;
            btnAbrirLog.IconColor = Color.White;
            btnAbrirLog.IconFont = IconFont.Auto;
            btnAbrirLog.IconSize = 20;
            btnAbrirLog.Location = new Point(138, 4);
            btnAbrirLog.Name = "btnAbrirLog";
            btnAbrirLog.Size = new Size(120, 32);
            btnAbrirLog.TabIndex = 1;
            btnAbrirLog.Text = "  Abrir Log";
            btnAbrirLog.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAbrirLog.UseVisualStyleBackColor = false;
            // 
            // btnCopyOutput
            // 
            btnCopyOutput.BackColor = Color.FromArgb(30, 40, 60);
            btnCopyOutput.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 90);
            btnCopyOutput.FlatAppearance.BorderSize = 0;
            btnCopyOutput.FlatStyle = FlatStyle.Flat;
            btnCopyOutput.ForeColor = Color.White;
            btnCopyOutput.IconChar = IconChar.Copy;
            btnCopyOutput.IconColor = Color.White;
            btnCopyOutput.IconFont = IconFont.Auto;
            btnCopyOutput.IconSize = 20;
            btnCopyOutput.Location = new Point(12, 4);
            btnCopyOutput.Name = "btnCopyOutput";
            btnCopyOutput.Size = new Size(120, 32);
            btnCopyOutput.TabIndex = 0;
            btnCopyOutput.Text = "  Copiar";
            btnCopyOutput.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCopyOutput.UseVisualStyleBackColor = false;
            // 
            // lblInfoText
            // 
            lblInfoText.Anchor = AnchorStyles.Left;
            lblInfoText.ForeColor = Color.White;
            lblInfoText.Location = new Point(12, 7);
            lblInfoText.Name = "lblInfoText";
            lblInfoText.Size = new Size(120, 15);
            lblInfoText.TabIndex = 0;
            lblInfoText.Text = "Información: 0/0";
            // 
            // animationTimer
            // 
            animationTimer.Interval = 500;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 30, 45);
            ClientSize = new Size(1000, 630);
            Controls.Add(panelMain);
            Controls.Add(panelTop);
            MinimumSize = new Size(1000, 630);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DesinstalaPhotoshop";
            panelTop.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelConsole.ResumeLayout(false);
            panelConsoleButtons.ResumeLayout(false);
            panelLeftB.ResumeLayout(false);
            panelCentralB.ResumeLayout(false);
            panelCentralB.PerformLayout();
            panelRightB.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelTop;
        private FontAwesome.Sharp.IconButton btnDetect;
        private FontAwesome.Sharp.IconButton btnUninstall;
        private FontAwesome.Sharp.IconButton btnCleanup;
        private FontAwesome.Sharp.IconButton btnTestMode;
        private FontAwesome.Sharp.IconButton btnCancel;
        private FontAwesome.Sharp.IconButton btnRestore;
        private Panel panelMain;
        private SplitContainer splitContainer;
        private ListView lstInstallations;
        private ColumnHeader colName;
        private ColumnHeader colVersion;
        private ColumnHeader colLocation;
        private ColumnHeader colConfidence;
        private Panel panelConsole;
        private RichTextBox txtConsole;
        private Panel panelConsoleButtons;
        private FontAwesome.Sharp.IconButton btnCopyOutput;
        private FontAwesome.Sharp.IconButton btnAbrirLog;
        private FontAwesome.Sharp.IconButton btnGenerarScript;
        private Label lblProgress;
        private ProgressBar progressBar;
        private Label lblInfoText;
        private Label lblAnimatedText;
        private System.Windows.Forms.Timer animationTimer;
        private Panel panelRightB;
        private Panel panelCentralB;
        private Panel panelLeftB;
    }
}
