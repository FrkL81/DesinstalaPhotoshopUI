namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Helpers;
using DesinstalaPhotoshop.Core.Services.Interfaces;

/// <summary>
/// Implementación del servicio de limpieza de residuos de Photoshop.
/// </summary>
[SupportedOSPlatform("windows")]
public class CleanupService : ICleanupService
{
    private readonly ILoggingService _logger;
    private readonly IBackupService _backupService;
    private readonly IProcessService _processService;
    private readonly IFileSystemHelper _fileSystemHelper;
    private readonly IRegistryHelper _registryHelper;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CleanupService"/>.
    /// </summary>
    /// <param name="logger">Servicio de registro.</param>
    /// <param name="backupService">Servicio de respaldo.</param>
    /// <param name="processService">Servicio para gestionar procesos.</param>
    /// <param name="fileSystemHelper">Ayudante para operaciones con el sistema de archivos.</param>
    /// <param name="registryHelper">Ayudante para operaciones con el registro.</param>
    public CleanupService(
        ILoggingService logger,
        IBackupService backupService,
        IProcessService processService,
        IFileSystemHelper fileSystemHelper,
        IRegistryHelper registryHelper)
    {
        _logger = logger;
        _backupService = backupService;
        _processService = processService;
        _fileSystemHelper = fileSystemHelper;
        _registryHelper = registryHelper;
    }

    /// <summary>
    /// Limpia residuos de Adobe Photoshop del sistema.
    /// </summary>
    /// <param name="installation">Instalación de Photoshop para la que se realizará la limpieza.</param>
    /// <param name="createBackup">Indica si se debe crear una copia de seguridad antes de limpiar.</param>
    /// <param name="whatIf">Indica si se debe simular la limpieza sin realizar cambios reales.</param>
    /// <param name="cleanupTempFiles">Indica si se deben limpiar archivos temporales.</param>
    /// <param name="cleanupRegistry">Indica si se deben limpiar entradas del registro.</param>
    /// <param name="cleanupConfigFiles">Indica si se deben limpiar archivos de configuración.</param>
    /// <param name="cleanupCacheFiles">Indica si se deben limpiar archivos de caché.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<OperationResult> CleanupAsync(
        PhotoshopInstallation installation,
        bool createBackup = true,
        bool whatIf = false,
        bool cleanupTempFiles = true,
        bool cleanupRegistry = true,
        bool cleanupConfigFiles = true,
        bool cleanupCacheFiles = true,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInfo($"Iniciando limpieza de residuos para {installation.DisplayName}...");
        progress?.Report(ProgressInfo.Running(0, "Limpieza", "Iniciando limpieza de residuos..."));

        var result = new OperationResult { Success = true };

        try
        {
            // Crear copia de seguridad si se solicita
            if (createBackup)
            {
                progress?.Report(ProgressInfo.Running(5, "Limpieza", "Creando copia de seguridad..."));
                _logger.LogInfo("Creando copia de seguridad antes de la limpieza...");

                string backupId = await _backupService.CreateBackupForCleanupAsync(
                    installation,
                    progress,
                    cancellationToken);

                result.BackupId = backupId;
                _logger.LogInfo($"Copia de seguridad creada con ID: {backupId}");

                progress?.Report(ProgressInfo.Running(20, "Limpieza", "Copia de seguridad completada."));
            }

            // Verificar si es una simulación
            if (whatIf)
            {
                _logger.LogInfo("Ejecutando en modo de simulación (WhatIf). No se realizarán cambios reales.");
                progress?.Report(ProgressInfo.Running(25, "Limpieza", "Modo de simulación activado. No se realizarán cambios reales."));
            }

            // Detener procesos de Adobe para evitar bloqueos de archivos
            progress?.Report(ProgressInfo.Running(30, "Limpieza", "Deteniendo procesos de Adobe..."));
            _logger.LogInfo("Deteniendo procesos de Adobe antes de la limpieza...");

            var processResult = await _processService.StopAdobeProcessesAsync(
                whatIf,
                progress,
                cancellationToken);

            if (!processResult.Success)
            {
                _logger.LogWarning($"Advertencia al detener procesos: {processResult.Message}");
                progress?.Report(ProgressInfo.Warning("Limpieza", $"Algunos procesos no pudieron detenerse: {processResult.Message}"));
            }
            else
            {
                _logger.LogInfo($"Procesos de Adobe detenidos: {processResult.Message}");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return OperationResult.Canceled("Limpieza cancelada por el usuario.");
            }

            // Limpiar archivos temporales
            if (cleanupTempFiles)
            {
                progress?.Report(ProgressInfo.Running(40, "Limpieza", "Limpiando archivos temporales..."));
                await CleanupTempFilesAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(50, "Limpieza", "Archivos temporales limpiados."));
            }

