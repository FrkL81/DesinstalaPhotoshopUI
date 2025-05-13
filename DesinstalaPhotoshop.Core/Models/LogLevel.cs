namespace DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Niveles de registro para el sistema de logging.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Información detallada, útil para depuración.
    /// </summary>
    Debug = 0,
    
    /// <summary>
    /// Información general sobre el funcionamiento de la aplicación.
    /// </summary>
    Info = 1,
    
    /// <summary>
    /// Advertencias que no impiden el funcionamiento pero requieren atención.
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// Errores que afectan a una operación específica pero no detienen la aplicación.
    /// </summary>
    Error = 3,
    
    /// <summary>
    /// Errores críticos que pueden detener la aplicación.
    /// </summary>
    Critical = 4
}