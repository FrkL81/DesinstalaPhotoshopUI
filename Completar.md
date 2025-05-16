## Instrucciones Detalladas para Completar Fase 1.4: Actualizar `RestoreBackupForm.cs`

**Objetivo:** Modificar `RestoreBackupForm.cs` para que todos los mensajes mostrados al usuario (errores, advertencias, confirmaciones, información) utilicen la librería `CustomMsgBoxLibrary.dll` (`CustomMsgBox.Show()`) en lugar de `System.Windows.Forms.MessageBox.Show()`. Esto asegurará una experiencia de usuario visualmente coherente con el resto de la aplicación.

**Archivo a Modificar:**
*   `DesinstalaPhotoshop.UI/RestoreBackupForm.cs`

**Documentación de Referencia:**
*   `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md` (para la API de `CustomMsgBox.Show()` y sus parámetros).
*   `ManualDesarrollo/03_GUI_Descripcion_Visual.md` (para el tema oscuro y la estética general).

---

**Pasos Detallados para la Actualización:**

### 1. Revisión del Código Existente en `RestoreBackupForm.cs`

Identifica todas las instancias donde se utiliza `MessageBox.Show()`. Según el código proporcionado anteriormente, los puntos principales son:

*   En el constructor, si `!IsUserAdministrator()` (Este bloque se eliminó en la corrección anterior, pero si por alguna razón se reintroduce o existe una lógica similar, también debería usar `CustomMsgBox`).
*   En `LoadBackups()` dentro del `catch (Exception ex)`.
*   En `BtnRestore_Click()`:
    *   Si `string.IsNullOrEmpty(SelectedBackupPath)`.
    *   Para la confirmación de restauración.
    *   Dentro del `catch (Exception ex)` para errores de restauración.
    *   Para mostrar el mensaje de "Restauración Parcial".
    *   Para mostrar el mensaje de "Restauración Completada".
    *   Si no se encuentra `manifest.json`.

### 2. Asegurar las Directivas `using`

Verifica que al principio del archivo `RestoreBackupForm.cs` estén presentes las siguientes directivas `using`:

```csharp
using CustomMsgBoxLibrary;
using CustomMsgBoxLibrary.Types; // Esencial para CustomDialogResult, CustomMessageBoxButtons, CustomMessageBoxIcon
```

Si no están, añádelas.

### 3. Modificación de las Llamadas a `MessageBox.Show()`

Reemplaza cada llamada a `MessageBox.Show()` con la llamada equivalente a `CustomMsgBox.Show()`. Presta atención a los siguientes parámetros:

*   `prompt`: El mensaje principal a mostrar.
*   `title`: El título de la ventana del diálogo.
*   `buttons`: El conjunto de botones (ej. `CustomMessageBoxButtons.OK`, `CustomMessageBoxButtons.YesNo`).
*   `icon`: El icono a mostrar (ej. `CustomMessageBoxIcon.Error`, `CustomMessageBoxIcon.Warning`, `CustomMessageBoxIcon.Question`, `CustomMessageBoxIcon.Information`).
*   `theme`: El tema visual. Debería ser `ThemeSettings.DarkTheme` para mantener la coherencia con el resto de la aplicación.

#### 3.1. Actualización en el Constructor (si aplica y si no se eliminó el chequeo de admin aquí)

Si el chequeo de administrador permanece en el constructor de `RestoreBackupForm` (aunque se recomendó moverlo a `MainForm`), actualízalo así:

*   **Antes:**
    ```csharp
    // if (!IsUserAdministrator())
    // {
    //     MessageBox.Show( // ... );
    //     this.Close();
    //     return;
    // }
    ```
*   **Después (si el chequeo se mantiene aquí):**
    ```csharp
    // if (!IsUserAdministrator())
    // {
    //     CustomMsgBox.Show(
    //         prompt: "Se requieren privilegios de administrador para restaurar copias de seguridad.",
    //         title: "Permisos insuficientes",
    //         buttons: CustomMessageBoxButtons.OK,
    //         icon: CustomMessageBoxIcon.Warning,
    //         theme: ThemeSettings.DarkTheme
    //     );
    //     this.Close();
    //     return;
    // }
    ```
    *Nota: La corrección anterior de la `ObjectDisposedException` implicaba eliminar este bloque del constructor de `RestoreBackupForm`. Si se siguió esa recomendación, este punto no aplica.*

