## **Plan Detallado de Avances Pendientes: DesinstalaPhotoshop**

**Prioridad General:** Estabilidad, funcionalidad completa del Core, consistencia en la UI, y luego pruebas exhaustivas.

---

### **Módulo 1: Consolidación y Refactorización del Core**

**Objetivo:** Establecer una base de código limpia, eliminando redundancias y asegurando la consistencia interna antes de abordar funcionalidades más complejas o pruebas extensivas. Esta fase es crucial para la mantenibilidad y eficiencia a largo plazo.

#### **Fase 1.1: Unificación de Clases Auxiliares (Helpers)**

*   **Tarea 1.1.1: Centralizar `FileSystemHelper` y `RegistryHelper`**
    *   **Descripción:** Actualmente existen dos conjuntos de clases `FileSystemHelper` y `RegistryHelper`: uno en `DesinstalaPhotoshop.Core/Utilities/` (estáticas, sin logging) y otro en `DesinstalaPhotoshop.Core/Services/Helpers/` (basadas en interfaces, con logging inyectado). Nos estandarizaremos en las versiones de `Services/Helpers/` que promueven mejores prácticas de diseño.
    *   **Instrucciones Técnicas:**
        1.  **Análisis de Uso:** Realizar una búsqueda exhaustiva en toda la solución para identificar cualquier uso de `DesinstalaPhotoshop.Core.Utilities.FileSystemHelper` o `DesinstalaPhotoshop.Core.Utilities.RegistryHelper`.
        2.  **Refactorización (si es necesario):**
            *   Si se encuentran usos, modificarlos para que utilicen las implementaciones de `DesinstalaPhotoshop.Core.Services.Helpers.FileSystemHelper` y `DesinstalaPhotoshop.Core.Services.Helpers.RegistryHelper` a través de sus respectivas interfaces (`IFileSystemHelper`, `IRegistryHelper`).
            *   Esto implicará, en los servicios que los consuman, asegurar que estas dependencias se inyecten a través del constructor.
        3.  **Eliminación:** Una vez confirmado que las clases en `Utilities/` no son referenciadas o que sus usos han sido refactorizados, proceder a:
            *   Eliminar la carpeta `DesinstalaPhotoshop.Core/Utilities/` del proyecto y del sistema de archivos.
            *   Eliminar cualquier directiva `using DesinstalaPhotoshop.Core.Utilities;` que haya quedado huérfana en los archivos `.cs`.
    *   **Justificación de la Decisión:** Se elimina código duplicado, lo que reduce la superficie de mantenimiento y el riesgo de inconsistencias. Estandarizar en helpers basados en interfaces facilita la inyección de dependencias y mejora significativamente la testeabilidad del código del Core (especialmente de los servicios que los utilizan).

#### **Fase 1.2: Optimización del Modelo `ProgressInfo`**

