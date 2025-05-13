namespace DesinstalaPhotoshop.Core.Services;

using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Interfaz para el servicio de detección de instalaciones de Adobe Photoshop.
/// </summary>
public interface IDetectionService
{
    /// <summary>
    /// Detecta instalaciones de Adobe Photoshop en el sistema.
    /// </summary>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Lista de instalaciones detectadas.</returns>
    Task<List<PhotoshopInstallation>> DetectInstallationsAsync(
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clasifica una instalación de Photoshop según su estado y completitud.
    /// </summary>
    /// <param name="installation">Instalación a clasificar.</param>
    /// <returns>La instalación con su tipo actualizado.</returns>
    PhotoshopInstallation ClassifyInstallation(PhotoshopInstallation installation);
    
    /// <summary>
    /// Enriquece la información de una instalación detectada con datos adicionales.
    /// </summary>
    /// <param name="installation">Instalación a enriquecer.</param>
    /// <returns>La instalación con información adicional.</returns>
    Task<PhotoshopInstallation> EnrichInstallationInfoAsync(PhotoshopInstallation installation);
}