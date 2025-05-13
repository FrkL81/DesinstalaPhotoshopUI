namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Implementación del servicio de logging para la aplicación.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ConcurrentQueue<LogEntry> _logs = new();
    private readonly LogLevel _minLevelToStore;
    private readonly bool _writeToConsole;
    
    /// <summary>
    /// Evento que se dispara cuando se registra un nuevo mensaje.
    /// </summary>
    public event EventHandler<LogEventArgs>? LogAdded;
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase LoggingService.
    /// </summary>
    /// <param name="minLevelToStore">Nivel mínimo de registro a almacenar.</param>
    /// <param name="writeToConsole">Indica si los mensajes deben escribirse en la consola.</param>
    public LoggingService(LogLevel minLevelToStore = LogLevel.Debug, bool writeToConsole = true)
    {
        _minLevelToStore = minLevelToStore;
        _writeToConsole = writeToConsole;
    }
    
    /// <summary>
    /// Registra un mensaje de depuración.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    public void LogDebug(string message) => Log(LogLevel.Debug, message);
    
    /// <summary>
    /// Registra un mensaje informativo.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    public void LogInfo(string message) => Log(LogLevel.Info, message);
    
    /// <summary>
    /// Registra un mensaje de advertencia.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    public void LogWarning(string message) => Log(LogLevel.Warning, message);
    
    /// <summary>
    /// Registra un mensaje de error.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    public void LogError(string message) => Log(LogLevel.Error, message);
    
    /// <summary>
    /// Registra un mensaje de error crítico.
    /// </summary>
    /// <param name="message">Mensaje a registrar.</param>
    public void LogCritical(string message) => Log(LogLevel.Critical, message);
    
    /// <summary>
    /// Registra un mensaje con el nivel especificado.
    /// </summary>
    /// <param name="level">Nivel de registro.</param>
    /// <param name="message">Mensaje a registrar.</param>
    public void Log(LogLevel level, string message)
    {
        var timestamp = DateTime.Now;
        var formattedMessage = FormatLogMessage(level, message, timestamp);
        
        // Escribir en consola si está habilitado
        if (_writeToConsole)
        {
            WriteToConsole(level, formattedMessage);
        }
        
        // Almacenar el mensaje si cumple con el nivel mínimo
        if (level >= _minLevelToStore)
        {
            var entry = new LogEntry(level, message, timestamp, formattedMessage);
            _logs.Enqueue(entry);
        }
        
        // Disparar evento
        LogAdded?.Invoke(this, new LogEventArgs(level, message, timestamp));
    }
    
    /// <summary>
    /// Obtiene todos los mensajes de registro.
    /// </summary>
    /// <returns>Lista de mensajes de registro.</returns>
    public IReadOnlyList<string> GetLogs()
    {
        return _logs.Select(l => l.FormattedMessage).ToList().AsReadOnly();
    }
    
    /// <summary>
    /// Obtiene los mensajes de registro filtrados por nivel.
    /// </summary>
    /// <param name="minLevel">Nivel mínimo de registro a incluir.</param>
    /// <returns>Lista de mensajes de registro filtrados.</returns>
    public IReadOnlyList<string> GetLogs(LogLevel minLevel)
    {
        return _logs
            .Where(l => l.Level >= minLevel)
            .Select(l => l.FormattedMessage)
            .ToList()
            .AsReadOnly();
    }
    
    /// <summary>
    /// Guarda los registros en un archivo.
    /// </summary>
    /// <param name="filePath">Ruta del archivo donde guardar los registros.</param>
    /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
    public async Task<bool> SaveLogsToFileAsync(string filePath)
    {
        try
        {
            // Crear directorio si no existe
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Escribir logs en el archivo
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            foreach (var log in _logs.OrderBy(l => l.Timestamp))
            {
                await writer.WriteLineAsync(log.FormattedMessage);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            // Registrar el error pero no usar Log para evitar recursión
            Console.WriteLine($"Error al guardar logs en archivo: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Limpia todos los registros almacenados.
    /// </summary>
    public void ClearLogs()
    {
        _logs.Clear();
    }
    
    /// <summary>
    /// Formatea un mensaje de registro.
    /// </summary>
    /// <param name="level">Nivel del mensaje.</param>
    /// <param name="message">Mensaje a formatear.</param>
    /// <param name="timestamp">Fecha y hora del registro.</param>
    /// <returns>Mensaje formateado.</returns>
    private string FormatLogMessage(LogLevel level, string message, DateTime timestamp)
    {
        return $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
    }
    
    /// <summary>
    /// Escribe un mensaje en la consola con el color correspondiente al nivel.
    /// </summary>
    /// <param name="level">Nivel del mensaje.</param>
    /// <param name="message">Mensaje a escribir.</param>
    private void WriteToConsole(LogLevel level, string message)
    {
        var originalColor = Console.ForegroundColor;
        
        Console.ForegroundColor = level switch
        {
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => originalColor
        };
        
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
    
    /// <summary>
    /// Clase interna para almacenar entradas de registro.
    /// </summary>
    private class LogEntry
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
        /// Mensaje formateado.
        /// </summary>
        public string FormattedMessage { get; }
        
        /// <summary>
        /// Inicializa una nueva instancia de la clase LogEntry.
        /// </summary>
        /// <param name="level">Nivel del mensaje de registro.</param>
        /// <param name="message">Mensaje de registro.</param>
        /// <param name="timestamp">Fecha y hora del registro.</param>
        /// <param name="formattedMessage">Mensaje formateado.</param>
        public LogEntry(LogLevel level, string message, DateTime timestamp, string formattedMessage)
        {
            Level = level;
            Message = message;
            Timestamp = timestamp;
            FormattedMessage = formattedMessage;
        }
    }
}