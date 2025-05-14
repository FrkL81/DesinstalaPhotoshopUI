namespace DesinstalaPhotoshop.Core.Services.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Proporciona métodos auxiliares para operaciones con el sistema de archivos.
/// </summary>
public class FileSystemHelper : IFileSystemHelper
{
    private readonly ILoggingService _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase FileSystemHelper.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    public FileSystemHelper(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Verifica si un directorio existe.
    /// </summary>
    /// <param name="path">Ruta del directorio.</param>
    /// <returns>True si el directorio existe, false en caso contrario.</returns>
    public bool DirectoryExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            return Directory.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al verificar si existe el directorio {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica si un archivo existe.
    /// </summary>
    /// <param name="path">Ruta del archivo.</param>
    /// <returns>True si el archivo existe, false en caso contrario.</returns>
    public bool FileExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            return File.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al verificar si existe el archivo {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Busca archivos que coincidan con un patrón en un directorio y sus subdirectorios.
    /// </summary>
    /// <param name="rootPath">Directorio raíz para la búsqueda.</param>
    /// <param name="searchPattern">Patrón de búsqueda (ej. "*.exe").</param>
    /// <param name="searchOption">Opciones de búsqueda.</param>
    /// <returns>Lista de rutas de archivos encontrados.</returns>
    public List<string> FindFiles(string rootPath, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
    {
        var result = new List<string>();

        if (!DirectoryExists(rootPath))
            return result;

        try
        {
            result.AddRange(Directory.GetFiles(rootPath, searchPattern, searchOption));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Acceso denegado al buscar archivos en {rootPath}: {ex.Message}");

            // Intenta buscar en los subdirectorios a los que sí tenemos acceso
            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    foreach (var dir in Directory.GetDirectories(rootPath))
                    {
                        try
                        {
                            result.AddRange(FindFiles(dir, searchPattern, searchOption));
                        }
                        catch
                        {
                            // Ignora directorios a los que no podemos acceder
                        }
                    }
                }
                catch
                {
                    // Ignora si no podemos listar los subdirectorios
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar archivos en {rootPath}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Obtiene el tamaño de un directorio en bytes.
    /// </summary>
    /// <param name="path">Ruta del directorio.</param>
    /// <returns>Tamaño en bytes, o 0 si hay un error.</returns>
    public long GetDirectorySize(string path)
    {
        if (!DirectoryExists(path))
            return 0;

        try
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al calcular el tamaño del directorio {path}: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Busca directorios que contengan un nombre específico.
    /// </summary>
    /// <param name="rootPath">Directorio raíz para la búsqueda.</param>
    /// <param name="directoryNameContains">Texto que debe contener el nombre del directorio.</param>
    /// <returns>Lista de rutas de directorios encontrados.</returns>
    public List<string> FindDirectories(string rootPath, string directoryNameContains)
    {
        var result = new List<string>();

        if (!DirectoryExists(rootPath))
            return result;

        try
        {
            // Busca directorios que contengan el texto especificado
            var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories)
                .Where(d => Path.GetFileName(d).Contains(directoryNameContains, StringComparison.OrdinalIgnoreCase))
                .ToList();

            result.AddRange(directories);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Acceso denegado al buscar directorios en {rootPath}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar directorios en {rootPath}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Crea un directorio si no existe.
    /// </summary>
    /// <param name="path">Ruta del directorio a crear.</param>
    /// <returns>True si el directorio se creó o ya existía, false en caso contrario.</returns>
    public bool CreateDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            if (Directory.Exists(path))
                return true;

            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al crear el directorio {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Copia un archivo de forma asíncrona.
    /// </summary>
    /// <param name="sourcePath">Ruta del archivo origen.</param>
    /// <param name="destinationPath">Ruta del archivo destino.</param>
    /// <param name="overwrite">Indica si se debe sobrescribir el archivo destino si ya existe.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Task que representa la operación asíncrona.</returns>
    public async Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Las rutas de origen y destino no pueden estar vacías.");

        if (!FileExists(sourcePath))
            throw new FileNotFoundException($"El archivo de origen no existe: {sourcePath}");

        try
        {
            // Crear el directorio destino si no existe
            string? destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
                CreateDirectory(destinationDir);

            // Copiar el archivo de forma asíncrona
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var destinationStream = new FileStream(destinationPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.CopyToAsync(destinationStream, 81920, cancellationToken);
            }

            _logger.LogInfo($"Archivo copiado con éxito de {sourcePath} a {destinationPath}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"Copia de archivo cancelada: {sourcePath} -> {destinationPath}");
            throw; // Re-lanzar para que el llamador sepa que fue cancelada
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al copiar el archivo {sourcePath} a {destinationPath}: {ex.Message}");
            throw; // Re-lanzar para que el llamador pueda manejar el error
        }
    }

    /// <summary>
    /// Elimina un archivo.
    /// </summary>
    /// <param name="path">Ruta del archivo a eliminar.</param>
    /// <returns>True si el archivo se eliminó correctamente, false en caso contrario.</returns>
    public bool DeleteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!FileExists(path))
            return true; // Si el archivo no existe, consideramos que la eliminación fue exitosa

        try
        {
            File.Delete(path);
            _logger.LogInfo($"Archivo eliminado: {path}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar el archivo {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Elimina un directorio y, opcionalmente, su contenido.
    /// </summary>
    /// <param name="path">Ruta del directorio a eliminar.</param>
    /// <param name="recursive">Indica si se debe eliminar también el contenido del directorio.</param>
    /// <returns>True si el directorio se eliminó correctamente, false en caso contrario.</returns>
    public bool DeleteDirectory(string path, bool recursive = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!DirectoryExists(path))
            return true; // Si el directorio no existe, consideramos que la eliminación fue exitosa

        try
        {
            Directory.Delete(path, recursive);
            _logger.LogInfo($"Directorio eliminado: {path}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar el directorio {path}: {ex.Message}");
            return false;
        }
    }
}