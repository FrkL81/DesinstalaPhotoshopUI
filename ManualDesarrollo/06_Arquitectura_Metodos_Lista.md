# Arquitectura y Métodos Clave

## Estructura General del Código

La aplicación DesinstalaPhotoshop está diseñada siguiendo una arquitectura en capas que separa claramente la lógica de negocio de la interfaz de usuario. Esta separación permite una mayor mantenibilidad, testabilidad y flexibilidad del código.

### Organización del Proyecto

El proyecto está organizado en dos componentes principales:

1. **DesinstalaPhotoshop.Core**: Contiene toda la lógica de negocio y es independiente de la interfaz de usuario.
   - **Models**: Clases que representan los datos de la aplicación (PhotoshopInstallation, OperationResult, etc.)
   - **Services**: Servicios que implementan la funcionalidad principal (DetectionService, UninstallService, etc.)
   - **Utilities**: Clases de utilidad para operaciones comunes (FileSystemHelper, RegistryHelper, etc.)

2. **DesinstalaPhotoshop.UI**: Contiene la interfaz gráfica de usuario.
   - **Forms**: Formularios de Windows Forms (MainForm, OptionsForm, etc.)
   - **Controls**: Controles personalizados, si los hay
   - **Resources**: Recursos como imágenes, iconos y cadenas localizadas

Esta separación permite:
- Reutilizar la lógica de negocio en diferentes interfaces (GUI, CLI, etc.)
- Probar la lógica de negocio de forma independiente
- Mantener y evolucionar cada capa de forma independiente

### Patrón de Diseño

La aplicación implementa varios patrones de diseño:

1. **Patrón de Servicios**: La funcionalidad principal está encapsulada en servicios especializados que son inyectados donde se necesitan. Cada servicio tiene una responsabilidad única y bien definida.

2. **Patrón Asíncrono**: Las operaciones potencialmente largas se implementan de forma asíncrona utilizando `async/await` para mantener la interfaz de usuario responsiva.

3. **Patrón de Observador**: Se utiliza a través del mecanismo de `IProgress<T>` para reportar el progreso de operaciones largas a la interfaz de usuario.

4. **Patrón de Fachada**: Cada servicio actúa como una fachada que oculta la complejidad de las operaciones que realiza, proporcionando una interfaz simple y coherente.

5. **Patrón de Estrategia**: Se utilizan diferentes estrategias para operaciones como la detección de instalaciones o la limpieza de residuos, permitiendo seleccionar la más adecuada en cada caso.

### Flujo de Datos y Control

El flujo típico de la aplicación es el siguiente:

1. La interfaz de usuario (MainForm) recibe una acción del usuario
2. MainForm llama al servicio correspondiente (DetectionService, UninstallService, etc.)
3. El servicio realiza la operación, reportando progreso a través de IProgress<ProgressInfo>
4. El servicio devuelve un OperationResult con el resultado de la operación
5. MainForm actualiza la interfaz en función del resultado

Este flujo asegura una clara separación de responsabilidades y facilita el mantenimiento y la evolución de la aplicación.

## Métodos Clave por Funcionalidad

### Detección de Instalaciones

La detección de instalaciones de Photoshop se realiza a través del servicio `DetectionService`, que implementa múltiples estrategias para encontrar instalaciones en el sistema.

#### Métodos Principales de DetectionService

##### DetectInstallationsAsync

- **Firma**: `public async Task<List<PhotoshopInstallation>> DetectInstallationsAsync(IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)`
- **Propósito**: Método principal que coordina la detección de instalaciones de Photoshop utilizando múltiples estrategias.
- **Parámetros**:
  - `progress`: Objeto para reportar el progreso de la operación
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Lista de instalaciones de Photoshop detectadas
- **Relaciones**: Llama a métodos específicos de detección y aplica puntuación heurística
- **Observaciones**: Este método es asíncrono para no bloquear la interfaz de usuario durante la detección

##### DetectFromInstalledPrograms

- **Firma**: `private List<PhotoshopInstallation> DetectFromInstalledPrograms()`
- **Propósito**: Detecta instalaciones de Photoshop registradas como programas instalados en Windows.
- **Retorno**: Lista de instalaciones detectadas desde programas instalados
- **Relaciones**: Utiliza WMI (Windows Management Instrumentation) y el registro de Windows
- **Observaciones**: Las instalaciones detectadas por este método tienen alta probabilidad de ser instalaciones principales

##### DetectFromRegistry

- **Firma**: `private List<PhotoshopInstallation> DetectFromRegistry()`
- **Propósito**: Busca instalaciones de Photoshop en el registro de Windows, incluyendo claves que no aparecen en la lista de programas instalados.
- **Retorno**: Lista de instalaciones detectadas desde el registro
- **Relaciones**: Utiliza `RegistryHelper` para acceder al registro de Windows
- **Observaciones**: Puede detectar instalaciones parciales o residuos que no aparecen en la lista de programas instalados. Examina claves como `HKCU\SOFTWARE\Adobe\Photoshop\[versión_numérica]`, `HKLM\SOFTWARE\Adobe\Photoshop\[versión_numérica]` y claves de asociación de archivos.

##### DetectFromFileSystem

