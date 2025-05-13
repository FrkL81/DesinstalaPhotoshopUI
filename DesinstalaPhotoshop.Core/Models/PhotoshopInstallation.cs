namespace DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Representa una instalación de Adobe Photoshop detectada en el sistema.
/// </summary>
public class PhotoshopInstallation
{
    /// <summary>
    /// Obtiene o establece el nombre de visualización de la instalación.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Obtiene o establece la versión de Photoshop.
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Obtiene o establece la ubicación de instalación principal.
    /// </summary>
    public string InstallLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// Obtiene o establece la ruta del desinstalador si está disponible.
    /// </summary>
    public string? UninstallString { get; set; }
    
    /// <summary>
    /// Obtiene o establece la fecha de instalación si está disponible.
    /// </summary>
    public DateTime? InstallDate { get; set; }
    
    /// <summary>
    /// Obtiene o establece el tamaño estimado de la instalación en bytes.
    /// </summary>
    public long EstimatedSize { get; set; }
    
    /// <summary>
    /// Obtiene o establece la puntuación de confianza de la detección (0-100).
    /// Un valor más alto indica mayor confianza de que es una instalación válida.
    /// </summary>
    public int ConfidenceScore { get; set; }
    
    /// <summary>
    /// Obtiene o establece el tipo de instalación detectada.
    /// </summary>
    public InstallationType Type { get; set; } = InstallationType.Unknown;
    
    /// <summary>
    /// Obtiene o establece una lista de ubicaciones adicionales relacionadas con esta instalación.
    /// </summary>
    public List<string> AdditionalLocations { get; set; } = new();
    
    /// <summary>
    /// Obtiene o establece una lista de claves de registro relacionadas con esta instalación.
    /// </summary>
    public List<string> RegistryKeys { get; set; } = new();
    
    /// <summary>
    /// Obtiene o establece el método por el cual se detectó esta instalación.
    /// </summary>
    public DetectionMethod DetectedBy { get; set; } = DetectionMethod.Unknown;
    
    /// <summary>
    /// Obtiene o establece notas adicionales sobre esta instalación.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Tipos de instalación de Photoshop.
/// </summary>
public enum InstallationType
{
    /// <summary>
    /// Tipo de instalación desconocido.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Instalación completa y funcional.
    /// </summary>
    Complete,
    
    /// <summary>
    /// Instalación parcial o dañada.
    /// </summary>
    Partial,
    
    /// <summary>
    /// Residuos de una instalación previa.
    /// </summary>
    Residual
}

/// <summary>
/// Métodos de detección de instalaciones.
/// </summary>
public enum DetectionMethod
{
    /// <summary>
    /// Método de detección desconocido.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Detectado desde la lista de programas instalados.
    /// </summary>
    InstalledPrograms,
    
    /// <summary>
    /// Detectado desde el registro de Windows.
    /// </summary>
    Registry,
    
    /// <summary>
    /// Detectado desde el sistema de archivos.
    /// </summary>
    FileSystem,
    
    /// <summary>
    /// Detectado manualmente por el usuario.
    /// </summary>
    Manual
}