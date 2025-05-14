using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace DesinstalaPhotoshop.Core.Services.Helpers;

/// <summary>
/// Proporciona métodos auxiliares para operaciones relacionadas con permisos de administrador.
/// </summary>
[SupportedOSPlatform("windows")]
public static class AdminHelper
{
    /// <summary>
    /// Verifica si la aplicación está ejecutándose con permisos de administrador.
    /// </summary>
    /// <returns>True si la aplicación tiene permisos de administrador, false en caso contrario.</returns>
    public static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Reinicia la aplicación con permisos de administrador.
    /// </summary>
    /// <param name="arguments">Argumentos adicionales para pasar a la aplicación reiniciada.</param>
    /// <returns>True si la aplicación se reinició correctamente, false en caso contrario.</returns>
    public static bool RestartAsAdmin(string arguments = "")
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule?.FileName ?? Process.GetCurrentProcess().StartInfo.FileName,
                Verb = "runas",
                Arguments = arguments
            };

            Process.Start(startInfo);
            // No podemos usar Application.Exit() aquí porque no tenemos referencia a System.Windows.Forms
            Environment.Exit(0);
            return true;
        }
        catch (Exception ex)
        {
            // Registrar el error pero no lanzar excepción
            // ya que esto podría ser llamado desde un manejador de eventos de UI
            Debug.WriteLine($"Error al reiniciar como administrador: {ex.Message}");
            return false;
        }
    }
}
