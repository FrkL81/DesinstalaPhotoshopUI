namespace DesinstalaPhotoshop.Core.Services.Helpers;

using System.Collections.Generic;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Define operaciones para interactuar con el registro de Windows.
/// </summary>
public interface IRegistryHelper
{
    /// <summary>
    /// Busca instalaciones de Adobe Photoshop en el registro de Windows.
    /// </summary>
    /// <returns>Lista de instalaciones encontradas.</returns>
    List<PhotoshopInstallation> FindPhotoshopInstallations();

    /// <summary>
    /// Verifica si una clave de registro existe.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <returns>True si la clave existe, false en caso contrario.</returns>
    bool KeyExists(string keyPath);

    /// <summary>
    /// Obtiene un valor de una clave del registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <param name="valueName">Nombre del valor.</param>
    /// <returns>El valor como objeto, o null si no existe.</returns>
    object? GetRegistryValue(string keyPath, string valueName);

    /// <summary>
    /// Exporta una clave del registro a un archivo .reg.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave a exportar.</param>
    /// <param name="filePath">Ruta del archivo donde se guardará la exportación.</param>
    /// <returns>True si la exportación fue exitosa, false en caso contrario.</returns>
    bool ExportRegistryKey(string keyPath, string filePath);

    /// <summary>
    /// Importa un archivo .reg al registro.
    /// </summary>
    /// <param name="filePath">Ruta del archivo .reg a importar.</param>
    /// <returns>True si la importación fue exitosa, false en caso contrario.</returns>
    bool ImportRegistryFile(string filePath);

    /// <summary>
    /// Busca claves de registro relacionadas con Adobe Photoshop en una clave raíz.
    /// </summary>
    /// <param name="rootKey">Clave raíz donde buscar.</param>
    /// <returns>Lista de claves de registro encontradas.</returns>
    List<string> FindPhotoshopRegistryKeys(string rootKey);

    /// <summary>
    /// Elimina una clave del registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, false en caso contrario.</returns>
    bool DeleteRegistryKey(string keyPath);

    /// <summary>
    /// Elimina una clave del registro utilizando reg.exe.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, false en caso contrario.</returns>
    bool DeleteRegistryKeyWithRegExe(string keyPath);
}