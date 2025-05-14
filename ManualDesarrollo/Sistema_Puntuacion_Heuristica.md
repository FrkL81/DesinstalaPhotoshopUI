# Sistema de Puntuación Heurística

Este documento detalla el sistema de puntuación heurística utilizado en la aplicación DesinstalaPhotoshop para clasificar las instalaciones detectadas como instalaciones principales o residuos.

## Objetivo del Sistema de Puntuación

El sistema de puntuación heurística tiene como objetivo principal determinar con precisión si una instalación detectada es:

1. Una **instalación principal** completa y funcional
2. Una **posible instalación principal** que requiere verificación adicional
3. **Residuos** de una instalación anterior o componentes parciales

Esta clasificación es fundamental para el correcto funcionamiento de la aplicación, ya que determina qué acciones se pueden realizar sobre cada instalación detectada.

## Criterios de Puntuación

El sistema asigna puntos positivos y negativos a cada instalación detectada según diversos criterios:

### Puntos Positivos

| Criterio | Puntos | Justificación |
|----------|--------|---------------|
| Presencia del ejecutable principal (`photoshop.exe`) | +3 | Indica una alta probabilidad de ser una instalación principal funcional |
| Desinstalador válido y existente | +3 | Las instalaciones completas siempre incluyen un desinstalador |
| Asociaciones de archivos activas | +2 | Indica que la instalación está registrada correctamente en el sistema |
| Claves de registro asociadas | +1 por cada clave relevante | Mayor número de claves indica una instalación más completa |
| Archivos de configuración | +1 | Indica que la instalación ha sido utilizada |
| Procesos en ejecución relacionados | +1 | Indica que la instalación es funcional |

### Puntos Negativos

| Criterio | Puntos | Justificación |
|----------|--------|---------------|
| Ausencia del ejecutable principal | -3 | Una instalación sin el ejecutable principal no es funcional |
| Ruta de instalación inexistente | -5 | Fuerte indicador de que es una instalación corrupta o residual |
| Ubicación en carpetas de componentes compartidos | -2 | Los componentes en Common Files suelen ser residuos |
| Desinstalador inexistente | -1 | Indica una instalación incompleta o corrupta |
| Ubicación en AppData | -2 | Indica componentes de usuario, no una instalación principal |

## Algoritmo de Clasificación

Basándose en la puntuación final, las instalaciones se clasifican en tres categorías:

1. **Instalación Principal** (puntuación ≥ 5 Y tiene ejecutable principal Y ubicación existe)
   - Se marca como `IsMainInstallation = true`
   - Se marca como `IsResidual = false`
   - Se habilita el botón "Desinstalar" para esta instalación

2. **Posible Instalación Principal** (puntuación entre 3 y 4 Y ubicación existe)
   - Se marca como `IsMainInstallation = false`
   - Se marca como `IsResidual = false`
   - Se habilita el botón "Desinstalar" para esta instalación, pero con advertencia

3. **Residuos** (puntuación < 3 O ubicación no existe)
   - Se marca como `IsMainInstallation = false`
   - Se marca como `IsResidual = true`
   - Se habilita el botón "Limpiar Residuos" para esta instalación (si no hay instalaciones principales)

## Implementación en el Código

El sistema de puntuación se implementa principalmente en dos métodos de la clase `DetectionService`:

### EnrichInstallationInfo

Este método calcula la puntuación de confianza para una instalación detectada:

