# Código Fuente de Métodos Clave

Este documento contiene el código fuente completo y comentado de los métodos más importantes del proyecto DesinstalaPhotoshop, organizados por funcionalidad. Estos métodos son los que se han descrito en el documento `06_Arquitectura_Metodos_Lista.md`.

## Estructura del Documento

El código fuente se presenta organizado en las siguientes secciones:

1. **Detección de Instalaciones**: Métodos relacionados con la detección de instalaciones de Photoshop.
2. **Limpieza de Residuos**: Métodos para eliminar archivos, carpetas y entradas de registro residuales.
3. **Gestión de Registro**: Métodos para acceder, modificar y eliminar claves y valores del registro.
4. **Sistema de Archivos**: Métodos para operaciones avanzadas con archivos y carpetas.
5. **Copias de Seguridad**: Métodos para crear y restaurar copias de seguridad.
6. **Interfaz de Usuario**: Métodos clave de la interfaz gráfica.
7. **Generación de Scripts**: Métodos para generar scripts de limpieza.

## Detección de Instalaciones

Esta sección contiene los métodos relacionados con la detección de instalaciones de Photoshop en el sistema.

### DetectionService.DetectInstallationsAsync

Este método es el punto de entrada principal para la detección de instalaciones de Photoshop. Implementa múltiples estrategias de detección y clasifica las instalaciones encontradas.

```csharp
/// <summary>
/// Detecta instalaciones de Adobe Photoshop en el sistema.
/// </summary>
/// <param name="progress">Objeto para reportar el progreso de la operación.</param>
/// <param name="cancellationToken">Token para cancelar la operación.</param>
/// <returns>Lista de instalaciones detectadas.</returns>
public async Task<List<PhotoshopInstallation>> DetectInstallationsAsync(
    IProgress<ProgressInfo>? progress = null,
    CancellationToken cancellationToken = default)
{
    _logger.LogInfo("Iniciando detección de instalaciones de Photoshop...");
    progress?.Report(ProgressInfo.Running(0, "Detectando instalaciones de Photoshop", "Iniciando..."));

    var installations = new List<PhotoshopInstallation>();

    #region Método 1: Detección desde programas instalados
    // Este método busca instalaciones registradas en Windows como programas instalados
    // utilizando WMI y el registro de Windows (Uninstall keys)
    await Task.Run(() =>
    {
        try
        {
            var fromPrograms = DetectFromInstalledPrograms();
            installations.AddRange(fromPrograms);
            _logger.LogInfo($"Detectadas {fromPrograms.Count} instalaciones desde programas instalados.");
            progress?.Report(ProgressInfo.Running(30, "Detectando instalaciones de Photoshop",
                                                $"Encontradas {fromPrograms.Count} instalaciones en programas instalados."));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al detectar instalaciones desde programas instalados: {ex.Message}");
        }
    }, cancellationToken);

    if (cancellationToken.IsCancellationRequested)
        return installations;
    #endregion

    #region Método 2: Detección desde el registro de Windows
    // Este método busca claves específicas de Adobe Photoshop en el registro
    // que podrían no estar registradas como programas instalados
    await Task.Run(() =>
    {
        try
        {
            var fromRegistry = DetectFromRegistry();

            // Filtrar duplicados (instalaciones ya detectadas por el método anterior)
            var newFromRegistry = fromRegistry
                .Where(r => !installations.Any(i =>
                    i.DisplayName == r.DisplayName &&
                    i.InstallLocation == r.InstallLocation))
                .ToList();

            installations.AddRange(newFromRegistry);
            _logger.LogInfo($"Detectadas {newFromRegistry.Count} instalaciones adicionales desde el registro.");
            progress?.Report(ProgressInfo.Running(60, "Detectando instalaciones de Photoshop",
                                                $"Encontradas {newFromRegistry.Count} instalaciones adicionales en el registro."));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al detectar instalaciones desde el registro: {ex.Message}");
        }
    }, cancellationToken);

    if (cancellationToken.IsCancellationRequested)
        return installations;
    #endregion

    #region Método 3: Detección desde el sistema de archivos
    // Este método busca carpetas y archivos en ubicaciones comunes
    // donde Photoshop podría estar instalado o haber dejado residuos
    await Task.Run(() =>
    {
        try
        {
            // DetectFromFileSystem busca en las siguientes ubicaciones:
            // 1. Directorios de instalación principal:
            //    - C:\Program Files\Adobe\Adobe Photoshop [versión]
            //    - C:\Program Files (x86)\Adobe\Adobe Photoshop [versión]
            // 2. Configuraciones de usuario:
            //    - %AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Adobe Photoshop [versión] Settings\
            //    - %AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Presets\
            // 3. Archivos temporales y de caché:
            //    - %LOCALAPPDATA%\Temp\ (patrones Photoshop Temp*, ~PST*.tmp)
            //    - %AppData%\Roaming\Adobe\Adobe Photoshop [versión]\CT Font Cache
            //    - %AppData%\Adobe\CameraRaw\Cache\
            // 4. Plugins y Extensiones (CEP/UXP):
            //    - %AppData%\Adobe\CEP\extensions\
            //    - %AppData%\Adobe\UXP\Plugins\External\
            //    - %AppData%\Adobe\UXP\PluginsStorage\PHSP\[versión_numérica_photoshop]\
            // 5. Datos de OOBE y SLStore:
            //    - %LocalAppData%\Adobe\OOBE\
            //    - C:\ProgramData\Adobe\SLStore_v1\
            var fromFileSystem = DetectFromFileSystem();

            // Filtrar duplicados
            var newFromFileSystem = fromFileSystem
                .Where(f => !installations.Any(i =>
                    i.InstallLocation == f.InstallLocation))
                .ToList();

            installations.AddRange(newFromFileSystem);
            _logger.LogInfo($"Detectadas {newFromFileSystem.Count} instalaciones adicionales desde el sistema de archivos.");
            progress?.Report(ProgressInfo.Running(90, "Detectando instalaciones de Photoshop",
                                                $"Encontradas {newFromFileSystem.Count} instalaciones adicionales en el sistema de archivos."));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al detectar instalaciones desde el sistema de archivos: {ex.Message}");
        }
    }, cancellationToken);
    #endregion

    #region Enriquecimiento y clasificación de instalaciones
    // Añadir información adicional a cada instalación y calcular su puntuación de confianza
    await Task.Run(() =>
    {
        foreach (var installation in installations)
        {
            try
            {
                EnrichInstallationInfo(installation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al enriquecer información de instalación {installation.DisplayName}: {ex.Message}");
            }
        }
    }, cancellationToken);

    // Filtrar instalaciones con puntuación muy baja (probablemente falsos positivos)
    // También filtrar instalaciones que no tienen claves de registro ni archivos asociados (probablemente no son de Photoshop)
    var filteredInstallations = installations
        .Where(i => i.ConfidenceScore > -5 &&
                   (i.AssociatedRegistryKeys.Count > 0 || i.AssociatedFiles.Count > 0))
        .ToList();

    // Clasificar las instalaciones según su puntuación de confianza
    foreach (var installation in filteredInstallations)
    {
        ClassifyInstallation(installation);
    }
    #endregion

    _logger.LogInfo($"Detección completada. Encontradas {filteredInstallations.Count} instalaciones válidas.");
    progress?.Report(ProgressInfo.Success(100, "Detección completada",
                                        $"Se encontraron {filteredInstallations.Count} instalaciones de Photoshop."));

    return filteredInstallations;
}
```

