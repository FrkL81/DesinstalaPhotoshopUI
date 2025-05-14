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
    *   Este `PlanDesarrollo.md` servirá como documento vivo. El progreso se marcará aquí, y se documentarán las decisiones o desviaciones significativas. (Originalmente se mencionaba `ProgresoDesarrollo.md`, pero integraremos su función aquí).

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
    *   ✅ Establecer las dependencias iniciales.
    *   ✅ Implementar el tema oscuro como se describe en `ManualDesarrollo/03_GUI_Descripcion_Visual.md`, incluyendo la supresión de advertencias (`WFO5001`, `WFO5002`).
    *   ✅ Configurar el icono de la aplicación `app.ico` como se detalla en `ManualDesarrollo/03_GUI_Descripcion_Visual.md` y `ManualDesarrollo/10_Anexos.md`.
*   **Revisar**: `ManualDesarrollo/01_Introduccion_Proyecto.md` (tecnologías), `ManualDesarrollo/10_Anexos.md` (requisitos desarrollador, dependencias).

#### Fase 1.2: Diseño de la Interfaz Principal (`MainForm`) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar la estructura visual de `MainForm` (paneles, `SplitContainer`) según `ManualDesarrollo/03_GUI_Descripcion_Visual.md`.
    *   ✅ Añadir los controles principales (botones, `ListView`, `RichTextBox`, `ProgressBar`, `Label`) descritos en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`.
    *   ✅ Configurar propiedades de los controles (nombres, texto, tamaño, anclaje, colores) según la documentación.
    *   ✅ Implementar la lógica de estado de los botones (`UpdateButtonsState`) y la animación de progreso (`StartProgressAnimation`, `AnimationTimer_Tick`) detallada en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`.
    *   ✅ Implementar los métodos `PrepareUIForOperation` y `RestoreUI`.
