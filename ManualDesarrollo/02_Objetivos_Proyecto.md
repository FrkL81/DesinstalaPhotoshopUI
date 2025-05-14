# Objetivos del Proyecto

## Objetivo Principal

El objetivo principal del proyecto DesinstalaPhotoshop es proporcionar una herramienta completa, eficiente y con interfaz gráfica intuitiva para la desinstalación total de Adobe Photoshop en sistemas Windows, eliminando no solo la instalación principal sino también todos los archivos residuales, entradas de registro y componentes asociados que normalmente permanecen en el sistema después de una desinstalación convencional.

## Subobjetivos Específicos

### 1. Detección Precisa de Instalaciones

- **Implementar un sistema de detección multicapa** que identifique instalaciones de Photoshop mediante:
  - Búsqueda en programas instalados (WMI/MSI)
  - Exploración del registro de Windows
  - Análisis del sistema de archivos

- **Diferenciar entre instalaciones principales y residuos** mediante un sistema de puntuación heurística que evalúe:
  - Presencia del ejecutable principal (`photoshop.exe`)
  - Existencia de desinstaladores válidos
  - Integridad de la instalación
  - Ubicación de los archivos (distinguiendo entre rutas de instalación principal y componentes compartidos)

- **Presentar información detallada** sobre cada instalación detectada, incluyendo:
  - Nombre y versión
  - Ubicación de instalación
  - Puntuación de confianza
  - Estado (instalación principal, posible instalación principal o residuos)

### 2. Eliminación Completa de Registros y Archivos

- **Desinstalar completamente** las instalaciones principales de Photoshop mediante:
  - Ejecución de desinstaladores nativos
  - Utilización de la API de Windows para desinstalación
  - Manejo de diferentes versiones de Photoshop (CS, CC, versiones específicas)

- **Eliminar archivos residuales** en múltiples ubicaciones:
  - Directorios de programa
  - Carpetas de datos de aplicación
  - Archivos temporales
  - Archivos de configuración
  - Componentes compartidos

- **Limpiar entradas de registro** relacionadas con Photoshop:
  - Claves de desinstalación
  - Configuraciones de usuario
  - Asociaciones de archivos
  - Componentes COM registrados
  - Extensiones de shell

#### Tipos de Residuos Específicos

La herramienta busca y elimina activamente los siguientes tipos de residuos de Adobe Photoshop, basándose en un análisis exhaustivo de versiones desde CS6 hasta las más recientes de Creative Cloud:

1. **Archivos de Instalación y Componentes Principales:**
   - Directorio principal: `C:\Program Files\Adobe\Adobe Photoshop [versión]` y su contraparte `(x86)`.