### DetectionService.ClassifyInstallation

Este método clasifica una instalación de Photoshop según su puntuación de confianza y otros criterios.

```csharp
/// <summary>
/// Clasifica una instalación de Photoshop según su puntuación de confianza.
/// </summary>
/// <param name="installation">Instalación a clasificar.</param>
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

### DetectionService.EnrichInstallationInfo

Este método enriquece la información de una instalación detectada, calculando su puntuación de confianza y recopilando información adicional.

```csharp
/// <summary>
/// Enriquece la información de una instalación detectada.
/// </summary>
/// <param name="installation">Instalación a enriquecer.</param>
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

## Limpieza de Residuos

Esta sección contiene los métodos relacionados con la limpieza de archivos, carpetas y entradas de registro residuales de Photoshop.

### CleanupService.CleanupAsync

Este método es el punto de entrada principal para la limpieza de residuos de Photoshop. Coordina todas las operaciones de limpieza.

```csharp
/// <summary>
/// Limpia residuos de Adobe Photoshop del sistema.
/// </summary>
/// <param name="createBackup">Indica si se debe crear una copia de seguridad antes de limpiar.</param>
/// <param name="whatIf">Indica si se debe simular la limpieza sin realizar cambios reales.</param>
/// <param name="progress">Objeto para reportar el progreso de la operación.</param>
/// <param name="cancellationToken">Token para cancelar la operación.</param>
/// <param name="installations">Lista opcional de instalaciones detectadas para limpiar sus claves de registro asociadas.</param>
/// <returns>Resultado de la operación.</returns>
public async Task<OperationResult> CleanupAsync(
    bool createBackup = true,
    bool whatIf = false,
    IProgress<ProgressInfo>? progress = null,
    CancellationToken cancellationToken = default,
    List<PhotoshopInstallation>? installations = null)
{
    _logger.LogInfo($"Iniciando limpieza de residuos de Photoshop (WhatIf: {whatIf})...");
    progress?.Report(ProgressInfo.Running(0, "Limpieza de residuos", "Iniciando..."));

    var result = new OperationResult { Success = true };

    try
    {
        #region Creación de copia de seguridad
        // Crear copia de seguridad si se solicita para permitir la restauración en caso de problemas
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

        if (cancellationToken.IsCancellationRequested)
            return OperationResult.Failed("Operación cancelada por el usuario.");
        #endregion

        try
        {
            #region Detención de procesos y servicios
            // Detener procesos relacionados con Adobe para evitar bloqueos de archivos
            progress?.Report(ProgressInfo.Running(10, "Deteniendo procesos", "Buscando procesos de Adobe en ejecución..."));
            var processResult = await _processService.StopAdobeProcessesAsync(
                whatIf,
                progress,
                cancellationToken);

            if (!processResult.Success)
            {
                _logger.LogWarning($"Advertencia al detener procesos: {processResult.ErrorMessage}");
                progress?.Report(ProgressInfo.Warning("Advertencia",
                                                    $"Algunos procesos no pudieron detenerse: {processResult.ErrorMessage}"));
            }

            if (cancellationToken.IsCancellationRequested)
                return OperationResult.Failed("Operación cancelada por el usuario.");

            // Detener servicios relacionados con Adobe
            progress?.Report(ProgressInfo.Running(20, "Deteniendo servicios", "Buscando servicios de Adobe..."));
            var servicesResult = await StopAdobeServicesAsync(whatIf, progress, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return OperationResult.Failed("Operación cancelada por el usuario.");
            #endregion

            #region Desinstalación de productos MSI
            // Desinstalar productos MSI relacionados con Adobe Photoshop
            progress?.Report(ProgressInfo.Running(30, "Desinstalando productos MSI", "Buscando productos de Adobe instalados..."));
            var msiResult = await UninstallMsiProductsAsync(whatIf, progress, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return OperationResult.Failed("Operación cancelada por el usuario.");
            #endregion

            #region Limpieza de archivos y registro
            // Limpiar archivos residuales en múltiples ubicaciones
            // CleanupFilesAsync elimina los siguientes tipos de residuos:
            // 1. Archivos de configuración de usuario en %AppData%\Adobe\Adobe Photoshop [versión]\
            // 2. Archivos de caché (fuentes, Camera Raw) en varias ubicaciones
            // 3. Archivos temporales (~PST*.tmp) en %LOCALAPPDATA%\Temp\
            // 4. Plugins y extensiones (CEP/UXP) en varias ubicaciones
            // 5. Archivos de OOBE y datos de licencia en %LocalAppData%\Adobe\OOBE\ y C:\ProgramData\Adobe\SLStore_v1\
            // 6. Logs y reportes de errores en varias ubicaciones
            progress?.Report(ProgressInfo.Running(40, "Limpiando archivos", "Eliminando archivos residuales..."));
            var filesResult = await CleanupFilesAsync(whatIf, progress, cancellationToken);

            if (!filesResult.Success)
            {
                _logger.LogWarning($"Advertencia al limpiar archivos: {filesResult.ErrorMessage}");
                progress?.Report(ProgressInfo.Warning("Advertencia",
                                                    $"Algunos archivos no pudieron eliminarse: {filesResult.ErrorMessage}"));
            }

            if (cancellationToken.IsCancellationRequested)
                return OperationResult.Failed("Operación cancelada por el usuario.");

            // Limpiar entradas de registro relacionadas con Photoshop
            // CleanupRegistryAsync elimina las siguientes claves de registro:
            // 1. Configuraciones de aplicación (Máquina): HKLM\SOFTWARE\Adobe\Photoshop\[versión_numérica]
            // 2. Configuraciones de Usuario: HKCU\SOFTWARE\Adobe\Photoshop\[versión_numérica]
            // 3. Información de Desinstalación: HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\[GUID]
            // 4. Asociaciones de Archivos: HKCR\.psd, HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.psd\
            // 5. Comandos de apertura: HKCR\Applications\Photoshop.exe\shell\open\command
            // 6. Claves de componentes COM y extensiones de shell
            progress?.Report(ProgressInfo.Running(50, "Limpiando registro", "Eliminando entradas de registro residuales..."));
            var registryResult = await CleanupRegistryAsync(whatIf, progress, cancellationToken, installations);

            if (!registryResult.Success)
            {
                _logger.LogWarning($"Advertencia al limpiar registro: {registryResult.ErrorMessage}");
                progress?.Report(ProgressInfo.Warning("Advertencia",
                                                    $"Algunas entradas de registro no pudieron eliminarse: {registryResult.ErrorMessage}"));
            }

            if (cancellationToken.IsCancellationRequested)
                return OperationResult.Failed("Operación cancelada por el usuario.");
            #endregion

            #region Procesamiento especial y programación de eliminación
            // Procesamiento especial para carpetas en Common Files que son más difíciles de eliminar
            progress?.Report(ProgressInfo.Running(55, "Procesando carpetas en Common Files", "Aplicando métodos especializados..."));
            await ProcessCommonFilesDirectoriesAsync(whatIf, progress, cancellationToken);

            // Programar eliminación de archivos persistentes al reinicio del sistema
            progress?.Report(ProgressInfo.Running(60, "Programando eliminación al reinicio", "Marcando archivos persistentes..."));
            var pendingRebootResult = await ScheduleFilesForDeletionAsync(whatIf, progress, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return OperationResult.Failed("Operación cancelada por el usuario.");

            // Combinar resultados y determinar si se requiere reinicio
            result.PendingRebootFiles.AddRange(pendingRebootResult.PendingRebootFiles);
            result.RequiresReboot = pendingRebootResult.PendingRebootFiles.Count > 0;
            #endregion

            // Finalizar y reportar éxito
            progress?.Report(ProgressInfo.Success(100, "Limpieza completada",
                                                "Se han eliminado los residuos de Photoshop."));
            _logger.LogInfo("Limpieza de residuos completada.");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la limpieza: {ex.Message}");
            return OperationResult.Failed($"Error durante la limpieza: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error general durante la limpieza: {ex.Message}");
        return OperationResult.Failed($"Error general durante la limpieza: {ex.Message}");
    }
}
```

