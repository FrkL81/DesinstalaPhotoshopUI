## Plan Detallado para Mejoras Adicionales en UI y Comportamiento

### **I. Selección automática del modo simulación al acceder desde “Modo Prueba”**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs` (método `BtnTestMode_Click`)
*   `DesinstalaPhotoshop.UI/UninstallOptionsForm.cs`
*   `DesinstalaPhotoshop.UI/UninstallOptionsForm.Designer.cs` (si se añade un nuevo constructor o propiedades)
*   `DesinstalaPhotoshop.UI/CleanupOptionsForm.cs`
*   `DesinstalaPhotoshop.UI/CleanupOptionsForm.Designer.cs` (si se añade un nuevo constructor o propiedades)

**Propósito:**
Asegurar que cuando el usuario entra a las opciones de desinstalación o limpieza a través del "Modo Prueba", la opción de simulación (`chkWhatIfMode`) esté preseleccionada y bloqueada, reflejando la intención del usuario.

**Cambios Específicos:**

1.  **Modificar Constructores de Formularios de Opciones:**
    *   Añadir un parámetro opcional `bool isSimulationContext = false` a los constructores de `UninstallOptionsForm` y `CleanupOptionsForm`.
    *   Dentro del constructor, si `isSimulationContext` es `true`, marcar `chkWhatIfMode.Checked = true;` y `chkWhatIfMode.Enabled = false;`.

2.  **Actualizar Llamadas en `MainForm.BtnTestMode_Click`:**
    *   Al crear instancias de `UninstallOptionsForm` y `CleanupOptionsForm` dentro de los casos `SimulateUninstall` y `SimulateCleanup`, pasar `isSimulationContext: true` al constructor.

**Código:**

**En `DesinstalaPhotoshop.UI/UninstallOptionsForm.cs`:**
```csharp
// File: DesinstalaPhotoshop.UI/UninstallOptionsForm.cs
// ... (usings) ...
public partial class UninstallOptionsForm : Form
{
    // ... (campos existentes) ...

    // MODIFICAR CONSTRUCTOR o AÑADIR NUEVO CONSTRUCTOR SOBRECARGADO:
    public UninstallOptionsForm(bool isSimulationContext = false) // Nuevo parámetro opcional
    {
        InitializeComponent();
        SetupForm(); // Llama a la configuración existente

        if (isSimulationContext)
        {
            chkWhatIfMode.Checked = true;
            chkWhatIfMode.Enabled = false; // Bloquear el checkbox
            // Actualizar el estado de la propiedad interna si es necesario
            _whatIfMode = true; 
        }
    }
    // ... (resto de la clase) ...
}
```

**En `DesinstalaPhotoshop.UI/CleanupOptionsForm.cs`:**
```csharp
// File: DesinstalaPhotoshop.UI/CleanupOptionsForm.cs
// ... (usings) ...
public partial class CleanupOptionsForm : Form
{
    // ... (campos existentes) ...

    // MODIFICAR CONSTRUCTOR o AÑADIR NUEVO CONSTRUCTOR SOBRECARGADO:
    public CleanupOptionsForm(bool isSimulationContext = false) // Nuevo parámetro opcional
    {
        InitializeComponent();
        SetupForm(); // Llama a la configuración existente

        if (isSimulationContext)
        {
            chkWhatIfMode.Checked = true;
            chkWhatIfMode.Enabled = false; // Bloquear el checkbox
            // Actualizar el estado de la propiedad interna si es necesario
            _whatIfMode = true;
        }
    }
    // ... (resto de la clase) ...
}
```

