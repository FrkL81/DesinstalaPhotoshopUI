# Project Brief: DesinstalaPhotoshop

## 1. Nombre del Proyecto
DesinstalaPhotoshop

## 2. Descripción General
DesinstalaPhotoshop es una aplicación desarrollada en C# para Windows, diseñada para la desinstalación completa y profunda de Adobe Photoshop. Su objetivo es eliminar no solo la instalación principal, sino también todos los archivos residuales, entradas de registro y componentes asociados que persisten tras una desinstalación estándar o incluso tras usar herramientas como Adobe CC Cleaner Tool.

Referencia: `ManualDesarrollo/01_Introduccion_Proyecto.md`

## 3. Objetivos High-Level
1.  **Detección Precisa:** Identificar de forma fiable todas las instalaciones de Photoshop (CS6 en adelante) y sus residuos, diferenciando entre instalaciones principales y restos.
2.  **Eliminación Completa:** Desinstalar y/o limpiar exhaustivamente Photoshop, incluyendo archivos, carpetas, claves de registro, configuraciones de usuario, cachés, componentes compartidos, plugins (CEP/UXP), datos de OOBE/SLStore.
3.  **Uso Eficaz de `reg.exe`:** Utilizar `reg.exe` como fallback robusto para la manipulación del registro y permitir la generación de scripts.
4.  **Sistema de Puntuación:** Implementar un sistema heurístico para clasificar las detecciones.
5.  **Informes y Scripts:** Generar reportes de operaciones y scripts de limpieza manual.
6.  **GUI Intuitiva:** Proveer una interfaz gráfica clara, responsiva y con buen feedback visual.
7.  **Seguridad y Recuperación:** Incluir creación de copias de seguridad, modo de prueba y restauración.

Referencia: `ManualDesarrollo/02_Objetivos_Proyecto.md`

## 4. Alcance
*   **Incluido:**
    *   Detección, desinstalación y limpieza de Adobe Photoshop (CS6 y versiones CC).
    *   Manejo de múltiples versiones y rastros específicos (archivos, registro).
    *   Interfaz gráfica de usuario (Windows Forms) con tema oscuro.
    *   Funcionalidades: Detección, desinstalación, limpieza de residuos, modo de prueba, copias de seguridad, restauración, generación de scripts, logging.
    *   Soporte para Windows 10 (64 bits) y superior.
*   **Excluido (Inicialmente):**
    *   Desinstalación/limpieza de otros productos Adobe (excepto componentes compartidos directamente ligados a Photoshop y que puedan ser eliminados sin afectar otras apps Adobe críticas).
    *   Soporte para sistemas operativos macOS o Linux.
    *   Soporte para versiones de Photoshop anteriores a CS6.
    *   Interfaz de línea de comandos (CLI) en esta versión.

## 5. Limitaciones Clave (Extraídas de la Documentación)
*   Puede no eliminar archivos en uso por otros procesos.
*   La eliminación programada depende de un reinicio correcto.
*   Permisos especiales más allá de Administrador pueden ser un obstáculo.
*   Instalaciones muy personalizadas podrían no ser detectadas/limpiadas completamente.
*   Posibles conflictos si otros productos Adobe CC se ejecutan durante la limpieza.
*   Limpieza principal en el contexto del usuario actual.
*   Versiones beta muy recientes o extremadamente antiguas podrían no ser 100% compatibles.

Referencia: `ManualDesarrollo/01_Introduccion_Proyecto.md`

## 6. Definición de Éxito
*   La aplicación detecta y clasifica correctamente las instalaciones de Photoshop y sus residuos.
*   La aplicación desinstala y limpia Photoshop de forma más completa que el desinstalador estándar y Adobe CC Cleaner Tool.
*   La interfaz de usuario es intuitiva, estable y proporciona feedback claro.
*   Las funciones de seguridad (copias de seguridad, modo de prueba) operan correctamente.
*   La aplicación es robusta y maneja errores de forma adecuada.
*   El código es mantenible y sigue las buenas prácticas documentadas.