# Configuración del campus

La pantalla reúne Pantalla, Audio, Accesibilidad y Partida. Usa los colores de `CampusMenu`: naranja para las acciones principales, azul oscuro en los fondos, blanco en las letras y celeste en los detalles. Los marcos conservan sus esquinas recortadas al cambiar de tamaño, y las opciones elegidas tienen un borde y un indicador propios.

El archivo editable es `Assets/CampusGlitch/Resources/CG/Presentation/CampusSettings.asset`. Permite cambiar títulos, fondo, distribución, movimiento y colores. Por defecto toma los colores del juego. Las fuentes siguen siendo las del diseño compartido del menú. En el Inspector de la escena `SC_Settings` se pueden mostrar los cuatro apartados antes de Play.

| Apartado | Funcionamiento |
|---|---|
| Pantalla | Modo, resolución, brillo del ambiente y sincronización de imagen. En el ejecutable, cambiar modo o resolución requiere confirmar en 15 segundos; al cancelar o no responder se recupera la pantalla anterior. |
| Audio | Silencio, volumen general, música, sonidos del juego y sonidos de botones. Se pueden escuchar salto, ataque, interacción, menú y alerta con los sonidos existentes. Las barras reciben la señal del sonido reproducido. |
| Accesibilidad | Consejos, tamaño de avisos, fondos sólidos para las letras y reducción de destellos. Las conversaciones con personajes siguen disponibles cuando se apagan los consejos. |
| Partida | Acceso a la pantalla de teclas y botones; borrar el avance con confirmación. Borrar avance conserva las opciones y las asignaciones de botones. |

Los cambios de sonido se escuchan antes de guardar. Salir con cambios abre una elección que inicialmente conserva el borrador. Restablecer prepara los valores originales de sonido y lectura; Guardar los conserva. Guardar solamente sonido no cambia el modo o la resolución de la pantalla. Si falla el guardado, el borrador permanece disponible para intentarlo otra vez.

## Vista del juego

El cuadro «Así se ve al jugar» genera el nivel que corresponde a Continuar, con el mismo constructor de escenarios, recursos, Guardián y personajes del juego. No utiliza el fondo del menú como sustituto del nivel.

La vista tiene una cámara y una escena propias, sin control del personaje ni simulación física. Se actualiza como máximo ocho veces por segundo y se detiene mientras el apartado Audio ocupa ese espacio. Al salir se destruyen sus objetos y se libera la imagen. Se conserva la definición mediante filtro Point. El brillo y el tamaño de los avisos pueden compararse allí.

## Navegación

Direcciones o stick eligen; izquierda/derecha ajustan valores; Enter/A acepta; Escape/B vuelve; Tab o LB/RB cambia de apartado. Las barras también admiten arrastre con el ratón. Mantener una dirección cambia un interruptor una sola vez. Las confirmaciones esperan a que se suelte la entrada anterior, para evitar una aceptación por arrastre.

Referencias de implementación: [cambio de resolución en Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/screen/setresolution) y [lectura de sonido reproducido](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/audiosource/getspectrumdata).
