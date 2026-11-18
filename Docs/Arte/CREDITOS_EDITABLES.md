# Créditos: imágenes y contenido editable

La escena `Assets/CampusGlitch/Scenes/SC_Credits.unity` presenta al equipo mediante cuatro tarjetas. Cada tarjeta abre una imagen más grande y una descripción. Agradecimientos especiales muestra a Ing. Noemí Moller Chuquimia e Ing. Rubén Soria; Arte y recursos reúne las fuentes y el contexto del proyecto.

El contenido está en `Assets/CampusGlitch/Resources/CG/Presentation/CampusCredits.asset`. Permite cambiar nombres, descripciones, imágenes, colores, fondo, separación de tarjetas, transparencia y movimiento. En el Inspector del objeto de la escena también se puede mostrar el equipo, los agradecimientos y el detalle de Víctor antes de pulsar Play.

## Criterios de las imágenes

Las seis imágenes están en `Assets/CampusGlitch/Resources/CG/Credits/`. El texto se dibuja por separado para poder corregirlo sin rehacer los retratos.

| Persona | Rasgos conservados y ambiente |
|---|---|
| Víctor | Imagen aportada por el usuario: rostro, lentes, peinado, ropa roja y azul con telarañas; programación, animación y videojuegos. Se conserva la temática de Spider-Man solicitada. |
| Carla | Cabello largo castaño, lentes redondos, sonrisa y ambas manos junto al rostro. Fondo del campus con movimiento de personajes, música y tonos violetas. |
| Vicente | Cabello corto levantado, lentes rectangulares y mano ajustándolos. Fondo del campus con pantallas, mapa, caminos conectados y luces celestes. |
| Marcelo | Cabello ondulado, lentes negros opacos, chaqueta oscura y rostro apoyado en el puño. Fondo con figuras de enemigos. |
| Noemí | Cabello corto castaño, lentes, aros y chaqueta violeta, siguiendo la referencia de agradecimientos. |
| Rubén | Lentes, sonrisa, suéter verde y muñeco de mono, siguiendo la referencia de agradecimientos. |

Los retratos son ilustraciones basadas en las referencias entregadas. Las descripciones distinguen los primeros aportes registrados y el área de trabajo de cada integrante, usando el reparto de responsabilidades y la tabla del README. La autoría del código se consulta en sus commits y revisiones, no se deduce de las ilustraciones.

## Uso

- Ratón: pulsar una tarjeta, Agradecimientos, Arte y recursos o Volver.
- Teclado: direcciones para elegir; Enter para abrir; Escape para cerrar la ventana actual. Dentro de un perfil, izquierda/derecha cambia de integrante.
- Mando: cruceta o stick para elegir; A para abrir; B para cerrar; LB/RB cambia de integrante en su perfil.
- Al cerrar una ventana se recupera la selección anterior. El fondo no recibe pulsaciones mientras la ventana está abierta.
- Reducir destellos elimina el desplazamiento de entrada y los adornos en movimiento. Los créditos no alteran los niveles completados ni las asignaciones de botones.

Las imágenes se importan con filtro Point, sin mapas de menor tamaño ni compresión, con límite de 1024 y sin copia de lectura en memoria. El retrato conserva su proporción y un encuadre superior para mantener visible el cabello. La presentación antes de Play utiliza el mismo contenido que el juego.
