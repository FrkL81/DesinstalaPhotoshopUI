namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;
using DesinstalaPhotoshop.Core.Services.Interfaces;

/// <summary>
/// Implementación del servicio para gestionar procesos relacionados con Adobe.
/// </summary>
[SupportedOSPlatform("windows")]
public class ProcessService : IProcessService
{
    private readonly ILoggingService _logger;

    // Lista de nombres de procesos de Adobe conocidos
    private readonly string[] _adobeProcessNames = new string[]
    {
        "Photoshop",
        "Adobe Photoshop",
        "AdobeIPCBroker",
        "Adobe CEF Helper",
        "Adobe Desktop Service",
        "Adobe Creative Cloud",
        "CCLibrary",
        "CCXProcess",
        "CoreSync",
        "AdobeUpdateService",
        "AdobeGCClient",
        "AdobeNotificationClient",
        "AdobeExtensionsService",
        "AdobeCollabSync",
        "AdobeAcrobat",
        "AdobeARM",
        "AGSService",
        "AGMService",
        "ACCFinderSync",
        "Adobe Crash Reporter",
        "Adobe Crash Processor",
        "Adobe QT32 Server",
        "Adobe CEF Helper",
        "Adobe Installer",
        "Adobe Genuine Service",
        "Adobe Genuine Monitor",
        "Adobe Genuine Software Service"
    };