#### 3.2. Actualización en `LoadBackups()`

*   **Ubicación:** Dentro del bloque `catch (Exception ex)` del método `LoadBackups`.
*   **Antes:**
    ```csharp
    // catch (Exception ex)
    // {
    //     MessageBox.Show(
    //         $"Error al cargar los backups: {ex.Message}",
    //         "Error",
    //         MessageBoxButtons.OK,
    //         MessageBoxIcon.Error
    //     );
    // }
    ```
*   **Después:**
    ```csharp
    catch (Exception ex)
    {
        CustomMsgBox.Show(
            prompt: $"Error al cargar los backups: {ex.Message}",
            title: "Error de Carga", // Título más específico
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error,
            theme: ThemeSettings.DarkTheme
        );
    }
    ```

#### 3.3. Actualización en `BtnRestore_Click()`

Hay varias instancias dentro de este método.

*   **Advertencia si no hay backup seleccionado:**
    *   **Antes:**
        ```csharp
        // if (string.IsNullOrEmpty(SelectedBackupPath))
        // {
        //     MessageBox.Show(
        //         "Por favor, seleccione un backup para restaurar.",
        //         "Aviso",
        //         MessageBoxButtons.OK,
        //         MessageBoxIcon.Warning
        //     );
        //     return;
        // }
        ```
    *   **Después:**
        ```csharp
        if (string.IsNullOrEmpty(SelectedBackupPath))
        {
            CustomMsgBox.Show(
                prompt: "Por favor, seleccione una copia de seguridad para restaurar.",
                title: "Selección Requerida", // Título más específico
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Warning,
                theme: ThemeSettings.DarkTheme
            );
            return;
        }
        ```
        *Nota: El código que proporcionaste ya usa `CustomMsgBox` para este caso. Solo asegúrate de que `theme` esté especificado si quieres forzar el oscuro o confía en el default de la librería si ya está configurado globalmente.*

*   **Confirmación de restauración (ya usa `CustomMsgBox`):**
    El código actual ya utiliza `CustomMsgBox` para la confirmación. Asegúrate de que se especifica el `theme: ThemeSettings.DarkTheme` si es necesario.
    ```csharp
    // var result = CustomMsgBox.Show(
    //     prompt: $"¿Está seguro que desea restaurar la siguiente copia de seguridad?\n\n" +
    //             // ...
    //             "Esta acción revertirá los archivos a su estado anterior.",
    //     title: "Confirmar Restauración",
    //     buttons: CustomMessageBoxButtons.YesNo,
    //     icon: CustomMessageBoxIcon.Question,
    //     theme: ThemeSettings.DarkTheme // Asegurar que el tema está aquí o es el default
    // );
    ```

*   **Manejo de error general en restauración:**
    *   **Ubicación:** En el bloque `catch (Exception ex)` principal de `BtnRestore_Click`.
    *   **Antes:**
        ```csharp
        // catch (Exception ex)
        // {
        //     MessageBox.Show(
        //         $"Error al restaurar la copia de seguridad: {ex.Message}",
        //         "Error",
        //         MessageBoxButtons.OK,
        //         MessageBoxIcon.Error
        //     );
        // }
        ```
    *   **Después:**
        ```csharp
        catch (Exception ex)
        {
            CustomMsgBox.Show(
                prompt: $"Error al restaurar la copia de seguridad: {ex.Message}",
                title: "Error de Restauración", // Título más específico
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Error,
                theme: ThemeSettings.DarkTheme
            );
        }
        ```

*   **Mensaje de "Restauración Parcial" (ya usa `CustomMsgBox`):**
    El código actual ya utiliza `CustomMsgBox`. Verificar que el `theme` esté presente.
    ```csharp
    // CustomMsgBox.Show(
    //     prompt: errorMessage,
    //     title: "Restauración Parcial",
    //     buttons: CustomMessageBoxButtons.OK,
    //     icon: CustomMessageBoxIcon.Warning,
    //     theme: ThemeSettings.DarkTheme // Asegurar tema
    // );
    ```

