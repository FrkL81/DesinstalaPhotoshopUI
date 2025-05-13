namespace DesinstalaPhotoshop.Core.Services;

using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Interfaz para el servicio de logging de la aplicación.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Registra un mensaje de depuración.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogDebug(string message);
    
    /// <summary>
    /// Registra un mensaje informativo.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogInfo(string message);
    
    /// <summary>
    /// Registra un mensaje de advertencia.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogWarning(string message);
    
    /// <summary>
    /// Registra un mensaje de error.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogError(string message);
    
    /// <summary>
    /// Registra un mensaje de error crítico.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    void LogCritical(string message);
    
    /// <summary>
    /// Registra un mensaje con el nivel especificado.
    /// </summary>
    /// <param name="level">Nivel de registro.</param>
    /// <param name="message">Mensaje a registrar.</param>
    void Log(LogLevel level, string message);
    
    /// <summary>
    /// Obtiene todos los mensajes de registro.
    /// </summary>
    /// <returns>Lista de mensajes de registro.</returns>
    IReadOnlyList<string> GetLogs();
    
    /// <summary>
    /// Obtiene los mensajes de registro filtrados por nivel.
    /// </summary>
    /// <param name="minLevel">Nivel mínimo de registro a incluir.</param>
    /// <returns>Lista de mensajes de registro filtrados.</returns>
    IReadOnlyList<string> GetLogs(LogLevel minLevel);
    
    /// <summary>
    /// Guarda los registros en un archivo.
    /// </summary>
    /// <param name="filePath">Ruta del archivo donde guardar los registros.</param>
    /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
    Task<bool> SaveLogsToFileAsync(string filePath);
    
    /// <summary>
    /// Limpia todos los registros almacenados.
    /// </summary>
    void ClearLogs();
    
    /// <summary>
    /// Evento que se dispara cuando se registra un nuevo mensaje.
    /// </summary>
    event EventHandler<LogEventArgs>? LogAdded;
}

/// <summary>
/// Argumentos para el evento de registro de mensajes.
/// </summary>
public class LogEventArgs : EventArgs
{
    /// <summary>
    /// Nivel del mensaje de registro.
    /// </summary>
    public LogLevel Level { get; }
    
    /// <summary>
    /// Mensaje de registro.
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Fecha y hora del registro.
    /// </summary>
    public DateTime Timestamp { get; }
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase LogEventArgs.
    /// </summary>
    /// <param name="level">Nivel del mensaje de registro.</param>
    /// <param name="message">Mensaje de registro.</param>
    /// <param name="timestamp">Fecha y hora del registro.</param>
    public LogEventArgs(LogLevel level, string message, DateTime timestamp)
    {
        Level = level;
        Message = message;
        Timestamp = timestamp;
    }
}