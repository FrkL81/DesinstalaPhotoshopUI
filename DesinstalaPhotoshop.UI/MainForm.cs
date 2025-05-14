using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using CustomMsgBoxLibrary;
using CustomMsgBoxLibrary.Types;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Helpers;

namespace DesinstalaPhotoshop.UI
{
    using DesinstalaPhotoshop.UI.Models;
    public partial class MainForm : Form
    {
        // Lista de instalaciones detectadas
        private List<object>? _detectedInstallations = new List<object>();

        // Token de cancelación para operaciones asíncronas
        private CancellationTokenSource? _cts = new CancellationTokenSource();

        // Estado de la animación de progreso
        private int _animationDots = 0;
        private string _currentOperation = string.Empty;

        // Estado de la animación de texto informativo
        private int _textAnimationState = 0;
        private string[] _animationTexts = new string[] { "Procesando", "Analizando", "Verificando", "Ejecutando" };
        private int _currentInfoCount = 0;
        private int _totalInfoCount = 0;

        // Indica si la aplicación está en modo de desarrollo
        private bool _developmentMode = false; // Cambiado a false para permitir la detección sin reiniciar

        public MainForm()
        {
            InitializeComponent();
            SetupControls();
            SetupEventHandlers();

            // Verificar si estamos en modo de desarrollo y mostrar advertencia
            if (_developmentMode)
            {
                LogWarning("Aplicación ejecutándose en MODO DESARROLLO (sin permisos elevados)");
                LogInfo("Las operaciones que requieran permisos elevados solicitarán reiniciar la aplicación");
            }
        }

        private void SetupControls()
        {
            // Establecer el icono del formulario
            try
            {
                // Intentar cargar el icono desde el archivo
                string iconPath = Path.Combine(Application.StartupPath, "Resources", "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new System.Drawing.Icon(iconPath);
                }
            }
            catch (Exception ex)
            {
                // Si hay algún error al cargar el icono, lo registramos pero continuamos
                Console.WriteLine($"Error al cargar el icono: {ex.Message}");
            }

            // Configurar propiedades del formulario
            this.Text = "DesinstalaPhotoshop";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(20, 30, 45); // Color de fondo principal

            // Configurar tooltips para los botones
            SetupTooltips();

            // Configurar el timer para la animación de progreso
            animationTimer.Tick += AnimationTimer_Tick!;

            // Inicializar el panel de información
            InitializeInfoPanel();

            // Inicializar el estado de los botones
            UpdateButtonsState();
        }

        private void SetupEventHandlers()
        {
            // Botones principales
            btnDetect.Click += BtnDetect_Click!;
            btnUninstall.Click += BtnUninstall_Click!;
            btnCleanup.Click += BtnCleanup_Click!;
            btnTestMode.Click += BtnTestMode_Click!;
            btnCancel.Click += BtnCancel_Click!;
            btnRestore.Click += BtnRestore_Click!;

            // Botones de consola
            btnCopyOutput.Click += BtnCopyOutput_Click!;
            btnAbrirLog.Click += BtnAbrirLog_Click!;
            btnGenerarScript.Click += BtnGenerarScript_Click!;

            // Lista de instalaciones
            lstInstallations.SelectedIndexChanged += LstInstallations_SelectedIndexChanged!;

            // Eventos del formulario para actualizar el layout
            this.Load += MainForm_Load_UpdateLayout!;
            this.Resize += MainForm_Resize_UpdateLayout!;
        }

        private void SetupTooltips()
        {
            // Crear tooltips para los botones
            ToolTip? toolTip = new ToolTip();
            toolTip.SetToolTip(btnDetect, "Detectar instalaciones de Photoshop en el sistema");
            toolTip.SetToolTip(btnUninstall, "Desinstalar la instalación principal seleccionada");
            toolTip.SetToolTip(btnCleanup, "Limpiar residuos de Photoshop");
            toolTip.SetToolTip(btnTestMode, "Ejecutar operaciones en modo de prueba sin realizar cambios reales");
            toolTip.SetToolTip(btnCancel, "Cancelar la operación en curso");
            toolTip.SetToolTip(btnRestore, "Restaurar copias de seguridad");
            toolTip.SetToolTip(btnCopyOutput, "Copiar el contenido de la consola al portapapeles");
            toolTip.SetToolTip(btnAbrirLog, "Abrir la carpeta de logs");
            toolTip.SetToolTip(btnGenerarScript, "Generar script de limpieza");
        }