*   **Tarea 1.2.1: Eliminar `OperationStatus` String de `ProgressInfo`**
    *   **Descripción:** Las clases `ProgressInfo` (tanto en `Core/Models` como en `UI/Models`) contienen una propiedad `OperationStatus` de tipo `string` que es redundante, dado que el estado de la operación puede inferirse de las propiedades booleanas existentes (`IsRunning`, `IsCompleted`, `HasError`, `IsCanceled`, `HasWarning`).
    *   **Instrucciones Técnicas:**
        1.  **Modificar `DesinstalaPhotoshop.Core/Models/ProgressInfo.cs`:**
            *   Eliminar la propiedad pública `public string OperationStatus { get; }`.
            *   Ajustar el constructor privado y los métodos fábrica estáticos (`Running`, `Completed`, `Error`, `Failed`, `Canceled`, `Warning`) para que ya no acepten ni establezcan un parámetro `operationStatus`.
        2.  **Modificar `DesinstalaPhotoshop.UI/Models/ProgressInfo.cs`:**
            *   Eliminar la propiedad pública `public string OperationStatus { get; }`.
            *   Ajustar el constructor y los métodos fábrica estáticos para que ya no acepten ni establezcan un parámetro `operationStatus`.
        3.  **Actualizar `DesinstalaPhotoshop.UI/MainForm.cs`:**
            *   Localizar el manejador del evento `Progress` en `BtnDetect_Click` (y cualquier otro lugar donde se use `Progress<Core.Models.ProgressInfo>`). La línea `progress.Report(new ProgressInfo(...))` debe ser actualizada.
                *   La instancia de `UI.Models.ProgressInfo` que se crea debe construirse utilizando las propiedades booleanas del objeto `info` (que es de tipo `Core.Models.ProgressInfo`) para determinar el estado actual.
            *   Localizar el método `UpdateProgress(int percentage, string? statusText = null)` y el método `AnimationTimer_Tick`. La lógica que actualiza `lblProgress.Text` y `lblAnimatedText.Text` debe ser modificada. En lugar de depender de `ProgressInfo.OperationStatus` (que ya no existirá), se debe determinar el texto descriptivo del estado (`"Operación en curso"`, `"Completado"`, `"Error"`, etc.) basándose en las propiedades booleanas del `ProgressInfo` recibido o en el estado actual de la operación almacenado en `MainForm` (ej. `_currentOperation`).
                *   **Ejemplo de lógica para `UpdateProgress`:**
                    ```csharp
                    // Dentro de UpdateProgress, asumiendo que 'info' es el objeto ProgressInfo
                    string statusString = "Estado desconocido";
                    if (info.IsRunning) statusString = _currentOperation; // O un texto genérico como "Procesando"
                    if (info.IsCompleted) statusString = "Completado";
                    if (info.HasError) statusString = "Error";
                    if (info.IsCanceled) statusString = "Cancelado";
                    if (info.HasWarning) statusString = "Advertencia";
                    
                    // Actualizar etiquetas con statusString y percentage
                    lblProgress.Text = $"{statusString} - {percentage}%";
                    if (percentage >= 100) {
                        lblAnimatedText.Text = $"{statusString}!";
                    }
                    ```
    *   **Justificación de la Decisión:** Se simplifican los modelos `ProgressInfo`, se elimina la redundancia de datos y se promueve un manejo de estado más robusto y menos propenso a errores (evitando comparaciones de strings). Las propiedades booleanas ofrecen una forma más clara y semántica de representar el estado.

---

### **Módulo 2: Pulido de la Interfaz de Usuario (UI)**

**Objetivo:** Completar las tareas pendientes menores en la UI para asegurar una experiencia de usuario consistente y profesional.

#### **Fase 2.1: Consistencia en Diálogos Modales**

*   **Tarea 2.1.1: Actualizar `RestoreBackupForm.cs` para Uso Exclusivo de `CustomMsgBox`**
    *   **Descripción:** El formulario `RestoreBackupForm.cs` tiene una instancia donde se utiliza `MessageBox.Show()` estándar en lugar de `CustomMsgBox.Show()`, específicamente en el bloque `catch` del método `LoadBackups`. Todas las interacciones modales con el usuario deben usar `CustomMsgBoxLibrary`.
    *   **Instrucciones Técnicas:**
        1.  Abrir `DesinstalaPhotoshop.UI/RestoreBackupForm.cs`.
        2.  Localizar la llamada a `MessageBox.Show(...)` dentro del bloque `catch (Exception ex)` del método `LoadBackups()`.
        3.  Reemplazarla con la siguiente llamada a `CustomMsgBox.Show()`:
            ```csharp
            CustomMsgBox.Show(
                prompt: $"Error al cargar los backups: {ex.Message}",
                title: "Error",
                buttons: CustomMessageBoxButtons.OK,
                icon: CustomMessageBoxIcon.Error,
                theme: ThemeSettings.DarkTheme // O una referencia al tema global si está disponible
            );
            ```
        4.  Asegurarse de que las directivas `using CustomMsgBoxLibrary;` y `using CustomMsgBoxLibrary.Types;` estén presentes al inicio del archivo.
    *   **Justificación de la Decisión:** Garantiza una experiencia de usuario visualmente homogénea en toda la aplicación, utilizando consistentemente la librería de diálogos modernos y personalizables.

