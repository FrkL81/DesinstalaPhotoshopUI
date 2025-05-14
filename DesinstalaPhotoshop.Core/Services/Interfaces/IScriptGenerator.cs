namespace DesinstalaPhotoshop.Core.Services.Interfaces;

/// <summary>
/// Define los tipos de script que se pueden generar.
/// </summary>
public enum ScriptType
{
    /// <summary>
    /// Script de archivo por lotes (.bat) para CMD.
    /// </summary>
    BAT,

    /// <summary>
    /// Script de PowerShell (.ps1).
    /// </summary>
    PS1
}

/// <summary>
/// Define operaciones para generar scripts de limpieza.
/// </summary>
public interface IScriptGenerator
{
    /// <summary>
    /// Genera un script de limpieza de registro.
    /// </summary>
    /// <param name="regDeleteCommands">Lista de comandos reg delete.</param>
    /// <param name="outputPath">Ruta donde guardar el script.</param>
    /// <param name="scriptType">Tipo de script (BAT o PS1).</param>
    /// <returns>true si el script se generó correctamente, false en caso contrario.</returns>
    bool GenerateCleanupScript(
        List<string> regDeleteCommands,
        string outputPath,
        ScriptType scriptType);

    /// <summary>
    /// Extrae comandos reg delete del texto de la consola.
    /// </summary>
    /// <param name="consoleText">Texto de la consola.</param>
    /// <returns>Lista de comandos reg delete encontrados.</returns>
    List<string> ExtractRegDeleteCommands(string consoleText);
}
