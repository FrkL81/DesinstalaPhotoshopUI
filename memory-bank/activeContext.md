# Active Context: DesinstalaPhotoshop

## 1. Tarea/Feature Activa Actual
El foco principal continúa en la implementación de la **Etapa 3: Limpieza y Desinstalación** del `PlanDesarrollo.md`. Se ha completado la implementación del servicio de limpieza (`CleanupService`), el servicio de desinstalación (`UninstallService`) y el servicio de procesos (`ProcessService`).

Específicamente:
1.  ✅ **Fase 3.2: Servicio de Desinstalación (`UninstallService`):** Se ha implementado `UninstallService.cs` en `DesinstalaPhotoshop.Core` con sus métodos principales:
    * `UninstallAsync`: Método principal para desinstalar una instalación de Photoshop
    * `CanUninstall`: Verifica si una instalación puede ser desinstalada
    * `GetUninstallerInfo`: Obtiene información sobre el desinstalador de una instalación
    * Métodos auxiliares para diferentes tipos de desinstaladores (ejecutable, MSI, Creative Cloud, manual)
    * Integración con el formulario `UninstallOptionsForm` para opciones de desinstalación
    * Integración con el servicio de procesos para detener procesos de Adobe antes de la desinstalación

2.  ✅ **Fase 3.1: Servicio de Limpieza (`CleanupService`):** Se ha completado la implementación de `CleanupService.cs` en `DesinstalaPhotoshop.Core`:
    * Implementación completa de métodos de limpieza:
        * `CleanupTempFilesAsync`: Para limpiar archivos temporales de Photoshop
        * `CleanupRegistryAsync`: Para limpiar entradas del registro relacionadas con Photoshop
        * `CleanupConfigFilesAsync`: Para limpiar archivos de configuración
        * `CleanupCacheFilesAsync`: Para limpiar archivos de caché
    * Implementación de métodos auxiliares:
        * `ProcessCommonFilesDirectoriesAsync`: Para procesar carpetas en Common Files
        * `ForceDeleteCommonFilesDirectoryAsync`: Para intentar eliminar carpetas difíciles
        * `ScheduleFilesForDeletionAsync`: Para programar eliminación de archivos persistentes
        * `ScheduleFileForDeletionAsync`: Para programar eliminación de un archivo
        * `ScheduleDirectoryForDeletionAsync`: Para programar eliminación de un directorio
    * Integración con el servicio de procesos para detener procesos de Adobe antes de la limpieza
    * Utilización de los servicios auxiliares (`FileSystemHelper`, `RegistryHelper`) para operaciones con archivos y registro
    * Implementación de la clase auxiliar `NativeMethods` para acceder a métodos nativos de Windows (MoveFileEx)

3.  ✅ **Fase 3.3: Servicios Auxiliares:** Se ha completado la implementación de los servicios auxiliares:
    *   ✅ **`BackupService`:** Se ha completado la implementación para crear y restaurar copias de seguridad antes de operaciones destructivas.
    *   ✅ **`ProcessService`:** Se ha implementado el servicio para detener procesos y servicios de Adobe antes de la desinstalación/limpieza, con los siguientes métodos:
        * `GetRunningAdobeProcesses`: Obtiene una lista de procesos de Adobe en ejecución
        * `StopAdobeProcessesAsync`: Detiene todos los procesos de Adobe en ejecución
        * `StopAdobeServicesAsync`: Detiene todos los servicios de Windows relacionados con Adobe

## 2. Cambios Recientes (Resumen)
*   **Implementación completa del servicio de limpieza (`CleanupService`)**: Se ha completado la implementación del servicio de limpieza con los siguientes métodos:
    *   Métodos de limpieza:
        *   `CleanupTempFilesAsync`: Para limpiar archivos temporales de Photoshop
        *   `CleanupRegistryAsync`: Para limpiar entradas del registro relacionadas con Photoshop
        *   `CleanupConfigFilesAsync`: Para limpiar archivos de configuración
        *   `CleanupCacheFilesAsync`: Para limpiar archivos de caché
    *   Métodos auxiliares:
        *   `ProcessCommonFilesDirectoriesAsync`: Para procesar carpetas en Common Files
        *   `ForceDeleteCommonFilesDirectoryAsync`: Para intentar eliminar carpetas difíciles
        *   `ScheduleFilesForDeletionAsync`: Para programar eliminación de archivos persistentes
        *   `ScheduleFileForDeletionAsync`: Para programar eliminación de un archivo
        *   `ScheduleDirectoryForDeletionAsync`: Para programar eliminación de un directorio
    *   Implementación de la clase auxiliar `NativeMethods` para acceder a métodos nativos de Windows (MoveFileEx)