---

### **Módulo 3: Integración con el Sistema y Seguridad Operativa**

**Objetivo:** Finalizar la implementación de la gestión de privilegios de administrador, asegurando que la aplicación pueda solicitar elevación cuando sea necesario y esté correctamente configurada para su distribución.

#### **Fase 3.1: Gestión de Privilegios de Administrador**

*   **Tarea 3.1.1: Integración Robusta de `AdminHelper` en `MainForm`**
    *   **Descripción:** El flujo para solicitar elevación de privilegios en `MainForm.cs` debe utilizar consistentemente el `AdminHelper` del proyecto Core.
    *   **Instrucciones Técnicas:**
        1.  En `DesinstalaPhotoshop.UI/MainForm.cs`, revisar el método `RequestElevatedPermissions(string arguments = "")`.
        2.  Asegurar que este método llame a `DesinstalaPhotoshop.Core.Services.Helpers.AdminHelper.RestartAsAdmin(arguments)`. La implementación actual de `AdminHelper.RestartAsAdmin` ya maneja `Environment.Exit(0)`, por lo que no es necesario hacerlo en `MainForm`.
        3.  Revisar el método `RunOperationAsync<T>(...)`. La condición `if (requiresElevation && !IsRunningAsAdmin())` es correcta. Asegurar que la llamada interna sea a `RequestElevatedPermissions()`.
        4.  Verificar que todos los manejadores de eventos que invocan operaciones críticas (ej. `BtnUninstall_Click`, `BtnCleanup_Click`, `BtnRestore_Click`) pasen el parámetro `requiresElevation: true` a `RunOperationAsync`. La lógica de `IsRunningAsAdmin()` dentro de `RunOperationAsync` ya considera la variable `_developmentMode`.
    *   **Justificación de la Decisión:** Centraliza la lógica de gestión de privilegios en el `AdminHelper` del Core, lo que promueve un código más limpio en la UI y facilita el mantenimiento de esta funcionalidad crítica. El `_developmentMode` permite flexibilidad durante el desarrollo.

---

### **Módulo 4: Pruebas Exhaustivas (Continuación de Etapa 5 del Plan)**

**Objetivo:** Verificar la calidad, estabilidad, corrección funcional y rendimiento de la aplicación a través de un conjunto completo de pruebas.

#### **Fase 4.1: Configuración y Desarrollo de Pruebas Unitarias (Core)**
    (Retomar donde se dejó, asegurando cobertura para los servicios y helpers principales)

*   **Tarea 4.1.1: Completar Proyecto de Pruebas Unitarias (`DesinstalaPhotoshop.Core.Tests`)**
    *   **Instrucciones Técnicas:**
        1.  Si aún no existe, crear un proyecto de prueba (MSTest recomendado por simplicidad, o NUnit/xUnit) llamado `DesinstalaPhotoshop.Core.Tests`.
        2.  Añadir referencia al proyecto `DesinstalaPhotoshop.Core`.
        3.  Añadir el paquete NuGet `Moq` para mocking de dependencias.
    *   **Justificación de la Decisión:** Un proyecto dedicado para pruebas unitarias es esencial para la organización y ejecución eficiente de las pruebas. `Moq` es un framework de mocking popular y potente.

