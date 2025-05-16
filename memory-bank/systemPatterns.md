# System Patterns: DesinstalaPhotoshop

## 1. Visión Arquitectónica General
La aplicación sigue una arquitectura en capas simple, separando la lógica de negocio de la interfaz de usuario:
*   **`DesinstalaPhotoshop.Core` (Capa de Lógica de Negocio):**
    *   Responsable de todas las operaciones de detección, desinstalación, limpieza, gestión de archivos/registro, copias de seguridad y logging.
    *   No tiene conocimiento de la UI.
    *   Contendrá Modelos (ej. `PhotoshopInstallation`, `OperationResult`), Servicios (ej. `DetectionService`, `CleanupService`) y Utilidades (ej. `FileSystemHelper`, `RegistryHelper`).
*   **`DesinstalaPhotoshop.UI` (Capa de Presentación):**
    *   Responsable de la interacción con el usuario y la visualización de datos.
    *   Utiliza Windows Forms.
    *   Invoca servicios del Core para realizar operaciones.
    *   Maneja el feedback al usuario (progreso, mensajes, logs en UI).

Referencia: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`, `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`

## 2. Patrones de Diseño Clave
*   **Patrón de Servicios (Service Layer):**
    *   La funcionalidad principal se encapsula en servicios especializados (ej. `DetectionService`, `UninstallService`, `CleanupService`, `BackupService`, `LoggingService`, `ProcessService`, `ScriptGenerator`).
    *   Todos los servicios están implementados y funcionales:
        *   `DetectionService`: Sistema de puntuación heurística y detección mejorada
        *   `UninstallService`: Soporte multi-desinstalador
        *   `CleanupService`: Limpieza profunda con programación
        *   `BackupService`: Sistema de copias de seguridad
        *   `ProcessService`: Gestión de procesos y servicios
        *   `LoggingService`: Logging completo
        *   `FileSystemHelper`: Operaciones con archivos robustas
        *   `RegistryHelper`: Operaciones con registro mejoradas
        *   `ScriptGenerator`: Generación de scripts .bat y .ps1
    *   Promueve la separación de responsabilidades y la testeabilidad.
    *   *Estado Actual:* Todos los servicios Core están implementados y funcionando correctamente, integrados con la UI.
*   **Programación Asíncrona (`async/await`):**
    *   Utilizado para todas las operaciones de larga duración (detección, desinstalación, limpieza, restauración) para mantener la UI responsiva.
    *   `MainForm.RunOperationAsync` es un método centralizado que:
        *   Maneja operaciones asíncronas con cancelación
        *   Implementa `IProgress<ProgressInfo>` para retroalimentación
        *   Gestiona estados de UI durante operaciones
        *   Maneja errores y excepciones
        *   Actualiza logs y UI en tiempo real
    *   Referencia: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`, `MainForm.cs`.
*   **Patrón Observador (implícito con `IProgress<T>` y Eventos):**
    *   `IProgress<ProgressInfo>` se usa para reportar progreso a la UI
    *   `LoggingService` con evento `LogAdded` implementado
    *   *Estado Actual:* Sistema de logging completo con UI actualizada en tiempo real
*   **Patrón Fachada (Facade):**
    *   Cada servicio del Core funciona como fachada:
        *   `CleanupService`: Orquesta limpieza de archivos, registro y programación
        *   `UninstallService`: Maneja múltiples tipos de desinstaladores
        *   `ProcessService`: Gestiona procesos y servicios
        *   `BackupService`: Orquesta copias de seguridad
    *   Referencia: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`.
*   **Patrón Estrategia (Strategy):**
    *   Implementado en múltiples servicios:
        *   `DetectionService`: Estrategias de detección (WMI, registro, sistema de archivos)
        *   `CleanupService`: Estrategias de limpieza (directa, programada, `reg.exe`)
        *   `UninstallService`: Estrategias de desinstalación (ejecutable, MSI, Creative Cloud)
        *   `ProcessService`: Estrategias de detención de procesos
    *   Referencia: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`, `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md`.

## 3. Flujos de Datos Principales
*   **Detección:**
    1.  UI (`MainForm.BtnDetect_Click`) -> Invoca `DetectionService.DetectInstallationsAsync()` (Core).
    2.  `DetectionService` utiliza `DetectFromInstalledPrograms`, `DetectFromRegistry`, `DetectFromFileSystem`.
    3.  `EnrichInstallationInfo` y `ClassifyInstallation` procesan los resultados.
    4.  Retorna `List<PhotoshopInstallation>` a la UI.
    5.  UI actualiza `lstInstallations` y estado de botones.
*   **Limpieza/Desinstalación:**
    1.  UI (ej. `MainForm.BtnCleanup_Click`) -> Recoge opciones del usuario (ej. `CleanupOptionsForm`).
    2.  Invoca el servicio correspondiente (ej. `CleanupService.CleanupAsync()`) con opciones y `IProgress`/`CancellationToken`.
    3.  Servicio realiza la operación (crea backup, detiene procesos, elimina archivos/registro, etc.), reportando progreso.
    4.  Servicio retorna `OperationResult`.
    5.  UI muestra resultado, actualiza logs, y posiblemente redispara detección.

Referencias: `ManualDesarrollo/05_Flujo_Aplicacion.md`, `ManualDesarrollo/07_Codigo_Fuente_Metodos_Clave.md`.

## 4. Gestión de Estado
*   **Estado de la UI:** Gestionado principalmente en `MainForm.cs` (ej. `_detectedInstallations`, estado de habilitación de botones, visibilidad de paneles de progreso).
*   **Estado de Operaciones:** El progreso y resultado de operaciones asíncronas se manejan a través de `IProgress<ProgressInfo>` y `Task<OperationResult>`.
*   **Configuración de Usuario (para operaciones):** Capturada a través de formularios modales como `CleanupOptionsForm`, `UninstallOptionsForm`.

## 5. Manejo de Errores y Excepciones
*   **Servicios del Core:** Deben lanzar excepciones específicas o retornar `OperationResult` con detalles del error.
*   **UI (`MainForm.RunOperationAsync`):** Captura excepciones de las operaciones del Core, las loguea, y muestra mensajes al usuario mediante `CustomMsgBox.Show()`.
*   **Logging:** Errores detallados se registran en la consola de la UI y en archivos de log persistentes (planificado vía `LoggingService`).

Referencia: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`.

## 6. Comunicación Inter-Componentes
*   **UI a Core:** Llamadas directas a métodos de servicios del Core.
*   **Core a UI (Feedback):** A través de `IProgress<T>` para progreso y `Task<OperationResult>` para resultados. Eventos para notificaciones más generales (ej. logs).