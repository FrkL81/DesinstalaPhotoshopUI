namespace DesinstalaPhotoshop.Core.Services.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
}