- **Firma**: `private List<PhotoshopInstallation> DetectFromFileSystem()`
- **Propósito**: Busca archivos y carpetas en el sistema que puedan pertenecer a instalaciones de Photoshop.
- **Retorno**: Lista de instalaciones detectadas desde el sistema de archivos
- **Relaciones**: Utiliza `FileSystemHelper` para buscar archivos y carpetas
- **Observaciones**: Útil para detectar residuos de instalaciones que han sido parcialmente desinstaladas. Busca en ubicaciones conocidas como `%AppData%\Adobe\Adobe Photoshop [versión]\`, `%LocalAppData%\Adobe\OOBE\`, carpetas de Plugins UXP/CEP, y directorios de instalación estándar.

##### EnrichInstallationInfo

- **Firma**: `private void EnrichInstallationInfo(PhotoshopInstallation installation)`
- **Propósito**: Enriquece la información de una instalación detectada y calcula su puntuación de confianza aplicando un algoritmo heurístico.
- **Parámetros**:
  - `installation`: Instalación a evaluar y enriquecer
- **Relaciones**: Actualiza las propiedades `ConfidenceScore`, `AssociatedFiles` y otras propiedades de la instalación
- **Observaciones**: La puntuación se basa en factores como la presencia del ejecutable principal, desinstalador válido, asociaciones de archivos, etc. Este método realiza la función que anteriormente se describía como `CalculateConfidenceScore`.

##### ClassifyInstallation

- **Firma**: `private void ClassifyInstallation(PhotoshopInstallation installation)`
- **Propósito**: Clasifica una instalación como principal, posible instalación principal o residuos basándose en su puntuación de confianza.
- **Parámetros**:
  - `installation`: Instalación a clasificar
- **Relaciones**: Utiliza la puntuación calculada por `CalculateConfidenceScore`
- **Observaciones**: La clasificación determina cómo se muestra la instalación en la interfaz y qué operaciones se pueden realizar sobre ella

#### Sistema de Puntuación Heurística

La aplicación utiliza un sistema de puntuación heurística para clasificar las instalaciones detectadas como instalaciones principales o residuos. Este sistema asigna puntos positivos y negativos según diversos criterios como la presencia del ejecutable principal, desinstalador válido, ubicación, etc.

> **Nota**: Para una descripción detallada del sistema de puntuación heurística, incluyendo todos los criterios, la implementación en el código y su impacto en la interfaz de usuario, consulte el documento [Sistema_Puntuacion_Heuristica.md](Sistema_Puntuacion_Heuristica.md).

### Desinstalación

La desinstalación de Photoshop se realiza a través del servicio `UninstallService`, que implementa diferentes estrategias para desinstalar completamente las instalaciones detectadas.

#### Métodos Principales de UninstallService

##### UninstallAsync

- **Firma**: `public async Task<OperationResult> UninstallAsync(PhotoshopInstallation installation, bool createBackup = true, bool cleanupAfterUninstall = true, bool whatIf = false, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)`
- **Propósito**: Método principal que coordina la desinstalación de una instalación de Photoshop.
- **Parámetros**:
  - `installation`: Instalación a desinstalar
  - `createBackup`: Indica si se debe crear una copia de seguridad antes de desinstalar
  - `cleanupAfterUninstall`: Indica si se deben limpiar residuos después de la desinstalación
  - `whatIf`: Indica si se debe simular la desinstalación sin realizar cambios reales
  - `progress`: Objeto para reportar el progreso de la operación
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `BackupService` para crear copias de seguridad y `CleanupService` para limpiar residuos
- **Observaciones**: Este método es asíncrono para no bloquear la interfaz de usuario durante la desinstalación

##### ExecuteUninstallerAsync

- **Firma**: `private async Task<OperationResult> ExecuteUninstallerAsync(PhotoshopInstallation installation, bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Ejecuta el desinstalador nativo de Photoshop.
- **Parámetros**:
  - `installation`: Instalación a desinstalar
  - `whatIf`: Indica si se debe simular la desinstalación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `Process.Start` para ejecutar el desinstalador
- **Observaciones**: Maneja diferentes tipos de desinstaladores (MSI, EXE, etc.)

##### UninstallMsiProductAsync

- **Firma**: `private async Task<OperationResult> UninstallMsiProductAsync(string productCode, bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Desinstala un producto MSI utilizando msiexec.
- **Parámetros**:
  - `productCode`: Código del producto MSI a desinstalar
  - `whatIf`: Indica si se debe simular la desinstalación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `Process.Start` para ejecutar msiexec
- **Observaciones**: Específico para productos instalados mediante Windows Installer (MSI)

##### UninstallExeAsync

- **Firma**: `private async Task<OperationResult> UninstallExeAsync(string uninstallString, string? arguments, bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Ejecuta un desinstalador en formato EXE.
- **Parámetros**:
  - `uninstallString`: Ruta al desinstalador
  - `arguments`: Argumentos adicionales para el desinstalador
  - `whatIf`: Indica si se debe simular la desinstalación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `Process.Start` para ejecutar el desinstalador
- **Observaciones**: Utilizado para desinstaladores que no son MSI

##### WaitForUninstallerToCompleteAsync

- **Firma**: `private async Task<bool> WaitForUninstallerToCompleteAsync(Process process, int timeoutMinutes, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Espera a que el proceso de desinstalación termine.
- **Parámetros**:
  - `process`: Proceso del desinstalador
  - `timeoutMinutes`: Tiempo máximo de espera en minutos
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: `true` si el proceso terminó correctamente, `false` si se agotó el tiempo de espera
- **Relaciones**: Utilizado por `ExecuteUninstallerAsync`
- **Observaciones**: Implementa un mecanismo de timeout para evitar esperas infinitas

#### Estrategias de Desinstalación

El servicio implementa diferentes estrategias de desinstalación según el tipo de instalación:

1. **Desinstalación MSI**: Para productos instalados mediante Windows Installer, utilizando `msiexec.exe /x {ProductCode}`.

2. **Desinstalación EXE**: Para productos con desinstalador propio, ejecutando el desinstalador con los argumentos adecuados.

3. **Desinstalación Manual**: Para instalaciones sin desinstalador válido, eliminando manualmente archivos y entradas de registro.

El servicio selecciona automáticamente la estrategia más adecuada según la información disponible en la instalación detectada.

### Limpieza de Residuos

La limpieza de residuos se realiza a través del servicio `CleanupService`, que implementa métodos para eliminar archivos, carpetas y entradas de registro residuales de Photoshop.

#### Métodos Principales de CleanupService

##### CleanupAsync

- **Firma**: `public async Task<OperationResult> CleanupAsync(bool createBackup = true, bool whatIf = false, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default, List<PhotoshopInstallation>? installations = null)`
- **Propósito**: Método principal que coordina la limpieza de residuos de Photoshop.
- **Parámetros**:
  - `createBackup`: Indica si se debe crear una copia de seguridad antes de limpiar
  - `whatIf`: Indica si se debe simular la limpieza sin realizar cambios reales
  - `progress`: Objeto para reportar el progreso de la operación
  - `cancellationToken`: Token para cancelar la operación
  - `installations`: Lista opcional de instalaciones detectadas para limpiar sus claves de registro asociadas
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `BackupService` para crear copias de seguridad y `ProcessService` para detener procesos
- **Observaciones**: Este método es asíncrono para no bloquear la interfaz de usuario durante la limpieza

##### CleanupFilesAsync

- **Firma**: `private async Task<OperationResult> CleanupFilesAsync(bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Elimina archivos y carpetas residuales de Photoshop.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la limpieza
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `FileSystemHelper` para eliminar archivos y carpetas
- **Observaciones**: Implementa múltiples estrategias para manejar archivos bloqueados o persistentes. Se enfoca en eliminar archivos de configuración de usuario, cachés (fuentes, Camera Raw), logs, archivos temporales (`~PST*.tmp`), restos de plugins, y carpetas de instalación.

##### CleanupRegistryAsync

- **Firma**: `private async Task<OperationResult> CleanupRegistryAsync(bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken, List<PhotoshopInstallation>? installations = null)`
- **Propósito**: Elimina entradas de registro residuales de Photoshop.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la limpieza
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
  - `installations`: Lista opcional de instalaciones detectadas para limpiar sus claves de registro asociadas
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `RegistryHelper` para acceder al registro de Windows
- **Observaciones**: Implementa métodos alternativos (como `reg.exe`) cuando los métodos estándar fallan. Apunta a claves de configuración de usuario y máquina, claves de desinstalación huérfanas, y asociaciones de archivos obsoletas.

##### StopAdobeServicesAsync

- **Firma**: `private async Task<OperationResult> StopAdobeServicesAsync(bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Detiene servicios de Windows relacionados con Adobe.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la operación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `ServiceController` para controlar servicios de Windows
- **Observaciones**: Importante para asegurar que no hay servicios activos que bloqueen archivos

##### UninstallMsiProductsAsync

- **Firma**: `private async Task<OperationResult> UninstallMsiProductsAsync(bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Desinstala productos MSI relacionados con Adobe Photoshop.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la operación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza WMI para detectar productos MSI y `Process.Start` para ejecutar msiexec
- **Observaciones**: Útil para eliminar componentes instalados mediante Windows Installer

##### ProcessCommonFilesDirectoriesAsync

- **Firma**: `private async Task<OperationResult> ProcessCommonFilesDirectoriesAsync(bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Aplica métodos especializados para eliminar carpetas persistentes en Common Files.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la operación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `FileSystemHelper.ForceDeleteCommonFilesDirectory`
- **Observaciones**: Las carpetas en Common Files suelen ser más difíciles de eliminar y requieren métodos especiales

##### ScheduleFilesForDeletionAsync

- **Firma**: `private async Task<OperationResult> ScheduleFilesForDeletionAsync(bool whatIf, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)`
- **Propósito**: Programa la eliminación de archivos persistentes al reiniciar el sistema.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la operación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `FileSystemHelper.ScheduleFileForDeletion` y `FileSystemHelper.ScheduleDirectoryForDeletion`
- **Observaciones**: Utiliza la API nativa de Windows (`MoveFileEx` con `MOVEFILE_DELAY_UNTIL_REBOOT`) para programar la eliminación

#### Estrategias de Limpieza

El servicio implementa múltiples estrategias para manejar diferentes tipos de residuos:

1. **Limpieza de Archivos y Carpetas**:
   - Eliminación directa mediante `File.Delete` y `Directory.Delete`
   - Forzar la recolección de basura y reintentar
   - Utilizar API nativa de Windows (`DeleteFile`)
   - Programar eliminación al reinicio (`MoveFileEx` con `MOVEFILE_DELAY_UNTIL_REBOOT`)
   - Métodos especializados para carpetas en Common Files

2. **Limpieza de Registro**:
   - Eliminación mediante `Registry.DeleteKey` y `Registry.DeleteValue`
   - Utilizar `reg.exe delete` como método alternativo
   - Generar scripts de limpieza para casos complejos

3. **Limpieza de Servicios y Procesos**:
   - Detener procesos relacionados con Adobe
   - Detener servicios de Windows relacionados con Adobe
   - Desinstalar productos MSI relacionados con Adobe

### Gestión de Registro

La gestión del registro de Windows se realiza principalmente a través de la clase de utilidad `RegistryHelper`, que proporciona métodos para acceder, modificar y eliminar claves y valores del registro.

#### Métodos Principales de RegistryHelper

##### DeleteRegistryKey

- **Firma**: `public static bool DeleteRegistryKey(string keyPath, bool whatIf = false, bool useRegExe = false)`
- **Propósito**: Elimina una clave del registro de Windows.
- **Parámetros**:
  - `keyPath`: Ruta completa de la clave a eliminar
  - `whatIf`: Indica si se debe simular la eliminación
  - `useRegExe`: Indica si se debe utilizar reg.exe como método alternativo
- **Retorno**: `true` si la clave se eliminó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `Registry.DeleteSubKeyTree` o `ExecuteRegDelete` según el valor de `useRegExe`
- **Observaciones**: Implementa un mecanismo de fallback para utilizar reg.exe cuando los métodos estándar fallan

##### DeleteRegistryValue

- **Firma**: `public static bool DeleteRegistryValue(string keyPath, string valueName, bool whatIf = false, bool useRegExe = false)`
- **Propósito**: Elimina un valor específico de una clave del registro.
- **Parámetros**:
  - `keyPath`: Ruta de la clave que contiene el valor
  - `valueName`: Nombre del valor a eliminar
  - `whatIf`: Indica si se debe simular la eliminación
  - `useRegExe`: Indica si se debe utilizar reg.exe como método alternativo
- **Retorno**: `true` si el valor se eliminó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `Registry.DeleteValue` o `ExecuteRegDelete` según el valor de `useRegExe`
- **Observaciones**: Útil para eliminar valores específicos sin eliminar toda la clave

##### ExecuteRegDelete

- **Firma**: `public static bool ExecuteRegDelete(string keyPath, string? valueName = null, bool whatIf = false)`
- **Propósito**: Ejecuta el comando reg.exe para eliminar una clave o valor del registro.
- **Parámetros**:
  - `keyPath`: Ruta de la clave a eliminar
  - `valueName`: Nombre del valor a eliminar (opcional)
  - `whatIf`: Indica si se debe simular la eliminación
- **Retorno**: `true` si la operación se completó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `Process.Start` para ejecutar reg.exe
- **Observaciones**: Método alternativo cuando los métodos estándar de .NET fallan

##### GetRegistryKeyValue

- **Firma**: `public static object? GetRegistryKeyValue(string keyPath, string valueName, object? defaultValue = null)`
- **Propósito**: Obtiene el valor de una clave del registro.
- **Parámetros**:
  - `keyPath`: Ruta de la clave
  - `valueName`: Nombre del valor a obtener
  - `defaultValue`: Valor por defecto si no se encuentra
- **Retorno**: Valor obtenido o `defaultValue` si no se encuentra
- **Relaciones**: Utiliza `Registry.GetValue`
- **Observaciones**: Maneja excepciones internamente para evitar errores en caso de claves inexistentes

##### KeyExists

- **Firma**: `public static bool KeyExists(string keyPath)`
- **Propósito**: Verifica si una clave del registro existe.
- **Parámetros**:
  - `keyPath`: Ruta de la clave a verificar
- **Retorno**: `true` si la clave existe, `false` en caso contrario
- **Relaciones**: Utiliza `Registry.OpenSubKey`
- **Observaciones**: Útil para verificar la existencia de claves antes de intentar acceder a ellas

##### ValueExists

- **Firma**: `public static bool ValueExists(string keyPath, string valueName)`
- **Propósito**: Verifica si un valor específico existe en una clave del registro.
- **Parámetros**:
  - `keyPath`: Ruta de la clave
  - `valueName`: Nombre del valor a verificar
- **Retorno**: `true` si el valor existe, `false` en caso contrario
- **Relaciones**: Utiliza `Registry.GetValue`
- **Observaciones**: Útil para verificar la existencia de valores antes de intentar acceder a ellos

##### BackupRegistryKey

- **Firma**: `public static string? BackupRegistryKey(string keyPath, string backupDirectory)`
- **Propósito**: Crea una copia de seguridad de una clave del registro.
- **Parámetros**:
  - `keyPath`: Ruta de la clave a respaldar
  - `backupDirectory`: Directorio donde se guardará la copia de seguridad
- **Retorno**: Ruta del archivo de copia de seguridad o `null` si falla
- **Relaciones**: Utiliza `Process.Start` para ejecutar reg.exe export
- **Observaciones**: Crea un archivo .reg que puede ser importado posteriormente

##### RestoreRegistryKey

- **Firma**: `public static bool RestoreRegistryKey(string backupFilePath, bool whatIf = false)`
- **Propósito**: Restaura una clave del registro desde una copia de seguridad.
- **Parámetros**:
  - `backupFilePath`: Ruta del archivo de copia de seguridad
  - `whatIf`: Indica si se debe simular la restauración
- **Retorno**: `true` si la restauración se completó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `Process.Start` para ejecutar reg.exe import
- **Observaciones**: Importa un archivo .reg previamente exportado

#### Conversión de Rutas de Registro

La clase `RegistryHelper` también proporciona métodos para convertir rutas de registro entre diferentes formatos:

##### ConvertRegistryPathToRegFormat

- **Firma**: `public static string ConvertRegistryPathToRegFormat(string keyPath)`
- **Propósito**: Convierte una ruta de registro al formato utilizado por reg.exe.
- **Parámetros**:
  - `keyPath`: Ruta de registro en formato .NET
- **Retorno**: Ruta de registro en formato reg.exe
- **Observaciones**: Convierte, por ejemplo, "HKEY_LOCAL_MACHINE\\Software\\Adobe" a "HKLM\\Software\\Adobe"

##### GetRegistryHiveFromPath

- **Firma**: `public static RegistryHive? GetRegistryHiveFromPath(string keyPath, out string? subKeyPath)`
- **Propósito**: Extrae la raíz del registro y la subruta de una ruta completa.
- **Parámetros**:
  - `keyPath`: Ruta completa de la clave
  - `subKeyPath`: Variable de salida para la subruta
- **Retorno**: La raíz del registro correspondiente o `null` si no se reconoce
- **Observaciones**: Útil para abrir claves del registro con `Registry.OpenSubKey`

### Gestión de Archivos

La gestión de archivos se realiza principalmente a través de la clase de utilidad `FileSystemHelper`, que proporciona métodos avanzados para manipular archivos y carpetas, especialmente aquellos que son difíciles de eliminar.

#### Métodos Principales de FileSystemHelper

##### DeleteFileWithRetry

- **Firma**: `public static bool DeleteFileWithRetry(string filePath, int maxRetries = 3, bool whatIf = false)`
- **Propósito**: Elimina un archivo utilizando múltiples estrategias y reintentos.
- **Parámetros**:
  - `filePath`: Ruta del archivo a eliminar
  - `maxRetries`: Número máximo de reintentos
  - `whatIf`: Indica si se debe simular la eliminación
- **Retorno**: `true` si el archivo se eliminó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `File.Delete`, `GC.Collect` y `NativeMethods.DeleteFile`
- **Observaciones**: Implementa una estrategia progresiva con múltiples métodos de eliminación

##### DeleteDirectoryWithRetry

- **Firma**: `public static bool DeleteDirectoryWithRetry(string directoryPath, bool recursive = true, int maxRetries = 3, bool whatIf = false)`
- **Propósito**: Elimina una carpeta utilizando múltiples estrategias y reintentos.
- **Parámetros**:
  - `directoryPath`: Ruta de la carpeta a eliminar
  - `recursive`: Indica si se deben eliminar también los contenidos
  - `maxRetries`: Número máximo de reintentos
  - `whatIf`: Indica si se debe simular la eliminación
- **Retorno**: `true` si la carpeta se eliminó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `Directory.Delete`, `GC.Collect` y métodos nativos
- **Observaciones**: Útil para carpetas que contienen archivos bloqueados o en uso

##### ForceDeleteCommonFilesDirectory

- **Firma**: `public static bool ForceDeleteCommonFilesDirectory(string directoryPath, bool whatIf = false)`
- **Propósito**: Aplica métodos especializados para eliminar carpetas persistentes en Common Files.
- **Parámetros**:
  - `directoryPath`: Ruta de la carpeta a eliminar
  - `whatIf`: Indica si se debe simular la eliminación
- **Retorno**: `true` si la carpeta se eliminó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `DeleteDirectoryWithRetry` y `ScheduleDirectoryForDeletion`
- **Observaciones**: Las carpetas en Common Files suelen ser más difíciles de eliminar y requieren métodos especiales
- **Nota de seguridad**: Este método modifica los permisos del directorio para permitir su eliminación. Estos permisos no se revierten automáticamente si la eliminación falla, lo que podría dejar el directorio con permisos diferentes a los originales. Esto generalmente es de bajo riesgo ya que el objetivo es eliminar el directorio, pero debe tenerse en cuenta en caso de fallo.

##### ScheduleFileForDeletion

- **Firma**: `public static bool ScheduleFileForDeletion(string filePath, bool whatIf = false)`
- **Propósito**: Programa la eliminación de un archivo al reiniciar el sistema.
- **Parámetros**:
  - `filePath`: Ruta del archivo a eliminar
  - `whatIf`: Indica si se debe simular la operación
- **Retorno**: `true` si la operación se completó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `NativeMethods.MoveFileEx` con `MOVEFILE_DELAY_UNTIL_REBOOT`
- **Observaciones**: Útil para archivos que no se pueden eliminar por métodos convencionales

##### ScheduleDirectoryForDeletion

- **Firma**: `public static bool ScheduleDirectoryForDeletion(string directoryPath, bool whatIf = false)`
- **Propósito**: Programa la eliminación de una carpeta y su contenido al reiniciar el sistema.
- **Parámetros**:
  - `directoryPath`: Ruta de la carpeta a eliminar
  - `whatIf`: Indica si se debe simular la operación
- **Retorno**: `true` si la operación se completó correctamente, `false` en caso contrario
- **Relaciones**: Utiliza `ScheduleFileForDeletion` recursivamente
- **Observaciones**: Programa la eliminación de todos los archivos y subcarpetas

##### BackupFile

- **Firma**: `public static BackupItem? BackupFile(string filePath, string backupDirectory)`
- **Propósito**: Crea una copia de seguridad de un archivo.
- **Parámetros**:
  - `filePath`: Ruta del archivo a respaldar
  - `backupDirectory`: Directorio donde se guardará la copia de seguridad
- **Retorno**: Objeto `BackupItem` con información sobre la copia de seguridad o `null` si falla
- **Relaciones**: Utilizado por `BackupService`
- **Observaciones**: Mantiene la estructura de carpetas relativa en el directorio de respaldo

##### BackupDirectory

- **Firma**: `public static BackupItem? BackupDirectory(string directoryPath, string backupDirectory)`
- **Propósito**: Crea una copia de seguridad de una carpeta y su contenido.
- **Parámetros**:
  - `directoryPath`: Ruta de la carpeta a respaldar
  - `backupDirectory`: Directorio donde se guardará la copia de seguridad
- **Retorno**: Objeto `BackupItem` con información sobre la copia de seguridad o `null` si falla
- **Relaciones**: Utilizado por `BackupService`
- **Observaciones**: Copia recursivamente todos los archivos y subcarpetas

##### RestoreBackup

- **Firma**: `public static bool RestoreBackup(BackupItem backupItem)`
- **Propósito**: Restaura un archivo o carpeta desde una copia de seguridad.
- **Parámetros**:
  - `backupItem`: Objeto que contiene información sobre la copia de seguridad
- **Retorno**: `true` si la restauración se completó correctamente, `false` en caso contrario
- **Relaciones**: Utilizado por `BackupService.RestoreBackupAsync`
- **Observaciones**: Restaura el elemento a su ubicación original

#### Métodos Nativos de Windows

La clase `FileSystemHelper` utiliza métodos nativos de Windows a través de la clase interna `NativeMethods` para operaciones avanzadas:

##### DeleteFile (Nativo)

- **Firma**: `[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool DeleteFile(string lpFileName)`
- **Propósito**: Elimina un archivo utilizando la API nativa de Windows.
- **Parámetros**:
  - `lpFileName`: Ruta del archivo a eliminar
- **Retorno**: `true` si el archivo se eliminó correctamente, `false` en caso contrario
- **Observaciones**: Puede tener éxito en casos donde `File.Delete` falla

##### MoveFileEx (Nativo)

- **Firma**: `[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, MoveFileFlags dwFlags)`
- **Propósito**: Mueve o elimina un archivo con opciones avanzadas.
- **Parámetros**:
  - `lpExistingFileName`: Ruta del archivo existente
  - `lpNewFileName`: Nueva ruta del archivo (o `null` para eliminar)
  - `dwFlags`: Opciones de la operación
- **Retorno**: `true` si la operación se completó correctamente, `false` en caso contrario
- **Observaciones**: Utilizado principalmente con `MOVEFILE_DELAY_UNTIL_REBOOT` para programar eliminaciones al reinicio

### Copias de Seguridad

La gestión de copias de seguridad se realiza a través del servicio `BackupService`, que proporciona métodos para crear y restaurar copias de seguridad de archivos, carpetas y claves de registro antes de realizar operaciones potencialmente destructivas.

#### Métodos Principales de BackupService

##### CreateBackupAsync

- **Firma**: `public async Task<OperationResult> CreateBackupAsync(PhotoshopInstallation installation, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)`
- **Propósito**: Crea una copia de seguridad de una instalación de Photoshop.
- **Parámetros**:
  - `installation`: Instalación a respaldar
  - `progress`: Objeto para reportar el progreso de la operación
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `FileSystemHelper.BackupFile`, `FileSystemHelper.BackupDirectory` y `RegistryHelper.BackupRegistryKey`
- **Observaciones**: Este método es asíncrono para no bloquear la interfaz de usuario durante la creación de la copia de seguridad

##### CreateBackupForCleanupAsync

- **Firma**: `public async Task<OperationResult> CreateBackupForCleanupAsync(List<string> filePatterns, List<RegistryPattern> registryPatterns, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)`
- **Propósito**: Crea una copia de seguridad para una operación de limpieza.
- **Parámetros**:
  - `filePatterns`: Patrones de archivos a respaldar
  - `registryPatterns`: Patrones de registro a respaldar
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `FileSystemHelper` y `RegistryHelper`
- **Observaciones**: Respalda archivos y claves de registro que coinciden con los patrones especificados

##### RestoreBackupAsync

- **Firma**: `public async Task<OperationResult> RestoreBackupAsync(string backupPath, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)`
- **Propósito**: Restaura una copia de seguridad.
- **Parámetros**:
  - `backupPath`: Ruta de la copia de seguridad a restaurar
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utiliza `FileSystemHelper.RestoreBackup` y `RegistryHelper.RestoreRegistryKey`
- **Observaciones**: Restaura archivos, carpetas y claves de registro a su estado original

##### GetAvailableBackups

- **Firma**: `public List<BackupInfo> GetAvailableBackups()`
- **Propósito**: Obtiene una lista de copias de seguridad disponibles.
- **Retorno**: Lista de información sobre copias de seguridad disponibles
- **Relaciones**: Utilizado por la interfaz para mostrar copias de seguridad disponibles
- **Observaciones**: Cada `BackupInfo` contiene metadatos sobre la copia de seguridad (fecha, tipo, elementos)

##### DeleteBackup

- **Firma**: `public bool DeleteBackup(string backupPath)`
- **Propósito**: Elimina una copia de seguridad.
- **Parámetros**:
  - `backupPath`: Ruta de la copia de seguridad a eliminar
- **Retorno**: `true` si la copia de seguridad se eliminó correctamente, `false` en caso contrario
- **Relaciones**: Utilizado para limpiar copias de seguridad antiguas o no necesarias
- **Observaciones**: Elimina el directorio de la copia de seguridad y sus metadatos

#### Estructura de las Copias de Seguridad

Las copias de seguridad se organizan en una estructura jerárquica:

1. **Directorio Principal**: Ubicado en el escritorio o en una ruta especificada, con formato `PhotoshopBackups/yyyyMMdd_HHmmss`

2. **Subdirectorios**:
   - `Files`: Contiene copias de seguridad de archivos
   - `Directories`: Contiene copias de seguridad de carpetas
   - `Registry`: Contiene copias de seguridad de claves de registro (archivos .reg)
   - `Metadata`: Contiene información sobre la copia de seguridad (elementos respaldados, fecha, etc.)

3. **Archivo de Metadatos**: `backup_info.json` que contiene:
   - Fecha y hora de la copia de seguridad
   - Tipo de operación (desinstalación o limpieza)
   - Lista de elementos respaldados
   - Información adicional sobre la instalación (si aplica)

#### Clases de Modelo Relacionadas

##### BackupItem

- **Propósito**: Representa un elemento respaldado (archivo, carpeta o clave de registro).
- **Propiedades principales**:
  - `OriginalPath`: Ruta original del elemento
  - `BackupPath`: Ruta donde se ha guardado la copia de seguridad
  - `ItemType`: Tipo de elemento (archivo, carpeta, clave de registro)
  - `Description`: Descripción legible del elemento
  - `BackupTime`: Fecha y hora de la copia de seguridad
  - `IsRestored`: Indica si el elemento ha sido restaurado
  - `RestoreTime`: Fecha y hora de la restauración (si aplica)

##### BackupInfo

- **Propósito**: Contiene información general sobre una copia de seguridad.
- **Propiedades principales**:
  - `BackupPath`: Ruta de la copia de seguridad
  - `CreationTime`: Fecha y hora de creación
  - `OperationType`: Tipo de operación (desinstalación o limpieza)
  - `InstallationName`: Nombre de la instalación (si aplica)
  - `ItemCount`: Número de elementos respaldados
  - `Description`: Descripción legible de la copia de seguridad

### Interfaz Gráfica

La interfaz gráfica se implementa principalmente en la clase `MainForm`, que coordina la interacción entre el usuario y los servicios de la capa de negocio.

#### Métodos Principales de MainForm

##### RunOperationAsync

- **Firma**: `private async Task RunOperationAsync(Func<IProgress<ProgressInfo>, CancellationToken, Task<OperationResult>> operation)`
- **Propósito**: Método genérico para ejecutar operaciones asíncronas manteniendo la interfaz responsiva.
- **Parámetros**:
  - `operation`: Función que representa la operación a ejecutar
- **Relaciones**: Utilizado por todos los métodos que ejecutan operaciones largas
- **Observaciones**: Implementa un patrón común para manejar operaciones asíncronas, progreso, cancelación y errores

##### UpdateProgress

- **Firma**: `private void UpdateProgress(ProgressInfo progressInfo)`
- **Propósito**: Actualiza la interfaz con información de progreso.
- **Parámetros**:
  - `progressInfo`: Información sobre el progreso actual
- **Relaciones**: Utilizado por `RunOperationAsync` a través de `Progress<ProgressInfo>`
- **Observaciones**: Actualiza la barra de progreso y la etiqueta de estado según el progreso reportado

##### StartProgressAnimation

- **Firma**: `private void StartProgressAnimation(string operation)`
- **Propósito**: Inicia una animación de texto para indicar que la aplicación está trabajando.
- **Parámetros**:
  - `operation`: Nombre de la operación en curso
- **Relaciones**: Utilizado durante la fase inicial (0%) de operaciones largas
- **Observaciones**: Proporciona retroalimentación visual durante operaciones que comienzan en 0%

##### AnimationTimer_Tick

- **Firma**: `private void AnimationTimer_Tick(object? sender, EventArgs e)`
- **Propósito**: Actualiza la animación de texto en cada tick del temporizador.
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Llamado por el temporizador de animación
- **Observaciones**: Cambia el texto de la etiqueta de progreso para crear un efecto de animación

##### PrepareUIForOperation

- **Firma**: `private void PrepareUIForOperation()`
- **Propósito**: Prepara la interfaz para una operación larga.
- **Relaciones**: Llamado al inicio de `RunOperationAsync`
- **Observaciones**: Deshabilita botones, muestra la barra de progreso y habilita el botón de cancelación

##### RestoreUI

- **Firma**: `private void RestoreUI()`
- **Propósito**: Restaura la interfaz después de una operación.
- **Relaciones**: Llamado al final de `RunOperationAsync`
- **Observaciones**: Habilita botones, oculta la barra de progreso y deshabilita el botón de cancelación

##### UpdateButtonsState

- **Firma**: `private void UpdateButtonsState()`
- **Propósito**: Actualiza el estado de los botones según las instalaciones detectadas.
- **Relaciones**: Llamado después de detectar instalaciones y después de operaciones
- **Observaciones**: Implementa la lógica condicional para habilitar/deshabilitar botones

##### BtnDetect_Click

- **Firma**: `private void BtnDetect_Click(object sender, EventArgs e)`
- **Propósito**: Maneja el evento de clic en el botón "Detectar Instalaciones".
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Llama a `DetectionService.DetectInstallationsAsync`
- **Observaciones**: Inicia el proceso de detección de instalaciones

##### BtnUninstall_Click

- **Firma**: `private void BtnUninstall_Click(object sender, EventArgs e)`
- **Propósito**: Maneja el evento de clic en el botón "Desinstalar".
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Llama a `UninstallService.UninstallAsync`
- **Observaciones**: Verifica privilegios de administrador y solicita confirmación mediante `CustomMsgBox.Show()` antes de desinstalar

##### BtnCleanup_Click

- **Firma**: `private void BtnCleanup_Click(object sender, EventArgs e)`
- **Propósito**: Maneja el evento de clic en el botón "Limpiar Residuos".
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Llama a `CleanupService.CleanupAsync`
- **Observaciones**: Verifica privilegios de administrador y solicita confirmación mediante `CustomMsgBox.Show()` antes de limpiar

##### BtnTestMode_Click

- **Firma**: `private void BtnTestMode_Click(object sender, EventArgs e)`
- **Propósito**: Maneja el evento de clic en el botón "Modo de Prueba".
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Muestra `TestModeOptionsForm` y ejecuta la operación seleccionada en modo de prueba
- **Observaciones**: Permite simular operaciones sin realizar cambios reales

##### BtnCancel_Click

- **Firma**: `private void BtnCancel_Click(object sender, EventArgs e)`
- **Propósito**: Maneja el evento de clic en el botón "Cancelar".
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Cancela la operación en curso
- **Observaciones**: Cancela el token de cancelación para detener la operación actual

##### BtnRestore_Click

- **Firma**: `private void BtnRestore_Click(object sender, EventArgs e)`
- **Propósito**: Maneja el evento de clic en el botón "Restaurar".
- **Parámetros**:
  - `sender`: Objeto que generó el evento
  - `e`: Argumentos del evento
- **Relaciones**: Llama a `BackupService.RestoreBackupAsync`
- **Observaciones**: Muestra un formulario para seleccionar la copia de seguridad a restaurar y utiliza `CustomMsgBox.Show()` para mostrar mensajes informativos o de error

##### AppendToConsole

- **Firma**: `private void AppendToConsole(string message, Color color)`
- **Propósito**: Añade un mensaje a la consola de salida con un color específico.
- **Parámetros**:
  - `message`: Mensaje a añadir
  - `color`: Color del texto
- **Relaciones**: Utilizado por los métodos de logging
- **Observaciones**: Formatea el mensaje con la hora actual y lo añade a la consola

##### LogInfo, LogSuccess, LogWarning, LogError, LogDebug

- **Propósito**: Métodos para registrar diferentes tipos de mensajes en la consola.
- **Parámetros**:
  - `message`: Mensaje a registrar
- **Relaciones**: Utilizan `AppendToConsole` con diferentes colores
- **Observaciones**: Proporcionan una forma consistente de mostrar mensajes en la consola

#### Formularios Adicionales

##### TestModeOptionsForm

- **Propósito**: Permite seleccionar el tipo de operación a simular en modo de prueba.
- **Métodos principales**:
  - `InitializeComponent`: Inicializa los controles del formulario
  - `rbDetectOnly_CheckedChanged`, `rbSimulateUninstall_CheckedChanged`, `rbSimulateCleanup_CheckedChanged`: Actualizan la operación seleccionada
  - `btnOK_Click`, `btnCancel_Click`: Manejan los eventos de clic en los botones OK y Cancelar
- **Propiedades**:
  - `SelectedOperation`: Operación seleccionada por el usuario

##### UninstallOptionsForm

- **Propósito**: Permite configurar opciones para la desinstalación.
- **Métodos principales**:
  - `InitializeComponent`: Inicializa los controles del formulario
  - `btnOK_Click`, `btnCancel_Click`: Manejan los eventos de clic en los botones OK y Cancelar
- **Propiedades**:
  - `CreateBackup`: Indica si se debe crear una copia de seguridad
  - `CleanupAfterUninstall`: Indica si se deben limpiar residuos después de la desinstalación

##### CleanupOptionsForm

- **Propósito**: Permite configurar opciones para la limpieza de residuos.
- **Métodos principales**:
  - `InitializeComponent`: Inicializa los controles del formulario
  - `btnOK_Click`, `btnCancel_Click`: Manejan los eventos de clic en los botones OK y Cancelar
- **Propiedades**:
  - `CreateBackup`: Indica si se debe crear una copia de seguridad

##### RestoreBackupForm

- **Propósito**: Permite seleccionar una copia de seguridad para restaurar.
- **Métodos principales**:
  - `InitializeComponent`: Inicializa los controles del formulario
  - `LoadBackups`: Carga la lista de copias de seguridad disponibles
  - `lstBackups_SelectedIndexChanged`: Actualiza la información mostrada cuando cambia la selección
  - `btnRestore_Click`, `btnCancel_Click`: Manejan los eventos de clic en los botones Restaurar y Cancelar
- **Propiedades**:
  - `SelectedBackup`: Ruta de la copia de seguridad seleccionada

### Utilidades

Además de los servicios y clases principales, el proyecto incluye varias clases de utilidad que proporcionan funcionalidad común utilizada en diferentes partes de la aplicación.

#### AdminHelper

La clase `AdminHelper` proporciona métodos para verificar y solicitar privilegios de administrador.

##### IsRunningAsAdmin

- **Firma**: `public static bool IsRunningAsAdmin()`
- **Propósito**: Verifica si la aplicación se está ejecutando con privilegios de administrador.
- **Retorno**: `true` si se está ejecutando como administrador, `false` en caso contrario
- **Relaciones**: Utilizado antes de realizar operaciones que requieren privilegios elevados
- **Observaciones**: Utiliza `WindowsIdentity` y `WindowsPrincipal` para verificar los roles

##### RestartAsAdmin

- **Firma**: `public static bool RestartAsAdmin(string arguments = "")`
- **Propósito**: Reinicia la aplicación con privilegios de administrador.
- **Parámetros**:
  - `arguments`: Argumentos adicionales para pasar a la nueva instancia
- **Retorno**: `true` si se inició el proceso de reinicio, `false` en caso contrario
- **Relaciones**: Utilizado cuando se detecta que se necesitan privilegios de administrador
- **Observaciones**: Utiliza `ProcessStartInfo` con `UseShellExecute = true` y `Verb = "runas"`

##### EnsureAdminPrivileges

- **Firma**: `public static bool EnsureAdminPrivileges(string message = "Esta operación requiere privilegios de administrador. ¿Desea reiniciar la aplicación con privilegios elevados?")`
- **Propósito**: Verifica si la aplicación tiene privilegios de administrador y, si no los tiene, solicita elevación.
- **Parámetros**:
  - `message`: Mensaje a mostrar al usuario
- **Retorno**: `true` si la aplicación ya tenía o ha obtenido privilegios de administrador, `false` en caso contrario
- **Relaciones**: Combina `IsRunningAsAdmin` y `RestartAsAdmin`
- **Observaciones**: Muestra un cuadro de diálogo para confirmar la elevación de privilegios

#### LoggingService

La clase `LoggingService` proporciona métodos para registrar mensajes en archivos de log y en la consola de la aplicación.

##### LogInfo, LogSuccess, LogWarning, LogError, LogDebug

- **Propósito**: Métodos para registrar diferentes tipos de mensajes.
- **Parámetros**:
  - `message`: Mensaje a registrar
- **Relaciones**: Utilizados por todos los servicios para registrar información
- **Observaciones**: Registran en archivo y, opcionalmente, en la consola de la aplicación

##### GetLogFilePath

- **Firma**: `public string GetLogFilePath()`
- **Propósito**: Obtiene la ruta del archivo de log actual.
- **Retorno**: Ruta completa del archivo de log
- **Relaciones**: Utilizado para abrir el archivo de log
- **Observaciones**: El archivo de log se crea en una carpeta específica con un nombre basado en la fecha

##### SetConsoleCallback

- **Firma**: `public void SetConsoleCallback(Action<string, Color> callback)`
- **Propósito**: Establece una función de callback para mostrar mensajes en la consola de la aplicación.
- **Parámetros**:
  - `callback`: Función que recibe un mensaje y un color
- **Relaciones**: Utilizado por `MainForm` para conectar el servicio de logging con la consola
- **Observaciones**: Permite que los mensajes de log se muestren en la interfaz gráfica

#### ProcessService

La clase `ProcessService` proporciona métodos para gestionar procesos relacionados con Adobe.

##### StopAdobeProcessesAsync

- **Firma**: `public async Task<OperationResult> StopAdobeProcessesAsync(bool whatIf = false, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)`
- **Propósito**: Detiene todos los procesos relacionados con Adobe Photoshop.
- **Parámetros**:
  - `whatIf`: Indica si se debe simular la operación
  - `progress`: Objeto para reportar progreso
  - `cancellationToken`: Token para cancelar la operación
- **Retorno**: Resultado de la operación
- **Relaciones**: Utilizado por `CleanupService` antes de intentar eliminar archivos
- **Observaciones**: Importante para evitar que archivos estén bloqueados por procesos en ejecución

##### GetRunningAdobeProcesses

- **Firma**: `public List<Process> GetRunningAdobeProcesses()`
- **Propósito**: Obtiene una lista de procesos de Adobe en ejecución.
- **Retorno**: Lista de procesos de Adobe en ejecución
- **Relaciones**: Utilizado por `StopAdobeProcessesAsync`
- **Observaciones**: Filtra los procesos por nombre para identificar los relacionados con Adobe

#### ScriptGenerator

La clase `ScriptGenerator` proporciona métodos para generar scripts de limpieza.

##### GenerateCleanupScript

- **Firma**: `public static bool GenerateCleanupScript(List<string> regDeleteCommands, string outputPath, ScriptType scriptType)`
- **Propósito**: Genera un script de limpieza de registro.
- **Parámetros**:
  - `regDeleteCommands`: Lista de comandos `reg delete`
  - `outputPath`: Ruta donde guardar el script
  - `scriptType`: Tipo de script (BAT o PS1)
- **Retorno**: `true` si el script se generó correctamente, `false` en caso contrario
- **Relaciones**: Utilizado por `MainForm.BtnGenerarScript_Click`
- **Observaciones**: Genera scripts en formato .bat (CMD) o .ps1 (PowerShell)

##### ExtractRegDeleteCommands

- **Firma**: `public static List<string> ExtractRegDeleteCommands(string consoleText)`
- **Propósito**: Extrae comandos `reg delete` del texto de la consola.
- **Parámetros**:
  - `consoleText`: Texto de la consola
- **Retorno**: Lista de comandos `reg delete` encontrados
- **Relaciones**: Utilizado por `MainForm.BtnGenerarScript_Click`
- **Observaciones**: Utiliza expresiones regulares para identificar comandos `reg delete`

#### Otras Utilidades

##### StringExtensions

- **Propósito**: Proporciona métodos de extensión para manipular cadenas.
- **Métodos principales**:
  - `ContainsAny`: Verifica si una cadena contiene alguna de las subcadenas especificadas
  - `ReplaceFirst`: Reemplaza solo la primera ocurrencia de una subcadena
  - `ToSafeFileName`: Convierte una cadena en un nombre de archivo seguro

##### PathHelper

- **Propósito**: Proporciona métodos para manipular rutas de archivo.
- **Métodos principales**:
  - `GetCommonProgramFilesPath`: Obtiene la ruta de Common Files
  - `GetProgramFilesPath`: Obtiene la ruta de Program Files
  - `GetAppDataPath`: Obtiene la ruta de AppData
  - `EnsureDirectoryExists`: Asegura que un directorio existe, creándolo si es necesario

## Formato de Documentación de Métodos

Para cada método clave, se proporcionará la siguiente información:

- **Firma**: Declaración completa del método
- **Clase**: Clase a la que pertenece el método
- **Propósito**: Descripción del propósito del método
- **Parámetros**: Descripción de los parámetros de entrada
- **Retorno**: Descripción del valor de retorno
- **Relaciones**: Relaciones con otros métodos o componentes
- **Observaciones/Advertencias**: Notas importantes o advertencias sobre el uso del método
