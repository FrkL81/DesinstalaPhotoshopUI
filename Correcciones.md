El error CS0723 es claro: no se puede declarar una instancia de una clase estática. `AdminHelper` está correctamente definido como estático en el Core, por lo que no necesitamos ni podemos crear una instancia de él en `MainForm`.

Aquí están las correcciones:

### **Solución al Error de Compilación CS0723 y Advertencias CS1998/CS8625**

#### **1. Corregir Error CS0723 en `DesinstalaPhotoshop.UI\MainForm.cs`**

*   **Problema:** Se está intentando declarar una instancia de la clase estática `AdminHelper`.
*   **Archivo:** `C:\MiRepo\DPui3\DesinstalaPhotoshop.UI\MainForm.cs`

*   **Fragmento de Código Original (con el error):**
    ```csharp
    // ...
    private readonly IScriptGenerator _scriptGenerator;
    private readonly AdminHelper _adminHelper; // <--- LÍNEA CON ERROR (37)

    // Lista de instalaciones detectadas (usando el modelo del Core)
    // ...
    ```
    Y en el constructor:
    ```csharp
    // ...
    _adminHelper = new AdminHelper(); // AdminHelper es estático, no se necesita instancia, pero lo dejamos para consistencia de uso futuro.
    // ...
    ```

*   **Reemplazar con (eliminar la declaración de instancia y la inicialización):**
    1.  **Eliminar la línea de declaración del campo:**
        Busca la línea:
        ```csharp
        private readonly AdminHelper _adminHelper; 
        ```
        Y elimínala completamente.

    2.  **Eliminar la línea de inicialización en el constructor:**
        Busca la línea dentro del constructor `public MainForm(bool startedElevated = false)`:
        ```csharp
        _adminHelper = new AdminHelper(); // AdminHelper es estático, no se necesita instancia, pero lo dejamos para consistencia de uso futuro.
        ```
        Y elimínala completamente.

    *   **Justificación:** `AdminHelper` en `DesinstalaPhotoshop.Core.Services.Helpers` es una clase estática. Sus métodos (`IsRunningAsAdmin`, `RestartAsAdmin`) se invocan directamente sobre el nombre de la clase (ej. `AdminHelper.IsRunningAsAdmin()`), no a través de una instancia. Esto ya se estaba haciendo correctamente en otras partes de `MainForm.cs`.

#### **2. Abordar Advertencias CS1998 (Método asincrónico sin `await`)**

Estas advertencias indican que un método marcado como `async Task` no contiene ningún operador `await`. Esto significa que el método se ejecutará sincrónicamente, lo cual puede no ser la intención y puede llevar a un comportamiento inesperado si se espera que la operación se ejecute en segundo plano.

*   **Archivos Afectados:**
    *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs` (varios métodos)
    *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\ProcessService.cs` (un método)

*   **Decisión:**
    *   Si el método *realmente no realiza ninguna operación asincrónica* (I/O-bound o CPU-bound que deba ser descargada), entonces la firma del método debe cambiarse de `async Task<OperationResult>` a `Task<OperationResult>` y devolver `Task.FromResult(new OperationResult(...))` o, si es `void`, cambiar de `async Task` a `void` (si no se necesita esperar su finalización) o `Task` y devolver `Task.CompletedTask`.
    *   Si se espera que el método sea asincrónico pero la implementación actual es un placeholder (como `await Task.Delay(500, cancellationToken); // Simulación`), entonces la advertencia es aceptable *temporalmente* durante el desarrollo, pero debe ser resuelta cuando se implemente la lógica real con operaciones `awaitable`.