### CleanupService.ScheduleFilesForDeletionAsync

Este método programa la eliminación de archivos persistentes al reiniciar el sistema, utilizando la API nativa de Windows.

```csharp
/// <summary>
/// Programa la eliminación de archivos persistentes al reiniciar el sistema.
/// </summary>
/// <param name="whatIf">Indica si se debe simular la operación.</param>
/// <param name="progress">Objeto para reportar progreso.</param>
/// <param name="cancellationToken">Token para cancelar la operación.</param>
/// <returns>Resultado de la operación.</returns>
private async Task<OperationResult> ScheduleFilesForDeletionAsync(
    bool whatIf,
    IProgress<ProgressInfo>? progress,
    CancellationToken cancellationToken)
{
    _logger.LogInfo("Programando eliminación de archivos persistentes al reiniciar el sistema...");
    var result = new OperationResult { Success = true };

    await Task.Run(() =>
    {
        try
        {
            // Obtener lista de archivos persistentes
            var persistentFiles = new List<string>();

            // Verificar archivos residuales conocidos
            foreach (var file in FilePatterns.GetPersistentFilePatterns())
            {
                try
                {
                    // Expandir patrones con comodines
                    if (file.Contains("*"))
                    {
                        string directory = Path.GetDirectoryName(file) ?? "";
                        string pattern = Path.GetFileName(file);

                        if (Directory.Exists(directory))
                        {
                            var matchingFiles = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                            persistentFiles.AddRange(matchingFiles);
                        }
                    }
                    else if (File.Exists(file) || Directory.Exists(file))
                    {
                        persistentFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al procesar patrón de archivo persistente {file}: {ex.Message}");
                }
            }

            // Programar eliminación de archivos persistentes
            if (persistentFiles.Count > 0)
            {
                _logger.LogInfo($"Se encontraron {persistentFiles.Count} archivos persistentes para programar eliminación al reinicio.");
                progress?.Report(ProgressInfo.Running(60, "Programando eliminación al reinicio",
                                                    $"Marcando {persistentFiles.Count} archivos persistentes..."));

                if (!whatIf)
                {
                    int processed = 0;
                    int total = persistentFiles.Count;
                    bool scheduled = false;

                    foreach (var file in persistentFiles)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        processed++;
                        int percentage = 60 + (processed * 10 / total);
                        progress?.Report(ProgressInfo.Running(percentage, "Programando eliminación al reinicio",
                                                            $"Procesando {processed}/{total}: {Path.GetFileName(file)}"));

                        try
                        {
                            if (File.Exists(file))
                            {
                                if (FileSystemHelper.ScheduleFileForDeletion(file))
                                {
                                    _logger.LogInfo($"Archivo persistente marcado para eliminación al reinicio: {file}");
                                    result.PendingRebootFiles.Add(file);
                                }
                            }
                            else if (Directory.Exists(file))
                            {
                                if (FileSystemHelper.ScheduleDirectoryForDeletion(file))
                                {
                                    _logger.LogInfo($"Carpeta persistente marcada para eliminación al reinicio: {file}");
                                    result.PendingRebootFiles.Add(file);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al marcar archivo persistente para eliminación: {ex.Message}");
                        }
                    }
                }
                else
                {
                    _logger.LogInfo("Modo WhatIf: No se programarán archivos para eliminación al reinicio.");
                    progress?.Report(ProgressInfo.Running(70, "Modo WhatIf",
                                                        "Simulando programación de eliminación al reinicio..."));
                }
            }
            else
            {
                _logger.LogInfo("No se encontraron archivos persistentes para programar eliminación al reinicio.");
                progress?.Report(ProgressInfo.Running(70, "Programando eliminación al reinicio",
                                                    "No se encontraron archivos persistentes."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al programar eliminación de archivos persistentes: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = $"Error al programar eliminación de archivos persistentes: {ex.Message}";
        }
    }, cancellationToken);

    return result;
}
```

## Sistema de Archivos

Esta sección contiene los métodos relacionados con operaciones avanzadas con archivos y carpetas, especialmente aquellos que son difíciles de eliminar.

### FileSystemHelper.ScheduleFileForDeletion

Este método programa la eliminación de un archivo al reiniciar el sistema, utilizando la API nativa de Windows.

```csharp
/// <summary>
/// Programa la eliminación de un archivo en el próximo reinicio.
/// </summary>
/// <param name="filePath">Ruta del archivo a eliminar.</param>
/// <param name="whatIf">Indica si se debe simular la operación.</param>
/// <returns>true si la operación se completó correctamente, false en caso contrario.</returns>
public static bool ScheduleFileForDeletion(string filePath, bool whatIf = false)
{
    try
    {
        _logger.LogDebug($"Programando archivo para eliminación en el próximo reinicio: {filePath}");

        if (whatIf)
        {
            _logger.LogInfo($"[WhatIf] Se programaría el archivo para eliminación: {filePath}");
            return true;
        }

        return NativeMethods.MoveFileEx(
            filePath,
            null,
            NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
    }
    catch (Exception ex)
    {
        _logger.LogWarning($"Error al programar archivo para eliminación: {ex.Message}");
        return false;
    }
}
```

