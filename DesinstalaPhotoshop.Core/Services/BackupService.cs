namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Helpers;
using DesinstalaPhotoshop.Core.Services.Interfaces;

/// <summary>
/// Implementación del servicio de respaldo y restauración.
/// </summary>
[SupportedOSPlatform("windows")]
public class BackupService : IBackupService
{
    private readonly ILoggingService _logger;
    private readonly IFileSystemHelper _fileSystemHelper;
    private readonly IRegistryHelper _registryHelper;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="BackupService"/>.
    /// </summary>
    /// <param name="logger">Servicio de registro.</param>
    /// <param name="fileSystemHelper">Ayudante para operaciones con el sistema de archivos.</param>
    /// <param name="registryHelper">Ayudante para operaciones con el registro.</param>
    public BackupService(
        ILoggingService logger,
        IFileSystemHelper fileSystemHelper,
        IRegistryHelper registryHelper)
    {
        _logger = logger;
        _fileSystemHelper = fileSystemHelper;
        _registryHelper = registryHelper;
    }

    /// <summary>
    /// Crea una copia de seguridad para una operación de limpieza.
    /// </summary>
    /// <param name="installation">Instalación de Photoshop para la que se creará la copia de seguridad.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Identificador único de la copia de seguridad creada.</returns>
    public async Task<string> CreateBackupForCleanupAsync(
        PhotoshopInstallation installation,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInfo($"Creando copia de seguridad para limpieza de {installation.DisplayName}...");
        progress?.Report(ProgressInfo.Running(0, "Copia de Seguridad", "Iniciando creación de copia de seguridad..."));

        // Implementación existente...
        // Generar un ID único para la copia de seguridad
        string backupId = $"Cleanup_{DateTime.Now:yyyyMMdd_HHmmss}";

        // Crear la estructura de directorios para la copia de seguridad
        string backupBaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PhotoshopBackups");

        string backupDir = Path.Combine(backupBaseDir, backupId);

        // Crear directorios
        _fileSystemHelper.CreateDirectory(backupDir);
        _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Files"));
        _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Directories"));
        _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Registry"));
        _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Metadata"));

        progress?.Report(ProgressInfo.Running(10, "Copia de Seguridad", "Estructura de directorios creada..."));

        // Crear metadatos de la copia de seguridad
        var metadata = new BackupMetadata
        {
            Id = backupId,
            CreationDate = DateTime.Now,
            OperationType = "Cleanup",
            InstallationName = installation.DisplayName,
            PhotoshopVersion = installation.Version,
            InstallationLocation = installation.InstallLocation,
            BackupPath = backupDir,
            Notes = "Copia de seguridad creada antes de la limpieza de residuos de Photoshop."
        };

        // Copiar archivos asociados
        int fileCount = 0;
        int totalFiles = installation.AssociatedFiles.Count;

        foreach (var filePath in installation.AssociatedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_fileSystemHelper.FileExists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(backupDir, "Files", fileName);

                await _fileSystemHelper.CopyFileAsync(filePath, destPath);
                metadata.BackedUpFiles.Add(filePath);

                fileCount++;
                int percentage = (int)((float)fileCount / totalFiles * 40) + 10; // 10-50%
                progress?.Report(ProgressInfo.Running(percentage, "Copia de Seguridad", $"Copiando archivos ({fileCount}/{totalFiles})..."));
            }
        }

        // Exportar claves de registro
        int regCount = 0;
        int totalRegs = installation.AssociatedRegistryKeys.Count;

        foreach (var regKey in installation.AssociatedRegistryKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string regFileName = $"reg_{regCount}.reg";
            string regFilePath = Path.Combine(backupDir, "Registry", regFileName);

            bool exported = await Task.Run(() => _registryHelper.ExportRegistryKey(regKey, regFilePath), cancellationToken);

            if (exported)
            {
                metadata.BackedUpRegistryKeys.Add(regKey);
            }

            regCount++;
            int percentage = (int)((float)regCount / totalRegs * 40) + 50; // 50-90%
            progress?.Report(ProgressInfo.Running(percentage, "Copia de Seguridad", $"Exportando claves de registro ({regCount}/{totalRegs})..."));
        }

