# Anexos

Este documento contiene información complementaria y de referencia para el proyecto DesinstalaPhotoshop, incluyendo recursos externos, comandos útiles, glosario de términos y requisitos del sistema.

## Estructura del Documento

1. [Recursos Externos](#recursos-externos)
2. [Comandos de Publicación](#comandos-de-publicación)
3. [Requisitos del Sistema](#requisitos-del-sistema)
4. [Dependencias del Proyecto](#dependencias-del-proyecto)
5. [Glosario de Términos](#glosario-de-términos)
6. [Referencias y Documentación](#referencias-y-documentación)

## Recursos Externos

El proyecto incluye una carpeta `Recursos_Externos` que contiene archivos que no forman parte funcional del proyecto, pero que son útiles como referencia o para propósitos de documentación.

### Contenido de la Carpeta Recursos_Externos

1. **CleanPhotoshop.ps1**: Script original de PowerShell que este proyecto reemplaza.
2. **DesinstalaPhotoshopCSharp.md**: Documentación del proyecto.
3. **GuiaDelUsuario.md**: Guía del usuario.
4. **images/**: Carpeta con recursos gráficos originales.
   - **Kill_Photoshop.ico**: Icono original que se ha incorporado en el proyecto.
5. **Mejoras.md**: Documento con sugerencias de mejoras para la aplicación.
6. **Informe_Deteccion.md**: Informe detallado sobre el proceso de detección de instalaciones.
7. **ComandoDePublicación.md**: Información sobre el comando para publicar la aplicación.
8. **ResiduosDePhotoshop.md**: Informe detallado sobre rastros y residuos de Adobe Photoshop, fuente principal para la lógica de limpieza profunda de esta aplicación.
9. **Temp_*.md**: Documentos temporales con información técnica y sugerencias de implementación.

### Nota sobre los Recursos Externos

Estos archivos se han movido a la carpeta `Recursos_Externos` para mantener limpia la estructura del proyecto. El icono ya está incorporado como recurso embebido en el proyecto, por lo que no es necesario mantener la carpeta "images" en la raíz del proyecto.

## Comandos de Publicación

Para publicar la aplicación como un ejecutable único y autónomo, se utiliza el siguiente comando:

```bash
dotnet publish DesinstalaPhotoshop.UI -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

### Parámetros del Comando

- `-c Release`: Compila en modo Release (optimizado)
- `-r win-x64`: Específico para Windows de 64 bits
- `--self-contained`: Incluye el runtime de .NET, por lo que no requiere que .NET esté instalado en la máquina destino
- `-p:PublishSingleFile=true`: Empaqueta todo en un único archivo ejecutable
- `-o publish`: Coloca el resultado en la carpeta "publish"

### Archivo de Manifiesto

El archivo `app.manifest` con la configuración `<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />` es lo que hace que el ejecutable se inicie con privilegios elevados.

A continuación se muestra el fragmento clave del archivo de manifiesto:

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="DesinstalaPhotoshop.UI.app"/>
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
    <security>
      <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
        <!-- Opciones del manifiesto UAC
             Si quiere cambiar el nivel del Control de cuentas de usuario de Windows reemplace el
             nodo requestedExecutionLevel por alguno de los siguientes.

        <requestedExecutionLevel  level="asInvoker" uiAccess="false" />
        <requestedExecutionLevel  level="requireAdministrator" uiAccess="false" />
        <requestedExecutionLevel  level="highestAvailable" uiAccess="false" />

            Especificar el elemento requestedExecutionLevel deshabilitará la virtualización de archivos y registros.
            Quite este elemento si la aplicación necesita esta virtualización para la compatibilidad
            con versiones anteriores.
        -->
        <requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>
</assembly>
```

Este es el método recomendado para publicar aplicaciones .NET como ejecutables independientes que requieren privilegios de administrador, tal como se solicitaba en las preferencias del usuario.

## Requisitos del Sistema

### Para Usuarios Finales

- **Sistema Operativo**: Windows 10 o superior
- **Arquitectura**: 64 bits (x64)
- **Espacio en Disco**: Al menos 100 MB para la aplicación y copias de seguridad
- **Privilegios**: Administrador (para realizar operaciones de limpieza y desinstalación)

### Para Desarrolladores

- **Framework**: .NET 9.0
- **IDE**: Visual Studio 2022 o superior
- **Paquetes NuGet**:
  - System.Management (9.0.4)
  - System.ServiceProcess.ServiceController (8.0.0)
- **Herramientas Adicionales**:
  - .NET SDK 9.0 o superior
  - Windows Forms Designer para .NET

## Dependencias del Proyecto

### Dependencias de DesinstalaPhotoshop.Core

```json
"dependencies": {
  "System.Management": "9.0.4",
  "System.ServiceProcess.ServiceController": "8.0.0"
}
```

### Dependencias de DesinstalaPhotoshop.UI

```json
"dependencies": {
  "CustomMsgBoxLibrary": "1.0.0" // o la versión correspondiente
}
```

> **Nota**: `CustomMsgBoxLibrary` puede estar referenciada directamente como DLL en el proyecto UI en lugar de como paquete NuGet, dependiendo de la configuración del proyecto.

### Propiedades del Proyecto DesinstalaPhotoshop.UI

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net9.0-windows</TargetFramework>
  <Nullable>enable</Nullable>
  <UseWindowsForms>true</UseWindowsForms>
  <ImplicitUsings>enable</ImplicitUsings>
  <ApplicationIcon>Resources\app.ico</ApplicationIcon>
  <NoWarn>$(NoWarn);WFO5001;WFO5002</NoWarn>
  <ApplicationManifest>app.manifest</ApplicationManifest>
  <Version>1.0.0</Version>
  <Authors>Augment Code</Authors>
  <Company>Augment Code</Company>
  <Product>DesinstalaPhotoshop</Product>
  <Description>Herramienta para desinstalar Adobe Photoshop y limpiar residuos</Description>
  <Copyright>Copyright © 2024</Copyright>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
</PropertyGroup>
```

## Glosario de Términos

### Términos Técnicos

| Término | Descripción |
|---------|-------------|
| **Instalación Principal** | Instalación completa y funcional de Adobe Photoshop, que incluye el ejecutable principal y los componentes necesarios para su funcionamiento. |
| **Residuos** | Archivos, carpetas y entradas de registro que quedan en el sistema después de una desinstalación convencional o que pertenecen a una instalación corrupta o incompleta. |
| **Clave de Registro** | Entrada en el Registro de Windows que almacena configuración y datos de la aplicación. |
| **MSI** | Microsoft Installer, formato de paquete de instalación utilizado por Windows. |
| **WMI** | Windows Management Instrumentation, infraestructura para administrar datos y operaciones en sistemas Windows. |
| **Puntuación de Confianza** | Valor numérico que indica la probabilidad de que una instalación detectada sea una instalación principal válida. Ver [Sistema_Puntuacion_Heuristica.md](Sistema_Puntuacion_Heuristica.md) para más detalles. |
| **Modo WhatIf** | Modo de simulación que muestra qué acciones se realizarían sin ejecutarlas realmente. |
| **Copia de Seguridad** | Respaldo de archivos y claves de registro antes de realizar operaciones de limpieza o desinstalación. |
| **Script de Limpieza** | Archivo .bat o .ps1 generado para eliminar manualmente claves de registro residuales. |
| **Programación de Eliminación** | Técnica para marcar archivos para su eliminación en el próximo reinicio del sistema. |
| **OOBE** | Out Of Box Experience. Carpeta (`%LocalAppData%\Adobe\OOBE\`) que almacena datos relacionados con la configuración inicial y activación de productos Adobe. |
| **SLStore** | Carpeta (`C:\ProgramData\Adobe\SLStore_v1\`) que contiene datos de licenciamiento de Adobe. |
| **CEP** | Common Extensibility Platform. Plataforma de extensiones más antigua de Adobe. Rastros en `%AppData%\Adobe\CEP\extensions\` y `C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\`. |
| **UXP** | Unified Extensibility Platform. Plataforma de plugins moderna de Adobe. Rastros en `%AppData%\Adobe\UXP\Plugins\External\`, `C:\Program Files\Common Files\Adobe\UXP\Plugins\External\` y datos en `%AppData%\Adobe\UXP\PluginsStorage\`. |
| **amt3.log** | Archivo de log de Creative Cloud (`%LOCALAPPDATA%\Temp\amt3.log`) útil para diagnosticar problemas de inicio o licenciamiento. |

### Rutas y Ubicaciones Importantes

| Ruta | Descripción |
|------|-------------|
| **C:\Program Files\Adobe\Adobe Photoshop [versión]\** | Ubicación típica de instalaciones principales de Adobe Photoshop. |
| **C:\Program Files (x86)\Adobe\Adobe Photoshop [versión]\** | Ubicación alternativa para instalaciones de 32 bits o en sistemas de 64 bits. |
| **C:\Program Files\Common Files\Adobe\** | Ubicación común de componentes compartidos de Adobe. |
| **C:\Program Files (x86)\Common Files\Adobe\** | Ubicación común de componentes compartidos de Adobe (32 bits). |
| **%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Adobe Photoshop [versión] Settings\** | Preferencias principales del usuario. |
| **%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\Presets\** | Pinceles, acciones, etc. |
| **%LOCALAPPDATA%\Temp\** | Archivos temporales de Photoshop (`Photoshop Temp*`, `~PST*.tmp`). |
| **%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\CT Font Cache** | Caché de fuentes. |
| **%AppData%\Adobe\CameraRaw\Cache\** | Caché de Camera Raw. |
| **%AppData%\Roaming\Adobe\Adobe Photoshop [versión]\AutoRecover\** | Archivos de autorecuperación (`.psb`). |
| **%LocalAppData%\Adobe\OOBE\** | Datos de configuración inicial y activación. |
| **C:\ProgramData\Adobe\SLStore_v1\** | Datos de licenciamiento. |
| **%AppData%\Adobe\CEP\extensions\** | Extensiones CEP de usuario. |
| **C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\** | Extensiones CEP a nivel de sistema. |
| **%AppData%\Adobe\UXP\Plugins\External\** | Plugins UXP de usuario. |
| **C:\Program Files\Common Files\Adobe\UXP\Plugins\External\** | Plugins UXP a nivel de sistema. |
| **%AppData%\Adobe\UXP\PluginsStorage\PHSP\[versión_numérica_photoshop]\** | Datos de plugins UXP. |
| **HKEY_LOCAL_MACHINE\SOFTWARE\Adobe\Photoshop\[versión_numérica]** | Clave de registro que contiene información de instalación de Photoshop. |
| **HKEY_CURRENT_USER\SOFTWARE\Adobe\Photoshop\[versión_numérica]** | Clave de registro que contiene configuración de usuario para Photoshop. |
| **HKEY_CURRENT_USER\SOFTWARE\Adobe\Camera Raw\[versión_numérica]** | Configuración de Camera Raw. |
| **HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\[GUID de Photoshop]** | Clave de desinstalación de Photoshop. |
| **HKCR\.psd** | Asociación de archivos para .psd. |
| **HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.psd\** | Asociaciones de archivos a nivel de usuario. |

## Referencias y Documentación

### Documentación Oficial

- [Documentación de .NET 9.0](https://learn.microsoft.com/es-es/dotnet/)
- [Documentación de Windows Forms](https://learn.microsoft.com/es-es/dotnet/desktop/winforms/?view=netdesktop-9.0)
- [Guía de Windows Registry](https://learn.microsoft.com/es-es/windows/win32/sysinfo/registry)
- [Documentación de System.Management](https://learn.microsoft.com/es-es/dotnet/api/system.management)
- [Documentación de System.ServiceProcess](https://learn.microsoft.com/es-es/dotnet/api/system.serviceprocess)

### Recursos Adicionales

- **CleanPhotoshop.ps1**: Script original de PowerShell que sirvió como base para este proyecto.
- **Informe_Deteccion.md**: Documento detallado sobre el proceso de detección de instalaciones.
- **GuiaDelUsuario.md**: Guía completa para usuarios finales.
- **ResiduosDePhotoshop.md**: Informe detallado sobre rastros y residuos de Adobe Photoshop.
- **recursos/CustomMsgBoxLibrary.md**: Documentación de la librería de diálogos personalizados utilizada en el proyecto.

### Herramientas Recomendadas

- **Visual Studio 2022**: IDE principal para el desarrollo del proyecto.
- **Process Explorer**: Herramienta útil para identificar procesos y archivos bloqueados.
- **Registry Explorer**: Herramienta para explorar y editar el registro de Windows.
- **Dependency Walker**: Herramienta para analizar dependencias de archivos ejecutables.
- **Adobe CC Cleaner Tool**: Herramienta oficial de Adobe para limpieza (aunque esta aplicación busca ser más exhaustiva).
- **Process Monitor (Sysinternals)**: Para rastrear accesos a archivos y registro.
- **Regshot**: Para comparar instantáneas del registro.
- **Everything (VoidTools)**: Para búsquedas rápidas de archivos.

## Notas Finales

Este documento de anexos proporciona información complementaria que puede ser útil durante el desarrollo, mantenimiento y uso de la aplicación DesinstalaPhotoshop. Se recomienda consultar este documento junto con el resto de la documentación del proyecto para obtener una comprensión completa de la aplicación.