*   **Instrucciones Específicas (Ejemplo para `CleanupTempFilesAsync` en `CleanupService.cs`):**
    *   **Archivo:** `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs`
    *   **Método:** `CleanupTempFilesAsync` (línea 180, según el log)
    *   **Análisis:** Este método realiza operaciones de sistema de archivos que son inherentemente I/O-bound. Aunque los métodos de `_fileSystemHelper` (ej. `DeleteFile`, `DeleteDirectory`) no son `async` en su firma actual, las operaciones de archivos *pueden* ser lentas. Si no hay versiones `Async` de estos helpers o de `System.IO` que se quieran usar, y se desea que la *unidad de trabajo completa* del método `CleanupTempFilesAsync` sea no bloqueante para el llamador, se puede envolver la lógica en `Task.Run`. Sin embargo, dado que `CleanupAsync` ya es `async` y llama a estos métodos, y los helpers no son `async`, la advertencia es porque `CleanupTempFilesAsync` no tiene *su propio* `await`.

    *   **Acción Sugerida (Opción 1: Mantener `async` y usar `Task.Run` si las operaciones internas son bloqueantes y largas):**
        Si se considera que las operaciones de `_fileSystemHelper.DeleteFile` y `_fileSystemHelper.DeleteDirectory` (y las búsquedas de archivos) pueden ser bloqueantes por un tiempo considerable y queremos asegurar que `CleanupTempFilesAsync` ceda el control rápidamente:
        ```csharp
        // En CleanupService.cs, dentro de CleanupTempFilesAsync
        private async Task CleanupTempFilesAsync(
            PhotoshopInstallation installation,
            bool whatIf,
            IProgress<ProgressInfo>? progress,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo("Iniciando limpieza de archivos temporales...");
        
            await Task.Run(() => // Envolver la lógica bloqueante
            {
                try
                {
                    // ... (lógica existente de CleanupTempFilesAsync) ...
                    // Por ejemplo:
                    string tempPath = Path.GetTempPath();
                    _logger.LogInfo($"Buscando archivos temporales de Photoshop en {tempPath}...");
                    string[] tempPatterns = { "~PST*.tmp", "Photoshop Temp*", "Adobe_Photoshop*.tmp" };

                    foreach (var pattern in tempPatterns)
                    {
                        if (cancellationToken.IsCancellationRequested) return; // Salir si se cancela
                        var tempFiles = _fileSystemHelper.FindFiles(tempPath, pattern);
                        // ... resto del bucle ...
                    }
                    // ... resto de la lógica ...
                    _logger.LogInfo("Limpieza de archivos temporales completada.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error durante la limpieza de archivos temporales: {ex.Message}");
                    // No relanzar aquí si se maneja el error y se quiere que la operación general continúe
                    // o se propague como un OperationResult fallido.
                    // Considerar si esta excepción debe propagarse o simplemente registrarse.
                }
            }, cancellationToken);
        }
        ```
        **Repetir un patrón similar para los otros métodos con CS1998:**
        *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs(474,24)`: `CleanupRegistryAsync`
        *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs(645,24)`: `CleanupConfigFilesAsync`
        *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs(873,30)`: `CleanupCacheFilesAsync`
        *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs(1049,30)`: `ScheduleFileForDeletionAsync` (y probablemente `ScheduleDirectoryForDeletionAsync` también, aunque no listada)
        *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\ProcessService.cs(220,41)`: `StopAdobeServicesAsync` (ya contiene bucles y esperas, podría beneficiarse de `Task.Run` o de usar APIs `async` de `ServiceController` si existen y son apropiadas).

    *   **Acción Sugerida (Opción 2: Convertir a sincrónico si no hay `await` intencional):**
        Si el método realmente no necesita ser `async` porque sus operaciones son rápidas o la asincronía se maneja en un nivel superior:
        ```csharp
        // Ejemplo para un método hipotético que no necesita ser async
        // private Task<OperationResult> MetodoSincronicoConTaskWrapperAsync(...)
        private Task<OperationResult> MetodoSincronicoConTaskWrapper(/*...*/)
        {
            // ... lógica sincrónica ...
            return Task.FromResult(OperationResult.SuccessResult("Operación síncrona completada"));
        }
        ```
        **Decisión para el Proyecto:** Para los métodos de `CleanupService` y `ProcessService` listados, la **Opción 1 (usar `await Task.Run`)** es más apropiada, ya que realizan operaciones de sistema de archivos, registro y control de servicios que pueden ser bloqueantes y se benefician de ser ejecutadas en un hilo de ThreadPool para no bloquear el hilo llamador (especialmente importante si el llamador es la UI o un servicio que maneja múltiples tareas).

#### **3. Abordar Advertencias CS8625 (Literal NULL a tipo no nullable)**

Estas advertencias aparecen cuando se pasa `null` a un parámetro que no está marcado como nullable (ej. `string` en lugar de `string?`).

*   **Archivos Afectados:**
    *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs(384,76)`: `await _backupService.CreateBackupForCleanupAsync(...)`
    *   `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\DetectionService.cs(736,76)` y `(745,92)`: en `DetectInstallationsAsync` al llamar a `progress?.Report(...)`