        // Guardar metadatos
        string metadataPath = Path.Combine(backupDir, "Metadata", "backup_info.json");
        string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataPath, metadataJson, cancellationToken);

        progress?.Report(ProgressInfo.Completed("Copia de Seguridad", "Copia de seguridad completada con éxito."));
        _logger.LogInfo($"Copia de seguridad creada con éxito: {backupId}");

        return backupId;
    }

    /// <summary>
    /// Crea una copia de seguridad de una instalación de Photoshop.
    /// </summary>
    /// <param name="installation">Instalación de la que se creará la copia de seguridad.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<OperationResult> CreateBackupAsync(
        PhotoshopInstallation installation,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfo($"Creando copia de seguridad de {installation.DisplayName}...");
            progress?.Report(ProgressInfo.Running(0, "Copia de Seguridad", "Iniciando creación de copia de seguridad..."));

            // Generar un ID único para la copia de seguridad
            string backupId = $"Uninstall_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Crear la estructura de directorios para la copia de seguridad
            string backupBaseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PhotoshopBackups");

            string backupDir = Path.Combine(backupBaseDir, backupId);

            // Crear directorios
            _fileSystemHelper.CreateDirectory(backupDir);
            _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Files"));
            _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Directories"));
            _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Registry"));
            _fileSystemHelper.CreateDirectory(Path.Combine(backupDir, "Metadata"));

            progress?.Report(ProgressInfo.Running(10, "Copia de Seguridad", "Estructura de directorios creada..."));

            // Crear metadatos de la copia de seguridad
            var metadata = new BackupMetadata
            {
                Id = backupId,
                CreationDate = DateTime.Now,
                OperationType = "Uninstall",
                InstallationName = installation.DisplayName,
                PhotoshopVersion = installation.Version,
                InstallationLocation = installation.InstallLocation,
                BackupPath = backupDir,
                Notes = "Copia de seguridad creada antes de la desinstalación de Photoshop."
            };

            // Copiar archivos asociados
            int fileCount = 0;
            int totalFiles = installation.AssociatedFiles.Count;

            foreach (var filePath in installation.AssociatedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_fileSystemHelper.FileExists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string destPath = Path.Combine(backupDir, "Files", fileName);

                    await _fileSystemHelper.CopyFileAsync(filePath, destPath);
                    metadata.BackedUpFiles.Add(filePath);

                    fileCount++;
                    int percentage = (int)((float)fileCount / totalFiles * 40) + 10; // 10-50%
                    progress?.Report(ProgressInfo.Running(percentage, "Copia de Seguridad", $"Copiando archivos ({fileCount}/{totalFiles})..."));
                }
            }

            // Exportar claves de registro
            int regCount = 0;
            int totalRegs = installation.AssociatedRegistryKeys.Count;

            foreach (var regKey in installation.AssociatedRegistryKeys)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string regFileName = $"reg_{regCount}.reg";
                string regFilePath = Path.Combine(backupDir, "Registry", regFileName);

                bool exported = await Task.Run(() => _registryHelper.ExportRegistryKey(regKey, regFilePath), cancellationToken);

                if (exported)
                {
                    metadata.BackedUpRegistryKeys.Add(regKey);
                }

                regCount++;
                int percentage = (int)((float)regCount / totalRegs * 40) + 50; // 50-90%
                progress?.Report(ProgressInfo.Running(percentage, "Copia de Seguridad", $"Exportando claves de registro ({regCount}/{totalRegs})..."));
            }

            // Guardar metadatos
            string metadataPath = Path.Combine(backupDir, "Metadata", "backup_info.json");
            string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, metadataJson, cancellationToken);

            progress?.Report(ProgressInfo.Completed("Copia de Seguridad", "Copia de seguridad completada con éxito."));
            _logger.LogInfo($"Copia de seguridad creada con éxito: {backupId}");

            return OperationResult.SuccessResult($"Copia de seguridad creada con éxito: {backupId}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Creación de copia de seguridad cancelada por el usuario.");
            progress?.Report(ProgressInfo.Canceled("Copia de Seguridad", "Creación de copia de seguridad cancelada por el usuario."));
            return OperationResult.Canceled("Creación de copia de seguridad cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al crear copia de seguridad: {ex.Message}");
            progress?.Report(ProgressInfo.Failed("Copia de Seguridad", $"Error al crear copia de seguridad: {ex.Message}"));
            return OperationResult.Failed($"Error al crear copia de seguridad: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene una lista de las copias de seguridad disponibles.
    /// </summary>
    /// <returns>Lista de metadatos de las copias de seguridad disponibles.</returns>
    public List<BackupMetadata> GetAvailableBackups()
    {
        _logger.LogInfo("Obteniendo lista de copias de seguridad disponibles...");

        var backups = new List<BackupMetadata>();

        // Obtener la ruta base de las copias de seguridad
        string backupBaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PhotoshopBackups");

        // Verificar si el directorio existe
        if (!Directory.Exists(backupBaseDir))
        {
            _logger.LogInfo("No se encontró el directorio de copias de seguridad.");
            return backups;
        }

        // Buscar subdirectorios (cada uno es una copia de seguridad)
        var backupDirs = Directory.GetDirectories(backupBaseDir);

        foreach (var backupDir in backupDirs)
        {
            string metadataPath = Path.Combine(backupDir, "Metadata", "backup_info.json");

            // Verificar si existe el archivo de metadatos
            if (File.Exists(metadataPath))
            {
                try
                {
                    // Leer y deserializar los metadatos
                    string metadataJson = File.ReadAllText(metadataPath);
                    var metadata = JsonSerializer.Deserialize<BackupMetadata>(metadataJson);

                    if (metadata != null)
                    {
                        // Actualizar la ruta de la copia de seguridad (por si ha cambiado)
                        metadata.BackupPath = backupDir;
                        backups.Add(metadata);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al leer metadatos de copia de seguridad {backupDir}: {ex.Message}");
                }
            }
        }

        _logger.LogInfo($"Se encontraron {backups.Count} copias de seguridad.");
        return backups;
    }

    /// <summary>
    /// Obtiene una lista de copias de seguridad disponibles.
    /// </summary>
    /// <returns>Lista de metadatos de copias de seguridad.</returns>
    public async Task<List<BackupMetadata>> GetBackupsAsync()
    {
        return await Task.Run(() => GetAvailableBackups());
    }

    /// <summary>
    /// Elimina una copia de seguridad.
    /// </summary>
    /// <param name="backupId">Identificador de la copia de seguridad a eliminar.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<OperationResult> DeleteBackupAsync(string backupId)
    {
        try
        {
            _logger.LogInfo($"Eliminando copia de seguridad {backupId}...");

            // Obtener la ruta de la copia de seguridad
            string backupBaseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PhotoshopBackups");

            string backupDir = Path.Combine(backupBaseDir, backupId);

            // Verificar si existe el directorio de la copia de seguridad
            if (!Directory.Exists(backupDir))
            {
                _logger.LogError($"No se encontró el directorio de la copia de seguridad: {backupDir}");
                return OperationResult.Failed($"No se encontró la copia de seguridad: {backupId}");
            }

            // Eliminar el directorio de la copia de seguridad
            await Task.Run(() => Directory.Delete(backupDir, true));

            _logger.LogInfo($"Copia de seguridad {backupId} eliminada con éxito.");
            return OperationResult.SuccessResult($"Copia de seguridad {backupId} eliminada con éxito.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar copia de seguridad {backupId}: {ex.Message}");
            return OperationResult.Failed($"Error al eliminar copia de seguridad: {ex.Message}");
        }
    }

    /// <summary>
    /// Restaura una copia de seguridad.
    /// </summary>
    /// <param name="backupId">Identificador de la copia de seguridad a restaurar.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<OperationResult> RestoreBackupAsync(
        string backupId,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInfo($"Restaurando copia de seguridad {backupId}...");
        progress?.Report(ProgressInfo.Running(0, "Restauración", "Iniciando restauración de copia de seguridad..."));

        // Obtener la ruta de la copia de seguridad
        string backupBaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PhotoshopBackups");

        string backupDir = Path.Combine(backupBaseDir, backupId);

        // Verificar si existe el directorio de la copia de seguridad
        if (!Directory.Exists(backupDir))
        {
            _logger.LogError($"No se encontró el directorio de la copia de seguridad: {backupDir}");
            progress?.Report(ProgressInfo.Failed("Restauración", "No se encontró la copia de seguridad."));
            return OperationResult.Failed("No se encontró la copia de seguridad.");
        }

        // Leer metadatos
        string metadataPath = Path.Combine(backupDir, "Metadata", "backup_info.json");

        if (!File.Exists(metadataPath))
        {
            _logger.LogError($"No se encontró el archivo de metadatos: {metadataPath}");
            progress?.Report(ProgressInfo.Failed("Restauración", "No se encontraron los metadatos de la copia de seguridad."));
            return OperationResult.Failed("No se encontraron los metadatos de la copia de seguridad.");
        }

        BackupMetadata? metadata;

        try
        {
            string metadataJson = await File.ReadAllTextAsync(metadataPath, cancellationToken);
            metadata = JsonSerializer.Deserialize<BackupMetadata>(metadataJson);

            if (metadata == null)
            {
                _logger.LogError("Error al deserializar los metadatos de la copia de seguridad.");
                progress?.Report(ProgressInfo.Failed("Restauración", "Error al leer los metadatos de la copia de seguridad."));
                return OperationResult.Failed("Error al leer los metadatos de la copia de seguridad.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al leer metadatos de copia de seguridad: {ex.Message}");
            progress?.Report(ProgressInfo.Failed("Restauración", $"Error al leer los metadatos: {ex.Message}"));
            return OperationResult.Failed($"Error al leer los metadatos: {ex.Message}");
        }

        progress?.Report(ProgressInfo.Running(10, "Restauración", "Metadatos leídos correctamente..."));

        // Restaurar archivos
        int fileCount = 0;
        int totalFiles = metadata.BackedUpFiles.Count;

        foreach (var filePath in metadata.BackedUpFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string fileName = Path.GetFileName(filePath);
            string sourcePath = Path.Combine(backupDir, "Files", fileName);

            if (File.Exists(sourcePath))
            {
                // Crear el directorio destino si no existe
                string? destDir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                await _fileSystemHelper.CopyFileAsync(sourcePath, filePath);
            }

            fileCount++;
            int percentage = (int)((float)fileCount / totalFiles * 40) + 10; // 10-50%
            progress?.Report(ProgressInfo.Running(percentage, "Restauración", $"Restaurando archivos ({fileCount}/{totalFiles})..."));
        }

        // Restaurar claves de registro
        int regCount = 0;
        int totalRegs = metadata.BackedUpRegistryKeys.Count;

        for (int i = 0; i < totalRegs; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string regFileName = $"reg_{i}.reg";
            string regFilePath = Path.Combine(backupDir, "Registry", regFileName);

            if (File.Exists(regFilePath))
            {
                await Task.Run(() => _registryHelper.ImportRegistryFile(regFilePath), cancellationToken);
            }

            regCount++;
            int percentage = (int)((float)regCount / totalRegs * 40) + 50; // 50-90%
            progress?.Report(ProgressInfo.Running(percentage, "Restauración", $"Restaurando claves de registro ({regCount}/{totalRegs})..."));
        }

        progress?.Report(ProgressInfo.Completed("Restauración", "Restauración completada con éxito."));
        _logger.LogInfo($"Copia de seguridad {backupId} restaurada con éxito.");

        return OperationResult.SuccessResult($"Copia de seguridad {backupId} restaurada con éxito.");
    }
}
