# Funcionalidad de Controles de la Interfaz

Este documento detalla los controles interactivos presentes en la interfaz gráfica de DesinstalaPhotoshop, incluyendo su nombre en código, tipo, ubicación, función específica, interacción esperada y eventos asociados.

## Controles Principales

### Botones de Acción

#### 1. Botón Detectar Instalaciones

- **Nombre en código**: `btnDetect`
- **Tipo**: `Button`
- **Ubicación**: Panel superior (panelTop)
- **Función específica**: Inicia el proceso de detección de instalaciones de Adobe Photoshop en el sistema
- **Interacción esperada**: Al hacer clic, limpia la lista de instalaciones y ejecuta el servicio de detección
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnDetect_Click` que llama a `DetectionService.DetectInstallationsAsync`
- **Estado condicional**: Siempre habilitado, excepto durante operaciones en curso

```csharp
private void BtnDetect_Click(object sender, EventArgs e)
{
    _ = RunOperationAsync(async (progress, token) =>
    {
        // Limpiar lista de instalaciones
        lstInstallations.Items.Clear();
        _detectedInstallations.Clear();

        // Detectar instalaciones
        _detectedInstallations = await _detectionService.DetectInstallationsAsync(progress, token);

        // Mostrar instalaciones y actualizar estado de botones
        // ...
    });
}
```

#### 2. Botón Desinstalar

- **Nombre en código**: `btnUninstall`
- **Tipo**: `Button`
- **Ubicación**: Panel superior (panelTop)
- **Función específica**: Desinstala la instalación de Photoshop seleccionada (solo opera sobre la instalación única actualmente seleccionada)
- **Interacción esperada**:
  - Verifica privilegios de administrador
  - Solicita confirmación del usuario
  - Muestra opciones de desinstalación
  - Ejecuta el proceso de desinstalación
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnUninstall_Click` que llama a `UninstallService.UninstallAsync`
- **Estado condicional**: Habilitado solo cuando hay una instalación principal o posible instalación principal detectada

```csharp
private void BtnUninstall_Click(object sender, EventArgs e)
{
    // Verificar privilegios de administrador
    if (!AdminHelper.IsRunningAsAdmin())
    {
        // Solicitar elevación de privilegios
        // ...
        return;
    }

    // Verificar si hay una instalación seleccionada
    if (lstInstallations.SelectedItems.Count == 0)
    {
        // Mostrar mensaje de selección requerida
        // ...
        return;
    }

    // Confirmar desinstalación y mostrar opciones
    // ...

    // Ejecutar desinstalación
    _ = RunOperationAsync(async (progress, token) =>
    {
        return await _uninstallService.UninstallAsync(
            installation,
            createBackup,
            cleanupAfterUninstall,
            false, // No es modo de prueba
            progress,
            token);
    });
}
```

#### 3. Botón Limpiar Residuos

- **Nombre en código**: `btnCleanup`
- **Tipo**: `Button`
- **Ubicación**: Panel superior (panelTop)
- **Función específica**: Elimina archivos y entradas de registro residuales de Photoshop
- **Interacción esperada**:
  - Verifica privilegios de administrador
  - Solicita confirmación del usuario
  - Muestra opciones de limpieza
  - Ejecuta el proceso de limpieza
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnCleanup_Click` que llama a `CleanupService.CleanupAsync`
- **Estado condicional**: Habilitado solo cuando no hay instalaciones principales pero sí hay residuos detectados

```csharp
private void BtnCleanup_Click(object sender, EventArgs e)
{
    // Verificar privilegios de administrador
    if (!AdminHelper.IsRunningAsAdmin())
    {
        // Solicitar elevación de privilegios
        // ...
        return;
    }

    // Confirmar limpieza
    // Asegurarse de tener: using CustomMsgBoxLibrary; using CustomMsgBoxLibrary.Types;
    CustomDialogResult cleanupConfirmation = CustomMsgBox.Show(
        prompt: "Está a punto de eliminar archivos y entradas de registro residuales de Adobe Photoshop.\n\n" +
                "¿Está seguro de que desea continuar?",
        title: "Confirmación de limpieza",
        buttons: CustomMessageBoxButtons.YesNo, // Usar el enum de CustomMsgBox
        icon: CustomMessageBoxIcon.Warning,     // Usar el enum de CustomMsgBox
        theme: ThemeSettings.DarkTheme // O el tema por defecto de la app
    );

    if (cleanupConfirmation != CustomDialogResult.Yes) // Usar el enum de CustomMsgBox
    {
        return;
    }

    // Mostrar opciones y ejecutar limpieza
    // ...
}
```

#### 4. Botón Modo de Prueba

- **Nombre en código**: `btnTestMode`
- **Tipo**: `Button`
- **Ubicación**: Panel superior (panelTop)
- **Función específica**: Permite simular operaciones sin realizar cambios reales en el sistema
- **Interacción esperada**:
  - Muestra un formulario de opciones de modo de prueba
  - Ejecuta la operación seleccionada en modo simulación
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnTestMode_Click` que muestra `TestModeOptionsForm`
- **Estado condicional**: Siempre habilitado, excepto durante operaciones en curso

