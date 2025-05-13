namespace DesinstalaPhotoshop.Core.Services.Helpers;

using System.IO;

/// <summary>
/// Define operaciones para interactuar con el sistema de archivos.
/// </summary>
public interface IFileSystemHelper
{
    /// <summary>
    /// Verifica si un directorio existe.
    /// </summary>
    /// <param name="path">Ruta del directorio.</param>
    /// <returns>True si el directorio existe, false en caso contrario.</returns>
    bool DirectoryExists(string path);
    
    /// <summary>
    /// Verifica si un archivo existe.
    /// </summary>
    /// <param name="path">Ruta del archivo.</param>
    /// <returns>True si el archivo existe, false en caso contrario.</returns>
    bool FileExists(string path);
    
    /// <summary>
    /// Busca archivos que coincidan con un patrón en un directorio y sus subdirectorios.
    /// </summary>
    /// <param name="rootPath">Directorio raíz para la búsqueda.</param>
    /// <param name="searchPattern">Patrón de búsqueda (ej. "*.exe").</param>
    /// <param name="searchOption">Opciones de búsqueda.</param>
    /// <returns>Lista de rutas de archivos encontrados.</returns>
    List<string> FindFiles(string rootPath, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories);
    
    /// <summary>
    /// Obtiene el tamaño de un directorio en bytes.
    /// </summary>
    /// <param name="path">Ruta del directorio.</param>
    /// <returns>Tamaño en bytes, o 0 si hay un error.</returns>
    long GetDirectorySize(string path);
    
    /// <summary>
    /// Busca directorios que contengan un nombre específico.
    /// </summary>
    /// <param name="rootPath">Directorio raíz para la búsqueda.</param>
    /// <param name="directoryNameContains">Texto que debe contener el nombre del directorio.</param>
    /// <returns>Lista de rutas de directorios encontrados.</returns>
    List<string> FindDirectories(string rootPath, string directoryNameContains);
}