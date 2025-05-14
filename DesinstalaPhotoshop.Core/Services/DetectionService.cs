namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Helpers;
using Microsoft.Win32;

/// <summary>
/// Implementación del servicio de detección de instalaciones de Adobe Photoshop.
/// </summary>
[SupportedOSPlatform("windows")]
public class DetectionService : IDetectionService
{
    private readonly ILoggingService _logger;
    private readonly IRegistryHelper _registryHelper;
    private readonly IFileSystemHelper _fileSystemHelper;

    /// <summary>
    /// Inicializa una nueva instancia de la clase DetectionService.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    /// <param name="registryHelper">Servicio auxiliar para operaciones con el registro.</param>
    /// <param name="fileSystemHelper">Servicio auxiliar para operaciones con el sistema de archivos.</param>
    public DetectionService(ILoggingService logger, IRegistryHelper registryHelper, IFileSystemHelper fileSystemHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registryHelper = registryHelper ?? throw new ArgumentNullException(nameof(registryHelper));
        _fileSystemHelper = fileSystemHelper ?? throw new ArgumentNullException(nameof(fileSystemHelper));
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase DetectionService con implementaciones predeterminadas.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    public DetectionService(ILoggingService logger)
        : this(logger, new RegistryHelper(logger), new FileSystemHelper(logger))
    {
    }

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

        try
        {
            // Método 1: Detección desde programas instalados
            _logger.LogInfo("Buscando instalaciones en programas instalados...");
            progress?.Report(ProgressInfo.Running(30, "Detectando instalaciones de Photoshop",
                "Buscando en programas instalados..."));

            var programsInstallations = DetectFromInstalledPrograms();
            installations.AddRange(programsInstallations);
            _logger.LogInfo($"Se encontraron {programsInstallations.Count} instalaciones en programas instalados.");

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Detección cancelada por el usuario.");
                return installations;
            }

            // Método 2: Detección desde el registro
            _logger.LogInfo("Buscando instalaciones en el registro de Windows...");
            progress?.Report(ProgressInfo.Running(60, "Detectando instalaciones de Photoshop",
                "Buscando en el registro de Windows..."));

            var registryInstallations = DetectFromRegistry();
            installations.AddRange(registryInstallations);
            _logger.LogInfo($"Se encontraron {registryInstallations.Count} instalaciones en el registro.");

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Detección cancelada por el usuario.");
                return installations;
            }

            // Método 3: Detección desde el sistema de archivos
            _logger.LogInfo("Buscando instalaciones en el sistema de archivos...");
            progress?.Report(ProgressInfo.Running(90, "Detectando instalaciones de Photoshop",
                "Buscando en el sistema de archivos..."));

            var filesystemInstallations = DetectFromFileSystem();
            installations.AddRange(filesystemInstallations);
            _logger.LogInfo($"Se encontraron {filesystemInstallations.Count} instalaciones en el sistema de archivos.");

            // Enriquecer y clasificar las instalaciones encontradas
            if (installations.Count > 0)
            {
                _logger.LogInfo($"Enriqueciendo información de {installations.Count} instalaciones encontradas...");

                for (int i = 0; i < installations.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    installations[i] = await EnrichInstallationInfoAsync(installations[i]);
                    installations[i] = ClassifyInstallation(installations[i]);
                }

                _logger.LogInfo("Detección de instalaciones completada.");
                progress?.Report(ProgressInfo.Completed("Detección de instalaciones",
                    $"Se encontraron {installations.Count} instalaciones de Photoshop."));
            }
            else
            {
                _logger.LogInfo("Detección de instalaciones completada. No se encontraron instalaciones.");
                progress?.Report(ProgressInfo.Completed("Detección de instalaciones",
                    "No se encontraron instalaciones de Photoshop."));
            }

