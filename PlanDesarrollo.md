# Plan de Desarrollo - DesinstalaPhotoshop

## 1. Introducción

Este documento describe el plan de desarrollo para la aplicación "DesinstalaPhotoshop". El objetivo es crear una herramienta robusta, intuitiva y eficaz para la desinstalación completa de Adobe Photoshop en sistemas Windows, superando las capacidades de los desinstaladores convencionales y herramientas como Adobe CC Cleaner Tool. Este plan se basa en la documentación exhaustiva proporcionada en el `ManualDesarrollo/` y busca asegurar un desarrollo alineado con los objetivos del proyecto.

Como se indica en el `ManualDesarrollo/01_Introduccion_Proyecto.md`, este proyecto es un reinicio de una versión anterior, buscando mejorar la arquitectura, la experiencia de usuario y la mantenibilidad.

## 2. Metodología de Desarrollo

Se seguirá una metodología incremental, dividiendo el proyecto en etapas y fases. En cada paso, la revisión y adhesión a la documentación existente es primordial.

### Revisión Continua de la Documentación

En cada fase del desarrollo, se realizará una revisión sistemática de la documentación del proyecto:

1.  **Revisión del Manual de Desarrollo**:
    *   Al inicio de cada fase, se revisarán los documentos relevantes como `ManualDesarrollo/01_Introduccion_Proyecto.md` y `ManualDesarrollo/02_Objetivos_Proyecto.md` para asegurar que el desarrollo se alinea con los objetivos y requisitos del proyecto.
    *   Se consultará el `ManualDesarrollo/00_Indice_Manual.md` para navegar eficientemente por la documentación.
    *   Antes de implementar una funcionalidad, se revisarán las secciones correspondientes en `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` y `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` para comprender la arquitectura y los métodos clave definidos.

2.  **Actualización del Progreso**:
    *   Este `PlanDesarrollo.md` servirá como documento vivo. El progreso se marcará aquí, y se documentarán las decisiones o desviaciones significativas.

3.  **Verificación de Requisitos**:
    *   Antes de dar por completada una fase, se verificarán los requisitos especificados en `ManualDesarrollo/02_Objetivos_Proyecto.md` y la funcionalidad descrita en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` y `ManualDesarrollo/05_Flujo_Aplicacion.md`.

## 3. Estructura del Proyecto

La estructura del proyecto seguirá una arquitectura en capas, como se detalla en `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` y `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`:

*   **DesinstalaPhotoshop.Core**: Lógica de negocio, servicios, modelos y utilidades.
*   **DesinstalaPhotoshop.UI**: Interfaz gráfica de usuario (Windows Forms).

Las dependencias y propiedades del proyecto se definirán según lo especificado en `ManualDesarrollo/10_Anexos.md`.

## 4. Etapas de Desarrollo

### Etapa 1: Interfaz de Usuario (UI)

**Objetivo**: Crear una interfaz gráfica intuitiva, responsiva y visualmente atractiva, aplicando un tema oscuro y mejorando la experiencia del usuario.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/03_GUI_Descripcion_Visual.md`
*   `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`
*   `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md`

#### Fase 1.1: Configuración del Proyecto y Estructura Básica ✅**COMPLETADA**

*   **Tareas**:
    *   ✅ Crear la solución y los proyectos `DesinstalaPhotoshop.Core` y `DesinstalaPhotoshop.UI`.
    *   ✅ Configurar las propiedades del proyecto (`TargetFramework`, `OutputType`, etc.) según `ManualDesarrollo/10_Anexos.md`.
    *   ✅ Establecer las dependencias iniciales (FontAwesome.Sharp, CustomMsgBoxLibrary).
    *   ✅ Implementar el tema oscuro como se describe en `ManualDesarrollo/03_GUI_Descripcion_Visual.md`, incluyendo la supresión de advertencias (`WFO5001`, `WFO5002`).
    *   ✅ Configurar el icono de la aplicación `app.ico` como se detalla en `ManualDesarrollo/03_GUI_Descripcion_Visual.md` y `ManualDesarrollo/10_Anexos.md`.
