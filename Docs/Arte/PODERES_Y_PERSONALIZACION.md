# Personalización y evolución del Guardián

La pantalla organiza el progreso real en **Apariencia**, **Habilidades** y **Evolución**. La sección «Efectos» se retiró. Se conserva el diseño aprobado del personaje y sus atlas jugables.

Las tarjetas de habilidad tienen ilustraciones propias, descripción, disponibilidad, coste de carga y estado de equipamiento. El panel **Cómo activar** reemplaza «Configuración guardada»: permite seleccionar **Teclado** o **Mando** y muestra la asignación real de Habilidad. `V` o `Y` alternan ese selector. No se cambian las asignaciones desde este selector; se editan en Controles.

## Recompensas por nivel

| Hito completado | Recompensa | Comportamiento |
|---|---|---|
| Inicio | Dash glitch, rango 1 | Impulso de 0,22 s; velocidad 10; recarga de 1,8 s. Respeta obstáculos sólidos. |
| N1 | Ráfaga glitch | Proyectil frontal de 1 de daño, velocidad 12, alcance 7 y recarga de 1,1 s. Se detiene en el primer enemigo u obstáculo sólido. |
| N2 | Escudo digital | Absorbe un golpe durante un máximo de 2 s; recarga de 6 s. También conserva el desbloqueo de la variante morada. |
| N3 | Corte de energía | Golpe frontal de 2 de daño, radio 1,3 y recarga de 2,5 s. No atraviesa superficies sólidas ni golpea a espaldas del personaje. |
| N4 | Dash, rango 2 | Velocidad 11; recarga de 1,5 s. |
| N5 | Escudo, rango 2 | Duración máxima de 2,5 s; recarga de 5 s. Sigue absorbiendo un único golpe. |
| N6 | Corte, rango 2 | Daño 3; radio 1,45; recarga de 2,2 s. |
| N7 | Ráfaga, rango 2 | Daño 2, velocidad 15, alcance 9 y recarga de 0,9 s. |
| N8 | Dash, rango 3 | Velocidad 12; recarga de 1,2 s. |
| N9 | Corte, rango 3 | Radio 1,6; recarga de 2 s. Conserva 3 de daño. |

El nivel extra conserva sus objetivos y no concede un décimo rango adicional. El catálogo está centralizado en `CGAbilities.cs`; las unidades de velocidad y alcance de la tabla son unidades del mundo Unity. La propuesta de doble salto se sustituyó por un proyectil; el salto normal del controlador conserva su física anterior.

## Carga y protección

El núcleo tiene **100 puntos**. Dash consume 20, Ráfaga 18, Escudo 40 y Corte 30. Una activación fallida no consume carga. La recuperación es de **15 puntos por segundo**, después de una espera de 1,2 s o la duración del poder si esta es mayor. La pausa detiene la recuperación del personaje. Los puntos son un recurso de combate de la sesión; no conceden hitos ni se compran con monedas.

El escudo forma una cúpula cian con malla hexagonal, ajustada al cuerpo del lobo y separada de su collider. Absorbe un golpe y muestra un impacto breve al consumirse; caduca si no recibe daño. El HUD indica el punto de protección restante, la duración y la recarga. Las mejoras de N5 amplían duración y reducen recarga, sin convertirlo en invulnerabilidad ilimitada.

La ráfaga crea un proyectil real. El barrido circular entre posiciones evita saltarse colliders a alta velocidad; los impactos producen una onda visual y aplican daño una sola vez. Se utiliza la [consulta circular de la escena física](https://docs.unity3d.com/cn/6000.0/ScriptReference/PhysicsScene2D.CircleCast.html). Dash deja ecos de la silueta; Corte dibuja garras energéticas con alcance visual acorde a su rango. Reducir efectos desactiva ecos y pulsos decorativos, conservando las señales necesarias para identificar los ataques.

`completed` es la evidencia de restauración. `latestUnlocked` conserva la compatibilidad con partidas anteriores: tener abierto N9 no equivale a haber terminado N9. La mejora final necesita el nivel 9 completado.

## Elegir, probar y aplicar

- Elegir una apariencia o habilidad mantiene un borrador. **Aplicar** guarda la elección; el rótulo superior avisa cuando hay cambios pendientes.
- Al salir con cambios, la opción inicial es **Seguir editando**. Descartar conserva la configuración guardada.
- Los poderes bloqueados pueden observarse y demostrarse; esas acciones no conceden progreso ni escriben archivos de guardado.
- **Probar**, `T` o `X` ejecutan una demostración ilustrada con proyectil, impactos, cúpula y ecos, sin saltos de exhibición. **Impacto** consume el escudo de la demostración. Carga y temporización comparten las reglas del controlador, pero esta presentación no sustituye una prueba física de un nivel.
- En el juego, los cuatro poderes usan una ranura activa y la acción Habilidad (`Q` / `RB` por defecto).
- El HUD muestra la habilidad activa, energía disponible, coste y recarga real. El control táctil incorpora Habilidad y carga. La validación de dispositivos táctiles y mandos físicos sigue siendo independiente.
- Victoria y final muestran la recompensa correspondiente al nivel completado. Los rangos se aplican desde el progreso guardado; no requieren monedas o experiencia adicionales.

## Recursos y edición

Las cuatro ilustraciones están en `Assets/CampusGlitch/Resources/CG/Abilities`: `Dash.png`, `EnergyBurst.png`, `Shield.png`, `EnergyCut.png`. No contienen texto; los nombres, bloqueos y controles se dibujan por separado y permanecen editables en código. Los recursos transparentes del combate se construyen una vez y se comparten con la demostración, de forma independiente de las tarjetas ilustradas.

El importador aplica únicamente a esa carpeta un límite de 512 px, filtro Point, Clamp y ausencia de mipmaps; las texturas no necesitan lectura desde CPU. Los sprites y sus efectos de combate conservan recursos separados de estas tarjetas.

La distribución se define en `CGCustomizationUI.cs` dentro del viewport común de 1280 × 720, con escala uniforme y respeto del área segura. Las familias tipográficas y el tema se comparten con el menú existente. El catálogo, las descripciones, el control equipado y las imágenes no están incrustados en una captura del menú.

En `SC_Customization`, seleccionar el objeto con `CGGame` permite **Mostrar Apariencia**, **Mostrar Habilidades**, **Mostrar Evolución**, **Mostrar activación con teclado** y **Mostrar activación con mando** desde Inspector, antes de Play. Los cuatro botones de demostración permiten observar cada poder y su carga desde Scene. Las vistas se reconstruyen usando el mismo flujo de dibujo del juego, sin regenerar los assets de escena.

## Corrección de Scene

La caché de botones anterior destruía las texturas del acento anterior al solicitar otro color. El Canvas de edición todavía podía referenciarlas, produciendo rectángulos blancos. La caché ahora conserva pares de texturas por color. El Canvas reserva el alfabeto, números y símbolos de cada tamaño de fuente antes de construir las mallas y repite la reconstrucción si cambia el atlas tipográfico, evitando letras incompletas al cambiar la demostración. Las etiquetas vacías no generan objetos adicionales.

Validación y capturas: [Personalización y poderes](../QA/PERSONALIZACION_2026_11_10.md).
