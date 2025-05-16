La excepción `System.ObjectDisposedException` con el mensaje "Cannot access a disposed object. Object name: 'DesinstalaPhotoshop.UI.RestoreBackupForm'." indica que se está intentando acceder a un objeto `RestoreBackupForm` que ya ha sido liberado de la memoria (normalmente después de llamar a su método `Dispose()`, que a su vez es llamado por `Close()` o al salir de un bloque `using`).

El stack trace apunta directamente al problema:
`at System.Windows.Forms.Form.ShowDialog(IWin32Window owner)`
`at DesinstalaPhotoshop.UI.MainForm.BtnRestore_Click(Object sender, EventArgs e) in C:\MiRepo\DPui3\DesinstalaPhotoshop.UI\MainForm.cs:line 642`

Esto significa que en el método `BtnRestore_Click` de `MainForm.cs`, cuando se llama a `restoreForm.ShowDialog(this)`, el objeto `restoreForm` ya ha sido "disposed".

Aquí están las instrucciones detalladas para remediar esta excepción:

---

## Instrucciones para Remediar `System.ObjectDisposedException` en `RestoreBackupForm`

**Objetivo:** Corregir el error que causa que se intente acceder a `RestoreBackupForm` después de que ha sido dispuesto, lo que resulta en una `ObjectDisposedException`.

**Causa Raíz Probable:**
El formulario `RestoreBackupForm` está siendo dispuesto prematuramente, posiblemente dentro de su propio código (por ejemplo, en su constructor o en un evento temprano si falla una condición) antes de que `MainForm` intente mostrarlo con `ShowDialog()`.

**Archivos a Revisar y Modificar:**
1.  `DesinstalaPhotoshop.UI/RestoreBackupForm.cs`
2.  `DesinstalaPhotoshop.UI/MainForm.cs` (específicamente el método `BtnRestore_Click`)

**Pasos Detallados para la Corrección:**

### 1. Análisis de `RestoreBackupForm.cs`

*   **Acción:** Revisar el constructor de `RestoreBackupForm.cs`.
    *   **Lógica Actual (identificada en el código proporcionado):**
        ```csharp
        public RestoreBackupForm()
        {
            InitializeComponent();
            
            if (!IsUserAdministrator()) // <--- PUNTO CRÍTICO
            {
                CustomMsgBox.Show(
                    prompt: "Se requieren privilegios de administrador para restaurar copias de seguridad.",
                    title: "Permisos insuficientes",
                    // ...
                );
                this.Close(); // <--- ESTO LLAMA A DISPOSE()
                return;       // El constructor termina aquí si no es admin.
            }
            
            SetupForm();
            LoadBackups();
        }
        ```
    *   **Problema:** Si `IsUserAdministrator()` devuelve `false`, el formulario llama a `this.Close()`. En Windows Forms, `Form.Close()` llama implícitamente a `Form.Dispose()`. Cuando `BtnRestore_Click` en `MainForm` intenta llamar a `restoreForm.ShowDialog(this)` sobre esta instancia ya dispuesta, ocurre la excepción.

### 2. Modificación Sugerida en `MainForm.cs` (Método `BtnRestore_Click`)

*   **Acción:** Modificar `MainForm.BtnRestore_Click` para manejar la posibilidad de que `RestoreBackupForm` no se pueda mostrar o se cierre prematuramente.
*   **Lógica Actual (extracto relevante):**
    ```csharp
    private async void BtnRestore_Click(object sender, EventArgs e)
    {
         _loggingService.LogInfo("Iniciando proceso de restauración de backup...");
        using (var restoreForm = new RestoreBackupForm()) // <--- 'using' asegura Dispose() al salir del bloque
        {
            if (restoreForm.ShowDialog(this) == DialogResult.OK) // <--- EXCEPCIÓN AQUÍ si restoreForm fue dispuesto en su constructor
            {
                // ... lógica de restauración ...
            }
            // ...
        }
    }
    ```
*   **Problema con la lógica actual:** El patrón `using` es correcto para asegurar que el formulario se disponga *después* de ser usado. Sin embargo, el problema ocurre *antes* de que `ShowDialog` pueda siquiera ejecutarse si `RestoreBackupForm` se cierra en su constructor.