*   **Revisar**: `ManualDesarrollo/01_Introduccion_Proyecto.md` (tecnologías), `ManualDesarrollo/10_Anexos.md` (requisitos desarrollador, dependencias).

#### Fase 1.2: Diseño de la Interfaz Principal (`MainForm`) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar la estructura visual de `MainForm` (paneles, `SplitContainer`) según `ManualDesarrollo/03_GUI_Descripcion_Visual.md`.
    *   ✅ Añadir los controles principales (botones con iconos FontAwesome, `ListView`, `RichTextBox`, `ProgressBar`, `Label`) descritos en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`.
    *   ✅ Configurar propiedades de los controles (nombres, texto, tamaño, anclaje, colores) según la documentación.
    *   ✅ Implementar la lógica de estado de los botones (`UpdateButtonsState`, `UpdateButtonColors`) y la animación de progreso (`AnimationTimer_Tick`) detallada en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` y `MainForm.cs`.
    *   ✅ Implementar los métodos `PrepareUIForOperation` y `RestoreUI`.
*   **Revisar**: `ManualDesarrollo/03_GUI_Descripcion_Visual.md` (diseño, colores, estructura), `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (nombres, eventos, estado).

#### Fase 1.3: Integración de Biblioteca de Diálogos Personalizados (`CustomMsgBoxLibrary`) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Asegurar la correcta referenciación de `CustomMsgBoxLibrary.dll`.
    *   ✅ Reemplazar `MessageBox.Show()` con `CustomMsgBox.Show()` en `MainForm` y la mayoría de formularios secundarios.
    *   ✅ Establecer un tema por defecto para `CustomMsgBox` coherente con el tema oscuro.
*   **Revisar**: `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md` (API, personalización), `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (uso en la aplicación).

#### Fase 1.4: Formularios Secundarios ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar los formularios `TestModeOptionsForm`, `UninstallOptionsForm`, `CleanupOptionsForm`, y `RestoreBackupForm` según la descripción en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`.
    *   ⏳ **Pendiente**: Actualizar `RestoreBackupForm.cs` para usar `CustomMsgBox.Show()` en lugar de `MessageBox.Show()` para los mensajes de error y confirmación (actualmente `RestoreBackupForm` usa `CustomMsgBox` en algunos puntos pero no consistentemente para todos los mensajes como `MessageBox.Show($"Error al cargar los backups: {ex.Message}"...`).
*   **Revisar**: `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (propósito y controles de cada formulario).

### Etapa 2: Detección de Instalaciones ✅ **COMPLETADA**

**Objetivo**: Implementar un sistema robusto para detectar instalaciones de Photoshop y sus residuos, clasificándolos adecuadamente.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/02_Objetivos_Proyecto.md` (Objetivo 1: Detección Precisa)
*   `ManualDesarrollo/05_Flujo_Aplicacion.md` (Flujo de Detección)
*   `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (Sección Detección de Instalaciones)
*   `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` (Sección Detección de Instalaciones)
*   `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`
*   `ManualDesarrollo/ResiduosDePhotoshop.md` (para ubicaciones y tipos de rastros)

#### Fase 2.1: Modelos de Datos ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `LoggingService` completo.
    *   ✅ Implementar las clases `PhotoshopInstallation`, `OperationResult`, `ProgressInfo`, `BackupMetadata`, `BackupItem`, `InstallationType`, `DetectionMethod`, `LogLevel` en `DesinstalaPhotoshop.Core` según las descripciones en `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`, `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` y archivos de código.
*   **Revisar**: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (descripción de modelos), `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md` (propiedades relevantes para la puntuación).

#### Fase 2.2: Servicio de Detección (`DetectionService`) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `DetectionService` en `DesinstalaPhotoshop.Core`.
    *   ✅ Implementar los servicios auxiliares necesarios: `FileSystemHelper` y `RegistryHelper`.
    *   ✅ Implementar los métodos `DetectFromInstalledPrograms`, `DetectFromRegistry` (incluyendo búsqueda de claves específicas y asociaciones de archivos) y `DetectFromFileSystem` (incluyendo ubicaciones adicionales como AppData, ProgramData).
    *   ✅ Implementar el método principal `DetectInstallationsAsync`.
*   **Revisar**: `ManualDesarrollo/05_Flujo_Aplicacion.md` (flujo detallado de detección), `ManualDesarrollo/ResiduosDePhotoshop.md` (lista exhaustiva de rastros).

#### Fase 2.3: Sistema de Puntuación Heurística ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar los métodos `EnrichInstallationInfoAsync` y `ClassifyInstallation` en `DetectionService` según `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` y `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
    *   ✅ Integrar los criterios de puntuación, incluyendo verificación de ejecutables, desinstaladores, ubicaciones, datos de usuario (plugins UXP, caché de fuentes, autorecuperación) y datos de licenciamiento (OOBE/SLStore).
    *   ✅ Conectar la lógica de clasificación con la actualización de la UI en `MainForm` (`lstInstallations`, iconos emoji, tooltips).