```csharp
private void BtnTestMode_Click(object sender, EventArgs e)
{
    // Mostrar opciones de modo de prueba
    var optionsForm = new TestModeOptionsForm();
    if (optionsForm.ShowDialog() != DialogResult.OK)
    {
        return; // Cancelado por el usuario
    }

    TestModeOperation operation = optionsForm.SelectedOperation;

    switch (operation)
    {
        case TestModeOperation.DetectOnly:
            BtnDetect_Click(sender, e);
            break;
        case TestModeOperation.SimulateUninstall:
            // Simular desinstalación
            // ...
            break;
        case TestModeOperation.SimulateCleanup:
            // Simular limpieza
            // ...
            break;
    }
}
```

#### 5. Botón Cancelar

- **Nombre en código**: `btnCancel`
- **Tipo**: `Button`
- **Ubicación**: Panel superior (panelTop)
- **Función específica**: Cancela la operación en curso
- **Interacción esperada**: Detiene inmediatamente la operación actual
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnCancel_Click` que cancela el token de cancelación
- **Estado condicional**: Habilitado solo durante operaciones en curso
- **Estilo visual**: Texto en color blanco para asegurar visibilidad (`ForeColor = Color.White`)

```csharp
private void BtnCancel_Click(object sender, EventArgs e)
{
    try
    {
        _cts?.Cancel();
        LogWarning("Operación cancelada por el usuario.");
    }
    catch (Exception ex)
    {
        LogError($"Error al cancelar la operación: {ex.Message}");
    }
}
```

#### 6. Botón Restaurar

- **Nombre en código**: `btnRestore`
- **Tipo**: `Button`
- **Ubicación**: Panel superior (panelTop)
- **Función específica**: Permite restaurar copias de seguridad creadas durante operaciones anteriores
- **Interacción esperada**:
  - Muestra un formulario con las copias de seguridad disponibles
  - Permite seleccionar y restaurar una copia de seguridad
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnRestore_Click` que llama a `BackupService.RestoreBackupAsync`
- **Estado condicional**: Habilitado cuando hay copias de seguridad disponibles

```csharp
private void BtnRestore_Click(object sender, EventArgs e)
{
    try
    {
        // Obtener copias de seguridad disponibles
        var backups = _backupService.GetAvailableBackups();

        if (backups.Count == 0)
        {
            CustomMsgBox.Show(
                prompt: "No se encontraron copias de seguridad disponibles.",
                title: "Información",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Information);
            return;
        }

        // Mostrar formulario de selección y restaurar
        // ...
    }
    catch (Exception ex)
    {
        CustomMsgBox.Show(
            prompt: $"Error al restaurar copia de seguridad: {ex.Message}",
            title: "Error",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error);
    }
}
```

#### 7. Botón Copiar Salida