        #region Métodos de Eventos

        private async void BtnDetect_Click(object sender, EventArgs e)
        {
            // Esta operación requiere permisos elevados
            LogInfo("Detectando instalaciones de Photoshop...");

            // Actualizar información de progreso inicial
            UpdateInfoProgress(0, 5);

            // Usar el método RunOperationAsync con requiresElevation=true
            await RunOperationAsync<List<PhotoshopInstallation>>(
                async (progress, token) =>
                {
                    try
                    {
                        // Crear instancias de los servicios necesarios
                        var loggingService = new DesinstalaPhotoshop.Core.Services.LoggingService();
                        var registryHelper = new DesinstalaPhotoshop.Core.Services.Helpers.RegistryHelper(loggingService);
                        var fileSystemHelper = new DesinstalaPhotoshop.Core.Services.Helpers.FileSystemHelper(loggingService);

                        // Crear una instancia del servicio de detección con todos los servicios necesarios
                        var detectionService = new DesinstalaPhotoshop.Core.Services.DetectionService(
                            loggingService,
                            registryHelper,
                            fileSystemHelper);

                        // Crear un objeto de progreso para pasar al servicio
                        var progressReporter = new Progress<DesinstalaPhotoshop.Core.Models.ProgressInfo>(info => {
                            // Actualizar la UI con la información de progreso
                            progress.Report(new ProgressInfo(
                                info.ProgressPercentage,
                                info.OperationTitle,
                                info.StatusMessage,
                                info.OperationStatus));

                            // Actualizar el contador de pasos
                            UpdateInfoProgress((int)(info.ProgressPercentage / 20), 5); // Convertir porcentaje a pasos
                        });

                        // Ejecutar la detección
                        LogInfo("Iniciando detección de instalaciones...");
                        var installations = await detectionService.DetectInstallationsAsync(progressReporter, token);
                        LogInfo($"Detección finalizada. Se encontraron {installations.Count} instalaciones.");

                        // Actualizar la lista de instalaciones detectadas
                        _detectedInstallations = new List<object>(installations.Cast<object>().ToList());

                        // Actualizar la UI con las instalaciones detectadas
                        UpdateInstallationsList();

                        LogSuccess($"Detección completada con éxito. Se encontraron {installations.Count} instalaciones.");
                        return installations;
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error durante la detección: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            LogError($"Error interno: {ex.InnerException.Message}");
                        }
                        throw;
                    }
                },
                "Detectando instalaciones",
                requiresElevation: true);
        }

