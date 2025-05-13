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
    /// Obtiene el mensaje de error si la operación ha fallado.
    /// </summary>
    public string? ErrorMessage { get; }
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase ProgressInfo.
    /// </summary>
    /// <param name="progressPercentage">Porcentaje de progreso (0-100).</param>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="statusMessage">Mensaje detallado sobre el progreso.</param>
    /// <param name="isRunning">Indica si la operación está en ejecución.</param>
    /// <param name="isCompleted">Indica si la operación ha finalizado con éxito.</param>
    /// <param name="hasError">Indica si la operación ha fallado.</param>
    /// <param name="errorMessage">Mensaje de error si la operación ha fallado.</param>
    private ProgressInfo(
        int progressPercentage,
        string operationTitle,
        string statusMessage,
        bool isRunning,
        bool isCompleted,
        bool hasError,
        string? errorMessage)
    {
        ProgressPercentage = Math.Clamp(progressPercentage, 0, 100);
        OperationTitle = operationTitle;
        StatusMessage = statusMessage;
        IsRunning = isRunning;
        IsCompleted = isCompleted;
        HasError = hasError;
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
        return new ProgressInfo(progressPercentage, operationTitle, statusMessage, true, false, false, null);
    }
    
    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación completada con éxito.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="statusMessage">Mensaje detallado sobre el resultado.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Completed(string operationTitle, string statusMessage)
    {
        return new ProgressInfo(100, operationTitle, statusMessage, false, true, false, null);
    }
    
    /// <summary>
    /// Crea una instancia de ProgressInfo para una operación fallida.
    /// </summary>
    /// <param name="operationTitle">Título de la operación.</param>
    /// <param name="errorMessage">Mensaje de error.</param>
    /// <returns>Una nueva instancia de ProgressInfo.</returns>
    public static ProgressInfo Error(string operationTitle, string errorMessage)
    {
        return new ProgressInfo(100, operationTitle, "Error", false, false, true, errorMessage);
    }
}