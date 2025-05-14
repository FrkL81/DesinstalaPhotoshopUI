# Flujo de la Aplicación

Este documento describe el flujo de ejecución de la aplicación DesinstalaPhotoshop, detallando las secuencias de operaciones, los procesos principales y cómo interactúan los diferentes componentes del sistema.

## Flujo General de la Aplicación

El flujo general de la aplicación sigue un patrón claro que comienza con la inicialización, continúa con la detección de instalaciones y finaliza con operaciones específicas según la elección del usuario.

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Inicialización │────▶│    Detección    │────▶│   Operaciones   │
│  de Aplicación  │     │ de Instalaciones│     │    Específicas  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### 1. Inicialización de la Aplicación

La aplicación comienza su ejecución en el método `Main` de la clase `Program`:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Configuración  │────▶│  Aplicación del │────▶│   Creación de   │
│     Inicial     │     │   Tema Oscuro   │     │    MainForm     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Configuración Inicial**:
   - Registro de manejadores de excepciones no controladas
   - Inicialización de la configuración de la aplicación

2. **Aplicación del Tema Oscuro**:
   - Llamada a `Application.SetColorMode(SystemColorMode.Dark)`

3. **Creación de MainForm**:
   - Instanciación del formulario principal
   - Inicialización de servicios y componentes

### 2. Detección de Instalaciones

Una vez que la aplicación está en ejecución, el usuario puede iniciar el proceso de detección de instalaciones de Photoshop:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Detección de   │────▶│  Enriquecimiento│────▶│  Clasificación  │
│  Instalaciones  │     │  de Información │     │ de Instalaciones│
└─────────────────┘     └─────────────────┘     └─────────────────┘
        │                                               │
        ▼                                               ▼