        private async void BtnUninstall_Click(object sender, EventArgs e)
        {
            // Esta operación requiere permisos elevados
            LogInfo("Preparando desinstalación de Photoshop...");

            // Verificar si hay una instalación seleccionada
            if (lstInstallations.SelectedItems.Count == 0)
            {
                CustomMsgBox.Show(
                    prompt: "Debe seleccionar una instalación para desinstalar.",
                    title: "Selección requerida",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Warning);
                return;
            }

            // Obtener la instalación seleccionada
            if (lstInstallations.SelectedItems[0].Tag is not PhotoshopInstallation selectedInstallation)
            {
                LogError("Error al obtener la instalación seleccionada.");
                return;
            }

            // Mostrar opciones de desinstalación
            using (var form = new UninstallOptionsForm())
            {
                if (form.ShowDialog() != DialogResult.OK)
                {
                    LogInfo("Desinstalación cancelada por el usuario.");
                    return;
                }

                // Confirmar la desinstalación
                var result = CustomMsgBox.Show(
                    prompt: $"¿Está seguro de que desea desinstalar {selectedInstallation.DisplayName}?\n\nEsta operación no se puede deshacer.",
                    title: "Confirmar desinstalación",
                    buttons: CustomMessageBoxButtons.YesNo,
                    icon: CustomMessageBoxIcon.Warning);

                if (result != CustomDialogResult.Yes)
                {
                    LogInfo("Desinstalación cancelada por el usuario.");
                    return;
                }

                // Actualizar información de progreso inicial
                UpdateInfoProgress(0, 5);

                // Usar el método RunOperationAsync con requiresElevation=true
                await RunOperationAsync<OperationResult>(
                    async (progress, token) =>
                    {
                        try
                        {
                            // Crear instancias de los servicios necesarios
                            var loggingService = new DesinstalaPhotoshop.Core.Services.LoggingService();
                            var registryHelper = new DesinstalaPhotoshop.Core.Services.Helpers.RegistryHelper(loggingService);
                            var fileSystemHelper = new DesinstalaPhotoshop.Core.Services.Helpers.FileSystemHelper(loggingService);
                            var backupService = new DesinstalaPhotoshop.Core.Services.BackupService(loggingService, fileSystemHelper, registryHelper);

                            // Crear una instancia del servicio de desinstalación
                            var uninstallService = new DesinstalaPhotoshop.Core.Services.UninstallService(
                                loggingService,
                                fileSystemHelper,
                                registryHelper,
                                backupService);

                            // Ejecutar la desinstalación
                            LogInfo($"Iniciando desinstalación de {selectedInstallation.DisplayName}...");
                            var result = await uninstallService.UninstallAsync(
                                selectedInstallation,
                                form.CreateBackup,
                                form.WhatIfMode,
                                form.RemoveUserData,
                                form.RemoveSharedComponents,
                                new Progress<DesinstalaPhotoshop.Core.Models.ProgressInfo>(p =>
                                {
                                    // Convertir de Core.Models.ProgressInfo a UI.Models.ProgressInfo
                                    var uiProgress = new DesinstalaPhotoshop.UI.Models.ProgressInfo(
                                        p.ProgressPercentage,
                                        p.OperationTitle,
                                        p.StatusMessage,
                                        p.OperationStatus);
                                    progress.Report(uiProgress);
                                }),
                                token);

                            if (result.Success)
                            {
                                LogSuccess($"Desinstalación completada con éxito: {result.Message}");

                                // Actualizar la lista de instalaciones (volver a detectar)
                                if (!form.WhatIfMode)
                                {
                                    LogInfo("Actualizando lista de instalaciones...");
                                    BtnDetect_Click(this, EventArgs.Empty);
                                }
                            }
                            else
                            {
                                LogError($"Error durante la desinstalación: {result.Message}");
                            }

                            return result;
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error durante la desinstalación: {ex.Message}");
                            if (ex.InnerException != null)
                            {
                                LogError($"Error interno: {ex.InnerException.Message}");
                            }
                            throw;
                        }
                    },
                    "Desinstalando Photoshop",
                    requiresElevation: true);
            }
        }

        private async void BtnCleanup_Click(object sender, EventArgs e)
        {
            // Implementación pendiente - Esta operación requiere permisos elevados
            LogInfo("Limpiando residuos de Photoshop...");

            // Actualizar información de progreso para demostración
            UpdateInfoProgress(0, 4);

            // Ejemplo de uso del método RunOperationAsync con requiresElevation=true
            await RunOperationAsync<bool>(
                async (progress, token) =>
                {
                    // Simulación de operación con actualización de progreso
                    for (int i = 1; i <= 4; i++)
                    {
                        await Task.Delay(1200, token);
                        UpdateInfoProgress(i, 4);
                        progress.Report(ProgressInfo.Running(i * 25, "Limpiando residuos", $"Limpieza {i} de 4 completada"));
                    }

                    LogSuccess("Limpieza completada con éxito.");
                    progress.Report(ProgressInfo.Completed("Limpieza", "Limpieza completada con éxito."));
                    return true;
                },
                "Limpiando residuos",
                requiresElevation: true);
        }

