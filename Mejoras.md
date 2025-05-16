## Plan Detallado para la Mejora de Solicitud de Privilegios

### **Objetivo General:**
Modificar el flujo de inicio y la interacción con el botón "Detectar" para que la solicitud de privilegios elevados sea explícita, informativa y menos disruptiva para el usuario.

---

### **I. Cambios en `DesinstalaPhotoshop.UI/Program.cs`**

**¿Qué y Por qué?**
Necesitamos una forma de que `MainForm` sepa si se ha reiniciado específicamente para realizar la detección después de que el usuario concediera privilegios. Esto se logrará pasando un argumento de línea de comandos distintivo.

**¿Cómo y Dónde?**
Modificar el método `Main`:

```csharp
// File: DesinstalaPhotoshop.UI/Program.cs
namespace DesinstalaPhotoshop.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) // Modificar para aceptar args
        {
            ApplicationConfiguration.Initialize();
            Application.SetColorMode(SystemColorMode.Dark);

            // Verificar si la aplicación se inició con el argumento --elevated (genérico)
            // o --elevated-for-detection (específico)
            bool isElevatedForGeneral = args.Contains("--elevated");
            bool isElevatedForDetection = args.Contains("--elevated-for-detection");

            // Pasar ambos flags al MainForm.
            // isElevated será true si cualquiera de los dos flags de elevación está presente.
            // justElevatedForDetection será true solo si el flag específico está presente.
            Application.Run(new MainForm(isElevatedForGeneral || isElevatedForDetection, isElevatedForDetection));
        }
    }
}
```

---

### **II. Cambios en `DesinstalaPhotoshop.UI/MainForm.cs`**

**1. Nuevos Campos de Estado y Constructor Modificado**

**¿Qué y Por qué?**
`MainForm` necesita rastrear su estado actual de privilegios y si acaba de ser elevado específicamente para la detección.

**¿Cómo y Dónde?**
Añadir/modificar campos y el constructor:

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
public partial class MainForm : Form
{
    // ... (servicios existentes) ...

    // Reemplazar _startedElevated o añadir nuevos para mayor claridad:
    // private readonly bool _startedElevated = false; // Comentar o eliminar si se reemplaza
    private readonly bool _isCurrentlyAdmin = false;      // True si la instancia actual tiene privilegios de admin
    private readonly bool _justElevatedForDetection = false; // True si esta instancia se acaba de reiniciar específicamente para detección

    // ... (resto de campos existentes) ...

    // Constructor modificado:
    public MainForm(bool startedElevated = false, bool justElevatedForDetection = false)
    {
        InitializeComponent();
        // _startedElevated = startedElevated; // Ya no es necesario de esta forma

        // Determinar el estado real de administrador
        _isCurrentlyAdmin = AdminHelper.IsRunningAsAdmin();

        // _justElevatedForDetection solo es true si el argumento específico estaba presente Y AHORA es admin.
        _justElevatedForDetection = justElevatedForDetection && _isCurrentlyAdmin;

        // Inicialización de servicios (como ya está)
        _loggingService = new LoggingService();
        _fileSystemHelper = new Core.Services.Helpers.FileSystemHelper(_loggingService);
        // ... (resto de servicios) ...

        SetupControls(); // Mover la lógica de configuración de controles iniciales a SetupControls
        SetupEventHandlers();

        _loggingService.LogInfo($"Aplicación iniciada. Admin: {_isCurrentlyAdmin}. Elevada para detección: {_justElevatedForDetection}.");
        if (_developmentMode)
        {
            _loggingService.LogWarning("MODO DESARROLLO ACTIVO: Las solicitudes de elevación pueden ser omitidas.");
        }
    }
    // ...
}
```

**2. Lógica de Configuración de Controles Iniciales (en `SetupControls`)**

**¿Qué y Por qué?**
El botón `btnDetect` (que ahora podría llamarse "Privilegios") y la barra de título deben reflejar el estado de privilegios al inicio. Si se acaba de elevar para detección, esta se iniciará automáticamente.

**¿Cómo y Dónde?**
Actualizar/añadir en el método `SetupControls()` (o si parte de esta lógica estaba en el constructor, moverla aquí):

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
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

    this.StartPosition = FormStartPosition.CenterScreen;
    this.MinimumSize = new Size(1000, 630);
    this.BackColor = Color.FromArgb(20, 30, 45);

    // Lógica de botón y título basada en privilegios
    if (_isCurrentlyAdmin)
    {
        this.Text = "DesinstalaPhotoshop (Administrador)";
        btnDetect.Text = "  Detectar";
        btnDetect.IconChar = FontAwesome.Sharp.IconChar.Search;
        toolTip.SetToolTip(btnDetect, "Detectar instalaciones de Photoshop en el sistema");

        if (_justElevatedForDetection)
        {
            _loggingService.LogInfo("Privilegios concedidos para detección. Iniciando detección automáticamente...");
            // Llamar a la lógica de detección inmediatamente.
            // Para esto, es mejor refactorizar la lógica de BtnDetect_Click.
            TriggerDetectionProcess();
        }
    }
    else
    {
        this.Text = "DesinstalaPhotoshop";
        btnDetect.Text = "  Privilegios";
        btnDetect.IconChar = FontAwesome.Sharp.IconChar.ShieldAlt; // O el icono elegido
        toolTip.SetToolTip(btnDetect, "Solicitar privilegios de administrador para funciones completas");
    }

    // SetupTooltips(); // Asegurarse que se llama después de establecer el texto/icono de btnDetect
    animationTimer.Tick += AnimationTimer_Tick!;
    
    lblProgress.Visible = false;
    progressBar.Visible = false;
    lblAnimatedText.Visible = false;

    UpdateButtonsState(); // Aplicar estados/colores iniciales
}

// Nuevo método para la lógica de detección que puede ser llamado desde varios sitios
private async void TriggerDetectionProcess()
{
    // Si el botón dice "Privilegios", no hacer nada aquí (se maneja en el click).
    // Esta función es para cuando *ya* se tienen privilegios.
    if (btnDetect.Text.Contains("Privilegios")) return;

    var installationsResult = await RunOperationAsync(
        (progress, token) => _detectionService.DetectInstallationsAsync(progress, token),
        "Detectando Instalaciones"
        // Ya no se necesita 'requiresElevation: true' en esta llamada a RunOperationAsync,
        // porque se asume que si llegamos aquí, ya somos admin o estamos en modo desarrollo.
        // La verificación de elevación se hará *antes* de llamar a esta función si es necesario.
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
```

