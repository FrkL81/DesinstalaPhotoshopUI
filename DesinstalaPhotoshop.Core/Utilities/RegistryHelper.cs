using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace DesinstalaPhotoshop.Core.Utilities
{
    /// <summary>
    /// Proporciona métodos para acceder, modificar y eliminar claves y valores del registro de Windows.
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Verifica si una clave del registro existe.
        /// </summary>
        /// <param name="keyPath">Ruta de la clave a verificar.</param>
        /// <returns>true si la clave existe, false en caso contrario.</returns>
        public static bool KeyExists(string keyPath)
        {
            try
            {
                var rootKey = GetRegistryKeyFromPath(keyPath, out string? subKeyPath);
                if (rootKey == null || string.IsNullOrEmpty(subKeyPath))
                    return false;

                using var key = rootKey.OpenSubKey(subKeyPath);
                return key != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si un valor específico existe en una clave del registro.
        /// </summary>
        /// <param name="keyPath">Ruta de la clave.</param>
        /// <param name="valueName">Nombre del valor a verificar.</param>
        /// <returns>true si el valor existe, false en caso contrario.</returns>
        public static bool ValueExists(string keyPath, string valueName)
        {
            try
            {
                var value = Registry.GetValue(keyPath, valueName, null);
                return value != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene el valor de una clave del registro.
        /// </summary>
        /// <param name="keyPath">Ruta de la clave.</param>
        /// <param name="valueName">Nombre del valor a obtener.</param>
        /// <param name="defaultValue">Valor por defecto si no se encuentra.</param>
        /// <returns>Valor obtenido o defaultValue si no se encuentra.</returns>
        public static object? GetRegistryKeyValue(string keyPath, string valueName, object? defaultValue = null)
        {
            try
            {
                return Registry.GetValue(keyPath, valueName, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Elimina una clave del registro de Windows.
        /// </summary>
        /// <param name="keyPath">Ruta completa de la clave a eliminar.</param>
        /// <param name="whatIf">Indica si se debe simular la eliminación.</param>
        /// <param name="useRegExe">Indica si se debe utilizar reg.exe como método alternativo.</param>
        /// <returns>true si la clave se eliminó correctamente, false en caso contrario.</returns>
        public static bool DeleteRegistryKey(string keyPath, bool whatIf = false, bool useRegExe = false)
        {
            if (whatIf)
            {
                return true;
            }

            if (useRegExe)
            {
                return ExecuteRegDelete(keyPath, null, whatIf);
            }

            try
            {
                var rootKey = GetRegistryKeyFromPath(keyPath, out string? subKeyPath);
                if (rootKey == null || string.IsNullOrEmpty(subKeyPath))
                    return false;

                // Obtener la clave padre y el nombre de la subclave a eliminar
                int lastBackslash = subKeyPath.LastIndexOf('\\');
                if (lastBackslash == -1)
                {
                    // No hay subclave, es una clave directa de la raíz
                    rootKey.DeleteSubKeyTree(subKeyPath, false);
                    return true;
                }

                string parentKeyPath = subKeyPath.Substring(0, lastBackslash);
                string subKeyName = subKeyPath.Substring(lastBackslash + 1);

                using var parentKey = rootKey.OpenSubKey(parentKeyPath, true);
                if (parentKey == null)
                    return false;

                parentKey.DeleteSubKeyTree(subKeyName, false);
                return true;
            }
            catch (Exception)
            {
                // Si falla, intentar con reg.exe
                if (!useRegExe)
                {
                    return DeleteRegistryKey(keyPath, whatIf, true);
                }
                return false;
            }
        }

        /// <summary>
        /// Elimina un valor específico de una clave del registro.
        /// </summary>
        /// <param name="keyPath">Ruta de la clave que contiene el valor.</param>
        /// <param name="valueName">Nombre del valor a eliminar.</param>
        /// <param name="whatIf">Indica si se debe simular la eliminación.</param>
        /// <param name="useRegExe">Indica si se debe utilizar reg.exe como método alternativo.</param>
        /// <returns>true si el valor se eliminó correctamente, false en caso contrario.</returns>
        public static bool DeleteRegistryValue(string keyPath, string valueName, bool whatIf = false, bool useRegExe = false)
        {
            if (whatIf)
            {
                return true;
            }

            if (useRegExe)
            {
                return ExecuteRegDelete(keyPath, valueName, whatIf);
            }

            try
            {
                var rootKey = GetRegistryKeyFromPath(keyPath, out string? subKeyPath);
                if (rootKey == null || string.IsNullOrEmpty(subKeyPath))
                    return false;

                using var key = rootKey.OpenSubKey(subKeyPath, true);
                if (key == null)
                    return false;

                key.DeleteValue(valueName, false);
                return true;
            }
            catch (Exception)
            {
                // Si falla, intentar con reg.exe
                if (!useRegExe)
                {
                    return DeleteRegistryValue(keyPath, valueName, whatIf, true);
                }
                return false;
            }
        }

        /// <summary>
        /// Ejecuta el comando reg.exe para eliminar una clave o valor del registro.
        /// </summary>
        /// <param name="keyPath">Ruta de la clave a eliminar.</param>
        /// <param name="valueName">Nombre del valor a eliminar (opcional).</param>
        /// <param name="whatIf">Indica si se debe simular la eliminación.</param>
        /// <returns>true si la operación se completó correctamente, false en caso contrario.</returns>
        public static bool ExecuteRegDelete(string keyPath, string? valueName = null, bool whatIf = false)
        {
            if (whatIf)
            {
                return true;
            }

            try
            {
                string regPath = ConvertRegistryPathToRegFormat(keyPath);
                string arguments = $"delete \"{regPath}\"";

                if (!string.IsNullOrEmpty(valueName))
                {
                    arguments += $" /v \"{valueName}\"";
                }

                arguments += " /f";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    return false;

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Convierte una ruta de registro al formato utilizado por reg.exe.
        /// </summary>
        /// <param name="keyPath">Ruta de registro en formato .NET.</param>
        /// <returns>Ruta de registro en formato reg.exe.</returns>
        public static string ConvertRegistryPathToRegFormat(string keyPath)
        {
            if (keyPath.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
            {
                return "HKLM\\" + keyPath.Substring("HKEY_LOCAL_MACHINE\\".Length);
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
            {
                return "HKCU\\" + keyPath.Substring("HKEY_CURRENT_USER\\".Length);
            }
            else if (keyPath.StartsWith("HKEY_CLASSES_ROOT\\", StringComparison.OrdinalIgnoreCase))
            {
                return "HKCR\\" + keyPath.Substring("HKEY_CLASSES_ROOT\\".Length);
            }
            else if (keyPath.StartsWith("HKEY_USERS\\", StringComparison.OrdinalIgnoreCase))
            {
                return "HKU\\" + keyPath.Substring("HKEY_USERS\\".Length);
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_CONFIG\\", StringComparison.OrdinalIgnoreCase))
            {
                return "HKCC\\" + keyPath.Substring("HKEY_CURRENT_CONFIG\\".Length);
            }

            return keyPath;
        }

        /// <summary>
        /// Extrae la raíz del registro y la subruta de una ruta completa.
        /// </summary>
        /// <param name="keyPath">Ruta completa de la clave.</param>
        /// <param name="subKeyPath">Variable de salida para la subruta.</param>
        /// <returns>La raíz del registro correspondiente o null si no se reconoce.</returns>
        public static RegistryKey? GetRegistryKeyFromPath(string keyPath, out string? subKeyPath)
        {
            subKeyPath = null;

            if (keyPath.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
            {
                subKeyPath = keyPath.Substring("HKEY_LOCAL_MACHINE\\".Length);
                return Registry.LocalMachine;
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
            {
                subKeyPath = keyPath.Substring("HKEY_CURRENT_USER\\".Length);
                return Registry.CurrentUser;
            }
            else if (keyPath.StartsWith("HKEY_CLASSES_ROOT\\", StringComparison.OrdinalIgnoreCase))
            {
                subKeyPath = keyPath.Substring("HKEY_CLASSES_ROOT\\".Length);
                return Registry.ClassesRoot;
            }
            else if (keyPath.StartsWith("HKEY_USERS\\", StringComparison.OrdinalIgnoreCase))
            {
                subKeyPath = keyPath.Substring("HKEY_USERS\\".Length);
                return Registry.Users;
            }
            else if (keyPath.StartsWith("HKEY_CURRENT_CONFIG\\", StringComparison.OrdinalIgnoreCase))
            {
                subKeyPath = keyPath.Substring("HKEY_CURRENT_CONFIG\\".Length);
                return Registry.CurrentConfig;
            }

            return null;
        }
    }
}