        private void BtnTestMode_Click(object sender, EventArgs e)
        {
            // Implementación pendiente - Esta operación no requiere permisos elevados
            LogInfo("Configurando modo de prueba...");

            // Actualizar información de progreso para demostración
            UpdateInfoProgress(0, 1);

            // Mostrar formulario de opciones de modo de prueba
            using (var form = new TestModeOptionsForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Actualizar progreso al completar
                    UpdateInfoProgress(1, 1);
                    LogSuccess($"Modo de prueba activado: {form.SelectedOperation}");
                }
                else
                {
                    // Actualizar progreso al cancelar
                    UpdateInfoProgress(0, 0);
                    LogInfo("Configuración de modo de prueba cancelada.");
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                _cts?.Cancel();
                AppendToConsole("Operación cancelada por el usuario.", Color.Yellow);
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error al cancelar la operación: {ex.Message}", Color.Red);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            // Implementación pendiente
            AppendToConsole("Restaurando copia de seguridad...", Color.White);
        }

        private void BtnCopyOutput_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtConsole != null && !string.IsNullOrEmpty(txtConsole.Text))
                {
                    Clipboard.SetText(txtConsole.Text);
                    CustomMsgBox.Show(
                        prompt: "El contenido de la consola ha sido copiado al portapapeles.",
                        title: "Información",
                        buttons: CustomMessageBoxButtons.OK,
                        icon: CustomMessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMsgBox.Show(
                    prompt: $"Error al copiar al portapapeles: {ex.Message}",
                    title: "Error",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error);
            }
        }

        private void BtnAbrirLog_Click(object sender, EventArgs e)
        {
            try
            {
                // Implementación pendiente - Obtener la ruta del archivo de log
                string logDirectory = Path.Combine(Application.StartupPath, "logs");

                if (!string.IsNullOrEmpty(logDirectory) && btnAbrirLog != null)
                {
                    // Crear la carpeta si no existe
                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    // Abrir la carpeta en el explorador
                    var psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = logDirectory,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                CustomMsgBox.Show(
                    prompt: $"Error al abrir la carpeta de logs: {ex.Message}",
                    title: "Error",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error);
            }
        }

        private void BtnGenerarScript_Click(object sender, EventArgs e)
        {
            // Implementación pendiente
            AppendToConsole("Generando script de limpieza...", Color.White);
        }

        private void LstInstallations_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonsState();
        }

        #endregion

        #region Métodos de UI

        /// <summary>
        /// Actualiza el estado de los botones según el estado actual de la aplicación
        /// </summary>
        private void UpdateButtonsState()
        {
            // Verificar si hay instalaciones residuales detectadas
            bool hasResiduals = false;

            if (_detectedInstallations != null && _detectedInstallations.Count > 0)
            {
                foreach (var obj in _detectedInstallations)
                {
                    if (obj is DesinstalaPhotoshop.Core.Models.PhotoshopInstallation installation)
                    {
                        if (installation.IsResidual)
                            hasResiduals = true;
                    }
                }
            }

            // Verificar si hay una selección en la lista
            bool hasSelection = lstInstallations.SelectedItems.Count > 0;

            // Verificar si la selección es una instalación principal o posible
            bool selectedMainInstallation = false;
            if (hasSelection && lstInstallations.SelectedItems[0].Tag is PhotoshopInstallation selectedInstallation)
            {
                selectedMainInstallation = selectedInstallation.IsMainInstallation ||
                                          selectedInstallation.InstallationType == InstallationType.PossibleMainInstallation;
            }

            // El botón Detectar siempre está habilitado excepto durante operaciones
            btnDetect.Enabled = true;

            // El botón Desinstalar requiere una selección de instalación principal o posible
            btnUninstall.Enabled = hasSelection && selectedMainInstallation;

            // El botón Limpiar está habilitado si hay residuos detectados
            btnCleanup.Enabled = hasResiduals;

            // El botón Modo de Prueba siempre está habilitado
            btnTestMode.Enabled = true;

            // El botón Cancelar solo está habilitado durante operaciones
            btnCancel.Enabled = false;

            // El botón Restaurar está deshabilitado por ahora
            btnRestore.Enabled = false;

            // Actualizar colores de los botones según su estado
            UpdateButtonColors();
        }

        /// <summary>
        /// Actualiza los colores de los botones según su estado (habilitado/deshabilitado)
        /// </summary>
        private void UpdateButtonColors()
        {
            // Color para botones deshabilitados
            Color disabledBackColor = Color.FromArgb(60, 60, 60);
            Color disabledForeColor = Color.Gray;

            // Actualizar colores de los botones principales
            UpdateButtonColor(btnDetect, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnUninstall, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnCleanup, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnTestMode, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnCancel, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnRestore, disabledBackColor, disabledForeColor);

            // Actualizar colores de los botones de consola
            UpdateButtonColor(btnCopyOutput, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnAbrirLog, disabledBackColor, disabledForeColor);
            UpdateButtonColor(btnGenerarScript, disabledBackColor, disabledForeColor);
        }