```csharp
private void EnrichInstallationInfo(PhotoshopInstallation installation)
{
    int confidenceScore = 0;

    // Verificar si existe el ejecutable principal
    if (!string.IsNullOrEmpty(installation.InstallLocation))
    {
        string exePath = Path.Combine(installation.InstallLocation, "photoshop.exe");
        bool hasExecutable = File.Exists(exePath);

        if (hasExecutable)
        {
            confidenceScore += 3; // Puntos por tener el ejecutable principal
            installation.AssociatedFiles.Add(exePath);
            _logger.LogDebug($"Ejecutable principal encontrado para {installation.DisplayName}: {exePath}");
        }
        else
        {
            confidenceScore -= 3; // Penalización por no tener el ejecutable principal
            _logger.LogDebug($"Ejecutable principal no encontrado para {installation.DisplayName}");
        }
    }

    // Verificar si existe el desinstalador
    if (!string.IsNullOrEmpty(installation.UninstallString))
    {
        string uninstallerPath = installation.UninstallString.Replace("\"", "").Split(' ')[0];
        bool hasUninstaller = File.Exists(uninstallerPath);

        if (hasUninstaller)
        {
            confidenceScore += 3; // Puntos por tener un desinstalador válido
            installation.AssociatedFiles.Add(uninstallerPath);
            _logger.LogDebug($"Desinstalador encontrado para {installation.DisplayName}: {uninstallerPath}");
        }
        else
        {
            confidenceScore -= 1; // Penalización por tener un desinstalador inexistente
            _logger.LogDebug($"Desinstalador no encontrado para {installation.DisplayName}: {uninstallerPath}");
        }
    }

    // Verificar asociaciones de archivos
    try
    {
        bool hasFileAssociations = CheckFileAssociations(installation.Version);
        installation.HasFileAssociations = hasFileAssociations;

        if (hasFileAssociations)
        {
            confidenceScore += 2; // Puntos por tener asociaciones de archivos
        }
    }
    catch (Exception ex)
    {
        _logger.LogDebug($"Error al verificar asociaciones de archivos: {ex.Message}");
    }

    // Verificar procesos en ejecución
    try
    {
        var processes = System.Diagnostics.Process.GetProcesses();
        bool hasRunningProcess = processes.Any(p =>
            p.ProcessName.Contains("Photoshop", StringComparison.OrdinalIgnoreCase) ||
            p.ProcessName.Contains("AdobeIPC", StringComparison.OrdinalIgnoreCase));

        if (hasRunningProcess)
        {
            confidenceScore += 1; // Puntos por tener procesos en ejecución
        }
    }
    catch (Exception ex)
    {
        _logger.LogDebug($"Error al verificar procesos en ejecución: {ex.Message}");
    }

    // Guardar la puntuación de confianza
    installation.ConfidenceScore = confidenceScore;

    // Verificar si la instalación parece estar corrupta o si las rutas no existen
    bool locationDoesNotExist = !string.IsNullOrEmpty(installation.InstallLocation) && !Directory.Exists(installation.InstallLocation);

    // Si la ubicación no existe, aplicar una penalización adicional
    if (locationDoesNotExist)
    {
        confidenceScore -= 5; // Penalización fuerte por tener una ruta que no existe
        _logger.LogDebug($"Penalización aplicada a {installation.DisplayName} por tener una ruta que no existe: {installation.InstallLocation}");
    }

    // Verificar si está en Common Files (probablemente residuos)
    if (!string.IsNullOrEmpty(installation.InstallLocation) &&
        (installation.InstallLocation.Contains("Common Files", StringComparison.OrdinalIgnoreCase) ||
         installation.InstallLocation.Contains("AppData", StringComparison.OrdinalIgnoreCase)))
    {
        confidenceScore -= 2; // Penalización por estar en Common Files o AppData
        _logger.LogDebug($"Penalización aplicada a {installation.DisplayName} por estar en Common Files o AppData");
    }

    // Actualizar la puntuación final
    installation.ConfidenceScore = confidenceScore;
    _logger.LogDebug($"Puntuación de confianza para {installation.DisplayName}: {confidenceScore}");
}
```

### ClassifyInstallation

Este método clasifica la instalación según su puntuación de confianza y otros criterios:

```csharp
private void ClassifyInstallation(PhotoshopInstallation installation)
{
    // Verificar si la instalación tiene el ejecutable principal
    bool hasMainExecutable = installation.AssociatedFiles.Any(f =>
        f.EndsWith("photoshop.exe", StringComparison.OrdinalIgnoreCase));

    // Verificar si la instalación tiene un desinstalador válido
    bool hasValidUninstaller = !string.IsNullOrEmpty(installation.UninstallString) &&
                              File.Exists(installation.UninstallString.Replace("\"", "").Split(' ')[0]);

    // Verificar si la ubicación de instalación existe
    bool locationExists = !string.IsNullOrEmpty(installation.InstallLocation) &&
                         Directory.Exists(installation.InstallLocation);

    // Clasificar según los criterios
    if (installation.ConfidenceScore >= 5 && locationExists && hasMainExecutable)
    {
        installation.InstallationType = InstallationType.MainInstallation;
        installation.IsMainInstallation = true;
        installation.IsResidual = false;
        _logger.LogInfo($"Clasificada como instalación principal: {installation.DisplayName}");
    }
    else if (installation.ConfidenceScore >= 3 && locationExists)
    {
        installation.InstallationType = InstallationType.PossibleMainInstallation;
        installation.IsMainInstallation = false;
        installation.IsResidual = false;
        _logger.LogInfo($"Clasificada como posible instalación principal: {installation.DisplayName}");
    }
    else
    {
        installation.InstallationType = InstallationType.Residual;
        installation.IsMainInstallation = false;
        installation.IsResidual = true;
        _logger.LogInfo($"Clasificada como residuos: {installation.DisplayName}");
    }
}
```