*   **Corrección para `CleanupService.cs(384,76)`:**
    *   **Contexto:** La llamada a `_backupService.CreateBackupForCleanupAsync` en `CleanupService.cs` se parece a la versión de `07_Codigo_Fuente_Metodos_Clave.md` que toma patrones de archivos/registro. Sin embargo, la implementación real de `_backupService.CreateBackupForCleanupAsync` en `BackupService.cs` toma una `PhotoshopInstallation`.
    *   **Fragmento de Código Actual (en `CleanupService.cs` dentro de `CleanupAsync`):**
        ```csharp
        // ...
        if (createBackup)
        {
            progress?.Report(ProgressInfo.Running(5, "Creando copia de seguridad", "Preparando respaldo..."));
            // LA SIGUIENTE LLAMADA CAUSA LA ADVERTENCIA PORQUE LA FIRMA EN 07_...MD ES DIFERENTE A LA IMPLEMENTACIÓN
            var backupResult = await _backupService.CreateBackupForCleanupAsync( 
                FilePatterns.GetCleanupFilePatterns(), // ESTO NO EXISTE
                RegistryPatterns.GetCleanupRegistryPatterns(), // ESTO NO EXISTE
                progress,
                cancellationToken);
        // ...
        ```
        *Debería ser:*
        ```csharp
        // ...
        if (createBackup)
        {
            progress?.Report(ProgressInfo.Running(5, "Creando copia de seguridad", "Preparando respaldo..."));
            // Aquí 'installation' es el parámetro de CleanupAsync
            string backupId = await _backupService.CreateBackupForCleanupAsync(
                installation, // Usar la instalación actual
                progress,
                cancellationToken);

            // El método CreateBackupForCleanupAsync devuelve string (backupId)
            // y OperationResult se maneja de otra forma o no es devuelto directamente por este método
            // según la interfaz IBackupService.cs.
            // Necesitamos adaptar esto.
            // El OperationResult de CreateBackupForCleanupAsync en 07_Codigo_Fuente_Metodos_Clave.md
            // no coincide con la firma en IBackupService.cs que devuelve Task<string>
            // Asumamos que necesitamos el backupId y que el éxito se infiere si no hay excepción.

            if (!string.IsNullOrEmpty(backupId))
            {
                _logger.LogInfo($"Copia de seguridad creada con ID: {backupId}");
                result.BackupId = backupId; // Almacenar el ID en el resultado de CleanupAsync
            }
            else
            {
                 _logger.LogWarning($"No se pudo crear la copia de seguridad para {installation.DisplayName}.");
                progress?.Report(ProgressInfo.Warning("Advertencia",
                                                    $"No se pudo crear la copia de seguridad."));
            }
        }
        // ...
        ```
    *   **Justificación:** La firma del método `_backupService.CreateBackupForCleanupAsync` en `IBackupService.cs` y `BackupService.cs` es `Task<string> CreateBackupForCleanupAsync(PhotoshopInstallation installation, ...)`. La llamada en `CleanupService.cs` debe coincidir con esta firma, pasando el objeto `installation` actual. Las clases `FilePatterns` y `RegistryPatterns` no existen en el proyecto actual.