**En `DesinstalaPhotoshop.UI/MainForm.cs` (método `BtnTestMode_Click`):**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private async void BtnTestMode_Click(object sender, EventArgs e)
{
    // ... (código existente) ...
    switch (form.SelectedOperation)
    {
        case TestModeOperation.DetectOnly:
            // ... (sin cambios aquí) ...
            break;

        case TestModeOperation.SimulateUninstall:
            // ... (código de verificación de admin y selectedInstallation) ...
            using (var uninstallOptsForm = new UninstallOptionsForm(isSimulationContext: true)) // MODIFICADO
            {
                // Ya no es necesario forzar chkWhatIfMode.Checked aquí, el constructor lo hará
                if (uninstallOptsForm.ShowDialog(this) == DialogResult.OK)
                {
                    // La propiedad uninstallOptsForm.WhatIfMode ya será true debido al constructor
                    await RunOperationAsync(
                        (progress, token) => _uninstallService.UninstallAsync(
                            selectedInstallation,
                            uninstallOptsForm.CreateBackup,
                            true, // Se pasa true explícitamente para el servicio
                            uninstallOptsForm.RemoveUserData,
                            uninstallOptsForm.RemoveSharedComponents,
                            progress,
                            token),
                        $"SIMULANDO Desinstalación de {selectedInstallation.DisplayName}",
                        requiresElevation: true 
                    );
                }
            }
            break;

        case TestModeOperation.SimulateCleanup:
            // ... (código de verificación de admin y selectedInstallation) ...
            using (var cleanupOptsForm = new CleanupOptionsForm(isSimulationContext: true)) // MODIFICADO
            {
                 // Ya no es necesario forzar chkWhatIfMode.Checked aquí, el constructor lo hará
                if (cleanupOptsForm.ShowDialog(this) == DialogResult.OK)
                {
                    // La propiedad cleanupOptsForm.WhatIfMode ya será true
                    await RunOperationAsync(
                        (progress, token) => _cleanupService.CleanupAsync(
                            selectedInstallation,
                            cleanupOptsForm.CreateBackup,
                            true, // Se pasa true explícitamente para el servicio
                            cleanupOptsForm.CleanupTempFiles,
                            cleanupOptsForm.CleanupRegistry,
                            cleanupOptsForm.CleanupConfigFiles,
                            cleanupOptsForm.CleanupCacheFiles,
                            progress,
                            token),
                        $"SIMULANDO Limpieza de {selectedInstallation.DisplayName}",
                        requiresElevation: true
                    );
                }
            }
            break;
    }
    // ... (resto del método) ...
}
```

---

### **II. Comportamiento Correcto del Botón “Cancelar” Tras Simulaciones**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs` (principalmente el `finally` block de `RunOperationAsync` y `RestoreUI`)

**Propósito:**
Asegurar que el botón "Cancelar" solo esté activo durante una operación real y que los demás botones se restauren correctamente al finalizar cualquier operación, incluyendo simulaciones.

**Cambios Específicos:**
La lógica actual en `RunOperationAsync` es:
```csharp
finally
{
    RestoreUI();
    _cancellationTokenSource?.Dispose();
    _cancellationTokenSource = null;
}
```
Y en `RestoreUI`:
```csharp
private void RestoreUI()
{
    animationTimer.Stop();
    // ... (lógica de visibilidad de progreso) ...
    _currentOperation = string.Empty;
    UpdateButtonsState(); // <--- Aquí se actualiza el estado de los botones
}
```
Y en `UpdateButtonsState`:
```csharp
bool isOperationRunning = _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;
// ...
btnCancel.Enabled = isOperationRunning;
// ... (otros botones se habilitan con !isOperationRunning) ...
```
Este orden implica que cuando `UpdateButtonsState` se llama desde `RestoreUI`, `_cancellationTokenSource` *aún no es null*. Por lo tanto, `isOperationRunning` sigue siendo `true` en esa llamada, lo que lleva al comportamiento incorrecto.

**Solución:**
Modificar el bloque `finally` en `RunOperationAsync` para limpiar `_cancellationTokenSource` *antes* de llamar a `RestoreUI`.

**Código:**

