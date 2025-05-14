# Active Context: DesinstalaPhotoshop

## 1. Tarea/Feature Activa Actual
El foco principal continúa en la implementación de la **Etapa 3: Limpieza y Desinstalación** del `PlanDesarrollo.md`. Se ha avanzado significativamente con la implementación del servicio de desinstalación (`UninstallService`).

Específicamente:
1.  ✅ **Fase 3.2: Servicio de Desinstalación (`UninstallService`):** Se ha implementado `UninstallService.cs` en `DesinstalaPhotoshop.Core` con sus métodos principales:
    * `UninstallAsync`: Método principal para desinstalar una instalación de Photoshop
    * `CanUninstall`: Verifica si una instalación puede ser desinstalada
    * `GetUninstallerInfo`: Obtiene información sobre el desinstalador de una instalación
    * Métodos auxiliares para diferentes tipos de desinstaladores (ejecutable, MSI, Creative Cloud, manual)
    * Integración con el formulario `UninstallOptionsForm` para opciones de desinstalación

2.  ⏳ **Fase 3.1: Servicio de Limpieza (`CleanupService`):** Implementar `CleanupService.cs` en `DesinstalaPhotoshop.Core` y sus métodos principales: `CleanupAsync`, `CleanupFilesAsync`, `CleanupRegistryAsync`, etc.
    *   Utilizar los servicios auxiliares ya implementados (`FileSystemHelper`, `RegistryHelper`) para operaciones con archivos y registro.
    *   Prestar especial atención a las ubicaciones de residuos detalladas en `ManualDesarrollo/ResiduosDePhotoshop.md`.

3.  ✅ **Fase 3.3: Servicios Auxiliares:** Se ha avanzado en la implementación de los servicios auxiliares:
    *   ✅ **`BackupService`:** Se ha completado la implementación para crear y restaurar copias de seguridad antes de operaciones destructivas.
    *   ⏳ **`ProcessService`:** Pendiente de implementar para detener procesos de Adobe antes de la desinstalación/limpieza.

## 2. Cambios Recientes (Resumen)
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
1.  **Implementar `CleanupService` (Fase 3.1):** Crear `CleanupService.cs` en `DesinstalaPhotoshop.Core/Services/` con los métodos clave para la limpieza de residuos de Photoshop.
2.  ✅ **Implementar `UninstallService` (Fase 3.2):** Se ha completado la implementación del servicio de desinstalación con soporte para diferentes tipos de desinstaladores.
3.  **Implementar `ProcessService` (Fase 3.3):** Crear `ProcessService.cs` en `DesinstalaPhotoshop.Core/Services/` para detener procesos de Adobe antes de la desinstalación/limpieza.
4.  ✅ **Implementar `BackupService` (Fase 3.3):** Se ha completado la implementación del servicio de copias de seguridad para crear y restaurar copias de seguridad antes de operaciones destructivas.
5.  **Actualizar `RestoreBackupForm`:** Modificar este formulario para usar `CustomMsgBox.Show()` en lugar de `MessageBox.Show()` para mantener la consistencia en la interfaz de usuario.
6.  **Completar la implementación de `UninstallService`:** Aunque se ha implementado la estructura básica, es necesario completar la implementación de los métodos para ejecutar desinstaladores y eliminar productos MSI.

## 4. Decisiones Recientes
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
*   **Implementación de métodos específicos en `UninstallService`**: Aunque se ha implementado la estructura básica del servicio de desinstalación, es necesario completar la implementación de los métodos para ejecutar desinstaladores y eliminar productos MSI.
*   **Implementación de `CleanupService`**: Es necesario implementar el servicio de limpieza para eliminar residuos de Photoshop después de la desinstalación.
*   **Implementación de `ProcessService`**: Es necesario implementar el servicio para detener procesos de Adobe antes de la desinstalación/limpieza.
*   ✅ **Problema resuelto con la funcionalidad de detección**: Se ha corregido el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI. La solución implicó:
    * Proporcionar todas las dependencias necesarias al `DetectionService`
    * Mejorar el manejo de errores y el registro de operaciones
    * Configurar el modo de desarrollo para permitir pruebas sin permisos elevados
*   La lógica de `UpdateButtonsState` en `MainForm.cs` ahora funciona correctamente con la información de `_detectedInstallations` proporcionada por el `DetectionService`.
*   ✅ Se ha implementado un sistema de puntuación heurística completo para clasificar las instalaciones detectadas, siguiendo las directrices de `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
*   Es necesario implementar la validación de permisos de administrador en todos los formularios que realicen operaciones críticas.

## 6. Aprendizajes Clave Recientes
*   **Importancia de la separación de responsabilidades**: La implementación del servicio de desinstalación ha demostrado la importancia de separar claramente las responsabilidades entre diferentes componentes del sistema.
*   **Valor de las opciones de configuración**: La implementación de opciones de configuración en el formulario `UninstallOptionsForm` ha demostrado ser muy útil para proporcionar flexibilidad y control al usuario.
*   **Importancia de la gestión de errores**: La implementación de la clase `OperationResult` ha demostrado ser muy útil para comunicar resultados de operaciones y manejar errores de manera consistente.
*   **Importancia de proporcionar todas las dependencias necesarias**: La solución al problema con el botón "Detectar" demuestra la importancia de proporcionar todas las dependencias necesarias a los servicios, especialmente cuando se utilizan inyección de dependencias.
*   **Valor del sistema de puntuación heurística**: La implementación del sistema de puntuación heurística ha demostrado ser muy útil para clasificar las instalaciones detectadas y proporcionar una mejor experiencia de usuario.
*   **Beneficios de la búsqueda en múltiples ubicaciones**: La mejora del método `DetectFromFileSystem` para buscar en ubicaciones adicionales ha aumentado significativamente la capacidad de la aplicación para detectar instalaciones y residuos de Photoshop.
*   **Importancia del manejo de errores robusto**: La mejora en el manejo de errores y el registro de operaciones ha facilitado la depuración y ha mejorado la estabilidad de la aplicación.
*   **Valor de la configuración de desarrollo**: Configurar el modo de desarrollo para permitir pruebas sin permisos elevados ha facilitado el desarrollo y las pruebas de la aplicación.
*   **Beneficios de la extracción de métodos auxiliares**: La implementación de métodos auxiliares como `ExtractUninstallerPath` ha mejorado la legibilidad y mantenibilidad del código.