2. **Configuraciones y Preferencias de Usuario:**
   - Ruta principal: `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Adobe Photoshop [versión] Settings\` (incluye `Adobe Photoshop [versión] Prefs.psp`).
   - Presets (pinceles, acciones `.atn`, etc.): `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Presets\`
   - Configuración Camera Raw: `%AppData%\Adobe\CameraRaw\`
   - Configuración Adobe Bridge: `%AppData%\Adobe\Bridge <versión>\`

3. **Archivos Temporales:**
   - Ubicación general: `%LOCALAPPDATA%\Temp\` (buscar `Photoshop Temp*`, `~PST####.tmp`).
   - Archivos de disco de trabajo (scratch disks).
   - Archivos de AutoRecuperación: `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\AutoRecover\` (archivos `.psb`).

4. **Archivos de Caché:**
   - Caché de fuentes: `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\CT Font Cache`
   - Caché de miniaturas (Bridge/Photoshop): En `MachinePrefs.psp` y `%AppData%\Adobe\Bridge <versión>\Cache\`.

5. **Archivos de Registro (Logs) y Reportes de Errores:**
   - Logs específicos de Photoshop: `C:\Users\[username]\AppData\Roaming\Adobe\Adobe Photoshop [versión]\Logs\` (o `LogFiles` en versiones nuevas).
   - Logs generales de Adobe: `C:\Program Files\Common Files\Adobe\Logs\`
   - Logs de instalación/desinstalación: `C:\Program Files (x86)\Common Files\Adobe\Installers\`
   - Reportes de errores (crash dumps): `%LOCALAPPDATA%\CrashDumps\` (ej. `Photoshop.exe.dmp`), `%LOCALAPPDATA%\Temp`.
   - Logs de Creative Cloud (ej. `amt3.log`): `%LOCALAPPDATA%\Temp`.

6. **Archivos Comunes de Adobe y Componentes Compartidos:**
   - Ubicación principal: `C:\Program Files\Common Files\Adobe\` (y `(x86)`).
   - Archivos OOBE: `%LocalAppData%\Adobe\OOBE\`
   - Configuraciones persistentes: `C:\ProgramData\Adobe\OperatingConfigs\`
   - Datos de licencias: `C:\ProgramData\Adobe\SLStore_v1\`

7. **Plugins y Extensiones (CEP y UXP):**
   - CEP (legacy):
     - Nivel sistema: `C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\`
     - Nivel usuario: `%AppData%\Adobe\CEP\extensions\`
   - UXP (moderna, PS 2021+):
     - Nivel sistema: `C:\Program Files\Common Files\Adobe\UXP\Plugins\External\`
     - Nivel usuario: `%AppData%\Adobe\UXP\Plugins\External\`
     - Datos de configuración UXP: `%AppData%\Adobe\UXP\PluginsStorage\PHSP\[versión_numérica_photoshop]\External\[PluginID]\`

8. **Accesos Directos:** Escritorio y Menú de Inicio.

9. **Archivos de Creative Cloud Sync:** `C:\Users\[User name]\Creative Cloud Files\`

10. **Claves de Registro:**
    - Configuración máquina: `HKLM\SOFTWARE\Adobe\Photoshop\[versión_numérica]` (y `Wow6432Node`).
    - Configuración usuario: `HKCU\SOFTWARE\Adobe\Photoshop\[versión_numérica]`, `HKCU\SOFTWARE\Adobe\Camera Raw\[versión_numérica]`.
    - Información de desinstalación: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\[GUID de Photoshop]`.
    - Asociaciones de archivos: `HKCR\.psd`, `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.psd\`, `HKCR\Applications\Photoshop.exe\shell\open\command`.

11. **Servicios y Procesos en Segundo Plano (Relacionados con Creative Cloud):**
    - Adobe Crash Processor, Creative Cloud Core Service, Adobe Update Service, Adobe Genuine Software Service, etc. La herramienta se enfocará en detener procesos que bloqueen la eliminación de archivos de Photoshop, más que en desinstalar estos servicios de CC directamente, a menos que sean explícitamente residuos de Photoshop.

### 3. Uso Eficiente de `reg.exe` para Limpieza de Registro

- **Implementar métodos alternativos para la eliminación de claves de registro** cuando los métodos estándar fallan:
  - Utilizar `reg.exe` como mecanismo de respaldo
  - Convertir rutas de registro al formato adecuado para `reg.exe`
  - Capturar y procesar la salida de `reg.exe` para verificar el éxito de las operaciones

- **Generar comandos de limpieza de registro** para su ejecución manual o programada:
  - Crear scripts `.bat` o `.ps1` con comandos `reg delete`
  - Incluir encabezados y comentarios explicativos en los scripts generados
  - Formatear correctamente los comandos según el tipo de script

### 4. Sistema de Puntuación Heurística

- **Desarrollar un algoritmo de puntuación** para evaluar la naturaleza de las instalaciones detectadas:
  - Asignar puntos positivos por indicadores de instalación principal
  - Aplicar penalizaciones por indicadores de instalación corrupta o residual

- **Clasificar automáticamente** las instalaciones en tres categorías:
  - Instalación principal (alta confianza)
  - Posible instalación principal (confianza media)
  - Residuos (baja confianza)

> **Nota**: Para una descripción detallada del sistema de puntuación heurística, consulte el documento [Sistema_Puntuacion_Heuristica.md](Sistema_Puntuacion_Heuristica.md).

### 5. Generación de Scripts y Reportes

- **Crear scripts de limpieza** para situaciones donde la limpieza automática no es posible:
  - Extraer comandos `reg delete` de las operaciones realizadas
  - Generar scripts en formato `.bat` (CMD) o `.ps1` (PowerShell)
  - Incluir encabezados, comentarios y mensajes informativos

- **Generar reportes detallados** de las operaciones realizadas:
  - Listar instalaciones detectadas con sus características
  - Documentar acciones realizadas (archivos eliminados, claves de registro limpiadas)
  - Registrar errores o advertencias encontrados durante el proceso
  - Proporcionar recomendaciones para acciones adicionales (como reiniciar el sistema)

- **Mantener un sistema de logging** completo:
  - Registrar todas las operaciones en archivos de log
  - Mostrar información relevante en la consola de la aplicación
  - Permitir copiar el contenido de la consola al portapapeles
  - Facilitar el acceso a los archivos de log desde la interfaz

### 6. Interfaz Gráfica Intuitiva y Responsiva

- **Proporcionar una interfaz gráfica** que:
  - Muestre claramente el estado de las instalaciones detectadas
  - Indique visualmente (mediante emojis y colores) el tipo de cada instalación
  - Ofrezca información detallada mediante tooltips
  - Mantenga al usuario informado del progreso de las operaciones

- **Garantizar una experiencia responsiva** mediante:
  - Operaciones asíncronas que no bloqueen la interfaz
  - Indicadores de progreso durante operaciones largas
  - Animación de texto durante la fase de escaneo inicial
  - Posibilidad de cancelar operaciones en curso

### 7. Seguridad y Prevención de Errores

- **Implementar mecanismos de seguridad** para prevenir daños al sistema:
  - Crear copias de seguridad antes de realizar cambios
  - Ofrecer un modo de prueba para simular operaciones sin realizar cambios reales
  - Solicitar confirmación antes de operaciones destructivas
  - Verificar privilegios de administrador para operaciones críticas

- **Proporcionar opciones de recuperación**:
  - Permitir restaurar copias de seguridad
  - Generar scripts para deshacer cambios
  - Recomendar acciones adicionales cuando sea necesario (como reiniciar el sistema)
