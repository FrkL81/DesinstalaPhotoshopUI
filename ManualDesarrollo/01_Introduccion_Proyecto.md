# Introducción al Proyecto

## Nombre del Proyecto

**DesinstalaPhotoshop**

## Descripción General

DesinstalaPhotoshop es una aplicación desarrollada en C# que proporciona una solución completa para la desinstalación de Adobe Photoshop en sistemas Windows. A diferencia del desinstalador estándar proporcionado por Adobe, esta herramienta está diseñada para realizar una limpieza profunda del sistema, eliminando todos los archivos residuales (incluyendo caché específica de fuentes, Camera Raw, configuraciones de OOBE, datos de plugins UXP/CEP), entradas de registro (como las relacionadas con asociaciones de archivos y versiones específicas en HKLM y HKCU) y otros componentes que normalmente permanecen después de una desinstalación convencional, incluso aquellos que herramientas como Adobe CC Cleaner Tool podrían omitir.

## Público Objetivo

Esta herramienta está diseñada principalmente para:

1. **Personal de soporte técnico y administradores de sistemas**: Profesionales que necesitan asegurar una eliminación completa de Adobe Photoshop en entornos empresariales o educativos.

2. **Usuarios técnicamente competentes**: Usuarios con conocimientos técnicos intermedios o avanzados que desean una limpieza más profunda que la proporcionada por el desinstalador estándar.

3. **Desarrolladores y probadores de software**: Profesionales que necesitan limpiar completamente el sistema entre pruebas de diferentes versiones de Photoshop.

4. **Técnicos de mantenimiento de equipos**: Personas encargadas de preparar o restaurar equipos, que necesitan eliminar completamente el software instalado.

La aplicación, aunque cuenta con una interfaz intuitiva, está orientada a usuarios con cierto nivel de conocimiento técnico debido a la naturaleza potencialmente sensible de las operaciones que realiza (modificación del registro, eliminación de archivos del sistema, etc.).

La aplicación ofrece una interfaz gráfica intuitiva que permite a los usuarios:

1. Detectar instalaciones de Adobe Photoshop en el sistema
2. Desinstalar completamente las instalaciones detectadas
3. Limpiar archivos residuales y entradas de registro
4. Crear copias de seguridad antes de realizar cambios
5. Simular operaciones en un modo de prueba sin realizar cambios reales
6. Generar informes detallados y scripts de limpieza

## Tecnologías Utilizadas

- **Lenguaje de programación**: C#
- **Framework**: .NET 9.0
- **Interfaz gráfica**: Windows Forms
- **Arquitectura**: Aplicación de escritorio con separación en capas (Core y UI)
- **Paradigma**: Programación orientada a objetos
- **Características avanzadas**:
  - Operaciones asíncronas para mantener la UI responsiva
  - Sistema de logging integrado
  - Gestión de copias de seguridad
  - Detección heurística de instalaciones
  - Generación de scripts de limpieza
  - Interfaz de usuario mejorada con diálogos modernos y personalizables (mediante `CustomMsgBoxLibrary`)

## Contexto del Proyecto

Este proyecto surge como una evolución y mejora de una solución anterior basada en PowerShell. El script original `CleanPhotoshop.ps1` proporcionaba funcionalidad para desinstalar Photoshop, pero presentaba varias limitaciones:

- Interfaz de línea de comandos poco amigable para usuarios no técnicos
- Falta de retroalimentación visual durante procesos largos
- Problemas de rendimiento en operaciones de limpieza extensas
- Dificultad para mantener y extender la funcionalidad
- Experiencia de usuario inconsistente

La aplicación actual mantiene y mejora todas las capacidades del script original, pero implementándolas en un entorno más robusto, mantenible y con una experiencia de usuario mejorada.

## Justificación del Reinicio

El proyecto actual representa un reinicio completo de una versión anterior que presentaba problemas significativos, principalmente relacionados con la interfaz gráfica. Específicamente:

1. **Problemas de visibilidad de la GUI**: La versión anterior sufría de problemas donde la interfaz gráfica no era visible correctamente, lo que hacía la aplicación inutilizable para los usuarios finales.

2. **Arquitectura mejorada**: El reinicio permitió implementar una arquitectura más limpia con una clara separación entre la lógica de negocio (Core) y la interfaz de usuario (UI).

3. **Aprovechamiento de características modernas**: La nueva implementación utiliza características modernas de C# y .NET 9, como la programación asíncrona, que mejoran significativamente el rendimiento y la experiencia del usuario.

4. **Mantenibilidad**: La estructura modular del nuevo proyecto facilita su mantenimiento y extensión futura.

5. **Tema oscuro nativo**: Se implementó un tema oscuro nativo que mejora la experiencia visual y se alinea con las tendencias actuales de diseño de interfaces.

Este reinicio ha permitido crear una aplicación más robusta, con mejor rendimiento y una experiencia de usuario significativamente mejorada, manteniendo todas las capacidades de limpieza profunda del script original.

## Limitaciones Conocidas y Casos Extremos

Aunque DesinstalaPhotoshop está diseñado para ser una herramienta robusta y completa, existen ciertas limitaciones y casos extremos que deben tenerse en cuenta:

### Limitaciones Técnicas

1. **Archivos en uso**: La aplicación puede no eliminar archivos que están siendo utilizados por otros procesos. Aunque intenta detener procesos relacionados con Adobe, algunos servicios del sistema pueden mantener bloqueos en ciertos archivos.

2. **Eliminación programada**: La eliminación de archivos persistentes se programa para el próximo reinicio utilizando `MoveFileEx` con `MOVEFILE_DELAY_UNTIL_REBOOT`. Si el sistema no se reinicia correctamente o se produce un bloqueo antes del reinicio, estos archivos no serán eliminados.

3. **Permisos de sistema**: Algunas claves de registro y archivos pueden requerir permisos especiales que van más allá de los privilegios de administrador estándar, especialmente en sistemas con políticas de seguridad restrictivas.

4. **Instalaciones personalizadas**: Las instalaciones de Adobe muy personalizadas o modificadas pueden no ser detectadas o limpiadas correctamente, ya que pueden no seguir los patrones estándar que la aplicación busca.

### Casos Extremos

1. **Conflictos con otros productos Adobe**: La limpieza de Photoshop puede afectar a componentes compartidos con otros productos de Adobe Creative Cloud si se ejecutan simultáneamente durante la limpieza.

2. **Sistemas con múltiples usuarios**: La aplicación limpia principalmente el contexto del usuario actual y las áreas del sistema. Los perfiles de otros usuarios pueden contener residuos que no serán eliminados.

3. **Versiones muy antiguas o muy recientes**: Las versiones extremadamente antiguas de Photoshop (anteriores a CS) o versiones beta muy recientes pueden no ser completamente compatibles con los métodos de detección y limpieza implementados.

4. **Sistemas con modificaciones profundas**: Sistemas que han sido significativamente modificados, como aquellos con herramientas de personalización profunda o sistemas operativos modificados, pueden presentar comportamientos inesperados.

### Recomendaciones para Casos Especiales

1. **Crear siempre copias de seguridad** antes de realizar operaciones de limpieza o desinstalación.
2. **Reiniciar el sistema** después de operaciones de limpieza para asegurar que los archivos programados para eliminación sean procesados.
3. **Verificar la compatibilidad** con otros productos de Adobe instalados antes de realizar limpiezas agresivas.
4. **Utilizar el modo de prueba** para simular operaciones antes de ejecutarlas realmente, especialmente en entornos críticos.
