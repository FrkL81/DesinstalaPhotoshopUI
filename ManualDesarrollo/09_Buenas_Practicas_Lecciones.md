# Buenas Prácticas y Lecciones Aprendidas

Este documento recopila las buenas prácticas implementadas durante el desarrollo del proyecto DesinstalaPhotoshop, así como las lecciones aprendidas que pueden ser útiles para futuros desarrollos o mantenimiento de la aplicación.

## Estructura del Documento

1. [Arquitectura y Patrones de Diseño](#arquitectura-y-patrones-de-diseño)
2. [Manejo de Errores y Excepciones](#manejo-de-errores-y-excepciones)
3. [Optimizaciones y Rendimiento](#optimizaciones-y-rendimiento)
4. [Seguridad](#seguridad)
5. [Interfaz de Usuario](#interfaz-de-usuario)
6. [Lecciones Aprendidas](#lecciones-aprendidas)

## Arquitectura y Patrones de Diseño

### Arquitectura en Capas

La aplicación implementa una arquitectura en capas clara y bien definida:

1. **Capa de Negocio (DesinstalaPhotoshop.Core)**: Contiene toda la lógica de negocio y es independiente de la interfaz de usuario.
   - **Models**: Clases que representan los datos de la aplicación
   - **Services**: Servicios que implementan la funcionalidad principal
   - **Utilities**: Clases de utilidad para operaciones comunes

2. **Capa de Presentación (DesinstalaPhotoshop.UI)**: Contiene la interfaz gráfica de usuario.
   - **Forms**: Formularios de Windows Forms
   - **Resources**: Recursos como imágenes, iconos y cadenas localizadas

Esta separación permite:
- Reutilizar la lógica de negocio en diferentes interfaces (GUI, CLI, etc.)
- Probar la lógica de negocio de forma independiente
- Mantener y evolucionar cada capa de forma independiente

### Patrones de Diseño Implementados

La aplicación implementa varios patrones de diseño que mejoran su mantenibilidad y extensibilidad:

#### 1. Patrón de Servicios

La funcionalidad principal está encapsulada en servicios especializados que son inyectados donde se necesitan. Cada servicio tiene una responsabilidad única y bien definida.

```csharp
// Inicializar servicios
_loggingService = new LoggingService();
_backupService = new BackupService(_loggingService);
_detectionService = new DetectionService(_loggingService);
_processService = new ProcessService(_loggingService);
_cleanupService = new CleanupService(_loggingService, _backupService, _processService);
_uninstallService = new UninstallService(_loggingService, _backupService, _cleanupService, _processService);
```

Beneficios:
- Separación clara de responsabilidades
- Facilita la prueba unitaria
- Mejora la mantenibilidad del código

#### 2. Patrón Asíncrono

Las operaciones potencialmente largas se implementan de forma asíncrona utilizando `async/await` para mantener la interfaz de usuario responsiva.

```csharp
private async Task RunOperationAsync(Func<IProgress<ProgressInfo>, CancellationToken, Task<OperationResult>> operation)
{
    try
    {
        // Preparar UI
        PrepareUIForOperation();

        // Crear token de cancelación
        _cts = new CancellationTokenSource();

        // Crear objeto de progreso
        var progress = new Progress<ProgressInfo>(UpdateProgress);

        // Ejecutar operación
        var result = await operation(progress, _cts.Token);

        // Mostrar resultado
        if (result.Success)
        {
            LogSuccess($"Operación completada correctamente: {result.Details}");
        }
        else
        {
            LogError($"Error en la operación: {result.ErrorMessage}");
            if (!string.IsNullOrEmpty(result.Details))
            {
                LogInfo($"Detalles: {result.Details}");
            }
        }
    }
    catch (OperationCanceledException)
    {
        LogWarning("Operación cancelada por el usuario.");
    }
    // ...
}
```

Beneficios:
- Interfaz de usuario siempre responsiva
- Posibilidad de cancelar operaciones largas
- Mejor experiencia de usuario

#### 3. Patrón de Observador

Se utiliza a través del mecanismo de `IProgress<T>` para reportar el progreso de operaciones largas a la interfaz de usuario, y mediante eventos para notificar cambios en el estado de la aplicación.

```csharp
// Suscribirse a eventos de log
_loggingService.LogAdded += LoggingService_LogAdded;

// Crear objeto de progreso
var progress = new Progress<ProgressInfo>(UpdateProgress);
```

Beneficios:
- Desacoplamiento entre los componentes
- Actualización en tiempo real de la interfaz de usuario
- Mejor experiencia de usuario

#### 4. Patrón de Fachada

Cada servicio actúa como una fachada que oculta la complejidad de las operaciones que realiza, proporcionando una interfaz simple y coherente.

```csharp
// Detectar instalaciones
_detectedInstallations = await _detectionService.DetectInstallationsAsync(progress, token);
```

Beneficios:
- Simplifica el uso de subsistemas complejos
- Reduce el acoplamiento entre componentes
- Mejora la legibilidad del código

#### 5. Patrón de Estrategia

Se utilizan diferentes estrategias para operaciones como la detección de instalaciones o la limpieza de residuos, permitiendo seleccionar la más adecuada en cada caso.

```csharp
// Método 1: Buscar en programas instalados (incluye WMI/MSI)
var fromPrograms = DetectFromInstalledPrograms();

// Método 2: Buscar en el registro de Windows
var fromRegistry = DetectFromRegistry();

// Método 3: Buscar en el sistema de archivos
var fromFileSystem = DetectFromFileSystem();
```

Beneficios:
- Flexibilidad para cambiar algoritmos en tiempo de ejecución
- Facilita la extensión con nuevas estrategias
- Mejora la mantenibilidad del código

## Manejo de Errores y Excepciones

### Manejo Global de Excepciones

La aplicación implementa un sistema de manejo global de excepciones que captura y registra todas las excepciones no controladas, evitando que la aplicación se cierre inesperadamente.

```csharp
// Agregar manejador de excepciones no controladas
Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

// Manejador de excepciones de hilo
private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
{
    HandleException(e.Exception, "Error en hilo de UI");
}

// Manejador de excepciones no controladas
private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    HandleException(e.ExceptionObject as Exception, "Error no controlado");
}

// Método común para manejar excepciones
private static void HandleException(Exception ex, string title)
{
    if (ex == null) return;

    string errorMessage = $"{title}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
    MessageBox.Show(errorMessage, "Error crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);

    // Guardar el error en un archivo
    string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DesinstalaPhotoshop_Error.log");
    File.WriteAllText(logPath, errorMessage);
}
```

### Manejo de Excepciones en Operaciones Críticas

En operaciones críticas, se implementa un manejo de excepciones específico que permite:
- Registrar el error en el log
- Informar al usuario de forma amigable
- Continuar con la operación cuando sea posible

```csharp
try
{
    // Operación crítica
}
catch (Exception ex)
{
    _logger.LogError($"Error durante la operación: {ex.Message}");
    progress?.Report(ProgressInfo.Warning("Error", $"Se produjo un error: {ex.Message}"));
    // Continuar con la siguiente operación si es posible
}
```

### Manejo de Errores en la Interfaz de Usuario

La interfaz de usuario está diseñada para ser robusta ante errores, evitando que los errores en la actualización de la UI afecten a la funcionalidad principal.

```csharp
private void LoggingService_LogAdded(object? sender, LogEventArgs e)
{
    try
    {
        if (InvokeRequired)
        {
            Invoke(new Action<LogEventArgs>(LogMessage), e);
            return;
        }

        LogMessage(e);
    }
    catch
    {
        // Ignorar errores al actualizar la UI
    }
}
```

## Optimizaciones y Rendimiento

### Operaciones Asíncronas

Todas las operaciones potencialmente largas se ejecutan de forma asíncrona para mantener la interfaz de usuario responsiva.

```csharp
private void BtnDetect_Click(object sender, EventArgs e)
{
    _ = RunOperationAsync(async (progress, token) =>
    {
        // Limpiar lista de instalaciones
        lstInstallations.Items.Clear();
        _detectedInstallations.Clear();

        // Detectar instalaciones
        _detectedInstallations = await _detectionService.DetectInstallationsAsync(progress, token);
        // ...
    });
}
```

### Cancelación de Operaciones

Se implementa un mecanismo de cancelación para todas las operaciones largas, permitiendo al usuario interrumpir la operación en cualquier momento.

```csharp
// Crear token de cancelación
_cts = new CancellationTokenSource();

// Verificar cancelación durante la operación
if (cancellationToken.IsCancellationRequested)
    return OperationResult.Failed("Operación cancelada por el usuario.");
```

### Estrategias de Eliminación de Archivos

Se implementan múltiples estrategias para la eliminación de archivos, utilizando la más adecuada según el caso:

```csharp
// Método 1: Eliminar directamente
if (TryDeleteFile(filePath))
{
    _logger.LogDebug($"Archivo eliminado (método estándar): {filePath}");
    return true;
}

// Método 2: Forzar recolección de basura y reintentar
for (int i = 0; i < maxRetries; i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();

    if (TryDeleteFile(filePath))
    {
        _logger.LogDebug($"Archivo eliminado (después de GC, intento {i + 1}): {filePath}");
        return true;
    }

    Thread.Sleep(100 * (i + 1)); // Espera incremental
}

// Método 3: Usar API nativa de Windows
if (DeleteFileNative(filePath))
{
    _logger.LogDebug($"Archivo eliminado (API nativa): {filePath}");
    return true;
}

// Método 4: Programar eliminación en el reinicio
if (ScheduleFileForDeletion(filePath))
{
    _logger.LogWarning($"Archivo programado para eliminación en el próximo reinicio: {filePath}");
    return true;
}
```

### Animación de Progreso

Se implementa una animación de progreso para operaciones que pueden tardar en mostrar avance, evitando que el usuario piense que la aplicación se ha bloqueado.

```csharp
private void AnimationTimer_Tick(object? sender, EventArgs e)
{
    try
    {
        if (!_isAnimating) return;

        _animationState = (_animationState + 1) % 4;
        string animationText = _animationState switch
        {
            0 => $"{_currentOperation} - 0%",
            1 => $"{_currentOperation} - 0% .",
            2 => $"{_currentOperation} - 0% ..",
            3 => $"{_currentOperation} - 0% ...",
            _ => $"{_currentOperation} - 0%"
        };

        lblProgress.Text = animationText;
    }
    catch
    {
        // Ignorar errores en la animación
    }
}
```

## Seguridad

### Verificación de Privilegios de Administrador

La aplicación verifica si se está ejecutando con privilegios de administrador y lo indica en la interfaz de usuario.

```csharp
// Verificar si se está ejecutando como administrador
if (AdminHelper.IsRunningAsAdmin())
{
    this.Text += " (Administrador)";
}
```

### Manejo Seguro de Permisos

Al modificar permisos de archivos y carpetas, se implementa un manejo seguro que evita problemas de seguridad.

```csharp
try
{
    // Obtener la información de seguridad actual
    var dirInfo = new DirectoryInfo(directoryPath);
    var security = dirInfo.GetAccessControl();

    // Dar control total al usuario actual
    var currentUser = WindowsIdentity.GetCurrent().User;
    if (currentUser != null)
    {
        security.AddAccessRule(new FileSystemAccessRule(
            currentUser,
            FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
            PropagationFlags.None,
            AccessControlType.Allow));

        dirInfo.SetAccessControl(security);
        _logger.LogDebug($"Permisos modificados para carpeta: {directoryPath}");
    }
}
catch (Exception ex)
{
    _logger.LogWarning($"Error al modificar permisos: {ex.Message}");
}
```

### Copias de Seguridad Automáticas

Antes de realizar operaciones potencialmente destructivas, la aplicación crea copias de seguridad automáticas que permiten restaurar el estado anterior en caso de problemas.

```csharp
// Crear copia de seguridad si se solicita
if (createBackup)
{
    progress?.Report(ProgressInfo.Running(5, "Creando copia de seguridad", "Preparando respaldo..."));
    var backupResult = await _backupService.CreateBackupForCleanupAsync(
        FilePatterns.GetCleanupFilePatterns(),
        RegistryPatterns.GetCleanupRegistryPatterns(),
        progress,
        cancellationToken);

    if (!backupResult.Success)
    {
        _logger.LogWarning($"Advertencia al crear copia de seguridad: {backupResult.ErrorMessage}");
        progress?.Report(ProgressInfo.Warning("Advertencia",
                                            $"No se pudo crear la copia de seguridad: {backupResult.ErrorMessage}"));
    }
    else
    {
        _logger.LogInfo($"Copia de seguridad creada en: {backupResult.BackupPath}");
        result.BackupPath = backupResult.BackupPath;
    }
}
```

## Interfaz de Usuario

### Tema Oscuro

La aplicación utiliza un tema oscuro para reducir la fatiga visual y proporcionar una experiencia moderna.

```csharp
Application.SetColorMode(SystemColorMode.Dark);
```

### Feedback Visual

Se proporciona feedback visual constante al usuario sobre el estado de las operaciones, mediante:
- Barra de progreso
- Mensajes en la consola con colores según el tipo de mensaje
- Iconos en la lista de instalaciones
- Animación durante operaciones largas

```csharp
private void LogInfo(string message)
{
    AppendToConsole(message, Color.White);
}

private void LogWarning(string message)
{
    AppendToConsole(message, Color.Yellow);
}

private void LogError(string message)
{
    AppendToConsole(message, Color.Red);
}

private void LogSuccess(string message)
{
    AppendToConsole(message, Color.LimeGreen);
}
```

### Estado de Botones Contextual

El estado de los botones (habilitado/deshabilitado) se actualiza según el contexto actual de la aplicación, guiando al usuario sobre las acciones disponibles.

```csharp
private void UpdateButtonsState()
{
    // Verificar si hay instalaciones principales detectadas
    bool hasMainInstallation = _detectedInstallations.Any(i => i.IsMainInstallation);

    // Verificar si hay instalaciones posibles detectadas
    bool hasPossibleMainInstallation = _detectedInstallations.Any(i => i.InstallationType == InstallationType.PossibleMainInstallation);

    // Verificar si hay residuos detectados
    bool hasResiduals = _detectedInstallations.Any(i => i.IsResidual);

    // Botón "Desinstalar": Habilitado si se detecta una instalación principal o posible
    btnUninstall.Enabled = hasMainInstallation || hasPossibleMainInstallation;

    // Botón "Limpiar Residuos": Habilitado si hay residuos y NO hay instalación principal
    btnCleanup.Enabled = hasResiduals && !hasMainInstallation;

    // Actualizar tooltips para explicar por qué los botones están deshabilitados
    if (!btnUninstall.Enabled)
    {
        toolTip.SetToolTip(btnUninstall, "No se detectaron instalaciones principales de Photoshop para desinstalar.");
    }
    else
    {
        toolTip.SetToolTip(btnUninstall, "Desinstalar las instalaciones principales de Photoshop detectadas.");
    }

    // ...
}
```

## Lecciones Aprendidas

### 1. Importancia de la Arquitectura en Capas

La separación clara entre la lógica de negocio (Core) y la interfaz de usuario (UI) ha demostrado ser fundamental para:
- Facilitar las pruebas unitarias
- Permitir cambios en la interfaz sin afectar la lógica
- Mejorar la mantenibilidad del código

### 2. Valor de la Programación Asíncrona

La implementación de operaciones asíncronas ha sido crucial para mantener la interfaz de usuario responsiva durante operaciones largas, mejorando significativamente la experiencia del usuario.

### 3. Manejo Robusto de Errores

Un sistema completo de manejo de errores, que incluye:
- Captura global de excepciones
- Registro detallado de errores
- Mensajes de error amigables para el usuario
- Capacidad de continuar después de errores no críticos

Ha demostrado ser esencial para la estabilidad y usabilidad de la aplicación.

### 4. Estrategias Múltiples para Problemas Complejos

La implementación de múltiples estrategias para problemas complejos, como la eliminación de archivos bloqueados o la detección de instalaciones, ha permitido manejar una amplia variedad de escenarios y mejorar la tasa de éxito de las operaciones.

### 5. Feedback Constante al Usuario

Proporcionar feedback constante al usuario sobre el estado de las operaciones, mediante:
- Barra de progreso
- Mensajes en la consola
- Animaciones durante operaciones largas
- Estado contextual de los botones

Ha mejorado significativamente la usabilidad de la aplicación y reducido la frustración del usuario durante operaciones largas.

### 6. Importancia de las Copias de Seguridad

La implementación de un sistema de copias de seguridad automáticas antes de operaciones potencialmente destructivas ha proporcionado una red de seguridad crucial, permitiendo recuperarse de problemas inesperados.

### 7. Valor de la Documentación Integrada

La inclusión de documentación detallada en el código, mediante comentarios XML y nombres descriptivos, ha facilitado el mantenimiento y la comprensión del código, especialmente en áreas complejas.

### 8. Profundidad en la Investigación de Residuos Específicos

La creación del informe `ResiduosDePhotoshop.md` demostró la importancia de investigar a fondo las ubicaciones específicas de archivos y claves de registro para cada aplicación objetivo. Una desinstalación verdaderamente completa requiere identificar patrones (ej. nomenclatura por versión, carpetas de usuario vs. sistema) y rastros menos obvios (ej. datos de OOBE, configuraciones de plugins UXP).

### 9. Utilidad de Herramientas de Diagnóstico Externas

Aunque la aplicación implementa su propia lógica de detección, herramientas como Process Monitor, Regshot y el propio Adobe CC Cleaner Tool son valiosas referencias para entender qué deja atrás una aplicación y para validar la efectividad de la herramienta de limpieza. La herramienta actual busca superar las limitaciones del CC Cleaner Tool.

### 10. Mejora Continua de la Interfaz de Usuario

La adopción de `CustomMsgBoxLibrary` es un paso hacia una mejor UX. Mantener una interfaz clara, informativa y estéticamente agradable es crucial para la confianza del usuario, especialmente con herramientas que realizan cambios sensibles en el sistema.

## Conclusión

La aplicación DesinstalaPhotoshop ha implementado numerosas buenas prácticas de desarrollo de software, desde una arquitectura bien diseñada hasta un manejo robusto de errores y una interfaz de usuario intuitiva. Estas prácticas han contribuido a crear una aplicación estable, mantenible y fácil de usar.

Las lecciones aprendidas durante el desarrollo de este proyecto serán valiosas para futuros desarrollos, permitiendo aplicar estas buenas prácticas desde el inicio y evitar problemas comunes.
