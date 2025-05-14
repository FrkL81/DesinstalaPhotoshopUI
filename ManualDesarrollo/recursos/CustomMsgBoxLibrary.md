# CustomMsgBox: Diálogos Modernos y Personalizables para Windows Forms

![CustomMsgBox Demo](images/demo.png)

¡Bienvenido a **CustomMsgBox**! Si alguna vez has sentido que los cuadros de diálogo estándar de Windows Forms (`MessageBox`) se ven un poco... anticuados, ¡estás en el lugar correcto! CustomMsgBox es tu varita mágica para transformarlos en experiencias modernas, visualmente atractivas y totalmente personalizables para tus usuarios.

Imagina poder controlar cada aspecto de tus mensajes: los colores, los iconos, los botones, ¡todo! Y lo mejor, de una forma sencilla e intuitiva.

## Tabla de Contenidos

1.  [¿Por qué elegir CustomMsgBox?](#1-por-qué-elegir-custommsgbox)
2.  [Características Principales](#2-características-principales)
3.  [Requisitos Previos](#3-requisitos-previos)
4.  [¡Manos a la Obra! Instalación Paso a Paso](#4-manos-a-la-obra-instalación-paso-a-paso)
    *   [Opción 1: Referencia de Proyecto (Recomendado para principiantes)](#opción-1-referencia-de-proyecto-recomendado-para-principiantes)
    *   [Opción 2: Referencia Directa al Archivo DLL (Para usuarios más avanzados)](#opción-2-referencia-directa-al-archivo-dll-para-usuarios-más-avanzados)
5.  [Tu Primer CustomMsgBox: Uso Básico](#5-tu-primer-custommsgbox-uso-básico)
    *   [La Sintaxis Mágica: `CustomMsgBox.Show()`](#la-sintaxis-mágica-custommsgboxshow)
    *   [Ejemplo 1: Un Saludo Simple](#ejemplo-1-un-saludo-simple)
    *   [Ejemplo 2: Haciendo una Pregunta Importante](#ejemplo-2-haciendo-una-pregunta-importante)
    *   [Ejemplo 3: ¡Alerta de Error!](#ejemplo-3-alerta-de-error)
    *   [¿Qué Decidió el Usuario? Interpretando el Resultado](#qué-decidió-el-usuario-interpretando-el-resultado)
6.  [Desata tu Creatividad: Personalización Avanzada](#6-desata-tu-creatividad-personalización-avanzada)
    *   [Vistiendo tus Diálogos: Temas Visuales](#vistiendo-tus-diálogos-temas-visuales)
        *   [Temas Listos para Usar (Predefinidos)](#temas-listos-para-usar-predefinidos)
        *   [Diseñador de Modas: Creando tus Propios Temas](#diseñador-de-modas-creando-tus-propios-temas)
    *   [Un Icono Vale Más Que Mil Palabras](#un-icono-vale-más-que-mil-palabras)
    *   [El Poder de los Botones](#el-poder-de-los-botones)
7.  [Funciones Extra para Usuarios Pro](#7-funciones-extra-para-usuarios-pro)
    *   [Siempre a la Vista: Comportamiento `TopMost`](#siempre-a-la-vista-comportamiento-topmost)
    *   [¡Rápido, que se Acaba el Tiempo! Auto-Cierre](#rápido-que-se-acaba-el-tiempo-auto-cierre)
    *   [¿Quién es el Dueño? El Parámetro `Owner`](#quién-es-el-dueño-el-parámetro-owner)
    *   [Trabajando en Equipo: Seguridad de Hilos (Thread Safety)](#trabajando-en-equipo-seguridad-de-hilos-thread-safety)
8.  [Referencia Rápida de "Chuletas" (Enumeraciones)](#8-referencia-rápida-de-chuletas-enumeraciones)
    *   [`CustomDialogResult`](#customdialogresult)
    *   [`CustomMessageBoxButtons`](#custommessageboxbuttons)
    *   [`CustomMessageBoxIcon`](#custommessageboxicon)
9.  [Probando Como un Campeón: Pruebas Automatizadas](#9-probando-como-un-campeón-pruebas-automatizadas)
10. [¿Quieres Ayudar? Contribuciones](#10-quieres-ayudar-contribuciones)
11. [Los Aspectos Legales: Licencia](#11-los-aspectos-legales-licencia)
12. [Sugerencias para Imágenes Adicionales (¡Ayúdanos a Mejorar!)](#12-sugerencias-para-imágenes-adicionales-ayúdanos-a-mejorar)

---

## 1. ¿Por qué elegir CustomMsgBox?

Piénsalo así: el `MessageBox` que viene por defecto en Windows Forms es como una camiseta blanca básica. Cumple su función, sí, pero ¿no sería genial tener un guardarropa lleno de opciones para cada ocasión? CustomMsgBox te da ese guardarropa:

*   ✨ **Estética Moderna:** Diálogos que parecen diseñados en este siglo.
*   🎨 **Personalización Total:** Eres el artista. Elige colores, fuentes, iconos, ¡todo!
*   👍 **Mejor Experiencia de Usuario:** Mensajes más claros, amigables y que guían mejor al usuario.
*   🧩 **Fácil de Poner y Quitar:** Se integra en tu código casi tan fácil como el `MessageBox` original.

---

## 2. Características Principales

*   **Diseño Vanguardista**: Interfaz pulcra y adaptable. ¡Adiós, aburrimiento!
*   **Iconos con Estilo**: Gracias a [FontAwesome.Sharp](https://github.com/FortAwesome/Font-Awesome), tus iconos serán nítidos y profesionales.
*   **Protagonista Indiscutible (`TopMost`)**: Por defecto, tus mensajes importantes no se perderán detrás de otras ventanas.
*   **Tic-Tac, Cierre Automático**: ¿Un mensaje que solo debe mostrarse unos segundos? ¡Hecho!
*   **A Prueba de Multitareas (Seguridad de Hilos)**: Llama a CustomMsgBox desde cualquier parte de tu código, sin preocupaciones.
*   **Un Tema para Cada Personalidad**: Oscuro (el clásico elegante), Claro (fresco y limpio), Azul (corporativo y confiable), Verde (natural y positivo) y Alto Contraste (accesibilidad total).
*   **Botones Inteligentes**: Todas las combinaciones que necesitas, y sabes exactamente qué eligió el usuario.
*   **Amigable para Desarrolladores**: Su API es tan familiar que te sentirás como en casa.

---

## 3. Requisitos Previos

Para que CustomMsgBox funcione a la perfección, necesitas:

*   **.NET 9.0 o superior** (para la librería y la app de pruebas tal como están). Si usas versiones anteriores de .NET Framework o .NET Core/5+ que soporten Windows Forms, ¡también debería funcionar!
*   Un proyecto de **Windows Forms**.
*   La librería **FontAwesome.Sharp**. Pero no te preocupes, si usas NuGet (el gestor de paquetes de .NET), se instalará sola como por arte de magia.

---

## 4. ¡Manos a la Obra! Instalación Paso a Paso

Integrar CustomMsgBox en tu proyecto es como armar un LEGO fácil. Tienes dos caminos:

### Opción 1: Referencia de Proyecto (Recomendado para principiantes)

Esta es la vía rápida y sencilla si tienes el código fuente de CustomMsgBox.

1.  **Añade el Proyecto a tu Solución de Visual Studio**:
    *   Abre tu proyecto principal en Visual Studio.
    *   En el "Explorador de Soluciones" (esa ventana que lista todos tus archivos), haz clic derecho sobre el nombre de tu **Solución** (usualmente el primer elemento en la lista).
    *   Ve a `Agregar` -> `Proyecto existente...`.
    *   Busca la carpeta `CustomMsgBoxSolution/CustomMsgBoxLibrary` y elige el archivo `CustomMsgBoxLibrary.csproj`. ¡Listo! Ya forma parte de tu equipo.

    *(Sugerencia de Imagen: `integracion_paso1_solution_explorer.png` - Una captura de pantalla mostrando el Explorador de Soluciones con CustomMsgBoxLibrary felizmente añadido).*

2.  **Conecta tu Proyecto Principal con CustomMsgBox**:
    *   Ahora, en el mismo "Explorador de Soluciones", haz clic derecho sobre **tu proyecto principal** (el que va a mostrar los mensajes bonitos).
    *   Selecciona `Agregar` -> `Referencia de proyecto...`.
    *   Aparecerá una ventana. Busca la pestaña `Proyectos` (o similar), marca la casilla junto a `CustomMsgBoxLibrary` y dale a `Aceptar`. ¡Ya están conectados!

    *(Sugerencia de Imagen: `integracion_paso2_add_reference.png` - Una captura del diálogo "Administrador de referencias" con CustomMsgBoxLibrary sonriendo y seleccionado).*

### Opción 2: Referencia Directa al Archivo DLL (Para usuarios más avanzados)

Si solo tienes el archivo `CustomMsgBoxLibrary.dll` (el "ejecutable" de la librería).

1.  **Consigue el DLL (si no lo tienes)**:
    *   Abre la solución `CustomMsgBoxSolution.sln` en Visual Studio.
    *   Arriba, busca una lista desplegable que dice `Debug`. Cámbiala a `Release` para una versión optimizada.
    *   En el "Explorador de Soluciones", haz clic derecho en `CustomMsgBoxLibrary` y selecciona `Compilar` (o `Recompilar`).
    *   El tesoro (`CustomMsgBoxLibrary.dll`) estará en una carpeta como `CustomMsgBoxSolution/CustomMsgBoxLibrary/bin/Release/net9.0-windows/`. La ruta exacta puede variar un poquito.

2.  **Añade la Referencia DLL a tu Proyecto**:
    *   En tu proyecto principal, haz clic derecho y elige `Agregar` -> `Referencia...`.
    *   En la ventana que aparece, abajo, busca y haz clic en el botón `Examinar...`.
    *   Navega hasta donde guardaste `CustomMsgBoxLibrary.dll`, selecciónalo y haz clic en `Agregar`.
    *   Asegúrate de que aparezca marcado en la lista y presiona `Aceptar`.

    *(Sugerencia de Imagen: `integracion_dll_browse_reference.png` - El "Administrador de referencias" mostrando cómo buscar ese DLL mágico).*

---

## 5. Tu Primer CustomMsgBox: Uso Básico

¡Llegó la hora de la diversión! Usar CustomMsgBox es súper fácil.

Primero, en el archivo de C# donde quieras mostrar un mensaje, añade estas líneas al principio:

```csharp
using CustomMsgBoxLibrary;
using CustomMsgBoxLibrary.Types; // ¡Importante! Para usar los tipos de resultado, botones e iconos.
```

### La Sintaxis Mágica: `CustomMsgBox.Show()`

La función principal es `CustomMsgBox.Show()`. Piensa en ella como un hechizo con varios ingredientes (parámetros). Algunos son obligatorios, otros opcionales (¡y muy útiles!).

**La Receta General (con argumentos con nombre, ¡la forma más clara!):**

```csharp
CustomMsgBox.Show(
    prompt: "Tu mensaje aquí", // El texto que verá el usuario (¡Obligatorio!)
    title: "El título de la ventana", // Opcional
    buttons: CustomMessageBoxButtons.OK, // Qué botones mostrar (Opcional)
    icon: CustomMessageBoxIcon.Information, // Qué icono usar (Opcional)
    defaultButton: MessageBoxDefaultButton.Button1, // Qué botón se activa con 'Enter' (Opcional)
    theme: ThemeSettings.DarkTheme, // ¡Elige un estilo! (Opcional)
    autoCloseMilliseconds: 0, // Para que se cierre solo (0 = no se cierra solo) (Opcional)
    topMost: true, // Para que esté siempre encima (Opcional)
    // owner: this, // Si quieres que pertenezca a una ventana específica (Opcional)
    // helpFilePath: "ruta/ayuda.chm", // Para ayuda futura (Opcional)
    // helpKeyword: "temaDeAyuda" // Para ayuda futura (Opcional)
);
```

**¿Qué son los "argumentos con nombre"?** Son como etiquetas para tus ingredientes. En lugar de depender del orden, dices `ingrediente: valor`. Así, tu código es más fácil de leer y no te equivocas de posición. ¡Altamente recomendado!

**Tabla de Ingredientes (Parámetros):**

| Parámetro             | Tipo                     | Obligatorio/Opcional                 | Descripción Corta                                                                | Valor por Defecto (si se omite)    |
| :-------------------- | :----------------------- | :----------------------------------- | :------------------------------------------------------------------------------- | :--------------------------------- |
| `owner`               | `IWin32Window`           | Opcional (a través de sobrecarga)  | La ventana "madre" del diálogo.                                                  | `null` (se centra en pantalla)     |
| `prompt`              | `string`                 | **¡SÍ!**                             | El mensaje principal. ¡Lo más importante!                                          | N/A                                |
| `title`               | `string`                 | Opcional                             | El título en la barra superior del diálogo.                                        | `"Mensaje"`                        |
| `buttons`             | `CustomMessageBoxButtons`  | Opcional                             | ¿"OK"? ¿"Sí/No"? Tú eliges los botones.                                           | `CustomMessageBoxButtons.OK`       |
| `icon`                | `CustomMessageBoxIcon`   | Opcional                             | Una imagen que acompaña al mensaje (info, error, etc.).                           | `CustomMessageBoxIcon.None`        |
| `defaultButton`       | `MessageBoxDefaultButton`| Opcional                             | El botón que se "presiona" si el usuario da Enter.                               | `MessageBoxDefaultButton.Button1`  |
| `theme`               | `ThemeSettings?`         | Opcional                             | El estilo visual del diálogo.                                                    | `ThemeSettings.DarkTheme`          |
| `autoCloseMilliseconds` | `int`                  | Opcional                             | Si quieres que desaparezca solo (en milisegundos, 0 para no).                    | `0`                                |
| `topMost`             | `bool`                   | Opcional                             | `true` para que siempre esté visible por encima de otras ventanas.                 | `true`                             |
| `helpFilePath`        | `string?`                | Opcional                             | (Para el futuro) Ruta a un archivo de ayuda.                                     | `null`                             |
| `helpKeyword`         | `string?`                | Opcional                             | (Para el futuro) Palabra clave de ayuda.                                         | `null`                             |

### Ejemplo 1: Un Saludo Simple

Vamos a mostrar un mensaje sencillo, como si fuera el `MessageBox.Show("Hola Mundo")` de toda la vida, pero con estilo.

```csharp
CustomMsgBox.Show(
    prompt: "¡Bienvenido a mi increíble aplicación!",
    title: "Saludo Cordial"
);
```
Como no especificamos `buttons`, `icon` ni `theme`, usará los valores por defecto: un botón "Aceptar", sin icono, y el tema oscuro.

*(Sugerencia de Imagen: `uso_basico_simple.png` - Una captura del diálogo de este ejemplo).*

### Ejemplo 2: Haciendo una Pregunta Importante

Ahora, preguntemos algo al usuario y démosle opciones.

```csharp
CustomDialogResult respuesta = CustomMsgBox.Show(
    prompt: "¿Desea activar las notificaciones avanzadas?",
    title: "Configuración de Notificaciones",
    buttons: CustomMessageBoxButtons.YesNo, // Botones "Sí" y "No"
    icon: CustomMessageBoxIcon.Question    // Icono de pregunta
);
```
Aquí, `respuesta` guardará lo que el usuario eligió.

*(Sugerencia de Imagen: `uso_basico_pregunta.png` - Una captura de este diálogo de pregunta).*

### Ejemplo 3: ¡Alerta de Error!

Imaginemos que algo salió mal y necesitamos informar al usuario, dándole opciones para reintentar o cancelar.

```csharp
CustomDialogResult accionUsuario = CustomMsgBox.Show(
    prompt: "No se pudo conectar al servidor. Por favor, verifique su conexión a internet.",
    title: "Error de Conexión",
    buttons: CustomMessageBoxButtons.RetryCancel, // Botones "Reintentar" y "Cancelar"
    icon: CustomMessageBoxIcon.Error,           // Icono de error
    theme: ThemeSettings.LightTheme             // Cambiamos al tema claro para este error
);
```
En este caso, también personalizamos el tema a `LightTheme` para que destaque.

### ¿Qué Decidió el Usuario? Interpretando el Resultado

El método `CustomMsgBox.Show()` devuelve un valor de tipo `CustomDialogResult`. Este valor te dice qué botón presionó el usuario.

```csharp
// Continuando el Ejemplo 2:
if (respuesta == CustomDialogResult.Yes)
{
    // El usuario dijo ¡SÍ!
    MessageBox.Show("¡Genial! Notificaciones avanzadas activadas.");
}
else if (respuesta == CustomDialogResult.No)
{
    // El usuario dijo NO.
    MessageBox.Show("Entendido. Las notificaciones avanzadas seguirán desactivadas.");
}

// Continuando el Ejemplo 3:
if (accionUsuario == CustomDialogResult.Retry)
{
    // El usuario quiere reintentar.
    // Aquí pondrías el código para intentar la conexión de nuevo.
}
else if (accionUsuario == CustomDialogResult.Cancel)
{
    // El usuario canceló.
    // Quizás cierras una ventana o vuelves al estado anterior.
}
```

---

## 6. Desata tu Creatividad: Personalización Avanzada

Aquí es donde CustomMsgBox realmente brilla. ¡Prepárate para ser un artista de los diálogos!

### Vistiendo tus Diálogos: Temas Visuales

Los temas cambian completamente la apariencia de tus mensajes.

![Temas CustomMsgBox](images/themes.png)
*(Imagen mostrando varios temas: Oscuro, Claro, Azul, Verde, Alto Contraste)*

#### Temas Listos para Usar (Predefinidos)

Tenemos un set de temas listos para que los uses directamente:

*   `ThemeSettings.DarkTheme` (el guapo por defecto)
*   `ThemeSettings.LightTheme`
*   `ThemeSettings.BlueTheme`
*   `ThemeSettings.GreenTheme`
*   `ThemeSettings.HighContrastTheme` (ideal para accesibilidad)

**Ejemplo: Usando el Tema Azul**

```csharp
CustomMsgBox.Show(
    prompt: "Este mensaje utiliza el elegante Tema Azul.",
    title: "Notificación Corporativa",
    icon: CustomMessageBoxIcon.Information,
    theme: ThemeSettings.BlueTheme // ¡Aplicando el estilo!
);
```

#### Diseñador de Modas: Creando tus Propios Temas

¿Ninguno te convence al 100%? ¡No hay problema! Crea tu propio tema a medida.

**Paso 1: Crea una instancia de `ThemeSettings`.**
Puedes empezar desde cero o, más fácil, clonar un tema existente y modificarlo.

```csharp
// Opción A: Clonar y modificar
ThemeSettings miTemaSuperOriginal = ThemeSettings.CreateFrom(ThemeSettings.DarkTheme); // Copiamos el tema oscuro

// Opción B: Usar un creador rápido (ideal para colores primarios y secundarios)
ThemeSettings miTemaVibrante = ThemeSettings.CreateCustomTheme(
    primaryColor: Color.OrangeRed,  // Color de fondo principal
    secondaryColor: Color.DarkOrange, // Color para botones, etc.
    textColor: Color.White,
    isDarkTheme: true // Ayuda a elegir buenos colores por defecto para iconos en temas oscuros
);
```

**Paso 2: ¡A personalizar se ha dicho!**
Modifica las propiedades que quieras. Hay muchísimas:

```csharp
miTemaSuperOriginal.BackgroundColor = Color.FromArgb(20, 20, 40); // Un azul noche profundo
miTemaSuperOriginal.TitleBarColor = Color.FromArgb(10, 10, 30);
miTemaSuperOriginal.TextColor = Color.Gold;
miTemaSuperOriginal.ButtonBackColor = Color.SlateBlue;
miTemaSuperOriginal.ButtonForeColor = Color.White;
miTemaSuperOriginal.ButtonBorderColor = Color.LightSteelBlue;
miTemaSuperOriginal.ButtonCornerRadius = 10; // ¡Botones con esquinas redondeadas!
miTemaSuperOriginal.IconWarningColor = Color.Orange;
miTemaSuperOriginal.Opacity = 0.98; // Un toque de transparencia (0.0 a 1.0)
miTemaSuperOriginal.MessageFont = new Font("Georgia", 11f, FontStyle.Italic); // Fuente personalizada para el mensaje
miTemaSuperOriginal.TitleFont = new Font("Arial", 10f, FontStyle.Bold); // Fuente para el título
```

**Paso 3: ¡Luce tu creación!**

```csharp
CustomMsgBox.Show(
    prompt: "Este mensaje luce mi nuevo tema súper original. ¿A que es una pasada?",
    title: "¡Diseño Exclusivo!",
    icon: CustomMessageBoxIcon.Confirmation,
    theme: miTemaSuperOriginal // ¡Aquí va tu obra de arte!
);
```

*(Sugerencia de Imagen: `personalizacion_tema_ejemplo.png` - Una captura de un diálogo con un tema muy personalizado y llamativo).*

### Un Icono Vale Más Que Mil Palabras

Los iconos ayudan a transmitir el propósito del mensaje de un vistazo.

| Icono Enum                  | Imagen Muestra                                         | Descripción                                  |
| :-------------------------- | :----------------------------------------------------- | :------------------------------------------- |
| `CustomMessageBoxIcon.None` | *(Sin Imagen)*                                         | Sin icono. Limpio y directo.                 |
| `Information`               | ![Information Icon](images/icons/Information.png)      | Para información general, neutra.            |
| `Warning`                   | ![Warning Icon](images/icons/Warning.png)              | ¡Atención! Algo podría no estar bien.        |
| `Error`                     | ![Error Icon](images/icons/Error.png)                  | ¡Ups! Ocurrió un error.                      |
| `CriticalError`             | ![Critical Error Icon](images/icons/CriticalError.png) | ¡Emergencia! Un error grave.                 |
| `Question`                  | ![Question Icon](images/icons/Question.png)            | ¿Necesitas una respuesta del usuario?        |
| `Success`                   | ![Success Icon](images/icons/Success.png)              | ¡Todo salió perfecto!                        |
| `Confirmation`              | ![Confirmation Icon](images/icons/Confirmation.png)    | Para confirmar una acción positiva.          |
| `Progress`                  | ![Progress Icon](images/icons/Progress.png)            | Algo se está procesando (¡gira!).            |
| `Alert`                     | ![Alert Icon](images/icons/Alert.png)                  | Una alerta importante.                       |
| `Notification`              | ![Notification Icon](images/icons/Notification.png)    | Una notificación menos urgente.              |
| `Lock`                      | ![Lock Icon](images/icons/Lock.png)                    | Algo está bloqueado o es seguro.             |
| `Unlock`                    | ![Unlock Icon](images/icons/Unlock.png)                | Algo se ha desbloqueado.                     |
| `Settings`                  | ![Settings Icon](images/icons/Settings.png)            | Relacionado con configuraciones.             |

**Ejemplo: Un mensaje de éxito rotundo**

```csharp
CustomMsgBox.Show(
    prompt: "¡Tu perfil se ha actualizado correctamente!",
    title: "Actualización Exitosa",
    icon: CustomMessageBoxIcon.Success, // Icono de check verde
    theme: ThemeSettings.GreenTheme    // Un tema verde para acompañar el éxito
);
```

### El Poder de los Botones

Define qué opciones le das al usuario.

| Botones Enum                      | Imágenes Muestra                                                                                                 | Descripción                                           |
| :-------------------------------- | :--------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
| `CustomMessageBoxButtons.OK`      | ![OK Button](images/buttons/OK.png)                                                                              | Un solo botón: "Aceptar". Simple y efectivo.          |
| `OKCancel`                        | ![OK Button](images/buttons/OK.png) ![Cancel Button](images/buttons/Cancel.png)                                  | Dos opciones: "Aceptar" o "Cancelar".                 |
| `AbortRetryIgnore`                | ![Abort Button](images/buttons/Abort.png) ![Retry Button](images/buttons/Retry.png) ![Ignore Button](images/buttons/Ignore.png) | Tres salidas: "Abortar", "Reintentar", "Ignorar".   |
| `YesNoCancel`                     | ![Yes Button](images/buttons/Yes.png) ![No Button](images/buttons/No.png) ![Cancel Button](images/buttons/Cancel.png) | La clásica triple opción: "Sí", "No", "Cancelar".   |
| `YesNo`                           | ![Yes Button](images/buttons/Yes.png) ![No Button](images/buttons/No.png)                                        | Decisión binaria: "Sí" o "No".                      |
| `RetryCancel`                     | ![Retry Button](images/buttons/Retry.png) ![Cancel Button](images/buttons/Cancel.png)                            | Para reintentar una acción o cancelarla.              |

**Ejemplo: ¿Guardar o no guardar antes de cerrar?**

```csharp
CustomDialogResult decision = CustomMsgBox.Show(
    prompt: "Hay cambios sin guardar. ¿Desea guardarlos antes de cerrar?",
    title: "Cambios Pendientes",
    buttons: CustomMessageBoxButtons.YesNoCancel, // Sí, No, o Cancelar el cierre
    icon: CustomMessageBoxIcon.Warning,
    defaultButton: MessageBoxDefaultButton.Button1 // "Sí" será el botón por defecto (el primero)
);

if (decision == CustomDialogResult.Yes) { /* Guardar y cerrar */ }
else if (decision == CustomDialogResult.No) { /* Cerrar sin guardar */ }
else if (decision == CustomDialogResult.Cancel) { /* No hacer nada, no cerrar */ }
```

---

## 7. Funciones Extra para Usuarios Pro

CustomMsgBox tiene algunos trucos bajo la manga:

### Siempre a la Vista: Comportamiento `TopMost`

Por defecto, tus CustomMsgBox son como celebridades en alfombra roja: siempre quieren estar al frente (`TopMost = true`). Si prefieres que puedan ser opacados por otras ventanas:

```csharp
CustomMsgBox.Show(
    prompt: "Este mensaje puede quedar detrás si haces clic en otra ventana.",
    title: "Modo No-Protagonista",
    topMost: false // ¡Aquí está el truco!
);
```

### ¡Rápido, que se Acaba el Tiempo! Auto-Cierre

Para mensajes informativos que no requieren interacción y deben desaparecer solos:

```csharp
int duracionMs = 3500; // 3.5 segundos

CustomDialogResult resultado = CustomMsgBox.Show(
    prompt: $"Esta notificación desaparecerá en {duracionMs / 1000.0} segundos...",
    title: "Aviso Temporal",
    icon: CustomMessageBoxIcon.Notification,
    autoCloseMilliseconds: duracionMs,
    theme: ThemeSettings.BlueTheme
);

if (resultado == CustomDialogResult.Timeout)
{
    Console.WriteLine("El mensaje se fue solito, como se esperaba.");
}
else
{
    // Si el usuario logró hacer clic en OK (si hubiera un botón OK) antes del tiempo.
    Console.WriteLine("¡El usuario fue más rápido que el temporizador!");
}
```

### ¿Quién es el Dueño? El Parámetro `Owner`

Puedes hacer que un CustomMsgBox "pertenezca" a una ventana específica de tu aplicación. Esto es útil para:

1.  **Centrarlo** respecto a esa ventana.
2.  Hacerlo **modal** a esa ventana (el usuario no podrá usar la ventana dueña hasta cerrar el mensaje).

```csharp
// Imagina que este código está dentro de un botón en tu Formulario llamado 'MiFormularioPrincipal'
public partial class MiFormularioPrincipal : Form
{
    private void btnMostrarMensajeImportante_Click(object sender, EventArgs e)
    {
        CustomMsgBox.Show(
            owner: this, // 'this' es MiFormularioPrincipal. ¡El mensaje le pertenece!
            prompt: "Este es un mensaje importante para MiFormularioPrincipal.",
            title: "Atención Requerida",
            icon: CustomMessageBoxIcon.Alert
        );
    }
}
```

### Trabajando en Equipo: Seguridad de Hilos (Thread Safety)

A veces, necesitas mostrar un mensaje desde una parte de tu código que no es el hilo principal de la interfaz de usuario (UI). ¡No te preocupes! CustomMsgBox es inteligente y se encarga de mostrar el mensaje en el hilo correcto automáticamente.

```csharp
using System.Threading.Tasks;

// ... en alguna función ...
Task.Run(() => {
    // Esta tarea se ejecuta en un hilo diferente...
    // ¡Pero CustomMsgBox lo maneja sin problemas!
    CustomMsgBox.Show(
        prompt: "Tarea en segundo plano completada.",
        title: "Notificación de Tarea",
        icon: CustomMessageBoxIcon.Information
    );
});
```

---

## 8. Referencia Rápida de "Chuletas" (Enumeraciones)

Estos son los tipos especiales que usarás para configurar CustomMsgBox:

### `CustomDialogResult`

Te dice qué botón presionó el usuario.

| Valor      | Descripción                                     |
| :--------- | :---------------------------------------------- |
| `None`     | (Raro) Ningún resultado.                        |
| `OK`       | El usuario presionó "Aceptar".                   |
| `Cancel`   | El usuario presionó "Cancelar".                  |
| `Abort`    | El usuario presionó "Abortar".                   |
| `Retry`    | El usuario presionó "Reintentar".                |
| `Ignore`   | El usuario presionó "Ignorar".                   |
| `Yes`      | El usuario presionó "Sí".                        |
| `No`       | El usuario presionó "No".                        |
| `Timeout`  | El mensaje se cerró solo por tiempo de espera.  |

### `CustomMessageBoxButtons`

Define qué botones aparecen. (Mira la sección [El Poder de los Botones](#el-poder-de-los-botones) para verlos en acción).

### `CustomMessageBoxIcon`

Elige el icono que mejor represente tu mensaje. (Revisa [Un Icono Vale Más Que Mil Palabras](#un-icono-vale-más-que-mil-palabras) para ver la galería).

---

## 9. Probando Como un Campeón: Pruebas Automatizadas

Si eres de los que les gusta que su código se pruebe solo (¡muy bien hecho!), CustomMsgBox te ayuda. Tiene un "modo de prueba" que simula la interacción sin mostrar ventanas reales.

```csharp
// En tu proyecto de pruebas:

// Paso 1: Activar el modo fantasma (modo de prueba)
CustomMsgBox.SetTestMode(true);

// Paso 2: (Opcional, pero útil) Dile qué quieres que "responda"
// Si no haces esto, generalmente simulará la primera opción o "Aceptar".
CustomMsgBox.SetTestResponse("Sí", CustomDialogResult.Yes);
CustomMsgBox.SetTestResponse("Cancelar", CustomDialogResult.Cancel);

// Paso 3: Llama a CustomMsgBox.Show() como si nada
// No verás ninguna ventana, ¡pero internamente está funcionando!
CustomDialogResult resultadoSimulado = CustomMsgBox.Show(
    prompt: "¿Realizar la acción de prueba X?",
    title: "Prueba Automatizada XYZ",
    buttons: CustomMessageBoxButtons.YesNoCancel, // Ofrece Sí, No, Cancelar
    icon: CustomMessageBoxIcon.Question
);

// Paso 4: Comprueba si "eligió" lo que esperabas
// Si configuraste que "Sí" devuelve Yes, y el primer botón es "Sí"...
// Assert.AreEqual(CustomDialogResult.Yes, resultadoSimulado); // Ejemplo con una librería de pruebas

Console.WriteLine($"Resultado de la simulación: {resultadoSimulado}"); // Debería ser Yes

// Paso 5: ¡No olvides desactivar el modo fantasma cuando termines tus pruebas!
CustomMsgBox.SetTestMode(false);
```
Esto es súper útil para pruebas unitarias o de integración. La aplicación `TestApp` que viene con CustomMsgBox tiene más ejemplos.

---

## 10. ¿Quieres Ayudar? Contribuciones

¡Nos encantaría tu ayuda! Si tienes ideas, encuentras un error, o quieres añadir una nueva función, echa un vistazo a nuestro archivo `CONTRIBUTING.md` para saber cómo.

---

## 11. Los Aspectos Legales: Licencia

CustomMsgBox es libre y de código abierto, bajo la Licencia MIT. Puedes ver los detalles en el archivo `LICENSE`.

---

## 12. Sugerencias para Imágenes Adicionales (¡Ayúdanos a Mejorar!)

Para que esta guía sea aún más visual y fácil para principiantes, ¡unas cuantas imágenes más vendrían de perlas! Si te animas a crearlas, serían geniales:

*   **`integracion_paso1_solution_explorer.png`**: Una captura del Explorador de Soluciones de Visual Studio justo después de añadir `CustomMsgBoxLibrary` a una solución de ejemplo.
*   **`integracion_paso2_add_reference.png`**: Captura del diálogo "Administrador de referencias" (cuando haces clic derecho en tu proyecto -> Agregar -> Referencia de proyecto...), mostrando `CustomMsgBoxLibrary` seleccionado.
*   **`integracion_dll_browse_reference.png`**: Captura del "Administrador de referencias" cuando se usa la opción "Examinar..." para añadir un DLL.
*   **`uso_basico_simple.png`**: El diálogo del "Ejemplo 1: Un Saludo Simple" en acción (tema oscuro).
*   **`uso_basico_pregunta.png`**: El diálogo del "Ejemplo 2: Haciendo una Pregunta Importante" (tema oscuro).
*   **`personalizacion_tema_ejemplo.png`**: Un diálogo que muestre un tema realmente personalizado y diferente, como el `miTemaSuperOriginal` del ejemplo.

Con estas imágenes, la guía sería aún más espectacular. ¡Gracias por leer y esperamos que disfrutes usando CustomMsgBox!