- **Nombre en código**: `btnCopyOutput`
- **Tipo**: `Button`
- **Ubicación**: Panel de botones de consola (panelConsoleButtons)
- **Función específica**: Copia el contenido de la consola al portapapeles
- **Interacción esperada**: Copia todo el texto de la consola y muestra un mensaje de confirmación
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnCopyOutput_Click` que llama a `Clipboard.SetText`
- **Estado condicional**: Siempre habilitado

```csharp
private void BtnCopyOutput_Click(object sender, EventArgs e)
{
    try
    {
        if (!string.IsNullOrEmpty(txtConsole.Text))
        {
            Clipboard.SetText(txtConsole.Text);
            CustomMsgBox.Show(
                prompt: "El contenido de la consola ha sido copiado al portapapeles.",
                title: "Información",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Information);
        }
    }
    catch (Exception ex)
    {
        CustomMsgBox.Show(
            prompt: $"Error al copiar al portapapeles: {ex.Message}",
            title: "Error",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error);
    }
}
```

#### 8. Botón Abrir Log

- **Nombre en código**: `btnAbrirLog`
- **Tipo**: `Button`
- **Ubicación**: Panel de botones de consola (panelConsoleButtons)
- **Función específica**: Abre la carpeta que contiene los archivos de log
- **Interacción esperada**: Abre el explorador de Windows en la ubicación de los archivos de log
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnAbrirLog_Click` que abre la carpeta de logs
- **Estado condicional**: Siempre habilitado

```csharp
private void BtnAbrirLog_Click(object sender, EventArgs e)
{
    try
    {
        // Obtener la ruta del archivo de log
        string logFilePath = _loggingService.GetLogFilePath();
        string logDirectory = Path.GetDirectoryName(logFilePath) ?? string.Empty;

        if (!string.IsNullOrEmpty(logDirectory))
        {
            // Crear la carpeta si no existe
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Abrir la carpeta en el explorador
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = logDirectory,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
    catch (Exception ex)
    {
        CustomMsgBox.Show(
            prompt: $"Error al abrir la carpeta de logs: {ex.Message}",
            title: "Error",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error);
    }
}
```

#### 9. Botón Generar Script

- **Nombre en código**: `btnGenerarScript`
- **Tipo**: `Button`
- **Ubicación**: Panel de botones de consola (panelConsoleButtons)
- **Función específica**: Genera scripts de limpieza de registro basados en los comandos mostrados en la consola
- **Interacción esperada**:
  - Extrae comandos `reg delete` de la consola
  - Muestra un diálogo para guardar el script
  - Genera un archivo .bat o .ps1 según la selección del usuario
- **Eventos asociados**:
  - `Click`: Ejecuta el método `BtnGenerarScript_Click`
- **Estado condicional**: Habilitado cuando hay comandos `reg delete` en la consola
- **Advertencia de seguridad**: La aplicación muestra una advertencia al usuario sobre los riesgos inherentes de ejecutar scripts, incluso los generados por la herramienta, especialmente cuando se ejecutan con privilegios administrativos. Se recomienda revisar el contenido del script antes de ejecutarlo.

```csharp
private void BtnGenerarScript_Click(object sender, EventArgs e)
{
    try
    {
        // Obtener los comandos reg delete del texto de la consola
        var comandos = ObtenerLineasDeRegDelete(txtConsole.Text);

        if (comandos.Count == 0)
        {
            CustomMsgBox.Show(
                prompt: "No se encontraron comandos de eliminación de registro en la consola.",
                title: "Información",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Information);
            return;
        }

        // Mostrar diálogo para guardar archivo y generar script
        // ...
    }
    catch (Exception ex)
    {
        LogError($"Error al generar script de limpieza: {ex.Message}");
        CustomMsgBox.Show(
            prompt: $"Error al generar script de limpieza: {ex.Message}",
            title: "Error",
            buttons: CustomMessageBoxButtons.OK,
            icon: CustomMessageBoxIcon.Error);
    }
}
```

### Controles de Visualización

#### 1. Lista de Instalaciones

- **Nombre en código**: `lstInstallations`
- **Tipo**: `ListView`
- **Ubicación**: Panel principal, sección superior (splitContainer.Panel1)
- **Función específica**: Muestra las instalaciones de Photoshop detectadas con sus detalles
- **Interacción esperada**:
  - Permite seleccionar instalaciones para desinstalar (solo se procesa la instalación actualmente seleccionada, no admite selección múltiple para desinstalación)
  - Muestra tooltips detallados al pasar el cursor sobre los elementos