    // Lista de nombres de servicios de Adobe conocidos
    private readonly string[] _adobeServiceNames = new string[]
    {
        "AdobeUpdateService",
        "AGSService",
        "AGMService",
        "AdobeARMService",
        "AdobeFlashPlayerUpdateSvc",
        "AdobeLM Service",
        "Adobe Genuine Software Integrity Service",
        "Adobe Acrobat Update Service",
        "Adobe Genuine Software Service"
    };

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ProcessService"/>.
    /// </summary>
    /// <param name="logger">Servicio de registro.</param>
    public ProcessService(ILoggingService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Detiene todos los procesos relacionados con Adobe Photoshop.
    /// </summary>
    /// <param name="whatIf">Indica si se debe simular la operación sin realizar cambios reales.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<OperationResult> StopAdobeProcessesAsync(
        bool whatIf = false,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInfo("Iniciando detención de procesos de Adobe...");
        progress?.Report(ProgressInfo.Running(0, "Deteniendo procesos", "Buscando procesos de Adobe en ejecución..."));

        try
        {
            // Obtener procesos de Adobe en ejecución
            var adobeProcesses = GetRunningAdobeProcesses();

            if (adobeProcesses.Count == 0)
            {
                _logger.LogInfo("No se encontraron procesos de Adobe en ejecución.");
                progress?.Report(ProgressInfo.Completed("Deteniendo procesos", "No se encontraron procesos de Adobe en ejecución."));
                return OperationResult.SuccessResult("No se encontraron procesos de Adobe en ejecución.");
            }

            _logger.LogInfo($"Se encontraron {adobeProcesses.Count} procesos de Adobe en ejecución.");
            progress?.Report(ProgressInfo.Running(20, "Deteniendo procesos", $"Se encontraron {adobeProcesses.Count} procesos de Adobe en ejecución."));

            // Si es modo simulación, no detener procesos
            if (whatIf)
            {
                _logger.LogInfo("Modo simulación activado. No se detendrán procesos.");
                progress?.Report(ProgressInfo.Completed("Deteniendo procesos", $"Simulación: Se detendrían {adobeProcesses.Count} procesos de Adobe."));
                return OperationResult.SuccessResult($"Simulación: Se detendrían {adobeProcesses.Count} procesos de Adobe.");
            }

            // Detener procesos
            int stoppedCount = 0;
            int failedCount = 0;
            List<string> failedProcesses = new List<string>();

            for (int i = 0; i < adobeProcesses.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Operación cancelada por el usuario.");
                    progress?.Report(ProgressInfo.Canceled("Deteniendo procesos", "Operación cancelada por el usuario."));
                    return OperationResult.Canceled("Operación cancelada por el usuario.");
                }

                var process = adobeProcesses[i];
                int progressPercentage = 20 + (i * 80 / adobeProcesses.Count);

                try
                {
                    _logger.LogInfo($"Intentando detener proceso: {process.ProcessName} (ID: {process.Id})");
                    progress?.Report(ProgressInfo.Running(progressPercentage, "Deteniendo procesos", $"Deteniendo {process.ProcessName} (ID: {process.Id})..."));

                    if (!process.HasExited)
                    {
                        process.Kill();
                        await Task.Delay(100, cancellationToken); // Pequeña pausa para permitir que el proceso termine
                        stoppedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al detener proceso {process.ProcessName} (ID: {process.Id}): {ex.Message}");
                    failedCount++;
                    failedProcesses.Add($"{process.ProcessName} (ID: {process.Id})");
                }
            }

            // Intentar detener servicios de Adobe
            await StopAdobeServicesAsync(whatIf, progress, cancellationToken);

            // Verificar resultados
            if (failedCount > 0)
            {
                string errorMessage = $"No se pudieron detener {failedCount} procesos: {string.Join(", ", failedProcesses)}";
                _logger.LogWarning(errorMessage);
                progress?.Report(ProgressInfo.Warning("Deteniendo procesos", errorMessage));
                return new OperationResult
                {
                    Success = stoppedCount > 0, // Consideramos éxito parcial si al menos se detuvo un proceso
                    Message = $"Se detuvieron {stoppedCount} procesos de Adobe. {errorMessage}"
                };
            }

            _logger.LogInfo($"Se detuvieron {stoppedCount} procesos de Adobe con éxito.");
            progress?.Report(ProgressInfo.Completed("Deteniendo procesos", $"Se detuvieron {stoppedCount} procesos de Adobe con éxito."));
            return OperationResult.SuccessResult($"Se detuvieron {stoppedCount} procesos de Adobe con éxito.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al detener procesos de Adobe: {ex.Message}");
            progress?.Report(ProgressInfo.Failed("Deteniendo procesos", $"Error al detener procesos de Adobe: {ex.Message}"));
            return OperationResult.Failed($"Error al detener procesos de Adobe: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene una lista de procesos de Adobe en ejecución.
    /// </summary>
    /// <returns>Lista de procesos de Adobe en ejecución.</returns>
    public List<Process> GetRunningAdobeProcesses()
    {
        _logger.LogInfo("Buscando procesos de Adobe en ejecución...");

        var processes = Process.GetProcesses();
        var adobeProcesses = new List<Process>();

        foreach (var process in processes)
        {
            try
            {
                // Verificar si el nombre del proceso contiene alguno de los nombres conocidos de Adobe
                if (_adobeProcessNames.Any(name => process.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||
                    process.ProcessName.Contains("Adobe", StringComparison.OrdinalIgnoreCase))
                {
                    adobeProcesses.Add(process);
                    _logger.LogInfo($"Proceso de Adobe encontrado: {process.ProcessName} (ID: {process.Id})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al acceder al proceso: {ex.Message}");
            }
        }

        _logger.LogInfo($"Se encontraron {adobeProcesses.Count} procesos de Adobe en ejecución.");
        return adobeProcesses;
    }

    /// <summary>
    /// Detiene servicios de Windows relacionados con Adobe.
    /// </summary>
    /// <param name="whatIf">Indica si se debe simular la operación sin realizar cambios reales.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    private async Task<OperationResult> StopAdobeServicesAsync(
        bool whatIf,
        IProgress<ProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Buscando servicios de Adobe...");
        progress?.Report(ProgressInfo.Running(0, "Deteniendo servicios", "Buscando servicios de Adobe..."));

        try
        {
            // Obtener todos los servicios
            var services = ServiceController.GetServices();

            // Filtrar servicios de Adobe
            var adobeServices = services.Where(s =>
                _adobeServiceNames.Any(name => s.ServiceName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||
                s.ServiceName.Contains("Adobe", StringComparison.OrdinalIgnoreCase) ||
                (s.DisplayName != null && s.DisplayName.Contains("Adobe", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (adobeServices.Count == 0)
            {
                _logger.LogInfo("No se encontraron servicios de Adobe.");
                progress?.Report(ProgressInfo.Completed("Deteniendo servicios", "No se encontraron servicios de Adobe."));
                return OperationResult.SuccessResult("No se encontraron servicios de Adobe.");
            }

            _logger.LogInfo($"Se encontraron {adobeServices.Count} servicios de Adobe.");
            progress?.Report(ProgressInfo.Running(20, "Deteniendo servicios", $"Se encontraron {adobeServices.Count} servicios de Adobe."));

            // Si es modo simulación, no detener servicios
            if (whatIf)
            {
                _logger.LogInfo("Modo simulación activado. No se detendrán servicios.");
                progress?.Report(ProgressInfo.Completed("Deteniendo servicios", $"Simulación: Se detendrían {adobeServices.Count} servicios de Adobe."));
                return OperationResult.SuccessResult($"Simulación: Se detendrían {adobeServices.Count} servicios de Adobe.");
            }

            // Detener servicios
            int stoppedCount = 0;
            int failedCount = 0;
            List<string> failedServices = new List<string>();

            for (int i = 0; i < adobeServices.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Operación cancelada por el usuario.");
                    progress?.Report(ProgressInfo.Canceled("Deteniendo servicios", "Operación cancelada por el usuario."));
                    return OperationResult.Canceled("Operación cancelada por el usuario.");
                }

                var service = adobeServices[i];
                int progressPercentage = 20 + (i * 80 / adobeServices.Count);

                try
                {
                    _logger.LogInfo($"Intentando detener servicio: {service.DisplayName} ({service.ServiceName})");
                    progress?.Report(ProgressInfo.Running(progressPercentage, "Deteniendo servicios", $"Deteniendo {service.DisplayName}..."));

                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        stoppedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al detener servicio {service.DisplayName}: {ex.Message}");
                    failedCount++;
                    failedServices.Add(service.DisplayName);
                }
            }

            // Verificar resultados
            if (failedCount > 0)
            {
                string errorMessage = $"No se pudieron detener {failedCount} servicios: {string.Join(", ", failedServices)}";
                _logger.LogWarning(errorMessage);
                progress?.Report(ProgressInfo.Warning("Deteniendo servicios", errorMessage));
                return new OperationResult
                {
                    Success = stoppedCount > 0, // Consideramos éxito parcial si al menos se detuvo un servicio
                    Message = $"Se detuvieron {stoppedCount} servicios de Adobe. {errorMessage}"
                };
            }

            _logger.LogInfo($"Se detuvieron {stoppedCount} servicios de Adobe con éxito.");
            progress?.Report(ProgressInfo.Completed("Deteniendo servicios", $"Se detuvieron {stoppedCount} servicios de Adobe con éxito."));
            return OperationResult.SuccessResult($"Se detuvieron {stoppedCount} servicios de Adobe con éxito.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al detener servicios de Adobe: {ex.Message}");
            progress?.Report(ProgressInfo.Failed("Deteniendo servicios", $"Error al detener servicios de Adobe: {ex.Message}"));
            return OperationResult.Failed($"Error al detener servicios de Adobe: {ex.Message}");
        }
    }
}
