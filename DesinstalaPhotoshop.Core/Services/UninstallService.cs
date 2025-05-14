namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Helpers;
using DesinstalaPhotoshop.Core.Services.Interfaces;

/// <summary>
/// Implementa operaciones para desinstalar Adobe Photoshop.
/// </summary>
public class UninstallService : IUninstallService
{
    private readonly ILoggingService _logger;
    private readonly IFileSystemHelper _fileSystemHelper;
    private readonly IRegistryHelper _registryHelper;
    private readonly IBackupService _backupService;

    /// <summary>
    /// Inicializa una nueva instancia de la clase UninstallService.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    /// <param name="fileSystemHelper">Servicio auxiliar para operaciones con el sistema de archivos.</param>
    /// <param name="registryHelper">Servicio auxiliar para operaciones con el registro.</param>
    /// <param name="backupService">Servicio de copias de seguridad.</param>
    public UninstallService(
        ILoggingService logger,
        IFileSystemHelper fileSystemHelper,
        IRegistryHelper registryHelper,
        IBackupService backupService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystemHelper = fileSystemHelper ?? throw new ArgumentNullException(nameof(fileSystemHelper));
        _registryHelper = registryHelper ?? throw new ArgumentNullException(nameof(registryHelper));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
    }

    /// <summary>
    /// Desinstala una instalación de Adobe Photoshop.
    /// </summary>
    /// <param name="installation">Instalación a desinstalar.</param>
    /// <param name="createBackup">Indica si se debe crear una copia de seguridad antes de desinstalar.</param>
    /// <param name="whatIf">Indica si se debe simular la desinstalación sin realizar cambios reales.</param>
    /// <param name="removeUserData">Indica si se deben eliminar los datos de usuario.</param>
    /// <param name="removeSharedComponents">Indica si se deben eliminar componentes compartidos.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<OperationResult> UninstallAsync(
        PhotoshopInstallation installation,
        bool createBackup = true,
        bool whatIf = false,
        bool removeUserData = false,
        bool removeSharedComponents = false,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInfo($"Iniciando desinstalación de {installation.DisplayName}...");
        progress?.Report(ProgressInfo.Running(0, "Desinstalación", "Iniciando desinstalación..."));

        var result = new OperationResult { Success = true, Message = "Desinstalación completada con éxito." };