- **Eventos asociados**: No tiene eventos directos, pero su selección afecta el comportamiento de otros controles
- **Configuración**:
  - `FullRowSelect = true`: Permite seleccionar filas completas
  - `View = View.Details`: Muestra en modo de detalles con columnas
  - `ShowItemToolTips = true`: Habilita tooltips para los elementos
- **Columnas**:
  - "Nombre" (345 píxeles)
  - "Versión" (100 píxeles)
  - "Ubicación" (410 píxeles)
  - "Confianza" (80 píxeles)

#### 2. Consola de Salida

- **Nombre en código**: `txtConsole`
- **Tipo**: `RichTextBox`
- **Ubicación**: Panel principal, sección inferior (splitContainer.Panel2 > panelConsole)
- **Función específica**: Muestra mensajes de log con formato y colores
- **Interacción esperada**: Muestra información en tiempo real sobre las operaciones
- **Eventos asociados**: No tiene eventos directos, se actualiza mediante el método `AppendToConsole`
- **Configuración**:
  - `BackColor = Color.FromArgb(20, 30, 45)`: Fondo azul oscuro
  - `ForeColor = Color.White`: Texto en color blanco
  - `ReadOnly = true`: No permite edición
  - `Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point)`: Fuente monoespaciada

#### 3. Barra de Progreso

- **Nombre en código**: `progressBar`
- **Tipo**: `ProgressBar`
- **Ubicación**: Panel de estado (panelStatus)
- **Función específica**: Muestra el progreso de las operaciones en curso
- **Interacción esperada**: Se actualiza automáticamente durante las operaciones
- **Eventos asociados**: No tiene eventos directos, se actualiza mediante el método `UpdateProgress`
- **Configuración**:
  - `Style = ProgressBarStyle.Continuous`: Estilo continuo
  - `Visible = false`: Oculto por defecto, visible solo durante operaciones

#### 4. Etiqueta de Progreso

- **Nombre en código**: `lblProgress`
- **Tipo**: `Label`
- **Ubicación**: Panel de estado (panelStatus)
- **Función específica**: Muestra texto descriptivo sobre la operación en curso y su progreso
- **Interacción esperada**: Se actualiza automáticamente durante las operaciones
- **Eventos asociados**: No tiene eventos directos, se actualiza mediante el método `UpdateProgress`
- **Configuración**:
  - `Visible = false`: Oculto por defecto, visible solo durante operaciones
- **Animación**: Durante la fase inicial (0%), muestra una animación de texto con puntos suspensivos

## Formularios Adicionales

### 1. Formulario de Opciones de Modo de Prueba

- **Nombre en código**: `TestModeOptionsForm`
- **Tipo**: `Form`
- **Función específica**: Permite seleccionar el tipo de operación a simular en modo de prueba
- **Controles principales**:
  - `rbDetectOnly`: RadioButton para seleccionar solo detección
  - `rbSimulateUninstall`: RadioButton para simular desinstalación
  - `rbSimulateCleanup`: RadioButton para simular limpieza
  - `btnOK`: Botón para confirmar la selección
  - `btnCancel`: Botón para cancelar
- **Eventos asociados**:
  - `rbDetectOnly.CheckedChanged`: Actualiza `SelectedOperation` a `TestModeOperation.DetectOnly`
  - `rbSimulateUninstall.CheckedChanged`: Actualiza `SelectedOperation` a `TestModeOperation.SimulateUninstall`
  - `rbSimulateCleanup.CheckedChanged`: Actualiza `SelectedOperation` a `TestModeOperation.SimulateCleanup`
  - `btnOK.Click`: Establece `DialogResult = DialogResult.OK`
  - `btnCancel.Click`: Establece `DialogResult = DialogResult.Cancel`

### 2. Formulario de Opciones de Desinstalación

