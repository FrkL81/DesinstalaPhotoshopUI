namespace DesinstalaPhotoshop.Core.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Define la interfaz para el servicio de respaldo y restauración.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Crea una copia de seguridad para una operación de limpieza.
    /// </summary>
    /// <param name="installation">Instalación de Photoshop para la que se creará la copia de seguridad.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Identificador único de la copia de seguridad creada.</returns>
    Task<string> CreateBackupForCleanupAsync(
        PhotoshopInstallation installation,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una lista de las copias de seguridad disponibles.
    /// </summary>
    /// <returns>Lista de metadatos de las copias de seguridad disponibles.</returns>
    List<BackupMetadata> GetAvailableBackups();

    /// <summary>
    /// Restaura una copia de seguridad.
    /// </summary>
    /// <param name="backupId">Identificador de la copia de seguridad a restaurar.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>True si la restauración fue exitosa, false en caso contrario.</returns>
    Task<bool> RestoreBackupAsync(
        string backupId,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);
}