*   **Revisar**: `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md` (criterios y algoritmo), `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (impacto en la UI).

### Etapa 3: Limpieza y Desinstalación ✅ **COMPLETADA**

**Objetivo**: Desarrollar la funcionalidad para desinstalar Photoshop y limpiar todos sus residuos de manera segura y eficaz.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/02_Objetivos_Proyecto.md` (Objetivos 2 y 3)
*   `ManualDesarrollo/05_Flujo_Aplicacion.md` (Flujos de Desinstalación y Limpieza)
*   `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (Secciones Desinstalación, Limpieza de Residuos, Gestión de Registro, Gestión de Archivos, Copias de Seguridad)
*   `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` (secciones correspondientes)

#### Fase 3.1: Servicio de Limpieza (`CleanupService`) ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `CleanupService` en `DesinstalaPhotoshop.Core`.
    *   ✅ Implementar métodos de limpieza: `CleanupTempFilesAsync`, `CleanupRegistryAsync`, `CleanupConfigFilesAsync`, `CleanupCacheFilesAsync`.
    *   ✅ Implementar métodos auxiliares: `ProcessCommonFilesDirectoriesAsync`, `ForceDeleteCommonFilesDirectoryAsync`, `ScheduleFilesForDeletionAsync` (y sus variantes para archivos/directorios individuales), `NativeMethods` para `MoveFileEx`.
    *   ✅ Integración con `RegistryHelper` y `FileSystemHelper`.
*   **Revisar**: `ManualDesarrollo/ResiduosDePhotoshop.md` (qué limpiar), `ManualDesarrollo/08_Formatos_Salida.md` (reportes de elementos no eliminados).

#### Fase 3.2: Servicio de Desinstalación (`UninstallService`) ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `UninstallService` en `DesinstalaPhotoshop.Core`.
    *   ✅ Implementar `UninstallAsync` y métodos auxiliares (`RunExecutableUninstallerAsync`, `RunMsiUninstallerAsync`, `RunCreativeCloudUninstallerAsync`, `PerformManualUninstallAsync`).
    *   ✅ Implementar `CanUninstall` y `GetUninstallerInfo` (incluyendo `FindDesinstallerInInstallLocation`, `ParseMsiUninstallString`, `ExtractExecutablePath`, `ExtractArguments`).
    *   ✅ Integrar con `UninstallOptionsForm` para opciones de desinstalación (CreateBackup, WhatIf, RemoveUserData, RemoveSharedComponents).
    *   ✅ Integrar con `ProcessService` para detener procesos de Adobe.
*   **Revisar**: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (estrategias de desinstalación).

#### Fase 3.3: Servicios Auxiliares (Helpers y Servicios de Soporte) ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ **`FileSystemHelper`**: Implementados métodos para operaciones con archivos y directorios (`DirectoryExists`, `FileExists`, `FindFiles`, `GetDirectorySize`, `FindDirectories`, `CreateDirectory`, `CopyFileAsync`, `DeleteFile`, `DeleteDirectory`).
    *   ✅ **`RegistryHelper`**: Implementados métodos para operaciones con el registro (`FindPhotoshopInstallations`, `KeyExists`, `GetRegistryValue`, `ExportRegistryKey`, `ImportRegistryFile`, `FindPhotoshopRegistryKeys`, `DeleteRegistryKey`, `DeleteRegistryKeyWithRegExe`).
    *   ✅ **`ProcessService`**: Implementado `ProcessService` con métodos para detener procesos y servicios de Adobe (`StopAdobeProcessesAsync`, `GetRunningAdobeProcesses`, `StopAdobeServicesAsync`).
    *   ✅ **`BackupService`**: Implementados `CreateBackupForCleanupAsync`, `CreateBackupAsync`, `GetAvailableBackups`, `GetBackupsAsync`, `DeleteBackupAsync`, `RestoreBackupAsync`. Asegurada la estructura de copias de seguridad y metadatos (`backup_info.json`).
    *   ✅ **`LoggingService`**: Implementado el servicio de logging para consola y archivo con niveles y evento `LogAdded`.
*   **Revisar**: `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md`, `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.