**En `DesinstalaPhotoshop.UI/MainForm.cs` (método `RunOperationAsync`):**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private async Task<T?> RunOperationAsync<T>(
    Func<IProgress<Core.Models.ProgressInfo>, CancellationToken, Task<T>> operation,
    string operationName,
    bool requiresElevation = false) where T : class
{
    // ... (código existente de verificación de elevación y preparación de UI) ...
    // PrepareUIForOperation(operationName); // Ya se llama aquí
    // _cancellationTokenSource = new CancellationTokenSource(); // Ya se crea aquí
    // ... (progressReporter y bloque try-catch) ...
    try
    {
        // ... (código existente) ...
    }
    catch (OperationCanceledException)
    {
        // ... (código existente) ...
    }
    catch (Exception ex)
    {
        // ... (código existente) ...
    }
    finally
    {
        // MODIFICAR EL ORDEN:
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;  // Limpiar PRIMERO

        RestoreUI(); // LUEGO restaurar UI, que llamará a UpdateButtonsState
                     // UpdateButtonsState ahora verá _cancellationTokenSource como null,
                     // por lo que isOperationRunning será false.
    }
}
```
Con este cambio, cuando `RestoreUI` llame a `UpdateButtonsState`, `_cancellationTokenSource` ya será `null`, por lo que `isOperationRunning` será `false`, deshabilitando correctamente `btnCancel` y habilitando los demás botones según corresponda.

---

### **III. Mejora en Visibilidad y Comportamiento de Elementos Visuales Post-Operación**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs` (método `RestoreUI`)
*   `DesinstalaPhotoshop.UI/MainForm.Designer.cs` (para añadir un nuevo `System.Windows.Forms.Timer`)

**Propósito:**
Mantener visibles los indicadores de progreso (`lblAnimatedText`, `progressBar`) y el estado final (`lblProgress`) por un breve periodo después de que una operación concluya, para mejorar la retroalimentación al usuario.

**Cambios Específicos:**
1.  **Añadir un nuevo Timer:** `postOperationDisplayTimer` en `MainForm.Designer.cs`.
2.  **Modificar `RestoreUI`:**
    *   No ocultar inmediatamente los controles de progreso.
    *   `lblProgress` ya muestra el estado final (Completado, Error, etc.).
    *   `lblAnimatedText` debe mostrar un mensaje de finalización estático.
    *   Iniciar `postOperationDisplayTimer` con un intervalo (ej. 3000 ms).
3.  **Implementar el evento `Tick` para `postOperationDisplayTimer`:**
    *   Cuando el timer haga tick, ocultará `lblProgress`, `progressBar`, y `lblAnimatedText`.

**Código:**

**En `DesinstalaPhotoshop.UI/MainForm.Designer.cs`:**
Añadir un nuevo Timer (puede hacerse visualmente o manualmente en `InitializeComponent`):
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.Designer.cs
// ...
private System.Windows.Forms.Timer animationTimer; // Ya existe
private System.Windows.Forms.Timer postOperationDisplayTimer; // AÑADIR ESTE
// ...

