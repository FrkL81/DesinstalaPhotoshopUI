namespace DesinstalaPhotoshop.Core.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Define la interfaz para el servicio de limpieza de residuos de Photoshop.
/// </summary>
public interface ICleanupService
{
    /// <summary>
    /// Limpia residuos de Adobe Photoshop del sistema.
    /// </summary>
    /// <param name="installation">Instalación de Photoshop para la que se realizará la limpieza.</param>
    /// <param name="createBackup">Indica si se debe crear una copia de seguridad antes de limpiar.</param>
    /// <param name="whatIf">Indica si se debe simular la limpieza sin realizar cambios reales.</param>
    /// <param name="cleanupTempFiles">Indica si se deben limpiar archivos temporales.</param>
    /// <param name="cleanupRegistry">Indica si se deben limpiar entradas del registro.</param>
    /// <param name="cleanupConfigFiles">Indica si se deben limpiar archivos de configuración.</param>
    /// <param name="cleanupCacheFiles">Indica si se deben limpiar archivos de caché.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult> CleanupAsync(
        PhotoshopInstallation installation,
        bool createBackup = true,
        bool whatIf = false,
        bool cleanupTempFiles = true,
        bool cleanupRegistry = true,
        bool cleanupConfigFiles = true,
        bool cleanupCacheFiles = true,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);
}
