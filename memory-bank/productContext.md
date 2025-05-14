# Product Context: DesinstalaPhotoshop

## 1. Problema a Resolver
Las desinstalaciones estándar de Adobe Photoshop, e incluso herramientas como Adobe CC Cleaner Tool, a menudo dejan atrás numerosos archivos residuales, entradas de registro y configuraciones. Esto puede causar problemas de rendimiento, conflictos con futuras instalaciones, o simplemente ocupar espacio innecesario. Los usuarios técnicos y administradores de sistemas necesitan una herramienta más exhaustiva.

Este proyecto es un reinicio de una versión anterior que tenía problemas de GUI, buscando una solución más robusta, mantenible y con mejor UX.

Referencia: `ManualDesarrollo/01_Introduccion_Proyecto.md`

## 2. Propuesta de Valor
DesinstalaPhotoshop ofrece una solución integral y profunda para la desinstalación completa de Adobe Photoshop, superando las herramientas convencionales. Proporciona control, seguridad (mediante copias de seguridad y modo de prueba) y una interfaz gráfica clara para usuarios con conocimientos técnicos.

## 3. Audiencia Objetivo
1.  **Personal de soporte técnico y administradores de sistemas.**
2.  **Usuarios técnicamente competentes.**
3.  **Desarrolladores y probadores de software.**
4.  **Técnicos de mantenimiento de equipos.**

Referencia: `ManualDesarrollo/01_Introduccion_Proyecto.md`

## 4. Descripción Funcional Detallada
La aplicación permitirá al usuario:
*   **Detectar Instalaciones:**
    *   Iniciar un escaneo del sistema para encontrar instalaciones de Photoshop y residuos.
    *   Ver una lista de elementos detectados, clasificados como "Instalación Principal", "Posible Instalación Principal" o "Residuos", con detalles como versión, ubicación y puntuación de confianza.
    *   Utilizar iconos y tooltips para identificar rápidamente el tipo y estado de cada detección.
*   **Desinstalar Photoshop:**
    *   Seleccionar una instalación principal (o posible principal) y ejecutar un proceso de desinstalación.
    *   Opcionalmente, crear una copia de seguridad antes de desinstalar.
    *   Opcionalmente, realizar una limpieza de residuos automáticamente después de la desinstalación.
*   **Limpiar Residuos:**
    *   Seleccionar elementos clasificados como residuos y ejecutar un proceso de limpieza.
    *   Opcionalmente, crear una copia de seguridad antes de limpiar.
    *   Elegir qué tipos de residuos limpiar (temporales, registro, configuración, caché) a través de `CleanupOptionsForm`.
*   **Modo de Prueba (Simulación):**
    *   Ejecutar las operaciones de detección, desinstalación o limpieza en un modo "WhatIf" que simula las acciones sin realizar cambios reales en el sistema.
    *   Ver un informe de lo que se habría hecho.
*   **Copias de Seguridad y Restauración:**
    *   Crear copias de seguridad de archivos y claves de registro antes de operaciones destructivas.
    *   Listar y seleccionar copias de seguridad existentes.
    *   Restaurar el sistema a un estado anterior a partir de una copia de seguridad.
*   **Generación de Scripts:**
    *   Generar scripts (.bat o .ps1) con comandos `reg delete` para eliminar entradas de registro que no pudieron ser eliminadas automáticamente o para revisión manual.
*   **Logging y Feedback:**
    *   Ver un log detallado de las operaciones en tiempo real en la consola de la UI.
    *   Copiar el contenido de la consola.
    *   Abrir la carpeta de archivos de log persistentes.
    *   Ver el progreso de operaciones largas mediante una barra y etiqueta de progreso.
    *   Cancelar operaciones en curso.
*   **Gestión de Permisos:**
    *   La aplicación está diseñada para ejecutarse con privilegios de administrador (configurable en `app.manifest` para desarrollo/producción).
    *   Si no se ejecuta con privilegios elevados (en modo desarrollo), solicitará reiniciar con ellos para operaciones críticas.

Referencias: `ManualDesarrollo/02_Objetivos_Proyecto.md`, `ManualDesarrollo/04_GUI_Funcionalidad_Controles.md`, `ManualDesarrollo/05_Flujo_Aplicacion.md`

## 5. Objetivos de Experiencia de Usuario (UX)
*   **Claridad:** La interfaz debe presentar la información de forma clara y comprensible, especialmente la distinción entre tipos de instalación.
*   **Control:** El usuario debe sentir que tiene control sobre las operaciones, con opciones claras y confirmaciones para acciones destructivas.
*   **Feedback:** Proporcionar feedback constante sobre el estado de las operaciones y el progreso.
*   **Eficiencia:** Permitir a los usuarios realizar tareas de limpieza profunda de manera más eficiente que los métodos manuales.
*   **Seguridad:** Transmitir seguridad mediante opciones de copia de seguridad, modo de prueba y mensajes de advertencia claros.
*   **Estética Moderna:** Utilizar un tema oscuro y diálogos personalizados (`CustomMsgBoxLibrary`) para una apariencia profesional y agradable.
*   **Responsividad:** La interfaz no debe bloquearse durante operaciones largas.

Referencias: `ManualDesarrollo/03_GUI_Descripcion_Visual.md`, `ManualDesarrollo/09_Buenas_Practicas_Lecciones.md`