*   **Consideración Adicional (Nueva Solicitud)**:
    *   **Iconos de Font Awesome**: Se ha solicitado la integración de iconos de [FontAwesome.Sharp](https://github.com/FortAwesome/Font-Awesome) en los botones para mejorar la estética y la claridad visual. Esto no está contemplado en el `ManualDesarrollo` actual.
        *   ✅ **Acción**: Añadir el paquete NuGet `FontAwesome.Sharp` a `DesinstalaPhotoshop.UI`.
        *   ✅ **Acción**: Modificar el diseño de los botones en `panelTop` y `panelConsoleButtons` para incluir iconos. Esto podría requerir ajustar el tamaño de los botones o del panel.
        *   ⏳ **Acción**: Documentar este cambio. Los documentos `ManualDesarrollo/03_GUI_Descripcion_Visual.md` y `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` deberán ser actualizados *posteriormente* para reflejar esta mejora.
*   **Revisar**: `ManualDesarrollo/03_GUI_Descripcion_Visual.md` (diseño, colores, estructura), `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (nombres, eventos, estado).

#### Fase 1.3: Integración de Biblioteca de Diálogos Personalizados (`CustomMsgBoxLibrary`) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Asegurar la correcta referenciación de `CustomMsgBoxLibrary.dll` (ver `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md` para instalación).
    *   ✅ Reemplazar todos los `MessageBox.Show()` estándar con llamadas a `CustomMsgBox.Show()`, utilizando los parámetros adecuados (`prompt`, `title`, `buttons`, `icon`, `theme`) como se ejemplifica en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (ej. en `BtnCleanup_Click`) y se detalla en `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md`.
    *   ✅ Establecer un tema por defecto para `CustomMsgBox` que sea coherente con el tema oscuro de la aplicación.
*   **Revisar**: `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md` (API, personalización), `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (uso en la aplicación).

#### Fase 1.4: Formularios Secundarios ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar los formularios `TestModeOptionsForm`, `UninstallOptionsForm`, `CleanupOptionsForm`, y `RestoreBackupForm` según la descripción en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`.
    *   ⏳ Asegurar que estos formularios también respeten el tema oscuro y utilicen `CustomMsgBox` para cualquier mensaje que muestren. **Pendiente**: Actualizar `RestoreBackupForm` para usar `CustomMsgBox`.
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
    *   ✅ Implementar `LoggingService` completo con soporte para diferentes niveles de log, almacenamiento en memoria y guardado en archivo.
    *   ✅ Corregir errores de compilación en `BackupMetadata` agregando las propiedades y colecciones faltantes.
    *   ✅ Implementar temporalmente `ProgressInfo` con la propiedad `OperationStatus` para resolver errores de compilación.
    *   ✅ Completar la implementación de las clases `PhotoshopInstallation`, `OperationResult` y `InstallationType` en `DesinstalaPhotoshop.Core` según las descripciones en `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` y su uso en `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md`.
*   **Revisar**: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (descripción de modelos), `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md` (propiedades relevantes para la puntuación).

#### Fase 2.2: Servicio de Detección (`DetectionService`) ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar la estructura básica de `DetectionService` en `DesinstalaPhotoshop.Core`.
    *   ✅ Implementar los servicios auxiliares necesarios:
        *   ✅ `FileSystemHelper`: Implementado con métodos para verificar existencia de archivos/directorios y eliminarlos con reintentos
        *   ✅ `RegistryHelper`: Implementado con métodos para acceder, verificar y manipular claves del registro
    *   ✅ Implementar el método `DetectFromRegistry` con detección mejorada que busca en:
        *   Claves de desinstalación
        *   Claves específicas de Adobe Photoshop
        *   Asociaciones de archivos
    *   ✅ Implementar los métodos `DetectFromInstalledPrograms` y `DetectFromFileSystem`.
    *   ✅ Asegurar que se buscan todas las ubicaciones de archivos y claves de registro especificadas en `ManualDesarrollo/ResiduosDePhotoshop.md` y `ManualDesarrollo/02_Objetivos_Proyecto.md`.
    *   ✅ Implementar el método principal `DetectInstallationsAsync`.
*   **Revisar**: `ManualDesarrollo/05_Flujo_Aplicacion.md` (flujo detallado de detección), `ManualDesarrollo/ResiduosDePhotoshop.md` (lista exhaustiva de rastros).
*   **PUNTO DE PRUEBA CRÍTICO**: ✅ Se ha resuelto el problema con el botón "Detectar". Ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI.

#### Fase 2.3: Sistema de Puntuación Heurística ✅**COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar los métodos `EnrichInstallationInfoAsync` y `ClassifyInstallation` en `DetectionService` según `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` y `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
    *   ✅ Integrar los criterios de puntuación propuestos en `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`, incluyendo:
        *   Verificación de ejecutables principales
        *   Verificación de desinstaladores válidos
        *   Verificación de ubicaciones de instalación
        *   Detección de residuos en ubicaciones no estándar
        *   Verificación de datos de usuario como plugins UXP, caché de fuentes y archivos de autorecuperación
    *   ✅ Conectar la lógica de clasificación con la actualización de la UI en `MainForm` (`lstInstallations`, iconos, tooltips).
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
    *   ✅ Implementar métodos de limpieza:
        *   ✅ `CleanupTempFilesAsync`: Para limpiar archivos temporales de Photoshop
        *   ✅ `CleanupRegistryAsync`: Para limpiar entradas del registro relacionadas con Photoshop
        *   ✅ `CleanupConfigFilesAsync`: Para limpiar archivos de configuración
        *   ✅ `CleanupCacheFilesAsync`: Para limpiar archivos de caché
    *   ✅ Implementar métodos auxiliares:
        *   ✅ `ProcessCommonFilesDirectoriesAsync`: Para procesar carpetas en Common Files
        *   ✅ `ForceDeleteCommonFilesDirectoryAsync`: Para intentar eliminar carpetas difíciles
        *   ✅ `ScheduleFilesForDeletionAsync`: Para programar eliminación de archivos persistentes
        *   ✅ `ScheduleFileForDeletionAsync`: Para programar eliminación de un archivo
        *   ✅ `ScheduleDirectoryForDeletionAsync`: Para programar eliminación de un directorio
    *   ✅ Implementar la clase auxiliar `NativeMethods` para acceder a métodos nativos de Windows (MoveFileEx)
    *   ✅ Asegurar que la limpieza del registro utilice `RegistryHelper` y considere el uso de `reg.exe` como fallback.
    *   ✅ Implementar métodos adicionales en `RegistryHelper`:
        *   ✅ `FindPhotoshopRegistryKeys`: Para buscar claves de registro relacionadas con Photoshop
        *   ✅ `DeleteRegistryKey`: Para eliminar una clave del registro
        *   ✅ `DeleteRegistryKeyWithRegExe`: Para eliminar una clave del registro usando reg.exe
    *   ✅ Implementar métodos adicionales en `FileSystemHelper`:
        *   ✅ `DeleteFile`: Para eliminar un archivo
        *   ✅ `DeleteDirectory`: Para eliminar un directorio y, opcionalmente, su contenido
*   **Revisar**: `ManualDesarrollo/ResiduosDePhotoshop.md` (qué limpiar), `ManualDesarrollo/08_Formatos_Salida.md` (reportes de elementos no eliminados).

#### Fase 3.2: Servicio de Desinstalación (`UninstallService`) ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `UninstallService` en `DesinstalaPhotoshop.Core`.
    *   ✅ Implementar la estructura básica de `UninstallAsync` y sus métodos auxiliares.
    *   ✅ Implementar soporte para diferentes tipos de desinstaladores (ejecutable, MSI, Creative Cloud, manual).
    *   ✅ Integrar con el formulario `UninstallOptionsForm` para opciones de desinstalación.
    *   ✅ Completar la implementación de métodos específicos para ejecutar desinstaladores y eliminar productos MSI.
    *   ✅ Integrar con el servicio de procesos para detener procesos de Adobe antes de la desinstalación.
*   **Revisar**: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (estrategias de desinstalación).

#### Fase 3.3: Servicios Auxiliares (Helpers y Servicios de Soporte) ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ **`FileSystemHelper`**: Implementados métodos básicos para operaciones con archivos y directorios.
    *   ✅ **`RegistryHelper`**: Implementados métodos básicos para operaciones con el registro.
    *   ✅ **`ProcessService`**: Implementado `ProcessService` con métodos para detener procesos y servicios de Adobe antes de la desinstalación/limpieza.
    *   ✅ **`BackupService`**: Implementados `CreateBackupAsync`, `CreateBackupForCleanupAsync`, `RestoreBackupAsync`, `GetAvailableBackups` y `DeleteBackupAsync`. Asegurada la estructura de copias de seguridad y metadatos (`backup_info.json`).
    *   ✅ **`LoggingService`**: Implementado el servicio de logging para consola y archivo.
*   **Revisar**: `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` (para implementaciones de referencia), `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (manejo de errores, optimizaciones).

### Etapa 4: Funcionalidades Avanzadas y Conexión UI-Core ⚠️ **PARCIALMENTE INICIADA**

**Objetivo**: Implementar funcionalidades adicionales y conectar la lógica del Core con la UI.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` (Eventos de botones)
*   `ManualDesarrollo/05_Flujo_Aplicacion.md` (Flujo de operaciones asíncronas)
*   `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` (ScriptGenerator, AdminHelper)
*   `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md` (ScriptGenerator, UI methods)
*   `ManualDesarrollo/08_Formatos_Salida.md` (Scripts de limpieza, Logs)

#### Fase 4.1: Conexión de la Lógica Principal en `MainForm` ✅**COMPLETADA**

*   **Tareas**:
    *   ✅ Implementar los manejadores de eventos para los botones de acción (`BtnDetect_Click`, `BtnUninstall_Click`, `BtnCleanup_Click`, etc.) en `MainForm` como se describe en `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md` y `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md`.
    *   ✅ Implementar el método `RunOperationAsync` para manejar operaciones asíncronas, progreso y cancelación, como se detalla en `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md` y `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.
    *   ✅ Conectar el `LoggingService` con el `txtConsole` de `MainForm`.
    *   ✅ **Problema resuelto**: Se ha corregido el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI. La solución implicó:
        *   Proporcionar todas las dependencias necesarias al `DetectionService`
        *   Mejorar el manejo de errores y el registro de operaciones
        *   Configurar el modo de desarrollo para permitir pruebas sin permisos elevados
*   **Revisar**: `ManualDesarrollo/05_Flujo_Aplicacion.md` (flujo general y de operaciones asíncronas).

#### Fase 4.2: Generación de Scripts ✅ **COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar `ScriptGenerator` en `DesinstalaPhotoshop.Core` con el método `GenerateCleanupScript` según `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md`.
    *   ✅ Implementar la funcionalidad del botón `btnGenerarScript` en `MainForm` para permitir al usuario guardar scripts `.bat` o `.ps1`.
    *   ✅ Implementar la extracción de comandos reg delete del texto de la consola.
    *   ✅ Implementar la conversión de comandos reg.exe a PowerShell para scripts .ps1.
    *   ✅ Implementar un diálogo para que el usuario elija el formato del script y la ubicación donde guardarlo.
    *   ✅ Implementar la opción para abrir el script generado con la aplicación predeterminada.
*   **Revisar**: `ManualDesarrollo/08_Formatos_Salida.md` (estructura de scripts), `ManualDesarrollo/02_Objetivos_Proyecto.md` (requisitos de generación de scripts).

#### Fase 4.3: Integración con Sistema (Admin) ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ Implementar funciones básicas para `IsRunningAsAdmin` y `RequestElevatedPermissions` en `MainForm.cs`.
    *   ✅ Configurar el manifiesto de la aplicación (`app.manifest`) para `asInvoker` durante el desarrollo para facilitar las pruebas.
    *   ⏳ Implementar `AdminHelper` en `DesinstalaPhotoshop.Core` con `IsRunningAsAdmin` y `RestartAsAdmin` (si se decide que la app no requiera admin por defecto, sino que lo solicite).
    *   ⏳ Asegurar que el manifiesto de la aplicación (`app.manifest`) se cambie a `requireAdministrator` para producción como se indica en `ManualDesarrollo/10_Anexos.md`.
    *   ⏳ Integrar las verificaciones de privilegios en los flujos de desinstalación y limpieza.
*   **Revisar**: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (seguridad), `ManualDesarrollo/10_Anexos.md` (manifiesto).

### Etapa 5: Pruebas y Optimización 🛑 **NO INICIADA**

**Objetivo**: Asegurar la calidad, estabilidad y rendimiento de la aplicación.

**Documentación de Referencia Primaria**:
*   `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (Pruebas, Optimizaciones)
*   `ManualDesarrollo/01_Introduccion_Proyecto.md` (Limitaciones conocidas)

#### Fase 5.1: Pruebas Unitarias y de Integración
*   **Tareas**:
    *   Desarrollar pruebas unitarias para los servicios del Core (`DetectionService`, `CleanupService`, `UninstallService`, Helpers).
    *   Realizar pruebas de integración entre el Core y la UI.
    *   Probar el modo de prueba (`whatIf = true`) exhaustivamente.
    *   Utilizar el modo de prueba de `CustomMsgBoxLibrary` si es aplicable para pruebas de UI, como se menciona en `ManualDesarrollo/recursos/CustomMsgBoxLibrary.md` (sección 9).
*   **Revisar**: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (lecciones aprendidas sobre pruebas).

#### Fase 5.2: Pruebas de Usuario (UAT) y Refinamiento
*   **Tareas**:
    *   Realizar pruebas en diferentes configuraciones de Windows y versiones de Photoshop.
    *   Recopilar feedback de usuarios (si es posible) o realizar pruebas basadas en los escenarios del público objetivo (`ManualDesarrollo/01_Introduccion_Proyecto.md`).
    *   Refinar la UI y los mensajes al usuario basándose en los resultados de las pruebas.
    *   Validar que los residuos específicos listados en `ManualDesarrollo/ResiduosDePhotoshop.md` y `ManualDesarrollo/02_Objetivos_Proyecto.md` son correctamente manejados.
*   **Revisar**: Todos los documentos del manual para asegurar que la aplicación final cumple con lo especificado.

#### Fase 5.3: Optimización y Manejo de Errores
*   **Tareas**:
    *   Analizar el rendimiento de operaciones largas (detección, limpieza).
    *   Optimizar consultas al registro y operaciones de sistema de archivos.
    *   Revisar y robustecer el manejo de excepciones y errores en toda la aplicación, aplicando las buenas prácticas de `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.
*   **Revisar**: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (optimizaciones y manejo de errores).

### Etapa 6: Documentación y Distribución ⚠️ **PARCIALMENTE COMPLETADA**

**Objetivo**: Finalizar la documentación y preparar la aplicación para su distribución.

**Documentación de Referencia Primaria**:
*   Todos los documentos del `ManualDesarrollo/`.
*   `ManualDesarrollo/10_Anexos.md` (Comandos de Publicación).

#### Fase 6.1: Finalización de la Documentación ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ Existe un extenso conjunto de documentos en `ManualDesarrollo/` que sirven como base para el desarrollo.
    *   ⏳ Revisar y actualizar todos los documentos del `ManualDesarrollo/` para que reflejen el estado final de la aplicación.
    *   ⏳ Asegurar que todas las capturas de pantalla en `ManualDesarrollo/03_GUI_Descripcion_Visual.md` y otras secciones estén actualizadas, especialmente después de la integración de Font Awesome y `CustomMsgBoxLibrary`.
    *   ⏳ Completar cualquier sección marcada como "Desarrollar..." en este plan o en otros documentos.
    *   ✅ Se ha organizado la documentación adicional en una carpeta `docs/` y se ha actualizado el `.gitignore` para excluir archivos no relacionados con el proyecto.
*   **Revisar**: `ManualDesarrollo/00_Readme_Manual.md` y `ManualDesarrollo/00_Indice_Manual.md` para asegurar la coherencia y completitud.

#### Fase 6.2: Empaquetado y Distribución ⚠️**PARCIALMENTE COMPLETADA**
*   **Tareas**:
    *   ✅ La configuración de publicación (`dotnet publish` y propiedades en `.csproj`) está definida.
    *   ⏳ Utilizar el comando de publicación especificado en `ManualDesarrollo/10_Anexos.md` para generar el ejecutable único y autónomo.
    *   ⏳ Verificar que el archivo `app.manifest` esté correctamente configurado para `requireAdministrator`.
    *   ⏳ Preparar un paquete de distribución (ej. un archivo ZIP) que incluya el ejecutable y una guía de usuario simplificada (basada en `ManualDesarrollo/01_Introduccion_Proyecto.md` y `ManualDesarrollo/03_GUI_Descripcion_Visual.md`).
*   **Revisar**: `ManualDesarrollo/10_Anexos.md` (sección de publicación y manifiesto).

## 5. Dependencias Externas

*   **.NET 9.0**: Framework principal.
*   **System.Management**: Para WMI (detección).
*   **System.ServiceProcess.ServiceController**: Para gestión de servicios.
*   **CustomMsgBoxLibrary.dll**: Para diálogos de usuario modernos (referencia directa o proyecto).
*   **FontAwesome.Sharp**: (Nueva dependencia) Para iconos en la UI.

Consultar `ManualDesarrollo/10_Anexos.md` para detalles de versiones y configuración.

## 6. Consideraciones Especiales

*   **Privilegios de Administrador**: La aplicación debe solicitar y ejecutarse con privilegios de administrador, como se detalla en `ManualDesarrollo/10_Anexos.md` (manifiesto) y `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md` (seguridad).
*   **Manejo de Errores**: Implementar un manejo de errores robusto es crucial, siguiendo las pautas de `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.
*   **Responsividad de la UI**: Todas las operaciones largas deben ser asíncronas (`ManualDesarrollo/05_Flujo_Aplicacion.md`, `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`).
*   **Copias de Seguridad**: La funcionalidad de copias de seguridad es vital (`ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`, `ManualDesarrollo/08_Formatos_Salida.md`).
*   **Impacto de Limpieza**: Ser consciente del impacto de eliminar componentes compartidos, como se discute en `ManualDesarrollo/01_Introduccion_Proyecto.md` (Limitaciones) y `ManualDesarrollo/ResiduosDePhotoshop.md`.

## 7. Estado Actual y Próximos Pasos

### Estado Actual (Mayo 2025)
El proyecto ha completado la **Etapa 1 (Interfaz de Usuario)**, la **Etapa 2 (Detección de Instalaciones)** y la **Etapa 3 (Limpieza y Desinstalación)**. Se ha avanzado significativamente en la **Etapa 4 (Funcionalidades Avanzadas)** con la implementación del generador de scripts (`ScriptGenerator`) y mejoras en la interfaz de usuario.

Se han implementado todos los servicios auxiliares necesarios para la detección y se ha resuelto el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones.

Se ha implementado un sistema de puntuación heurística completo que permite clasificar las instalaciones detectadas como instalaciones principales, posibles instalaciones principales o residuos, según diversos criterios como la presencia de ejecutables, desinstaladores válidos, ubicaciones de instalación, etc.

Se ha completado la implementación del servicio de limpieza (`CleanupService`) con métodos para limpiar archivos temporales, entradas del registro, archivos de configuración y caché. También se han implementado métodos auxiliares para procesar carpetas en Common Files, intentar eliminar carpetas difíciles y programar la eliminación de archivos persistentes al reiniciar el sistema. Se ha implementado la clase auxiliar `NativeMethods` para acceder a métodos nativos de Windows (MoveFileEx).

Se ha completado la implementación del servicio de desinstalación (`UninstallService`) con soporte para diferentes tipos de desinstaladores (ejecutable, MSI, Creative Cloud, manual) y opciones adicionales como eliminar datos de usuario y componentes compartidos. También se ha implementado el servicio de copias de seguridad (`BackupService`) para crear y restaurar copias de seguridad antes de operaciones destructivas.

Se ha implementado el servicio de procesos (`ProcessService`) para detener procesos y servicios de Adobe antes de realizar operaciones de limpieza o desinstalación, lo que es crucial para evitar problemas de bloqueo de archivos durante estas operaciones. Este servicio se ha integrado con los servicios de limpieza y desinstalación.

Se ha implementado el generador de scripts (`ScriptGenerator`) que permite crear scripts de limpieza en formato .bat (CMD) o .ps1 (PowerShell). También se han realizado mejoras en la interfaz de usuario, como la implementación de emojis en el DataGrid para diferenciar visualmente los tipos de instalaciones detectadas, la mejora de las animaciones de progreso para proporcionar retroalimentación visual inmediata al usuario, y la corrección del desbordamiento de texto en el panel central.

### Próximos Pasos Prioritarios
1. **Completar la implementación del servicio de desinstalación**:
   - ✅ Desarrollar `UninstallService` con soporte para diferentes métodos de desinstalación
   - ✅ Completar la implementación de métodos específicos para ejecutar desinstaladores y eliminar productos MSI

2. **Implementar el servicio de limpieza**:
   - ✅ Desarrollar `CleanupService` para eliminar residuos de instalaciones
   - ✅ Implementar métodos para limpiar archivos temporales, entradas del registro, archivos de configuración y caché
   - ✅ Implementar métodos auxiliares para procesar carpetas en Common Files, intentar eliminar carpetas difíciles y programar la eliminación de archivos persistentes

3. **Implementar el sistema de copias de seguridad**:
   - ✅ Desarrollar `BackupService` para crear y restaurar copias de seguridad
   - ✅ Asegurar que todas las operaciones de limpieza y desinstalación creen copias de seguridad

4. **Implementar el servicio de procesos**:
   - ✅ Desarrollar `ProcessService` para detener procesos de Adobe antes de la desinstalación/limpieza

5. **Implementar el generador de scripts**:
   - ✅ Desarrollar `ScriptGenerator` para generar scripts de limpieza en formato .bat (CMD) o .ps1 (PowerShell)
   - ✅ Implementar la extracción de comandos reg delete del texto de la consola
   - ✅ Implementar la conversión de comandos reg.exe a PowerShell para scripts .ps1
   - ✅ Implementar un diálogo para que el usuario elija el formato del script y la ubicación donde guardarlo

6. **Mejorar la interfaz de usuario**:
   - ✅ Implementar emojis en el DataGrid para diferenciar visualmente los tipos de instalaciones detectadas
   - ✅ Mejorar las animaciones de progreso para proporcionar retroalimentación visual inmediata al usuario
   - ✅ Corregir el desbordamiento de texto en el panel central

7. **Implementar la integración con el sistema**:
   - ⏳ Desarrollar la integración con el sistema para permitir la ejecución de operaciones que requieren permisos elevados

## 8. Conclusión

Este plan de desarrollo proporciona una hoja de ruta detallada para la creación de la aplicación DesinstalaPhotoshop. La clave del éxito radicará en la adherencia a este plan, la revisión constante de la documentación de soporte y la implementación de las buenas prácticas identificadas.

---