### FileSystemHelper.ScheduleDirectoryForDeletion

Este método programa la eliminación de una carpeta y su contenido al reiniciar el sistema.

```csharp
/// <summary>
/// Programa la eliminación de una carpeta en el próximo reinicio.
/// </summary>
/// <param name="directoryPath">Ruta de la carpeta a eliminar.</param>
/// <param name="whatIf">Indica si se debe simular la operación.</param>
/// <returns>true si la operación se completó correctamente, false en caso contrario.</returns>
public static bool ScheduleDirectoryForDeletion(string directoryPath, bool whatIf = false)
{
    try
    {
        _logger.LogDebug($"Programando carpeta para eliminación en el próximo reinicio: {directoryPath}");

        if (whatIf)
        {
            _logger.LogInfo($"[WhatIf] Se programaría la carpeta para eliminación: {directoryPath}");
            return true;
        }

        // Verificar si es una carpeta de Common Files (tratamiento especial)
        bool isCommonFilesPath = directoryPath.Contains("Common Files\\Adobe", StringComparison.OrdinalIgnoreCase) ||
                               directoryPath.Contains("Common Files/Adobe", StringComparison.OrdinalIgnoreCase);

        if (isCommonFilesPath)
        {
            _logger.LogInfo($"Aplicando tratamiento especial para carpeta de Common Files: {directoryPath}");
        }

        // Programar la eliminación de todos los archivos dentro de la carpeta
        try
        {
            foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    // Intentar quitar atributos de solo lectura primero
                    FileAttributes attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                    }

                    // Programar eliminación
                    NativeMethods.MoveFileEx(
                        file,
                        null,
                        NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                    if (isCommonFilesPath)
                    {
                        _logger.LogDebug($"Archivo de Common Files programado para eliminación: {file}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al programar archivo para eliminación: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error al enumerar archivos para eliminación: {ex.Message}");
        }

        // Programar la eliminación de todas las subcarpetas (de abajo hacia arriba)
        try
        {
            foreach (string subDir in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                                              .OrderByDescending(d => d.Length))
            {
                try
                {
                    NativeMethods.MoveFileEx(
                        subDir,
                        null,
                        NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                    if (isCommonFilesPath)
                    {
                        _logger.LogDebug($"Subcarpeta de Common Files programada para eliminación: {subDir}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al programar subcarpeta para eliminación: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error al enumerar subcarpetas para eliminación: {ex.Message}");
        }

        // Programar la eliminación de la carpeta principal
        bool result = NativeMethods.MoveFileEx(
            directoryPath,
            null,
            NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

        if (result && isCommonFilesPath)
        {
            _logger.LogInfo($"Carpeta principal de Common Files programada para eliminación: {directoryPath}");
        }

        return result;
    }
    catch (Exception ex)
    {
        _logger.LogWarning($"Error al programar carpeta para eliminación: {ex.Message}");
        return false;
    }
}
```

### FileSystemHelper.ForceDeleteCommonFilesDirectory

Este método aplica técnicas especializadas para eliminar carpetas persistentes en Common Files.

```csharp
/// <summary>
/// Método especializado para eliminar carpetas en Common Files que son persistentes.
/// </summary>
/// <param name="directoryPath">Ruta de la carpeta a eliminar.</param>
/// <param name="whatIf">Indica si se debe simular la operación.</param>
/// <returns>true si la operación se completó correctamente, false en caso contrario.</returns>
public static bool ForceDeleteCommonFilesDirectory(string directoryPath, bool whatIf = false)
{
    if (!directoryPath.Contains("Common Files", StringComparison.OrdinalIgnoreCase))
    {
        return false; // Este método es solo para carpetas en Common Files
    }

    _logger.LogInfo($"Aplicando eliminación forzada para carpeta persistente en Common Files: {directoryPath}");

    if (whatIf)
    {
        _logger.LogInfo($"[WhatIf] Se aplicaría eliminación forzada a: {directoryPath}");
        return true;
    }

    try
    {
        // 1. Intentar cambiar permisos de la carpeta para asegurar acceso total
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
            _logger.LogWarning($"Error al modificar permisos de carpeta {directoryPath}: {ex.Message}");
        }

        // 2. Intentar eliminar archivos uno por uno con múltiples métodos
        bool allFilesDeleted = true;
        try
        {
            foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    // Quitar atributos de solo lectura
                    FileAttributes attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                    }

                    // Intentar eliminar con múltiples métodos
                    if (!DeleteFileWithRetry(file, 5))
                    {
                        // Si falla, programar para eliminación al reinicio
                        if (!ScheduleFileForDeletion(file))
                        {
                            allFilesDeleted = false;
                            _logger.LogWarning($"No se pudo eliminar ni programar el archivo: {file}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    allFilesDeleted = false;
                    _logger.LogWarning($"Error al procesar archivo {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error al enumerar archivos en {directoryPath}: {ex.Message}");
        }

        // 3. Programar la eliminación de la carpeta al reinicio
        bool scheduled = ScheduleDirectoryForDeletion(directoryPath);

        return scheduled || allFilesDeleted;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error en eliminación forzada de carpeta Common Files {directoryPath}: {ex.Message}");
        return false;
    }
}
```

## Gestión de Registro

Esta sección contiene los métodos relacionados con la gestión del registro de Windows, que son fundamentales para la limpieza de residuos de Photoshop.

### RegistryHelper.DeleteRegistryKey

Este método elimina una clave del registro de Windows, con soporte para fallback a reg.exe.