*   **Tarea 4.1.2: Implementar Pruebas Unitarias para Servicios (Continuación)**
    *   **Instrucciones Técnicas:**
        *   **`DetectionService`:**
            *   Crear Mocks para `IRegistryHelper` y `IFileSystemHelper` que simulen diferentes escenarios:
                *   Sistema limpio (sin Photoshop).
                *   Instalación completa de Photoshop CC (registro y archivos).
                *   Instalación de Photoshop CS6.
                *   Solo residuos de registro.
                *   Solo residuos de archivos (en AppData, Program Files, etc.).
                *   Múltiples versiones instaladas.
            *   Probar `DetectInstallationsAsync`: verificar que el número y tipo de `PhotoshopInstallation` devueltas sean correctos para cada escenario.
            *   Probar `EnrichInstallationInfoAsync`: para una `PhotoshopInstallation` dada con datos simulados (ej. `InstallLocation` que existe/no existe, `UninstallString` válido/inválido), verificar que `ConfidenceScore` y `AssociatedFiles` se actualizan según la lógica actual.
            *   Probar `ClassifyInstallation`: para `PhotoshopInstallation` con diferentes `ConfidenceScore` y propiedades, verificar que `InstallationType`, `IsMainInstallation`, `IsResidual` se establecen correctamente.
        *   **`CleanupService`:**
            *   Mockear `IBackupService`, `IProcessService`, `IFileSystemHelper`, `IRegistryHelper`.
            *   Probar `CleanupAsync`:
                *   Caso `createBackup = true`: Verificar que `_backupService.CreateBackupForCleanupAsync` es llamado.
                *   Caso `whatIf = true`: Verificar que los métodos destructivos de `IFileSystemHelper` (ej. `DeleteFile`, `DeleteDirectory`) y `IRegistryHelper` (ej. `DeleteRegistryKey`) **NO** son llamados.
                *   Caso `whatIf = false`: Verificar que los métodos destructivos **SÍ** son llamados.
                *   Probar diferentes combinaciones de `cleanupTempFiles`, `cleanupRegistry`, etc., y verificar que los métodos internos correspondientes (ej. `CleanupTempFilesAsync`) son invocados.
                *   Verificar que `_processService.StopAdobeProcessesAsync` es llamado.
        *   **`UninstallService`:**
            *   Mockear `IBackupService`, `IProcessService`, `IFileSystemHelper`, `IRegistryHelper`.
            *   Probar `CanUninstall`: pasar objetos `PhotoshopInstallation` con y sin `UninstallString`, y con `InstallLocation` existente/inexistente.
            *   Probar `GetUninstallerInfo`: pasar `PhotoshopInstallation` con diferentes `UninstallString` (MSI, EXE, Creative Cloud) y verificar que se parsea correctamente a `UninstallerInfo`.
            *   Probar `UninstallAsync`:
                *   Verificar llamadas a `CanUninstall`, `_backupService.CreateBackupAsync`, `_processService.StopAdobeProcessesAsync`.
                *   Simular diferentes `UninstallerInfo` y verificar que se invoca el método interno de desinstalación apropiado (ej. `RunMsiUninstallerAsync`).
                *   Verificar que se llaman `RemoveUserDataAsync` y `RemoveSharedComponentsAsync` si los flags correspondientes son `true` y la desinstalación principal fue exitosa.
        *   **`BackupService`:**
            *   Mockear `IFileSystemHelper` y `IRegistryHelper`.
            *   Probar `CreateBackupAsync`/`CreateBackupForCleanupAsync`: Verificar que se intentan crear los directorios (`_fileSystemHelper.CreateDirectory`), copiar archivos (`_fileSystemHelper.CopyFileAsync`) y exportar claves de registro (`_registryHelper.ExportRegistryKey`). Verificar que se intenta escribir el `backup_info.json` (mock `File.WriteAllTextAsync`).
            *   Probar `GetAvailableBackups`: Simular una estructura de directorios de backup con archivos `backup_info.json` válidos e inválidos (o ausentes). Verificar que se deserializan y devuelven los `BackupMetadata` correctos.
            *   Probar `RestoreBackupAsync`: Mockear un `backup_info.json`. Verificar que se intenta copiar archivos y importar claves de registro según los metadatos.
            *   Probar `DeleteBackupAsync`: Verificar que se llama a `Directory.Delete` en la ruta correcta.
        *   **`ScriptGenerator`:**
            *   Probar `ExtractRegDeleteCommands` con varios ejemplos de texto de consola (incluyendo casos borde y sin comandos).
            *   Probar `GenerateCleanupScript` para `.bat` y `.ps1`. Mockear `File.WriteAllText` y verificar que el contenido del string del script es el esperado (incluye encabezados, comandos convertidos/formateados).
            *   Probar `ConvertRegCommandToPowerShell` con ejemplos de `reg delete key` y `reg delete value`.
        *   **`ProcessService`:**
            *   Probar `GetRunningAdobeProcesses`: Mockear `Process.GetProcesses()` para devolver una lista de procesos, algunos de los cuales deben coincidir con `_adobeProcessNames`. Verificar que se filtran correctamente.
            *   Probar `StopAdobeProcessesAsync`: Mockear `GetRunningAdobeProcesses` para devolver procesos. Verificar que se intenta llamar a `process.Kill()` para los procesos correctos cuando `whatIf = false`.
            *   Probar `StopAdobeServicesAsync` (puede ser más difícil de unit-testar sin una capa de abstracción sobre `ServiceController`; considerar testear la lógica de filtrado de nombres de servicio).
    *   **Justificación de la Decisión:** Una cobertura de pruebas unitarias robusta para la capa Core es fundamental para garantizar la fiabilidad de la lógica de negocio, la cual realiza operaciones críticas en el sistema del usuario. El mocking es esencial para aislar las unidades bajo prueba.