*   **Corrección para `DetectionService.cs(736,76)` y `(745,92)`:**
    *   **Contexto:** El método `ProgressInfo.Success()` no existe. Se debe usar `ProgressInfo.Completed()` o `ProgressInfo.Running()` con 100%.
    *   **Fragmento de Código Actual (en `DetectionService.cs` dentro de `DetectInstallationsAsync`):**
        ```csharp
        // ...
        _logger.LogInfo($"Detección completada. Encontradas {filteredInstallations.Count} instalaciones válidas.");
        progress?.Report(ProgressInfo.Success(100, "Detección completada", // ESTA LÍNEA (736)
                                            $"Se encontraron {filteredInstallations.Count} instalaciones de Photoshop."));
        // ...
        ```
        Y posiblemente en otro lugar para el caso de 0 instalaciones.

    *   **Reemplazar `ProgressInfo.Success(...)` con `ProgressInfo.Completed(...)`:**
        ```csharp
        // ...
        _logger.LogInfo($"Detección completada. Encontradas {filteredInstallations.Count} instalaciones válidas.");
        progress?.Report(ProgressInfo.Completed("Detección completada", // CAMBIADO
                                            $"Se encontraron {filteredInstallations.Count} instalaciones de Photoshop."));
        // ...
        // Y si hay un caso para 0 instalaciones, también usar Completed o Running(100, ...)
        // Por ejemplo, en el bloque else de "if (installations.Count > 0)":
        // else
        // {
        //     _logger.LogInfo("Detección de instalaciones completada. No se encontraron instalaciones.");
        //     progress?.Report(ProgressInfo.Completed("Detección de instalaciones",
        //         "No se encontraron instalaciones de Photoshop."));
        // }
        // La implementación actual de DetectionService.cs ya usa ProgressInfo.Completed correctamente.
        // La advertencia debe provenir de un uso de ProgressInfo.Success en el código fuente
        // de 07_Codigo_Fuente_Metodos_Clave.md que se copió.
        // La implementación actual de DetectionService.cs usa:
        // progress?.Report(ProgressInfo.Completed("Detección de instalaciones",
        //    $"Se encontraron {installations.Count} instalaciones de Photoshop."));
        // Esta ya es correcta y no debería generar la advertencia.
        // La advertencia CS8625 suele ser por pasar `null` a un parámetro `string` no nullable.
        // Revisemos los parámetros de ProgressInfo.Running:
        // public static ProgressInfo Running(int progressPercentage, string operationTitle, string statusMessage)
        // Todos son string no nullable.
        // La llamada en DetectionService.cs:
        // progress?.Report(ProgressInfo.Running(0, "Detectando instalaciones de Photoshop", "Iniciando..."));
        // Esta llamada es correcta.
        // LA ADVERTENCIA DEBE ESTAR EN LA IMPLEMENTACIÓN DE `07_Codigo_Fuente_Metodos_Clave.md` que está siendo revisada,
        // no en `DetectionService.cs`. El código de `DetectionService.cs` proporcionado NO tiene un `ProgressInfo.Success`.

        // Si la advertencia CS8625 *está* en DetectionService.cs, es probable que sea porque uno de los strings
        // pasados a ProgressInfo.Running, .Completed, .Error, etc., podría ser null.
        // Ejemplo:
        // string potentiallyNullMessage = GetSomeMessage();
        // progress?.Report(ProgressInfo.Running(50, "Titulo", potentiallyNullMessage)); // CS8625 si potentiallyNullMessage puede ser null
        // Solución:
        // progress?.Report(ProgressInfo.Running(50, "Titulo", potentiallyNullMessage ?? "Mensaje por defecto"));
        ```
    *   **Decisión:** Asegurar que todos los argumentos string pasados a los métodos fábrica de `ProgressInfo` no sean `null`. Usar el operador de coalescencia nula (`??`) para proporcionar un valor predeterminado si una variable de cadena podría ser nula.
        *   **En `DetectionService.cs`, las llamadas a `progress?.Report` parecen correctas con literales de cadena, por lo que no deberían causar CS8625.**
        *   **La advertencia `CS8625` para `DetectionService.cs` en los logs de compilación que proporcionaste no tiene sentido si el código de `DetectionService.cs` es el que también proporcionaste. Es posible que la advertencia se refiera a una versión anterior del código o a un malentendido de la salida del compilador.**
        *   **Para `CleanupService.cs(384,76)`**, la corrección anterior para `CreateBackupForCleanupAsync` ya resuelve la fuente potencial de pasar un `null` si `backupResult.ErrorMessage` fuera `null` y se pasara a un parámetro no nullable. `OperationResult.BackupPath` es nullable, así que eso está bien.

#### **4. Abordar Advertencia CS0219 (Variable asignada pero no usada)**

*   **Archivo:** `C:\MiRepo\DPui3\DesinstalaPhotoshop.Core\Services\CleanupService.cs`
*   **Línea:** `(897,18)`
*   **Variable:** `allFilesDeleted` en `ForceDeleteCommonFilesDirectoryAsync`