### Etapa 4: Funcionalidades Avanzadas y Conexión UI-Core ✅ **COMPLETADA**

**Objetivo**: Implementar funcionalidades adicionales y conectar la lógica del Core con la UI.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (Eventos de botones)
*   `ManualDesarrollo/05_Flujo_Aplicacion.md` (Flujo de operaciones asíncronas)
*   `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (ScriptGenerator, AdminHelper)
*   `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` (ScriptGenerator, UI methods)
*   `ManualDesarrollo/08_Formatos_Salida.md` (Scripts de limpieza, Logs)

#### Fase 4.1: Conexión de la Lógica Principal en `MainForm` ✅**COMPLETADA**

*   **Tareas**:
    *   ✅ Implementar los manejadores de eventos para los botones de acción (`BtnDetect_Click`, `BtnUninstall_Click`, `BtnCleanup_Click`, `BtnTestMode_Click`, `BtnCancel_Click`, `BtnRestore_Click`, `BtnCopyOutput_Click`, `BtnAbrirLog_Click`, `BtnGenerarScript_Click`) en `MainForm` como se describe en la documentación y el código.
    *   ✅ Implementar `RunOperationAsync` para manejar operaciones asíncronas, progreso y cancelación.
    *   ✅ Conectar `LoggingService` con `txtConsole` de `MainForm` a través del evento `LogAdded`.
    *   ✅ Implementada la lógica de privilegios en `MainForm` para detección y otras operaciones, incluyendo reinicio con argumentos (`--elevated`, `--elevated-for-detection`).
    *   ✅ Lógica de actualización de UI (`UpdateInstallationsList`, `SelectFirstMainInstallation`, `UpdatePanelInfoLayout`) funcional.
*   **Revisar**: `ManualDesarrollo/05_Flujo_Aplicacion.md`.

#### Fase 4.2: Generación de Scripts ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `ScriptGenerator` en `DesinstalaPhotoshop.Core` con `GenerateCleanupScript`, `ExtractRegDeleteCommands`, `GenerateBatScript`, `GeneratePowerShellScript`, `ConvertRegCommandToPowerShell`.
    *   ✅ Implementar la funcionalidad del botón `btnGenerarScript` en `MainForm` para guardar scripts `.bat` o `.ps1`.
*   **Revisar**: `ManualDesarrollo/08_Formatos_Salida.md`, `ManualDesarrollo/02_Objetivos_Proyecto.md`.

#### Fase 4.3: Integración con Sistema (Admin) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `AdminHelper` en `DesinstalaPhotoshop.Core` con `IsRunningAsAdmin` y `RestartAsAdmin`.
    *   ✅ `MainForm.cs` utiliza `AdminHelper` y maneja la lógica de elevación.
    *   ✅ Configurado el manifiesto de la aplicación (`app.manifest`) para `asInvoker` (facilitando el desarrollo), pero con la intención de cambiar a `requireAdministrator` para producción. El código de `Program.cs` y `MainForm.cs` maneja la detección de argumentos como `--elevated` para saber si se ha reiniciado con privilegios.
*   **Revisar**: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (seguridad), `ManualDesarrollo/10_Anexos.md` (manifiesto).

### Etapa 5: Pruebas y Optimización 🛑 **NO INICIADA**

**Objetivo**: Asegurar la calidad, estabilidad y rendimiento de la aplicación.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (Pruebas, Optimizaciones)
*   `ManualDesarrollo/01_Introduccion_Proyecto.md` (Limitaciones conocidas)

#### Fase 5.1: Pruebas Unitarias y de Integración
*   **Tareas**:
    *   Desarrollar pruebas unitarias para los servicios del Core.
    *   Realizar pruebas de integración entre el Core y la UI.
    *   Probar el modo de prueba (`whatIf = true`) exhaustivamente.
    *   Utilizar el modo de prueba de `CustomMsgBoxLibrary` si es aplicable.
*   **Revisar**: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.

#### Fase 5.2: Pruebas de Usuario (UAT) y Refinamiento
*   **Tareas**:
    *   Realizar pruebas en diferentes configuraciones de Windows y versiones de Photoshop.
    *   Recopilar feedback o realizar pruebas basadas en escenarios del público objetivo.
    *   Refinar la UI y los mensajes al usuario.
    *   Validar manejo de residuos de `ManualDesarrollo/ResiduosDePhotoshop.md`.
*   **Revisar**: Todos los documentos del manual.

#### Fase 5.3: Optimización y Manejo de Errores
*   **Tareas**:
    *   Analizar el rendimiento de operaciones largas.
    *   Optimizar consultas al registro y operaciones de sistema de archivos.
    *   Revisar y robustecer el manejo de excepciones.
*   **Revisar**: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.

### Etapa 6: Documentación y Distribución ⚠️ **PARCIALMENTE COMPLETADA**

**Objetivo**: Finalizar la documentación y preparar la aplicación para su distribución.

**Documentación de Referencia Primaria**:
*   Todos los documentos del `ManualDesarrollo/`.
*   `ManualDesarrollo/10_Anexos.md` (Comandos de Publicación).

#### Fase 6.1: Finalización de la Documentación ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ Existe un extenso conjunto de documentos en `ManualDesarrollo/`.
    *   ⏳ Revisar y actualizar todos los documentos del `ManualDesarrollo/` para que reflejen el estado final de la aplicación.
    *   ⏳ Asegurar que todas las capturas de pantalla estén actualizadas.
    *   ⏳ Completar cualquier sección marcada como "Desarrollar..." o TODO.
    *   ✅ `.gitignore` configurado.
*   **Revisar**: `ManualDesarrollo/00_Readme_Manual.md` y `ManualDesarrollo/00_Indice_Manual.md`.

#### Fase 6.2: Empaquetado y Distribución ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ La configuración de publicación (`dotnet publish` y propiedades en `.csproj`) está definida.
    *   ⏳ Utilizar el comando de publicación para generar el ejecutable único y autónomo.
    *   ⏳ Verificar que `app.manifest` se cambie a `requireAdministrator` para producción.
    *   ⏳ Preparar un paquete de distribución (ej. ZIP).
*   **Revisar**: `ManualDesarrollo/10_Anexos.md`.

## 5. Dependencias Externas

*   **.NET 9.0**: Framework principal.
*   **System.Management**: Para WMI (detección).
*   **System.ServiceProcess.ServiceController**: Para gestión de servicios.
*   **CustomMsgBoxLibrary.dll**: Para diálogos de usuario modernos (referencia directa).
*   **FontAwesome.Sharp**: Para iconos en la UI.

Consultar `ManualDesarrollo/10_Anexos.md` y archivos `.csproj` para detalles.

## 6. Consideraciones Especiales

*   **Privilegios de Administrador**: La aplicación gestiona la solicitud de privilegios. Se debe cambiar el `app.manifest` a `requireAdministrator` para la versión de producción.
*   **Manejo de Errores**: Implementado robustamente a través de `OperationResult` y logging.
*   **Responsividad de la UI**: Asegurada mediante `async/await`.
*   **Copias de Seguridad**: Funcionalidad implementada.
*   **Impacto de Limpieza**: Se debe ser consciente del impacto de eliminar componentes compartidos; las opciones de limpieza lo permiten.

## 7. Estado Actual y Próximos Pasos

### Estado Actual (Mayo 2025)
El proyecto ha completado las **Etapas 1, 2, 3 y 4**. La funcionalidad principal está implementada:
*   **UI**: Completa, con tema oscuro, iconos, diálogos personalizados (salvo revisión menor en `RestoreBackupForm`).
*   **Detección**: Completa, con sistema de puntuación heurística y búsqueda en múltiples ubicaciones.
*   **Core Logic**:
    *   `DetectionService`: Completo.
    *   `CleanupService`: Completo.
    *   `UninstallService`: Completo.
    *   `ProcessService`: Completo.
    *   `BackupService`: Completo.
    *   `ScriptGenerator`: Completo.
    *   `LoggingService`: Completo.
    *   Helpers (`FileSystemHelper`, `RegistryHelper`, `AdminHelper`): Completos.
*   **Conexión UI-Core**: Completa, incluyendo manejo de operaciones asíncronas, progreso y cancelación.
*   **Gestión de Privilegios**: Implementada para desarrollo y con plan para producción.

### Próximos Pasos Prioritarios
1.  **Actualizar `RestoreBackupForm.cs`**: Usar `CustomMsgBox.Show()` consistentemente (Fase 1.4).
2.  **Etapa 5: Pruebas y Optimización**:
    *   Iniciar Fase 5.1: Pruebas Unitarias y de Integración.
    *   Proceder con Fase 5.2: Pruebas de Usuario (UAT) y Refinamiento.
    *   Ejecutar Fase 5.3: Optimización y Manejo de Errores (revisión final).
3.  **Etapa 6: Documentación y Distribución**:
    *   Completar Fase 6.1: Finalizar la revisión y actualización de toda la documentación del `ManualDesarrollo/` para reflejar el estado final, incluyendo capturas de pantalla.
    *   Ejecutar Fase 6.2: Empaquetado y preparación para distribución, asegurando que `app.manifest` se cambie a `requireAdministrator`.

## 8. Riesgos y Desafíos Potenciales
*   **Complejidad de Pruebas**: Probar en múltiples versiones de Photoshop y configuraciones de Windows puede ser extenso.
*   **Falsos Positivos/Negativos en Detección**: El sistema heurístico, aunque robusto, podría necesitar ajustes tras pruebas en escenarios reales variados.
*   **Cambios en Adobe Photoshop**: Futuras versiones de Photoshop podrían alterar la ubicación de archivos/registros, requiriendo actualizaciones en la lógica de detección y limpieza.
*   **Impacto en Componentes Compartidos**: Aunque se ofrecen opciones, la eliminación de componentes compartidos siempre conlleva un riesgo si el usuario no está seguro.

## 9. Conclusión

Este plan de desarrollo proporciona una hoja de ruta detallada para la creación de la aplicación DesinstalaPhotoshop. El proyecto ha alcanzado un alto grado de completitud funcional. Las próximas etapas se centrarán en asegurar la calidad, robustez y la preparación para el lanzamiento.