            // Limpiar entradas del registro
            if (cleanupRegistry)
            {
                progress?.Report(ProgressInfo.Running(60, "Limpieza", "Limpiando entradas del registro..."));
                await CleanupRegistryAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(70, "Limpieza", "Entradas del registro limpiadas."));
            }

            // Limpiar archivos de configuración
            if (cleanupConfigFiles)
            {
                progress?.Report(ProgressInfo.Running(75, "Limpieza", "Limpiando archivos de configuración..."));
                await CleanupConfigFilesAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(85, "Limpieza", "Archivos de configuración limpiados."));
            }

            // Limpiar archivos de caché
            if (cleanupCacheFiles)
            {
                progress?.Report(ProgressInfo.Running(90, "Limpieza", "Limpiando archivos de caché..."));
                await CleanupCacheFilesAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(95, "Limpieza", "Archivos de caché limpiados."));
            }

            // Finalizar limpieza
            progress?.Report(ProgressInfo.Completed("Limpieza", "Limpieza completada con éxito."));
            _logger.LogInfo("Limpieza de residuos completada con éxito.");

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Limpieza cancelada por el usuario.");
            progress?.Report(ProgressInfo.Canceled("Limpieza", "Limpieza cancelada por el usuario."));
            return OperationResult.Canceled("Limpieza cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la limpieza: {ex.Message}");
            progress?.Report(ProgressInfo.Failed("Limpieza", $"Error durante la limpieza: {ex.Message}"));
            return OperationResult.Failed($"Error durante la limpieza: {ex.Message}");
        }
    }

    // Métodos privados para implementar cada tipo de limpieza

    private async Task CleanupTempFilesAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Iniciando limpieza de archivos temporales...");

        try
        {
            // 1. Limpiar archivos temporales en %TEMP%
            string tempPath = Path.GetTempPath();
            _logger.LogInfo($"Buscando archivos temporales de Photoshop en {tempPath}...");

            // Patrones de archivos temporales de Photoshop
            string[] tempPatterns = { "~PST*.tmp", "Photoshop Temp*", "Adobe_Photoshop*.tmp" };

            foreach (var pattern in tempPatterns)
            {
                var tempFiles = _fileSystemHelper.FindFiles(tempPath, pattern);
                _logger.LogInfo($"Encontrados {tempFiles.Count} archivos con patrón {pattern}");

                foreach (var file in tempFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    _logger.LogInfo($"Eliminando archivo temporal: {file}");

                    if (!whatIf)
                    {
                        if (_fileSystemHelper.DeleteFile(file))
                        {
                            _logger.LogInfo($"Archivo temporal eliminado: {file}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar el archivo temporal: {file}");
                        }
                    }
                    else
                    {
                        _logger.LogInfo($"[WhatIf] Se eliminaría el archivo temporal: {file}");
                    }
                }
            }

            // 2. Buscar y eliminar archivos de autorecuperación
            string autoRecoverPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Adobe", "Adobe Photoshop", "AutoRecover");

            if (_fileSystemHelper.DirectoryExists(autoRecoverPath))
            {
                _logger.LogInfo($"Buscando archivos de autorecuperación en {autoRecoverPath}...");

                var autoRecoverFiles = _fileSystemHelper.FindFiles(autoRecoverPath, "*.*");
                _logger.LogInfo($"Encontrados {autoRecoverFiles.Count} archivos de autorecuperación");

                foreach (var file in autoRecoverFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    _logger.LogInfo($"Eliminando archivo de autorecuperación: {file}");

                    if (!whatIf)
                    {
                        if (_fileSystemHelper.DeleteFile(file))
                        {
                            _logger.LogInfo($"Archivo de autorecuperación eliminado: {file}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar el archivo de autorecuperación: {file}");
                        }
                    }
                    else
                    {
                        _logger.LogInfo($"[WhatIf] Se eliminaría el archivo de autorecuperación: {file}");
                    }
                }

                // Intentar eliminar el directorio de autorecuperación si está vacío
                if (!whatIf && _fileSystemHelper.FindFiles(autoRecoverPath, "*.*").Count == 0)
                {
                    if (_fileSystemHelper.DeleteDirectory(autoRecoverPath))
                    {
                        _logger.LogInfo($"Directorio de autorecuperación eliminado: {autoRecoverPath}");
                    }
                }
            }

            // 3. Buscar archivos de disco de trabajo (scratch disks) en las unidades
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
            {
                string drivePath = drive.RootDirectory.FullName;
                _logger.LogInfo($"Buscando archivos de disco de trabajo en {drivePath}...");

                var scratchFiles = _fileSystemHelper.FindFiles(drivePath, "Photoshop Temp*", SearchOption.TopDirectoryOnly);
                _logger.LogInfo($"Encontrados {scratchFiles.Count} archivos de disco de trabajo en {drivePath}");

                foreach (var file in scratchFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    _logger.LogInfo($"Eliminando archivo de disco de trabajo: {file}");

                    if (!whatIf)
                    {
                        if (_fileSystemHelper.DeleteFile(file))
                        {
                            _logger.LogInfo($"Archivo de disco de trabajo eliminado: {file}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar el archivo de disco de trabajo: {file}");
                        }
                    }
                    else
                    {
                        _logger.LogInfo($"[WhatIf] Se eliminaría el archivo de disco de trabajo: {file}");
                    }
                }
            }

            _logger.LogInfo("Limpieza de archivos temporales completada.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la limpieza de archivos temporales: {ex.Message}");
            throw;
        }
    }

    private async Task CleanupRegistryAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Iniciando limpieza de entradas del registro...");

        try
        {
            // Lista de claves de registro a limpiar
            List<string> keysToClean = new List<string>();

            // 1. Claves específicas de la instalación
            if (installation.AssociatedRegistryKeys != null && installation.AssociatedRegistryKeys.Count > 0)
            {
                foreach (var key in installation.AssociatedRegistryKeys)
                {
                    keysToClean.Add(key);
                    _logger.LogInfo($"Agregada clave de registro específica de la instalación: {key}");
                }
            }

            // 2. Claves de desinstalación
            string uninstallKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            string uninstallKeyWow64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

            // Buscar claves de desinstalación relacionadas con Photoshop
            var photoshopKeys = _registryHelper.FindPhotoshopRegistryKeys(uninstallKey);
            photoshopKeys.AddRange(_registryHelper.FindPhotoshopRegistryKeys(uninstallKeyWow64));

            foreach (var key in photoshopKeys)
            {
                if (!keysToClean.Contains(key))
                {
                    keysToClean.Add(key);
                    _logger.LogInfo($"Agregada clave de desinstalación: {key}");
                }
            }

            // 3. Claves específicas de Adobe Photoshop
            string[] adobeKeyPaths = {
                @"HKEY_CURRENT_USER\Software\Adobe\Photoshop",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Adobe\Photoshop",
                @"HKEY_CURRENT_USER\Software\Adobe\Adobe Photoshop",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Adobe Photoshop",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Adobe\Adobe Photoshop"
            };

            foreach (var keyPath in adobeKeyPaths)
            {
                if (_registryHelper.KeyExists(keyPath))
                {
                    keysToClean.Add(keyPath);
                    _logger.LogInfo($"Agregada clave específica de Adobe Photoshop: {keyPath}");
                }
            }

            // 4. Asociaciones de archivos
            string[] extensions = { ".psd", ".psb", ".pdd", ".abr", ".atn", ".pat" };

            foreach (var extension in extensions)
            {
                string keyPath = $"HKEY_CLASSES_ROOT\\{extension}";
                if (_registryHelper.KeyExists(keyPath))
                {
                    // Obtener el ProgID asociado
                    var progId = _registryHelper.GetRegistryValue(keyPath, null) as string;
                    if (!string.IsNullOrEmpty(progId) && progId.Contains("Photoshop", StringComparison.OrdinalIgnoreCase))
                    {
                        // Solo limpiar la asociación si está relacionada con Photoshop
                        keysToClean.Add(keyPath);
                        _logger.LogInfo($"Agregada asociación de archivo: {keyPath} (ProgID: {progId})");

                        // También agregar la clave del ProgID
                        string progIdKeyPath = $"HKEY_CLASSES_ROOT\\{progId}";
                        if (_registryHelper.KeyExists(progIdKeyPath))
                        {
                            keysToClean.Add(progIdKeyPath);
                            _logger.LogInfo($"Agregada clave de ProgID: {progIdKeyPath}");
                        }
                    }
                }
            }

            // 5. Limpiar las claves identificadas
            int totalKeys = keysToClean.Count;
            int processedKeys = 0;

            foreach (var key in keysToClean)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                processedKeys++;
                double progressPercentage = 60 + (processedKeys * 10.0 / totalKeys);
                progress?.Report(ProgressInfo.Running((int)progressPercentage, "Limpieza de registro", $"Procesando clave {processedKeys} de {totalKeys}..."));

                _logger.LogInfo($"Eliminando clave de registro: {key}");

                if (!whatIf)
                {
                    // Crear copia de seguridad de la clave antes de eliminarla
                    string backupDir = Path.Combine(Path.GetTempPath(), "DesinstalaPhotoshop", "RegBackup");
                    _fileSystemHelper.CreateDirectory(backupDir);

                    string backupFile = Path.Combine(backupDir, $"key_{Path.GetRandomFileName()}.reg");
                    bool backupCreated = _registryHelper.ExportRegistryKey(key, backupFile);

                    if (backupCreated)
                    {
                        _logger.LogInfo($"Copia de seguridad de la clave creada en: {backupFile}");
                    }

                    // Eliminar la clave
                    bool deleted = _registryHelper.DeleteRegistryKey(key);

                    if (deleted)
                    {
                        _logger.LogInfo($"Clave de registro eliminada: {key}");
                    }
                    else
                    {
                        _logger.LogWarning($"No se pudo eliminar la clave de registro: {key}");

                        // Intentar con reg.exe como fallback
                        _logger.LogInfo($"Intentando eliminar la clave con reg.exe: {key}");
                        bool regExeDeleted = _registryHelper.DeleteRegistryKeyWithRegExe(key);

                        if (regExeDeleted)
                        {
                            _logger.LogInfo($"Clave de registro eliminada con reg.exe: {key}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar la clave de registro con reg.exe: {key}");
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"[WhatIf] Se eliminaría la clave de registro: {key}");
                }

                // Pequeña pausa para no sobrecargar el sistema
                await Task.Delay(50, cancellationToken);
            }

            _logger.LogInfo("Limpieza de entradas del registro completada.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la limpieza de entradas del registro: {ex.Message}");
            throw;
        }
    }

    private async Task CleanupConfigFilesAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Iniciando limpieza de archivos de configuración...");

        try
        {
            // 1. Archivos de configuración en %AppData%\Adobe\Adobe Photoshop [versión]\
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string adobeAppDataPath = Path.Combine(appDataPath, "Adobe");

            _logger.LogInfo($"Buscando archivos de configuración en {adobeAppDataPath}...");

            if (_fileSystemHelper.DirectoryExists(adobeAppDataPath))
            {
                // Buscar carpetas de Photoshop
                var photoshopDirs = _fileSystemHelper.FindDirectories(adobeAppDataPath, "Photoshop");
                _logger.LogInfo($"Encontrados {photoshopDirs.Count} directorios de Photoshop en {adobeAppDataPath}");

                foreach (var dir in photoshopDirs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    _logger.LogInfo($"Limpiando directorio de configuración: {dir}");

                    if (!whatIf)
                    {
                        if (_fileSystemHelper.DeleteDirectory(dir, true))
                        {
                            _logger.LogInfo($"Directorio de configuración eliminado: {dir}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar el directorio de configuración: {dir}");

                            // Intentar eliminar archivos individuales
                            var files = _fileSystemHelper.FindFiles(dir, "*.*");
                            foreach (var file in files)
                            {
                                if (_fileSystemHelper.DeleteFile(file))
                                {
                                    _logger.LogInfo($"Archivo de configuración eliminado: {file}");
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInfo($"[WhatIf] Se eliminaría el directorio de configuración: {dir}");
                    }
                }
            }

            // 2. Archivos de configuración en %LocalAppData%\Adobe\
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string adobeLocalAppDataPath = Path.Combine(localAppDataPath, "Adobe");

            _logger.LogInfo($"Buscando archivos de configuración en {adobeLocalAppDataPath}...");

            if (_fileSystemHelper.DirectoryExists(adobeLocalAppDataPath))
            {
                // Buscar carpetas específicas de Photoshop
                string[] photoshopConfigDirs = {
                    Path.Combine(adobeLocalAppDataPath, "Adobe Photoshop"),
                    Path.Combine(adobeLocalAppDataPath, "Photoshop"),
                    Path.Combine(adobeLocalAppDataPath, "OOBE") // Datos de licenciamiento
                };

                foreach (var dir in photoshopConfigDirs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (_fileSystemHelper.DirectoryExists(dir))
                    {
                        _logger.LogInfo($"Limpiando directorio de configuración: {dir}");

                        if (!whatIf)
                        {
                            if (_fileSystemHelper.DeleteDirectory(dir, true))
                            {
                                _logger.LogInfo($"Directorio de configuración eliminado: {dir}");
                            }
                            else
                            {
                                _logger.LogWarning($"No se pudo eliminar el directorio de configuración: {dir}");

                                // Intentar eliminar archivos individuales
                                var files = _fileSystemHelper.FindFiles(dir, "*.*");
                                foreach (var file in files)
                                {
                                    if (_fileSystemHelper.DeleteFile(file))
                                    {
                                        _logger.LogInfo($"Archivo de configuración eliminado: {file}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInfo($"[WhatIf] Se eliminaría el directorio de configuración: {dir}");
                        }
                    }
                }
            }

            // 3. Archivos de configuración en %ProgramData%\Adobe\
            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string adobeProgramDataPath = Path.Combine(programDataPath, "Adobe");

            _logger.LogInfo($"Buscando archivos de configuración en {adobeProgramDataPath}...");

            if (_fileSystemHelper.DirectoryExists(adobeProgramDataPath))
            {
                // Buscar carpetas específicas de Photoshop
                string[] photoshopProgramDataDirs = {
                    Path.Combine(adobeProgramDataPath, "SLStore_v1"), // Datos de licenciamiento
                    Path.Combine(adobeProgramDataPath, "OperatingConfigs")
                };

                foreach (var dir in photoshopProgramDataDirs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (_fileSystemHelper.DirectoryExists(dir))
                    {
                        _logger.LogInfo($"Limpiando directorio de configuración: {dir}");

                        if (!whatIf)
                        {
                            if (_fileSystemHelper.DeleteDirectory(dir, true))
                            {
                                _logger.LogInfo($"Directorio de configuración eliminado: {dir}");
                            }
                            else
                            {
                                _logger.LogWarning($"No se pudo eliminar el directorio de configuración: {dir}");

                                // Intentar eliminar archivos individuales
                                var files = _fileSystemHelper.FindFiles(dir, "*.*");
                                foreach (var file in files)
                                {
                                    if (_fileSystemHelper.DeleteFile(file))
                                    {
                                        _logger.LogInfo($"Archivo de configuración eliminado: {file}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInfo($"[WhatIf] Se eliminaría el directorio de configuración: {dir}");
                        }
                    }
                }
            }

            _logger.LogInfo("Limpieza de archivos de configuración completada.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la limpieza de archivos de configuración: {ex.Message}");
            throw;
        }
    }

    private async Task CleanupCacheFilesAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Iniciando limpieza de archivos de caché...");

        try
        {
            // 1. Caché de fuentes
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string fontCachePath = Path.Combine(localAppDataPath, "Adobe", "TypeSupport", "CS6");

            _logger.LogInfo($"Buscando caché de fuentes en {fontCachePath}...");

            if (_fileSystemHelper.DirectoryExists(fontCachePath))
            {
                _logger.LogInfo($"Limpiando caché de fuentes: {fontCachePath}");

                if (!whatIf)
                {
                    if (_fileSystemHelper.DeleteDirectory(fontCachePath, true))
                    {
                        _logger.LogInfo($"Caché de fuentes eliminado: {fontCachePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"No se pudo eliminar el caché de fuentes: {fontCachePath}");

                        // Intentar eliminar archivos individuales
                        var files = _fileSystemHelper.FindFiles(fontCachePath, "*.*");
                        foreach (var file in files)
                        {
                            if (_fileSystemHelper.DeleteFile(file))
                            {
                                _logger.LogInfo($"Archivo de caché de fuentes eliminado: {file}");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"[WhatIf] Se eliminaría el caché de fuentes: {fontCachePath}");
                }
            }

            // 2. Caché de Camera Raw
            string cameraRawCachePath = Path.Combine(localAppDataPath, "Adobe", "CameraRaw", "Cache");

            _logger.LogInfo($"Buscando caché de Camera Raw en {cameraRawCachePath}...");

            if (_fileSystemHelper.DirectoryExists(cameraRawCachePath))
            {
                _logger.LogInfo($"Limpiando caché de Camera Raw: {cameraRawCachePath}");

                if (!whatIf)
                {
                    if (_fileSystemHelper.DeleteDirectory(cameraRawCachePath, true))
                    {
                        _logger.LogInfo($"Caché de Camera Raw eliminado: {cameraRawCachePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"No se pudo eliminar el caché de Camera Raw: {cameraRawCachePath}");

                        // Intentar eliminar archivos individuales
                        var files = _fileSystemHelper.FindFiles(cameraRawCachePath, "*.*");
                        foreach (var file in files)
                        {
                            if (_fileSystemHelper.DeleteFile(file))
                            {
                                _logger.LogInfo($"Archivo de caché de Camera Raw eliminado: {file}");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"[WhatIf] Se eliminaría el caché de Camera Raw: {cameraRawCachePath}");
                }
            }

            // 3. Caché de plugins y extensiones
            string[] pluginCachePaths = {
                Path.Combine(localAppDataPath, "Adobe", "CEP", "extensions"),
                Path.Combine(localAppDataPath, "Adobe", "UXP")
            };

            foreach (var cachePath in pluginCachePaths)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                _logger.LogInfo($"Buscando caché de plugins en {cachePath}...");

                if (_fileSystemHelper.DirectoryExists(cachePath))
                {
                    _logger.LogInfo($"Limpiando caché de plugins: {cachePath}");

                    if (!whatIf)
                    {
                        if (_fileSystemHelper.DeleteDirectory(cachePath, true))
                        {
                            _logger.LogInfo($"Caché de plugins eliminado: {cachePath}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar el caché de plugins: {cachePath}");

                            // Intentar eliminar archivos individuales
                            var files = _fileSystemHelper.FindFiles(cachePath, "*.*");
                            foreach (var file in files)
                            {
                                if (_fileSystemHelper.DeleteFile(file))
                                {
                                    _logger.LogInfo($"Archivo de caché de plugins eliminado: {file}");
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInfo($"[WhatIf] Se eliminaría el caché de plugins: {cachePath}");
                    }
                }
            }

            _logger.LogInfo("Limpieza de archivos de caché completada.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la limpieza de archivos de caché: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Procesa carpetas en Common Files que son más difíciles de eliminar.
    /// </summary>
    /// <param name="whatIf">Indica si se debe simular la operación sin realizar cambios reales.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    private async Task<OperationResult> ProcessCommonFilesDirectoriesAsync(
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Iniciando procesamiento de carpetas en Common Files...");

        try
        {
            // Obtener la ruta de Common Files
            string commonFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            string commonFilesX86Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);

            // Carpetas de Adobe en Common Files
            string[] adobeDirs = {
                Path.Combine(commonFilesPath, "Adobe"),
                Path.Combine(commonFilesX86Path, "Adobe")
            };

            foreach (var adobeDir in adobeDirs)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult.Canceled("Operación cancelada por el usuario.");

                if (!_fileSystemHelper.DirectoryExists(adobeDir))
                    continue;

                _logger.LogInfo($"Procesando carpeta de Adobe en Common Files: {adobeDir}");

                // Buscar carpetas específicas de Photoshop
                string[] photoshopDirs = {
                    Path.Combine(adobeDir, "Photoshop"),
                    Path.Combine(adobeDir, "Adobe Photoshop")
                };

                foreach (var photoshopDir in photoshopDirs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return OperationResult.Canceled("Operación cancelada por el usuario.");

                    if (!_fileSystemHelper.DirectoryExists(photoshopDir))
                        continue;

                    _logger.LogInfo($"Procesando carpeta de Photoshop en Common Files: {photoshopDir}");

                    if (!whatIf)
                    {
                        // Intentar eliminar la carpeta con métodos especiales
                        bool success = await ForceDeleteCommonFilesDirectoryAsync(photoshopDir, cancellationToken);

                        if (success)
                        {
                            _logger.LogInfo($"Carpeta de Photoshop en Common Files eliminada: {photoshopDir}");
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo eliminar la carpeta de Photoshop en Common Files: {photoshopDir}");

                            // Programar para eliminación al reinicio
                            await ScheduleDirectoryForDeletionAsync(photoshopDir, cancellationToken);
                        }
                    }
                    else
                    {
                        _logger.LogInfo($"[WhatIf] Se eliminaría la carpeta de Photoshop en Common Files: {photoshopDir}");
                    }
                }
            }

            return OperationResult.SuccessResult("Procesamiento de carpetas en Common Files completado.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante el procesamiento de carpetas en Common Files: {ex.Message}");
            return OperationResult.Failed($"Error durante el procesamiento de carpetas en Common Files: {ex.Message}");
        }
    }

    /// <summary>
    /// Intenta eliminar una carpeta en Common Files con métodos especiales.
    /// </summary>
    /// <param name="directoryPath">Ruta de la carpeta a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>True si la eliminación fue exitosa, false en caso contrario.</returns>
    private async Task<bool> ForceDeleteCommonFilesDirectoryAsync(string directoryPath, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Intentando eliminar carpeta en Common Files con métodos especiales: {directoryPath}");

            // 1. Intentar eliminar directamente
            if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
            {
                _logger.LogInfo($"Carpeta eliminada directamente: {directoryPath}");
                return true;
            }

            // 2. Forzar la recolección de basura y reintentar
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
            {
                _logger.LogInfo($"Carpeta eliminada después de forzar GC: {directoryPath}");
                return true;
            }

            // 3. Intentar eliminar archivos individuales
            bool allFilesDeleted = true;

            try
            {
                foreach (var file in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return false;

                        if (_fileSystemHelper.DeleteFile(file))
                        {
                            _logger.LogInfo($"Archivo eliminado: {file}");
                        }
                        else
                        {
                            allFilesDeleted = false;
                            _logger.LogWarning($"No se pudo eliminar el archivo: {file}");
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

            // Intentar eliminar la carpeta nuevamente
            if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
            {
                _logger.LogInfo($"Carpeta eliminada después de eliminar archivos individuales: {directoryPath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en eliminación forzada de carpeta Common Files {directoryPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Programa la eliminación de archivos persistentes al reiniciar el sistema.
    /// </summary>
    /// <param name="whatIf">Indica si se debe simular la operación sin realizar cambios reales.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    private async Task<OperationResult> ScheduleFilesForDeletionAsync(
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Iniciando programación de eliminación de archivos persistentes...");

        try
        {
            // Obtener la lista de archivos y carpetas que no se pudieron eliminar
            var pendingFiles = new List<string>();

            // Buscar archivos persistentes en ubicaciones comunes
            string[] commonLocations = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Adobe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "Adobe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "Adobe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Adobe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Adobe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Adobe")
            };

            foreach (var location in commonLocations)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult.Canceled("Operación cancelada por el usuario.");

                if (_fileSystemHelper.DirectoryExists(location))
                {
                    // Buscar carpetas de Photoshop
                    var photoshopDirs = _fileSystemHelper.FindDirectories(location, "Photoshop");
                    foreach (var dir in photoshopDirs)
                    {
                        if (_fileSystemHelper.DirectoryExists(dir))
                        {
                            pendingFiles.Add(dir);
                        }
                    }
                }
            }

            // Programar la eliminación de archivos persistentes
            int totalPending = pendingFiles.Count;
            int processed = 0;

            foreach (var filePath in pendingFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult.Canceled("Operación cancelada por el usuario.");

                processed++;
                double progressPercentage = 60 + (processed * 10.0 / totalPending);
                progress?.Report(ProgressInfo.Running((int)progressPercentage, "Programando eliminación", $"Procesando {processed} de {totalPending}..."));

                _logger.LogInfo($"Programando eliminación al reinicio: {filePath}");

                if (!whatIf)
                {
                    if (_fileSystemHelper.FileExists(filePath))
                    {
                        await ScheduleFileForDeletionAsync(filePath, cancellationToken);
                    }
                    else if (_fileSystemHelper.DirectoryExists(filePath))
                    {
                        await ScheduleDirectoryForDeletionAsync(filePath, cancellationToken);
                    }
                }
                else
                {
                    _logger.LogInfo($"[WhatIf] Se programaría la eliminación al reinicio: {filePath}");
                }
            }

            if (pendingFiles.Count > 0)
            {
                return OperationResult.SuccessResult($"Se han programado {pendingFiles.Count} archivos para eliminación al reinicio.");
            }
            else
            {
                return OperationResult.SuccessResult("No se encontraron archivos persistentes para programar su eliminación.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la programación de eliminación de archivos: {ex.Message}");
            return OperationResult.Failed($"Error durante la programación de eliminación de archivos: {ex.Message}");
        }
    }

    /// <summary>
    /// Programa la eliminación de un archivo al reiniciar el sistema.
    /// </summary>
    /// <param name="filePath">Ruta del archivo a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>True si la programación fue exitosa, false en caso contrario.</returns>
    private async Task<bool> ScheduleFileForDeletionAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Programando eliminación de archivo al reinicio: {filePath}");

            // Usar MoveFileEx con MOVEFILE_DELAY_UNTIL_REBOOT
            bool success = NativeMethods.MoveFileEx(
                filePath,
                null,
                NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

            if (success)
            {
                _logger.LogInfo($"Archivo programado para eliminación al reinicio: {filePath}");
                return true;
            }
            else
            {
                int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                _logger.LogWarning($"No se pudo programar la eliminación del archivo: {filePath}, Error: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al programar la eliminación del archivo {filePath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Programa la eliminación de un directorio al reiniciar el sistema.
    /// </summary>
    /// <param name="directoryPath">Ruta del directorio a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>True si la programación fue exitosa, false en caso contrario.</returns>
    private async Task<bool> ScheduleDirectoryForDeletionAsync(string directoryPath, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Programando eliminación de directorio al reinicio: {directoryPath}");

            // Programar la eliminación de todos los archivos
            bool allScheduled = true;

            try
            {
                foreach (var file in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    bool fileScheduled = await ScheduleFileForDeletionAsync(file, cancellationToken);
                    if (!fileScheduled)
                    {
                        allScheduled = false;
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
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    bool success = NativeMethods.MoveFileEx(
                        subDir,
                        null,
                        NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

                    if (!success)
                    {
                        int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                        _logger.LogWarning($"No se pudo programar la eliminación de la subcarpeta: {subDir}, Error: {error}");
                        allScheduled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al enumerar subcarpetas para eliminación: {ex.Message}");
            }

            // Programar la eliminación del directorio principal
            bool dirScheduled = NativeMethods.MoveFileEx(
                directoryPath,
                null,
                NativeMethods.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

            if (dirScheduled)
            {
                _logger.LogInfo($"Directorio programado para eliminación al reinicio: {directoryPath}");
            }
            else
            {
                int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                _logger.LogWarning($"No se pudo programar la eliminación del directorio: {directoryPath}, Error: {error}");
                allScheduled = false;
            }

            return allScheduled;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al programar la eliminación del directorio {directoryPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clase para acceder a métodos nativos de Windows.
    /// </summary>
    private static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, MoveFileFlags dwFlags);

        [Flags]
        public enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 0x00000001,
            MOVEFILE_COPY_ALLOWED = 0x00000002,
            MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,
            MOVEFILE_WRITE_THROUGH = 0x00000008,
            MOVEFILE_CREATE_HARDLINK = 0x00000010,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020
        }
    }
}
