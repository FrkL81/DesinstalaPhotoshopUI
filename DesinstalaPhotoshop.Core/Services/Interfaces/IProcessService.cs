namespace DesinstalaPhotoshop.Core.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Define operaciones para gestionar procesos relacionados con Adobe.
/// </summary>
public interface IProcessService
{
    /// <summary>
    /// Detiene todos los procesos relacionados con Adobe Photoshop.
    /// </summary>
    /// <param name="whatIf">Indica si se debe simular la operación sin realizar cambios reales.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult> StopAdobeProcessesAsync(
        bool whatIf = false,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista de procesos de Adobe en ejecución.
    /// </summary>
    /// <returns>Lista de procesos de Adobe en ejecución.</returns>
    List<Process> GetRunningAdobeProcesses();
}