*   **Implementación de métodos adicionales en `RegistryHelper`**: Se han implementado nuevos métodos en el servicio auxiliar `RegistryHelper`:
    *   `FindPhotoshopRegistryKeys`: Para buscar claves de registro relacionadas con Photoshop
    *   `DeleteRegistryKey`: Para eliminar una clave del registro
    *   `DeleteRegistryKeyWithRegExe`: Para eliminar una clave del registro usando reg.exe
*   **Implementación de métodos adicionales en `FileSystemHelper`**: Se han implementado nuevos métodos en el servicio auxiliar `FileSystemHelper`:
    *   `DeleteFile`: Para eliminar un archivo
    *   `DeleteDirectory`: Para eliminar un directorio y, opcionalmente, su contenido
*   **Implementación del servicio de procesos (`ProcessService`)**: Se ha implementado el servicio para detener procesos y servicios de Adobe antes de la desinstalación/limpieza:
    *   Detección de procesos de Adobe en ejecución
    *   Detención de procesos de Adobe
    *   Detención de servicios de Windows relacionados con Adobe
    *   Integración con los servicios de limpieza y desinstalación
*   **Integración del servicio de procesos con los servicios de limpieza y desinstalación**: Se ha integrado el servicio de procesos con los servicios de limpieza y desinstalación para asegurar que todos los procesos y servicios de Adobe estén detenidos antes de realizar operaciones destructivas.
*   **Implementación del servicio de desinstalación (`UninstallService`)**: Se ha implementado el servicio de desinstalación con soporte para diferentes tipos de desinstaladores:
    *   Desinstaladores ejecutables (.exe)
    *   Desinstaladores MSI
    *   Desinstaladores de Creative Cloud
    *   Desinstalación manual (cuando no se encuentra un desinstalador)
    *   Integración con el formulario `UninstallOptionsForm` para opciones de desinstalación
*   **Mejora del formulario `UninstallOptionsForm`**: Se ha actualizado el formulario para incluir nuevas opciones:
    *   Eliminar datos de usuario
    *   Eliminar componentes compartidos
    *   Modo de simulación (WhatIf)
*   **Implementación completa del servicio de copias de seguridad (`BackupService`)**: Se ha completado la implementación del servicio para crear y restaurar copias de seguridad antes de operaciones destructivas.
*   **Corrección de errores en la clase `OperationResult`**: Se ha actualizado la clase para agregar la propiedad `Message` y el método `SuccessResult`.
*   **Solución del problema con el botón "Detectar"**: Se ha corregido el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI. La solución implicó:
    *   Proporcionar todas las dependencias necesarias al `DetectionService`
    *   Mejorar el manejo de errores y el registro de operaciones
    *   Configurar el modo de desarrollo para permitir pruebas sin permisos elevados
*   **Implementación completa del sistema de puntuación heurística**: Se han implementado los métodos `EnrichInstallationInfoAsync` y `ClassifyInstallation` en `DetectionService` para calcular una puntuación de confianza más precisa y clasificar las instalaciones según diversos criterios.
*   **Mejora del método `DetectFromFileSystem`**: Se ha mejorado el método para buscar instalaciones en ubicaciones adicionales, incluyendo AppData, ProgramData y Documents, y detectar residuos en ubicaciones no estándar.
*   **Actualización del `PlanDesarrollo.md`**: Se ha actualizado el plan de desarrollo para reflejar que la Etapa 2 (Detección de Instalaciones) ha sido completada con éxito.
*   La estructura base de la UI (`MainForm` y formularios secundarios) ha sido completada y refinada.
*   Se ha integrado `FontAwesome.Sharp` para iconos y `CustomMsgBoxLibrary.dll` para diálogos en la mayoría de los formularios.
*   Se ha establecido el tema oscuro y la configuración del proyecto (`.csproj`, `app.manifest` para modo desarrollo).
*   `MainForm.cs` incluye manejadores de eventos para los botones principales y una estructura (`RunOperationAsync`) para manejar operaciones asíncronas, incluyendo chequeos básicos de permisos de administrador y la infraestructura para cancelación y progreso.