#### **Fase 4.2: Pruebas de Integración y Usuario (UAT)**
    (Estas pruebas son manuales y requieren entornos controlados)

*   **Tarea 4.2.1: Ejecución de Escenarios de Prueba en Máquinas Virtuales**
    *   **Descripción:** Probar la aplicación completa en entornos que simulen configuraciones de usuario reales.
    *   **Instrucciones Técnicas:**
        1.  **Preparación de Entornos:**
            *   Configurar al menos 2-3 VMs:
                *   VM 1: Windows 10 (última versión) + Photoshop CC (versión reciente).
                *   VM 2: Windows 11 (última versión) + Photoshop CS6 + Photoshop CC (versión intermedia).
                *   (Opcional) VM 3: Windows 10/11 con una instalación "problemática" de Photoshop (ej. parcialmente desinstalada, con muchos residuos conocidos de `ResiduosDePhotoshop.md`).
            *   Tomar snapshots de las VMs antes de cada ciclo de prueba para poder revertir fácilmente.
        2.  **Ejecución de Casos de Uso Principales (documentados en `ManualDesarrollo/05_Flujo_Aplicacion.md`):**
            *   **Detección:** En cada VM, ejecutar la detección.
                *   Validar: ¿Se detectan todas las instalaciones? ¿La clasificación (Principal, Posible, Residuo) es correcta según la puntuación heurística esperada? ¿La información (versión, ruta) es precisa? ¿Los iconos y tooltips son correctos?
            *   **Modo de Prueba (WhatIf):**
                *   Para una instalación principal, ejecutar Desinstalación en modo prueba. Revisar logs.
                *   Para residuos, ejecutar Limpieza en modo prueba. Revisar logs.
                *   Validar: ¿Los logs indican las acciones que se *habrían* tomado? ¿No se realizan cambios reales en el sistema?
            *   **Desinstalación Real:**
                *   Seleccionar una instalación principal. Usar `UninstallOptionsForm` (probar con y sin backup, con y sin limpieza post-desinstalación, con y sin eliminación de datos de usuario/componentes compartidos).
                *   Validar: ¿Se crea el backup si se seleccionó? ¿El desinstalador de Photoshop se ejecuta (si aplica)? ¿La aplicación reporta éxito? ¿Se eliminan los archivos principales y claves de registro asociadas a la instalación? ¿La limpieza post-desinstalación funciona? Revisar logs.
            *   **Limpieza Real:**
                *   Después de una desinstalación, o en una VM con residuos, ejecutar Limpieza. Usar `CleanupOptionsForm` (probar con y sin backup, diferentes combinaciones de tipos de limpieza).
                *   Validar: ¿Se crea el backup? ¿Se eliminan los archivos/registros residuales esperados (comparar con `ResiduosDePhotoshop.md`)? ¿Se manejan correctamente los archivos bloqueados (programación para reinicio)? Revisar logs.
            *   **Generación de Scripts:**
                *   Después de una limpieza (real o simulada) donde se reporten elementos no eliminados (especialmente de registro), usar `btnGenerarScript`.
                *   Generar tanto `.bat` como `.ps1`.
                *   Inspeccionar el contenido: ¿Los comandos son correctos? ¿La sintaxis es válida para cada tipo de script?
                *   (Opcional avanzado y con precaución) Ejecutar los scripts en una VM de prueba para verificar su efecto.
            *   **Restauración de Backups:**
                *   Realizar una desinstalación/limpieza con creación de backup.
                *   Verificar que el backup aparece en `RestoreBackupForm`.
                *   Restaurar el backup.
                *   Validar: ¿Se restauran los archivos y claves de registro a su estado anterior? ¿Photoshop vuelve a ser funcional (si se restauró una desinstalación completa)?
            *   **Cancelación de Operaciones:** Intentar cancelar operaciones largas (detección, limpieza con muchos elementos) y verificar que la operación se detiene y la UI se restaura correctamente.
            *   **Manejo de Errores:** Intentar simular errores (ej. denegar acceso a una carpeta clave, corromper un archivo de backup) y observar cómo la aplicación maneja y reporta estos errores.
    *   **Justificación de la Decisión:** Las pruebas UAT en entornos realistas son indispensables para descubrir problemas de compatibilidad, errores de lógica que solo aparecen en escenarios complejos, y para evaluar la usabilidad general de la aplicación desde la perspectiva del usuario final.