**3. Modificar el Manejador de Eventos `BtnDetect_Click`**

**¿Qué y Por qué?**
Este método ahora tiene una doble responsabilidad:
1.  Si la aplicación no tiene privilegios, y el botón dice "Privilegios", su clic mostrará el `CustomMsgBox` para solicitar la elevación.
2.  Si la aplicación ya tiene privilegios (y el botón dice "Detectar"), su clic iniciará la detección.

**¿Cómo y Dónde?**

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private async void BtnDetect_Click(object sender, EventArgs e)
{
    if (!_isCurrentlyAdmin) // Si no somos admin, el botón dirá "Privilegios"
    {
        _loggingService.LogInfo("Botón 'Privilegios' presionado. Solicitando confirmación para elevación.");
        var result = CustomMsgBox.Show(
            prompt: "DesinstalaPhotoshop necesita privilegios de administrador para:\n\n" +
                    "  • Detectar completamente todas las instalaciones y residuos.\n" +
                    "  • Realizar operaciones de desinstalación y limpieza.\n\n" +
                    "Al conceder privilegios, la aplicación se reiniciará.\n\n" +
                    "¿Desea continuar y otorgar privilegios de administrador?",
            title: "Solicitud de Privilegios Elevados",
            buttons: CustomMessageBoxButtons.YesNo,
            icon: CustomMessageBoxIcon.Shield, // O Question, Information
            theme: ThemeSettings.DarkTheme
        );

        if (result == CustomDialogResult.Yes)
        {
            _loggingService.LogInfo("Usuario aceptó la elevación de privilegios. Reiniciando como administrador para detección...");
            // Usar el argumento específico
            if (AdminHelper.RestartAsAdmin("--elevated-for-detection"))
            {
                // La aplicación actual se cerrará por Environment.Exit(0) en AdminHelper.
            }
            else
            {
                _loggingService.LogError("No se pudo reiniciar la aplicación con privilegios de administrador.");
                CustomMsgBox.Show(
                    prompt: "No se pudo reiniciar la aplicación con permisos de administrador. Por favor, intente ejecutar la aplicación manualmente como administrador.",
                    title: "Error de Elevación",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error,
                    theme: ThemeSettings.DarkTheme
                );
            }
        }
        else
        {
            _loggingService.LogInfo("Usuario denegó la elevación de privilegios.");
            CustomMsgBox.Show(
                prompt: "No se concedieron privilegios de administrador. La funcionalidad de detección y otras operaciones críticas estarán limitadas o no disponibles.",
                title: "Privilegios Denegados",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Warning,
                theme: ThemeSettings.DarkTheme
            );
        }
    }
    else // Ya es admin, el botón dice "Detectar"
    {
        _loggingService.LogInfo("Botón 'Detectar' presionado (con privilegios).");
        TriggerDetectionProcess(); // Llamar al método refactorizado
    }
}
```

**4. Actualizar el Tooltip del Botón en `SetupTooltips()`**

**¿Qué y Por qué?**
El tooltip debe ser dinámico o establecerse correctamente según el estado inicial. `SetupControls` ya lo maneja.

**¿Cómo y Dónde?**
Asegurarse de que en `SetupControls()`, después de cambiar el texto y el icono de `btnDetect`, se llama a `toolTip.SetToolTip(btnDetect, "...");` con el mensaje apropiado. Esto ya está cubierto por la lógica de `SetupControls` propuesta anteriormente.

**5. Modificar `RunOperationAsync` para que no solicite elevación si ya se es admin**

**¿Qué y Por qué?**
El método `RunOperationAsync` actualmente tiene una lógica para solicitar elevación si `requiresElevation` es `true` y no se es admin. Con el nuevo flujo, si `btnDetect` dice "Detectar", ya deberíamos ser admin, por lo que `RunOperationAsync` no debería intentar elevar de nuevo para la detección. Para otras operaciones (Uninstall, Cleanup), la verificación de admin se hará antes de llamar a `RunOperationAsync`.

**¿Cómo y Dónde?**

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private async Task<T?> RunOperationAsync<T>(
    Func<IProgress<Core.Models.ProgressInfo>, CancellationToken, Task<T>> operation,
    string operationName,
    bool requiresElevation = false) where T : class
{
    // La lógica de elevación ahora es más explícita ANTES de llamar a RunOperationAsync
    // para operaciones como Uninstall y Cleanup.
    // Para Detect, si llegamos aquí, ya deberíamos tener los permisos o es modo desarrollo.
    if (requiresElevation && !_isCurrentlyAdmin && !_developmentMode)
    {
        // Este bloque ahora es más un seguro o para operaciones que SÍ deben verificar
        // y no han pasado por el flujo de BtnDetect_Click
        _loggingService.LogWarning($"La operación '{operationName}' requiere elevación pero no se tienen privilegios. Esto no debería ocurrir si el flujo es correcto.");
        CustomMsgBox.Show(
            prompt: $"La operación '{operationName}' requiere privilegios de administrador. La operación ha sido cancelada. Por favor, use el botón 'Privilegios' primero.",
            title: "Privilegios insuficientes",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error,
            theme: ThemeSettings.DarkTheme);
        return default;
    }
    // Si estamos en modo desarrollo y requiresElevation es true pero no somos admin, loguear pero continuar.
    if (requiresElevation && !_isCurrentlyAdmin && _developmentMode)
    {
        _loggingService.LogWarning($"MODO DESARROLLO: Ejecutando '{operationName}' que requiere elevación, sin elevación real. Puede fallar.");
    }


    PrepareUIForOperation(operationName);
    _cancellationTokenSource = new CancellationTokenSource();
    
    var progressReporter = new Progress<Core.Models.ProgressInfo>(info => {
        UpdateProgress(info.ProgressPercentage, info.StatusMessage);
        if (!string.IsNullOrWhiteSpace(info.StatusMessage) && info.ProgressPercentage < 100)
        {
            _loggingService.LogDebug($"Progreso ({operationName}): {info.ProgressPercentage}% - {info.StatusMessage}");
        }
    });

    try
    {
        // ... (resto del método igual)
    }
    // ... (catch y finally igual)
}
```
**Importante:** Para `BtnUninstall_Click` y `BtnCleanup_Click`, ahora la verificación de `_isCurrentlyAdmin` debe hacerse *antes* de llamar a `RunOperationAsync`. Si no es admin, se debe guiar al usuario a usar el botón "Privilegios" primero.

