namespace DesinstalaPhotoshop.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DesinstalaPhotoshop.Core.Services.Interfaces;

/// <summary>
/// Implementación del generador de scripts de limpieza.
/// </summary>
public class ScriptGenerator : IScriptGenerator
{
    private readonly ILoggingService _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ScriptGenerator"/>.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    public ScriptGenerator(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Genera un script de limpieza de registro.
    /// </summary>
    /// <param name="regDeleteCommands">Lista de comandos reg delete.</param>
    /// <param name="outputPath">Ruta donde guardar el script.</param>
    /// <param name="scriptType">Tipo de script (BAT o PS1).</param>
    /// <returns>true si el script se generó correctamente, false en caso contrario.</returns>
    public bool GenerateCleanupScript(
        List<string> regDeleteCommands,
        string outputPath,
        ScriptType scriptType)
    {
        try
        {
            _logger.LogInfo($"Generando script de limpieza en formato {scriptType} en {outputPath}");

            // Crear directorio si no existe
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Crear contenido del script según el tipo
            StringBuilder scriptContent = new StringBuilder();

            if (scriptType == ScriptType.BAT)
            {
                GenerateBatScript(scriptContent, regDeleteCommands);
            }
            else if (scriptType == ScriptType.PS1)
            {
                GeneratePowerShellScript(scriptContent, regDeleteCommands);
            }

            // Guardar script
            File.WriteAllText(outputPath, scriptContent.ToString());
            _logger.LogInfo($"Script de limpieza generado correctamente en {outputPath}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al generar script de limpieza: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Extrae comandos reg delete del texto de la consola.
    /// </summary>
    /// <param name="consoleText">Texto de la consola.</param>
    /// <returns>Lista de comandos reg delete encontrados.</returns>
    public List<string> ExtractRegDeleteCommands(string consoleText)
    {
        var commands = new List<string>();

        try
        {
            // Patrón para detectar comandos reg delete
            string pattern = @"reg delete\s+""?([^""]+)""?\s+(/v\s+""?[^""]+""?)?\s+(/f)?";
            var matches = Regex.Matches(consoleText, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                commands.Add(match.Value.Trim());
            }

            _logger.LogInfo($"Se encontraron {commands.Count} comandos reg delete en el texto de la consola");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al extraer comandos reg delete: {ex.Message}");
        }

        return commands;
    }

    /// <summary>
    /// Genera un script de archivo por lotes (.bat) para CMD.
    /// </summary>
    /// <param name="scriptContent">StringBuilder donde se agregará el contenido del script.</param>
    /// <param name="regDeleteCommands">Lista de comandos reg delete.</param>
    private void GenerateBatScript(StringBuilder scriptContent, List<string> regDeleteCommands)
    {
        // Encabezado para script .bat
        scriptContent.AppendLine("@echo off");
        scriptContent.AppendLine("echo ===================================================");
        scriptContent.AppendLine("echo Script de limpieza de residuos de Adobe Photoshop");
        scriptContent.AppendLine("echo Generado por DesinstalaPhotoshop");
        scriptContent.AppendLine($"echo Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        scriptContent.AppendLine("echo ===================================================");
        scriptContent.AppendLine("echo.");
        scriptContent.AppendLine("echo Este script eliminará entradas de registro residuales de Adobe Photoshop.");
        scriptContent.AppendLine("echo Se recomienda crear una copia de seguridad del registro antes de ejecutar este script.");
        scriptContent.AppendLine("echo.");
        scriptContent.AppendLine("pause");
        scriptContent.AppendLine("echo.");
        scriptContent.AppendLine("echo Eliminando entradas de registro...");
        scriptContent.AppendLine("echo.");

        // Agregar comandos
        foreach (var command in regDeleteCommands)
        {
            scriptContent.AppendLine($"echo Ejecutando: {command}");
            scriptContent.AppendLine(command);
            scriptContent.AppendLine("if %ERRORLEVEL% NEQ 0 echo   - Error al ejecutar el comando");
            scriptContent.AppendLine("echo.");
        }

        // Finalizar script
        scriptContent.AppendLine("echo.");
        scriptContent.AppendLine("echo Limpieza completada.");
        scriptContent.AppendLine("echo.");
        scriptContent.AppendLine("pause");
    }

    /// <summary>
    /// Genera un script de PowerShell (.ps1).
    /// </summary>
    /// <param name="scriptContent">StringBuilder donde se agregará el contenido del script.</param>
    /// <param name="regDeleteCommands">Lista de comandos reg delete.</param>
    private void GeneratePowerShellScript(StringBuilder scriptContent, List<string> regDeleteCommands)
    {
        // Encabezado para script .ps1
        scriptContent.AppendLine("# Script de limpieza de residuos de Adobe Photoshop");
        scriptContent.AppendLine("# Generado por DesinstalaPhotoshop");
        scriptContent.AppendLine($"# Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        scriptContent.AppendLine("");
        scriptContent.AppendLine("Write-Host \"===================================================\" -ForegroundColor Cyan");
        scriptContent.AppendLine("Write-Host \"Script de limpieza de residuos de Adobe Photoshop\" -ForegroundColor Cyan");
        scriptContent.AppendLine("Write-Host \"Generado por DesinstalaPhotoshop\" -ForegroundColor Cyan");
        scriptContent.AppendLine($"Write-Host \"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\" -ForegroundColor Cyan");
        scriptContent.AppendLine("Write-Host \"===================================================\" -ForegroundColor Cyan");
        scriptContent.AppendLine("Write-Host \"\"");
        scriptContent.AppendLine("Write-Host \"Este script eliminará entradas de registro residuales de Adobe Photoshop.\" -ForegroundColor Yellow");
        scriptContent.AppendLine("Write-Host \"Se recomienda crear una copia de seguridad del registro antes de ejecutar este script.\" -ForegroundColor Yellow");
        scriptContent.AppendLine("Write-Host \"\"");
        scriptContent.AppendLine("Read-Host \"Presione Enter para continuar\"");
        scriptContent.AppendLine("Write-Host \"\"");
        scriptContent.AppendLine("Write-Host \"Eliminando entradas de registro...\" -ForegroundColor Green");
        scriptContent.AppendLine("Write-Host \"\"");

        // Agregar comandos (convertidos a PowerShell)
        foreach (var command in regDeleteCommands)
        {
            // Convertir comando reg.exe a PowerShell
            string psCommand = ConvertRegCommandToPowerShell(command);

            scriptContent.AppendLine($"Write-Host \"Ejecutando: {command}\" -ForegroundColor Gray");
            scriptContent.AppendLine("try {");
            scriptContent.AppendLine($"    {psCommand}");
            scriptContent.AppendLine("    Write-Host \"  - Comando ejecutado correctamente\" -ForegroundColor Green");
            scriptContent.AppendLine("} catch {");
            scriptContent.AppendLine("    Write-Host \"  - Error al ejecutar el comando: $_\" -ForegroundColor Red");
            scriptContent.AppendLine("}");
            scriptContent.AppendLine("Write-Host \"\"");
        }

        // Finalizar script
        scriptContent.AppendLine("Write-Host \"\"");
        scriptContent.AppendLine("Write-Host \"Limpieza completada.\" -ForegroundColor Green");
        scriptContent.AppendLine("Write-Host \"\"");
        scriptContent.AppendLine("Read-Host \"Presione Enter para salir\"");
    }

    /// <summary>
    /// Convierte un comando reg.exe a PowerShell.
    /// </summary>
    /// <param name="regCommand">Comando reg.exe.</param>
    /// <returns>Comando PowerShell equivalente.</returns>
    private string ConvertRegCommandToPowerShell(string regCommand)
    {
        try
        {
            // Ejemplo: reg delete "HKLM\Software\Adobe" /v "Version" /f
            // Convertir a: Remove-Item -Path "HKLM:\Software\Adobe" -Name "Version" -Force

            // Extraer la ruta del registro
            var match = Regex.Match(regCommand, @"reg delete\s+""?([^""]+)""?");
            if (!match.Success)
            {
                return $"& {regCommand}"; // Ejecutar como comando externo si no se puede convertir
            }

            string regPath = match.Groups[1].Value.Replace("HKLM\\", "HKLM:\\").Replace("HKCU\\", "HKCU:\\");

            // Verificar si hay un valor específico
            var valueMatch = Regex.Match(regCommand, @"/v\s+""?([^""]+)""?");
            string psCommand;

            if (valueMatch.Success)
            {
                string valueName = valueMatch.Groups[1].Value;
                psCommand = $"Remove-ItemProperty -Path \"{regPath}\" -Name \"{valueName}\" -Force";
            }
            else
            {
                psCommand = $"Remove-Item -Path \"{regPath}\" -Force -Recurse";
            }

            return psCommand;
        }
        catch
        {
            // En caso de error, ejecutar el comando original como proceso externo
            return $"& {regCommand}";
        }
    }
}
