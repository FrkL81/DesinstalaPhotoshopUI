namespace DesinstalaPhotoshop.Core.Services.Helpers;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>
    /// Crea un directorio si no existe.
    /// </summary>
    /// <param name="path">Ruta del directorio a crear.</param>
    /// <returns>True si el directorio se creó o ya existía, false en caso contrario.</returns>
    bool CreateDirectory(string path);

    /// <summary>
    /// Copia un archivo de forma asíncrona.
    /// </summary>
    /// <param name="sourcePath">Ruta del archivo origen.</param>
    /// <param name="destinationPath">Ruta del archivo destino.</param>
    /// <param name="overwrite">Indica si se debe sobrescribir el archivo destino si ya existe.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Task que representa la operación asíncrona.</returns>
    Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = true, CancellationToken cancellationToken = default);
}