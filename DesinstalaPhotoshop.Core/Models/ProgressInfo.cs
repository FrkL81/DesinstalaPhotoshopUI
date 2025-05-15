namespace DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Representa la información de progreso de una operación.
/// </summary>
public class ProgressInfo
{
    /// <summary>
    /// Obtiene el porcentaje de progreso (0-100).
    /// </summary>
    public int ProgressPercentage { get; }

    /// <summary>
    /// Obtiene el título de la operación en curso.
    /// </summary>
    public string OperationTitle { get; }

    /// <summary>
    /// Obtiene el mensaje detallado sobre el progreso actual.
    /// </summary>
    public string StatusMessage { get; }

    /// <summary>
    /// Obtiene un valor que indica si la operación está en ejecución.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// Obtiene un valor que indica si la operación ha finalizado con éxito.
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// Obtiene un valor que indica si la operación ha fallado.
    /// </summary>
    public bool HasError { get; }

    /// <summary>
    /// Obtiene un valor que indica si la operación ha sido cancelada.
    /// </summary>
    public bool IsCanceled { get; }

    /// <summary>
    /// Obtiene un valor que indica si la operación tiene una advertencia.
    /// </summary>
    public bool HasWarning { get; }

    /// <summary>
    /// Obtiene el mensaje de error si la operación ha fallado.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Crea una nueva instancia de ProgressInfo para una operación en curso.
    /// </summary>


    /// <summary>
    /// Crea una nueva instancia de ProgressInfo para una operación completada.
    /// </summary>
    public static ProgressInfo Completed(int progress, string title, string message)
    {
        return new ProgressInfo(progress, title, message, false, true, false, false, false, null);
    }

    /// <summary>
    /// Crea una nueva instancia de ProgressInfo para una operación con error.
    /// </summary>
    public static ProgressInfo Error(int progress, string title, string message, string errorMessage)
    {
        return new ProgressInfo(progress, title, message, false, false, true, false, false, errorMessage);
    }

    /// <summary>
    /// Crea una nueva instancia de ProgressInfo para una operación cancelada.
    /// </summary>
    public static ProgressInfo Canceled(int progress, string title, string message)
    {
        return new ProgressInfo(progress, title, message, false, false, false, true, false, null);
    }

    /// <summary>
    /// Crea una nueva instancia de ProgressInfo para una operación con advertencia.
    /// </summary>
    public static ProgressInfo Warning(int progress, string title, string message)
    {
        return new ProgressInfo(progress, title, message, false, false, false, false, true, null);
    }


    /// <summary>
    /// Obtiene el estado de la operación.
    /// NOTA: Esta es una propiedad temporal para resolver errores de compilación.
    /// TODO: Esta propiedad debe ser reemplazada o actualizada según el plan de desarrollo.
    /// </summary>
    public string OperationStatus => IsRunning ? "Running" :
                                    IsCompleted ? "Completed" :
                                    HasError ? "Failed" :
                                    IsCanceled ? "Canceled" :
                                    HasWarning ? "Warning" : "Unknown";

    /// <summary>
    /// Inicializa una nueva instancia de la clase ProgressInfo.
    /// </summary>
    /// <param name="progressPercentage">Porcentaje de progreso (0-100).</param>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="statusMessage">Mensaje detallado sobre el progreso.</param>
    /// <param name="isRunning">Indica si la operación está en ejecución.</param>
    /// <param name="isCompleted">Indica si la operación ha finalizado con éxito.</param>
    /// <param name="hasError">Indica si la operación ha fallado.</param>
    /// <param name="isCanceled">Indica si la operación ha sido cancelada.</param>
    /// <param name="hasWarning">Indica si la operación tiene una advertencia.</param>
    /// <param name="errorMessage">Mensaje de error si la operación ha fallado.</param>
    private ProgressInfo(
        int progressPercentage,
        string operationTitle,
        string statusMessage,
        bool isRunning,
        bool isCompleted,
        bool hasError,
        bool isCanceled,
        bool hasWarning,
        string? errorMessage)
    {
        ProgressPercentage = Math.Clamp(progressPercentage, 0, 100);
        OperationTitle = operationTitle;
        StatusMessage = statusMessage;
        IsRunning = isRunning;
        IsCompleted = isCompleted;
        HasError = hasError;
        IsCanceled = isCanceled;
        HasWarning = hasWarning;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación en ejecución.
    /// </summary>
    /// <param name="progressPercentage">Porcentaje de progreso (0-100).</param>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="statusMessage">Mensaje detallado sobre el progreso.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Running(int progressPercentage, string operationTitle, string statusMessage)
    {
        return new ProgressInfo(progressPercentage, operationTitle, statusMessage, true, false, false, false, false, null);
    }

    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación completada con éxito.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="statusMessage">Mensaje detallado sobre el resultado.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Completed(string operationTitle, string statusMessage)
    {
        return new ProgressInfo(100, operationTitle, statusMessage, false, true, false, false, false, null);
    }

    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación fallida.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="errorMessage">Mensaje de error.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Error(string operationTitle, string errorMessage)
    {
        return new ProgressInfo(100, operationTitle, "Error", false, false, true, false, false, errorMessage);
    }

    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación fallida.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="errorMessage">Mensaje de error.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Failed(string operationTitle, string errorMessage)
    {
        return new ProgressInfo(100, operationTitle, "Error: " + errorMessage, false, false, true, false, false, errorMessage);
    }

    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación cancelada.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="message">Mensaje de cancelación.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Canceled(string operationTitle, string message)
    {
        return new ProgressInfo(100, operationTitle, "Cancelado: " + message, false, false, false, true, false, message);
    }

    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación con advertencia.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="message">Mensaje de advertencia.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Warning(string operationTitle, string message)
    {
        return new ProgressInfo(100, operationTitle, "Advertencia: " + message, false, false, false, false, true, message);
    }
}