┌─────────────────┐                          ┌─────────────────┐
│  Actualización  │◀─────────────────────────│  Actualización  │
│    de la UI     │                          │  de Botones     │
└─────────────────┘                          └─────────────────┘
```

1. **Detección de Instalaciones**:
   - El usuario hace clic en "Detectar Instalaciones"
   - Se llama a `DetectionService.DetectInstallationsAsync`
   - Se utilizan tres métodos de detección en secuencia:
     - Detección desde programas instalados (WMI/MSI)
     - Detección desde el sistema de archivos
     - Detección desde el registro de Windows

2. **Enriquecimiento de Información**:
   - Para cada instalación detectada, se llama a `EnrichInstallationInfo`
   - Se buscan archivos ejecutables, especialmente `photoshop.exe`
   - Se verifican asociaciones de archivos
   - Se buscan claves de registro relacionadas
   - Se calcula una puntuación de confianza

3. **Clasificación de Instalaciones**:
   - Basándose en la puntuación de confianza, se clasifica cada instalación como:
     - Instalación Principal (`InstallationType.MainInstallation`)
     - Posible Instalación Principal (`InstallationType.PossibleMainInstallation`)
     - Residuos (`InstallationType.Residual`)

4. **Actualización de la UI**:
   - Se muestra la lista de instalaciones detectadas
   - Se actualiza el estado de los botones según las instalaciones detectadas

### 3. Operaciones Específicas

Después de la detección, el usuario puede realizar diferentes operaciones según el tipo de instalaciones detectadas:

#### 3.1. Desinstalación

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Verificación   │────▶│  Creación de    │────▶│  Ejecución del  │
│ de Privilegios  │     │ Copia Seguridad │     │  Desinstalador  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                                         │
                                                         ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Actualización  │◀────│  Limpieza de    │◀────│  Detención de   │
│    de la UI     │     │    Residuos     │     │    Procesos     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Verificación de Privilegios**:
   - Se verifica si la aplicación tiene privilegios de administrador
   - Si no los tiene, se solicita elevación

2. **Creación de Copia de Seguridad** (opcional):
   - Si el usuario lo ha seleccionado, se crea una copia de seguridad
   - Se utiliza `BackupService.CreateBackupAsync`

3. **Ejecución del Desinstalador**:
   - Se llama a `UninstallService.UninstallAsync`
   - Se determina el tipo de desinstalador (MSI o EXE)
   - Se ejecuta el desinstalador con los parámetros adecuados

4. **Detención de Procesos**:
   - Se detienen procesos relacionados con Adobe
   - Se utiliza `ProcessService.StopAdobeProcessesAsync`

5. **Limpieza de Residuos** (opcional):
   - Si el usuario lo ha seleccionado, se limpian residuos después de la desinstalación
   - Se utiliza `CleanupService.CleanupAsync`

6. **Actualización de la UI**:
   - Se actualiza la lista de instalaciones
   - Se muestra un mensaje de éxito
   - Se recomienda reiniciar el sistema si es necesario

#### 3.2. Limpieza de Residuos

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Verificación   │────▶│  Creación de    │────▶│  Detención de   │
│ de Privilegios  │     │ Copia Seguridad │     │ Procesos/Servic.│
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                                         │
                                                         ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Actualización  │◀────│ Programación de │◀────│  Limpieza de    │
│    de la UI     │     │ Elim. al Reinicio│     │ Archivos/Regist.│
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Verificación de Privilegios**:
   - Se verifica si la aplicación tiene privilegios de administrador
   - Si no los tiene, se solicita elevación

2. **Creación de Copia de Seguridad** (opcional):
   - Si el usuario lo ha seleccionado, se crea una copia de seguridad
   - Se utiliza `BackupService.CreateBackupForCleanupAsync`

3. **Detención de Procesos y Servicios**:
   - Se detienen procesos relacionados con Adobe
   - Se detienen servicios de Windows relacionados con Adobe
   - Se desinstalan productos MSI relacionados con Adobe

4. **Limpieza de Archivos y Registro**:
   - Se eliminan archivos y carpetas residuales
   - Se eliminan entradas de registro residuales
   - Se aplican métodos especiales para carpetas en Common Files

5. **Programación de Eliminación al Reinicio**:
   - Para archivos persistentes que no se pueden eliminar
   - Se utiliza `MoveFileEx` con `MOVEFILE_DELAY_UNTIL_REBOOT`

6. **Actualización de la UI**:
   - Se actualiza la lista de instalaciones
   - Se muestra un mensaje de éxito
   - Se recomienda reiniciar el sistema

#### 3.3. Modo de Prueba

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Selección de   │────▶│  Simulación de  │────▶│  Actualización  │
│    Operación    │     │    Operación    │     │    de la UI     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Selección de Operación**:
   - El usuario selecciona el tipo de operación a simular
   - Opciones: Detección, Desinstalación o Limpieza

2. **Simulación de Operación**:
   - Se ejecuta la operación seleccionada con `whatIf = true`
   - No se realizan cambios reales en el sistema

3. **Actualización de la UI**:
   - Se muestra información sobre las operaciones que se habrían realizado
   - Se actualiza la consola con mensajes informativos

## Flujo Detallado de Detección

El proceso de detección es uno de los más complejos y críticos de la aplicación. A continuación se detalla su flujo:

### Detección desde Programas Instalados

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Búsqueda en    │────▶│  Búsqueda en    │────▶│  Búsqueda con   │
│     HKLM        │     │     HKCU        │     │      WMI        │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Búsqueda en HKLM**:
   - Se busca en `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`
   - Se busca en ambas vistas del registro (64 y 32 bits)

2. **Búsqueda en HKCU**:
   - Se busca en `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`
   - Se busca en ambas vistas del registro (64 y 32 bits)

3. **Búsqueda con WMI**:
   - Se utiliza `ManagementObjectSearcher` para buscar productos instalados
   - Consulta: `SELECT * FROM Win32_Product WHERE Name LIKE '%Photoshop%'`

### Detección desde Sistema de Archivos

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Búsqueda en    │────▶│  Verificación   │────▶│  Clasificación  │
│ Rutas Específicas│     │ de photoshop.exe│     │ de Instalación  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Búsqueda en Rutas Específicas**:
   - Directorios de instalación principal: `C:\Program Files\Adobe\Adobe Photoshop [versión]`, `C:\Program Files (x86)\Adobe\Adobe Photoshop [versión]`
   - Configuraciones de usuario: `%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Adobe Photoshop [versión] Settings\`, `%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Presets\`
   - Archivos temporales y de caché: `%LOCALAPPDATA%\Temp\` (patrones `Photoshop Temp*`, `~PST*.tmp`), `%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\CT Font Cache`, `%AppData%\Adobe\CameraRaw\Cache\`
   - Archivos de Autorecuperación: `%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\AutoRecover\`
   - Componentes compartidos: `C:\Program Files\Common Files\Adobe\`, `C:\Program Files (x86)\Common Files\Adobe\`
   - Plugins y Extensiones (CEP/UXP): `%AppData%\Adobe\CEP\extensions\`, `C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\`, `%AppData%\Adobe\UXP\Plugins\External\`, `C:\Program Files\Common Files\Adobe\UXP\Plugins\External\`, `%AppData%\Adobe\UXP\PluginsStorage\PHSP\[versión_numérica_photoshop]\`
   - Datos de OOBE y SLStore: `%LocalAppData%\Adobe\OOBE\`, `C:\ProgramData\Adobe\SLStore_v1\`
   - Logs: `%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Logs\`, `C:\Program Files\Common Files\Adobe\Logs\`, `%LOCALAPPDATA%\Temp\amt3.log`

2. **Verificación de photoshop.exe**:
   - Se verifica si existe `photoshop.exe` en la carpeta
   - Se añade a la lista de archivos asociados
   - Se verifica la versión del archivo para determinar la versión de Photoshop

3. **Clasificación de Instalación**:
   - Si está en Common Files o AppData, se clasifica como residuo
   - Si tiene photoshop.exe, se clasifica como posible instalación principal
   - Si no tiene photoshop.exe, se clasifica como residuo
   - Si contiene solo archivos de caché o temporales, se clasifica como residuo
   - Si contiene archivos de configuración de usuario pero no el ejecutable principal, se clasifica como residuo

### Detección desde Registro

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Búsqueda de    │────▶│  Búsqueda de    │────▶│  Filtrado de    │
│ Claves Adobe    │     │ Claves Photoshop│     │   Duplicados    │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Búsqueda de Claves Adobe y Photoshop Específicas**:
   - Configuraciones de aplicación (Máquina): `HKLM\SOFTWARE\Adobe\Photoshop\[versión_numérica]` y `HKLM\SOFTWARE\Wow6432Node\Adobe\Photoshop\[versión_numérica]`
   - Configuraciones de Usuario: `HKCU\SOFTWARE\Adobe\Photoshop\[versión_numérica]`, `HKCU\SOFTWARE\Adobe\Camera Raw\[versión_numérica]`
   - Información de Desinstalación: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\[GUID de Photoshop]` (para obtener ProductCode)
   - Asociaciones de Archivos: `HKCR\.psd`, `HKCR\.psb`, etc., y `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.psd\`
   - Comandos de apertura: `HKCR\Applications\Photoshop.exe\shell\open\command`
   - Claves de componentes COM: `HKLM\SOFTWARE\Classes\CLSID\{GUID}\InprocServer32`
   - Claves de extensiones de shell: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellIconOverlayIdentifiers`

2. **Extracción de Información**:
   - Se extraen rutas de instalación de las claves de registro
   - Se extraen versiones de Photoshop de las claves
   - Se extraen GUIDs de productos MSI para desinstalación
   - Se verifican asociaciones de archivos para determinar la instalación predeterminada

3. **Filtrado de Duplicados**:
   - Se eliminan instalaciones duplicadas basándose en la ruta de instalación
   - Se eliminan instalaciones duplicadas basándose en el nombre y versión
   - Se combinan datos de diferentes fuentes para la misma instalación

### Enriquecimiento y Clasificación

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Cálculo de     │────▶│  Verificación   │────▶│  Asignación de  │
│   Puntuación    │     │  de Criterios   │     │     Tipo        │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Cálculo de Puntuación**:
   - Se inicia con una puntuación base de 0
   - Se suman puntos por criterios positivos
   - Se restan puntos por criterios negativos

2. **Verificación de Criterios**:
   - **Criterios de Puntuación**:
     - Para una descripción detallada del sistema de puntuación heurística, consulte el documento [Sistema_Puntuacion_Heuristica.md](Sistema_Puntuacion_Heuristica.md).

3. **Asignación de Tipo**:
   - **Instalación Principal**: Puntuación >= 5 y ruta existente
   - **Posible Instalación Principal**: Puntuación >= 3 y ruta existente
   - **Residuos**: Puntuación < 3 o ruta inexistente

## Flujo de Operaciones Asíncronas

Todas las operaciones principales se ejecutan de forma asíncrona para mantener la interfaz de usuario responsiva:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Preparación    │────▶│  Ejecución de   │────▶│  Manejo de      │
│     de UI       │     │    Operación    │     │  Resultados     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
        │                       │                       │
        ▼                       ▼                       ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Deshabilitación│     │  Reporte de     │     │  Restauración   │
│   de Botones    │     │   Progreso      │     │     de UI       │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Preparación de UI**:
   - Se deshabilitan los botones de acción
   - Se muestra la barra de progreso
   - Se habilita el botón de cancelación