```csharp
/// <summary>
/// Elimina una clave del registro de Windows.
/// </summary>
/// <param name="keyPath">Ruta completa de la clave a eliminar.</param>
/// <param name="whatIf">Indica si se debe simular la eliminación.</param>
/// <param name="useRegExe">Indica si se debe utilizar reg.exe como método alternativo.</param>
/// <returns>true si la clave se eliminó correctamente, false en caso contrario.</returns>
public static bool DeleteRegistryKey(string keyPath, bool whatIf = false, bool useRegExe = false)
{
    _logger.LogDebug($"Eliminando clave de registro: {keyPath} (WhatIf: {whatIf}, UseRegExe: {useRegExe})");

    if (whatIf)
    {
        _logger.LogInfo($"[WhatIf] Se eliminaría la clave de registro: {keyPath}");
        return true;
    }

    if (useRegExe)
    {
        return ExecuteRegDelete(keyPath);
    }

    // Dividir la ruta en partes (hive y path)
    string[] parts = keyPath.Split('\\', 2);
    if (parts.Length != 2)
    {
        _logger.LogWarning($"Formato de ruta de registro inválido: {keyPath}");
        return false;
    }

    // Determinar la base del registro
    RegistryHive hive;
    switch (parts[0].ToUpperInvariant())
    {
        case "HKEY_LOCAL_MACHINE":
        case "HKLM":
            hive = RegistryHive.LocalMachine;
            break;
        case "HKEY_CURRENT_USER":
        case "HKCU":
            hive = RegistryHive.CurrentUser;
            break;
        case "HKEY_CLASSES_ROOT":
        case "HKCR":
            hive = RegistryHive.ClassesRoot;
            break;
        case "HKEY_USERS":
        case "HKU":
            hive = RegistryHive.Users;
            break;
        case "HKEY_CURRENT_CONFIG":
        case "HKCC":
            hive = RegistryHive.CurrentConfig;
            break;
        default:
            _logger.LogWarning($"Base de registro no reconocida: {parts[0]}");
            return false;
    }

    // Separar la ruta en componentes
    string keyPath = parts[1];
    int lastBackslash = keyPath.LastIndexOf('\\');

    // Implementar reintentos
    bool success = false;
    Exception? lastException = null;

    for (int i = 0; i < 3; i++)
    {
        try
        {
            if (lastBackslash == -1)
            {
                // Es una clave de nivel superior, no se puede eliminar
                _logger.LogWarning($"No se puede eliminar una clave de nivel superior: {keyPath}");
                return false;
            }

            string parentPath = keyPath.Substring(0, lastBackslash);
            string subKeyName = keyPath.Substring(lastBackslash + 1);

            using (RegistryKey? parentKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenSubKey(parentPath, true))
            {
                if (parentKey == null)
                {
                    _logger.LogWarning($"No se pudo abrir la clave padre: {parentPath}");
                    return false;
                }

                parentKey.DeleteSubKeyTree(subKeyName, false);
                _logger.LogInfo($"Clave de registro eliminada: {keyPath}");
                success = true;
                break;
            }
        }
        catch (Exception ex)
        {
            lastException = ex;
            _logger.LogWarning($"Error al eliminar clave de registro (intento {i+1}): {ex.Message}");

            // Esperar un poco antes de reintentar
            Thread.Sleep(100 * (i + 1));
        }
    }

    // Si falló con el método estándar, intentar con reg.exe
    if (!success && !useRegExe)
    {
        _logger.LogInfo($"Intentando eliminar clave con reg.exe: {keyPath}");
        return ExecuteRegDelete(keyPath);
    }

    return success;
}
```

### RegistryHelper.ExecuteRegDelete

Este método ejecuta el comando reg.exe para eliminar una clave o valor del registro.

```csharp
/// <summary>
/// Ejecuta el comando reg.exe para eliminar una clave o valor del registro.
/// </summary>
/// <param name="keyPath">Ruta de la clave a eliminar.</param>
/// <param name="valueName">Nombre del valor a eliminar (opcional).</param>
/// <param name="whatIf">Indica si se debe simular la eliminación.</param>
/// <returns>true si la operación se completó correctamente, false en caso contrario.</returns>
public static bool ExecuteRegDelete(string keyPath, string? valueName = null, bool whatIf = false)
{
    _logger.LogDebug($"Ejecutando reg.exe para eliminar {(string.IsNullOrEmpty(valueName) ? "clave" : "valor")} de registro: {keyPath}{(string.IsNullOrEmpty(valueName) ? "" : "\\" + valueName)} (WhatIf: {whatIf})");

    if (whatIf)
    {
        _logger.LogInfo($"[WhatIf] Se ejecutaría reg.exe para eliminar {(string.IsNullOrEmpty(valueName) ? "clave" : "valor")}: {keyPath}{(string.IsNullOrEmpty(valueName) ? "" : "\\" + valueName)}");
        return true;
    }

    try
    {
        // Construir comando para eliminar clave o valor
        string regDeleteCmd = string.IsNullOrEmpty(valueName)
            ? $"reg delete \"{keyPath}\" /f"
            : $"reg delete \"{keyPath}\" /v \"{valueName}\" /f";

        // Ejecutar comando
        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {regDeleteCmd}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = System.Diagnostics.Process.Start(processStartInfo))
        {
            if (process != null)
            {
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInfo($"Comando reg.exe ejecutado correctamente: {regDeleteCmd}");
                    return true;
                }
                else
                {
                    string error = process.StandardError.ReadToEnd();
                    _logger.LogWarning($"Error al ejecutar reg.exe: {error}");
                }
            }
        }

        return false;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error al ejecutar reg.exe: {ex.Message}");
        return false;
    }
}
```

## Copias de Seguridad

Esta sección contiene los métodos relacionados con la creación y restauración de copias de seguridad antes de realizar operaciones de limpieza.

### BackupService.CreateBackupForCleanupAsync

Este método crea una copia de seguridad de archivos y claves de registro antes de realizar una operación de limpieza.