```csharp
// Ejemplo de modificación en BtnUninstall_Click
private async void BtnUninstall_Click(object sender, EventArgs e)
{
    if (!_isCurrentlyAdmin && !_developmentMode) // Chequeo de privilegios
    {
        CustomMsgBox.Show(
            prompt: "Esta operación requiere privilegios de administrador. Por favor, use primero el botón 'Privilegios'.",
            title: "Privilegios Requeridos",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Warning,
            theme: ThemeSettings.DarkTheme);
        return;
    }
    // ... resto de la lógica de BtnUninstall_Click,
    // la llamada a RunOperationAsync ya no necesitará 'requiresElevation: true' o será redundante.
    // Se puede mantener 'requiresElevation: true' como una doble verificación si se desea.
    var uninstallOpResult = await RunOperationAsync(
        (progress, token) => _uninstallService.UninstallAsync(
            selectedInstallation,
            optionsForm.CreateBackup,
            optionsForm.WhatIfMode, // Este es el 'whatIf' para el servicio
            optionsForm.RemoveUserData,
            optionsForm.RemoveSharedComponents,
            progress,
            token),
        $"Desinstalando {selectedInstallation.DisplayName}",
        requiresElevation: true // Opcional, ya que hemos verificado _isCurrentlyAdmin
    );
    // ...
}
// Lógica similar para BtnCleanup_Click
```

---

### **III. Revisión del Modo Prueba**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs` (método `BtnTestMode_Click`)

**¿Qué y Por qué?**
Determinar si las simulaciones realmente necesitan privilegios y ajustar el flujo si no es así.

**¿Cómo y Dónde?**

**En `DesinstalaPhotoshop.UI/MainForm.cs` (`BtnTestMode_Click`):**

1.  **`TestModeOperation.DetectOnly`:**
    *   Este caso llama a `BtnDetect_Click(sender, e);`. Con los cambios anteriores, si no se es admin, el flujo de `BtnDetect_Click` solicitará privilegios. Si ya se es admin, ejecutará la detección. Este comportamiento es consistente y probablemente correcto, ya que la detección, incluso para simulación, es mejor con acceso completo.

2.  **`TestModeOperation.SimulateUninstall` y `TestModeOperation.SimulateCleanup`:**
    *   Actualmente, las llamadas a `RunOperationAsync` para estas simulaciones tienen `requiresElevation: true`.
    *   **Análisis:** Como se mencionó antes, incluso en modo "WhatIf", los servicios `_uninstallService` y `_cleanupService` podrían intentar leer información de ubicaciones protegidas (HKLM, Program Files) para determinar qué *se haría*. Una simulación sin estos accesos podría ser incompleta o engañosa.
    *   **Decisión Recomendada:** **Mantener `requiresElevation: true`** para estas simulaciones. La explicación al usuario (si se le pide elevar para modo prueba) podría ser: "Para simular con precisión las acciones de desinstalación/limpieza, se recomienda ejecutar con privilegios de administrador. Esto permite a la aplicación identificar todos los elementos que serían afectados."
    *   El flujo sería:
        *   Usuario hace clic en `BtnTestMode`.
        *   Selecciona `SimulateUninstall` o `SimulateCleanup`.
        *   Si `!_isCurrentlyAdmin`:
            *   Mostrar `CustomMsgBox` explicando que la simulación es mejor con privilegios.
            *   Si acepta: `AdminHelper.RestartAsAdmin("--elevated");` (argumento genérico, el usuario deberá volver a seleccionar la opción de modo prueba).
            *   Si declina: `_loggingService.LogWarning("Continuando simulación sin privilegios elevados. Los resultados pueden ser incompletos.");` y proceder con la llamada a `RunOperationAsync` pero pasando `requiresElevation: false` (o ajustar `RunOperationAsync` para que no fuerce el reinicio en este escenario si `_developmentMode` no está activo). **Esta parte es la más compleja de hacer opcional sin cambiar mucho `RunOperationAsync`.**
            *   La opción más simple: si el modo prueba para Uninstall/Cleanup se considera que *necesita* elevación para ser útil, entonces el flujo actual de `RunOperationAsync` (que pide elevación si `requiresElevation` es `true` y no se es admin) es adecuado.

    *   **Alternativa Simplificada para Modo Prueba (si se quiere evitar la complejidad anterior):**
        *   Si se selecciona `SimulateUninstall` o `SimulateCleanup` y `!_isCurrentlyAdmin`:
            *   Mostrar un `CustomMsgBox` que diga: "La simulación de esta operación requiere privilegios de administrador. Por favor, use primero el botón 'Privilegios' y luego vuelva a intentar el Modo Prueba."
            *   No proceder con la simulación.
        *   Esto mantiene la lógica de `RunOperationAsync` simple y fuerza el flujo de elevación principal.