private void InitializeComponent()
{
    // ... (componentes existentes) ...
    this.animationTimer = new System.Windows.Forms.Timer(this.components); // Ya existe
    this.postOperationDisplayTimer = new System.Windows.Forms.Timer(this.components); // AÑADIR ESTE

    // Configurar postOperationDisplayTimer
    this.postOperationDisplayTimer.Interval = 3000; // 3 segundos
    this.postOperationDisplayTimer.Tick += new System.EventHandler(this.PostOperationDisplayTimer_Tick);
    // ...
}
```

**En `DesinstalaPhotoshop.UI/MainForm.cs`:**

Añadir el manejador para el nuevo timer y modificar `RestoreUI`:
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs

// ... (otros métodos) ...

private void RestoreUI()
{
    animationTimer.Stop(); // Detener la animación de "procesando..."

    // Determinar el mensaje final para lblAnimatedText y lblProgress
    string finalProgressMessage = $"{_currentOperation} - Finalizado";
    string finalAnimatedTextMessage = "Operación Concluida.";

    if (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested)
    {
        finalProgressMessage = $"{_currentOperation} - Cancelado";
        finalAnimatedTextMessage = "Operación Cancelada.";
        if (progressBar.Value < 100) progressBar.Value = 0; // O mantener el progreso si se desea
    }
    else if (progressBar.Value >= 100) // Asumiendo que 100% es éxito o error ya manejado
    {
        // La lógica existente en UpdateProgress ya establece el mensaje de "Completado (100%)" o error.
        // Podemos usar el texto actual de lblProgress si es más específico.
        finalProgressMessage = lblProgress.Text; // Mantener el mensaje detallado de UpdateProgress
        // finalAnimatedTextMessage se establecerá abajo según el estado
    }
    else // Operación no completada al 100% y no cancelada (podría ser un error que no llegó a 100%)
    {
        finalProgressMessage = $"{_currentOperation} - Interrumpido ({progressBar.Value}%)";
        finalAnimatedTextMessage = "Operación Interrumpida.";
    }
    
    // Actualizar textos para el display post-operación
    lblProgress.Text = finalProgressMessage;
    
    // Si la última ProgressInfo indicó error, reflejarlo
    // Esto es un poco heurístico, idealmente el OperationResult se pasaría a RestoreUI
    // Pero para simplificar, basémonos en el texto actual de lblProgress si contiene "Error"
    if (lblProgress.Text.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
        lblProgress.Text.Contains("Falló", StringComparison.OrdinalIgnoreCase))
    {
        finalAnimatedTextMessage = "Operación Fallida.";
    }
    else if (finalAnimatedTextMessage == "Operación Concluida." && progressBar.Value >=100)
    {
         finalAnimatedTextMessage = "Operación Completada con Éxito.";
    }

    lblAnimatedText.Text = finalAnimatedTextMessage;

    // Asegurar que estén visibles para el display post-operación
    lblProgress.Visible = true;
    progressBar.Visible = true;
    lblAnimatedText.Visible = true;

    _currentOperation = string.Empty; // Limpiar para la próxima operación
    UpdateButtonsState(); // Actualizar estado de botones (Cancel se deshabilitará)

    // Iniciar el timer para ocultar los controles de progreso después de un tiempo
    postOperationDisplayTimer.Start();
}

// AÑADIR ESTE MÉTODO (manejador del Tick para el nuevo Timer)
private void PostOperationDisplayTimer_Tick(object? sender, EventArgs e)
{
    postOperationDisplayTimer.Stop(); // Detener el timer para que solo se ejecute una vez

    // Ocultar los controles de progreso
    lblProgress.Visible = false;
    progressBar.Visible = false;
    lblAnimatedText.Visible = false;
    _loggingService.LogDebug("Controles de progreso post-operación ocultados.");
}
```

---

### **IV. Mejora en la Respuesta Visual al Iniciar una Operación**

**Archivos Afectados:**
*   `DesinstalaPhotoshop.UI/MainForm.cs` (método `PrepareUIForOperation`)

**Propósito:**
Hacer que la animación inicial de la UI (`lblAnimatedText`) aparezca lo más rápido posible para dar una sensación de respuesta inmediata.

**Cambios Específicos:**
1.  En `PrepareUIForOperation`, después de hacer visibles los controles y actualizar sus textos iniciales, y después de iniciar `animationTimer`, forzar un "tick" de la animación y un repintado de la UI *antes* del `Application.DoEvents()`.

**Código:**

**En `DesinstalaPhotoshop.UI/MainForm.cs` (método `PrepareUIForOperation`):**
```csharp
// File: DesinstalaPhotoshop.UI/MainForm.cs
private void PrepareUIForOperation(string operationName)
{
    _currentOperation = operationName;
    _animationDots = 0;
    _textAnimationState = 0; // Asegurar que la animación de texto comience desde el primer estado
    progressBar.Value = 0;

    lblProgress.Text = $"{_currentOperation} - 0%";
    lblAnimatedText.Text = _animationTexts[_textAnimationState] + new string('.', _animationDots); // Establecer texto inicial animado

    // Hacerlos visibles ANTES de DoEvents y el tick manual
    lblProgress.Visible = true;
    progressBar.Visible = true;
    lblAnimatedText.Visible = true;
    
    UpdateButtonsState(); // Esto deshabilitará/habilitará botones según isOperationRunning
    
    animationTimer.Start(); // Iniciar el timer que continuará la animación

    // Forzar el primer estado de la animación y el repintado de la UI
    AnimationTimer_Tick(null, EventArgs.Empty); // Ejecuta la lógica del tick una vez
    Application.DoEvents(); // Procesar eventos de UI para que se muestren los cambios
}
```