## 3. Próximos Pasos Inmediatos
1.  ✅ **Implementar `CleanupService` (Fase 3.1):** Se ha completado la implementación del servicio de limpieza con métodos para limpiar archivos temporales, entradas del registro, archivos de configuración y caché.
2.  ✅ **Implementar `UninstallService` (Fase 3.2):** Se ha completado la implementación del servicio de desinstalación con soporte para diferentes tipos de desinstaladores.
3.  ✅ **Implementar `ProcessService` (Fase 3.3):** Se ha completado la implementación del servicio para detener procesos y servicios de Adobe antes de la desinstalación/limpieza.
4.  ✅ **Implementar `BackupService` (Fase 3.3):** Se ha completado la implementación del servicio de copias de seguridad para crear y restaurar copias de seguridad antes de operaciones destructivas.
5.  **Actualizar `RestoreBackupForm`:** Modificar este formulario para usar `CustomMsgBox.Show()` en lugar de `MessageBox.Show()` para mantener la consistencia en la interfaz de usuario.
6.  **Implementar `ScriptGenerator` (Fase 4.2):** Crear `ScriptGenerator.cs` en `DesinstalaPhotoshop.Core/Services/` para generar scripts de limpieza y desinstalación.
7.  **Implementar integración con el sistema (Fase 4.3):** Implementar la integración con el sistema para permitir la ejecución de operaciones que requieren permisos elevados.

## 4. Decisiones Recientes
*   **Implementación del servicio de procesos**: Se ha decidido implementar el servicio de procesos (`ProcessService`) para detener procesos y servicios de Adobe antes de realizar operaciones de limpieza o desinstalación. Esto es crucial para evitar problemas de bloqueo de archivos durante estas operaciones.
*   **Integración del servicio de procesos con los servicios de limpieza y desinstalación**: Se ha decidido integrar el servicio de procesos con los servicios de limpieza y desinstalación para asegurar que todos los procesos y servicios de Adobe estén detenidos antes de realizar operaciones destructivas.
*   **Implementación del servicio de desinstalación**: Se ha decidido implementar el servicio de desinstalación con soporte para diferentes tipos de desinstaladores (ejecutable, MSI, Creative Cloud, manual) y opciones adicionales como eliminar datos de usuario y componentes compartidos.
*   **Mejora del formulario de opciones de desinstalación**: Se ha decidido mejorar el formulario `UninstallOptionsForm` para incluir nuevas opciones como eliminar datos de usuario, eliminar componentes compartidos y modo de simulación (WhatIf).
*   **Implementación del servicio de copias de seguridad**: Se ha decidido implementar el servicio de copias de seguridad para crear y restaurar copias de seguridad antes de operaciones destructivas.
*   **Corrección de errores en la clase `OperationResult`**: Se ha decidido actualizar la clase para agregar la propiedad `Message` y el método `SuccessResult` para mejorar la comunicación de resultados de operaciones.
*   Continuar adhiriéndose estrictamente al `PlanDesarrollo.md` y la documentación existente en `ManualDesarrollo/`. La documentación es la guía principal.
*   El manifiesto (`app.manifest`) se mantiene en `asInvoker` durante el desarrollo para facilitar las pruebas sin elevación constante, con la funcionalidad de `RequestElevatedPermissions` en `MainForm.cs` para probar el flujo de elevación. Se cambiará a `requireAdministrator` para producción.
*   Se ha decidido implementar un sistema de puntuación heurística completo para clasificar las instalaciones detectadas, siguiendo las directrices de `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
*   Se ha decidido mejorar el método `DetectFromFileSystem` para buscar instalaciones en ubicaciones adicionales, incluyendo AppData, ProgramData y Documents, y detectar residuos en ubicaciones no estándar.
*   Se ha decidido implementar el método `ExtractUninstallerPath` para extraer la ruta del desinstalador a partir del string de desinstalación, mejorando la robustez del sistema.

## 5. Consideraciones / Bloqueos Actuales
*   ✅ **Implementación completa de `CleanupService`**: Se ha completado la implementación del servicio de limpieza con métodos para limpiar archivos temporales, entradas del registro, archivos de configuración y caché.
*   ✅ **Implementación de métodos específicos en `UninstallService`**: Se ha completado la implementación del servicio de desinstalación con soporte para diferentes tipos de desinstaladores.
*   ✅ **Implementación de `ProcessService`**: Se ha completado la implementación del servicio para detener procesos y servicios de Adobe antes de la desinstalación/limpieza.
*   **Implementación de `ScriptGenerator`**: Es necesario implementar el generador de scripts para crear scripts de limpieza y desinstalación.
*   **Implementación de integración con el sistema**: Es necesario implementar la integración con el sistema para permitir la ejecución de operaciones que requieren permisos elevados.
*   ✅ **Problema resuelto con la funcionalidad de detección**: Se ha corregido el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI. La solución implicó:
    * Proporcionar todas las dependencias necesarias al `DetectionService`
    * Mejorar el manejo de errores y el registro de operaciones
    * Configurar el modo de desarrollo para permitir pruebas sin permisos elevados
*   La lógica de `UpdateButtonsState` en `MainForm.cs` ahora funciona correctamente con la información de `_detectedInstallations` proporcionada por el `DetectionService`.
*   ✅ Se ha implementado un sistema de puntuación heurística completo para clasificar las instalaciones detectadas, siguiendo las directrices de `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
*   Es necesario implementar la validación de permisos de administrador en todos los formularios que realicen operaciones críticas.