```csharp
/// <summary>
/// Crea una copia de seguridad para una operación de limpieza.
/// </summary>
/// <param name="filePatterns">Patrones de archivos a respaldar.</param>
/// <param name="registryPatterns">Patrones de registro a respaldar.</param>
/// <param name="progress">Objeto para reportar progreso.</param>
/// <param name="cancellationToken">Token para cancelar la operación.</param>
/// <returns>Resultado de la operación.</returns>
public async Task<OperationResult> CreateBackupForCleanupAsync(
    List<string> filePatterns,
    List<RegistryPattern> registryPatterns,
    IProgress<ProgressInfo>? progress = null,
    CancellationToken cancellationToken = default)
{
    _logger.LogInfo("Creando copia de seguridad para limpieza...");
    progress?.Report(ProgressInfo.Running(0, "Creando copia de seguridad", "Preparando..."));

    var result = new OperationResult { Success = true };

    try
    {
        // Crear directorio de copia de seguridad
        string backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PhotoshopBackups",
            $"Cleanup_{DateTime.Now:yyyyMMdd_HHmmss}");

        Directory.CreateDirectory(backupDir);
        _logger.LogInfo($"Directorio de copia de seguridad creado: {backupDir}");
        result.BackupPath = backupDir;

        // Crear subdirectorios
        string filesBackupDir = Path.Combine(backupDir, "Files");
        string directoriesBackupDir = Path.Combine(backupDir, "Directories");
        string registryBackupDir = Path.Combine(backupDir, "Registry");
        string metadataDir = Path.Combine(backupDir, "Metadata");

        Directory.CreateDirectory(filesBackupDir);
        Directory.CreateDirectory(directoriesBackupDir);
        Directory.CreateDirectory(registryBackupDir);
        Directory.CreateDirectory(metadataDir);

        // Lista para almacenar información de elementos respaldados
        var backupItems = new List<BackupItem>();

        // Respaldar archivos
        await Task.Run(() =>
        {
            try
            {
                int totalPatterns = filePatterns.Count;
                int processedPatterns = 0;

                foreach (var pattern in filePatterns)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    processedPatterns++;
                    int percentage = (processedPatterns * 40) / totalPatterns;
                    progress?.Report(ProgressInfo.Running(percentage, "Respaldando archivos",
                                                        $"Procesando patrón {processedPatterns}/{totalPatterns}: {pattern}"));

                    try
                    {
                        // Expandir patrones con comodines
                        if (pattern.Contains("*"))
                        {
                            string directory = Path.GetDirectoryName(pattern) ?? "";
                            string filePattern = Path.GetFileName(pattern);

                            if (Directory.Exists(directory))
                            {
                                // Respaldar archivos que coinciden con el patrón
                                foreach (var file in Directory.GetFiles(directory, filePattern, SearchOption.AllDirectories))
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                        break;

                                    var backupItem = FileSystemHelper.BackupFile(file, filesBackupDir);
                                    if (backupItem != null)
                                    {
                                        backupItems.Add(backupItem);
                                    }
                                }
                            }
                        }
                        else if (File.Exists(pattern))
                        {
                            // Respaldar archivo específico
                            var backupItem = FileSystemHelper.BackupFile(pattern, filesBackupDir);
                            if (backupItem != null)
                            {
                                backupItems.Add(backupItem);
                            }
                        }
                        else if (Directory.Exists(pattern))
                        {
                            // Respaldar directorio
                            var backupItem = FileSystemHelper.BackupDirectory(pattern, directoriesBackupDir);
                            if (backupItem != null)
                            {
                                backupItems.Add(backupItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al respaldar patrón {pattern}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al respaldar archivos: {ex.Message}");
                result.Success = false;
                result.ErrorMessage = $"Error al respaldar archivos: {ex.Message}";
            }
        }, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Operación de respaldo cancelada por el usuario.");
            return OperationResult.Failed("Operación cancelada por el usuario.");
        }

        // Respaldar claves de registro
        await Task.Run(() =>
        {
            try
            {
                int totalPatterns = registryPatterns.Count;
                int processedPatterns = 0;

                foreach (var pattern in registryPatterns)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    processedPatterns++;
                    int percentage = 40 + (processedPatterns * 40) / totalPatterns;
                    progress?.Report(ProgressInfo.Running(percentage, "Respaldando registro",
                                                        $"Procesando patrón {processedPatterns}/{totalPatterns}: {pattern.KeyPath}"));

                    try
                    {
                        // Verificar si la clave existe
                        if (RegistryHelper.KeyExists(pattern.KeyPath))
                        {
                            // Generar nombre de archivo para la copia de seguridad
                            string regFileName = $"{pattern.KeyPath.Replace('\\', '_').Replace(':', '_')}.reg";
                            string backupFilePath = Path.Combine(registryBackupDir, regFileName);

                            // Exportar clave de registro
                            bool exported = RegistryHelper.BackupRegistryKey(pattern.KeyPath, backupFilePath);

                            if (exported)
                            {
                                var fileInfo = new FileInfo(backupFilePath);
                                backupItems.Add(new BackupItem
                                {
                                    OriginalPath = pattern.KeyPath,
                                    BackupPath = backupFilePath,
                                    ItemType = CleanupItemType.Registry,
                                    Description = $"Clave de registro: {pattern.KeyPath}",
                                    Size = fileInfo.Length,
                                    BackupTime = DateTime.Now
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al respaldar clave de registro {pattern.KeyPath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al respaldar claves de registro: {ex.Message}");
                result.Success = false;
                result.ErrorMessage = $"Error al respaldar claves de registro: {ex.Message}";
            }
        }, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Operación de respaldo cancelada por el usuario.");
            return OperationResult.Failed("Operación cancelada por el usuario.");
        }

        // Guardar metadatos
        await Task.Run(() =>
        {
            try
            {
                progress?.Report(ProgressInfo.Running(90, "Guardando metadatos", "Finalizando copia de seguridad..."));

                // Crear objeto de metadatos
                var backupMetadata = new BackupMetadata
                {
                    BackupTime = DateTime.Now,
                    BackupType = BackupType.Cleanup,
                    BackupPath = backupDir,
                    ItemCount = backupItems.Count,
                    TotalSize = backupItems.Sum(i => i.Size),
                    Items = backupItems
                };

                // Guardar metadatos en archivo JSON
                string metadataFilePath = Path.Combine(metadataDir, "backup_info.json");
                string json = JsonSerializer.Serialize(backupMetadata, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(metadataFilePath, json);

                _logger.LogInfo($"Metadatos de copia de seguridad guardados en: {metadataFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar metadatos de copia de seguridad: {ex.Message}");
                result.Success = false;
                result.ErrorMessage = $"Error al guardar metadatos de copia de seguridad: {ex.Message}";
            }
        }, cancellationToken);

        // Finalizar
        if (result.Success)
        {
            progress?.Report(ProgressInfo.Success(100, "Copia de seguridad completada",
                                                $"Se respaldaron {backupItems.Count} elementos."));
            _logger.LogInfo($"Copia de seguridad completada. Se respaldaron {backupItems.Count} elementos.");
        }
        else
        {
            progress?.Report(ProgressInfo.Warning("Advertencia",
                                                $"Copia de seguridad completada con advertencias: {result.ErrorMessage}"));
            _logger.LogWarning($"Copia de seguridad completada con advertencias: {result.ErrorMessage}");
        }

        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error general durante la copia de seguridad: {ex.Message}");
        return OperationResult.Failed($"Error general durante la copia de seguridad: {ex.Message}");
    }
}
```

## Interfaz de Usuario

Esta sección contiene los métodos clave de la interfaz gráfica de usuario.

### MainForm.UpdateButtonsState

Este método actualiza el estado de los botones "Desinstalar" y "Limpiar Residuos" según la lógica requerida.

```csharp
/// <summary>
/// Actualiza el estado de los botones "Desinstalar" y "Limpiar Residuos" según la lógica requerida.
/// </summary>
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

    if (!btnCleanup.Enabled)
    {
        if (hasMainInstallation)
        {
            toolTip.SetToolTip(btnCleanup, "Primero debe desinstalar las instalaciones principales antes de limpiar residuos.");
        }
        else if (!hasResiduals)
        {
            toolTip.SetToolTip(btnCleanup, "No se detectaron residuos de Photoshop para limpiar.");
        }
    }
    else
    {
        toolTip.SetToolTip(btnCleanup, "Limpiar residuos de Photoshop detectados.");
    }

    // Actualizar el estado del botón "Generar Script"
    btnGenerarScript.Enabled = hasResiduals;

    // Actualizar el estado del botón "Restaurar"
    btnRestore.Enabled = Directory.Exists(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "PhotoshopBackups"));
}
```

### MainForm.BtnDetect_Click

Este método maneja el evento de clic en el botón "Detectar Instalaciones".