*   **Mensaje de "Restauración Completada" (ya usa `CustomMsgBox`):**
    El código actual ya utiliza `CustomMsgBox`. Verificar que el `theme` esté presente.
    ```csharp
    // CustomMsgBox.Show(
    //     prompt: "La copia de seguridad se ha restaurado correctamente.",
    //     title: "Restauración Completada",
    //     buttons: CustomMessageBoxButtons.OK,
    //     icon: CustomMessageBoxIcon.Information, // Cambiado de Success a Information si es más apropiado, o mantener Success.
    //     theme: ThemeSettings.DarkTheme // Asegurar tema
    // );
    ```

*   **Mensaje si `manifest.json` no se encuentra (implícito, puede ser manejado por el `catch` general, pero si se quiere un mensaje específico):**
    Si hay un `throw new FileNotFoundException("No se encontró el archivo manifest.json en el backup.");`, el `catch` general lo manejará. Si se quiere un mensaje más específico *antes* de que el `catch` general actúe, se podría hacer:

    ```csharp
    // if (!File.Exists(manifestPath))
    // {
    //     CustomMsgBox.Show(
    //         prompt: "No se encontró el archivo de manifiesto (manifest.json) en la copia de seguridad seleccionada. La restauración no puede continuar.",
    //         title: "Error de Backup",
    //         buttons: CustomMessageBoxButtons.OK,
    //         icon: CustomMessageBoxIcon.Error,
    //         theme: ThemeSettings.DarkTheme
    //     );
    //     // ... lógica para abortar restauración ...
    //     return; // o manejarlo para que el finally se ejecute apropiadamente
    // }
    ```
    *Nota: En el código actual, esta condición se maneja dentro de la lógica de `BtnRestore_Click`, donde un `FileNotFoundException` es lanzado si `manifest.json` no existe, y el bloque `catch` general de `BtnRestore_Click` lo manejaría. La implementación actual con el `throw` y el `catch` general es aceptable.*

### 4. Revisión de Parámetros de `CustomMsgBox.Show()`

Para cada llamada a `CustomMsgBox.Show()`, asegúrate de que:
*   Los **títulos** sean descriptivos (ej. "Error de Carga", "Selección Requerida", "Error de Restauración").
*   Los **iconos** sean apropiados para el tipo de mensaje (`Error`, `Warning`, `Information`, `Question`).
*   El `theme` sea `ThemeSettings.DarkTheme` para consistencia, a menos que haya una razón específica para usar otro.
*   Los `buttons` sean los adecuados para la interacción requerida.

### 5. Verificación y Pruebas

1.  **Recompilar la Solución:** Asegurarse de que el proyecto `DesinstalaPhotoshop.UI` se compile sin errores.
2.  **Probar cada escenario donde se muestra un mensaje en `RestoreBackupForm`:**
    *   **Error al cargar backups:** Simula una excepción en `LoadBackups` (ej. corrompiendo temporalmente un archivo de backup o quitando permisos a la carpeta de backups) para verificar que el `CustomMsgBox` de error se muestra correctamente.
    *   **No seleccionar backup y presionar "Restaurar":** Verifica que el `CustomMsgBox` de advertencia "Selección Requerida" aparece.
    *   **Confirmación de restauración:** Verifica que el diálogo de confirmación se muestra con el tema oscuro y la información correcta.
    *   **Simular error durante la restauración:** (Esto puede ser más difícil de simular sin backups reales y escenarios de fallo). Verifica que, si ocurre una excepción durante el proceso de restauración, el `CustomMsgBox` de error general se muestre.
    *   **Simular restauración parcial:** Si tienes un backup que se pueda corromper parcialmente para que algunos ítems fallen, verifica el mensaje de "Restauración Parcial".
    *   **Restauración exitosa:** Si tienes un backup funcional, verifica el mensaje de "Restauración Completada".
3.  **Consistencia Visual:** Asegúrate de que todos los diálogos generados por `RestoreBackupForm` tengan la misma apariencia (tema oscuro, estilo de botones, etc.) que los diálogos en `MainForm`.

---

Al seguir estos pasos, `RestoreBackupForm.cs` quedará completamente alineado con el uso de `CustomMsgBoxLibrary`, completando así la tarea pendiente de la Fase 1.4.