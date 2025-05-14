# Tech Context: DesinstalaPhotoshop

## 1. Stack Tecnológico Principal
*   **Lenguaje:** C#
*   **Framework:** .NET 9.0
*   **Interfaz Gráfica:** Windows Forms
*   **Runtime Target:** win-x64 (para publicación autónoma)

Referencias: `ManualDesarrollo/01_Introduccion_Proyecto.md`, `DesinstalaPhotoshop.UI.csproj`

## 2. Entorno de Desarrollo
*   **IDE:** Visual Studio 2022 (o compatible con .NET 9.0 y Windows Forms)
*   **SDK:** .NET SDK 9.0 o superior
*   **Control de Versiones:** Git (implícito por la estructura de carpetas `C:/MiRepo/`)

Referencias: `ManualDesarrollo/10_Anexos.md`

## 3. Dependencias Clave del Proyecto
*   **`DesinstalaPhotoshop.Core`:**
    *   `System.Management`: Para WMI (detección de programas instalados, información del sistema). Versión: 9.0.4
    *   `System.ServiceProcess.ServiceController`: Para interactuar con servicios de Windows (detener servicios de Adobe). Versión: 8.0.0
    *   Referencias: `DesinstalaPhotoshop.Core.csproj`, `DesinstalaPhotoshop.Core/obj/project.assets.json`.
*   **`DesinstalaPhotoshop.UI`:**
    *   `DesinstalaPhotoshop.Core`: Referencia al proyecto de lógica de negocio.
    *   `FontAwesome.Sharp`: Para iconos en botones y elementos de la UI. Versión: 6.6.0
    *   `CustomMsgBoxLibrary.dll`: Librería externa (DLL referenciada directamente) para diálogos modales modernos y personalizables. Ubicada en `DesinstalaPhotoshop.UI/lib/`.
    *   Referencias: `DesinstalaPhotoshop.UI.csproj`, `DesinstalaPhotoshop.UI/obj/project.assets.json`.

## 4. Restricciones Técnicas
*   **Plataforma:** Exclusivamente Windows (Windows 10 x64 y superior).
*   **Privilegios:** Requiere privilegios de administrador para la mayoría de sus funcionalidades clave (modificación del registro, eliminación de archivos en `Program Files`, gestión de servicios). El `app.manifest` controla esto.
*   **.NET Runtime:** Aunque se puede publicar como self-contained, el desarrollo se basa en .NET 9.
*   **Tema Oscuro:** La API `Application.SetColorMode(SystemColorMode.Dark)` es experimental en .NET 9 y requiere la supresión de advertencias (`WFO5001`, `WFO5002`) en el `.csproj`.

Referencias: `ManualDesarrollo/03_GUI_Descripcion_Visual.md`, `ManualDesarrollo/10_Anexos.md`, `app.manifest`.

## 5. Convenciones de Código y Estilo
*   **Nomenclatura:** PascalCase para clases y métodos, camelCase para variables locales, `_camelCase` para campos privados.
*   **Asincronía:** Uso extensivo de `async/await` para operaciones I/O y tareas de larga duración para mantener la UI responsiva.
*   **Manejo de Errores:** Uso de `try-catch` bloques, logging de excepciones.
*   **Comentarios XML:** Para documentación de API pública en el Core (pendiente de implementar exhaustivamente).
*   **Separación de Lógica y UI:** La lógica de negocio reside en `DesinstalaPhotoshop.Core`, la UI en `DesinstalaPhotoshop.UI`.
*   **Logging:** Se planea un `LoggingService`, actualmente implementado de forma básica en `MainForm` con `AppendToConsole`.
*   **Uso de `CustomMsgBoxLibrary`:** Preferido sobre `MessageBox.Show()` estándar para consistencia estética.

Referencias: `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`, código fuente existente.

## 6. Sistema de Build y Publicación
*   **Build:** A través de Visual Studio o `dotnet build`.
*   **Publicación:** `dotnet publish DesinstalaPhotoshop.UI -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish`
    *   Esto crea un ejecutable único auto-contenido en la carpeta `publish`.

Referencia: `ManualDesarrollo/10_Anexos.md`, `DesinstalaPhotoshop.UI.csproj`.

## 7. Estructura de Archivos Relevante
*   `DesinstalaPhotoshop.sln`: Archivo de solución.
*   `DesinstalaPhotoshop.Core/`: Proyecto de lógica de negocio.
*   `DesinstalaPhotoshop.UI/`: Proyecto de interfaz de usuario.
    *   `MainForm.cs` / `MainForm.Designer.cs`: Formulario principal.
    *   `Program.cs`: Punto de entrada.
    *   `app.manifest`: Manifiesto de aplicación (control de UAC).
    *   `Resources/app.ico`: Icono de la aplicación.
    *   `lib/CustomMsgBoxLibrary.dll`: Librería de diálogos.
    *   Otros formularios: `CleanupOptionsForm`, `RestoreBackupForm`, `TestModeOptionsForm`, `UninstallOptionsForm`.
*   `ManualDesarrollo/`: Contiene toda la documentación del proyecto.
    *   Es la **fuente de verdad** para el diseño y la implementación. Su revisión es constante.
*   `PlanDesarrollo.md`: Hoja de ruta del desarrollo.