```csharp
/// <summary>
/// Maneja el evento de clic en el botón "Detectar Instalaciones".
/// </summary>
private async void BtnDetect_Click(object sender, EventArgs e)
{
    try
    {
        // Limpiar la lista de instalaciones
        lstInstallations.Items.Clear();
        _detectedInstallations.Clear();

        // Deshabilitar botones durante la detección
        SetButtonsEnabled(false);

        // Mostrar panel de progreso
        ShowProgressPanel(true);

        // Crear objeto para reportar progreso
        var progress = new Progress<ProgressInfo>(UpdateProgress);

        // Crear token de cancelación
        _cancellationTokenSource = new CancellationTokenSource();

        // Iniciar detección
        _detectedInstallations = await _detectionService.DetectInstallationsAsync(
            progress,
            _cancellationTokenSource.Token);

        // Mostrar instalaciones detectadas
        foreach (var installation in _detectedInstallations)
        {
            var item = new ListViewItem(installation.DisplayName);
            item.SubItems.Add(installation.InstallLocation ?? "Desconocida");

            // Determinar el icono según el tipo de instalación
            if (installation.IsMainInstallation)
            {
                item.ImageIndex = 0; // Icono de instalación principal
                item.ToolTipText = "Instalación principal de Photoshop";
            }
            else if (installation.InstallationType == InstallationType.PossibleMainInstallation)
            {
                item.ImageIndex = 1; // Icono de posible instalación
                item.ToolTipText = "Posible instalación principal de Photoshop";
            }
            else
            {
                item.ImageIndex = 2; // Icono de residuos
                item.ToolTipText = "Residuos de Photoshop";
            }

            // Guardar referencia a la instalación
            item.Tag = installation;

            // Añadir a la lista
            lstInstallations.Items.Add(item);
        }

        // Actualizar estado de los botones
        UpdateButtonsState();

        // Mostrar mensaje si no se encontraron instalaciones
        if (_detectedInstallations.Count == 0)
        {
            LogToConsole("No se encontraron instalaciones de Photoshop en el sistema.", LogLevel.Warning);
        }
        else
        {
            int mainCount = _detectedInstallations.Count(i => i.IsMainInstallation);
            int possibleCount = _detectedInstallations.Count(i => i.InstallationType == InstallationType.PossibleMainInstallation);
            int residualCount = _detectedInstallations.Count(i => i.IsResidual);

            LogToConsole($"Detección completada. Se encontraron:", LogLevel.Info);
            LogToConsole($"- {mainCount} instalaciones principales", LogLevel.Info);
            LogToConsole($"- {possibleCount} posibles instalaciones", LogLevel.Info);
            LogToConsole($"- {residualCount} residuos", LogLevel.Info);
        }
    }
    catch (OperationCanceledException)
    {
        LogToConsole("Operación cancelada por el usuario.", LogLevel.Warning);
    }
    catch (Exception ex)
    {
        LogToConsole($"Error durante la detección: {ex.Message}", LogLevel.Error);
    }
    finally
    {
        // Ocultar panel de progreso
        ShowProgressPanel(false);

        // Habilitar botones
        SetButtonsEnabled(true);

        // Limpiar token de cancelación
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
```

### MainForm.BtnCleanup_Click

Este método maneja el evento de clic en el botón "Limpiar Residuos".

```csharp
/// <summary>
/// Maneja el evento de clic en el botón "Limpiar Residuos".
/// </summary>
private async void BtnCleanup_Click(object sender, EventArgs e)
{
    // Verificar si se está ejecutando como administrador
    if (!AdminHelper.IsRunningAsAdmin())
    {
        // Usar CustomMsgBox en lugar de MessageBox para mantener la estética moderna
        CustomMsgBox.Show(
            prompt: "Esta operación requiere privilegios de administrador. Por favor, ejecute la aplicación como administrador.",
            title: "Privilegios insuficientes",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Warning);
        return;
    }

    // Solicitar confirmación usando CustomMsgBox para mantener la estética consistente
    var result = CustomMsgBox.Show(
        prompt: "¿Está seguro de que desea limpiar los residuos de Photoshop? Esta operación eliminará archivos y entradas de registro residuales.",
        title: "Confirmar limpieza",
        buttons: CustomMessageBoxButtons.YesNo,
        icon: CustomMessageBoxIcon.Question);

    if (result != CustomDialogResult.Yes) // Usar CustomDialogResult en lugar de DialogResult
        return;

    try
    {
        // Mostrar opciones de limpieza
        using (var optionsForm = new CleanupOptionsForm())
        {
            if (optionsForm.ShowDialog() != DialogResult.OK)
                return;

            // Obtener opciones
            bool createBackup = optionsForm.CreateBackup;
            bool whatIf = optionsForm.WhatIf;

            // Deshabilitar botones durante la limpieza
            SetButtonsEnabled(false);

            // Mostrar panel de progreso
            ShowProgressPanel(true);

            // Crear objeto para reportar progreso
            var progress = new Progress<ProgressInfo>(UpdateProgress);

            // Crear token de cancelación
            _cancellationTokenSource = new CancellationTokenSource();

            // Iniciar limpieza
            var cleanupResult = await _cleanupService.CleanupAsync(
                createBackup,
                whatIf,
                progress,
                _cancellationTokenSource.Token,
                _detectedInstallations);

            // Verificar resultado
            if (cleanupResult.Success)
            {
                LogToConsole("Limpieza completada correctamente.", LogLevel.Success);

                // Verificar si se requiere reinicio
                if (cleanupResult.RequiresReboot)
                {
                    LogToConsole("Se requiere reiniciar el sistema para completar la limpieza.", LogLevel.Warning);

                    var rebootResult = CustomMsgBox.Show(
                        prompt: "Se requiere reiniciar el sistema para completar la limpieza. ¿Desea reiniciar ahora?",
                        title: "Reinicio requerido",
                        buttons: CustomMessageBoxButtons.YesNo,
                        icon: CustomMessageBoxIcon.Question);

                    if (rebootResult == CustomDialogResult.Yes) // Usar CustomDialogResult en lugar de DialogResult
                    {
                        // Reiniciar el sistema
                        Process.Start("shutdown", "/r /t 10 /c \"Reiniciando para completar la limpieza de Photoshop\"");
                    }
                }

                // Mostrar ruta de la copia de seguridad si se creó
                if (!string.IsNullOrEmpty(cleanupResult.BackupPath))
                {
                    LogToConsole($"Se creó una copia de seguridad en: {cleanupResult.BackupPath}", LogLevel.Info);
                }
            }
            else
            {
                LogToConsole($"Error durante la limpieza: {cleanupResult.ErrorMessage}", LogLevel.Error);
            }

            // Volver a detectar instalaciones para actualizar la lista
            await RefreshInstallationsAsync();
        }
    }
    catch (OperationCanceledException)
    {
        LogToConsole("Operación cancelada por el usuario.", LogLevel.Warning);
    }
    catch (Exception ex)
    {
        LogToConsole($"Error durante la limpieza: {ex.Message}", LogLevel.Error);
    }
    finally
    {
        // Ocultar panel de progreso
        ShowProgressPanel(false);

        // Habilitar botones
        SetButtonsEnabled(true);

        // Limpiar token de cancelación
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
```