*   **Solución Propuesta:**
    1.  **Mover la verificación de administrador a `MainForm` *antes* de crear `RestoreBackupForm`**. Esto es más consistente con cómo se manejan los permisos en otros botones (como `BtnCleanup_Click` y `BtnUninstall_Click`).
    2.  Si `RestoreBackupForm` *debe* mantener su propia lógica de cierre (lo cual es menos ideal para este escenario), entonces `MainForm` necesita verificar si el formulario fue dispuesto antes de llamar a `ShowDialog`. Sin embargo, la primera opción es más limpia.

*   **Implementación de la Solución Propuesta (Opción 1 - Preferida):**

    **Archivo: `DesinstalaPhotoshop.UI/MainForm.cs`**

    Modificar el método `BtnRestore_Click`:

    ```csharp
    private async void BtnRestore_Click(object sender, EventArgs e)
    {
        _loggingService.LogInfo("Iniciando proceso de restauración de backup...");

        // 1. Verificar permisos ANTES de crear el formulario RestoreBackupForm
        if (!_isCurrentlyAdmin) // Usar el campo _isCurrentlyAdmin que ya existe y se inicializa en el constructor de MainForm
        {
            _loggingService.LogWarning("La restauración de backups requiere privilegios de administrador.");
            CustomMsgBox.Show(
                prompt: "La restauración de copias de seguridad requiere privilegios de administrador. Por favor, reinicie la aplicación como administrador.",
                title: "Privilegios Insuficientes",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Warning,
                theme: ThemeSettings.DarkTheme);
            
            // Opcional: ofrecer reiniciar como admin si la lógica de RequestElevation está disponible y es adecuada aquí
            // if (RequestElevation("--elevated")) return; // Esto cerraría la app actual

            return; // No continuar si no es admin
        }

        // Ahora que sabemos que somos admin (o estamos en modo desarrollo sin el chequeo), podemos proceder.
        // El bloque using sigue siendo bueno para asegurar que se disponga después de su uso.
        using (var restoreForm = new RestoreBackupForm()) 
        {
            // Dado que movimos el chequeo de admin a MainForm, RestoreBackupForm no debería cerrarse a sí mismo en el constructor
            // por falta de permisos. Si aún existe ese chequeo en RestoreBackupForm, debe eliminarse o modificarse.

            if (restoreForm.IsDisposed) // Añadir una verificación por si acaso se dispone por otra razón
            {
                _loggingService.LogError("RestoreBackupForm fue dispuesto inesperadamente antes de ShowDialog.");
                CustomMsgBox.Show(
                    prompt: "No se pudo abrir el diálogo de restauración porque ya fue cerrado.",
                    title: "Error de Diálogo",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Error,
                    theme: ThemeSettings.DarkTheme);
                return;
            }

            if (restoreForm.ShowDialog(this) == DialogResult.OK)
            {
                string backupPathToRestore = restoreForm.SelectedBackupPath; // Cambiado para usar la propiedad pública como está
                if (string.IsNullOrEmpty(backupPathToRestore))
                {
                    _loggingService.LogError("No se seleccionó una ruta de backup válida para restaurar.");
                     CustomMsgBox.Show(
                        prompt: "No se seleccionó una copia de seguridad válida para restaurar.",
                        title: "Selección Requerida",
                        buttons: CustomMessageBoxButtons.OK,
                        icon: CustomMessageBoxIcon.Warning,
                        theme: ThemeSettings.DarkTheme);
                    return;
                }
                
                _loggingService.LogInfo($"Ruta de backup seleccionada para restaurar: {backupPathToRestore}");

                // El backupId para el servicio BackupService es el nombre de la carpeta.
                string backupId = Path.GetFileName(backupPathToRestore); 

                var restoreOpResult = await RunOperationAsync(
                    (progress, token) => _backupService.RestoreBackupAsync(backupId, progress, token),
                    $"Restaurando Backup {backupId}",
                    requiresElevation: true // La operación de restauración en sí misma también puede requerir elevación.
                );

                 if (restoreOpResult != null)
                {
                    if (restoreOpResult.Success)
                    {
                        _loggingService.LogInfo($"Restauración del backup '{backupId}' completada: {restoreOpResult.Message}");
                        // _loggingService.LogInfo("Refrescando lista de instalaciones después de la restauración..."); // Ya se hace en UpdateButtonsState
                        // await Task.Delay(1000); 
                        // BtnDetect_Click(sender, e); // No llamar recursivamente, sino actualizar estado.
                        await TriggerDetectionProcess(); // Usar el método de detección dedicado
                    }
                    else
                    {
                        _loggingService.LogError($"Error durante la restauración del backup '{backupId}': {restoreOpResult.ErrorMessage} - {restoreOpResult.Message}");
                    }
                }
                 else
                {
                     _loggingService.LogWarning($"La operación de restauración para el backup '{backupId}' no devolvió un resultado (posiblemente cancelada o error previo).");
                 }
            }
            else
            {
                _loggingService.LogInfo("Restauración de backup cancelada por el usuario desde el diálogo de selección.");
            }
        }
        UpdateButtonsState(); // Asegurar que los botones se actualicen después de cerrar el diálogo.
    }
    ```

