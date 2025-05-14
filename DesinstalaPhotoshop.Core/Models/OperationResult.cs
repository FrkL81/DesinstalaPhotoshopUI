namespace DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Representa el resultado de una operación.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Obtiene o establece un valor que indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si la operación fue cancelada.
    /// </summary>
    public bool IsCanceled { get; set; }

    /// <summary>
    /// Obtiene o establece el mensaje de error si la operación falló.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Obtiene o establece el mensaje de la operación.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el identificador de la copia de seguridad creada durante la operación.
    /// </summary>
    public string? BackupId { get; set; }

    /// <summary>
    /// Crea un resultado de operación exitosa.
    /// </summary>
    /// <param name="backupId">Identificador de la copia de seguridad creada durante la operación.</param>
    /// <returns>Resultado de operación exitosa.</returns>
    public static OperationResult Successful(string? backupId = null)
    {
        return new OperationResult
        {
            Success = true,
            IsCanceled = false,
            ErrorMessage = null,
            Message = "Operación completada con éxito.",
            BackupId = backupId
        };
    }

    /// <summary>
    /// Crea un resultado de operación exitosa con un mensaje personalizado.
    /// </summary>
    /// <param name="message">Mensaje de éxito.</param>
    /// <param name="backupId">Identificador de la copia de seguridad creada durante la operación.</param>
    /// <returns>Resultado de operación exitosa.</returns>
    public static OperationResult SuccessResult(string message, string? backupId = null)
    {
        return new OperationResult
        {
            Success = true,
            IsCanceled = false,
            ErrorMessage = null,
            Message = message,
            BackupId = backupId
        };
    }

    /// <summary>
    /// Crea un resultado de operación fallida.
    /// </summary>
    /// <param name="errorMessage">Mensaje de error.</param>
    /// <returns>Resultado de operación fallida.</returns>
    public static OperationResult Failed(string errorMessage)
    {
        return new OperationResult
        {
            Success = false,
            IsCanceled = false,
            ErrorMessage = errorMessage,
            Message = errorMessage,
            BackupId = null
        };
    }

    /// <summary>
    /// Crea un resultado de operación cancelada.
    /// </summary>
    /// <param name="message">Mensaje de cancelación.</param>
    /// <returns>Resultado de operación cancelada.</returns>
    public static OperationResult Canceled(string message)
    {
        return new OperationResult
        {
            Success = false,
            IsCanceled = true,
            ErrorMessage = message,
            Message = message,
            BackupId = null
        };
    }
}