## 6. Aprendizajes Clave Recientes
*   **Importancia de la limpieza completa**: La implementación del servicio de limpieza ha demostrado la importancia de realizar una limpieza completa del sistema para eliminar todos los residuos de Photoshop, incluyendo archivos temporales, entradas del registro, archivos de configuración y caché.
*   **Valor de los métodos auxiliares**: La implementación de métodos auxiliares como `ProcessCommonFilesDirectoriesAsync`, `ForceDeleteCommonFilesDirectoryAsync`, `ScheduleFilesForDeletionAsync`, etc., ha demostrado ser muy útil para organizar el código y facilitar su mantenimiento.
*   **Importancia de la programación de eliminación de archivos persistentes**: La implementación de métodos para programar la eliminación de archivos persistentes al reiniciar el sistema ha demostrado ser muy útil para eliminar archivos que no se pueden eliminar durante la ejecución normal de la aplicación.
*   **Valor de los métodos nativos de Windows**: La implementación de la clase `NativeMethods` para acceder a métodos nativos de Windows ha demostrado ser muy útil para realizar operaciones que no se pueden realizar con las API estándar de .NET.
*   **Importancia de detener procesos antes de operaciones destructivas**: La implementación del servicio de procesos ha demostrado la importancia de detener procesos y servicios antes de realizar operaciones de limpieza o desinstalación para evitar problemas de bloqueo de archivos.
*   **Valor de la integración entre servicios**: La integración del servicio de procesos con los servicios de limpieza y desinstalación ha demostrado la importancia de la comunicación y colaboración entre diferentes componentes del sistema.
*   **Importancia de la separación de responsabilidades**: La implementación del servicio de desinstalación ha demostrado la importancia de separar claramente las responsabilidades entre diferentes componentes del sistema.
*   **Valor de las opciones de configuración**: La implementación de opciones de configuración en el formulario `UninstallOptionsForm` ha demostrado ser muy útil para proporcionar flexibilidad y control al usuario.
*   **Importancia de la gestión de errores**: La implementación de la clase `OperationResult` ha demostrado ser muy útil para comunicar resultados de operaciones y manejar errores de manera consistente.
*   **Importancia de proporcionar todas las dependencias necesarias**: La solución al problema con el botón "Detectar" demuestra la importancia de proporcionar todas las dependencias necesarias a los servicios, especialmente cuando se utilizan inyección de dependencias.
*   **Valor del sistema de puntuación heurística**: La implementación del sistema de puntuación heurística ha demostrado ser muy útil para clasificar las instalaciones detectadas y proporcionar una mejor experiencia de usuario.
*   **Beneficios de la búsqueda en múltiples ubicaciones**: La mejora del método `DetectFromFileSystem` para buscar en ubicaciones adicionales ha aumentado significativamente la capacidad de la aplicación para detectar instalaciones y residuos de Photoshop.
*   **Importancia del manejo de errores robusto**: La mejora en el manejo de errores y el registro de operaciones ha facilitado la depuración y ha mejorado la estabilidad de la aplicación.
*   **Valor de la configuración de desarrollo**: Configurar el modo de desarrollo para permitir pruebas sin permisos elevados ha facilitado el desarrollo y las pruebas de la aplicación.
*   **Beneficios de la extracción de métodos auxiliares**: La implementación de métodos auxiliares como `ExtractUninstallerPath` ha mejorado la legibilidad y mantenibilidad del código.