        try
        {
            // Verificar si la instalación puede ser desinstalada
            if (!CanUninstall(installation))
            {
                _logger.LogWarning($"La instalación {installation.DisplayName} no puede ser desinstalada automáticamente.");
                progress?.Report(ProgressInfo.Warning("Desinstalación", "Esta instalación no puede ser desinstalada automáticamente."));
                return OperationResult.Failed("Esta instalación no puede ser desinstalada automáticamente.");
            }

            // Crear copia de seguridad si es necesario
            if (createBackup)
            {
                progress?.Report(ProgressInfo.Running(10, "Desinstalación", "Creando copia de seguridad..."));
                var backupResult = await _backupService.CreateBackupAsync(installation, progress, cancellationToken);
                if (!backupResult.Success)
                {
                    _logger.LogWarning($"No se pudo crear la copia de seguridad: {backupResult.Message}");
                    progress?.Report(ProgressInfo.Warning("Desinstalación", $"No se pudo crear la copia de seguridad: {backupResult.Message}"));
                    // Continuar con la desinstalación aunque la copia de seguridad falle
                }
            }

            // Verificar si es una simulación
            if (whatIf)
            {
                _logger.LogInfo("Ejecutando en modo de simulación (WhatIf). No se realizarán cambios reales.");
                progress?.Report(ProgressInfo.Running(20, "Desinstalación", "Modo de simulación activado. No se realizarán cambios reales."));
            }

            // Obtener información del desinstalador
            var uninstallerInfo = GetUninstallerInfo(installation);
            if (uninstallerInfo == null)
            {
                _logger.LogWarning($"No se encontró un desinstalador para {installation.DisplayName}.");
                progress?.Report(ProgressInfo.Warning("Desinstalación", "No se encontró un desinstalador. Se intentará una desinstalación manual."));

                // Intentar desinstalación manual
                return await PerformManualUninstallAsync(installation, whatIf, removeUserData, removeSharedComponents, progress, cancellationToken);
            }

            // Ejecutar el desinstalador según su tipo
            progress?.Report(ProgressInfo.Running(30, "Desinstalación", "Ejecutando desinstalador..."));

            switch (uninstallerInfo.Type)
            {
                case UninstallerType.Executable:
                    result = await RunExecutableUninstallerAsync(uninstallerInfo, whatIf, progress, cancellationToken);
                    break;

                case UninstallerType.MSI:
                    result = await RunMsiUninstallerAsync(uninstallerInfo, whatIf, progress, cancellationToken);
                    break;

                case UninstallerType.CreativeCloud:
                    result = await RunCreativeCloudUninstallerAsync(uninstallerInfo, whatIf, progress, cancellationToken);
                    break;

                case UninstallerType.Manual:
                    result = await PerformManualUninstallAsync(installation, whatIf, removeUserData, removeSharedComponents, progress, cancellationToken);
                    break;

                default:
                    _logger.LogWarning($"Tipo de desinstalador desconocido: {uninstallerInfo.Type}");
                    result = OperationResult.Failed($"Tipo de desinstalador desconocido: {uninstallerInfo.Type}");
                    break;
            }

            // Limpiar datos de usuario si es necesario
            if (result.Success && removeUserData)
            {
                progress?.Report(ProgressInfo.Running(80, "Desinstalación", "Eliminando datos de usuario..."));
                await RemoveUserDataAsync(installation, whatIf, progress, cancellationToken);
            }

            // Limpiar componentes compartidos si es necesario
            if (result.Success && removeSharedComponents)
            {
                progress?.Report(ProgressInfo.Running(90, "Desinstalación", "Eliminando componentes compartidos..."));
                await RemoveSharedComponentsAsync(installation, whatIf, progress, cancellationToken);
            }

            // Finalizar desinstalación
            if (result.Success)
            {
                progress?.Report(ProgressInfo.Completed("Desinstalación", "Desinstalación completada con éxito."));
                _logger.LogInfo("Desinstalación completada con éxito.");
            }
            else
            {
                progress?.Report(ProgressInfo.Failed("Desinstalación", $"Error durante la desinstalación: {result.Message}"));
                _logger.LogError($"Error durante la desinstalación: {result.Message}");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Desinstalación cancelada por el usuario.");
            progress?.Report(ProgressInfo.Canceled("Desinstalación", "Desinstalación cancelada por el usuario."));
            return OperationResult.Canceled("Desinstalación cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la desinstalación: {ex.Message}");
            progress?.Report(ProgressInfo.Failed("Desinstalación", $"Error durante la desinstalación: {ex.Message}"));
            return OperationResult.Failed($"Error durante la desinstalación: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si una instalación puede ser desinstalada.
    /// </summary>
    /// <param name="installation">Instalación a verificar.</param>
    /// <returns>True si la instalación puede ser desinstalada, false en caso contrario.</returns>
    public bool CanUninstall(PhotoshopInstallation installation)
    {
        // Una instalación puede ser desinstalada si:
        // 1. Tiene un string de desinstalación válido, o
        // 2. Es una instalación principal con una ubicación válida

        if (!string.IsNullOrEmpty(installation.UninstallString))
        {
            return true;
        }

        if (installation.InstallationType == InstallationType.MainInstallation &&
            !string.IsNullOrEmpty(installation.InstallLocation) &&
            _fileSystemHelper.DirectoryExists(installation.InstallLocation))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Obtiene información sobre el desinstalador de una instalación.
    /// </summary>
    /// <param name="installation">Instalación a verificar.</param>
    /// <returns>Información sobre el desinstalador, o null si no se encuentra.</returns>
    public UninstallerInfo? GetUninstallerInfo(PhotoshopInstallation installation)
    {
        // Si no hay string de desinstalación, intentar buscar un desinstalador en la ubicación de instalación
        if (string.IsNullOrEmpty(installation.UninstallString))
        {
            return FindDesinstallerInInstallLocation(installation);
        }

        // Analizar el string de desinstalación
        string uninstallString = installation.UninstallString;

        // Verificar si es un desinstalador MSI
        if (uninstallString.Contains("MsiExec.exe", StringComparison.OrdinalIgnoreCase) ||
            uninstallString.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase))
        {
            return ParseMsiUninstallString(uninstallString);
        }

        // Verificar si es un desinstalador de Creative Cloud
        if (uninstallString.Contains("Creative Cloud", StringComparison.OrdinalIgnoreCase) ||
            uninstallString.Contains("Adobe Cloud", StringComparison.OrdinalIgnoreCase))
        {
            return new UninstallerInfo
            {
                Path = ExtractExecutablePath(uninstallString),
                Arguments = ExtractArguments(uninstallString),
                Type = UninstallerType.CreativeCloud
            };
        }

        // Desinstalador ejecutable estándar
        return new UninstallerInfo
        {
            Path = ExtractExecutablePath(uninstallString),
            Arguments = ExtractArguments(uninstallString),
            Type = UninstallerType.Executable
        };
    }

    // Métodos privados para implementar cada tipo de desinstalación

    private async Task<OperationResult> RunExecutableUninstallerAsync(
        UninstallerInfo uninstallerInfo,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Desinstalación con ejecutable completada.");
        return OperationResult.SuccessResult("Desinstalación con ejecutable completada con éxito.");
    }

    private async Task<OperationResult> RunMsiUninstallerAsync(
        UninstallerInfo uninstallerInfo,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Desinstalación MSI completada.");
        return OperationResult.SuccessResult("Desinstalación MSI completada con éxito.");
    }

    private async Task<OperationResult> RunCreativeCloudUninstallerAsync(
        UninstallerInfo uninstallerInfo,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Desinstalación de Creative Cloud completada.");
        return OperationResult.SuccessResult("Desinstalación de Creative Cloud completada con éxito.");
    }

    private async Task<OperationResult> PerformManualUninstallAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        bool removeUserData,
        bool removeSharedComponents,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Desinstalación manual completada.");
        return OperationResult.SuccessResult("Desinstalación manual completada con éxito.");
    }

    private async Task RemoveUserDataAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Eliminación de datos de usuario completada.");
    }

    private async Task RemoveSharedComponentsAsync(
        PhotoshopInstallation installation,
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        // Implementación pendiente
        await Task.Delay(500, cancellationToken); // Simulación
        _logger.LogInfo("Eliminación de componentes compartidos completada.");
    }

    // Métodos auxiliares

    private UninstallerInfo? FindDesinstallerInInstallLocation(PhotoshopInstallation installation)
    {
        if (string.IsNullOrEmpty(installation.InstallLocation) ||
            !_fileSystemHelper.DirectoryExists(installation.InstallLocation))
        {
            return null;
        }

        // Buscar archivos de desinstalación comunes
        var uninstallFiles = _fileSystemHelper.FindFiles(installation.InstallLocation, "uninstall*.exe");
        uninstallFiles.AddRange(_fileSystemHelper.FindFiles(installation.InstallLocation, "uninst*.exe"));
        uninstallFiles.AddRange(_fileSystemHelper.FindFiles(installation.InstallLocation, "setup.exe"));

        if (uninstallFiles.Count > 0)
        {
            return new UninstallerInfo
            {
                Path = uninstallFiles[0],
                Arguments = "/uninstall /silent",
                Type = UninstallerType.Executable
            };
        }

        return new UninstallerInfo
        {
            Path = installation.InstallLocation,
            Type = UninstallerType.Manual
        };
    }

    private UninstallerInfo ParseMsiUninstallString(string uninstallString)
    {
        var productCodeMatch = Regex.Match(uninstallString, @"\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}", RegexOptions.IgnoreCase);
        string productCode = productCodeMatch.Success ? productCodeMatch.Value : string.Empty;

        return new UninstallerInfo
        {
            Path = "msiexec.exe",
            Arguments = $"/x {productCode} /qb- /norestart",
            Type = UninstallerType.MSI,
            ProductCode = productCode
        };
    }

    private string ExtractExecutablePath(string commandLine)
    {
        // Extraer la ruta del ejecutable de una línea de comando
        if (string.IsNullOrEmpty(commandLine))
            return string.Empty;

        // Si está entre comillas, extraer el contenido
        var match = Regex.Match(commandLine, @"^""([^""]+)""");
        if (match.Success)
            return match.Groups[1].Value;

        // Si no está entre comillas, tomar hasta el primer espacio
        int spaceIndex = commandLine.IndexOf(' ');
        return spaceIndex > 0 ? commandLine.Substring(0, spaceIndex) : commandLine;
    }

    private string ExtractArguments(string commandLine)
    {
        // Extraer los argumentos de una línea de comando
        if (string.IsNullOrEmpty(commandLine))
            return string.Empty;

        // Si el ejecutable está entre comillas
        var match = Regex.Match(commandLine, @"^""[^""]+""(.*)");
        if (match.Success)
            return match.Groups[1].Value.Trim();

        // Si no está entre comillas
        int spaceIndex = commandLine.IndexOf(' ');
        return spaceIndex > 0 ? commandLine.Substring(spaceIndex + 1).Trim() : string.Empty;
    }
}