**Implementación de la Alternativa Simplificada (Recomendada por simplicidad):**

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
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
                    // Esto seguirá el nuevo flujo de BtnDetect_Click (solicitará elevación si es necesario)
                    BtnDetect_Click(sender, e);
                    break;

                case TestModeOperation.SimulateUninstall:
                    if (!_isCurrentlyAdmin && !_developmentMode)
                    {
                        CustomMsgBox.Show("La simulación de desinstalación requiere privilegios de administrador. Por favor, use primero el botón 'Privilegios'.", "Privilegios Requeridos", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                        return;
                    }
                    if (selectedInstallation == null) /* ... (manejo de no selección) ... */ return;
                    using (var uninstallOptsForm = new UninstallOptionsForm(isSimulation: true)) // Pasar flag al constructor
                    {
                        // uninstallOptsForm.SetSimulationMode(true); // Método para deshabilitar checkboxes no relevantes si es simulación
                        if (uninstallOptsForm.ShowDialog(this) == DialogResult.OK)
                        {
                            await RunOperationAsync(
                                (progress, token) => _uninstallService.UninstallAsync(
                                    selectedInstallation,
                                    uninstallOptsForm.CreateBackup,
                                    true, // Forzar WhatIf para la simulación
                                    uninstallOptsForm.RemoveUserData,
                                    uninstallOptsForm.RemoveSharedComponents,
                                    progress,
                                    token),
                                $"SIMULANDO Desinstalación de {selectedInstallation.DisplayName}",
                                requiresElevation: true // Mantenemos true, ya que hemos chequeado _isCurrentlyAdmin antes
                            );
                        }
                    }
                    break;

                case TestModeOperation.SimulateCleanup:
                    if (!_isCurrentlyAdmin && !_developmentMode)
                    {
                        CustomMsgBox.Show("La simulación de limpieza requiere privilegios de administrador. Por favor, use primero el botón 'Privilegios'.", "Privilegios Requeridos", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                        return;
                    }
                    if (selectedInstallation == null || !selectedInstallation.IsResidual) /* ... (manejo de no selección) ... */ return;
                    using (var cleanupOptsForm = new CleanupOptionsForm(isSimulation: true)) // Pasar flag al constructor
                    {
                        // cleanupOptsForm.SetSimulationMode(true);
                        if (cleanupOptsForm.ShowDialog(this) == DialogResult.OK)
                        {
                            await RunOperationAsync(
                                (progress, token) => _cleanupService.CleanupAsync(
                                    selectedInstallation,
                                    cleanupOptsForm.CreateBackup,
                                    true, // Forzar WhatIf para la simulación
                                    cleanupOptsForm.CleanupTempFiles,
                                    cleanupOptsForm.CleanupRegistry,
                                    cleanupOptsForm.CleanupConfigFiles,
                                    cleanupOptsForm.CleanupCacheFiles,
                                    progress,
                                    token),
                                $"SIMULANDO Limpieza de {selectedInstallation.DisplayName}",
                                requiresElevation: true // Mantenemos true
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

// Es posible que necesites modificar los constructores de UninstallOptionsForm y CleanupOptionsForm:
// public UninstallOptionsForm(bool isSimulation = false) { /* ... */ if (isSimulation) chkWhatIfMode.Enabled = false; chkWhatIfMode.Checked = true; }
// public CleanupOptionsForm(bool isSimulation = false) { /* ... */ if (isSimulation) chkWhatIfMode.Enabled = false; chkWhatIfMode.Checked = true; }
```

---

### **IV. Actualización de la Documentación**

*   **`ManualDesarrollo/03_GUI_Descripcion_Visual.md`**:
    *   Actualizar la descripción del botón `btnDetect` (ahora "Privilegios" inicialmente).
    *   Añadir descripción del indicador "(Administrador)" en la barra de título.
    *   Actualizar capturas de pantalla relevantes.
*   **`ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`**:
    *   Modificar la sección del botón `btnDetect` para describir su nuevo comportamiento dual y el flujo de solicitud de privilegios.
    *   Mencionar el `CustomMsgBox` de explicación.
*   **`ManualDesarrollo/05_Flujo_Aplicacion.md`**:
    *   Actualizar el diagrama de flujo inicial y el de detección para incluir el nuevo paso de solicitud/concesión de privilegios.
    *   Describir el reinicio y la continuación automática de la detección.
*   **`PlanDesarrollo.md`**:
    *   Añadir esta mejora como una tarea específica, idealmente en una etapa temprana de "Mejoras de UX" o dentro de la "Etapa 4: Funcionalidades Avanzadas y Conexión UI-Core" si se considera una refactorización de la lógica existente.
*   **`memory-bank/activeContext.md`**:
    *   Si este es el trabajo actual, reflejarlo aquí.

---

### **Beneficios Esperados (Reiteración):**
*   **Mejora drástica de la UX:** El usuario entenderá por qué se necesita elevación y cuándo ocurrirá.
*   **Flujo más natural:** Si se concede elevación para detectar, la detección se inicia automáticamente, evitando un clic redundante.
*   **Mayor claridad:** El título de la ventana y el botón reflejan el estado actual de privilegios.
*   **Reducción de confusión:** Se evita que el reinicio abrupto se perciba como un error.

Este plan es detallado y debería proporcionar una guía clara para el agente de IA que realice la implementación. Se han incluido los nombres de archivo, nombres de métodos, y fragmentos de código específicos para minimizar la ambigüedad.```
Entendido. He estudiado tu proyecto y la propuesta de mejora. El plan es claro y apunta directamente a mejorar la experiencia del usuario en un punto crítico de interacción.

Aquí está el plan de implementación detallado, diseñado para ser lo más explícito posible para un agente de IA que realice los cambios:

---

## Plan Detallado para la Mejora de Solicitud de Privilegios

### **Objetivo General:**
Modificar el flujo de inicio y la interacción con el botón `btnDetect` para que la solicitud de privilegios elevados sea explícita, informativa y menos disruptiva para el usuario. El botón `btnDetect` pasará a llamarse "Privilegios" inicialmente y cambiará a "Detectar" tras la concesión de permisos, momento en el cual la detección se iniciará automáticamente.

---

### **I. Cambios en `DesinstalaPhotoshop.UI/Program.cs`**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/Program.cs`

**Propósito:**
Permitir que `MainForm` distinga entre un reinicio genérico con elevación y un reinicio específico solicitado para la detección.

**Cambios Específicos:**
1.  Modificar el método `Main` para que acepte argumentos de línea de comandos.
2.  Introducir un nuevo argumento, por ejemplo, `"--elevated-for-detection"`.
3.  Pasar dos booleanos al constructor de `MainForm`: uno para indicar si la aplicación se inició elevada en general, y otro para indicar si se elevó específicamente para la detección.

**Código:**
```csharp
// File: DesinstalaPhotoshop.UI/Program.cs
// namespace DesinstalaPhotoshop.UI
// {
//     internal static class Program
//     {
//         [STAThread]
//         static void Main(string[] args) // <--- MODIFICAR: Añadir 'string[] args'
//         {
//             ApplicationConfiguration.Initialize();
//             Application.SetColorMode(SystemColorMode.Dark);

//             // Verificar si la aplicación se inició con argumentos de elevación
//             bool isElevatedGenerally = args.Contains("--elevated", StringComparer.OrdinalIgnoreCase);
//             bool isElevatedForDetection = args.Contains("--elevated-for-detection", StringComparer.OrdinalIgnoreCase);
            
//             // Determinar el estado final de 'startedElevated' y 'justElevatedForDetection'
//             bool startedElevated = isElevatedGenerally || isElevatedForDetection;
//             bool justElevatedForDetection = isElevatedForDetection;

//             Application.Run(new MainForm(startedElevated, justElevatedForDetection)); // <--- MODIFICAR: Pasar nuevos argumentos
//         }
//     }
// }
```
**Reemplazar el contenido de `DesinstalaPhotoshop.UI/Program.cs` con:**
```csharp
using System;
using System.Linq; // Necesario para args.Contains
using System.Windows.Forms;

namespace DesinstalaPhotoshop.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) // MODIFICADO: Añadir 'string[] args'
        {
            // Configuración de la aplicación
            ApplicationConfiguration.Initialize();

            // Aplicar tema oscuro
            Application.SetColorMode(SystemColorMode.Dark);

            // Verificar si la aplicación se inició con argumentos de elevación
            bool isElevatedGenerally = args.Contains("--elevated", StringComparer.OrdinalIgnoreCase);
            bool isElevatedForDetection = args.Contains("--elevated-for-detection", StringComparer.OrdinalIgnoreCase);
            
            // Determinar el estado final de 'startedElevated' y 'justElevatedForDetection'
            // 'startedElevated' será true si CUALQUIERA de los dos flags de elevación está presente (o si AdminHelper.IsRunningAsAdmin() es true).
            // Para simplificar, pasaremos ambos flags y MainForm determinará el estado real.
            // Aquí solo nos preocupamos de los flags que indican el *intento* de elevación y su propósito.

            Application.Run(new MainForm(isElevatedGenerally, isElevatedForDetection)); // MODIFICADO: Pasar nuevos argumentos
        }
    }
}
```

---

### **II. Cambios en `DesinstalaPhotoshop.UI/MainForm.cs`**

**1. Nuevos Campos de Instancia y Constructor Modificado**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs`

**Propósito:**
`MainForm` necesita mantener el estado de sus privilegios actuales y si fue reiniciada específicamente para la detección.

**Cambios Específicos:**
1.  Añadir dos nuevos campos booleanos: `_isCurrentlyAdmin` y `_justElevatedForDetection`.
2.  Modificar el constructor de `MainForm` para aceptar los dos nuevos parámetros booleanos de `Program.cs`.
3.  Inicializar los nuevos campos en el constructor.

**Código:**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
public partial class MainForm : Form
{
    // ... (servicios existentes) ...

    // ELIMINAR o COMENTAR el campo existente:
    // private readonly bool _startedElevated = false; 

    // AÑADIR NUEVOS CAMPOS:
    private bool _isCurrentlyAdmin = false;
    private readonly bool _initialCheckJustElevatedForDetection = false; // Flag para el chequeo inicial en el constructor

    // ... (resto de campos existentes) ...

    // MODIFICAR CONSTRUCTOR:
    public MainForm(bool startedElevatedArgument = false, bool justElevatedForDetectionArgument = false)
    {
        InitializeComponent();
        
        _isCurrentlyAdmin = AdminHelper.IsRunningAsAdmin(); // Comprobar estado real de admin
        _initialCheckJustElevatedForDetection = justElevatedForDetectionArgument && _isCurrentlyAdmin; // True si se pasó el flag Y realmente se elevó

        // Inicialización de servicios (existente)
        _loggingService = new LoggingService(); 
        // ... (resto de inicialización de servicios)

        // Esta llamada configurará el texto/icono del botón y título basado en _isCurrentlyAdmin
        SetupControls(); 
        SetupEventHandlers();

        _loggingService.LogInfo($"Aplicación iniciada. Admin: {_isCurrentlyAdmin}. Flag 'elevada para detección' (argumento): {justElevatedForDetectionArgument}. Resultado 'justElevatedForDetection': {_initialCheckJustElevatedForDetection}.");
        if (_developmentMode) // _developmentMode ya existe
        {
            _loggingService.LogWarning("MODO DESARROLLO ACTIVO: Las solicitudes de elevación pueden ser omitidas.");
        }
    }
    // ...
}
```

**2. Ajustes en `SetupControls()` para Estado Inicial del Botón y Título**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs`

**Propósito:**
Configurar la apariencia inicial de `btnDetect` (texto, icono, tooltip) y la barra de título del formulario según el estado de privilegios. Si la aplicación acaba de ser elevada para detección, se debe iniciar el proceso de detección automáticamente.

**Cambios Específicos:**
1.  Dentro de `SetupControls`, añadir lógica condicional para `btnDetect` y `this.Text`.
2.  Si `_initialCheckJustElevatedForDetection` es `true`, llamar a un nuevo método (que crearemos) para iniciar la detección.

**Código:**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private void SetupControls()
{
    // ... (código existente para cargar icono, StartPosition, MinimumSize, BackColor) ...
    
    // MODIFICAR/AÑADIR LÓGICA PARA btnDetect Y TÍTULO:
    if (_isCurrentlyAdmin)
    {
        this.Text = "DesinstalaPhotoshop (Administrador)";
        btnDetect.Text = "  Detectar";
        btnDetect.IconChar = FontAwesome.Sharp.IconChar.Search;
        toolTip.SetToolTip(btnDetect, "Detectar instalaciones de Photoshop en el sistema");

        if (_initialCheckJustElevatedForDetection)
        {
            _loggingService.LogInfo("Privilegios concedidos para detección. Iniciando detección automáticamente post-elevación...");
            // Usar BeginInvoke para asegurar que el formulario esté completamente cargado antes de iniciar la operación
            this.BeginInvoke(new Action(async () => await PerformDetectionAsync()));
        }
    }
    else
    {
        this.Text = "DesinstalaPhotoshop";
        btnDetect.Text = "  Privilegios";
        // Asegúrate que IconChar.ShieldAlt exista o elige uno adecuado como UserShield, KeyLock, etc.
        btnDetect.IconChar = FontAwesome.Sharp.IconChar.ShieldAlt; 
        toolTip.SetToolTip(btnDetect, "Solicitar privilegios de administrador para funciones completas");
    }
    
    // SetupTooltips(); // Se llama DESPUÉS de configurar btnDetect
    // ... (resto de SetupControls: animationTimer.Tick, ocultar progreso, UpdateButtonsState) ...
}

// AÑADIR ESTE NUEVO MÉTODO (refactorizado de la lógica de BtnDetect_Click):
private async Task PerformDetectionAsync()
{
    // Esta condición es un seguro, pero la lógica de llamada debería garantizarlo.
    if (!_isCurrentlyAdmin && !_developmentMode) 
    {
        _loggingService.LogWarning("PerformDetectionAsync llamado sin privilegios y no en modo desarrollo.");
        // Podrías mostrar un mensaje, pero idealmente esto no debería ocurrir.
        return;
    }

    var installationsResult = await RunOperationAsync(
        (progress, token) => _detectionService.DetectInstallationsAsync(progress, token),
        "Detectando Instalaciones"
        // 'requiresElevation' ya no es necesario aquí en RunOperationAsync si este método
        // solo se llama cuando ya se es admin o en modo desarrollo.
        // O puedes dejarlo como 'requiresElevation: _isCurrentlyAdmin' si quieres que RunOperationAsync sea más genérico.
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
```

**3. Modificar el Manejador de Eventos `BtnDetect_Click`**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs`

**Propósito:**
Este método ahora gestionará dos escenarios:
1.  Si no se tienen privilegios (botón "Privilegios"): Mostrar el `CustomMsgBox` solicitando la elevación.
2.  Si ya se tienen privilegios (botón "Detectar"): Iniciar el proceso de detección llamando a `PerformDetectionAsync`.

**Cambios Específicos:**

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private async void BtnDetect_Click(object sender, EventArgs e)
{
    if (!_isCurrentlyAdmin && !_developmentMode) // Si no somos admin (y no en modo dev)
    {
        _loggingService.LogInfo("Botón 'Privilegios' presionado. Solicitando confirmación para elevación.");
        var result = CustomMsgBox.Show(
            prompt: "DesinstalaPhotoshop necesita privilegios de administrador para:\n\n" +
                    "  • Detectar completamente todas las instalaciones y residuos.\n" +
                    "  • Realizar operaciones de desinstalación y limpieza.\n\n" +
                    "Al conceder privilegios, la aplicación se reiniciará.\n\n" +
                    "¿Desea continuar y otorgar privilegios de administrador?",
            title: "Solicitud de Privilegios Elevados",
            buttons: CustomMessageBoxButtons.YesNo,
            icon: CustomMessageBoxIcon.Shield, // Usar un icono apropiado
            theme: ThemeSettings.DarkTheme
        );

        if (result == CustomDialogResult.Yes)
        {
            _loggingService.LogInfo("Usuario aceptó la elevación de privilegios. Reiniciando como administrador para detección...");
            // Usar el argumento específico para que la nueva instancia sepa que debe auto-iniciar la detección
            if (!AdminHelper.RestartAsAdmin("--elevated-for-detection"))
            {
                // AdminHelper.RestartAsAdmin llama a Environment.Exit, así que si devuelve false, es porque falló al iniciar el proceso.
                _loggingService.LogError("No se pudo reiniciar la aplicación con privilegios de administrador.");
                CustomMsgBox.Show(
                    prompt: "No se pudo reiniciar la aplicación con permisos de administrador. Por favor, intente ejecutar la aplicación manualmente como administrador.",
                    title: "Error de Elevación",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error,
                    theme: ThemeSettings.DarkTheme
                );
            }
            // Si RestartAsAdmin tiene éxito, la aplicación actual se cierra.
        }
        else
        {
            _loggingService.LogInfo("Usuario denegó la elevación de privilegios.");
            CustomMsgBox.Show(
                prompt: "No se concedieron privilegios de administrador. La funcionalidad de detección y otras operaciones críticas estarán limitadas o no disponibles.",
                title: "Privilegios Denegados",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Warning,
                theme: ThemeSettings.DarkTheme
            );
        }
    }
    else // Ya es admin (o en modo dev), el botón dice "Detectar"
    {
        _loggingService.LogInfo("Botón 'Detectar' presionado (con privilegios o modo desarrollo).");
        await PerformDetectionAsync(); // Llamar al método refactorizado
    }
}
```

**4. Revisión de `RunOperationAsync`**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs`

**Propósito:**
La lógica de elevación de privilegios dentro de `RunOperationAsync` ahora es mayormente un *fallback* o para operaciones distintas a la detección inicial. La detección ya habrá manejado su elevación. Otras operaciones como Desinstalar/Limpiar verificarán `_isCurrentlyAdmin` *antes* de llamar a `RunOperationAsync`.

**Cambios Específicos:**
Mantener la verificación `requiresElevation` pero entender que para el flujo de detección, no debería ser necesario que `RunOperationAsync` intente elevar de nuevo.

```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private async Task<T?> RunOperationAsync<T>(
    Func<IProgress<Core.Models.ProgressInfo>, CancellationToken, Task<T>> operation,
    string operationName,
    bool requiresElevation = false) where T : class
{
    // La lógica de elevación principal ahora está ANTES de llamar a RunOperationAsync para Detect, Uninstall, Cleanup.
    // Esta sección es más una salvaguarda.
    if (requiresElevation && !_isCurrentlyAdmin && !_developmentMode)
    {
        _loggingService.LogWarning($"Operación '{operationName}' requiere elevación, pero no se poseen. Guiar al usuario a 'Privilegios'.");
        CustomMsgBox.Show(
            prompt: $"La operación '{operationName}' requiere privilegios de administrador. Por favor, use primero el botón 'Privilegios'.",
            title: "Privilegios insuficientes",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error,
            theme: ThemeSettings.DarkTheme);
        return default; 
    }
    if (requiresElevation && !_isCurrentlyAdmin && _developmentMode)
    {
        _loggingService.LogWarning($"MODO DESARROLLO: Ejecutando '{operationName}' que nominalmente requiere elevación, sin elevación real. Puede fallar.");
    }

    // ... (resto del método permanece igual: PrepareUIForOperation, CancellationTokenSource, Progress, try-catch-finally) ...
}
```
**Importante:** Para los manejadores `BtnUninstall_Click`, `BtnCleanup_Click`, `BtnRestore_Click`:
Debes añadir una comprobación de `_isCurrentlyAdmin` al inicio de estos métodos. Si no se es admin (y no `_developmentMode`), muestra un `CustomMsgBox` indicando que se necesitan privilegios y que usen el botón "Privilegios" primero, y luego haz `return;`.

**Ejemplo para `BtnUninstall_Click`:**
```csharp
private async void BtnUninstall_Click(object sender, EventArgs e)
{
    if (!_isCurrentlyAdmin && !_developmentMode)
    {
        CustomMsgBox.Show("La desinstalación requiere privilegios de administrador.\nPor favor, use primero el botón 'Privilegios'.", 
                          "Privilegios Requeridos", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
        return;
    }

    // ... resto de la lógica existente de BtnUninstall_Click ...
    // La llamada a RunOperationAsync puede mantener 'requiresElevation: true' como una doble comprobación,
    // o puedes cambiarlo a 'requiresElevation: false' ya que ya has comprobado _isCurrentlyAdmin.
    // Mantenerlo en true es más seguro por si se llama a RunOperationAsync desde otro lugar.
}
// Aplicar lógica similar a BtnCleanup_Click y BtnRestore_Click.
```

---

### **III. Ajustes en `DesinstalaPhotoshop.UI/MainForm.Designer.cs`**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.Designer.cs` (o mediante el Diseñador de Formularios de Visual Studio)

**Propósito:**
Establecer el texto e icono iniciales del botón `btnDetect` a "Privilegios" y el icono correspondiente.

**Cambios Específicos:**
1.  En el método `InitializeComponent()`, localizar la definición de `btnDetect`.
2.  Cambiar `this.btnDetect.Text` a `"  Privilegios"`.
3.  Cambiar `this.btnDetect.IconChar` a `FontAwesome.Sharp.IconChar.ShieldAlt` (o el icono elegido).

**Código (fragmento relevante):**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.Designer.cs
// Dentro de InitializeComponent()

// this.btnDetect
// 
this.btnDetect.BackColor = System.Drawing.Color.FromArgb(30, 40, 60);
this.btnDetect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 70, 90);
this.btnDetect.FlatAppearance.BorderSize = 0;
this.btnDetect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
this.btnDetect.ForeColor = System.Drawing.Color.White;
// MODIFICAR ICONO:
this.btnDetect.IconChar = FontAwesome.Sharp.IconChar.ShieldAlt; // O UserShield, KeyLock etc.
this.btnDetect.IconColor = System.Drawing.Color.White;
this.btnDetect.IconFont = FontAwesome.Sharp.IconFont.Auto;
this.btnDetect.IconSize = 24;
this.btnDetect.Location = new System.Drawing.Point(12, 12);
this.btnDetect.Name = "btnDetect";
this.btnDetect.Size = new System.Drawing.Size(120, 36); // Ajustar si es necesario para el nuevo texto
this.btnDetect.TabIndex = 0;
// MODIFICAR TEXTO:
this.btnDetect.Text = "  Privilegios"; 
this.btnDetect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
this.btnDetect.UseVisualStyleBackColor = false;
// this.btnDetect.Click += new System.EventHandler(this.BtnDetect_Click); // Esto ya está
```

---

### **IV. Revisión del Modo Prueba (`BtnTestMode_Click`)**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs`

**Propósito:**
Asegurar que el modo prueba no solicite innecesariamente privilegios si las operaciones de simulación (`whatIf: true`) no los requieren estrictamente.

**Cambios Específicos:**
1.  Analizar cada caso dentro de `BtnTestMode_Click`.
2.  Para `TestModeOperation.DetectOnly`: El flujo actual que llama a `BtnDetect_Click` es adecuado. Si no hay privilegios, se guiará al usuario para obtenerlos.
3.  Para `TestModeOperation.SimulateUninstall` y `TestModeOperation.SimulateCleanup`:
    *   **Mantener `requiresElevation: true` en la llamada a `RunOperationAsync`**. Aunque sea simulación, una enumeración precisa de lo que *se haría* puede necesitar acceso a HKLM o `C:\Program Files`.
    *   **Antes de llamar a `RunOperationAsync`**, verificar `_isCurrentlyAdmin`. Si es `false` (y no `_developmentMode`):
        *   Mostrar un `CustomMsgBox` explicando que la simulación será más precisa con privilegios elevados y preguntar si desea elevar.
        *   Si el usuario acepta, llamar a `AdminHelper.RestartAsAdmin("--elevated");` (un argumento genérico es suficiente aquí, ya que no se espera auto-ejecución de la simulación). Luego `return;` de `BtnTestMode_Click` ya que la app se reiniciará.
        *   Si el usuario declina, proceder con la simulación, pero se podría loguear una advertencia de que los resultados pueden ser incompletos. La llamada a `RunOperationAsync` se haría, y si `requiresElevation` es `true`, su lógica interna (como la modificamos) ahora solo mostraría el error de "use el botón Privilegios primero" si `_developmentMode` es false.
        *   **Recomendación más simple y robusta:** Si para `SimulateUninstall` o `SimulateCleanup` no se es admin, simplemente mostrar un `CustomMsgBox` indicando: "Esta simulación requiere privilegios de administrador. Por favor, use primero el botón 'Privilegios' y luego intente esta opción del modo de prueba." y hacer `return;`. Esto evita complejidades en `RunOperationAsync` para el modo prueba.

**Código (Implementando la recomendación más simple para Modo Prueba):**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
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
                    BtnDetect_Click(sender, e); // Sigue el nuevo flujo de solicitud de elevación si es necesario
                    break;

                case TestModeOperation.SimulateUninstall:
                    if (!_isCurrentlyAdmin && !_developmentMode)
                    {
                        CustomMsgBox.Show("La simulación de desinstalación requiere privilegios de administrador.\nPor favor, use primero el botón 'Privilegios'.", 
                                          "Privilegios Requeridos para Simulación", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                        return;
                    }
                    // ... (resto del caso SimulateUninstall existente) ...
                    // La llamada a RunOperationAsync seguirá con requiresElevation: true
                    // y la lógica interna de RunOperationAsync ya no intentará una elevación forzada si _isCurrentlyAdmin es true.
                    break;

                case TestModeOperation.SimulateCleanup:
                    if (!_isCurrentlyAdmin && !_developmentMode)
                    {
                        CustomMsgBox.Show("La simulación de limpieza requiere privilegios de administrador.\nPor favor, use primero el botón 'Privilegios'.", 
                                          "Privilegios Requeridos para Simulación", CustomMessageBoxButtons.OK, CustomMessageBoxIcon.Warning, theme: ThemeSettings.DarkTheme);
                        return;
                    }
                    // ... (resto del caso SimulateCleanup existente) ...
                    break;
            }
        }
        // ... (else para DialogResult.OK)
    }
}
```

---

### **V. Actualización de Documentación Relacionada**

**Archivos Afectados:**
*   `ManualDesarrollo/03_GUI_Descripcion_Visual.md`
*   `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`
*   `ManualDesarrollo/05_Flujo_Aplicacion.md`
*   `PlanDesarrollo.md`
*   `memory-bank/activeContext.md` (si esta es la tarea activa)

**Propósito:**
Mantener la documentación actualizada con los nuevos comportamientos y apariencias.

**Cambios Específicos:**
*   **`03_GUI_Descripcion_Visual.md`**: Actualizar descripción e imágenes del botón `btnDetect` (ahora "Privilegios") y mencionar el indicador "(Administrador)" en la barra de título.
*   **`04_GUI_Funcionalidad_Controles.md`**: Detallar el nuevo comportamiento dual del botón `btnDetect` y el flujo de `CustomMsgBox` para solicitud de privilegios.
*   **`05_Flujo_Aplicacion.md`**: Modificar diagramas de flujo para reflejar el nuevo paso de solicitud de privilegios y el reinicio/continuación automática.
*   **`PlanDesarrollo.md`**: Registrar esta mejora.
*   **`activeContext.md`**: Actualizar si es la tarea en curso.