- **Nombre en código**: `UninstallOptionsForm`
- **Tipo**: `Form`
- **Función específica**: Permite configurar opciones para la desinstalación
- **Controles principales**:
  - Opciones para crear copia de seguridad
  - Opciones para limpiar residuos después de la desinstalación
  - Botones para confirmar o cancelar
- **Propiedades expuestas**:
  - `CreateBackup`: Indica si se debe crear una copia de seguridad
  - `CleanupAfterUninstall`: Indica si se deben limpiar residuos después de la desinstalación

### 3. Formulario de Opciones de Limpieza

- **Nombre en código**: `CleanupOptionsForm`
- **Tipo**: `Form`
- **Función específica**: Permite configurar opciones para la limpieza de residuos
- **Controles principales**:
  - Opciones para crear copia de seguridad
  - Botones para confirmar o cancelar
- **Propiedades expuestas**:
  - `CreateBackup`: Indica si se debe crear una copia de seguridad

### 4. Formulario de Restauración de Copias de Seguridad

- **Nombre en código**: `RestoreBackupForm`
- **Tipo**: `Form`
- **Función específica**: Permite seleccionar una copia de seguridad para restaurar
- **Controles principales**:
  - Lista de copias de seguridad disponibles
  - Información sobre cada copia de seguridad
  - Botones para confirmar o cancelar
- **Propiedades expuestas**:
  - `SelectedBackup`: Ruta de la copia de seguridad seleccionada

## Diálogos de Usuario con `CustomMsgBox`

Todos los diálogos de confirmación, información y error presentados al usuario se gestionan a través de la librería `CustomMsgBoxLibrary`. Esto asegura una apariencia consistente y moderna, alineada con el tema oscuro de la aplicación. Para utilizarla, se debe incluir `using CustomMsgBoxLibrary;` y `using CustomMsgBoxLibrary.Types;`.

### Ejemplo de uso para una confirmación:
```csharp
CustomDialogResult userChoice = CustomMsgBox.Show(
    prompt: "¿Está seguro de que desea realizar esta acción?",
    title: "Confirmar Acción",
    buttons: CustomMessageBoxButtons.YesNo,
    icon: CustomMessageBoxIcon.Question,
    defaultButton: MessageBoxDefaultButton.Button2 // Ejemplo: 'No' por defecto
);
if (userChoice == CustomDialogResult.Yes)
{
    // Procesar acción afirmativa
}
```

### Tipos de diálogos disponibles:

1. **Diálogos de información**:
   ```csharp
   CustomMsgBox.Show(
       prompt: "La operación se ha completado correctamente.",
       title: "Información",
       buttons: CustomMessageBoxButtons.OK,
       icon: CustomMessageBoxIcon.Information
   );
   ```

2. **Diálogos de advertencia**:
   ```csharp
   CustomDialogResult warningResult = CustomMsgBox.Show(
       prompt: "Esta acción podría tener consecuencias importantes.",
       title: "Advertencia",
       buttons: CustomMessageBoxButtons.OKCancel,
       icon: CustomMessageBoxIcon.Warning
   );
   ```

3. **Diálogos de error**:
   ```csharp
   CustomMsgBox.Show(
       prompt: "Se ha producido un error al procesar la solicitud.",
       title: "Error",
       buttons: CustomMessageBoxButtons.OK,
       icon: CustomMessageBoxIcon.Error
   );
   ```

4. **Diálogos de confirmación**:
   ```csharp
   CustomDialogResult confirmResult = CustomMsgBox.Show(
       prompt: "¿Desea guardar los cambios antes de salir?",
       title: "Confirmar",
       buttons: CustomMessageBoxButtons.YesNoCancel,
       icon: CustomMessageBoxIcon.Question
   );
   ```

### Personalización de diálogos:

La librería permite personalizar varios aspectos de los diálogos:

```csharp
CustomMsgBox.Show(
    prompt: "Mensaje personalizado con formato.",
    title: "Título Personalizado",
    buttons: CustomMessageBoxButtons.YesNo,
    icon: CustomMessageBoxIcon.Question,
    defaultButton: MessageBoxDefaultButton.Button1,
    theme: ThemeSettings.DarkTheme,
    owner: this, // Formulario propietario
    maxWidth: 500 // Ancho máximo del diálogo
);
```

