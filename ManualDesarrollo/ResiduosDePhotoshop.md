# Informe Consolidado sobre Rastros y Residuos de Adobe Photoshop en Windows

## Introducción

Adobe Photoshop, en sus diversas iteraciones desde CS6 hasta las versiones más recientes de Creative Cloud (CC), deja una huella considerable en los sistemas operativos Windows (desde Windows 7 en adelante). Esta huella se manifiesta en forma de archivos de instalación, configuraciones de usuario, archivos temporales y de caché, entradas en el Registro de Windows, logs de diagnóstico, asociaciones de archivos y otros artefactos que pueden persistir incluso después de una desinstalación estándar. La comprensión detallada de estos rastros es fundamental para diversos propósitos, incluyendo análisis forenses digitales, optimización del rendimiento del sistema mediante la eliminación de archivos innecesarios y la garantía de una eliminación completa del software cuando sea requerido.

## Versiones de Photoshop y Compatibilidad con Windows

Este informe abarca Adobe Photoshop CS6 (compatible con Windows 7 SP1, Windows 8/8.1) y todas las versiones de Creative Cloud (CC) compatibles con Windows 7 SP1 y posteriores. Versiones más recientes de Photoshop (ej. Photoshop 2023 en adelante) requieren Windows 10 (versión 20H2 o posterior) o Windows 11. El análisis se centra en las ubicaciones y tipos de rastros comunes que persisten a través de estas versiones.

## Rastros en el Sistema de Archivos

Photoshop genera y almacena una multitud de archivos en diversas ubicaciones del sistema.

### 1. Directorios de Instalación
Contienen los archivos binarios principales de la aplicación (ejecutables, DLLs).
*   **Ruta principal:** `C:\Program Files\Adobe\Adobe Photoshop [versión]`
*   **Para versiones de 32 bits o antiguas en sistemas de 64 bits:** `C:\Program Files (x86)\Adobe\Adobe Photoshop [versión]`

### 2. Configuraciones y Preferencias de Usuario
Almacenan personalizaciones como espacios de trabajo, preferencias generales, historial de archivos recientes y presets.
*   **Ruta principal:** `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Adobe Photoshop [versión] Settings\`
    *   **Ejemplo de archivo de preferencias:** `Adobe Photoshop [versión] Prefs.psp` (o `Adobe Photoshop X64 CS6 Prefs.psp` para CS6).
*   **Presets (pinceles, patrones, estilos, acciones):** `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Presets\`
    *   **Archivos de acciones:** Subcarpeta `Actions\` (archivos `.atn`), y `Actions Palette.psp` en la carpeta `Settings`.
*   **Otros archivos de configuración (ej. Camera Raw):**
    *   `%AppData%\Adobe\CameraRaw\` (ajustes y presets)
    *   `%AppData%\Adobe\Bridge <versión>\` (caché de miniaturas y colecciones de Adobe Bridge)

### 3. Archivos Temporales
Generados durante la operación de Photoshop para mejorar el rendimiento y gestionar la memoria.
*   **Ubicación general:** `C:\Users\[username]\AppData\Local\Temp\` (o `%LOCALAPPDATA%\Temp\`)
    *   **Nomenclatura común:** `Photoshop Temp*` o `~PST####.tmp`. Estos archivos pueden ocupar gigabytes y, aunque deberían eliminarse al cerrar Photoshop, a menudo persisten si la aplicación se cierra incorrectamente.
