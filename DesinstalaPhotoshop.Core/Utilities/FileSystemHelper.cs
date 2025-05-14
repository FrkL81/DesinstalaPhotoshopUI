using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace DesinstalaPhotoshop.Core.Utilities
{
    /// <summary>
    /// Proporciona métodos avanzados para manipular archivos y carpetas, especialmente aquellos que son difíciles de eliminar.
    /// </summary>
    public static class FileSystemHelper
    {
        /// <summary>
        /// Elimina un archivo utilizando múltiples estrategias y reintentos.
        /// </summary>
        /// <param name="filePath">Ruta del archivo a eliminar.</param>
        /// <param name="maxRetries">Número máximo de reintentos.</param>
        /// <param name="whatIf">Indica si se debe simular la eliminación.</param>
        /// <returns>true si el archivo se eliminó correctamente, false en caso contrario.</returns>
        public static bool DeleteFileWithRetry(string filePath, int maxRetries = 3, bool whatIf = false)
        {
            if (whatIf)
            {
                return true;
            }

            if (!File.Exists(filePath))
            {
                return true;
            }

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (IOException)
                {
                    // El archivo podría estar en uso, intentar liberar recursos
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(100 * (attempt + 1)); // Esperar un poco más en cada intento
                }
                catch (UnauthorizedAccessException)
                {
                    // Intentar cambiar atributos del archivo
                    try
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }
                    catch
                    {
                        // Ignorar errores al cambiar atributos
                    }
                }
                catch (Exception)
                {
                    // Cualquier otra excepción, continuar con el siguiente intento
                }
            }

            // Si llegamos aquí, todos los intentos fallaron
            return false;
        }

        /// <summary>
        /// Elimina una carpeta utilizando múltiples estrategias y reintentos.
        /// </summary>
        /// <param name="directoryPath">Ruta de la carpeta a eliminar.</param>
        /// <param name="recursive">Indica si se deben eliminar también los contenidos.</param>
        /// <param name="maxRetries">Número máximo de reintentos.</param>
        /// <param name="whatIf">Indica si se debe simular la eliminación.</param>
        /// <returns>true si la carpeta se eliminó correctamente, false en caso contrario.</returns>
        public static bool DeleteDirectoryWithRetry(string directoryPath, bool recursive = true, int maxRetries = 3, bool whatIf = false)
        {
            if (whatIf)
            {
                return true;
            }

            if (!Directory.Exists(directoryPath))
            {
                return true;
            }

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    Directory.Delete(directoryPath, recursive);
                    return true;
                }
                catch (IOException)
                {
                    // La carpeta podría contener archivos en uso, intentar liberar recursos
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(100 * (attempt + 1)); // Esperar un poco más en cada intento

                    // Si es recursivo, intentar eliminar archivos individualmente
                    if (recursive)
                    {
                        try
                        {
                            foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                            {
                                DeleteFileWithRetry(file, 1, whatIf);
                            }
                        }
                        catch
                        {
                            // Ignorar errores al eliminar archivos individuales
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Intentar cambiar atributos de la carpeta
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                        dirInfo.Attributes = FileAttributes.Normal;
                    }
                    catch
                    {
                        // Ignorar errores al cambiar atributos
                    }
                }
                catch (Exception)
                {
                    // Cualquier otra excepción, continuar con el siguiente intento
                }
            }

            // Si llegamos aquí, todos los intentos fallaron
            return false;
        }

        /// <summary>
        /// Verifica si un archivo existe y es accesible.
        /// </summary>
        /// <param name="filePath">Ruta del archivo a verificar.</param>
        /// <returns>true si el archivo existe y es accesible, false en caso contrario.</returns>
        public static bool FileExistsAndAccessible(string filePath)
        {
            try
            {
                return File.Exists(filePath) && new FileInfo(filePath).Length >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si una carpeta existe y es accesible.
        /// </summary>
        /// <param name="directoryPath">Ruta de la carpeta a verificar.</param>
        /// <returns>true si la carpeta existe y es accesible, false en caso contrario.</returns>
        public static bool DirectoryExistsAndAccessible(string directoryPath)
        {
            try
            {
                return Directory.Exists(directoryPath) && Directory.GetLastWriteTime(directoryPath) != DateTime.MinValue;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Busca archivos que coincidan con un patrón en una carpeta y sus subcarpetas.
        /// </summary>
        /// <param name="rootDirectory">Carpeta raíz donde comenzar la búsqueda.</param>
        /// <param name="searchPattern">Patrón de búsqueda (ej. "*.exe").</param>
        /// <param name="searchOption">Indica si se deben buscar en subcarpetas.</param>
        /// <returns>Array de rutas de archivos encontrados.</returns>
        public static string[] FindFiles(string rootDirectory, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            try
            {
                if (!Directory.Exists(rootDirectory))
                {
                    return Array.Empty<string>();
                }

                return Directory.GetFiles(rootDirectory, searchPattern, searchOption);
            }
            catch (Exception)
            {
                // En caso de error (ej. acceso denegado a alguna subcarpeta), intentar una búsqueda más segura
                try
                {
                    if (searchOption == SearchOption.AllDirectories)
                    {
                        var files = new System.Collections.Generic.List<string>();
                        
                        // Obtener archivos en el directorio raíz
                        try
                        {
                            files.AddRange(Directory.GetFiles(rootDirectory, searchPattern, SearchOption.TopDirectoryOnly));
                        }
                        catch
                        {
                            // Ignorar errores en el directorio raíz
                        }

                        // Intentar obtener subdirectorios
                        try
                        {
                            foreach (var dir in Directory.GetDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly))
                            {
                                try
                                {
                                    files.AddRange(FindFiles(dir, searchPattern, SearchOption.AllDirectories));
                                }
                                catch
                                {
                                    // Ignorar errores en subdirectorios
                                }
                            }
                        }
                        catch
                        {
                            // Ignorar errores al obtener subdirectorios
                        }

                        return files.ToArray();
                    }
                    else
                    {
                        return Directory.GetFiles(rootDirectory, searchPattern, SearchOption.TopDirectoryOnly);
                    }
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
        }

        /// <summary>
        /// Expande variables de entorno en una ruta.
        /// </summary>
        /// <param name="path">Ruta con variables de entorno (ej. %APPDATA%\Adobe).</param>
        /// <returns>Ruta expandida.</returns>
        public static string ExpandEnvironmentVariables(string path)
        {
            return Environment.ExpandEnvironmentVariables(path);
        }
    }
}
