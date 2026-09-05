# High FPS Support

**Movimiento más fluido en Terraria, a la frecuencia de tu pantalla.**  
Lanzador no oficial de código abierto de **pavlikmeow** · Versión **1.1.0**

[English](../README.md) · [Русский](README.ru.md) · [Deutsch](README.de.md) · **Español** · [Français](README.fr.md) · [Português (Brasil)](README.pt-BR.md) · [简体中文](README.zh-CN.md)

Terraria actualiza su mundo 60 veces por segundo. Este mod dibuja posiciones intermedias de jugadores, enemigos, proyectiles y objetos caídos, haciendo que el movimiento se vea más fluido en pantallas de 120/144/165/240 Hz. La velocidad del juego no cambia. No necesitas tModLoader.

## Requisitos

Solo se admite **Steam Terraria 1.4.5.8 para Windows, EXE original x86**. Debes tener .NET Framework 4.x y XNA Framework 4: inicia el juego original una vez desde Steam para completar sus requisitos. No se admiten otras versiones, plataformas, tModLoader ni otros parches del EXE. Usa tu propia copia con licencia. Haz una copia de tus mundos y personajes importantes: se usan las partidas normales de Terraria.

## Empieza a jugar

1. En **Releases** de este repositorio, descarga `HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip`. **Extrae todo el archivo** en una carpeta. No ejecutes el programa dentro del ZIP ni muevas solo el EXE.
2. Verifica la descarga como se indica abajo. Cierra Terraria y deja Steam abierto.
3. Abre **`HighFpsSupport.exe`**. Arriba a la derecha, en **Language / Язык**, elige **Español**. La selección se guarda y solo cambia el lanzador.
4. Comprueba la carpeta detectada. Si hace falta, selecciona la que contiene **`Terraria.exe` y `Content`**. En Steam: Terraria → Propiedades → Archivos instalados → Explorar.
5. Pulsa **Instalar y jugar**. Las próximas veces, usa **Jugar**.

El mod activa **Frame Skip: Off**. Selecciona en Windows la frecuencia real de tu pantalla. Dibujar más fotogramas requiere capacidad de CPU/GPU; no se garantiza una cifra de FPS.

**Actualizar:** cierra el juego, extrae la nueva versión del mod en otra carpeta y pulsa **Instalar / actualizar**. Tras actualizar Terraria, necesitas una versión del mod que admita expresamente esa versión del juego. Las incompatibles se rechazan.

**Eliminar:** cierra el juego y pulsa **Eliminar High FPS**. Steam sigue iniciando el juego original. Para quitarlo manualmente, borra únicamente `Terraria.HighFPS.exe`, `HighFPS.Support.dll`, `HighFPS.Support.install.txt` y `HighFPS.Support.log` de la carpeta del juego. Se conservan `Terraria.exe` y las partidas. Las preferencias del lanzador están en `%LOCALAPPDATA%\TerrariaHighFPS`; Terraria puede conservar Frame Skip: Off en su configuración.

## Verifica la descarga

Compara el hash del ZIP con las [sumas de la misma versión](release-hashes.md). Abre PowerShell en la carpeta de descarga:

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
```

El archivo incluye `SHA256SUMS.txt`. Tras extraerlo, lee `verify-release.ps1` y ejecuta desde esa carpeta:

```powershell
powershell -NoProfile -File .\verify-release.ps1
```

Si los scripts están bloqueados, compara archivos individuales con `Get-FileHash` y `SHA256SUMS.txt`; no necesitas cambiar la política de seguridad. Si algún hash difiere, no ejecutes el programa.

**Un hash coincidente confirma que el archivo coincide con una suma de confianza, no que sea inofensivo.** La aplicación no está firmada: Windows puede mostrar un editor desconocido o SmartScreen. No se afirma que exista una auditoría de seguridad independiente ni compilaciones reproducibles bit a bit. Mantén el antivirus activado.

## Cómo funciona y qué cambia

El lanzador crea `Terraria.HighFPS.exe` y `HighFPS.Support.dll` por separado. **No sobrescribe ni renombra el original.** Tres llamadas añadidas capturan el estado antes de un tick, interpolan las posiciones durante el dibujo y restauran después las coordenadas de simulación. La lógica y la red no se aceleran. La interpolación puede añadir hasta un tick de retraso visual; no afecta a todas las animaciones.

El lanzador no tiene telemetría, inicio de sesión, descargas durante su uso ni actualizaciones automáticas, y no instala servicios ni controladores. Guarda localmente la ruta, el idioma, los datos de instalación y los registros. Steam y Terraria conservan su actividad de red normal. Al compilar puede descargarse de NuGet la versión fijada de Mono.Cecil, verificando sus hashes.

Si falla: cierra completamente el juego, extrae todo el ZIP de nuevo y comprueba versión y permisos de escritura. **Instalar / actualizar** permite reparar la instalación. Consulta **Detalles técnicos** y elimina las rutas personales antes de compartir el diagnóstico. [Ayuda ampliada (EN)](guide.md) · [Seguridad (EN/RU)](../SECURITY.md) · [Arquitectura (EN)](architecture.md) · [Compilar (EN)](building.md).

## Licencia y créditos

Código y documentación propios: [MIT](../LICENSE), © 2026 pavlikmeow. Mono.Cecil tiene su propio aviso MIT. Se reconoce la idea descrita públicamente por [TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS); esta mención no autoriza copiar código sin licencia. [Avisos completos (EN/RU)](../THIRD-PARTY-NOTICES.md).

Proyecto de aficionados independiente, sin afiliación ni aprobación de Re-Logic, Valve o Microsoft. Terraria pertenece a Re-Logic; las demás marcas, a sus titulares. No se distribuyen el juego, sus recursos ni XNA. No publiques el EXE del juego generado localmente. La licencia del proyecto no concede derechos sobre esos productos; siguen aplicándose sus condiciones y la legislación correspondiente.