#### **Fase 4.3: Revisión de Rendimiento**
    (Esta fase puede superponerse con las pruebas UAT)

*   **Tarea 4.3.1: Identificación y Optimización de Cuellos de Botella**
    *   **Descripción:** Analizar el tiempo de ejecución de las operaciones más intensivas (detección, limpieza profunda) e identificar áreas de mejora.
    *   **Instrucciones Técnicas:**
        1.  Durante las pruebas UAT, usar un cronómetro o `System.Diagnostics.Stopwatch` (si se añaden puntos de medición temporales en el código) para registrar los tiempos de las operaciones clave en las diferentes VMs.
        2.  Si se identifican operaciones particularmente lentas (ej. `DetectFromFileSystem` en un disco grande y lleno, o `CleanupRegistryAsync` con miles de claves a verificar/eliminar):
            *   **Analizar el Código:** Revisar la implementación en busca de algoritmos ineficientes, bucles anidados innecesarios, o un uso excesivo de operaciones costosas de I/O o registro dentro de bucles.
            *   **Optimización Potencial para `DetectFromFileSystem`:**
                *   Asegurar que las búsquedas no se repitan innecesariamente.
                *   Considerar si es posible limitar la profundidad de búsqueda en ciertas rutas genéricas.
                *   Paralelizar la búsqueda en diferentes directorios raíz (ej. `Program Files`, `AppData` de diferentes usuarios) usando `Task.WhenAll`, asegurándose de que el acceso a la lista de resultados compartida sea seguro (thread-safe).
            *   **Optimización Potencial para `CleanupRegistryAsync` y `CleanupFilesAsync`:**
                *   Agrupar operaciones similares si es posible.
                *   Revisar las consultas LINQ para asegurar que no son ineficientes en colecciones grandes.
            *   **Medición:** Después de aplicar una optimización, volver a medir para confirmar la mejora.
    *   **Justificación de la Decisión:** Aunque la responsividad de la UI se logra con `async/await`, la duración total de las operaciones sigue siendo importante para la experiencia del usuario. Optimizar las partes más lentas puede hacer la aplicación significativamente más agradable de usar.

---

### **Módulo 5: Preparación para Lanzamiento (Continuación de Etapa 6 del Plan)**

**Objetivo:** Finalizar todos los aspectos de documentación, configuración y empaquetado para asegurar una distribución exitosa.

#### **Fase 5.1: Finalización de la Documentación**

