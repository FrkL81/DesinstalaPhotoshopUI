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
}