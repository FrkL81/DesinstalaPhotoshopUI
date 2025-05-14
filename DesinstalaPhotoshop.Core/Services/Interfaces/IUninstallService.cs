namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Define operaciones para desinstalar Adobe Photoshop.
/// </summary>
public interface IUninstallService
{
    /// <summary>
    /// Desinstala una instalación de Adobe Photoshop.
    /// </summary>
    /// <param name="installation">Instalación a desinstalar.</param>
    /// <param name="createBackup">Indica si se debe crear una copia de seguridad antes de desinstalar.</param>
    /// <param name="whatIf">Indica si se debe simular la desinstalación sin realizar cambios reales.</param>
    /// <param name="removeUserData">Indica si se deben eliminar los datos de usuario.</param>
    /// <param name="removeSharedComponents">Indica si se deben eliminar componentes compartidos.</param>
    /// <param name="progress">Objeto para reportar el progreso de la operación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult> UninstallAsync(
        PhotoshopInstallation installation,
        bool createBackup = true,
        bool whatIf = false,
        bool removeUserData = false,
        bool removeSharedComponents = false,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si una instalación puede ser desinstalada.
    /// </summary>
    /// <param name="installation">Instalación a verificar.</param>
    /// <returns>True si la instalación puede ser desinstalada, false en caso contrario.</returns>
    bool CanUninstall(PhotoshopInstallation installation);

    /// <summary>
    /// Obtiene información sobre el desinstalador de una instalación.
    /// </summary>
    /// <param name="installation">Instalación a verificar.</param>
    /// <returns>Información sobre el desinstalador, o null si no se encuentra.</returns>
    UninstallerInfo? GetUninstallerInfo(PhotoshopInstallation installation);
}

/// <summary>
/// Contiene información sobre un desinstalador.
/// </summary>
public class UninstallerInfo
{
    /// <summary>
    /// Obtiene o establece la ruta del desinstalador.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece los argumentos para el desinstalador.
    /// </summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo de desinstalador.
    /// </summary>
    public UninstallerType Type { get; set; } = UninstallerType.Unknown;

    /// <summary>
    /// Obtiene o establece el código de producto MSI (si aplica).
    /// </summary>
    public string? ProductCode { get; set; }
}

/// <summary>
/// Tipos de desinstaladores.
/// </summary>
public enum UninstallerType
{
    /// <summary>
    /// Tipo de desinstalador desconocido.
    /// </summary>
    Unknown,

    /// <summary>
    /// Desinstalador ejecutable (.exe).
    /// </summary>
    Executable,

    /// <summary>
    /// Desinstalador MSI.
    /// </summary>
    MSI,

    /// <summary>
    /// Desinstalador de Creative Cloud.
    /// </summary>
    CreativeCloud,

    /// <summary>
    /// Sin desinstalador (requiere eliminación manual).
    /// </summary>
    Manual
}
