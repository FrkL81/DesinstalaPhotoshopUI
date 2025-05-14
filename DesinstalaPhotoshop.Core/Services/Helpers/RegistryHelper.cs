namespace DesinstalaPhotoshop.Core.Services.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Win32;
using DesinstalaPhotoshop.Core.Models;

/// <summary>
/// Proporciona métodos auxiliares para operaciones con el registro de Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public class RegistryHelper : IRegistryHelper
{
    private readonly ILoggingService _logger;

    // Rutas comunes del registro donde se pueden encontrar instalaciones de software
    private static readonly string[] UninstallKeys = new[]
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    /// <summary>
    /// Inicializa una nueva instancia de la clase RegistryHelper.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    public RegistryHelper(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Busca instalaciones de Adobe Photoshop en el registro de Windows.
    /// </summary>
    /// <returns>Lista de instalaciones encontradas.</returns>
    public List<PhotoshopInstallation> FindPhotoshopInstallations()
    {
        var installations = new List<PhotoshopInstallation>();

        try
        {
            // Buscar en las claves de desinstalación de 64 y 32 bits
            foreach (var uninstallKey in UninstallKeys)
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                using var key = baseKey.OpenSubKey(uninstallKey);

                if (key == null)
                    continue;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null)
                        continue;

                    var displayName = subKey.GetValue("DisplayName") as string;

                    // Verificar si es una instalación de Photoshop
                    if (!string.IsNullOrEmpty(displayName) &&
                        (displayName.Contains("Adobe Photoshop", StringComparison.OrdinalIgnoreCase) ||
                         displayName.Contains("Photoshop", StringComparison.OrdinalIgnoreCase)))
                    {
                        var installation = new PhotoshopInstallation
                        {
                            DisplayName = displayName,
                            Version = subKey.GetValue("DisplayVersion") as string ?? string.Empty,
                            InstallLocation = subKey.GetValue("InstallLocation") as string ?? string.Empty,
                            UninstallString = subKey.GetValue("UninstallString") as string,
                            EstimatedSize = Convert.ToInt64(subKey.GetValue("EstimatedSize") ?? 0) * 1024, // KB a bytes
                            DetectedBy = DetectionMethod.Registry,
                            ConfidenceScore = 80, // Alta confianza para detecciones de registro
                            AssociatedRegistryKeys = new List<string> { $"HKLM\\{uninstallKey}\\{subKeyName}" }
                        };

                        // Intentar obtener la fecha de instalación
                        if (subKey.GetValue("InstallDate") is string installDateStr &&
                            !string.IsNullOrEmpty(installDateStr))
                        {
                            if (DateTime.TryParseExact(installDateStr, "yyyyMMdd", null,
                                System.Globalization.DateTimeStyles.None, out var installDate))
                            {
                                installation.InstallDate = installDate;
                            }
                        }

                        installations.Add(installation);
                        _logger.LogInfo($"Instalación de Photoshop encontrada en registro: {displayName}");
                    }
                }
            }

            // Buscar claves específicas de Adobe
            FindAdobeSpecificKeys(installations);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar instalaciones en el registro: {ex.Message}");
        }

        return installations;
    }

    /// <summary>
    /// Busca claves específicas de Adobe en el registro que puedan contener información adicional.
    /// </summary>
    /// <param name="installations">Lista de instalaciones a enriquecer.</param>
    private void FindAdobeSpecificKeys(List<PhotoshopInstallation> installations)
    {
        try
        {
            // Buscar en la clave de Adobe
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            using var adobeKey = baseKey.OpenSubKey(@"SOFTWARE\Adobe");

            if (adobeKey == null)
                return;

            foreach (var productKeyName in adobeKey.GetSubKeyNames())
            {
                // Verificar si es una clave relacionada con Photoshop
                if (productKeyName.Contains("Photoshop", StringComparison.OrdinalIgnoreCase))
                {
                    using var productKey = adobeKey.OpenSubKey(productKeyName);
                    if (productKey == null)
                        continue;

                    // Buscar instalaciones existentes para enriquecer o crear nuevas si no existen
                    var version = productKey.GetValue("Version") as string ?? string.Empty;
                    var installDir = productKey.GetValue("InstallPath") as string ?? string.Empty;

                    // Registrar la clave encontrada
                    var registryKeyPath = $"HKLM\\SOFTWARE\\Adobe\\{productKeyName}";
                    _logger.LogInfo($"Clave de registro específica de Adobe encontrada: {registryKeyPath}");

                    // Buscar si ya existe una instalación con esta versión
                    var existingInstallation = installations.FirstOrDefault(i =>
                        i.Version == version ||
                        (!string.IsNullOrEmpty(installDir) && i.InstallLocation == installDir));

                    if (existingInstallation != null)
                    {
                        // Enriquecer la instalación existente
                        if (!string.IsNullOrEmpty(installDir) && string.IsNullOrEmpty(existingInstallation.InstallLocation))
                            existingInstallation.InstallLocation = installDir;

                        if (!string.IsNullOrEmpty(version) && string.IsNullOrEmpty(existingInstallation.Version))
                            existingInstallation.Version = version;

                        existingInstallation.AssociatedRegistryKeys.Add(registryKeyPath);
                        existingInstallation.ConfidenceScore += 10; // Aumentar confianza por encontrar más evidencia
                    }
                    else if (!string.IsNullOrEmpty(installDir))
                    {
                        // Crear una nueva instalación
                        var newInstallation = new PhotoshopInstallation
                        {
                            DisplayName = $"Adobe Photoshop {version}",
                            Version = version,
                            InstallLocation = installDir,
                            DetectedBy = DetectionMethod.Registry,
                            ConfidenceScore = 60, // Confianza media para detecciones de claves específicas
                            AssociatedRegistryKeys = new List<string> { registryKeyPath }
                        };

                        installations.Add(newInstallation);
                        _logger.LogInfo($"Nueva instalación de Photoshop detectada en clave específica: {newInstallation.DisplayName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar claves específicas de Adobe: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si una clave de registro existe.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <returns>True si la clave existe, false en caso contrario.</returns>
    public bool KeyExists(string keyPath)
    {
        try
        {
            using var key = GetRegistryKey(keyPath);
            return key != null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al verificar si existe la clave {keyPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene un valor de una clave del registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <param name="valueName">Nombre del valor.</param>
    /// <returns>El valor como objeto, o null si no existe.</returns>
    public object? GetRegistryValue(string keyPath, string valueName)
    {
        try
        {
            using var key = GetRegistryKey(keyPath);
            return key?.GetValue(valueName);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener el valor {valueName} de la clave {keyPath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene una clave del registro a partir de su ruta.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave.</param>
    /// <returns>La clave del registro, o null si no existe.</returns>
    private RegistryKey? GetRegistryKey(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
            return null;

        try
        {
            // Determinar la base de la clave
            RegistryHive hive;
            if (keyPath.StartsWith("HKEY_LOCAL_MACHINE") || keyPath.StartsWith("HKLM"))
                hive = RegistryHive.LocalMachine;
            else if (keyPath.StartsWith("HKEY_CURRENT_USER") || keyPath.StartsWith("HKCU"))
                hive = RegistryHive.CurrentUser;
            else if (keyPath.StartsWith("HKEY_CLASSES_ROOT") || keyPath.StartsWith("HKCR"))
                hive = RegistryHive.ClassesRoot;
            else if (keyPath.StartsWith("HKEY_USERS") || keyPath.StartsWith("HKU"))
                hive = RegistryHive.Users;
            else
                return null;

            // Extraer la parte de la ruta después de la base
            string subKeyPath;
            if (keyPath.StartsWith("HKLM\\"))
                subKeyPath = keyPath.Substring(5);
            else if (keyPath.StartsWith("HKEY_LOCAL_MACHINE\\"))
                subKeyPath = keyPath.Substring(19);
            else if (keyPath.StartsWith("HKCU\\"))
                subKeyPath = keyPath.Substring(5);
            else if (keyPath.StartsWith("HKEY_CURRENT_USER\\"))
                subKeyPath = keyPath.Substring(18);
            else if (keyPath.StartsWith("HKCR\\"))
                subKeyPath = keyPath.Substring(5);
            else if (keyPath.StartsWith("HKEY_CLASSES_ROOT\\"))
                subKeyPath = keyPath.Substring(18);
            else if (keyPath.StartsWith("HKU\\"))
                subKeyPath = keyPath.Substring(4);
            else if (keyPath.StartsWith("HKEY_USERS\\"))
                subKeyPath = keyPath.Substring(11);
            else
                return null;

            // Abrir la clave
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            return baseKey.OpenSubKey(subKeyPath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener la clave {keyPath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exporta una clave del registro a un archivo .reg.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave a exportar.</param>
    /// <param name="filePath">Ruta del archivo donde se guardará la exportación.</param>
    /// <returns>True si la exportación fue exitosa, false en caso contrario.</returns>
    public bool ExportRegistryKey(string keyPath, string filePath)
    {
        try
        {
            _logger.LogInfo($"Exportando clave de registro {keyPath} a {filePath}...");

            // Asegurarse de que el directorio destino exista
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Usar el comando reg.exe para exportar la clave
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{keyPath}\" \"{filePath}\" /y",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError($"No se pudo iniciar el proceso para exportar la clave {keyPath}");
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Error al exportar la clave {keyPath}: {error}");
                return false;
            }

            _logger.LogInfo($"Clave de registro {keyPath} exportada con éxito a {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al exportar la clave {keyPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Importa un archivo .reg al registro.
    /// </summary>
    /// <param name="filePath">Ruta del archivo .reg a importar.</param>
    /// <returns>True si la importación fue exitosa, false en caso contrario.</returns>
    public bool ImportRegistryFile(string filePath)
    {
        try
        {
            _logger.LogInfo($"Importando archivo de registro {filePath}...");

            // Verificar que el archivo exista
            if (!File.Exists(filePath))
            {
                _logger.LogError($"El archivo {filePath} no existe");
                return false;
            }

            // Usar el comando reg.exe para importar el archivo
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"import \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError($"No se pudo iniciar el proceso para importar el archivo {filePath}");
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Error al importar el archivo {filePath}: {error}");
                return false;
            }

            _logger.LogInfo($"Archivo de registro {filePath} importado con éxito");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al importar el archivo {filePath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Busca claves de registro relacionadas con Adobe Photoshop en una clave raíz.
    /// </summary>
    /// <param name="rootKey">Clave raíz donde buscar.</param>
    /// <returns>Lista de claves de registro encontradas.</returns>
    public List<string> FindPhotoshopRegistryKeys(string rootKey)
    {
        var keys = new List<string>();

        try
        {
            _logger.LogInfo($"Buscando claves de registro relacionadas con Photoshop en {rootKey}...");

            // Determinar la base de la clave
            RegistryHive hive;
            string subKeyPath;

            if (rootKey.StartsWith("HKEY_LOCAL_MACHINE") || rootKey.StartsWith("HKLM"))
            {
                hive = RegistryHive.LocalMachine;
                subKeyPath = rootKey.StartsWith("HKLM\\") ? rootKey.Substring(5) : rootKey.Substring(19);
            }
            else if (rootKey.StartsWith("HKEY_CURRENT_USER") || rootKey.StartsWith("HKCU"))
            {
                hive = RegistryHive.CurrentUser;
                subKeyPath = rootKey.StartsWith("HKCU\\") ? rootKey.Substring(5) : rootKey.Substring(18);
            }
            else if (rootKey.StartsWith("HKEY_CLASSES_ROOT") || rootKey.StartsWith("HKCR"))
            {
                hive = RegistryHive.ClassesRoot;
                subKeyPath = rootKey.StartsWith("HKCR\\") ? rootKey.Substring(5) : rootKey.Substring(18);
            }
            else
            {
                _logger.LogWarning($"Clave raíz no soportada: {rootKey}");
                return keys;
            }

            // Abrir la clave raíz
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKeyPath);

            if (key == null)
            {
                _logger.LogWarning($"No se pudo abrir la clave raíz: {rootKey}");
                return keys;
            }

            // Buscar subclaves
            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null)
                    continue;

                var displayName = subKey.GetValue("DisplayName") as string;

                // Verificar si es una clave relacionada con Photoshop
                if (!string.IsNullOrEmpty(displayName) &&
                    (displayName.Contains("Adobe Photoshop", StringComparison.OrdinalIgnoreCase) ||
                     displayName.Contains("Photoshop", StringComparison.OrdinalIgnoreCase)))
                {
                    string fullKeyPath = $"{rootKey}\\{subKeyName}";
                    keys.Add(fullKeyPath);
                    _logger.LogInfo($"Encontrada clave de registro relacionada con Photoshop: {fullKeyPath}");
                }
            }

            _logger.LogInfo($"Se encontraron {keys.Count} claves de registro relacionadas con Photoshop en {rootKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al buscar claves de registro relacionadas con Photoshop: {ex.Message}");
        }

        return keys;
    }

    /// <summary>
    /// Elimina una clave del registro.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, false en caso contrario.</returns>
    public bool DeleteRegistryKey(string keyPath)
    {
        try
        {
            _logger.LogInfo($"Eliminando clave de registro: {keyPath}...");

            // Determinar la base de la clave
            RegistryHive hive;
            string subKeyPath;
            string parentKeyPath;
            string keyName;

            if (keyPath.StartsWith("HKEY_LOCAL_MACHINE") || keyPath.StartsWith("HKLM"))
            {
                hive = RegistryHive.LocalMachine;
                subKeyPath = keyPath.StartsWith("HKLM\\") ? keyPath.Substring(5) : keyPath.Substring(19);
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_USER") || keyPath.StartsWith("HKCU"))
            {
                hive = RegistryHive.CurrentUser;
                subKeyPath = keyPath.StartsWith("HKCU\\") ? keyPath.Substring(5) : keyPath.Substring(18);
            }
            else if (keyPath.StartsWith("HKEY_CLASSES_ROOT") || keyPath.StartsWith("HKCR"))
            {
                hive = RegistryHive.ClassesRoot;
                subKeyPath = keyPath.StartsWith("HKCR\\") ? keyPath.Substring(5) : keyPath.Substring(18);
            }
            else
            {
                _logger.LogWarning($"Clave no soportada: {keyPath}");
                return false;
            }

            // Separar la ruta de la clave padre y el nombre de la clave a eliminar
            int lastBackslashIndex = subKeyPath.LastIndexOf('\\');
            if (lastBackslashIndex == -1)
            {
                // Es una clave de primer nivel, no se puede eliminar
                _logger.LogWarning($"No se puede eliminar una clave de primer nivel: {keyPath}");
                return false;
            }

            parentKeyPath = subKeyPath.Substring(0, lastBackslashIndex);
            keyName = subKeyPath.Substring(lastBackslashIndex + 1);

            // Abrir la clave padre
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var parentKey = baseKey.OpenSubKey(parentKeyPath, true);

            if (parentKey == null)
            {
                _logger.LogWarning($"No se pudo abrir la clave padre: {parentKeyPath}");
                return false;
            }

            // Eliminar la clave
            parentKey.DeleteSubKeyTree(keyName, false);
            _logger.LogInfo($"Clave de registro eliminada con éxito: {keyPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar la clave {keyPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Elimina una clave del registro utilizando reg.exe.
    /// </summary>
    /// <param name="keyPath">Ruta de la clave a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, false en caso contrario.</returns>
    public bool DeleteRegistryKeyWithRegExe(string keyPath)
    {
        try
        {
            _logger.LogInfo($"Eliminando clave de registro con reg.exe: {keyPath}...");

            // Usar el comando reg.exe para eliminar la clave
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"delete \"{keyPath}\" /f",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError($"No se pudo iniciar el proceso para eliminar la clave {keyPath}");
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Error al eliminar la clave {keyPath} con reg.exe: {error}");
                return false;
            }

            _logger.LogInfo($"Clave de registro eliminada con éxito con reg.exe: {keyPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar la clave {keyPath} con reg.exe: {ex.Message}");
            return false;
        }
    }
}