## Generación de Scripts

Esta sección contiene los métodos relacionados con la generación de scripts de limpieza.

### ScriptGenerator.GenerateCleanupScript

Este método genera un script de limpieza de registro.

```csharp
/// <summary>
/// Genera un script de limpieza de registro.
/// </summary>
/// <param name="regDeleteCommands">Lista de comandos reg delete.</param>
/// <param name="outputPath">Ruta donde guardar el script.</param>
/// <param name="scriptType">Tipo de script (BAT o PS1).</param>
/// <returns>true si el script se generó correctamente, false en caso contrario.</returns>
public static bool GenerateCleanupScript(
    List<string> regDeleteCommands,
    string outputPath,
    ScriptType scriptType)
{
    try
    {
        // Crear directorio si no existe
        string directory = Path.GetDirectoryName(outputPath) ?? "";
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Crear contenido del script según el tipo
        StringBuilder scriptContent = new StringBuilder();

        if (scriptType == ScriptType.BAT)
        {
            // Encabezado para script .bat
            scriptContent.AppendLine("@echo off");
            scriptContent.AppendLine("echo ===================================================");
            scriptContent.AppendLine("echo Script de limpieza de residuos de Adobe Photoshop");
            scriptContent.AppendLine("echo Generado por DesinstalaPhotoshop");
            scriptContent.AppendLine($"echo Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            scriptContent.AppendLine("echo ===================================================");
            scriptContent.AppendLine("echo.");
            scriptContent.AppendLine("echo Este script eliminará entradas de registro residuales de Adobe Photoshop.");
            scriptContent.AppendLine("echo Se recomienda crear una copia de seguridad del registro antes de ejecutar este script.");
            scriptContent.AppendLine("echo.");
            scriptContent.AppendLine("pause");
            scriptContent.AppendLine("echo.");
            scriptContent.AppendLine("echo Eliminando entradas de registro...");
            scriptContent.AppendLine("echo.");

            // Agregar comandos
            foreach (var command in regDeleteCommands)
            {
                scriptContent.AppendLine($"echo Ejecutando: {command}");
                scriptContent.AppendLine(command);
                scriptContent.AppendLine("if %ERRORLEVEL% NEQ 0 echo   - Error al ejecutar el comando");
                scriptContent.AppendLine("echo.");
            }

            // Finalizar script
            scriptContent.AppendLine("echo.");
            scriptContent.AppendLine("echo Limpieza completada.");
            scriptContent.AppendLine("echo.");
            scriptContent.AppendLine("pause");
        }
        else if (scriptType == ScriptType.PS1)
        {
            // Encabezado para script .ps1
            scriptContent.AppendLine("# Script de limpieza de residuos de Adobe Photoshop");
            scriptContent.AppendLine("# Generado por DesinstalaPhotoshop");
            scriptContent.AppendLine($"# Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            scriptContent.AppendLine("");
            scriptContent.AppendLine("Write-Host \"===================================================\" -ForegroundColor Cyan");
            scriptContent.AppendLine("Write-Host \"Script de limpieza de residuos de Adobe Photoshop\" -ForegroundColor Cyan");
            scriptContent.AppendLine("Write-Host \"Generado por DesinstalaPhotoshop\" -ForegroundColor Cyan");
            scriptContent.AppendLine($"Write-Host \"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\" -ForegroundColor Cyan");
            scriptContent.AppendLine("Write-Host \"===================================================\" -ForegroundColor Cyan");
            scriptContent.AppendLine("Write-Host \"\"");
            scriptContent.AppendLine("Write-Host \"Este script eliminará entradas de registro residuales de Adobe Photoshop.\" -ForegroundColor Yellow");
            scriptContent.AppendLine("Write-Host \"Se recomienda crear una copia de seguridad del registro antes de ejecutar este script.\" -ForegroundColor Yellow");
            scriptContent.AppendLine("Write-Host \"\"");
            scriptContent.AppendLine("Read-Host \"Presione Enter para continuar\"");
            scriptContent.AppendLine("Write-Host \"\"");
            scriptContent.AppendLine("Write-Host \"Eliminando entradas de registro...\" -ForegroundColor Green");
            scriptContent.AppendLine("Write-Host \"\"");

            // Agregar comandos (convertidos a PowerShell)
            foreach (var command in regDeleteCommands)
            {
                // Convertir comando reg.exe a PowerShell
                string psCommand = ConvertRegCommandToPowerShell(command);

                scriptContent.AppendLine($"Write-Host \"Ejecutando: {command}\" -ForegroundColor Gray");
                scriptContent.AppendLine("try {");
                scriptContent.AppendLine($"    {psCommand}");
                scriptContent.AppendLine("    Write-Host \"  - Comando ejecutado correctamente\" -ForegroundColor Green");
                scriptContent.AppendLine("} catch {");
                scriptContent.AppendLine("    Write-Host \"  - Error al ejecutar el comando: $_\" -ForegroundColor Red");
                scriptContent.AppendLine("}");
                scriptContent.AppendLine("Write-Host \"\"");
            }

            // Finalizar script
            scriptContent.AppendLine("Write-Host \"\"");
            scriptContent.AppendLine("Write-Host \"Limpieza completada.\" -ForegroundColor Green");
            scriptContent.AppendLine("Write-Host \"\"");
            scriptContent.AppendLine("Read-Host \"Presione Enter para salir\"");
        }

        // Guardar script
        File.WriteAllText(outputPath, scriptContent.ToString());

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error al generar script de limpieza: {ex.Message}");
        return false;
    }
}
```

## Conclusión

Este documento ha presentado el código fuente de los métodos clave del proyecto DesinstalaPhotoshop, organizados por funcionalidad. Estos métodos implementan las principales características de la aplicación, como la detección de instalaciones, la limpieza de residuos, la gestión del registro, las operaciones avanzadas con archivos, las copias de seguridad, la interfaz de usuario y la generación de scripts.

La implementación de estos métodos sigue las mejores prácticas de programación en C#, como el uso de programación asíncrona para mantener la interfaz de usuario responsiva, el manejo adecuado de excepciones, la implementación de patrones de diseño como el patrón de servicio, y la separación clara de responsabilidades entre las diferentes capas de la aplicación.

Además, se han implementado múltiples estrategias para manejar casos complejos, como la eliminación de archivos bloqueados o persistentes, la limpieza de entradas de registro difíciles de eliminar, y la programación de eliminación de archivos al reiniciar el sistema.

Estos métodos forman el núcleo de la aplicación DesinstalaPhotoshop y proporcionan una solución completa y robusta para la desinstalación y limpieza de Adobe Photoshop en sistemas Windows.
```