*   **Tarea 5.1.1: Actualización y Verificación del `ManualDesarrollo/`**
    *   **Descripción:** Asegurar que toda la documentación técnica refleje el estado final y probado del código.
    *   **Instrucciones Técnicas:**
        1.  Realizar una lectura completa de todos los documentos en `ManualDesarrollo/`, comparándolos con el código fuente final.
        2.  **Especial atención a:**
            *   `03_GUI_Descripcion_Visual.md`: Actualizar capturas de pantalla para reflejar el uso de `CustomMsgBox` y los iconos de `FontAwesome.Sharp`.
            *   `04_GUI_Funcionalidad_Controles.md`: Verificar que las descripciones de comportamiento y estado condicional de los botones sean precisas.
            *   `06_Arquitectura_Metodos_Lista.md`: Confirmar que las firmas y propósitos de los métodos listados coinciden con la implementación.
            *   `07_Codigo_Fuente_Metodos_Clave.md`: Actualizar cualquier fragmento de código para que sea idéntico al código de producción. Si se decidió mantener el esquema de puntuación actual y no el de `Sistema_Puntuacion_Heuristica.md`, indicarlo claramente aquí.
            *   `Sistema_Puntuacion_Heuristica.md`: Añadir una nota al principio del documento que aclare si la implementación actual difiere y si este documento representa una mejora futura o una guía conceptual.
        3.  Verificar todos los enlaces internos dentro del manual.
    *   **Justificación de la Decisión:** Una documentación precisa es crucial para el mantenimiento a largo plazo, la incorporación de nuevos desarrolladores y como referencia para la resolución de problemas.

#### **Fase 5.2: Configuración de Producción y Empaquetado**

*   **Tarea 5.2.1: Configurar `app.manifest` para `requireAdministrator`**
    *   **Descripción:** Para la distribución final, la aplicación debe solicitar privilegios de administrador al iniciarse.
    *   **Instrucciones Técnicas:**
        1.  Abrir el archivo `DesinstalaPhotoshop.UI/app.manifest`.
        2.  Cambiar la línea:
            `<requestedExecutionLevel level="asInvoker" uiAccess="false" />`
            a:
            `<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />`
        3.  Guardar el archivo.
    *   **Justificación de la Decisión:** Las funciones principales de la aplicación (desinstalación, limpieza de registro, eliminación de archivos en directorios protegidos) requieren privilegios de administrador. Solicitar estos privilegios al inicio garantiza que la aplicación pueda operar sin fallos inesperados de permisos.

*   **Tarea 5.2.2: Generar el Build de Release Final y Crear Paquete de Distribución**
    *   **Descripción:** Compilar la versión final de la aplicación y empaquetarla para los usuarios.
    *   **Instrucciones Técnicas:**
        1.  Realizar un `git clean -fdx` (con precaución, esto elimina archivos no rastreados) o limpiar manualmente los directorios `bin/` y `obj/` de ambos proyectos para asegurar una compilación limpia.
        2.  Abrir la solución en Visual Studio y seleccionar la configuración `Release`.
        3.  Recompilar toda la solución.
        4.  Desde la línea de comandos, en el directorio raíz del proyecto (`C:/MiRepo/DPui3/`), ejecutar el comando de publicación:
            ```bash
            dotnet publish DesinstalaPhotoshop.UI -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
            ```
        5.  Verificar que en la carpeta `publish/` (creada en `C:/MiRepo/DPui3/publish/`) se encuentre un único archivo `DesinstalaPhotoshop.UI.exe`.
        6.  Probar este ejecutable en las VMs de prueba para una última verificación rápida.
        7.  Crear un archivo ZIP (ej. `DesinstalaPhotoshop_v1.0.0.zip`) que contenga:
            *   El archivo `DesinstalaPhotoshop.UI.exe` de la carpeta `publish/`.
            *   Una versión simplificada de `README.md` adaptada para el usuario final (Guía Rápida de Uso).
            *   El archivo `LICENSE`.
    *   **Justificación de la Decisión:** El empaquetado como un único ejecutable auto-contenido (`PublishSingleFile=true`, `SelfContained=true`) simplifica la distribución y el uso para el usuario final, ya que no requiere la instalación previa del runtime de .NET en su sistema.