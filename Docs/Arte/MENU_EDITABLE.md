# Menú del campus: edición y presentación

El menú usa el fondo del campus como imagen base; sus textos, botones, estados y navegación se dibujan por separado. Las tarjetas del mapa muestran capturas de los diez escenarios reales. El panel lateral lee los desbloqueos y la finalización del guardado, sin monedas, experiencia ni objetivos que todavía no existan.

## Editar desde Unity

1. Abrir `Assets/CampusGlitch/Scenes/SC_MainMenu.unity` sin iniciar Play.
2. Seleccionar `CampusGlitch_SC_MainMenu` en Hierarchy.
3. Expandir **Diseño compartido del menú** en Inspector. El botón **Seleccionar archivo de diseño** abre `Assets/CampusGlitch/Resources/CG/Presentation/CampusMenu.asset`.
4. Ajustar fondo, fuentes, colores, textos, iconos, descripciones, rectángulo del protagonista, panel, posición y separación de botones. Los valores se expresan sobre una base 1280 × 720; la pantalla se escala manteniendo proporción y respetando el área segura. Los márgenes sobrantes conservan la composición sin deformarla.
5. Ajustar duración de transición, velocidad de foco y desplazamiento del fondo. **Reducir destellos** elimina el movimiento decorativo y acorta la presentación a estados estables.
6. **Alternar vista de entrada animada** permite revisar el acceso universitario dentro de Scene, sin Play. Volver a pulsarlo recupera el menú. Abrir otra escena restablece su propia presentación.

Los siete rótulos corresponden en orden a jugar, mapa, personalización, configuración, ayuda/controles, créditos y salir. Cambiar un rótulo no cambia su acción. El avance se conserva mediante los sistemas existentes.

La primera opción utiliza **New Game Label** y **Continue Label**; `{0}` representa el siguiente nivel desbloqueado. Los iconos predeterminados son pequeños recursos gráficos generados por código y almacenados en caché; pueden sustituirse por texturas en **Main Icons**. **Main Descriptions** contiene la explicación breve de cada selección.

**Main Panel Opacity**, **Button Opacity**, **Header Shade** y **Right Shade** permiten controlar por separado la transparencia del panel, botones y velos de contraste. El foco tiene borde y relleno cálidos; los rótulos se mantienen opacos. La fachada queda expuesta entre el personaje y las opciones.

## Entrada y efectos

El ingreso utiliza un vestíbulo arquitectónico, con suelo en perspectiva, cristal, luces cálidas y recepción. El fondo `MenuAccessHall.png` deja la puerta abierta; dos hojas de cristal se dibujan por separado y se deslizan lateralmente mientras la imagen avanza suavemente. **Entrance Background**, **Entrance Door Rect** y **Entrance Duration** permiten cambiar fondo, encuadre de las hojas y duración. **El acceso aparece exclusivamente al iniciar un nivel**, después de validar su desbloqueo. A/Enter o una pulsación permiten omitir su duración decorativa; la escena solo se activa cuando termina su carga real. Abrir configuración, ayuda, personalización, créditos, mapa o volver al menú utiliza una transición breve con indicación del destino, sin repetir el ingreso universitario.

**Glitch Highlights**, **Glitch Intensity** y **Glitch Speed** controlan destellos de borde, trazos luminosos y el barrido breve de la opción enfocada. No se dibujan letras, números aleatorios ni lluvia de código. Los nombres de opciones conservan su texto estable. Los textos superiores utilizan Barlow Condensed SemiBold sobre bandas oscuras para mantener contraste frente a la fachada clara.

**Guardian Glitch**, **Cycle Guardian Variants**, **Guardian Cycle Seconds** y **Guardian Glitch Duration** controlan una transición por bandas entre poses y variantes del retrato. Es una presentación visual del menú; la skin equipada sigue indicada arriba y conserva los bloqueos existentes. El cambio visual no escribe progreso ni modifica la selección del personaje jugable.

**Reducir destellos** desactiva barridos, ingreso decorativo, cambio automático del retrato, desplazamiento del fondo y animación del foco. El reloj de presentación usa tiempo real y sigue funcionando al pausar la simulación.

Se consultaron las técnicas de [KinoGlitch URP, del autor Keijiro Takahashi](https://github.com/keijiro/KinoGlitchURP): desplazamiento de líneas, separación de color y corrupción por bloques. El proyecto aplica una implementación propia al retrato mediante `MenuGlitch.shader`: franjas horizontales irregulares, muestras R/G/B con pequeños desplazamientos y atenuación de líneas. La transición combina poses y variantes por bandas durante un pulso corto; el procesamiento se actualiza como máximo a 30 Hz y se omite fuera del pulso. El muestreo queda limitado a cada celda del atlas para evitar mezclar fotogramas vecinos.

Las texturas temporales de origen y destino son distintas. Se conservan el render target activo y la escritura sRGB según la [documentación de Graphics.Blit para Unity 6000.3](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.Blit.html). Los rótulos y el fondo universitario se mantienen fuera del shader del retrato.

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
- **MenuCampusLaPaz (fondo activo):** interpretación de la fachada blanca de seis plantas, ventanas oscuras, franja negra, terraza con cubierta y paneles solares, acceso curvo con banderas. Se consultó la [fotografía oficial de la sede La Paz](https://ux.unifranz.edu.bo/images/campus/la-paz.webp), publicada en [Arquitectura UNIFRANZ](https://ux.unifranz.edu.bo/arquitectura). La ilustración adapta el acceso y entorno a la composición del juego; no es una fotografía ni un plano exacto. Luz de atardecer, ciudad al fondo, espacio tranquilo a la derecha para las opciones, sin menús ni personajes pintados. El letrero UNIFRANZ forma parte de la fachada; todos los textos de navegación son editables.
- **MenuAccessHall:** vestíbulo con perspectiva hacia recepción, hormigón blanco, marcos oscuros, franjas naranja, plantas y luces cálidas. La versión final mantiene abierto el paso central para que las hojas animadas se compongan en Unity. El interior es una interpretación artística, sin afirmar que corresponde a una fotografía del vestíbulo real.
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