        /// <summary>
        /// Actualiza los colores de un botón según su estado
        /// </summary>
        private void UpdateButtonColor(IconButton button, Color disabledBackColor, Color disabledForeColor)
        {
            if (button.Enabled)
            {
                // Colores para botón habilitado
                button.BackColor = Color.FromArgb(30, 40, 60);
                button.ForeColor = Color.White;
                button.IconColor = Color.White;
            }
            else
            {
                // Colores para botón deshabilitado
                button.BackColor = disabledBackColor;
                button.ForeColor = disabledForeColor;
                button.IconColor = disabledForeColor;
            }
        }

        /// <summary>
        /// Prepara la UI para una operación en curso
        /// </summary>
        private void PrepareUIForOperation(string operationName)
        {
            // Deshabilitar botones durante la operación
            btnDetect.Enabled = false;
            btnUninstall.Enabled = false;
            btnCleanup.Enabled = false;
            btnTestMode.Enabled = false;
            btnRestore.Enabled = false;

            // Habilitar botón de cancelar
            btnCancel.Enabled = true;

            // Actualizar colores de los botones
            UpdateButtonColors();

            // Mostrar controles de progreso
            lblProgress.Visible = true;
            progressBar.Visible = true;
            lblAnimatedText.Visible = true;

            // Iniciar animación de progreso
            _currentOperation = operationName;
            _animationDots = 0;
            progressBar.Value = 0;
            lblProgress.Text = $"{_currentOperation} - 0%";
            animationTimer.Start();
        }

        /// <summary>
        /// Restaura la UI después de una operación
        /// </summary>
        private void RestoreUI()
        {
            // Detener animación
            animationTimer.Stop();

            // Ocultar controles de progreso
            lblProgress.Visible = false;
            progressBar.Visible = false;
            lblAnimatedText.Visible = false;

            // Restaurar estado de los botones
            UpdateButtonsState();
        }

        /// <summary>
        /// Inicia la animación de progreso
        /// </summary>
        private void StartProgressAnimation(string operationName)
        {
            PrepareUIForOperation(operationName);
        }