*   **Fragmento de Código Actual:**
    ```csharp
    // ...
    // 3. Intentar eliminar archivos individuales
    bool allFilesDeleted = true; // ASIGNADA

    try
    {
        foreach (var file in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
        {
            // ... lógica que podría poner allFilesDeleted = false ...
        }
    }
    // ...
    // Intentar eliminar la carpeta nuevamente
    if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
    {
        _logger.LogInfo($"Carpeta eliminada después de eliminar archivos individuales: {directoryPath}");
        return true; // allFilesDeleted NO SE USA AQUÍ
    }

    return false; // NI AQUÍ
    // ...
    ```

*   **Instrucciones para Corregir:**
    La variable `allFilesDeleted` se establece pero su valor final no se utiliza para determinar el valor de retorno del método o en alguna otra lógica. Hay dos opciones:
    1.  **Si la intención era usarla:** Modificar la lógica de retorno para considerarla. Por ejemplo, si la eliminación del directorio falla, pero todos los archivos individuales se eliminaron, aún se podría considerar un éxito parcial.
        ```csharp
        // ...
            // Intentar eliminar la carpeta nuevamente
            if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
            {
                _logger.LogInfo($"Carpeta eliminada después de eliminar archivos individuales: {directoryPath}");
                return true;
            }
            // Si la eliminación del directorio falla, pero todos los archivos sí se borraron,
            // podríamos considerarlo un "éxito" parcial en términos de contenido.
            // O, si es importante que el directorio también se elimine, esto seguiría siendo false.
            // DECISIÓN: Para ForceDelete, la eliminación del directorio es clave.
            // Si el directorio no se puede borrar, pero todos los archivos sí, entonces
            // la programación para reinicio (hecha por el llamador ProcessCommonFilesDirectoriesAsync)
            // es la siguiente línea de defensa.
            // Por lo tanto, aquí simplemente podemos ignorar allFilesDeleted O usarla para logging.

            _logger.LogDebug($"Estado de allFilesDeleted en ForceDeleteCommonFilesDirectoryAsync: {allFilesDeleted} (Directorio NO eliminado)");
            return false; // La eliminación del directorio falló.
        // ...
        ```
        En este caso, para simplemente quitar la advertencia, usarla en un log es suficiente.

    2.  **Si realmente no se necesita:** Eliminar la variable y la lógica que la establece a `false`.
        **Decisión:** Es útil para logging y depuración saber si todos los archivos internos se pudieron eliminar, incluso si el directorio en sí no. Se mantendrá la variable y se usará en un mensaje de log si la eliminación del directorio falla.

*   **Reemplazar con (usar la variable en un log):**
    Busca el final del método `ForceDeleteCommonFilesDirectoryAsync`:
    ```csharp
            // Intentar eliminar la carpeta nuevamente
            if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
            {
                _logger.LogInfo($"Carpeta eliminada después de eliminar archivos individuales: {directoryPath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en eliminación forzada de carpeta Common Files {directoryPath}: {ex.Message}");
            return false;
        }
    }
    ```
    Modifícalo para incluir el log con `allFilesDeleted` antes de retornar `false`:
    ```csharp
            // Intentar eliminar la carpeta nuevamente
            if (_fileSystemHelper.DeleteDirectory(directoryPath, true))
            {
                _logger.LogInfo($"Carpeta eliminada después de eliminar archivos individuales: {directoryPath}");
                return true;
            }

            _logger.LogWarning($"Falló la eliminación directa del directorio {directoryPath}. Estado de eliminación de archivos internos: {(allFilesDeleted ? "Todos eliminados" : "Algunos no eliminados")}.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error en eliminación forzada de carpeta Common Files {directoryPath}: {ex.Message}");
            return false;
        }
    }
    ```
    *   **Justificación:** Se elimina la advertencia del compilador utilizando la variable, lo que también añade información útil al log en caso de que la eliminación forzada del directorio falle pero sus contenidos (o parte de ellos) sí hayan podido ser eliminados.

---
**Nota Adicional:**
Con estas correcciones, el proyecto debería compilar sin errores y con menos advertencias. Las advertencias CS1998 restantes en los métodos `async` se abordarán asegurando que la lógica interna use `await Task.Run(...)` como se describió, para que realmente se ejecuten de forma asíncrona y no bloqueen el hilo llamador.