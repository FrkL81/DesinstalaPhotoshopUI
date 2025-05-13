namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Helpers;

/// <summary>
/// Implementación del servicio de detección de instalaciones de Adobe Photoshop.
/// </summary>
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
        
        // Implementación preliminar - será completada en futuras iteraciones
        // según el sistema de puntuación heurística documentado
        
        // Por defecto, marcar como desconocido
        installation.Type = InstallationType.Unknown;
        
        // Lógica básica de clasificación
        if (installation.ConfidenceScore >= 80)
        {
            installation.Type = InstallationType.Complete;
            _logger.LogInfo($"Instalación clasificada como completa: {installation.DisplayName}");
        }
        else if (installation.ConfidenceScore >= 40)
        {
            installation.Type = InstallationType.Partial;
            _logger.LogInfo($"Instalación clasificada como parcial: {installation.DisplayName}");
        }
        else
        {
            installation.Type = InstallationType.Residual;
            _logger.LogInfo($"Instalación clasificada como residual: {installation.DisplayName}");
        }
        
        return installation;
    }
    
    /// <summary>
    /// Enriquece la información de una instalación detectada con datos adicionales.
    /// </summary>
    /// <param name="installation">Instalación a enriquecer.</param>
    /// <returns>La instalación con información adicional.</returns>
    public async Task<PhotoshopInstallation> EnrichInstallationInfoAsync(PhotoshopInstallation installation)
    {
        _logger.LogInfo($"Enriqueciendo información de instalación: {installation.DisplayName}");
        
        // Implementación preliminar - será completada en futuras iteraciones
        await Task.Delay(100); // Simulación de trabajo asíncrono
        
        // Calcular puntuación de confianza basada en la información disponible
        int score = 0;
        
        // Verificar si tiene ubicación de instalación
        if (!string.IsNullOrEmpty(installation.InstallLocation))
        {
            score += 30;
            _logger.LogDebug($"Puntuación +30 por tener ubicación de instalación: {installation.InstallLocation}");
        }
        
        // Verificar si tiene string de desinstalación
        if (!string.IsNullOrEmpty(installation.UninstallString))
        {
            score += 30;
            _logger.LogDebug($"Puntuación +30 por tener string de desinstalación");
        }
        
        // Verificar si tiene versión
        if (!string.IsNullOrEmpty(installation.Version))
        {
            score += 20;
            _logger.LogDebug($"Puntuación +20 por tener información de versión: {installation.Version}");
        }
        
        // Verificar si tiene fecha de instalación
        if (installation.InstallDate.HasValue)
        {
            score += 10;
            _logger.LogDebug($"Puntuación +10 por tener fecha de instalación: {installation.InstallDate}");
        }
        
        // Verificar si tiene ubicaciones adicionales
        if (installation.AdditionalLocations.Count > 0)
        {
            score += 5;
            _logger.LogDebug($"Puntuación +5 por tener {installation.AdditionalLocations.Count} ubicaciones adicionales");
        }
        
        // Verificar si tiene claves de registro
        if (installation.RegistryKeys.Count > 0)
        {
            score += 5;
            _logger.LogDebug($"Puntuación +5 por tener {installation.RegistryKeys.Count} claves de registro");
        }
        
        // Limitar la puntuación a 100
        installation.ConfidenceScore = Math.Min(score, 100);
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
        
        try
        {
            // Utilizar el RegistryHelper para buscar instalaciones en el registro
            var installations = _registryHelper.FindPhotoshopInstallations();
            
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
                            AdditionalLocations = new List<string> { dir }
                        };
                        
                        installations.Add(installation);
                        _logger.LogInfo($"Instalación detectada en sistema de archivos: {displayName}");
                    }
                }
            }
            
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
}