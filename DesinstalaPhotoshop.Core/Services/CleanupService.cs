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
    private readonly IFileSystemHelper _fileSystemHelper;
    private readonly IRegistryHelper _registryHelper;
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CleanupService"/>.
    /// </summary>
    /// <param name="logger">Servicio de registro.</param>
    /// <param name="backupService">Servicio de respaldo.</param>
    /// <param name="fileSystemHelper">Ayudante para operaciones con el sistema de archivos.</param>
    /// <param name="registryHelper">Ayudante para operaciones con el registro.</param>
    public CleanupService(
        ILoggingService logger,
        IBackupService backupService,
        IFileSystemHelper fileSystemHelper,
        IRegistryHelper registryHelper)
    {
        _logger = logger;
        _backupService = backupService;
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
            
            // Limpiar archivos temporales
            if (cleanupTempFiles)
            {
                progress?.Report(ProgressInfo.Running(30, "Limpieza", "Limpiando archivos temporales..."));
                await CleanupTempFilesAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(40, "Limpieza", "Archivos temporales limpiados."));
            }
            
            // Limpiar entradas del registro
            if (cleanupRegistry)
            {
                progress?.Report(ProgressInfo.Running(50, "Limpieza", "Limpiando entradas del registro..."));
                await CleanupRegistryAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(60, "Limpieza", "Entradas del registro limpiadas."));
            }
            
            // Limpiar archivos de configuración
            if (cleanupConfigFiles)
            {
                progress?.Report(ProgressInfo.Running(70, "Limpieza", "Limpiando archivos de configuración..."));
                await CleanupConfigFilesAsync(installation, whatIf, progress, cancellationToken);
                progress?.Report(ProgressInfo.Running(80, "Limpieza", "Archivos de configuración limpiados."));
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
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Limpieza de archivos temporales completada.");
    }
    
    private async Task CleanupRegistryAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Limpieza de entradas del registro completada.");
    }
    
    private async Task CleanupConfigFilesAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Limpieza de archivos de configuración completada.");
    }
    
    private async Task CleanupCacheFilesAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Limpieza de archivos de caché completada.");
    }
}