2. **Ejecución de Operación**:
   - Se crea un token de cancelación
   - Se crea un objeto de progreso
   - Se ejecuta la operación de forma asíncrona

3. **Reporte de Progreso**:
   - La operación reporta su progreso a través de `IProgress<ProgressInfo>`
   - Se actualiza la barra de progreso y la etiqueta de estado
   - Se muestra una animación durante la fase inicial (0%)

4. **Manejo de Resultados**:
   - Se muestra el resultado de la operación
   - Se registra en el log
   - Se actualiza la lista de instalaciones si es necesario

5. **Restauración de UI**:
   - Se habilitan los botones de acción
   - Se oculta la barra de progreso
   - Se deshabilita el botón de cancelación

## Flujo de Cancelación

El usuario puede cancelar operaciones en curso:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Clic en Botón  │────▶│  Cancelación    │────▶│  Restauración   │
│    Cancelar     │     │   de Token      │     │     de UI       │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Clic en Botón Cancelar**:
   - El usuario hace clic en el botón "Cancelar"
   - Se llama a `BtnCancel_Click`

2. **Cancelación de Token**:
   - Se cancela el token de cancelación
   - Las operaciones verifican periódicamente si se ha solicitado cancelación

3. **Restauración de UI**:
   - Se restaura la interfaz de usuario
   - Se muestra un mensaje indicando que la operación fue cancelada

## Flujo de Copias de Seguridad

Las copias de seguridad se crean antes de operaciones potencialmente destructivas:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Creación de    │────▶│  Respaldo de    │────▶│  Respaldo de    │
│   Directorio    │     │    Archivos     │     │    Registro     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
        │                       │                       │
        ▼                       ▼                       ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Guardado de    │     │  Copia de       │     │  Exportación    │
│   Metadatos     │     │   Archivos      │     │  de Claves      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Creación de Directorio**:
   - Se crea un directorio para la copia de seguridad
   - Formato: `PhotoshopBackups/yyyyMMdd_HHmmss`

2. **Respaldo de Archivos**:
   - Se copian archivos y carpetas a respaldar
   - Se mantiene la estructura de directorios

3. **Respaldo de Registro**:
   - Se exportan claves de registro a archivos .reg
   - Se utiliza `reg.exe export`

4. **Guardado de Metadatos**:
   - Se guarda información sobre la copia de seguridad
   - Se crea un archivo `backup_info.json`

## Flujo de Restauración

El proceso de restauración permite recuperar el sistema a un estado anterior utilizando las copias de seguridad creadas:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Usuario hace   │────▶│  Obtener Lista  │────▶│ Mostrar Formulario │
│ clic en Restaurar│     │   de Backups    │     │  de Restauración  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                                         │
                                                         ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Actualizar UI  │◀────│    Ejecutar     │◀────│    Confirmar    │
│                 │     │ RestoreBackupAsync │     │  Restauración   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

1. **Usuario hace clic en Restaurar**:
   - Se inicia el proceso de restauración desde el botón `btnRestore`
   - Se ejecuta el método `BtnRestore_Click`

2. **Obtener Lista de Backups**:
   - Se llama a `BackupService.GetAvailableBackups()`
   - Se verifica si hay copias de seguridad disponibles
   - Si no hay copias, se muestra un mensaje y se termina el proceso

3. **Mostrar Formulario de Restauración**:
   - Se crea una instancia de `RestoreBackupForm`
   - Se muestra la lista de copias de seguridad disponibles con sus detalles
   - El usuario selecciona una copia de seguridad para restaurar

4. **Confirmar Restauración**:
   - El usuario confirma la restauración haciendo clic en "Restaurar"
   - Se obtiene la ruta de la copia de seguridad seleccionada

5. **Ejecutar RestoreBackupAsync**:
   - Se llama a `BackupService.RestoreBackupAsync`
   - Se restauran archivos y claves de registro
   - Se actualiza el estado de la copia de seguridad

6. **Actualizar UI**:
   - Se muestra un mensaje de éxito o error
   - Se actualiza la lista de instalaciones si es necesario
   - Se recomienda reiniciar el sistema si se restauraron claves de registro

## Conclusión

El flujo de la aplicación DesinstalaPhotoshop está diseñado para proporcionar una experiencia de usuario fluida y responsiva, mientras se realizan operaciones potencialmente complejas y destructivas de manera segura. La arquitectura asíncrona, el manejo de errores robusto y el sistema de copias de seguridad garantizan que el usuario pueda desinstalar Photoshop y limpiar residuos con confianza.