Se recomienda consultar `recursos/CustomMsgBoxLibrary.md` para conocer todas las opciones de personalización.

## Lógica de Estado de Controles

La aplicación implementa una lógica condicional para habilitar o deshabilitar los botones según el estado actual:

```csharp
private void UpdateButtonsState()
{
    // Verificar si hay instalaciones principales detectadas
    bool hasMainInstallation = _detectedInstallations.Any(i => i.IsMainInstallation);

    // Verificar si hay instalaciones posibles detectadas
    bool hasPossibleMainInstallation = _detectedInstallations.Any(i => i.InstallationType == InstallationType.PossibleMainInstallation);

    // Verificar si hay residuos detectados
    bool hasResiduals = _detectedInstallations.Any(i => i.IsResidual);

    // Botón "Desinstalar": Habilitado si se detecta una instalación principal o posible
    btnUninstall.Enabled = hasMainInstallation || hasPossibleMainInstallation;

    // Botón "Limpiar Residuos": Habilitado solo si NO hay instalación principal ni posible, pero SÍ hay residuos
    btnCleanup.Enabled = !(hasMainInstallation || hasPossibleMainInstallation) && hasResiduals;

    // Configurar tooltips explicativos
    // ...
}
```

Esta lógica se aplica:
- Al iniciar la aplicación
- Después de cada operación de detección
- Al restaurar la interfaz después de una operación

## Manejo de Operaciones Asíncronas

La aplicación utiliza un patrón asíncrono para ejecutar operaciones sin bloquear la interfaz:

```csharp
private async Task RunOperationAsync(Func<IProgress<ProgressInfo>, CancellationToken, Task<OperationResult>> operation)
{
    try
    {
        // Preparar UI
        PrepareUIForOperation();

        // Crear token de cancelación
        _cts = new CancellationTokenSource();

        // Crear objeto de progreso
        var progress = new Progress<ProgressInfo>(UpdateProgress);

        // Ejecutar operación
        var result = await operation(progress, _cts.Token);

        // Mostrar resultado
        // ...
    }
    catch (OperationCanceledException)
    {
        LogWarning("Operación cancelada por el usuario.");
    }
    catch (Exception ex)
    {
        LogError($"Error: {ex.Message}");
    }
    finally
    {
        // Restaurar UI
        RestoreUI();

        // Liberar recursos
        _cts?.Dispose();
        _cts = null;
    }
}
```

Este método:
1. Prepara la interfaz para la operación (deshabilita botones, muestra barra de progreso)
2. Crea un token de cancelación y un objeto de progreso
3. Ejecuta la operación asíncrona
4. Maneja excepciones y cancelaciones
5. Restaura la interfaz al finalizar

## Animación de Progreso

Para mejorar la experiencia de usuario durante operaciones que comienzan en 0%, la aplicación implementa una animación de texto:

```csharp
private void StartProgressAnimation(string operation)
{
    if (_animationTimer == null)
    {
        _animationTimer = new System.Windows.Forms.Timer();
        _animationTimer.Interval = 500; // Parpadeo cada 500ms
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    _currentOperation = operation;
    _isAnimating = true;
    _animationState = 0;
    _animationTimer.Start();
}

private void AnimationTimer_Tick(object? sender, EventArgs e)
{
    try
    {
        if (!_isAnimating) return;

        _animationState = (_animationState + 1) % 4;
        string animationText = _animationState switch
        {
            0 => $"{_currentOperation} - 0%",
            1 => $"{_currentOperation} - 0% .",
            2 => $"{_currentOperation} - 0% ..",
            3 => $"{_currentOperation} - 0% ...",
            _ => $"{_currentOperation} - 0%"
        };

        lblProgress.Text = animationText;
    }
    catch
    {
        // Ignorar errores en la animación
    }
}
```

Esta animación proporciona retroalimentación visual durante operaciones largas que comienzan en 0%, evitando que el usuario piense que la aplicación está congelada.
