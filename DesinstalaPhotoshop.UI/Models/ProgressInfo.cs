using System;

namespace DesinstalaPhotoshop.UI.Models
{
    /// <summary>
    /// Representa la información de progreso de una operación en la UI.
    /// NOTA: Esta es una implementación temporal para resolver errores de compilación.
    /// TODO: Esta clase debe ser reemplazada o actualizada según el plan de desarrollo.
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
        /// Obtiene el estado de la operación.
        /// </summary>
        public string OperationStatus { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase ProgressInfo.
        /// </summary>
        /// <param name="progressPercentage">Porcentaje de progreso (0-100).</param>
        /// <param name="operationTitle">Título de la operación.</param>
        /// <param name="statusMessage">Mensaje detallado sobre el progreso.</param>
        /// <param name="operationStatus">Estado de la operación.</param>
        public ProgressInfo(
            int progressPercentage,
            string operationTitle,
            string statusMessage,
            string operationStatus)
        {
            ProgressPercentage = Math.Clamp(progressPercentage, 0, 100);
            OperationTitle = operationTitle;
            StatusMessage = statusMessage;
            OperationStatus = operationStatus;
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
            return new ProgressInfo(progressPercentage, operationTitle, statusMessage, "Running");
        }

        /// <summary>
        /// Crea una instancia de ProgressInfo para una operación completada con éxito.
        /// </summary>
        /// <param name="operationTitle">Título de la operación.</param>
        /// <param name="statusMessage">Mensaje detallado sobre el resultado.</param>
        /// <returns>Una nueva instancia de ProgressInfo.</returns>
        public static ProgressInfo Completed(string operationTitle, string statusMessage)
        {
            return new ProgressInfo(100, operationTitle, statusMessage, "Completed");
        }

        /// <summary>
        /// Crea una instancia de ProgressInfo para una operación fallida.
        /// </summary>
        /// <param name="operationTitle">Título de la operación.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Una nueva instancia de ProgressInfo.</returns>
        public static ProgressInfo Failed(string operationTitle, string errorMessage)
        {
            return new ProgressInfo(100, operationTitle, "Error: " + errorMessage, "Failed");
        }

        /// <summary>
        /// Crea una instancia de ProgressInfo para una operación cancelada.
        /// </summary>
        /// <param name="operationTitle">Título de la operación.</param>
        /// <param name="message">Mensaje de cancelación.</param>
        /// <returns>Una nueva instancia de ProgressInfo.</returns>
        public static ProgressInfo Canceled(string operationTitle, string message)
        {
            return new ProgressInfo(100, operationTitle, "Cancelado: " + message, "Canceled");
        }

        /// <summary>
        /// Crea una instancia de ProgressInfo para una operación con advertencia.
        /// </summary>
        /// <param name="operationTitle">Título de la operación.</param>
        /// <param name="message">Mensaje de advertencia.</param>
        /// <returns>Una nueva instancia de ProgressInfo.</returns>
        public static ProgressInfo Warning(string operationTitle, string message)
        {
            return new ProgressInfo(100, operationTitle, "Advertencia: " + message, "Warning");
        }
    }
}