        /// <summary>
        /// Actualiza el progreso de la operación en curso
        /// </summary>
        private void UpdateProgress(int percentage, string? statusText = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(percentage, statusText)));
                return;
            }

            progressBar.Value = percentage;

            if (!string.IsNullOrEmpty(statusText))
            {
                lblProgress.Text = $"{statusText} - {percentage}%";
            }
            else
            {
                lblProgress.Text = $"{_currentOperation} - {percentage}%";
            }

            // Si llegamos al 100%, detener la animación
            if (percentage >= 100)
            {
                animationTimer.Stop();
            }
        }

        /// <summary>
        /// Inicializa el panel de información
        /// </summary>
        private void InitializeInfoPanel()
        {
            // Inicializar valores
            _currentInfoCount = 0;
            _totalInfoCount = 0;
            _textAnimationState = 0;

            // Actualizar texto informativo
            UpdateInfoText();

            // Inicializar barra de progreso
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            // Ocultar controles de progreso inicialmente
            lblProgress.Visible = false;
            progressBar.Visible = false;
            lblAnimatedText.Visible = false;
        }

        private void MainForm_Load_UpdateLayout(object sender, EventArgs e)
        {
            UpdatePanelInfoLayout();
        }

        private void MainForm_Resize_UpdateLayout(object sender, EventArgs e)
        {
            UpdatePanelInfoLayout();
        }

        private void UpdatePanelInfoLayout()
        {
            // Asegurarse de que los controles existan
            if (panelConsoleButtons == null || progressBar == null || lblAnimatedText == null)
                return;

            // Solo ejecutar si el panel es visible y tiene un ancho > 0
            if (!panelConsoleButtons.Visible || panelConsoleButtons.ClientSize.Width <= 0)
                return;

            int margin = 5;
            int spaceBetweenControls = 5;
            int panelClientWidth = panelConsoleButtons.ClientSize.Width;
            int panelClientHeight = panelConsoleButtons.ClientSize.Height;

            // Calcular el ancho de la barra de progreso (60% del ancho del panel)
            // Nota: Usamos un porcentaje mayor ya que la barra de progreso ahora está en panelConsoleButtons
            int progressBarTargetWidth = (int)(panelClientWidth * 0.6);
            progressBarTargetWidth = Math.Max(50, progressBarTargetWidth); // Ancho mínimo

            // Actualizar tamaño y posición de la barra de progreso
            // Nota: progressBar ya tiene un tamaño fijo en el diseñador, así que solo ajustamos si es necesario
            if (progressBar.Width != progressBarTargetWidth)
            {
                progressBar.Width = progressBarTargetWidth;
                progressBar.Location = new Point(
                    panelClientWidth - progressBarTargetWidth - margin,
                    (panelClientHeight - progressBar.Height) / 2);
            }

            // Actualizar tamaño y posición del texto animado
            lblAnimatedText.Location = new Point(margin, (panelClientHeight - lblAnimatedText.Height) / 2);
            lblAnimatedText.Width = progressBar.Location.X - margin - spaceBetweenControls;
        }

        /// <summary>
        /// Actualiza el texto informativo con el contador de progreso
        /// </summary>
        private void UpdateInfoText()
        {
            lblInfoText.Text = $"Información: {_currentInfoCount}/{_totalInfoCount}";
        }

        /// <summary>
        /// Actualiza el progreso de información
        /// </summary>
        private void UpdateInfoProgress(int current, int total)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateInfoProgress(current, total)));
                return;
            }

            _currentInfoCount = current;
            _totalInfoCount = total;

            // Actualizar texto informativo
            UpdateInfoText();

            // Actualizar barra de progreso
            if (total > 0)
            {
                int percentage = (int)((float)current / total * 100);
                progressBar.Value = percentage;
            }
            else
            {
                progressBar.Value = 0;
            }
        }

        /// <summary>
        /// Actualiza la lista de instalaciones detectadas en la UI
        /// </summary>
        private void UpdateInstallationsList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateInstallationsList));
                return;
            }

            // Limpiar la lista actual
            lstInstallations.Items.Clear();

            // Si no hay instalaciones, salir
            if (_detectedInstallations == null || _detectedInstallations.Count == 0)
            {
                LogInfo("No se encontraron instalaciones de Photoshop.");
                return;
            }

            // Agregar cada instalación a la lista
            foreach (var obj in _detectedInstallations)
            {
                if (obj is DesinstalaPhotoshop.Core.Models.PhotoshopInstallation installation)
                {
                    // Crear un ListViewItem con la información principal
                    var item = new ListViewItem(installation.DisplayName);
                    item.SubItems.Add(installation.Version);
                    item.SubItems.Add(installation.InstallLocation);
                    item.SubItems.Add(installation.InstallationType.ToString());
                    item.SubItems.Add(installation.ConfidenceScore.ToString() + "%");

                    // Guardar la referencia a la instalación en el Tag para acceso posterior
                    item.Tag = installation;

                    // Agregar el item a la lista
                    lstInstallations.Items.Add(item);
                }
            }

            // Actualizar el estado de los botones
            UpdateButtonsState();
        }

        /// <summary>
        /// Actualiza el texto animado
        /// </summary>
        private void UpdateAnimatedText()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateAnimatedText));
                return;
            }

            // Actualizar el texto animado
            string text = _animationTexts[_textAnimationState];
            string dots = new string('.', _animationDots);
            lblAnimatedText.Text = $"{text}{dots}";

            // Avanzar al siguiente estado
            _textAnimationState = (_textAnimationState + 1) % _animationTexts.Length;
            _animationDots = (_animationDots + 1) % 4;
        }

        /// <summary>
        /// Manejador del evento Tick del timer de animación
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            UpdateAnimatedText();

            // Actualizar texto de progreso con animación de puntos
            string dots = new string('.', _animationDots);
            lblProgress.Text = $"{_currentOperation}{dots} - {progressBar.Value}%";
        }

        #endregion

        #region Métodos de Consola

        /// <summary>
        /// Agrega texto a la consola con el color especificado
        /// </summary>
        private void AppendToConsole(string text, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendToConsole(text, color)));
                return;
            }

            // Agregar texto con el color especificado
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.SelectionLength = 0;
            txtConsole.SelectionColor = color;
            txtConsole.AppendText(text + Environment.NewLine);
            txtConsole.SelectionColor = txtConsole.ForeColor;

            // Desplazar al final
            txtConsole.ScrollToCaret();
        }

        /// <summary>
        /// Agrega un mensaje informativo a la consola
        /// </summary>
        private void LogInfo(string message)
        {
            AppendToConsole($"[INFO] {message}", Color.White);
        }

        /// <summary>
        /// Agrega un mensaje de éxito a la consola
        /// </summary>
        private void LogSuccess(string message)
        {
            AppendToConsole($"[ÉXITO] {message}", Color.LimeGreen);
        }

        /// <summary>
        /// Agrega un mensaje de advertencia a la consola
        /// </summary>
        private void LogWarning(string message)
        {
            AppendToConsole($"[ADVERTENCIA] {message}", Color.Yellow);
        }

        /// <summary>
        /// Agrega un mensaje de error a la consola
        /// </summary>
        private void LogError(string message)
        {
            AppendToConsole($"[ERROR] {message}", Color.Red);
        }

        #endregion

        #region Métodos de Operaciones Asíncronas

        /// <summary>
        /// Ejecuta una operación asíncrona con soporte para cancelación y progreso
        /// </summary>
        /// <typeparam name="T">Tipo de resultado de la operación</typeparam>
        /// <param name="operation">Función que implementa la operación</param>
        /// <param name="operationName">Nombre de la operación para mostrar en la UI</param>
        /// <param name="requiresElevation">Indica si la operación requiere permisos elevados</param>
        /// <returns>Resultado de la operación</returns>
        private async Task<T> RunOperationAsync<T>(
            Func<IProgress<ProgressInfo>, CancellationToken, Task<T>> operation,
            string operationName,
            bool requiresElevation = false)
        {
            // Verificar si necesitamos permisos elevados
            if (requiresElevation && !IsRunningAsAdmin())
            {
                LogWarning("Esta operación requiere permisos de administrador.");
                LogInfo("Se reiniciará la aplicación con permisos elevados...");

                // Solicitar permisos elevados
                LogInfo("Solicitando permisos de administrador...");
                RequestElevatedPermissions();
                return default; // No continuamos con la ejecución ya que la aplicación se reiniciará
            }

            try
            {
                // Preparar la UI para la operación
                StartProgressAnimation(operationName);
                LogInfo($"Iniciando operación: {operationName}");

                // Crear un nuevo token de cancelación
                _cts = new CancellationTokenSource();

                // Crear un objeto de progreso que actualiza la UI
                var progress = new Progress<ProgressInfo>(info => {
                    // Actualizar porcentaje y mensaje de estado
                    UpdateProgress(info.ProgressPercentage, info.StatusMessage);

                    // Mostrar mensajes de estado en la consola
                    if (!string.IsNullOrEmpty(info.StatusMessage))
                    {
                        LogInfo(info.StatusMessage);
                    }
                });

                // Ejecutar la operación
                LogInfo("Ejecutando operación...");
                var result = await operation(progress, _cts.Token);
                LogInfo($"Operación {operationName} completada con éxito");
                return result;
            }
            catch (OperationCanceledException)
            {
                LogWarning("Operación cancelada por el usuario.");
                throw; // Re-lanzar la excepción para que el llamador sepa que fue cancelada
            }
            catch (Exception ex)
            {
                LogError($"Error al ejecutar la operación: {ex.Message}");
                throw; // Re-lanzar la excepción para que el llamador pueda manejarla
            }
            finally
            {
                // Restaurar la UI
                RestoreUI();
            }
        }

        /// <summary>
        /// Verifica si la aplicación está ejecutándose con permisos de administrador
        /// </summary>
        private bool IsRunningAsAdmin()
        {
            // En modo de desarrollo, simulamos que tenemos permisos elevados para permitir la detección
            if (_developmentMode)
                return true;

            return AdminHelper.IsRunningAsAdmin();
        }

        /// <summary>
        /// Solicita permisos de administrador reiniciando la aplicación
        /// </summary>
        /// <param name="arguments">Argumentos adicionales para pasar a la aplicación reiniciada</param>
        private void RequestElevatedPermissions(string arguments = "")
        {
            if (!AdminHelper.RestartAsAdmin(arguments))
            {
                CustomMsgBox.Show(
                    prompt: "No se pudo reiniciar la aplicación con permisos de administrador.",
                    title: "Error de Elevación",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