*   **Archivos de disco de trabajo (scratch disks):** Son archivos `Photoshop Temp*` creados en la raíz del disco configurado en las preferencias de Photoshop cuando se necesita más memoria que la RAM disponible.
*   **Archivos de AutoRecuperación (Autoguardado):** Para recuperar trabajos no guardados tras un cierre inesperado.
    *   **Ubicación:** `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\AutoRecover\` (o `C:\Users\[user name]\AppData\Roaming\Adobe\Adobe Photoshop\AutoRecover` para versiones más recientes). Los archivos suelen ser `.psb`.

### 4. Archivos de Caché
Para acelerar el acceso a recursos utilizados frecuentemente.
*   **Caché de fuentes:** `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\CT Font Cache`
*   **Caché de miniaturas (Bridge/Photoshop):**
    *   `C:\Users\[User Name]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Adobe Photoshop [versión] Settings\MachinePrefs.psp` (almacena miniaturas de archivos recientes de Photoshop).
    *   `%AppData%\Adobe\Bridge <versión>\Cache\` (para Adobe Bridge).
    *   Windows puede generar `thumbs.db` en carpetas con imágenes.

### 5. Archivos de Registro (Logs) y Reportes de Errores
Útiles para depuración, análisis forense y resolución de problemas.
*   **Logs específicos de Photoshop:**
    *   `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Logs\`
    *   `C:\Users\[User]\AppData\Roaming\Adobe\Adobe Photoshop [Version]\LogFiles\` (versiones más recientes)
    *   **Photoshop Event Logger (ej. Photoshop 2024):** `C:\Users\[User Name]\AppData\Roaming\Adobe\Adobe Photoshop 2024\Logs\`
*   **Logs generales de Adobe:** `C:\Program Files\Common Files\Adobe\Logs\`
*   **Logs de instalación/desinstalación:**
    *   `C:\Program Files (x86)\Common Files\Adobe\Installers\` (Windows 64-bit)
    *   `C:\Program Files\Common Files\Adobe\Installers\` (Windows 32-bit)
    *   **Nomenclatura:** `install.log` o `<Nombre del Producto> <Versión> <Fecha>.log.gz`.
*   **Reportes de errores (volcado de memoria / crash dumps):**
    *   `C:\Users\[username]\AppData\Local\Temp\`
    *   `%LOCALAPPDATA%\CrashDumps\` (ej. `Photoshop.exe.dmp`)
    *   Nivel de sistema: `C:\Windows\MEMORY.DMP` (completo), `C:\Windows\minidump\` (mini volcados).
*   **Logs de Creative Cloud (ej. problemas de inicio):** `amt3.log` en `%LOCALAPPDATA%\Temp`.

### 6. Archivos Comunes de Adobe y Componentes Compartidos
Utilizados por múltiples aplicaciones de Adobe.
*   **Ubicación principal:** `C:\Program Files\Common Files\Adobe\` (y su contraparte `(x86)`)
    *   Incluye perfiles de color, componentes de licenciamiento, etc.
*   **Archivos de instalación inicial (Out Of Box Experience - OOBE):** `%LocalAppData%\Adobe\OOBE\` (a menudo persisten).
*   **Configuraciones persistentes de Adobe:** `C:\ProgramData\Adobe\OperatingConfigs\`
*   **Datos de licencias Adobe:** `C:\ProgramData\Adobe\SLStore_v1\`

### 7. Plugins y Extensiones (CEP y UXP)
Para extender la funcionalidad de Photoshop.
*   **CEP (Common Extensibility Platform - legacy):**
    *   **Nivel de sistema:**
        *   `C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\`
        *   `C:\Program Files\Common Files\Adobe\CEP\extensions\`
    *   **Nivel de usuario:** `%AppData%\Adobe\CEP\extensions\`
*   **UXP (Unified Extensibility Platform - moderna, PS 2021+):**
    *   **Nivel de sistema (instalados globalmente):** `C:\Program Files\Common Files\Adobe\UXP\Plugins\External\`
    *   **Nivel de usuario (instalados desde app CC):** `%AppData%\Adobe\UXP\Plugins\External\`
    *   **Datos de configuración de plugins UXP:** `%AppData%\Adobe\UXP\PluginsStorage\PHSP\[versión_numérica_photoshop]\External\[PluginID]\` (contiene subcarpetas como `PluginData` y `SettingsStorage`).

### 8. Accesos Directos
Iconos para lanzar la aplicación.
*   **Escritorio:** `C:\Users\[username]\Desktop\`
*   **Menú de Inicio:** `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Adobe Photoshop [versión]\`

### 9. Archivos de Creative Cloud
Para usuarios de Creative Cloud, se almacenan archivos sincronizados localmente.
*   **Ubicación:** `C:\Users\[User name]\Creative Cloud Files\`

## Rastros en el Registro de Windows

Photoshop crea y modifica numerosas claves en el Registro de Windows para configuraciones, licencias, asociaciones de archivos y desinstalación.

| Categoría                     | Ruta (Ejemplos principales)                                                                                                 | Explicación                                                                                                |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| **Configuraciones de Aplicación (Nivel Máquina)** | `HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\[versión_numérica]`                                            | Detalles de instalación, licencias (a veces cifradas), configuraciones globales.                           |
|                               | `HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Adobe\Photoshop\[versión_numérica]`                                                | Para versiones de 32 bits en sistemas de 64 bits.                                                          |
| **Configuraciones de Usuario**  | `HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\[versión_numérica]`                                                             | Preferencias específicas del usuario (ej. `VisitedDirs`, guías, historial de carpetas).                    |
|                               | `HKEY_CURRENT_USER\SOFTWARE\Adobe\Camera Raw\[versión_numérica]`                                                            | Ajustes y preferencias de Adobe Camera Raw.                                                                |
|                               | `HKEY_CURRENT_USER\SOFTWARE\Adobe\Bridge\[versión_numérica]\Preferences\`                                                  | Preferencias de Adobe Bridge.                                                                              |
| **Información de Desinstalación** | `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\[GUID de Photoshop]`                              | Datos para el desinstalador de Windows, incluyendo el ProductCode único de la versión.                   |
| **Asociaciones de Archivos**    | `HKEY_CLASSES_ROOT\.psd` (y otras extensiones como `.jpg`, `.psb`, `.pdd`, `.abr`, `.atn`, `.pat`)                       | Asocia extensiones de archivo con Photoshop.                                                               |
|                               | `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.psd\`                                      | Preferencias de usuario para asociaciones de archivos.                                                      |
|                               | `HKEY_CLASSES_ROOT\Applications\Photoshop.exe\shell\open\command`                                                           | Comando para abrir archivos asociados.                                                                     |
| **Control de Características**  | Claves específicas pueden existir para bloquear o habilitar funciones (ej. `FeatureLockDown` en algunas versiones). | Control granular sobre funcionalidades.                                                                    |

## Servicios y Procesos en Segundo Plano

Photoshop en sí no instala servicios dedicados, pero las versiones CC dependen de procesos gestionados por la aplicación de escritorio Adobe Creative Cloud:
*   **Adobe Crash Processor (o Reportes de error de Adobe):** Reporta fallos de aplicaciones CC.
*   **Creative Cloud Core Service (o Servicio principal de Adobe Creative Cloud):** Gestiona licencias y actualizaciones.
*   **Creative Cloud Update Service (o Adobe Update Service):** Maneja privilegios para actualizaciones y sincronización.
*   **Otros servicios/procesos Adobe:** Adobe Genuine Software Service, Adobe CollabSync, AdobeGCInvoker, etc., relacionados con Creative Cloud y licenciamiento.
    *   Estos residen principalmente en `C:\Program Files\Adobe\Adobe Creative Cloud\` y pueden dejar rastros si Creative Cloud no se desinstala.

## Persistencia de Datos Tras la Desinstalación

Incluso después de una desinstalación estándar (Panel de Control o app Creative Cloud), muchos rastros permanecen:
*   **Carpetas de usuario:** Configuración en `%AppData%\Adobe\Adobe Photoshop [versión]\` y extensiones CEP/UXP.
*   **Carpetas compartidas/sistema:** `C:\ProgramData\Adobe\OperatingConfigs\`, `C:\ProgramData\Adobe\SLStore_v1\`, `%LocalAppData%\Adobe\OOBE\`.
*   **Claves de registro:** Especialmente en `HKEY_CURRENT_USER`, y algunas asociaciones de archivos.
*   La **Herramienta Adobe CC Cleaner** está diseñada para eliminar archivos, carpetas y claves residuales que podrían interferir con nuevas instalaciones. Sin embargo, incluso esta herramienta puede no eliminar todo, especialmente contenido creado por el usuario o ciertas carpetas como las mencionadas en `ProgramData` y `OOBE`.

## Patrones para Detectar Versiones Futuras

Adobe mantiene convenciones consistentes, facilitando la predicción de rastros:
*   **Directorios:** Los nombres incluyen el número de versión o año (ej. `Adobe Photoshop 2024`, `Adobe Photoshop 25.0`).
    *   Regex para carpetas de usuario: `^.*\\Adobe\\Adobe Photoshop (?:CS\d+|CC\s?\d{4}|[2-9]\d{3})\\`
    *   Regex para plugins UXP: `%APPDATA%\\Adobe\\UXP\\PluginsStorage\\PHSP\\\d+\\.*`
*   **Claves del Registro:** Reflejan el número de versión (ej. `HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\25.0`).
    *   Regex para claves de versión: `^HKCU\\Software\\Adobe\\Photoshop\\\d+\.\d+$`
*   **Nomenclatura de archivos temporales:** Sigue siendo `Photoshop Temp*` o `~PST*.tmp`.
*   **Asociaciones de archivos:** Extensiones como `.psd`, `.psb` seguirán siendo relevantes.
    *   Regex: `^HKCR\\\.ps[dpb]$`

## Consideraciones en Sistemas Multi-Usuario

En sistemas compartidos, la mayoría de los datos específicos del usuario (preferencias, caché, temporales, AutoRecover, presets, plugins UXP de usuario) se almacenan dentro del perfil de cada usuario (`C:\Users\[username]\...` y `HKEY_CURRENT_USER`). Las instalaciones, archivos comunes y algunas configuraciones globales son a nivel de máquina (`Program Files`, `ProgramData`, `HKLM`). Una limpieza o análisis exhaustivo requiere examinar cada perfil de usuario.

## Metodología de Investigación Utilizada para este Consolidado

La información se recopiló analizando y comparando varias investigaciones, que a su vez se basaron en:
1.  **Análisis de documentación oficial de Adobe, foros de soporte y artículos técnicos.**
2.  **Identificación de ubicaciones de archivos y registros a través de fuentes confiables y experiencia de usuario.**
3.  **Comparación entre versiones (CS6 vs. CC) para identificar patrones consistentes y variaciones.**
    Aunque no se realizó un análisis forense en tiempo real con herramientas como Process Monitor para este consolidado, las fuentes originales sugieren que dichas herramientas son valiosas para la identificación de estos rastros.

## Recomendaciones para la Detección y Eliminación Exhaustiva de Residuos

1.  **Copia de Seguridad:** Antes de cualquier eliminación manual o uso de herramientas, realizar una copia de seguridad del sistema y datos importantes.
2.  **Desinstalación Estándar:** Utilizar la aplicación Adobe Creative Cloud o el Panel de Control de Windows.
3.  **Herramienta Adobe CC Cleaner:**
    *   Descargar la versión más reciente desde el sitio oficial de Adobe.
    *   Ejecutar como administrador.
    *   Seleccionar específicamente la versión de Photoshop a limpiar.
    *   Puede requerir múltiples ejecuciones. Revisar el log generado.
4.  **Eliminación Manual (Post-Limpieza):**
    *   **Sistema de Archivos:** Revisar y eliminar las carpetas mencionadas en este informe (especialmente en `%AppData%`, `%LocalAppData%`, `ProgramData`, carpetas de plugins CEP/UXP, y archivos temporales).
    *   **Registro de Windows (`regedit`):** Con precaución (previa copia de seguridad del registro o creación de punto de restauración), buscar y eliminar las claves de Photoshop listadas, especialmente bajo `HKCU\Software\Adobe\Photoshop` y `HKLM\SOFTWARE\Adobe\Photoshop`. Revisar asociaciones de archivos.
5.  **Uso de Herramientas Adicionales (para detección y verificación):**
    *   **Process Monitor (Sysinternals):** Para rastrear accesos a archivos y registro en tiempo real.
    *   **Everything (VoidTools):** Para búsquedas rápidas de archivos por nombre en todo el sistema.
    *   **Regshot / Registry Workshop:** Para comparar instantáneas del registro antes y después de operaciones.
6.  **Verificación Final:** Reiniciar el sistema y verificar que los rastros hayan sido eliminados.

## Conclusión

Adobe Photoshop deja una extensa red de rastros en los sistemas Windows. Comprender las ubicaciones de archivos, las claves de registro, los procesos en segundo plano y los patrones de nomenclatura es crucial para la gestión eficaz de estos residuos. Aunque las herramientas de Adobe ayudan, la eliminación completa a menudo requiere una intervención manual y sistemática. Las versiones futuras probablemente seguirán patrones similares, lo que facilitará la predicción y gestión de sus rastros. Este informe consolidado proporciona una guía integral para identificar, analizar y eliminar los restos de Photoshop, contribuyendo a un sistema más limpio, optimizado y a un análisis forense más preciso.

---