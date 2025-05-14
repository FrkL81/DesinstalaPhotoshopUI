# Active Context: DesinstalaPhotoshop

## 1. Tarea/Feature Activa Actual
El foco principal se ha ampliado para incluir la **Etapa 4: Funcionalidades Avanzadas y Conexión UI-Core** del `PlanDesarrollo.md`. Se ha completado la implementación del servicio de limpieza (`CleanupService`), el servicio de desinstalación (`UninstallService`), el servicio de procesos (`ProcessService`) y ahora se ha implementado el generador de scripts (`ScriptGenerator`).

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
*   **Implementación del Generador de Scripts (`ScriptGenerator`)**: Se ha implementado el generador de scripts con las siguientes funcionalidades:
    *   Generación de scripts de limpieza en formato .bat (CMD) o .ps1 (PowerShell)
    *   Extracción de comandos reg delete del texto de la consola
    *   Conversión de comandos reg.exe a PowerShell para scripts .ps1
    *   Diálogo para que el usuario elija el formato del script y la ubicación donde guardarlo
    *   Opción para abrir el script generado con la aplicación predeterminada

*   **Mejora de Animaciones de Progreso**: Se han realizado mejoras en las animaciones de progreso:
    *   Reducción del intervalo del timer de animación de 500ms a 200ms para una animación más fluida
    *   Modificación del método PrepareUIForOperation para actualizar inmediatamente el texto animado
    *   Adición de Application.DoEvents() en el método RunOperationAsync para forzar la actualización de la UI
    *   Implementación de una animación de puntos suspensivos más visible desde el inicio de las operaciones

*   **Corrección del Desbordamiento de Texto en el Panel Central**: Se han realizado ajustes en el control lblProgress:
    *   Cambio de alineación de AnchorStyles.Bottom | AnchorStyles.Right a AnchorStyles.Left | AnchorStyles.Right
    *   Desactivación de AutoSize para evitar que el texto se expanda más allá del panel
    *   Ajuste del ancho para ocupar todo el panel central
    *   Configuración de TextAlign a ContentAlignment.MiddleLeft
    *   Limitación de la longitud del texto de operación a 20 caracteres

*   **Implementación de Emojis y Mejora de Visualización en el DataGrid**: Se ha mejorado la visualización en el DataGrid:
    *   Inclusión de emojis según el tipo de instalación:
        *   ✅ Marca de verificación verde para instalaciones principales
        *   ⚠️ Señal de advertencia para posibles instalaciones principales
        *   🗑️ Papelera para residuos (incluyendo claves de registro)
    *   Mejora de los tooltips con información detallada:
        *   Tipo de instalación
        *   Puntuación de confianza
        *   Método de detección
        *   Número de claves de registro asociadas
        *   Número de archivos asociados
        *   Notas adicionales

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
6.  ✅ **Implementar `ScriptGenerator` (Fase 4.2):** Se ha completado la implementación del generador de scripts para crear scripts de limpieza en formato .bat (CMD) o .ps1 (PowerShell).
7.  **Implementar integración con el sistema (Fase 4.3):** Implementar la integración con el sistema para permitir la ejecución de operaciones que requieren permisos elevados.
8.  **Mejorar la documentación:** Actualizar la documentación para reflejar los cambios recientes y las nuevas funcionalidades implementadas.

## 4. Decisiones Recientes
*   **Implementación del generador de scripts**: Se ha decidido implementar el generador de scripts (`ScriptGenerator`) para crear scripts de limpieza en formato .bat (CMD) o .ps1 (PowerShell), permitiendo a los usuarios ejecutar operaciones de limpieza sin necesidad de la aplicación.
*   **Mejora de las animaciones de progreso**: Se ha decidido mejorar las animaciones de progreso para proporcionar retroalimentación visual inmediata al usuario cuando se inicia una operación, evitando la sensación de que la aplicación está congelada.
*   **Corrección del desbordamiento de texto**: Se ha decidido ajustar las propiedades del control lblProgress para evitar que el texto se desborde del panel central, manteniendo una apariencia limpia y profesional de la interfaz.
*   **Implementación de emojis en el DataGrid**: Se ha decidido incluir emojis en el DataGrid para diferenciar visualmente los tipos de instalaciones detectadas, mejorando la experiencia del usuario.
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
*   ✅ **Implementación de `ScriptGenerator`**: Se ha completado la implementación del generador de scripts para crear scripts de limpieza en formato .bat (CMD) o .ps1 (PowerShell).
*   **Implementación de integración con el sistema**: Es necesario implementar la integración con el sistema para permitir la ejecución de operaciones que requieren permisos elevados.
*   ✅ **Problema resuelto con la funcionalidad de detección**: Se ha corregido el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI. La solución implicó:
    * Proporcionar todas las dependencias necesarias al `DetectionService`
    * Mejorar el manejo de errores y el registro de operaciones
    * Configurar el modo de desarrollo para permitir pruebas sin permisos elevados
*   ✅ **Mejora de las animaciones de progreso**: Se ha mejorado la retroalimentación visual durante las operaciones largas para evitar la sensación de que la aplicación está congelada.
*   ✅ **Corrección del desbordamiento de texto**: Se ha corregido el problema de desbordamiento de texto en el panel central, manteniendo una apariencia limpia y profesional de la interfaz.
*   ✅ **Implementación de emojis en el DataGrid**: Se han incluido emojis en el DataGrid para diferenciar visualmente los tipos de instalaciones detectadas, mejorando la experiencia del usuario.
*   La lógica de `UpdateButtonsState` en `MainForm.cs` ahora funciona correctamente con la información de `_detectedInstallations` proporcionada por el `DetectionService`.
*   ✅ Se ha implementado un sistema de puntuación heurística completo para clasificar las instalaciones detectadas, siguiendo las directrices de `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
*   Es necesario implementar la validación de permisos de administrador en todos los formularios que realicen operaciones críticas.

## 6. Aprendizajes Clave Recientes
*   **Valor del generador de scripts**: La implementación del generador de scripts ha demostrado ser muy útil para proporcionar a los usuarios una forma de realizar operaciones de limpieza sin necesidad de la aplicación, especialmente en sistemas donde no se pueden instalar aplicaciones adicionales.
*   **Importancia de la retroalimentación visual inmediata**: La mejora de las animaciones de progreso ha demostrado la importancia de proporcionar retroalimentación visual inmediata al usuario cuando se inicia una operación, evitando la sensación de que la aplicación está congelada.
*   **Valor de la corrección del desbordamiento de texto**: La corrección del problema de desbordamiento de texto en el panel central ha mejorado significativamente la apariencia y profesionalidad de la interfaz de usuario.
*   **Beneficios de los emojis en la UI**: La implementación de emojis en el DataGrid ha mejorado la experiencia del usuario al proporcionar una forma rápida y visual de diferenciar los tipos de instalaciones detectadas.
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