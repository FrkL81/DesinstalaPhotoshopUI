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
using System.Text;
using System.Security.Principal; // Para WindowsIdentity y WindowsPrincipal
using CustomMsgBoxLibrary;
using CustomMsgBoxLibrary.Types;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services;
using DesinstalaPhotoshop.Core.Services.Helpers;
using DesinstalaPhotoshop.Core.Services.Interfaces;

// UI.Models.ProgressInfo ahora se considera redundante, se usará Core.Models.ProgressInfo
// using DesinstalaPhotoshop.UI.Models; 

namespace DesinstalaPhotoshop.UI
{
    public partial class MainForm : Form
    {
        // Servicios del Core
        private readonly ILoggingService _loggingService;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IRegistryHelper _registryHelper;
        private readonly IDetectionService _detectionService;
        private readonly IUninstallService _uninstallService;
        private readonly ICleanupService _cleanupService;
        private readonly IBackupService _backupService;
        private readonly IProcessService _processService;
        private readonly IScriptGenerator _scriptGenerator;

        // Lista de instalaciones detectadas (usando el modelo del Core)
        private List<PhotoshopInstallation> _detectedInstallations = new List<PhotoshopInstallation>();

        // Token de cancelación para operaciones asíncronas
        private CancellationTokenSource? _cancellationTokenSource;

        // Estado de la animación de progreso
        private int _animationDots = 0;
        private string _currentOperation = string.Empty;

        // Estado de la animación de texto informativo
        private int _textAnimationState = 0;
        private readonly string[] _animationTexts = { "Procesando", "Analizando", "Verificando", "Ejecutando" };
        
        // Tooltip para botones
        private readonly ToolTip toolTip = new ToolTip();

        // Indica si la aplicación está en modo de desarrollo (afecta la solicitud de elevación)
        // DECISIÓN: Mantener en false para producción. En un entorno de desarrollo real, esto podría ser configurable.
        private readonly bool _developmentMode = false; 

        // Indica si la aplicación se inició con el argumento --elevated
        private readonly bool _startedElevated = false;


        // Constructor
        public MainForm(bool startedElevated = false)
        {
            InitializeComponent();
            _startedElevated = startedElevated;

            // Inicialización de servicios (Inyección de dependencias manual básica)
            _loggingService = new LoggingService(); // LoggingService no tiene dependencias complejas

            _fileSystemHelper = new Core.Services.Helpers.FileSystemHelper(_loggingService);
            _registryHelper = new Core.Services.Helpers.RegistryHelper(_loggingService);

            _processService = new Core.Services.ProcessService(_loggingService);
            _backupService = new Core.Services.BackupService(_loggingService, _fileSystemHelper, _registryHelper);
            
            _detectionService = new Core.Services.DetectionService(_loggingService, _registryHelper, _fileSystemHelper);
            _cleanupService = new Core.Services.CleanupService(_loggingService, _backupService, _processService, _fileSystemHelper, _registryHelper);
            _uninstallService = new Core.Services.UninstallService(_loggingService, _fileSystemHelper, _registryHelper, _backupService, _processService);
            _scriptGenerator = new Core.Services.ScriptGenerator(_loggingService);

            SetupControls();
            SetupEventHandlers();

            // Loguear estado de inicio
            _loggingService.LogInfo($"Aplicación iniciada. Elevada por argumento: {_startedElevated}. Privilegios actuales: { (IsElevated() ? "Sí" : "No") }.");
            if (_developmentMode)
            {
                _loggingService.LogWarning("MODO DESARROLLO ACTIVO: Las solicitudes de elevación pueden ser omitidas.");
            }
        }

        private void SetupControls()
        {
            try
            {
                string iconPath = Path.Combine(Application.StartupPath, "Resources", "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new System.Drawing.Icon(iconPath);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al cargar el icono de la aplicación: {ex.Message}");
            }

            this.Text = "DesinstalaPhotoshop";
             if (_startedElevated && IsElevated() && !this.Text.Contains("(Elevado)"))
            {
                this.Text += " (Elevado)";
            }
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 630); 
            this.BackColor = Color.FromArgb(20, 30, 45);

            SetupTooltips();
            animationTimer.Tick += AnimationTimer_Tick!;
            
            // Ocultar controles de progreso inicialmente
            lblProgress.Visible = false;
            progressBar.Visible = false;
            lblAnimatedText.Visible = false;

            UpdateButtonsState();
        }

        private void SetupEventHandlers()
        {
            _loggingService.LogAdded += LoggingService_LogAdded;

            btnDetect.Click += BtnDetect_Click!;
            btnUninstall.Click += BtnUninstall_Click!;
            btnCleanup.Click += BtnCleanup_Click!;
            btnTestMode.Click += BtnTestMode_Click!;
            btnCancel.Click += BtnCancel_Click!;
            btnRestore.Click += BtnRestore_Click!;

            btnCopyOutput.Click += BtnCopyOutput_Click!;
            btnAbrirLog.Click += BtnAbrirLog_Click!;
            btnGenerarScript.Click += BtnGenerarScript_Click!;

            lstInstallations.SelectedIndexChanged += LstInstallations_SelectedIndexChanged!;
            contextMenuDataGrid.Opening += ContextMenuDataGrid_Opening;
            menuItemCopyRow.Click += MenuItemCopyRow_Click!;
            menuItemCopyTable.Click += MenuItemCopyTable_Click!;

            this.Load += MainForm_Load_UpdateLayout!;
            this.Resize += MainForm_Resize_UpdateLayout!;
        }
        
