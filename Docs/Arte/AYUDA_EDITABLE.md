# Guía del campus

La ayuda usa los colores y las fuentes del menú. El lateral reúne Cómo jugar, Objetivos, Iconos del juego, Personajes y Guardado. El botón **Controles** abre la pantalla de asignaciones; Mapa del campus abre los destinos reales.

En `Assets/CampusGlitch/Resources/CG/Presentation/CampusHelp.asset` se editan el título, los cinco rótulos laterales, el fondo, el color del panel, los seis pasos, las cuatro respuestas y los tiempos de transición y tutorial. Mantener estas cantidades conserva la distribución. En el Inspector de `SC_Help`, los botones de Ayuda muestran cada apartado antes de Play.

Las instrucciones toman las teclas y botones asignados por el jugador. Tab o LB/RB cambian de apartado; izquierda/derecha recorren su contenido; las direcciones seleccionan acciones y A/Enter confirma. El dispositivo utilizado cambia las indicaciones. Anterior, Siguiente y las tarjetas también responden al ratón.

Ver tutorial recorre seis instrucciones con avance automático; B/Escape detiene el recorrido. B/Escape desde la ayuda devuelve al menú. Abrir Controles o Mapa no activa la entrada de niveles. Los cambios de contenido usan un desvanecimiento y desplazamiento breves; el foco de los botones tiene un indicador propio. Reducir destellos mantiene la información sin estos movimientos.

Los objetivos y recompensas proceden de los datos del juego. Los ocho personajes, el enemigo y los objetos utilizan sus imágenes existentes. El cuadro del escenario comparte la vista real de Configuración, sin activar control ni físicas; se libera al abandonar la pantalla. Scene y Play comparten el dibujo. La guía distingue expresamente el progreso de nivel guardado del punto de regreso temporal.