### 3. Modificación Sugerida en `RestoreBackupForm.cs`

*   **Acción:** Eliminar la lógica de verificación de administrador y cierre automático del constructor de `RestoreBackupForm`.
*   **Implementación:**

    **Archivo: `DesinstalaPhotoshop.UI/RestoreBackupForm.cs`**

    Modificar el constructor:
    ```csharp
    public RestoreBackupForm()
    {
        InitializeComponent();
        
        // ELIMINAR O COMENTAR ESTE BLOQUE:
        /* 
        if (!IsUserAdministrator()) 
        {
            CustomMsgBox.Show(
                prompt: "Se requieren privilegios de administrador para restaurar copias de seguridad.",
                title: "Permisos insuficientes",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Warning,
                theme: ThemeSettings.DarkTheme
            );
            this.Close(); // Esto causa la ObjectDisposedException
            return;
        }
        */
        
        SetupForm();
        LoadBackups();
    }

    // También eliminar el método IsUserAdministrator() de esta clase si ya no se usa en ningún otro lugar aquí.
    /*
    private bool IsUserAdministrator()
    {
        // ...
    }
    */
    ```

**Justificación de la Solución Propuesta:**

*   **Centralización de la Lógica de Permisos:** La responsabilidad de verificar los permisos antes de iniciar una operación (como abrir un diálogo que requiere permisos) recae de forma más natural en el formulario que invoca la acción (`MainForm`). Esto sigue el patrón ya establecido en `BtnCleanup_Click` y `BtnUninstall_Click`.
*   **Ciclo de Vida del Formulario:** Permite que `RestoreBackupForm` se construya completamente sin cerrarse a sí mismo. Luego, `MainForm` puede mostrarlo con `ShowDialog()`. El bloque `using` en `MainForm` se encargará de llamar a `Dispose()` en `restoreForm` cuando el diálogo se cierre (ya sea por OK, Cancelar, o la 'X' de la ventana), que es el comportamiento esperado.
*   **Evita la Excepción:** Al no llamar a `this.Close()` (y por ende `Dispose()`) dentro del constructor de `RestoreBackupForm` por falta de permisos, el objeto no estará dispuesto cuando `ShowDialog()` sea invocado.
*   **Robustez Adicional:** La verificación `if (restoreForm.IsDisposed)` en `MainForm` antes de `ShowDialog` es una salvaguarda adicional por si el formulario se dispone por alguna otra razón inesperada antes de ser mostrado.

### 4. Verificación y Pruebas

1.  **Recompilar la Solución:** Asegurarse de que todos los cambios se compilen sin errores.
2.  **Probar sin Privilegios de Administrador:**
    *   Ejecutar la aplicación (asegurándose de que se inicie sin privilegios de administrador; si el `app.manifest` está en `requireAdministrator`, cambiarlo temporalmente a `asInvoker` para esta prueba y luego revertir).
    *   Hacer clic en el botón "Restaurar".
    *   **Comportamiento Esperado:** `MainForm` debería mostrar el `CustomMsgBox` indicando "Privilegios Insuficientes" y el diálogo `RestoreBackupForm` NO debería intentar abrirse ni causar una excepción.
3.  **Probar con Privilegios de Administrador:**
    *   Ejecutar la aplicación con privilegios de administrador.
    *   Hacer clic en el botón "Restaurar".
    *   **Comportamiento Esperado:** El diálogo `RestoreBackupForm` debería abrirse correctamente, listando los backups (si existen).
    *   Probar a seleccionar un backup y hacer clic en "Restaurar" (si hay backups) y en "Cancelar" para asegurar que el flujo post-diálogo en `MainForm` funciona.
4.  **Revisar Logs:** Verificar que los mensajes de log en `_loggingService` reflejen correctamente el flujo, especialmente los mensajes de advertencia o error si se intentó la operación sin permisos.

---

Siguiendo estos pasos, la `ObjectDisposedException` debería resolverse, y el manejo de permisos para la funcionalidad de restauración será más consistente con el resto de la aplicación.