## Impacto en la Interfaz de Usuario

La clasificación de instalaciones tiene un impacto directo en la interfaz de usuario:

1. Las instalaciones se muestran con diferentes iconos según su tipo:
   - ⚡ para instalaciones principales
   - ✔️ para posibles instalaciones principales
   - 🗑️ para residuos

2. El estado de los botones se actualiza según las instalaciones detectadas:
   - El botón "Desinstalar" solo se habilita si hay instalaciones principales o posibles
   - El botón "Limpiar Residuos" solo se habilita si hay residuos y NO hay instalaciones principales

3. Se muestran tooltips explicativos cuando los botones están deshabilitados, indicando por qué no se pueden realizar ciertas acciones.

## Criterios de Puntuación Adicionales (Propuestos)

Basándose en el análisis detallado de `ResiduosDePhotoshop.md`, se proponen los siguientes criterios adicionales para mejorar la precisión del sistema de puntuación:

### Criterios Propuestos

| Criterio | Puntos | Justificación |
|----------|--------|---------------|
| Presencia de carpetas de configuración de plugins UXP | +1 | La presencia de datos en `%AppData%\Adobe\UXP\PluginsStorage\PHSP\[versión_numérica_photoshop]\` indica uso activo de la aplicación |
| Presencia de carpetas OOBE o SLStore intactas sin otros componentes | -1 | Si se detectan solo carpetas de licenciamiento (`%LocalAppData%\Adobe\OOBE\` o `C:\ProgramData\Adobe\SLStore_v1\`) sin otros componentes clave, probablemente son residuos |
| Presencia de archivos de caché de fuentes | +1 | La existencia de `CT Font Cache` indica que la aplicación ha sido utilizada |
| Presencia de archivos de autorecuperación recientes | +2 | Archivos `.psb` en la carpeta `AutoRecover` indican uso reciente y activo |

### Implementación Conceptual

```csharp
// Ejemplo conceptual para UXP
string uxpDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                               "Adobe", "UXP", "PluginsStorage", "PHSP", installation.NumericVersion, "External");
if (Directory.Exists(uxpDataPath) && Directory.GetDirectories(uxpDataPath).Length > 0)
{
    confidenceScore += 1; // Puntos por tener datos de plugins UXP
    _logger.LogDebug($"Datos de plugins UXP encontrados para {installation.DisplayName}");
}

// Ejemplo conceptual para OOBE/SLStore
string oobePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                             "Adobe", "OOBE");
string slStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                "Adobe", "SLStore_v1");

bool hasOnlyLicensingData = (Directory.Exists(oobePath) || Directory.Exists(slStorePath)) &&
                           !hasMainExecutable && !hasValidUninstaller;
if (hasOnlyLicensingData)
{
    confidenceScore -= 1; // Penalización por tener solo datos de licenciamiento
    _logger.LogDebug($"Penalización aplicada a {installation.DisplayName} por tener solo datos de licenciamiento");
}
```

## Conclusión

El sistema de puntuación heurística es un componente crítico de la aplicación DesinstalaPhotoshop, ya que permite distinguir con precisión entre instalaciones principales y residuos. Esta distinción es fundamental para guiar al usuario en el proceso de desinstalación y limpieza, evitando acciones incorrectas que podrían dejar el sistema en un estado inconsistente.

La implementación actual del sistema ha demostrado ser efectiva en la mayoría de los escenarios, pero podría mejorarse en futuras versiones con los criterios adicionales propuestos o ajustes en la ponderación de los criterios existentes. El análisis detallado de residuos en `ResiduosDePhotoshop.md` proporciona una base sólida para estas mejoras.
