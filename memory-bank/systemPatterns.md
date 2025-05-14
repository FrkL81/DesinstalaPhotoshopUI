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
    *   La funcionalidad principal se encapsula en servicios especializados (ej. `DetectionService`, `UninstallService`, `CleanupService`, `BackupService`, `LoggingService`, `ProcessService`).
    *   Estos servicios son instanciados en `MainForm` y utilizados para orquestar las operaciones.
    *   Promueve la separación de responsabilidades y la testeabilidad.
    *   *Estado Actual:* La UI tiene stubs para llamar a estos servicios; los servicios en Core están por implementarse. `MainForm.cs` actualmente contiene cierta lógica que se migrará a servicios (ej. logging básico, manejo de admin).
*   **Programación Asíncrona (`async/await`):**
    *   Utilizado para todas las operaciones de larga duración (detección, desinstalación, limpieza, restauración) para mantener la UI responsiva.
    *   `MainForm.RunOperationAsync` es un método centralizado en la UI para manejar este patrón, incluyendo la gestión de `CancellationTokenSource` y `IProgress<T>`.
    *   Referencia: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`, `MainForm.cs`.
*   **Patrón Observador (implícito con `IProgress<T>` y Eventos):**
    *   `IProgress<ProgressInfo>` (planificado) se usará para que los servicios del Core reporten el progreso a la UI sin acoplamiento directo.
    *   Se planea un `LoggingService` con un evento `LogAdded` para que la UI se suscriba y actualice la consola.
    *   *Estado Actual:* `MainForm.RunOperationAsync` define un `Progress<object>` básico. El logging está directamente en `MainForm.AppendToConsole`.
*   **Patrón Fachada (Facade):**
    *   Cada servicio del Core actuará como una fachada, simplificando la interfaz para operaciones complejas. Por ejemplo, `CleanupService.CleanupAsync()` orquestará múltiples sub-operaciones (detener procesos, limpiar archivos, limpiar registro).
    *   Referencia: `ManualDesarrollo/06_Arquitectura_Metodos_Lista.md`.
*   **Patrón Estrategia (Strategy):**
    *   Se utilizará implícitamente dentro de los servicios para diferentes métodos de detección (WMI, registro, sistema de archivos) y limpieza (eliminación directa, programación al reinicio, `reg.exe`).
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