        private void ContextMenuDataGrid_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasSelection = lstInstallations.SelectedItems.Count > 0;
            menuItemCopyRow.Enabled = hasSelection;
            menuItemCopyTable.Enabled = lstInstallations.Items.Count > 0;
        }


        private void SetupTooltips()
        {
            toolTip.SetToolTip(btnDetect, "Detectar instalaciones de Photoshop en el sistema");
            toolTip.SetToolTip(btnUninstall, "Desinstalar la instalación principal seleccionada");
            toolTip.SetToolTip(btnCleanup, "Limpiar residuos de Photoshop para la instalación seleccionada");
            toolTip.SetToolTip(btnTestMode, "Ejecutar operaciones en modo de prueba sin realizar cambios reales");
            toolTip.SetToolTip(btnCancel, "Cancelar la operación en curso");
            toolTip.SetToolTip(btnRestore, "Restaurar copias de seguridad");
            toolTip.SetToolTip(btnCopyOutput, "Copiar el contenido de la consola al portapapeles");
            toolTip.SetToolTip(btnAbrirLog, "Abrir la carpeta de logs de esta aplicación");
            toolTip.SetToolTip(btnGenerarScript, "Generar script de limpieza basado en los comandos de la consola");
        }

        #region Lógica de Privilegios
        private bool IsElevated()
        {
            // DECISIÓN: Usar directamente AdminHelper.IsRunningAsAdmin() para consistencia.
            return AdminHelper.IsRunningAsAdmin();
        }

        private bool RequestElevation(string arguments = "")
        {
            _loggingService.LogInfo("Solicitando elevación de privilegios...");
            if (!AdminHelper.RestartAsAdmin(arguments))
            {
                 CustomMsgBox.Show(
                    prompt: "No se pudo reiniciar la aplicación con permisos de administrador. Intente ejecutar la aplicación manualmente como administrador.",
                    title: "Error de Elevación",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error,
                    theme: ThemeSettings.DarkTheme);
                return false;
            }
            // Application.Exit() es llamado por AdminHelper.RestartAsAdmin si tiene éxito.
            return true; // Aunque este punto no se alcanzará si RestartAsAdmin() tiene éxito.
        }
        #endregion

        #region Manejo de Operaciones Asíncronas
        private async Task<T?> RunOperationAsync<T>(
            Func<IProgress<Core.Models.ProgressInfo>, CancellationToken, Task<T>> operation,
            string operationName,
            bool requiresElevation = false) where T : class // Restricción para que T sea tipo de referencia para default(T)! -> null
        {
            if (requiresElevation && !IsElevated())
            {
                if (!_developmentMode)
                {
                     _loggingService.LogWarning($"La operación '{operationName}' requiere elevación. Intentando reiniciar.");
                    if (RequestElevation("--elevated")) // Pasar argumento para que la nueva instancia sepa que fue elevada
                    {
                        // Si RequestElevation tiene éxito, la aplicación actual se cierra.
                        // Devolvemos null (o default para tipos de valor) para indicar que la operación no se completó en esta instancia.
                        return default; 
                    }
                    else
                    {
                        _loggingService.LogError($"No se pudieron obtener privilegios de administrador para '{operationName}'. Operación cancelada.");
                        CustomMsgBox.Show(
                            prompt: $"La operación '{operationName}' requiere privilegios de administrador. La operación ha sido cancelada.",
                            title: "Privilegios insuficientes",
                            buttons: CustomMessageBoxButtons.OK,
                            icon: CustomMessageBoxIcon.Error,
                            theme: ThemeSettings.DarkTheme);
                        return default;
                    }
                }
                else
                {
                    _loggingService.LogWarning($"MODO DESARROLLO: Ejecutando '{operationName}' sin elevación real, puede fallar.");
                }
            }

            PrepareUIForOperation(operationName);
            _cancellationTokenSource = new CancellationTokenSource();
            
            // DECISIÓN: Usar Core.Models.ProgressInfo directamente.
            // La UI puede manejar este modelo. La conversión añade una capa innecesaria si los modelos son idénticos.
            var progressReporter = new Progress<Core.Models.ProgressInfo>(info => {
                UpdateProgress(info.ProgressPercentage, info.StatusMessage);
                // Loguear mensajes de estado del progreso también es útil
                if (!string.IsNullOrWhiteSpace(info.StatusMessage) && info.ProgressPercentage < 100)
                {
                     // Evitar loguear el mensaje final de "Completado" o "Error" dos veces
                    _loggingService.LogDebug($"Progreso ({operationName}): {info.ProgressPercentage}% - {info.StatusMessage}");
                }
            });

            try
            {
                _loggingService.LogInfo($"Iniciando operación: {operationName}");
                T? result = await operation(progressReporter, _cancellationTokenSource.Token);
                
                if (result is OperationResult opResult && opResult.IsCanceled) {
                     _loggingService.LogWarning($"Operación '{operationName}' cancelada por el usuario (detectado por resultado).");
                } else {
                    _loggingService.LogInfo($"Operación '{operationName}' finalizada (revisar resultado para éxito/fallo).");
                }
                return result;
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogWarning($"Operación '{operationName}' cancelada por el usuario.");
                // No es necesario re-lanzar si el resultado de la operación (T) ya puede indicar cancelación.
                // Si T no es OperationResult, entonces sí sería bueno re-lanzar.
                // Para este caso, la mayoría de las operaciones devuelven OperationResult.
                return default; 
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error crítico durante la operación '{operationName}': {ex.Message}\nStackTrace: {ex.StackTrace}");
                 CustomMsgBox.Show(
                    prompt: $"Se produjo un error inesperado durante '{operationName}':\n{ex.Message}",
                    title: "Error Crítico",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.CriticalError,
                    theme: ThemeSettings.DarkTheme);
                return default;
            }
            finally
            {
                RestoreUI();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
        #endregion

        #region Manejadores de Eventos de Botones Principales
        private async void BtnDetect_Click(object sender, EventArgs e)
        {
            var installationsResult = await RunOperationAsync(
                (progress, token) => _detectionService.DetectInstallationsAsync(progress, token),
                "Detectando Instalaciones",
                requiresElevation: true // La detección podría necesitar acceso a HKLM o Program Files
            );

            if (installationsResult != null)
            {
                _detectedInstallations = installationsResult;
                UpdateInstallationsList();
                if (installationsResult.Count == 0)
                {
                    _loggingService.LogInfo("No se encontraron instalaciones de Photoshop.");
                }
                else
                {
                    _loggingService.LogInfo($"Detección completada. Se encontraron {installationsResult.Count} elementos.");
                }
            }
            else
            {
                 _loggingService.LogWarning("La detección de instalaciones no devolvió resultados (posiblemente cancelada o error previo).");
            }
            UpdateButtonsState();
        }

        private async void BtnUninstall_Click(object sender, EventArgs e)
        {
            if (lstInstallations.SelectedItems.Count == 0 || !(lstInstallations.SelectedItems[0].Tag is PhotoshopInstallation selectedInstallation))
            {
                CustomMsgBox.Show("Por favor, seleccione una instalación de Photoshop para desinstalar.", "Selección Requerida", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                return;
            }

            if (!selectedInstallation.IsMainInstallation && selectedInstallation.InstallationType != InstallationType.PossibleMainInstallation)
            {
                 CustomMsgBox.Show("Solo se pueden desinstalar instalaciones principales o posibles instalaciones principales.", "Operación no válida", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Information, theme: ThemeSettings.DarkTheme);
                return;
            }
            
            using (var optionsForm = new UninstallOptionsForm())
            {
                if (optionsForm.ShowDialog(this) != DialogResult.OK)
                {
                    _loggingService.LogInfo("Desinstalación cancelada por el usuario en el formulario de opciones.");
                    return;
                }

                var confirmResult = CustomMsgBox.Show(
                    $"¿Está seguro de que desea desinstalar '{selectedInstallation.DisplayName}'?\nEsta acción no se puede deshacer.",
                    "Confirmar Desinstalación",
                    CustomMessageBoxButtons.YesNo,
                    CustomMessageBoxIcon.Question,
                    theme: ThemeSettings.DarkTheme);

                if (confirmResult != CustomDialogResult.Yes)
                {
                    _loggingService.LogInfo("Desinstalación cancelada por el usuario en la confirmación.");
                    return;
                }

                var uninstallOpResult = await RunOperationAsync(
                    (progress, token) => _uninstallService.UninstallAsync(
                        selectedInstallation,
                        optionsForm.CreateBackup,
                        optionsForm.WhatIfMode,
                        optionsForm.RemoveUserData,
                        optionsForm.RemoveSharedComponents,
                        progress,
                        token),
                    $"Desinstalando {selectedInstallation.DisplayName}",
                    requiresElevation: true
                );

                if (uninstallOpResult != null)
                {
                    if (uninstallOpResult.Success)
                    {
                        _loggingService.LogInfo($"Desinstalación de '{selectedInstallation.DisplayName}' completada: {uninstallOpResult.Message}");
                        if (!optionsForm.WhatIfMode)
                        {
                            _loggingService.LogInfo("Refrescando lista de instalaciones después de la desinstalación...");
                            await Task.Delay(1000); // Pequeña pausa antes de volver a detectar
                            BtnDetect_Click(sender, e); // Re-detectar
                        }
                    }
                    else
                    {
                        _loggingService.LogError($"Error durante la desinstalación de '{selectedInstallation.DisplayName}': {uninstallOpResult.ErrorMessage} - {uninstallOpResult.Message}");
                    }
                }
                else
                {
                     _loggingService.LogWarning($"La operación de desinstalación para '{selectedInstallation.DisplayName}' no devolvió un resultado.");
                }
            }
            UpdateButtonsState();
        }

        private async void BtnCleanup_Click(object sender, EventArgs e)
        {
            if (lstInstallations.SelectedItems.Count == 0 || !(lstInstallations.SelectedItems[0].Tag is PhotoshopInstallation selectedInstallation))
            {
                CustomMsgBox.Show("Por favor, seleccione un elemento residual de Photoshop para limpiar.", "Selección Requerida", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                return;
            }
             if (!selectedInstallation.IsResidual)
            {
                 CustomMsgBox.Show("Solo se pueden limpiar elementos marcados como residuos.", "Operación no válida", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Information, theme: ThemeSettings.DarkTheme);
                return;
            }

            using (var optionsForm = new CleanupOptionsForm())
            {
                if (optionsForm.ShowDialog(this) != DialogResult.OK)
                {
                     _loggingService.LogInfo("Limpieza cancelada por el usuario en el formulario de opciones.");
                    return;
                }

                var confirmResult = CustomMsgBox.Show(
                    $"¿Está seguro de que desea limpiar los residuos asociados a '{selectedInstallation.DisplayName}'?\nEsta acción eliminará archivos y entradas de registro.",
                    "Confirmar Limpieza de Residuos",
                    CustomMessageBoxButtons.YesNo,
                    CustomMessageBoxIcon.Question,
                    theme: ThemeSettings.DarkTheme);

                if (confirmResult != CustomDialogResult.Yes)
                {
                    _loggingService.LogInfo("Limpieza cancelada por el usuario en la confirmación.");
                    return;
                }

                var cleanupOpResult = await RunOperationAsync(
                    (progress, token) => _cleanupService.CleanupAsync(
                        selectedInstallation,
                        optionsForm.CreateBackup,
                        optionsForm.WhatIfMode,
                        optionsForm.CleanupTempFiles,
                        optionsForm.CleanupRegistry,
                        optionsForm.CleanupConfigFiles,
                        optionsForm.CleanupCacheFiles,
                        progress,
                        token),
                    $"Limpiando Residuos de {selectedInstallation.DisplayName}",
                    requiresElevation: true
                );
                 if (cleanupOpResult != null)
                {
                    if (cleanupOpResult.Success)
                    {
                        _loggingService.LogInfo($"Limpieza de residuos para '{selectedInstallation.DisplayName}' completada: {cleanupOpResult.Message}");
                         if (!optionsForm.WhatIfMode)
                        {
                            _loggingService.LogInfo("Refrescando lista de instalaciones después de la limpieza...");
                            await Task.Delay(1000); 
                            BtnDetect_Click(sender, e); 
                        }
                    }
                    else
                    {
                        _loggingService.LogError($"Error durante la limpieza de residuos para '{selectedInstallation.DisplayName}': {cleanupOpResult.ErrorMessage} - {cleanupOpResult.Message}");
                    }
                }
                 else
                {
                     _loggingService.LogWarning($"La operación de limpieza para '{selectedInstallation.DisplayName}' no devolvió un resultado.");
                }
            }
            UpdateButtonsState();
        }
        
        private async void BtnTestMode_Click(object sender, EventArgs e)
        {
            _loggingService.LogInfo("Ingresando a Opciones de Modo de Prueba...");
            using (var form = new TestModeOptionsForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _loggingService.LogInfo($"Modo de prueba seleccionado: {form.SelectedOperation}");
                    PhotoshopInstallation? selectedInstallation = null;
                    if (lstInstallations.SelectedItems.Count > 0 && lstInstallations.SelectedItems[0].Tag is PhotoshopInstallation inst)
                    {
                        selectedInstallation = inst;
                    }

                    switch (form.SelectedOperation)
                    {
                        case TestModeOperation.DetectOnly:
                            BtnDetect_Click(sender, e); // Ya maneja su propia lógica de RunOperationAsync
                            break;

                        case TestModeOperation.SimulateUninstall:
                            if (selectedInstallation == null)
                            {
                                CustomMsgBox.Show("Seleccione una instalación para simular la desinstalación.", "Selección Requerida", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                                return;
                            }
                            using (var uninstallOptsForm = new UninstallOptionsForm())
                            {
                                // Forzar modo simulación y deshabilitar checkbox
                                if (uninstallOptsForm.ShowDialog(this) == DialogResult.OK)
                                {
                                    await RunOperationAsync(
                                        (progress, token) => _uninstallService.UninstallAsync(
                                            selectedInstallation,
                                            uninstallOptsForm.CreateBackup,
                                            true, // Forzar WhatIf
                                            uninstallOptsForm.RemoveUserData,
                                            uninstallOptsForm.RemoveSharedComponents,
                                            progress,
                                            token),
                                        $"SIMULANDO Desinstalación de {selectedInstallation.DisplayName}",
                                        requiresElevation: true // Simulación de operación elevada
                                    );
                                }
                            }
                            break;

                        case TestModeOperation.SimulateCleanup:
                             if (selectedInstallation == null || !selectedInstallation.IsResidual)
                            {
                                CustomMsgBox.Show("Seleccione un elemento residual para simular la limpieza.", "Selección Requerida", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                                return;
                            }
                            using (var cleanupOptsForm = new CleanupOptionsForm())
                            {
                                // Forzar modo simulación y deshabilitar checkbox
                                if (cleanupOptsForm.ShowDialog(this) == DialogResult.OK)
                                {
                                    await RunOperationAsync(
                                        (progress, token) => _cleanupService.CleanupAsync(
                                            selectedInstallation,
                                            cleanupOptsForm.CreateBackup,
                                            true, // Forzar WhatIf
                                            cleanupOptsForm.CleanupTempFiles,
                                            cleanupOptsForm.CleanupRegistry,
                                            cleanupOptsForm.CleanupConfigFiles,
                                            cleanupOptsForm.CleanupCacheFiles,
                                            progress,
                                            token),
                                        $"SIMULANDO Limpieza de {selectedInstallation.DisplayName}",
                                        requiresElevation: true // Simulación de operación elevada
                                    );
                                }
                            }
                            break;
                    }
                }
                else
                {
                    _loggingService.LogInfo("Configuración de modo de prueba cancelada.");
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _loggingService.LogWarning("Solicitando cancelación de operación...");
                _cancellationTokenSource.Cancel();
            }
            else
            {
                _loggingService.LogInfo("No hay operación en curso para cancelar.");
            }
        }

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
             _loggingService.LogInfo("Iniciando proceso de restauración de backup...");
            using (var restoreForm = new RestoreBackupForm()) // RestoreBackupForm maneja su propia lógica de elevación si es necesario.
            {
                if (restoreForm.ShowDialog(this) == DialogResult.OK)
                {
                    string backupIdToRestore = Path.GetFileName(restoreForm.SelectedBackupPath); // Suponiendo que el ID es el nombre de la carpeta/archivo sin extensión. Ajustar si es necesario.
                    if (string.IsNullOrEmpty(backupIdToRestore))
                    {
                        _loggingService.LogError("No se seleccionó un ID de backup válido para restaurar.");
                        return;
                    }
                    
                    _loggingService.LogInfo($"ID de backup seleccionado para restaurar: {backupIdToRestore}");

                    var restoreOpResult = await RunOperationAsync(
                        (progress, token) => _backupService.RestoreBackupAsync(backupIdToRestore, progress, token),
                        $"Restaurando Backup {backupIdToRestore}",
                        requiresElevation: true
                    );
                     if (restoreOpResult != null)
                    {
                        if (restoreOpResult.Success)
                        {
                            _loggingService.LogInfo($"Restauración del backup '{backupIdToRestore}' completada: {restoreOpResult.Message}");
                            _loggingService.LogInfo("Refrescando lista de instalaciones después de la restauración...");
                            await Task.Delay(1000); 
                            BtnDetect_Click(sender, e);
                        }
                        else
                        {
                            _loggingService.LogError($"Error durante la restauración del backup '{backupIdToRestore}': {restoreOpResult.ErrorMessage} - {restoreOpResult.Message}");
                        }
                    }
                     else
                    {
                         _loggingService.LogWarning($"La operación de restauración para el backup '{backupIdToRestore}' no devolvió un resultado.");
                     }
                }
                else
                {
                    _loggingService.LogInfo("Restauración de backup cancelada por el usuario.");
                }
            }
        }
        #endregion

        #region Manejadores de Eventos de Botones de Consola
        private void BtnCopyOutput_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtConsole.Text))
                {
                    Clipboard.SetText(txtConsole.Text);
                    CustomMsgBox.Show("El contenido de la consola ha sido copiado al portapapeles.", "Copiado", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Information, theme: ThemeSettings.DarkTheme);
                    _loggingService.LogInfo("Contenido de la consola copiado al portapapeles.");
                }
                else
                {
                    CustomMsgBox.Show("La consola está vacía.", "Información", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Information, theme: ThemeSettings.DarkTheme);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al copiar contenido de la consola: {ex.Message}");
                CustomMsgBox.Show($"Error al copiar al portapapeles: {ex.Message}", "Error", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Error, theme: ThemeSettings.DarkTheme);
            }
        }

        private void BtnAbrirLog_Click(object sender, EventArgs e)
        {
            try
            {
                // DECISIÓN: La ruta de logs de la aplicación es más relevante que la de Photoshop para este botón.
                // Basado en ManualDesarrollo/08_Formatos_Salida.md
                string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesinstalaPhotoshop", "Logs");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                    _loggingService.LogInfo($"Directorio de logs creado en: {logDirectory}");
                }

                Process.Start("explorer.exe", logDirectory);
                _loggingService.LogInfo($"Abriendo directorio de logs: {logDirectory}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al abrir carpeta de logs: {ex.Message}");
                CustomMsgBox.Show($"Error al abrir la carpeta de logs: {ex.Message}", "Error", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Error, theme: ThemeSettings.DarkTheme);
            }
        }

        private void BtnGenerarScript_Click(object sender, EventArgs e)
        {
            try
            {
                _loggingService.LogInfo("Iniciando generación de script de limpieza...");
                var regDeleteCommands = _scriptGenerator.ExtractRegDeleteCommands(txtConsole.Text);

                if (regDeleteCommands.Count == 0)
                {
                    CustomMsgBox.Show("No se encontraron comandos 'reg delete' en la consola para generar un script.", "Sin Comandos", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Information, theme: ThemeSettings.DarkTheme);
                    _loggingService.LogInfo("No se encontraron comandos 'reg delete' para el script.");
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Archivo por lotes (*.bat)|*.bat|Script de PowerShell (*.ps1)|*.ps1";
                    saveDialog.Title = "Guardar Script de Limpieza de Registro";
                    saveDialog.FileName = $"LimpiezaPhotoshop_Registro_{DateTime.Now:yyyyMMdd_HHmmss}";
                    saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (saveDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        ScriptType scriptType = saveDialog.FileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase)
                            ? ScriptType.PS1
                            : ScriptType.BAT;

                        bool success = _scriptGenerator.GenerateCleanupScript(regDeleteCommands, saveDialog.FileName, scriptType);

                        if (success)
                        {
                            _loggingService.LogInfo($"Script de limpieza generado: {saveDialog.FileName}");
                            var openResult = CustomMsgBox.Show(
                                $"Script generado correctamente en:\n{saveDialog.FileName}\n\n¿Desea abrir el archivo ahora? (Recuerde ejecutarlo como administrador si es necesario).",
                                "Script Generado",
                                CustomMessageBoxButtons.YesNo,
                                CustomMessageBoxIcon.Success,
                                theme: ThemeSettings.DarkTheme);

                            if (openResult == CustomDialogResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                            }
                        }
                        else
                        {
                            _loggingService.LogError("Error al generar el script de limpieza.");
                             CustomMsgBox.Show("Error al generar el script de limpieza. Revise los logs para más detalles.", "Error de Generación", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Error, theme: ThemeSettings.DarkTheme);
                        }
                    }
                    else
                    {
                        _loggingService.LogInfo("Generación de script cancelada por el usuario.");
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error crítico al generar script: {ex.Message}");
                 CustomMsgBox.Show($"Error al generar script: {ex.Message}", "Error", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Error, theme: ThemeSettings.DarkTheme);
            }
        }
        #endregion

        #region Actualización de UI
         private void LstInstallations_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonsState();
        }
        
        private void UpdateButtonsState()
        {
            bool isOperationRunning = _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;

            btnDetect.Enabled = !isOperationRunning;
            btnTestMode.Enabled = !isOperationRunning;
            btnCancel.Enabled = isOperationRunning;

            bool hasSelection = lstInstallations.SelectedItems.Count > 0;
            PhotoshopInstallation? selectedInstallation = null;
            if (hasSelection && lstInstallations.SelectedItems[0].Tag is PhotoshopInstallation inst)
            {
                selectedInstallation = inst;
            }

            bool canUninstallSelected = selectedInstallation != null && (selectedInstallation.IsMainInstallation || selectedInstallation.InstallationType == InstallationType.PossibleMainInstallation);
            btnUninstall.Enabled = !isOperationRunning && canUninstallSelected;

            bool hasAnyMainOrPossible = _detectedInstallations.Any(i => i.IsMainInstallation || i.InstallationType == InstallationType.PossibleMainInstallation);
            bool canCleanupSelectedResidual = selectedInstallation != null && selectedInstallation.IsResidual;
            
            // Botón "Limpiar Residuos": Habilitado si hay un residuo seleccionado Y NO hay instalación principal/posible detectada en general
            btnCleanup.Enabled = !isOperationRunning && canCleanupSelectedResidual && !hasAnyMainOrPossible;


            string backupBaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PhotoshopBackups");
            btnRestore.Enabled = !isOperationRunning && Directory.Exists(backupBaseDir) && Directory.GetDirectories(backupBaseDir).Any();


            // Tooltips
            if (btnUninstall.Enabled) toolTip.SetToolTip(btnUninstall, "Desinstalar la instalación seleccionada.");
            else if (isOperationRunning) toolTip.SetToolTip(btnUninstall, "Operación en curso...");
            else if (!hasSelection) toolTip.SetToolTip(btnUninstall, "Seleccione una instalación para desinstalar.");
            else toolTip.SetToolTip(btnUninstall, "La instalación seleccionada no es apta para desinstalación directa (puede ser un residuo).");

            if (btnCleanup.Enabled) toolTip.SetToolTip(btnCleanup, "Limpiar los residuos seleccionados.");
            else if (isOperationRunning) toolTip.SetToolTip(btnCleanup, "Operación en curso...");
            else if (hasAnyMainOrPossible) toolTip.SetToolTip(btnCleanup, "Primero debe desinstalar las instalaciones principales antes de limpiar residuos.");
            else if (!canCleanupSelectedResidual && _detectedInstallations.Any(i => i.IsResidual)) toolTip.SetToolTip(btnCleanup, "Seleccione un elemento residual de la lista para limpiar.");
            else if (!_detectedInstallations.Any(i => i.IsResidual)) toolTip.SetToolTip(btnCleanup, "No se detectaron residuos de Photoshop para limpiar.");
            else toolTip.SetToolTip(btnCleanup, "Seleccione un residuo para activar la limpieza.");

            // Botones de consola
            btnCopyOutput.Enabled = !isOperationRunning && !string.IsNullOrEmpty(txtConsole.Text);
            btnAbrirLog.Enabled = !isOperationRunning;
            btnGenerarScript.Enabled = !isOperationRunning && txtConsole.Text.Contains("reg delete", StringComparison.OrdinalIgnoreCase);
            
            UpdateButtonColors();
        }
        
        private void UpdateButtonColors()
        {
            Color enabledBackColor = Color.FromArgb(30, 40, 60);
            Color enabledForeColor = Color.White;
            Color disabledBackColor = Color.FromArgb(60, 60, 60);
            Color disabledForeColor = Color.Gray;

            Action<IconButton> setColor = (button) =>
            {
                if (button.Enabled)
                {
                    button.BackColor = enabledBackColor;
                    button.ForeColor = enabledForeColor;
                    button.IconColor = enabledForeColor;
                }
                else
                {
                    button.BackColor = disabledBackColor;
                    button.ForeColor = disabledForeColor;
                    button.IconColor = disabledForeColor;
                }
            };

            setColor(btnDetect);
            setColor(btnUninstall);
            setColor(btnCleanup);
            setColor(btnTestMode);
            setColor(btnCancel); // Cancel se verá diferente
            if (btnCancel.Enabled) { // Color especial para Cancel cuando está activo
                 btnCancel.BackColor = Color.FromArgb(192, 57, 43); // Rojo
                 btnCancel.ForeColor = Color.White;
                 btnCancel.IconColor = Color.White;
            }
            setColor(btnRestore);
            setColor(btnCopyOutput);
            setColor(btnAbrirLog);
            setColor(btnGenerarScript);
        }


        private void PrepareUIForOperation(string operationName)
        {
            _currentOperation = operationName;
            _animationDots = 0;
            _textAnimationState = 0;
            progressBar.Value = 0;

            lblProgress.Text = $"{_currentOperation} - 0%";
            lblProgress.Visible = true;
            progressBar.Visible = true;
            lblAnimatedText.Text = _animationTexts[0] + "..."; // Mostrar texto inicial de animación
            lblAnimatedText.Visible = true;
            
            UpdateButtonsState(); // Esto deshabilitará/habilitará botones según isOperationRunning
            animationTimer.Start();
            Application.DoEvents(); // Forzar actualización para que la UI refleje los cambios inmediatamente
        }

        private void RestoreUI()
        {
            animationTimer.Stop();
             // Si la operación no se completó al 100% o fue cancelada/errónea, ocultar progreso.
            // Si se completó al 100% con éxito, progressBar.Value será 100.
            if (progressBar.Value < 100 || (_cancellationTokenSource !=null && _cancellationTokenSource.IsCancellationRequested) )
            {
                lblProgress.Visible = false;
                progressBar.Visible = false;
                lblAnimatedText.Visible = false;
            } else { // Mantenemos visible el 100% completado
                 lblProgress.Text = $"{_currentOperation} - Completado (100%)";
                 lblAnimatedText.Text = "Operación finalizada.";
            }
            
            _currentOperation = string.Empty;
            UpdateButtonsState();
        }

        private void UpdateProgress(int percentage, string? statusText)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(percentage, statusText)));
                return;
            }

            progressBar.Value = Math.Clamp(percentage, 0, 100);
            string currentStatus = string.IsNullOrWhiteSpace(statusText) ? _currentOperation : statusText;
            
            // Limitar longitud para evitar desbordamiento
            if (currentStatus.Length > 50) currentStatus = currentStatus.Substring(0, 47) + "...";

            lblProgress.Text = $"{currentStatus} - {progressBar.Value}%";

            if (progressBar.Value >= 100)
            {
                animationTimer.Stop();
                lblProgress.Text = $"{currentStatus} - Completado (100%)";
                lblAnimatedText.Text = "Operación completada."; // Mensaje más genérico, ya que el resultado específico se loguea.
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (progressBar.Value >= 100) {
                animationTimer.Stop();
                return;
            }

            _animationDots = (_animationDots + 1) % 4;
            string dots = new string('.', _animationDots);
            
            _textAnimationState = (_textAnimationState + 1) % _animationTexts.Length;
            lblAnimatedText.Text = _animationTexts[_textAnimationState] + dots;
            
            // El texto de lblProgress ya se actualiza en UpdateProgress,
            // pero si quisiéramos una animación de puntos también allí:
            // string operationText = _currentOperation;
            // if (operationText.Length > 20) operationText = operationText.Substring(0, 17) + "...";
            // lblProgress.Text = $"{operationText}{dots} - {progressBar.Value}%";
        }

        private void UpdateInstallationsList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateInstallationsList));
                return;
            }

            lstInstallations.BeginUpdate(); // Optimizar rendimiento
            lstInstallations.Items.Clear();

            if (_detectedInstallations == null || _detectedInstallations.Count == 0)
            {
                _loggingService.LogInfo("No se encontraron instalaciones de Photoshop.");
                lstInstallations.EndUpdate();
                UpdateButtonsState();
                return;
            }

            foreach (var installation in _detectedInstallations)
            {
                string emoji = "❓"; // Desconocido por defecto
                string tooltipBaseText = "Tipo desconocido";

                if (installation.IsMainInstallation)
                {
                    emoji = "✅"; 
                    tooltipBaseText = "Instalación Principal de Photoshop";
                }
                else if (installation.InstallationType == InstallationType.PossibleMainInstallation)
                {
                    emoji = "⚠️"; 
                    tooltipBaseText = "Posible Instalación Principal de Photoshop";
                }
                else if (installation.IsResidual)
                {
                    emoji = "🗑️";
                    tooltipBaseText = "Residuos de Photoshop";
                }

                string displayName = installation.DisplayName;
                string additionalInfo = string.Empty;

                if (!string.IsNullOrEmpty(installation.InstallLocation))
                {
                    string folderName = Path.GetFileName(installation.InstallLocation.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    if (!string.IsNullOrEmpty(folderName)) additionalInfo = $"Carpeta: {folderName}";
                }
                else if (installation.DetectedBy == DetectionMethod.Registry || installation.AssociatedRegistryKeys.Any())
                {
                    additionalInfo = "Solo Registro";
                }
                 if (installation.IsResidual && installation.InstallLocation != null)
                {
                    if (installation.InstallLocation.Contains("AppData", StringComparison.OrdinalIgnoreCase)) additionalInfo = "Residuos en AppData";
                    else if (installation.InstallLocation.Contains("Documents", StringComparison.OrdinalIgnoreCase)) additionalInfo = "Residuos en Documents";
                    else if (installation.InstallLocation.Contains("ProgramData", StringComparison.OrdinalIgnoreCase)) additionalInfo = "Residuos en ProgramData";
                }
                if (!string.IsNullOrEmpty(additionalInfo)) displayName = $"{displayName} ({additionalInfo})";

                var item = new ListViewItem(emoji + " " + displayName);
                item.SubItems.Add(string.IsNullOrWhiteSpace(installation.Version) ? "N/A" : installation.Version);
                item.SubItems.Add(string.IsNullOrWhiteSpace(installation.InstallLocation) ? "N/A" : installation.InstallLocation);

                string confidenceWord;
                if (installation.ConfidenceScore >= 75) confidenceWord = "Muy Alta";
                else if (installation.ConfidenceScore >= 50) confidenceWord = "Alta";
                else if (installation.ConfidenceScore >= 30) confidenceWord = "Media";
                else if (installation.ConfidenceScore >= 10) confidenceWord = "Baja";
                else confidenceWord = "Muy Baja";
                item.SubItems.Add($"{installation.ConfidenceScore} ({confidenceWord})");

                // Construcción del ToolTipText
                var sbToolTip = new StringBuilder();
                sbToolTip.AppendLine($"{tooltipBaseText}");
                sbToolTip.AppendLine($"Nombre: {installation.DisplayName}");
                if (!string.IsNullOrWhiteSpace(installation.Version)) sbToolTip.AppendLine($"Versión: {installation.Version}");
                if (!string.IsNullOrWhiteSpace(installation.InstallLocation)) sbToolTip.AppendLine($"Ubicación: {installation.InstallLocation}");
                sbToolTip.AppendLine($"Confianza: {installation.ConfidenceScore} ({confidenceWord})");
                sbToolTip.AppendLine($"Detectado por: {installation.DetectedBy}");
                
                bool hasExe = !string.IsNullOrEmpty(installation.InstallLocation) && File.Exists(Path.Combine(installation.InstallLocation, "photoshop.exe"));
                sbToolTip.AppendLine($"Ejecutable Principal (photoshop.exe): {(hasExe ? "✓ Encontrado" : "✗ No Encontrado")}");
                
                bool hasUninstaller = !string.IsNullOrEmpty(installation.UninstallString) && 
                                      File.Exists(installation.UninstallString.Trim('"').Split(' ')[0]);
                sbToolTip.AppendLine($"Desinstalador: {(hasUninstaller ? $"✓ Encontrado ({installation.UninstallString})" : "✗ No Encontrado")}");

                if (installation.AssociatedFiles.Any())
                {
                    sbToolTip.AppendLine($"Archivos Asociados ({installation.AssociatedFiles.Count}):");
                    installation.AssociatedFiles.Take(3).ToList().ForEach(f => sbToolTip.AppendLine($"  - {TruncatePath(f,60)}"));
                    if (installation.AssociatedFiles.Count > 3) sbToolTip.AppendLine($"  - y {installation.AssociatedFiles.Count - 3} más...");
                }
                if (installation.AssociatedRegistryKeys.Any())
                {
                    sbToolTip.AppendLine($"Claves de Registro Asociadas ({installation.AssociatedRegistryKeys.Count}):");
                    installation.AssociatedRegistryKeys.Take(3).ToList().ForEach(k => sbToolTip.AppendLine($"  - {TruncatePath(k,60)}"));
                    if (installation.AssociatedRegistryKeys.Count > 3) sbToolTip.AppendLine($"  - y {installation.AssociatedRegistryKeys.Count - 3} más...");
                }
                if (!string.IsNullOrWhiteSpace(installation.Notes)) sbToolTip.AppendLine($"Notas: {installation.Notes}");
                item.ToolTipText = sbToolTip.ToString().TrimEnd();
                
                item.Tag = installation;
                lstInstallations.Items.Add(item);
            }

            SelectFirstMainInstallation();
            lstInstallations.EndUpdate();
            UpdateButtonsState();
        }
        
        private string TruncatePath(string path, int maxLength)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength) return path;
            return "..." + path.Substring(path.Length - (maxLength - 3));
        }


        private void SelectFirstMainInstallation()
        {
            if (lstInstallations.Items.Count == 0) return;

            ListViewItem? itemToSelect = lstInstallations.Items
                .OfType<ListViewItem>()
                .FirstOrDefault(item => item.Tag is PhotoshopInstallation inst && (inst.IsMainInstallation || inst.InstallationType == InstallationType.PossibleMainInstallation));

            if (itemToSelect == null && lstInstallations.Items.Count > 0)
            {
                itemToSelect = lstInstallations.Items[0]; // Seleccionar el primero si no hay principal/posible
            }

            if (itemToSelect != null)
            {
                itemToSelect.Selected = true;
                itemToSelect.Focused = true;
                itemToSelect.EnsureVisible();
                _loggingService.LogDebug($"Seleccionada automáticamente: {itemToSelect.Text.Trim()}");
            }
        }
        
        // Actualización del layout del panel de información
        private void MainForm_Load_UpdateLayout(object sender, EventArgs e) => UpdatePanelInfoLayout();
        private void MainForm_Resize_UpdateLayout(object sender, EventArgs e) => UpdatePanelInfoLayout();

        private void UpdatePanelInfoLayout()
        {
            if (panelConsoleButtons == null || progressBar == null || lblAnimatedText == null || lblProgress == null) return;
            if (!panelConsoleButtons.Visible || panelConsoleButtons.ClientSize.Width <= 0) return;

            int margin = 5;
            int progressBarWidth = (int)(panelConsoleButtons.ClientSize.Width * 0.35); // 35% para la barra
            progressBarWidth = Math.Max(100, progressBarWidth); // Ancho mínimo

            int animatedTextWidth = (int)(panelConsoleButtons.ClientSize.Width * 0.30); // 30% para texto animado
            animatedTextWidth = Math.Max(100, animatedTextWidth);

            int progressLabelWidth = panelConsoleButtons.ClientSize.Width - progressBarWidth - animatedTextWidth - (margin * 4);
            progressLabelWidth = Math.Max(100, progressLabelWidth); // Ancho mínimo

            // Posicionar lblAnimatedText a la izquierda
            lblAnimatedText.Location = new Point(margin, (panelConsoleButtons.ClientSize.Height - lblAnimatedText.Height) / 2);
            lblAnimatedText.Size = new Size(animatedTextWidth, lblAnimatedText.Height);

            // Posicionar lblProgress en el centro
            lblProgress.Location = new Point(lblAnimatedText.Right + margin, lblAnimatedText.Top);
            lblProgress.Size = new Size(progressLabelWidth, lblProgress.Height);

            // Posicionar progressBar a la derecha
            progressBar.Location = new Point(lblProgress.Right + margin, lblProgress.Top);
            progressBar.Size = new Size(progressBarWidth, progressBar.Height);
        }
        #endregion

        #region Métodos de Logging para UI
        private void LoggingService_LogAdded(object? sender, LogEventArgs e)
        {
            // DECISIÓN: Definir colores aquí para la UI, ya que ConsoleColor no aplica a RichTextBox.
            Color color = e.Level switch
            {
                LogLevel.Debug => Color.Gray,
                LogLevel.Info => Color.LightSkyBlue, // Un azul más claro para Info
                LogLevel.Warning => Color.Yellow,
                LogLevel.Error => Color.OrangeRed,
                LogLevel.Critical => Color.Red,
                _ => Color.White
            };
            AppendToConsoleUI(e.Message, color, e.Timestamp, e.Level);
        }

        private void AppendToConsoleUI(string message, Color color, DateTime timestamp, LogLevel level)
        {
            if (txtConsole.InvokeRequired)
            {
                txtConsole.Invoke(new Action(() => AppendToConsoleUI(message, color, timestamp, level)));
                return;
            }
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.SelectionLength = 0;
            txtConsole.SelectionColor = color;
            txtConsole.AppendText($"[{timestamp:HH:mm:ss}] [{level.ToString().ToUpper()}] {message}{Environment.NewLine}");
            txtConsole.SelectionColor = txtConsole.ForeColor; // Reset color
            txtConsole.ScrollToCaret();
        }
        #endregion
        
        #region Menú Contextual DataGrid
        private void MenuItemCopyRow_Click(object sender, EventArgs e)
        {
            if (lstInstallations.SelectedItems.Count == 0) return;
            ListViewItem selectedItem = lstInstallations.SelectedItems[0];
            
            var sb = new StringBuilder();
            for (int i = 0; i < lstInstallations.Columns.Count; i++)
            {
                sb.Append(lstInstallations.Columns[i].Text);
                if (i < lstInstallations.Columns.Count - 1) sb.Append("\t");
            }
            sb.AppendLine();
            for (int i = 0; i < selectedItem.SubItems.Count; i++)
            {
                sb.Append(selectedItem.SubItems[i].Text);
                if (i < selectedItem.SubItems.Count - 1) sb.Append("\t");
            }
            
            // Añadir ToolTipText
            if (!string.IsNullOrWhiteSpace(selectedItem.ToolTipText))
            {
                sb.AppendLine();
                sb.AppendLine("--- Información Detallada ---");
                sb.Append(selectedItem.ToolTipText);
            }

            try
            {
                Clipboard.SetText(sb.ToString());
                _loggingService.LogInfo("Fila seleccionada (con detalles) copiada al portapapeles.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al copiar fila: {ex.Message}");
            }
        }

        private void MenuItemCopyTable_Click(object sender, EventArgs e)
        {
            if (lstInstallations.Items.Count == 0) return;
            var sb = new StringBuilder();

            // Encabezados
            for (int i = 0; i < lstInstallations.Columns.Count; i++)
            {
                sb.Append(lstInstallations.Columns[i].Text);
                if (i < lstInstallations.Columns.Count - 1) sb.Append("\t");
            }
            sb.AppendLine();

            // Filas
            foreach (ListViewItem item in lstInstallations.Items)
            {
                for (int i = 0; i < item.SubItems.Count; i++)
                {
                    sb.Append(item.SubItems[i].Text);
                    if (i < item.SubItems.Count - 1) sb.Append("\t");
                }
                sb.AppendLine();
            }
             try
            {
                Clipboard.SetText(sb.ToString());
                _loggingService.LogInfo($"Tabla completa ({lstInstallations.Items.Count} filas) copiada al portapapeles.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al copiar tabla: {ex.Message}");
            }
        }
        #endregion

    }
}