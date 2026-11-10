# Controles interactivos del campus

La pantalla presenta las ocho acciones existentes: izquierda, derecha, saltar, correr, ataque básico, interactuar, poder equipado y pausa. **Teclado** y **Mando** seleccionan una vista y un borrador de asignaciones. No se añaden acciones de inventario, cámara libre o agacharse que el controlador del juego no implementa.

## Entrada, edición y prueba

- La lista y el esquema muestran el borrador actual; al pulsar **Aplicar controles**, se guarda en los mismos campos `actionKeys` y `padBindings` utilizados por el juego.
- Una tecla o botón ya asignado abre una confirmación para intercambiar las dos acciones. **Cancelar** está seleccionado inicialmente. No se generan duplicados silenciosos.
- **Restaurar valores** prepara los ocho controles originales de teclado y mando, sin borrar el progreso del campus. Se guardan al pulsar Aplicar.
- Salir con cambios selecciona inicialmente **Seguir editando**. Cancelar una captura conserva la asignación previa. La captura caduca a los veinte segundos.
- **Probar entrada** aísla el diagnóstico de la navegación: las pulsaciones iluminan teclas, botones y gatillos; los sticks desplazan sus indicadores. La prueba no crea un nivel ni mueve al personaje.
- `Esc` / botón Este (`B` o Círculo) cancela la captura o termina la prueba. Para asignar ese botón reservado de cancelación se puede utilizar el selector visual de botones de la captura.
- En el teclado, pulsar una tecla asignada en el dibujo selecciona su acción. Durante una captura se puede escribir físicamente o elegir una tecla en el esquema.
- Los gatillos se admiten tanto en captura como en acciones de una sola pulsación. Mantenerlos no repite continuamente una acción; correr sigue siendo una acción mantenida.

## Detección y tipos de mando

La interfaz ofrece únicamente **Xbox** y **PlayStation**; no existe una opción Auto. Al aparecer un dispositivo compatible o recibir actividad, su tipo y descripción determinan el esquema. Xbox/XInput utiliza A/B/X/Y y LB/RB/LT/RT. PlayStation/DualShock/DualSense utiliza símbolos y L1/R1/L2/R2. Sin un dispositivo reconocido se puede elegir el dibujo manualmente para consultar o configurar asignaciones. Esa elección no realiza emparejamiento Bluetooth ni modifica controladores del sistema.

Se utiliza `Gamepad.current`, que representa el mando activo del Input System. Los dispositivos genéricos sin identificación no se presentan falsamente como Xbox o PlayStation. Un HID expuesto como Joystick necesita un perfil específico fuera del alcance de esta pantalla.

El stick izquierdo conserva el movimiento horizontal existente. El derecho y los clics L3/R3 se visualizan como diagnóstico; el derecho no se anuncia como control de cámara porque esa función no existe en este juego. View/Create y Menu/Options permanecen diferenciados de las acciones del campus.

## Teclado y distribución local

El dibujo utiliza un teclado compacto por filas. Cada casilla contiene un identificador físico del Input System, rectángulo y etiqueta de respaldo. Para letras, números y puntuación se consulta el nombre de la tecla según el teclado activo: la etiqueta y el identificador físico no se confunden. Las teclas asignadas se destacan en naranja; la selección y pulsación añaden borde y brillo cian. El modo de efectos reducidos suprime el desplazamiento decorativo y el desvanecimiento, manteniendo el estado pulsado visible.

El dibujo no certifica la presencia de cada tecla en cualquier modelo de portátil. Las asignaciones de teclas no representadas siguen siendo editables con entrada física y visibles en la lista.

## Arte original y archivos fuente

| Archivo | Contenido |
|---|---|
| `ArtSource/Controles/CampusController.aseprite` | 512 × 300, RGBA, ocho capas y ocho frames: reposo, seleccionado, pulsado y desconectado para Xbox y PlayStation. |
| `ArtSource/Controles/CampusKeyboard.aseprite` | 512 × 300, cinco capas y cuatro frames del teclado. |
| `ArtSource/Controles/Controller_Estados.png` | Hoja de consulta de los ocho estados del mando. |
| `ArtSource/Controles/Controller_Anatomia.json` | Coordenadas de sticks, cruceta, botones y controles centrales. |
| `Assets/CampusGlitch/Resources/CG/Controls` | Carcasas, estados de botones/teclas y `KeyboardLayout.json` consumidos por Unity. |
| `Tools/Arte/crear_mando_editable.py` | Fuente reproducible del arte y escritor del formato Aseprite. Requiere Python 3 y Pillow. |

Las carcasas conservan la ubicación reconocible de los controles y añaden la identidad de Campus Glitch: azul nocturno, naranja, violeta, empuñaduras con textura y emblema original. Las fotos y diagramas consultados no se copian al proyecto.

Las capas separan carcasa, empuñaduras, panel, pozos de sticks, botones, símbolos, iluminación e identidad. Los frames de Aseprite documentan estados; en ejecución las entradas independientes activan sus zonas individuales, no una animación global que enciende botones incorrectos.

El arte fuente queda fuera de Assets para no incluir documentos de trabajo en el build. Las exportaciones utilizan Point, sin mipmaps, Clamp y dimensiones originales. Unity valida los `.aseprite` con su lector oficial; no se afirma haber abierto la aplicación de escritorio Aseprite.

Para regenerar las exportaciones iniciales:

```powershell
python Tools/Arte/crear_mando_editable.py
```

Este comando regenera los dos archivos fuente y sus exportaciones: si se editó el Aseprite manualmente, conservar esa edición y exportar sus capas desde Aseprite en lugar de ejecutar el generador encima. Los GUID existentes se preservan. La CLI oficial permite exportar capas o frames con opciones como `--layer`, `--save-as` y `--sheet`.

## Editar desde Unity

El diseño compartido `CampusMenu.asset` incorpora paneles de acciones/dispositivo, carcasas Xbox y PlayStation, atlas de estados, teclado, colores de respaldo y amplitud de pulsación. Sus referencias de imagen pueden sustituirse desde Inspector. Los textos y bindings continúan siendo elementos separados de la imagen.

`CGControlsUI.cs` define navegación, captura, confirmaciones y zonas del mando. `KeyboardLayout.json` define las casillas del teclado. `CGControlsArt.cs` carga las exportaciones y crea los símbolos complementarios una sola vez. `CGGameEditor` permite mostrar Teclado, Mando, Xbox o PlayStation en Scene sin entrar a Play. El mismo flujo de dibujo se utiliza durante la ejecución.

## Referencias verificadas

- [Microsoft: anatomía, ejes y gatillos de un gamepad](https://learn.microsoft.com/en-us/windows/uwp/gaming/gamepad-and-vibration).
- [Xbox: controlador y vistas oficiales](https://www.xbox.com/en-US/accessories/controllers/xbox-wireless-controller).
- [PlayStation: botones del DualSense](https://www.playstation.com/en-za/support/hardware/ps5-button-functions/).
- [Unity Input System 1.19: soporte y posiciones de Gamepad](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Gamepad.html).
- [Microsoft: teclado, códigos físicos y distribuciones](https://learn.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input).
- [Aseprite: especificación del formato](https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md) y [exportación por CLI](https://www.aseprite.org/docs/cli/).

Resultados y límites: [QA de Controles](../QA/CONTROLES_2026_11_10.md).
