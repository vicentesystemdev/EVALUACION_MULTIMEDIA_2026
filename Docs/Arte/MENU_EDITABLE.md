# Menú del campus: edición y presentación

El menú usa el fondo del campus como imagen base; sus textos, botones, estados y navegación se dibujan por separado. Las tarjetas del mapa muestran capturas de los diez escenarios reales. El panel lateral lee los desbloqueos y la finalización del guardado, sin monedas, experiencia ni objetivos que todavía no existan.

## Editar desde Unity

1. Abrir `Assets/CampusGlitch/Scenes/SC_MainMenu.unity` sin iniciar Play.
2. Seleccionar `CampusGlitch_SC_MainMenu` en Hierarchy.
3. Expandir **Diseño compartido del menú** en Inspector. El botón **Seleccionar archivo de diseño** abre `Assets/CampusGlitch/Resources/CG/Presentation/CampusMenu.asset`.
4. Ajustar fondo, fuentes, colores, textos, rectángulo del protagonista, panel, posición y separación de botones. Los valores se expresan sobre una base 1280 × 720; la pantalla se escala manteniendo proporción.
5. Ajustar duración de transición, velocidad de foco y desplazamiento del fondo. **Reducir destellos** elimina el movimiento decorativo y acorta la presentación a estados estables.

Los siete rótulos corresponden en orden a jugar, mapa, personalización, configuración, ayuda/controles, créditos y salir. Cambiar un rótulo no cambia su acción. El avance se conserva mediante los sistemas existentes.

Scene muestra un Canvas temporal que consume la misma composición del menú en ejecución. Sus elementos `CG_EDITOR_MENU_PREVIEW` se reconstruyen automáticamente: las modificaciones persistentes se hacen en el archivo de diseño, no en esa jerarquía temporal. La vista anterior queda desactivada en las 18 escenas; se conservaron sus objetos y GUID.

Las ocho cámaras principales de interfaz usan centro (0, 0), Z=-10 y tamaño ortográfico 3,6 para encuadrar el Canvas de 12,8 × 7,2 unidades. Esto corrige la miniatura alejada de Camera Preview. Las diez cámaras de niveles conservan el tamaño de juego 5,7.

## Tipografía

| Uso | Fuente | Motivo |
|---|---|---|
| Identidad y títulos principales | Audiowide | Trazos tecnológicos y formas abiertas, con sombra de separación |
| Botones, tarjetas y navegación | Barlow Condensed SemiBold | Mayor presencia y espacio para nombres largos |
| Descripciones y lectura | Questrial | Texto limpio con formas redondeadas |
| Recursos anteriores compatibles | Bungee, Rajdhani SemiBold, Atkinson Hyperlegible | Se conservan con sus licencias; pueden elegirse en el diseño |

Fuentes incluidas en el proyecto, sin depender de una instalación del sistema. Proceden del repositorio oficial [Google Fonts](https://github.com/google/fonts), con sus archivos OFL junto a cada familia. Referencias: [Audiowide](https://github.com/google/fonts/tree/main/ofl/audiowide), [Barlow Condensed](https://github.com/google/fonts/tree/main/ofl/barlowcondensed), [Questrial](https://github.com/google/fonts/tree/main/ofl/questrial). Se comprobó la disponibilidad de ÁÉÍÓÚÑáéíóúñ¿¡ en Unity.

## Presentación del Guardián

`CG/GuardianMenu/GuardianMenuHD.png` contiene ocho poses de menú: cuatro rojas y cuatro moradas, con espera, respiración y parpadeo. Se creó tomando como referencia el diseño acorazado aprobado: hocico, melena, placa U, núcleo, cola y garras. Se integra como presentación de mayor definición; los atlas y animaciones del personaje jugable permanecen separados.

El archivo original tiene 1448 × 1086 píxeles. Los recortes neutros tienen aproximadamente 490 píxeles de alto y se muestran reducidos en el menú, en lugar de ampliar fotogramas pequeños de juego. La importación usa Point, sin mipmaps ni compresión. Una máscara de render elimina el halo de alfa bajo sin modificar el PNG. El recorte y los pivotes se describen en `GuardianMenuHD_Frames.json`.

## Dirección de las imágenes

- **MenuCampus:** campus nocturno de La Paz, arquitectura en cristal con luz ámbar, montañas, plaza y plataforma vacía para el Guardián a la izquierda; espacio oscuro y tranquilo para opciones a la derecha. Sin texto ni personajes incrustados.
- **MapCampus:** vista elevada de un campus con terrazas, senderos, jardines, biblioteca y laboratorios; izquierda disponible para tarjetas y derecha oscura para detalle. Sin tarjetas ni indicadores pintados en el fondo.
- **GuardianMenuHD:** cuadrícula de cuatro columnas y dos filas sobre transparencia; misma anatomía y armadura del diseño aprobado; primera fila roja, segunda morada; espera, respiración, parpadeo y reposo, con cambios sutiles y contorno definido.

Los fondos son interpretaciones del campus para el juego, no fotografías ni un plano arquitectónico. Los datos de ambientes y límites de la referencia real se documentan en [Campus La Paz: ambientes y profundidad](CAMPUS_LA_PAZ_PROFUNDIDAD.md).

## Controles de navegación

- Direcciones, WASD, cruceta o stick: mover selección; el stick tiene zona muerta y repetición controlada.
- A / Enter: confirmar o jugar el nivel seleccionado. B / Escape: volver o cancelar una asignación.
- Mapa: Y / Tab abre detalle; LB / RB y PageUp / PageDown cambian nivel. Las tarjetas admiten ratón y el botón Jugar inicia la selección.
- Ayuda y Configuración permiten abrir Controles. En Controles, LB / Tab alterna entre teclado y mando.
- START pausa al jugar. Confirmar al reanudar no dispara un salto accidental.
- Borrar progreso está dentro de Configuración y abre con **Cancelar** seleccionado.

El arnés de recorridos ahora responde la terminal mediante teclas o botones reales. La comprobación de mando utiliza un dispositivo virtual de Input System; no certifica conexión física, latencia o compatibilidad de un modelo concreto de mando.
