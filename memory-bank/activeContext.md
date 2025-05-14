# Active Context: DesinstalaPhotoshop

## 1. Tarea/Feature Activa Actual
El foco principal se ha desplazado a la implementación de la **Etapa 3: Limpieza y Desinstalación** del `PlanDesarrollo.md`, ya que la **Etapa 2: Detección de Instalaciones** ha sido completada con éxito.
Específicamente:
1.  **Fase 3.1: Servicio de Limpieza (`CleanupService`):** Implementar `CleanupService.cs` en `DesinstalaPhotoshop.Core` y sus métodos principales: `CleanupAsync`, `CleanupFilesAsync`, `CleanupRegistryAsync`, etc.
    *   Utilizar los servicios auxiliares ya implementados (`FileSystemHelper`, `RegistryHelper`) para operaciones con archivos y registro.
    *   Prestar especial atención a las ubicaciones de residuos detalladas en `ManualDesarrollo/ResiduosDePhotoshop.md`.
2.  **Fase 3.2: Servicio de Desinstalación (`UninstallService`):** Implementar `UninstallService.cs` en `DesinstalaPhotoshop.Core` y sus métodos principales: `UninstallAsync`, `ExecuteUninstallerAsync`, `UninstallMsiProductAsync`, etc.
3.  **Fase 3.3: Servicios Auxiliares:** Implementar los servicios auxiliares restantes:
    *   **`ProcessService`:** Para detener procesos de Adobe antes de la desinstalación/limpieza.
    *   **`BackupService`:** Para crear y restaurar copias de seguridad antes de operaciones destructivas.

## 2. Cambios Recientes (Resumen)
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
2.  **Implementar `UninstallService` (Fase 3.2):** Crear `UninstallService.cs` en `DesinstalaPhotoshop.Core/Services/` con los métodos clave para la desinstalación de Photoshop.
3.  **Implementar `ProcessService` (Fase 3.3):** Crear `ProcessService.cs` en `DesinstalaPhotoshop.Core/Services/` para detener procesos de Adobe antes de la desinstalación/limpieza.
4.  **Implementar `BackupService` (Fase 3.3):** Crear `BackupService.cs` en `DesinstalaPhotoshop.Core/Services/` para crear y restaurar copias de seguridad antes de operaciones destructivas.
5.  **Actualizar `RestoreBackupForm`:** Modificar este formulario para usar `CustomMsgBox.Show()` en lugar de `MessageBox.Show()` para mantener la consistencia en la interfaz de usuario.

## 4. Decisiones Recientes
*   Continuar adhiriéndose estrictamente al `PlanDesarrollo.md` y la documentación existente en `ManualDesarrollo/`. La documentación es la guía principal.
*   El manifiesto (`app.manifest`) se mantiene en `asInvoker` durante el desarrollo para facilitar las pruebas sin elevación constante, con la funcionalidad de `RequestElevatedPermissions` en `MainForm.cs` para probar el flujo de elevación. Se cambiará a `requireAdministrator` para producción.
*   Se ha decidido implementar un sistema de puntuación heurística completo para clasificar las instalaciones detectadas, siguiendo las directrices de `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
*   Se ha decidido mejorar el método `DetectFromFileSystem` para buscar instalaciones en ubicaciones adicionales, incluyendo AppData, ProgramData y Documents, y detectar residuos en ubicaciones no estándar.
*   Se ha decidido implementar el método `ExtractUninstallerPath` para extraer la ruta del desinstalador a partir del string de desinstalación, mejorando la robustez del sistema.

## 5. Consideraciones / Bloqueos Actuales
*   ✅ **Problema resuelto con la funcionalidad de detección**: Se ha corregido el problema con el botón "Detectar", que ahora realiza correctamente la detección de instalaciones en lugar de solo reiniciar la UI. La solución implicó:
    * Proporcionar todas las dependencias necesarias al `DetectionService`
    * Mejorar el manejo de errores y el registro de operaciones
    * Configurar el modo de desarrollo para permitir pruebas sin permisos elevados
*   La implementación de los servicios de limpieza y desinstalación es el próximo paso crítico para avanzar en la funcionalidad real de la aplicación.
*   La lógica de `UpdateButtonsState` en `MainForm.cs` ahora funciona correctamente con la información de `_detectedInstallations` proporcionada por el `DetectionService`.
*   ✅ Se ha implementado un sistema de puntuación heurística completo para clasificar las instalaciones detectadas, siguiendo las directrices de `ManualDesarrollo/Sistema_Puntuacion_Heuristica.md`.
*   Es necesario implementar la validación de permisos de administrador en todos los formularios que realicen operaciones críticas.

## 6. Aprendizajes Clave Recientes
*   **Importancia de proporcionar todas las dependencias necesarias**: La solución al problema con el botón "Detectar" demuestra la importancia de proporcionar todas las dependencias necesarias a los servicios, especialmente cuando se utilizan inyección de dependencias.
*   **Valor del sistema de puntuación heurística**: La implementación del sistema de puntuación heurística ha demostrado ser muy útil para clasificar las instalaciones detectadas y proporcionar una mejor experiencia de usuario.
*   **Beneficios de la búsqueda en múltiples ubicaciones**: La mejora del método `DetectFromFileSystem` para buscar en ubicaciones adicionales ha aumentado significativamente la capacidad de la aplicación para detectar instalaciones y residuos de Photoshop.
*   **Importancia del manejo de errores robusto**: La mejora en el manejo de errores y el registro de operaciones ha facilitado la depuración y ha mejorado la estabilidad de la aplicación.
*   **Valor de la configuración de desarrollo**: Configurar el modo de desarrollo para permitir pruebas sin permisos elevados ha facilitado el desarrollo y las pruebas de la aplicación.
*   **Beneficios de la extracción de métodos auxiliares**: La implementación de métodos auxiliares como `ExtractUninstallerPath` ha mejorado la legibilidad y mantenibilidad del código.