            return installations;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Detección cancelada por el usuario.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la detección de instalaciones: {ex.Message}");
            progress?.Report(ProgressInfo.Error("Detección de instalaciones",
                $"Error durante la detección: {ex.Message}"));
            throw;
        }
    }

    /// <summary>
    /// Clasifica una instalación de Photoshop según su estado y completitud.
    /// </summary>
    /// <param name="installation">Instalación a clasificar.</param>
    /// <returns>La instalación con su tipo actualizado.</returns>
    public PhotoshopInstallation ClassifyInstallation(PhotoshopInstallation installation)
    {
        _logger.LogInfo($"Clasificando instalación: {installation.DisplayName}");

        // Verificar si la ubicación de instalación existe
        bool locationExists = !string.IsNullOrEmpty(installation.InstallLocation) &&
                             _fileSystemHelper.DirectoryExists(installation.InstallLocation);

        // Verificar si tiene el ejecutable principal
        bool hasMainExecutable = false;
        if (locationExists)
        {
            var exeFiles = _fileSystemHelper.FindFiles(installation.InstallLocation, "Photoshop*.exe");
            hasMainExecutable = exeFiles.Count > 0;
        }
        else
        {
            // También verificar si tiene algún ejecutable de Photoshop en los archivos asociados
            hasMainExecutable = installation.AssociatedFiles.Any(f =>
                Path.GetFileName(f).StartsWith("Photoshop", StringComparison.OrdinalIgnoreCase) &&
                Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase));
        }

        // Verificar si tiene un desinstalador válido
        bool hasValidUninstaller = false;
        if (!string.IsNullOrEmpty(installation.UninstallString))
        {
            // Extraer la ruta del desinstalador
            string uninstallerPath = ExtractUninstallerPath(installation.UninstallString);
            hasValidUninstaller = _fileSystemHelper.FileExists(uninstallerPath);
        }

        // Por defecto, marcar como desconocido
        installation.InstallationType = InstallationType.Unknown;
        installation.IsMainInstallation = false;
        installation.IsResidual = false;

        // Lógica de clasificación según el sistema de puntuación heurística documentado
        if (installation.ConfidenceScore >= 50 && (locationExists || hasMainExecutable))
        {
            installation.InstallationType = InstallationType.MainInstallation;
            installation.IsMainInstallation = true;
            installation.IsResidual = false;
            _logger.LogInfo($"Clasificada como instalación principal: {installation.DisplayName}");
        }
        else if (installation.ConfidenceScore >= 30 && locationExists)
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
            _logger.LogInfo($"Clasificada como residual: {installation.DisplayName}");
        }

        return installation;
    }

    /// <summary>
    /// Extrae la ruta del desinstalador a partir del string de desinstalación.
    /// </summary>
    /// <param name="uninstallString">String de desinstalación.</param>
    /// <returns>Ruta del desinstalador.</returns>
    private string ExtractUninstallerPath(string uninstallString)
    {
        if (string.IsNullOrEmpty(uninstallString))
            return string.Empty;

        string uninstallerPath = uninstallString;

        // Si el string comienza con comillas, extraer la parte entre comillas
        if (uninstallerPath.StartsWith("\""))
        {
            var endQuoteIndex = uninstallerPath.IndexOf('"', 1);
            if (endQuoteIndex > 0)
            {
                uninstallerPath = uninstallerPath.Substring(1, endQuoteIndex - 1);
            }
        }
        // Si no tiene comillas pero tiene espacios, puede tener parámetros
        else if (uninstallerPath.Contains(" "))
        {
            uninstallerPath = uninstallerPath.Split(' ')[0];
        }

        return uninstallerPath;
    }

    /// <summary>
    /// Enriquece la información de una instalación detectada con datos adicionales.
    /// </summary>
    /// <param name="installation">Instalación a enriquecer.</param>
    /// <returns>La instalación con información adicional.</returns>
    public async Task<PhotoshopInstallation> EnrichInstallationInfoAsync(PhotoshopInstallation installation)
    {
        _logger.LogInfo($"Enriqueciendo información de instalación: {installation.DisplayName}");

        // Inicializar puntuación
        int score = 0;

        // Verificar si tiene ubicación de instalación
        bool locationExists = false;
        if (!string.IsNullOrEmpty(installation.InstallLocation))
        {
            locationExists = _fileSystemHelper.DirectoryExists(installation.InstallLocation);
            if (locationExists)
            {
                score += 30;
                _logger.LogDebug($"Puntuación +30 por tener ubicación de instalación válida: {installation.InstallLocation}");
            }
            else
            {
                score -= 5; // Penalización por tener una ruta que no existe
                _logger.LogDebug($"Penalización -5 por tener una ruta que no existe: {installation.InstallLocation}");
            }
        }

        // Verificar si tiene el ejecutable principal
        bool hasMainExecutable = false;
        if (locationExists)
        {
            var exeFiles = _fileSystemHelper.FindFiles(installation.InstallLocation, "Photoshop*.exe");
            hasMainExecutable = exeFiles.Count > 0;

            if (hasMainExecutable)
            {
                score += 30;
                _logger.LogDebug($"Puntuación +30 por tener ejecutable principal");

                // Agregar los ejecutables a los archivos asociados
                foreach (var exeFile in exeFiles)
                {
                    if (!installation.AssociatedFiles.Contains(exeFile))
                    {
                        installation.AssociatedFiles.Add(exeFile);
                    }
                }
            }
            else
            {
                score -= 20;
                _logger.LogDebug($"Penalización -20 por no tener ejecutable principal");
            }
        }

        // Verificar si tiene string de desinstalación
        bool hasValidUninstaller = false;
        if (!string.IsNullOrEmpty(installation.UninstallString))
        {
            // Extraer la ruta del archivo del string de desinstalación (puede contener parámetros)
            string uninstallerPath = installation.UninstallString;

            // Si el string comienza con comillas, extraer la parte entre comillas
            if (uninstallerPath.StartsWith("\""))
            {
                var endQuoteIndex = uninstallerPath.IndexOf('"', 1);
                if (endQuoteIndex > 0)
                {
                    uninstallerPath = uninstallerPath.Substring(1, endQuoteIndex - 1);
                }
            }
            // Si no tiene comillas pero tiene espacios, puede tener parámetros
            else if (uninstallerPath.Contains(" "))
            {
                uninstallerPath = uninstallerPath.Split(' ')[0];
            }

            // Verificar si el archivo existe
            hasValidUninstaller = _fileSystemHelper.FileExists(uninstallerPath);
            if (hasValidUninstaller)
            {
                score += 20;
                _logger.LogDebug($"Puntuación +20 por tener desinstalador válido: {uninstallerPath}");

                // Agregar el desinstalador a los archivos asociados
                if (!installation.AssociatedFiles.Contains(uninstallerPath))
                {
                    installation.AssociatedFiles.Add(uninstallerPath);
                }
            }
            else
            {
                score -= 10;
                _logger.LogDebug($"Penalización -10 por tener desinstalador inexistente: {uninstallerPath}");
            }
        }

        // Verificar si tiene versión
        if (!string.IsNullOrEmpty(installation.Version))
        {
            score += 10;
            _logger.LogDebug($"Puntuación +10 por tener información de versión: {installation.Version}");
        }

        // Verificar si tiene fecha de instalación
        if (installation.InstallDate.HasValue)
        {
            score += 5;
            _logger.LogDebug($"Puntuación +5 por tener fecha de instalación: {installation.InstallDate}");
        }

        // Verificar si tiene archivos asociados
        if (installation.AssociatedFiles.Count > 0)
        {
            score += 5;
            _logger.LogDebug($"Puntuación +5 por tener {installation.AssociatedFiles.Count} archivos asociados");
        }

        // Verificar si tiene claves de registro
        if (installation.AssociatedRegistryKeys.Count > 0)
        {
            score += 5;
            _logger.LogDebug($"Puntuación +5 por tener {installation.AssociatedRegistryKeys.Count} claves de registro");
        }

        // Verificar si está en Common Files o AppData (probablemente residuos)
        if (!string.IsNullOrEmpty(installation.InstallLocation) &&
            (installation.InstallLocation.Contains("Common Files", StringComparison.OrdinalIgnoreCase) ||
             installation.InstallLocation.Contains("AppData", StringComparison.OrdinalIgnoreCase)))
        {
            score -= 15; // Penalización por estar en Common Files o AppData
            _logger.LogDebug($"Penalización -15 por estar en Common Files o AppData");
        }

        // Verificar presencia de carpetas de configuración de plugins UXP
        await Task.Run(() => {
            try
            {
                // Extraer versión numérica (si es posible)
                string numericVersion = string.Empty;
                if (!string.IsNullOrEmpty(installation.Version))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(installation.Version, @"\d+\.\d+");
                    if (match.Success)
                    {
                        numericVersion = match.Value;
                    }
                }

                if (!string.IsNullOrEmpty(numericVersion))
                {
                    string uxpDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Adobe", "UXP", "PluginsStorage", "PHSP", numericVersion, "External");

                    if (_fileSystemHelper.DirectoryExists(uxpDataPath) &&
                        _fileSystemHelper.FindDirectories(uxpDataPath, "*").Count > 0)
                    {
                        score += 10; // Puntos por tener datos de plugins UXP
                        _logger.LogDebug($"Puntuación +10 por tener datos de plugins UXP");

                        // Agregar la carpeta a los archivos asociados
                        if (!installation.AssociatedFiles.Contains(uxpDataPath))
                        {
                            installation.AssociatedFiles.Add(uxpDataPath);
                        }
                    }
                }

                // Verificar presencia de archivos de caché de fuentes
                string fontCachePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Adobe", "CT Font Cache");

                if (_fileSystemHelper.DirectoryExists(fontCachePath))
                {
                    score += 5; // Puntos por tener caché de fuentes
                    _logger.LogDebug($"Puntuación +5 por tener caché de fuentes");

                    // Agregar la carpeta a los archivos asociados
                    if (!installation.AssociatedFiles.Contains(fontCachePath))
                    {
                        installation.AssociatedFiles.Add(fontCachePath);
                    }
                }

                // Verificar presencia de archivos de autorecuperación recientes
                string autoRecoverPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Adobe", "Adobe Photoshop", numericVersion, "AutoRecover");

                if (_fileSystemHelper.DirectoryExists(autoRecoverPath) &&
                    _fileSystemHelper.FindFiles(autoRecoverPath, "*.psb").Count > 0)
                {
                    score += 10; // Puntos por tener archivos de autorecuperación recientes
                    _logger.LogDebug($"Puntuación +10 por tener archivos de autorecuperación recientes");

                    // Agregar la carpeta a los archivos asociados
                    if (!installation.AssociatedFiles.Contains(autoRecoverPath))
                    {
                        installation.AssociatedFiles.Add(autoRecoverPath);
                    }
                }

                // Verificar si solo tiene datos de licenciamiento
                string oobePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Adobe", "OOBE");

                string slStorePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Adobe", "SLStore_v1");

                bool hasOnlyLicensingData = (_fileSystemHelper.DirectoryExists(oobePath) ||
                                           _fileSystemHelper.DirectoryExists(slStorePath)) &&
                                           !hasMainExecutable && !hasValidUninstaller;

                if (hasOnlyLicensingData)
                {
                    score -= 10; // Penalización por tener solo datos de licenciamiento
                    _logger.LogDebug($"Penalización -10 por tener solo datos de licenciamiento");

                    // Agregar las carpetas a los archivos asociados
                    if (_fileSystemHelper.DirectoryExists(oobePath) &&
                        !installation.AssociatedFiles.Contains(oobePath))
                    {
                        installation.AssociatedFiles.Add(oobePath);
                    }

                    if (_fileSystemHelper.DirectoryExists(slStorePath) &&
                        !installation.AssociatedFiles.Contains(slStorePath))
                    {
                        installation.AssociatedFiles.Add(slStorePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al verificar datos adicionales: {ex.Message}");
            }
        });

        // Limitar la puntuación a 100 y asegurar que no sea negativa
        installation.ConfidenceScore = Math.Clamp(score, 0, 100);
        _logger.LogInfo($"Puntuación de confianza calculada: {installation.ConfidenceScore}");

        return installation;
    }

    // Métodos privados que serán implementados en futuras iteraciones
    private List<PhotoshopInstallation> DetectFromInstalledPrograms()
    {
        _logger.LogInfo("Ejecutando detección desde programas instalados...");

        try
        {
            // La detección desde programas instalados utiliza el mismo mecanismo que la detección desde el registro,
            // pero filtra específicamente por instalaciones que tienen un desinstalador válido
            var allRegistryInstallations = _registryHelper.FindPhotoshopInstallations();

            // Filtrar solo las instalaciones que tienen un string de desinstalación válido
            var programInstallations = allRegistryInstallations
                .Where(i => !string.IsNullOrEmpty(i.UninstallString))
                .ToList();

            // Marcar estas instalaciones como detectadas por el método de programas instalados
            foreach (var installation in programInstallations)
            {
                installation.DetectedBy = DetectionMethod.InstalledPrograms;

                // Verificar si el desinstalador existe como archivo
                if (!string.IsNullOrEmpty(installation.UninstallString))
                {
                    // Extraer la ruta del archivo del string de desinstalación (puede contener parámetros)
                    string uninstallerPath = installation.UninstallString;

                    // Si el string comienza con comillas, extraer la parte entre comillas
                    if (uninstallerPath.StartsWith("\""))
                    {
                        var endQuoteIndex = uninstallerPath.IndexOf('"', 1);
                        if (endQuoteIndex > 0)
                        {
                            uninstallerPath = uninstallerPath.Substring(1, endQuoteIndex - 1);
                        }
                    }
                    // Si no tiene comillas pero tiene espacios, puede tener parámetros
                    else if (uninstallerPath.Contains(" "))
                    {
                        uninstallerPath = uninstallerPath.Split(' ')[0];
                    }

                    // Verificar si el archivo existe
                    if (_fileSystemHelper.FileExists(uninstallerPath))
                    {
                        _logger.LogInfo($"Verificado desinstalador existente: {uninstallerPath}");
                        installation.ConfidenceScore += 10; // Aumentar confianza si el desinstalador existe
                    }
                    else
                    {
                        _logger.LogWarning($"Desinstalador no encontrado: {uninstallerPath}");
                    }
                }
            }

            if (programInstallations.Count > 0)
            {
                _logger.LogInfo($"Se encontraron {programInstallations.Count} instalaciones en programas instalados.");

                // Registrar detalles de cada instalación encontrada
                foreach (var installation in programInstallations)
                {
                    _logger.LogInfo($"Programa instalado: {installation.DisplayName}, Versión: {installation.Version}");
                    _logger.LogInfo($"  Ubicación: {installation.InstallLocation}");
                    _logger.LogInfo($"  Desinstalador: {installation.UninstallString}");
                    _logger.LogInfo($"  Puntuación: {installation.ConfidenceScore}");
                }
            }
            else
            {
                _logger.LogInfo("No se encontraron instalaciones de Photoshop en programas instalados.");
            }

            return programInstallations;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la detección desde programas instalados: {ex.Message}");
            return new List<PhotoshopInstallation>();
        }
    }

    private List<PhotoshopInstallation> DetectFromRegistry()
    {
        _logger.LogInfo("Ejecutando detección desde el registro de Windows...");
        var installations = new List<PhotoshopInstallation>();

        try
        {
            // 1. Utilizar el RegistryHelper para buscar instalaciones en el registro
            var registryInstallations = _registryHelper.FindPhotoshopInstallations();
            installations.AddRange(registryInstallations);

            // 2. Buscar claves específicas de Photoshop según ResiduosDePhotoshop.md
            SearchPhotoshopSpecificKeys(installations);

            // 3. Buscar asociaciones de archivos
            SearchFileAssociations(installations);

            if (installations.Count > 0)
            {
                _logger.LogInfo($"Se encontraron {installations.Count} instalaciones de Photoshop en el registro.");

                // Registrar detalles de cada instalación encontrada
                foreach (var installation in installations)
                {
                    _logger.LogInfo($"Instalación encontrada: {installation.DisplayName}, Versión: {installation.Version}");
                    _logger.LogInfo($"  Ubicación: {installation.InstallLocation}");
                    _logger.LogInfo($"  Desinstalador: {installation.UninstallString ?? "No disponible"}");
                    _logger.LogInfo($"  Puntuación inicial: {installation.ConfidenceScore}");
                    _logger.LogInfo($"  Claves de registro asociadas: {installation.AssociatedRegistryKeys.Count}");
                }
            }
            else
            {
                _logger.LogInfo("No se encontraron instalaciones de Photoshop en el registro.");
            }

            return installations;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la detección desde el registro: {ex.Message}");
            return new List<PhotoshopInstallation>();
        }
    }

    /// <summary>
    /// Busca claves específicas de Photoshop en el registro según la documentación.
    /// </summary>
    /// <param name="installations">Lista de instalaciones donde se agregarán las encontradas.</param>
    private void SearchPhotoshopSpecificKeys(List<PhotoshopInstallation> installations)
    {
        _logger.LogInfo("Buscando claves específicas de Photoshop en el registro...");

        try
        {
            // Claves específicas de Photoshop según ResiduosDePhotoshop.md
            string[] photoshopKeys = new[]
            {
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Adobe\Photoshop",
                @"HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop",
                @"HKEY_CURRENT_USER\SOFTWARE\Adobe\Camera Raw",
                @"HKEY_CURRENT_USER\SOFTWARE\Adobe\Bridge"
            };

            foreach (var keyPath in photoshopKeys)
            {
                if (_registryHelper.KeyExists(keyPath))
                {
                    _logger.LogInfo($"Encontrada clave específica de Photoshop: {keyPath}");

                    // Buscar versiones dentro de esta clave
                    var versionKeys = GetVersionSubkeys(keyPath);
                    foreach (var versionKey in versionKeys)
                    {
                        string version = ExtractVersionFromKey(versionKey);
                        string displayName = $"Adobe Photoshop {version}".Trim();

                        // Buscar información de instalación
                        string? installLocation = _registryHelper.GetRegistryValue(versionKey, "ApplicationPath") as string;
                        if (string.IsNullOrEmpty(installLocation))
                        {
                            installLocation = _registryHelper.GetRegistryValue(versionKey, "InstallPath") as string;
                        }

                        // Verificar si ya existe una instalación con esta versión o ubicación
                        var existingInstallation = installations.FirstOrDefault(i =>
                            (!string.IsNullOrEmpty(version) && i.Version == version) ||
                            (!string.IsNullOrEmpty(installLocation) && i.InstallLocation == installLocation));

                        if (existingInstallation != null)
                        {
                            // Actualizar la instalación existente
                            _logger.LogInfo($"Actualizando instalación existente con información de clave específica: {existingInstallation.DisplayName}");

                            if (!string.IsNullOrEmpty(installLocation) && string.IsNullOrEmpty(existingInstallation.InstallLocation))
                            {
                                existingInstallation.InstallLocation = installLocation;
                            }

                            if (!existingInstallation.AssociatedRegistryKeys.Contains(versionKey))
                            {
                                existingInstallation.AssociatedRegistryKeys.Add(versionKey);
                            }

                            existingInstallation.ConfidenceScore += 5; // Aumentar confianza por encontrar más evidencia
                        }
                        else if (!string.IsNullOrEmpty(installLocation) || keyPath.Contains("Camera Raw") || keyPath.Contains("Bridge"))
                        {
                            // Crear una nueva instalación
                            var newInstallation = new PhotoshopInstallation
                            {
                                DisplayName = displayName,
                                Version = version,
                                InstallLocation = installLocation ?? string.Empty,
                                DetectedBy = DetectionMethod.Registry,
                                ConfidenceScore = 40, // Confianza media-baja para detecciones de claves específicas sin más información
                                AssociatedRegistryKeys = new List<string> { versionKey }
                            };

                            // Si es Camera Raw o Bridge, marcar como residual
                            if (keyPath.Contains("Camera Raw") || keyPath.Contains("Bridge"))
                            {
                                newInstallation.IsResidual = true;
                                newInstallation.Notes = "Detectado a través de componentes asociados (Camera Raw/Bridge)";
                            }

                            installations.Add(newInstallation);
                            _logger.LogInfo($"Nueva instalación de Photoshop detectada en clave específica: {newInstallation.DisplayName}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar claves específicas de Photoshop: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca asociaciones de archivos de Photoshop en el registro.
    /// </summary>
    /// <param name="installations">Lista de instalaciones donde se agregarán las encontradas.</param>
    private void SearchFileAssociations(List<PhotoshopInstallation> installations)
    {
        _logger.LogInfo("Buscando asociaciones de archivos de Photoshop...");

        try
        {
            // Extensiones comunes asociadas con Photoshop según ResiduosDePhotoshop.md
            string[] extensions = { ".psd", ".psb", ".pdd", ".abr", ".atn", ".pat" };

            foreach (var extension in extensions)
            {
                string keyPath = $"HKEY_CLASSES_ROOT\\{extension}";
                if (_registryHelper.KeyExists(keyPath))
                {
                    _logger.LogInfo($"Encontrada asociación de archivo: {keyPath}");

                    // Obtener el ProgID asociado
                    var progId = _registryHelper.GetRegistryValue(keyPath, null) as string;
                    if (!string.IsNullOrEmpty(progId) && progId.Contains("Photoshop", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInfo($"Asociación de {extension} con Photoshop: {progId}");

                        // Buscar el comando de apertura
                        string commandKeyPath = $"HKEY_CLASSES_ROOT\\{progId}\\shell\\open\\command";
                        if (_registryHelper.KeyExists(commandKeyPath))
                        {
                            var command = _registryHelper.GetRegistryValue(commandKeyPath, null) as string;
                            if (!string.IsNullOrEmpty(command) && command.Contains("Photoshop", StringComparison.OrdinalIgnoreCase))
                            {
                                // Extraer la ruta del ejecutable
                                string exePath = ExtractExecutablePath(command);
                                if (!string.IsNullOrEmpty(exePath) && _fileSystemHelper.FileExists(exePath))
                                {
                                    string? installDir = Path.GetDirectoryName(exePath);
                                    if (!string.IsNullOrEmpty(installDir))
                                    {
                                        // Verificar si ya existe una instalación con esta ubicación
                                        var existingInstallation = installations.FirstOrDefault(i =>
                                            string.Equals(i.InstallLocation, installDir, StringComparison.OrdinalIgnoreCase));

                                        if (existingInstallation != null)
                                        {
                                            // Actualizar la instalación existente
                                            if (!existingInstallation.AssociatedRegistryKeys.Contains(keyPath))
                                            {
                                                existingInstallation.AssociatedRegistryKeys.Add(keyPath);
                                            }
                                            if (!existingInstallation.AssociatedRegistryKeys.Contains(commandKeyPath))
                                            {
                                                existingInstallation.AssociatedRegistryKeys.Add(commandKeyPath);
                                            }
                                            if (!existingInstallation.AssociatedFiles.Contains(exePath))
                                            {
                                                existingInstallation.AssociatedFiles.Add(exePath);
                                            }

                                            existingInstallation.ConfidenceScore += 3; // Aumentar confianza por tener asociaciones
                                            _logger.LogInfo($"Actualizada instalación existente con asociación de archivos: {existingInstallation.DisplayName}");
                                        }
                                        else
                                        {
                                            // Extraer versión del nombre del archivo o directorio
                                            string version = ExtractVersionFromPath(installDir);
                                            string displayName = $"Adobe Photoshop {version}".Trim();

                                            var newInstallation = new PhotoshopInstallation
                                            {
                                                DisplayName = displayName,
                                                Version = version,
                                                InstallLocation = installDir,
                                                DetectedBy = DetectionMethod.Registry,
                                                ConfidenceScore = 50, // Confianza media para detecciones de asociaciones con ejecutable
                                                AssociatedRegistryKeys = new List<string> { keyPath, commandKeyPath },
                                                AssociatedFiles = new List<string> { exePath }
                                            };

                                            installations.Add(newInstallation);
                                            _logger.LogInfo($"Nueva instalación de Photoshop detectada desde asociación de archivos: {newInstallation.DisplayName}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Verificar también las asociaciones de usuario
                string userExtKeyPath = $"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{extension}";
                if (_registryHelper.KeyExists(userExtKeyPath))
                {
                    _logger.LogInfo($"Encontrada asociación de usuario para {extension}");

                    // Añadir esta clave a las instalaciones existentes que tengan asociaciones con esta extensión
                    foreach (var installation in installations)
                    {
                        if (installation.AssociatedRegistryKeys.Any(k => k.Contains(extension)))
                        {
                            if (!installation.AssociatedRegistryKeys.Contains(userExtKeyPath))
                            {
                                installation.AssociatedRegistryKeys.Add(userExtKeyPath);
                                _logger.LogInfo($"Añadida asociación de usuario para {extension} a {installation.DisplayName}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar asociaciones de archivos: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene las subclaves de versión de una clave de registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave principal.</param>
    /// <returns>Lista de rutas de subclaves de versión.</returns>
    private List<string> GetVersionSubkeys(string keyPath)
    {
        var result = new List<string>();

        try
        {
            // Obtener las subclaves
            var subKeyNames = GetSubKeyNames(keyPath);
            if (subKeyNames == null || subKeyNames.Length == 0)
                return result;

            foreach (var subKeyName in subKeyNames)
            {
                // Las versiones de Photoshop suelen tener claves con nombres numéricos (ej. "23.0")
                // o años (ej. "2023")
                if (IsVersionKey(subKeyName))
                {
                    result.Add($"{keyPath}\\{subKeyName}");
                }
                else
                {
                    // Buscar recursivamente en subclaves que no son versiones
                    var nestedVersions = GetVersionSubkeys($"{keyPath}\\{subKeyName}");
                    result.AddRange(nestedVersions);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener subclaves de versión para {keyPath}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Verifica si una clave es una clave de versión.
    /// </summary>
    /// <param name="keyName">Nombre de la clave.</param>
    /// <returns>true si es una clave de versión, false en caso contrario.</returns>
    private bool IsVersionKey(string keyName)
    {
        // Versiones numéricas (ej. "23.0")
        if (System.Text.RegularExpressions.Regex.IsMatch(keyName, @"^\d+\.\d+$"))
            return true;

        // Años (ej. "2023")
        if (System.Text.RegularExpressions.Regex.IsMatch(keyName, @"^20\d{2}$"))
            return true;

        // Versiones CC (ej. "CC 2023")
        if (keyName.StartsWith("CC", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Extrae la versión de una clave de registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <returns>Versión extraída o cadena vacía si no se puede determinar.</returns>
    private string ExtractVersionFromKey(string keyPath)
    {
        try
        {
            // Obtener el nombre de la clave (última parte de la ruta)
            string keyName = keyPath.Split('\\').Last();

            // Versiones numéricas (ej. "23.0")
            if (System.Text.RegularExpressions.Regex.IsMatch(keyName, @"^\d+\.\d+$"))
                return keyName;

            // Años (ej. "2023")
            if (System.Text.RegularExpressions.Regex.IsMatch(keyName, @"^20\d{2}$"))
                return keyName;

            // Versiones CC (ej. "CC 2023")
            if (keyName.StartsWith("CC", StringComparison.OrdinalIgnoreCase))
                return keyName;

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Extrae la ruta del ejecutable de un comando de apertura.
    /// </summary>
    /// <param name="command">Comando de apertura.</param>
    /// <returns>Ruta del ejecutable o cadena vacía si no se puede extraer.</returns>
    private string ExtractExecutablePath(string command)
    {
        try
        {
            // Los comandos suelen tener el formato: "C:\Path\To\Photoshop.exe" "%1"
            var match = System.Text.RegularExpressions.Regex.Match(command, @"""([^""]+)""");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Si no hay comillas, intentar extraer hasta el primer espacio
            int spaceIndex = command.IndexOf(' ');
            if (spaceIndex > 0)
            {
                return command.Substring(0, spaceIndex);
            }

            return command; // Devolver el comando completo si no hay espacios
        }
        catch
        {
            return string.Empty;
        }
    }

    private List<PhotoshopInstallation> DetectFromFileSystem()
    {
        _logger.LogInfo("Ejecutando detección desde el sistema de archivos...");

        var installations = new List<PhotoshopInstallation>();

        try
        {
            // Rutas comunes donde se instala Photoshop
            var commonPaths = new[]
            {
                @"C:\Program Files\Adobe",
                @"C:\Program Files (x86)\Adobe",
                @"C:\Program Files\Common Files\Adobe",
                @"C:\Program Files (x86)\Common Files\Adobe"
            };

            // Buscar en rutas comunes
            foreach (var basePath in commonPaths)
            {
                if (!_fileSystemHelper.DirectoryExists(basePath))
                {
                    _logger.LogInfo($"Directorio {basePath} no encontrado, omitiendo...");
                    continue;
                }

                _logger.LogInfo($"Buscando en directorio: {basePath}");

                // Buscar directorios que contengan "Photoshop"
                var photoshopDirs = _fileSystemHelper.FindDirectories(basePath, "Photoshop");

                foreach (var dir in photoshopDirs)
                {
                    _logger.LogInfo($"Directorio potencial de Photoshop encontrado: {dir}");

                    // Verificar si contiene archivos ejecutables de Photoshop
                    var exeFiles = _fileSystemHelper.FindFiles(dir, "Photoshop*.exe");

                    if (exeFiles.Count > 0)
                    {
                        _logger.LogInfo($"Encontrado ejecutable de Photoshop en: {dir}");

                        // Extraer versión del nombre del directorio (si es posible)
                        string version = ExtractVersionFromPath(dir);
                        string displayName = $"Adobe Photoshop {version}".Trim();

                        // Crear una nueva instalación
                        var installation = new PhotoshopInstallation
                        {
                            DisplayName = displayName,
                            Version = version,
                            InstallLocation = dir,
                            EstimatedSize = _fileSystemHelper.GetDirectorySize(dir),
                            DetectedBy = DetectionMethod.FileSystem,
                            ConfidenceScore = 70, // Confianza alta para detecciones de sistema de archivos con ejecutable
                            AssociatedFiles = new List<string> { dir }
                        };

                        // Agregar los ejecutables a los archivos asociados
                        foreach (var exeFile in exeFiles)
                        {
                            if (!installation.AssociatedFiles.Contains(exeFile))
                            {
                                installation.AssociatedFiles.Add(exeFile);
                            }
                        }

                        installations.Add(installation);
                        _logger.LogInfo($"Instalación detectada en sistema de archivos: {displayName}");
                    }
                }
            }

            // Buscar en ubicaciones adicionales según ResiduosDePhotoshop.md
            SearchAdditionalLocations(installations);

            if (installations.Count > 0)
            {
                _logger.LogInfo($"Se encontraron {installations.Count} instalaciones de Photoshop en el sistema de archivos.");
            }
            else
            {
                _logger.LogInfo("No se encontraron instalaciones de Photoshop en el sistema de archivos.");
            }

            return installations;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la detección desde el sistema de archivos: {ex.Message}");
            return new List<PhotoshopInstallation>();
        }
    }

    /// <summary>
    /// Busca instalaciones de Photoshop en ubicaciones adicionales.
    /// </summary>
    /// <param name="installations">Lista de instalaciones donde se agregarán las encontradas.</param>
    private void SearchAdditionalLocations(List<PhotoshopInstallation> installations)
    {
        try
        {
            // Ubicaciones adicionales donde pueden existir residuos de Photoshop
            var additionalLocations = new Dictionary<string, string>
            {
                // Datos de usuario
                { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Adobe"), "AppData\\Roaming\\Adobe" },
                { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Adobe"), "AppData\\Local\\Adobe" },

                // Datos de sistema
                { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Adobe"), "ProgramData\\Adobe" },

                // Carpetas de documentos
                { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Adobe"), "Documents\\Adobe" }
            };

            foreach (var location in additionalLocations)
            {
                string path = location.Key;
                string displayPath = location.Value;

                if (!_fileSystemHelper.DirectoryExists(path))
                {
                    _logger.LogInfo($"Directorio adicional {displayPath} no encontrado, omitiendo...");
                    continue;
                }

                _logger.LogInfo($"Buscando en directorio adicional: {displayPath}");

                // Buscar directorios que contengan "Photoshop"
                var photoshopDirs = _fileSystemHelper.FindDirectories(path, "Photoshop");

                if (photoshopDirs.Count > 0)
                {
                    _logger.LogInfo($"Encontrados {photoshopDirs.Count} directorios relacionados con Photoshop en {displayPath}");

                    // Verificar si ya existe una instalación que incluya estos directorios
                    bool addedToExisting = false;

                    foreach (var dir in photoshopDirs)
                    {
                        // Extraer versión del nombre del directorio (si es posible)
                        string version = ExtractVersionFromPath(dir);

                        // Buscar una instalación existente con la misma versión
                        var existingInstallation = installations.FirstOrDefault(i =>
                            !string.IsNullOrEmpty(i.Version) && i.Version == version);

                        if (existingInstallation != null)
                        {
                            // Agregar el directorio a los archivos asociados de la instalación existente
                            if (!existingInstallation.AssociatedFiles.Contains(dir))
                            {
                                existingInstallation.AssociatedFiles.Add(dir);
                                addedToExisting = true;
                                _logger.LogInfo($"Directorio {dir} agregado a instalación existente: {existingInstallation.DisplayName}");
                            }
                        }
                    }

                    // Si no se agregó a ninguna instalación existente, crear una nueva instalación de tipo residual
                    if (!addedToExisting)
                    {
                        // Crear una nueva instalación de tipo residual
                        var installation = new PhotoshopInstallation
                        {
                            DisplayName = $"Adobe Photoshop (Residuos en {displayPath})",
                            Version = string.Empty,
                            InstallLocation = path,
                            DetectedBy = DetectionMethod.FileSystem,
                            ConfidenceScore = 30, // Confianza baja para detecciones de sistema de archivos sin ejecutable
                            AssociatedFiles = new List<string>(photoshopDirs),
                            IsResidual = true
                        };

                        installations.Add(installation);
                        _logger.LogInfo($"Residuos de Photoshop detectados en {displayPath}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar en ubicaciones adicionales: {ex.Message}");
        }
    }

    /// <summary>
    /// Extrae la versión de Photoshop a partir de la ruta de instalación.
    /// </summary>
    /// <param name="path">Ruta de instalación.</param>
    /// <returns>Versión extraída o cadena vacía si no se puede determinar.</returns>
    private string ExtractVersionFromPath(string path)
    {
        try
        {
            // Intentar extraer la versión del nombre del directorio
            var dirName = Path.GetFileName(path);

            // Patrones comunes: "Adobe Photoshop CC 2020", "Photoshop 2021", etc.
            if (dirName.Contains("CC"))
            {
                // Extraer año después de "CC"
                var ccIndex = dirName.IndexOf("CC");
                if (ccIndex >= 0 && ccIndex + 2 < dirName.Length)
                {
                    var versionPart = dirName.Substring(ccIndex + 2).Trim();
                    if (!string.IsNullOrEmpty(versionPart))
                        return $"CC {versionPart}";
                    return "CC";
                }
            }

            // Buscar patrón de año (20XX)
            var yearMatch = System.Text.RegularExpressions.Regex.Match(dirName, @"20\d{2}");
            if (yearMatch.Success)
            {
                return yearMatch.Value;
            }

            // Buscar patrón de versión (XX.X)
            var versionMatch = System.Text.RegularExpressions.Regex.Match(dirName, @"\d+\.\d+");
            if (versionMatch.Success)
            {
                return versionMatch.Value;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Obtiene los nombres de las subclaves de una clave de registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <returns>Array con los nombres de las subclaves o array vacío si hay error.</returns>
    private string[] GetSubKeyNames(string keyPath)
    {
        try
        {
            if (keyPath.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
            {
                string subKeyPath = keyPath.Substring("HKEY_LOCAL_MACHINE\\".Length);
                using var key = Registry.LocalMachine.OpenSubKey(subKeyPath);
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
            {
                string subKeyPath = keyPath.Substring("HKEY_CURRENT_USER\\".Length);
                using var key = Registry.CurrentUser.OpenSubKey(subKeyPath);
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }
            else if (keyPath.StartsWith("HKEY_CLASSES_ROOT\\", StringComparison.OrdinalIgnoreCase))
            {
                string subKeyPath = keyPath.Substring("HKEY_CLASSES_ROOT\\".Length);
                using var key = Registry.ClassesRoot.OpenSubKey(subKeyPath);
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }
            else if (keyPath.StartsWith("HKEY_USERS\\", StringComparison.OrdinalIgnoreCase))
            {
                string subKeyPath = keyPath.Substring("HKEY_USERS\\".Length);
                using var key = Registry.Users.OpenSubKey(subKeyPath);
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_CONFIG\\", StringComparison.OrdinalIgnoreCase))
            {
                string subKeyPath = keyPath.Substring("HKEY_CURRENT_CONFIG\\".Length);
                using var key = Registry.CurrentConfig.OpenSubKey(subKeyPath);
                return key?.GetSubKeyNames() ?? Array.Empty<string>();
            }

            _logger.LogWarning($"Formato de clave de registro no reconocido: {keyPath}");
            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener subclaves de {keyPath}: {ex.Message}");
            return Array.Empty<string>();
        }
    }
}