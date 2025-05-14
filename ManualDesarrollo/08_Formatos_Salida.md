# Formatos de Salida

Este documento describe los diferentes formatos de salida generados por la aplicación DesinstalaPhotoshop, incluyendo reportes, scripts de limpieza y logs. Estos formatos son fundamentales para proporcionar información detallada al usuario sobre las operaciones realizadas y para facilitar la limpieza manual cuando sea necesario.

## Estructura del Documento

1. [Reportes Generados](#reportes-generados)
2. [Scripts de Limpieza](#scripts-de-limpieza)
3. [Logs de Consola/UI](#logs-de-consolaui)
4. [Copias de Seguridad](#copias-de-seguridad)

## Reportes Generados

La aplicación genera diversos reportes para informar al usuario sobre el resultado de las operaciones realizadas, especialmente cuando se encuentran elementos que no se pudieron eliminar automáticamente.

### Reporte de Claves de Registro No Eliminadas

Este reporte se genera cuando la aplicación no puede eliminar ciertas claves del registro de Windows durante el proceso de limpieza.

#### Ubicación y Nomenclatura

Los reportes se guardan en el escritorio del usuario, en la carpeta `DesinstalaPhotoshop_Reports`, con el siguiente formato de nombre:

```
RegistryCleanupReport_yyyyMMdd_HHmmss.txt
```

La ruta base exacta es:
```
Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DesinstalaPhotoshop_Reports\\"
```

Donde `yyyyMMdd_HHmmss` representa la fecha y hora de generación del reporte.

#### Estructura del Reporte

El reporte tiene la siguiente estructura:

```
=== REPORTE DE CLAVES DE REGISTRO NO ELIMINADAS ===
Fecha: yyyy-MM-dd HH:mm:ss
Total de claves no eliminadas: N

CLAVES NO ELIMINADAS:
--------------------
[Lista de claves que no se pudieron eliminar con sus mensajes de error]

COMANDOS PARA ELIMINACIÓN MANUAL:
-----------------------------
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\..." /f
reg delete "HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\..." /f
[Más comandos reg delete...]
```

#### Ejemplo de Reporte

```
=== REPORTE DE CLAVES DE REGISTRO NO ELIMINADAS ===
Fecha: 2023-11-15 14:32:45
Total de claves no eliminadas: 4

CLAVES NO ELIMINADAS:
--------------------
Error al eliminar clave de registro: HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\25.0 - Acceso denegado
Error al eliminar clave de registro: HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\25.0\Plugins\UXPStorage\PHSP\2500\External\[PluginID]\PluginData - Clave en uso
Error al eliminar clave de registro: HKEY_CLASSES_ROOT\.psd\ShellNew - Permisos insuficientes
Error al eliminar clave de registro: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{GUID-Photoshop-XX} - No se pudo abrir la clave.

COMANDOS PARA ELIMINACIÓN MANUAL:
-----------------------------
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\25.0" /f
reg delete "HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\25.0\Plugins\UXPStorage\PHSP\2500\External\[PluginID]\PluginData" /f
reg delete "HKEY_CLASSES_ROOT\.psd\ShellNew" /f
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{GUID-Photoshop-XX}" /f
```

### Reporte de Residuos

Cuando la aplicación detecta residuos que no se pudieron eliminar completamente, genera un reporte detallado.

#### Ubicación y Nomenclatura

El reporte se guarda en el escritorio del usuario con el siguiente formato de nombre:

```
DesinstalaPhotoshop_Residuos_yyyyMMdd_HHmmss.txt
```

La ruta base exacta es:
```
Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DesinstalaPhotoshop_Reports\\"
```

#### Estructura del Reporte

El reporte incluye información detallada sobre los residuos detectados, incluyendo:

- Archivos y carpetas que no se pudieron eliminar
- Claves de registro que no se pudieron eliminar
- Información sobre por qué no se pudieron eliminar (permisos, archivos en uso, etc.)
- Sugerencias para la eliminación manual

## Scripts de Limpieza

La aplicación puede generar scripts de limpieza para ayudar al usuario a eliminar manualmente los elementos que no se pudieron eliminar automáticamente. Estos scripts se pueden generar en dos formatos: archivos por lotes (`.bat`) para CMD o scripts de PowerShell (`.ps1`).

### Generación de Scripts

Los scripts se generan a partir de los comandos `reg delete` encontrados en la consola de la aplicación o en los reportes de claves de registro no eliminadas. El usuario puede elegir el formato del script (`.bat` o `.ps1`) y la ubicación donde guardarlo.

> **⚠️ ADVERTENCIA DE SEGURIDAD**: La ejecución de scripts, incluso los generados por esta aplicación, conlleva riesgos inherentes. Estos scripts pueden realizar cambios permanentes en el sistema, especialmente cuando se ejecutan con privilegios administrativos. Se recomienda encarecidamente revisar el contenido del script antes de ejecutarlo para comprender exactamente qué acciones realizará. La aplicación no se hace responsable por daños causados por la ejecución incorrecta o no supervisada de los scripts generados.

### Script de Limpieza en Formato .bat

#### Estructura del Script .bat

```bat
@echo off
echo ===================================================
echo Script de limpieza de residuos de Adobe Photoshop
echo Generado por DesinstalaPhotoshop
echo Fecha: yyyy-MM-dd HH:mm:ss
echo ===================================================
echo.
echo Este script eliminará entradas de registro residuales de Adobe Photoshop.
echo Se recomienda crear una copia de seguridad del registro antes de ejecutar este script.
echo.
pause
echo.
echo Eliminando entradas de registro...
echo.

echo Ejecutando: reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\110.0" /f
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\110.0" /f
if %ERRORLEVEL% NEQ 0 echo   - Error al ejecutar el comando
echo.

echo Ejecutando: reg delete "HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\110.0" /f
reg delete "HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\110.0" /f
if %ERRORLEVEL% NEQ 0 echo   - Error al ejecutar el comando
echo.

echo.
echo Limpieza completada.
echo.
pause
```

### Script de Limpieza en Formato .ps1

#### Estructura del Script .ps1

```powershell
# Script de limpieza de residuos de Adobe Photoshop
# Generado por DesinstalaPhotoshop
# Fecha: yyyy-MM-dd HH:mm:ss

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Script de limpieza de residuos de Adobe Photoshop" -ForegroundColor Cyan
Write-Host "Generado por DesinstalaPhotoshop" -ForegroundColor Cyan
Write-Host "Fecha: yyyy-MM-dd HH:mm:ss" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Este script eliminará entradas de registro residuales de Adobe Photoshop." -ForegroundColor Yellow
Write-Host "Se recomienda crear una copia de seguridad del registro antes de ejecutar este script." -ForegroundColor Yellow
Write-Host ""
Read-Host "Presione Enter para continuar"
Write-Host ""
Write-Host "Eliminando entradas de registro..." -ForegroundColor Green
Write-Host ""

Write-Host "Ejecutando: reg delete 'HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\110.0' /f" -ForegroundColor Gray
try {
    Remove-Item -Path "HKLM:\SOFTWARE\Adobe\Photoshop\110.0" -Force -Recurse
    Write-Host "  - Comando ejecutado correctamente" -ForegroundColor Green
} catch {
    Write-Host "  - Error al ejecutar el comando: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "Ejecutando: reg delete 'HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\110.0' /f" -ForegroundColor Gray
try {
    Remove-Item -Path "HKCU:\SOFTWARE\Adobe\Photoshop\110.0" -Force -Recurse
    Write-Host "  - Comando ejecutado correctamente" -ForegroundColor Green
} catch {
    Write-Host "  - Error al ejecutar el comando: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host ""
Write-Host "Limpieza completada." -ForegroundColor Green
Write-Host ""
Read-Host "Presione Enter para salir"
```

## Logs de Consola/UI

La aplicación utiliza un sistema de logging para registrar información sobre las operaciones realizadas. Estos logs se muestran en la consola de la aplicación y también se guardan en archivos de texto.

### Logs en la Consola de la Aplicación

Los mensajes de log en la consola de la aplicación se muestran con diferentes colores según su nivel:

- **Info**: Blanco
- **Warning**: Amarillo
- **Error**: Rojo
- **Success**: Verde
- **Debug**: Gris (solo visible en modo debug)

Cada mensaje incluye una marca de tiempo en formato `[HH:mm:ss]`.

#### Ejemplo de Logs en la Consola

```
[14:30:15] Iniciando detección de instalaciones de Photoshop...
[14:30:18] Detectadas 2 instalaciones desde programas instalados.
[14:30:20] Detectadas 1 instalaciones adicionales desde el registro.
[14:30:22] Detectadas 0 instalaciones adicionales desde el sistema de archivos.
[14:30:25] Detección completada. Encontradas 3 instalaciones válidas.
[14:30:25] Clasificada como instalación principal: Adobe Photoshop 2023
[14:30:25] Clasificada como residuos: Adobe Photoshop CC
[14:30:25] Clasificada como residuos: Adobe Photoshop Elements
```

### Archivos de Log

Los logs también se guardan en archivos de texto para referencia futura y depuración.

#### Ubicación y Nomenclatura

Los archivos de log se guardan en la carpeta `Logs` dentro del directorio de la aplicación, con el siguiente formato de nombre:

```
DesinstalaPhotoshop_yyyyMMdd_HHmmss.log
```

La ruta base exacta es:
```
Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesinstalaPhotoshop", "Logs")
```

Que típicamente corresponde a:
```
C:\Users\[Usuario]\AppData\Local\DesinstalaPhotoshop\Logs
```

> **Nota**: El archivo `ResiduosDePhotoshop.md` también menciona `C:\Users\[User]\AppData\Roaming\Adobe\Adobe Photoshop [Version]\LogFiles\` y `C:\Program Files\Common Files\Adobe\Logs\` y `%LOCALAPPDATA%\Temp\amt3.log` como fuentes de logs de Photoshop/Adobe. La aplicación DesinstalaPhotoshop genera sus propios logs en la ruta `%LocalAppData%\DesinstalaPhotoshop\Logs`. Se puede considerar agregar una funcionalidad para *recopilar* también estos otros logs de Adobe si fuera útil para el diagnóstico.

#### Estructura del Archivo de Log

Cada línea del archivo de log tiene el siguiente formato:

```
[yyyy-MM-dd HH:mm:ss.fff] [NIVEL] Mensaje
```

Donde:
- `yyyy-MM-dd HH:mm:ss.fff` es la fecha y hora con milisegundos
- `NIVEL` puede ser `DEBUG`, `INFO`, `WARNING`, `ERROR` o `CRITICAL`
- `Mensaje` es el contenido del log

#### Ejemplo de Archivo de Log

```
[2023-11-15 14:30:15.123] [INFO] === Inicio de sesión de DesinstalaPhotoshop ===
[2023-11-15 14:30:15.125] [INFO] Versión: 1.0.0.0
[2023-11-15 14:30:15.126] [INFO] Sistema operativo: Microsoft Windows NT 10.0.19045.0
[2023-11-15 14:30:15.127] [INFO] Cultura: es-ES
[2023-11-15 14:30:15.128] [INFO] Directorio de logs: C:\Users\Usuario\AppData\Local\DesinstalaPhotoshop\Logs
[2023-11-15 14:30:15.129] [INFO] ===============================================
[2023-11-15 14:30:15.130] [INFO] Iniciando detección de instalaciones de Photoshop...
[2023-11-15 14:30:18.456] [INFO] Detectadas 2 instalaciones desde programas instalados.
[2023-11-15 14:30:20.789] [INFO] Detectadas 1 instalaciones adicionales desde el registro.
[2023-11-15 14:30:22.123] [INFO] Detectadas 0 instalaciones adicionales desde el sistema de archivos.
[2023-11-15 14:30:25.456] [INFO] Detección completada. Encontradas 3 instalaciones válidas.
[2023-11-15 14:30:25.457] [INFO] Clasificada como instalación principal: Adobe Photoshop 2023
[2023-11-15 14:30:25.458] [INFO] Clasificada como residuos: Adobe Photoshop CC
[2023-11-15 14:30:25.459] [INFO] Clasificada como residuos: Adobe Photoshop Elements
[2023-11-15 14:31:05.123] [DEBUG] Intentando eliminar carpeta de UXP plugin: C:\Users\Usuario\AppData\Roaming\Adobe\UXP\PluginsStorage\PHSP\2500\External\com.adobe.photoshop.plugins.myplugin\
[2023-11-15 14:31:05.567] [INFO] Carpeta UXP eliminada: C:\Users\Usuario\AppData\Roaming\Adobe\UXP\PluginsStorage\PHSP\2500\External\com.adobe.photoshop.plugins.myplugin\
[2023-11-15 14:31:10.321] [DEBUG] Buscando archivos de caché de fuentes en: C:\Users\Usuario\AppData\Roaming\Adobe\Adobe Photoshop 2023\CT Font Cache
[2023-11-15 14:31:10.789] [INFO] Eliminados archivos de caché de fuentes: 15 archivos
[2023-11-15 14:32:10.321] [DEBUG] Buscando clave de registro: HKCU\SOFTWARE\Adobe\Photoshop\25.0\VisitedDirs
[2023-11-15 14:32:10.455] [INFO] Clave HKCU\SOFTWARE\Adobe\Photoshop\25.0\VisitedDirs eliminada.
[2023-11-15 14:32:15.678] [DEBUG] Verificando asociaciones de archivos para extensión: .psd
[2023-11-15 14:32:15.789] [INFO] Eliminada asociación de archivo para .psd en HKCR\.psd
[2023-11-15 14:32:20.123] [DEBUG] Buscando datos de OOBE en: C:\Users\Usuario\AppData\Local\Adobe\OOBE\
[2023-11-15 14:32:20.456] [INFO] Eliminados datos de OOBE para Photoshop
```

## Copias de Seguridad

La aplicación crea copias de seguridad antes de realizar operaciones de limpieza o desinstalación para permitir la restauración en caso de problemas.

### Estructura de las Copias de Seguridad

Las copias de seguridad se organizan en una estructura de directorios específica:

```
PhotoshopBackups/
└── Cleanup_yyyyMMdd_HHmmss/
    ├── Files/                  # Copias de seguridad de archivos
    ├── Directories/            # Copias de seguridad de carpetas
    ├── Registry/               # Copias de seguridad de claves de registro (.reg)
    └── Metadata/               # Información sobre la copia de seguridad
        └── backup_info.json    # Metadatos en formato JSON
```

La ruta base exacta es:
```
Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PhotoshopBackups")
```

Que típicamente corresponde a:
```
C:\Users\[Usuario]\Documents\PhotoshopBackups
```

### Archivo de Metadatos (backup_info.json)

El archivo `backup_info.json` contiene información detallada sobre la copia de seguridad en formato JSON:

```json
{
  "BackupTime": "2023-11-15T14:30:00",
  "BackupType": "Cleanup",
  "BackupPath": "C:\\Users\\Usuario\\Documents\\PhotoshopBackups\\Cleanup_20231115_143000",
  "ItemCount": 25,
  "TotalSize": 15678945,
  "Items": [
    {
      "Id": "12345678-1234-1234-1234-123456789012",
      "OriginalPath": "C:\\Program Files\\Adobe\\Adobe Photoshop 2023\\Photoshop.exe",
      "BackupPath": "C:\\Users\\Usuario\\Documents\\PhotoshopBackups\\Cleanup_20231115_143000\\Files\\Photoshop.exe.20231115_143000.bak",
      "ItemType": "File",
      "BackupTime": "2023-11-15T14:30:05",
      "Description": "Archivo: C:\\Program Files\\Adobe\\Adobe Photoshop 2023\\Photoshop.exe",
      "Size": 123456,
      "IsRestored": false,
      "RestoreTime": null,
      "RestoreErrorMessage": null
    },
    {
      "Id": "87654321-4321-4321-4321-210987654321",
      "OriginalPath": "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Photoshop\\110.0",
      "BackupPath": "C:\\Users\\Usuario\\Documents\\PhotoshopBackups\\Cleanup_20231115_143000\\Registry\\HKEY_LOCAL_MACHINE_SOFTWARE_Adobe_Photoshop_110.0.reg",
      "ItemType": "Registry",
      "BackupTime": "2023-11-15T14:30:10",
      "Description": "Clave de registro: HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Photoshop\\110.0",
      "Size": 5678,
      "IsRestored": false,
      "RestoreTime": null,
      "RestoreErrorMessage": null
    }
  ]
}
```

### Archivos de Registro (.reg)

Las copias de seguridad de las claves de registro se guardan en archivos `.reg` que pueden ser importados directamente en el registro de Windows para restaurar las claves.

#### Ejemplo de Archivo .reg

```reg
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\110.0]
"InstallPath"="C:\\Program Files\\Adobe\\Adobe Photoshop 2023\\"
"Version"="24.0"

[HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\110.0\Settings]
"Language"="es_ES"
"FirstRun"="0"
```

## Conclusión

Los formatos de salida descritos en este documento son fundamentales para proporcionar información detallada al usuario sobre las operaciones realizadas por la aplicación DesinstalaPhotoshop. Los reportes y scripts de limpieza permiten al usuario completar manualmente la limpieza cuando sea necesario, mientras que los logs y copias de seguridad proporcionan información para depuración y recuperación en caso de problemas.

Estos formatos están diseñados para ser claros, informativos y fáciles de usar, siguiendo las mejores prácticas de diseño de interfaces y experiencia de usuario.
