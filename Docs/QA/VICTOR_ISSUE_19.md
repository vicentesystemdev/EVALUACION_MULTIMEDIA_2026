# Problema 19 — caída natural, alturas y colisiones

Víctor · 7 de noviembre de 2026 · Unity 6000.3.16f1.
Rama: `fix/victor/19-salto-plataformas`, desde `origin/develop` (`048b3e3`). Refs #19. PR #30 hacia `develop`.

## Resultado implementado

- El collider del personaje usa fricción 0 y rebote 0. Al empujar contra un lateral, la gravedad continúa actuando; soltar la dirección mantiene el frenado habitual sobre el suelo.
- **Todas las plataformas elevadas son sólidas**, por arriba, abajo y los lados. Cada una conserva un `BoxCollider2D` alineado con su superficie dibujada; no hay plataformas de un solo sentido.
- Primera superficie: y=2.96, con base en 2.64. Segunda: y=5.48. Tercera: y=8.34. La separación mínima entre superficies superpuestas deja más de dos unidades libres para el personaje.
- Impulso de salto 14.4 con la gravedad anterior: altura medida **3.164 unidades**. Se comprobó ascenso y aterrizaje caminando y corriendo en las tres alturas, usando el nivel inferior como apoyo para las superiores.
- La cámara conserva la altura de suelo y acompaña al personaje al subir a rutas superiores.
- Los barandales están unidos a las plataformas físicas y alineados con ellas. Se sustituyó su generación decorativa independiente, que daba la apariencia de plataformas sin colisión.

Los scripts modificados son `CGEntities.cs` y `CGWorld.cs`. Las escenas y GUID existentes se conservan; el mundo se genera al iniciar Play. El inventario de arte, el guardado, el daño, Game Over, los controles y las reglas del portal no se modifican.

## Física comprobada

Antes de corregir la fricción, el personaje se sostenía alrededor de y=0.996 al empujar contra un lateral desde y=1. Después cae hasta y=-0.507 manteniendo el empuje. Se reprodujo en ambos sentidos, a velocidades 3.5 y 5.8.

La batería final produjo **56 comprobaciones correctas y 0 fallidas**:

- Cuatro caídas laterales y cuatro comprobaciones de que el personaje no atraviesa el obstáculo sólido.
- Frenado sobre suelo sin deslizamiento ni rebote.
- Seis ascensos y aterrizajes sobre plataformas reales, con marcha y carrera.
- Golpe contra la cara inferior y vuelta al suelo sin quedar suspendido.
- En cada uno de los diez niveles: todos los colliders sólidos activos, correspondencia con el dibujo, barandales con apoyo físico y separación vertical suficiente.

El arnés usa escenas de física 2D aisladas y ejecuta los métodos reales del personaje y del generador. Esas comprobaciones no sustituyen el recorrido del nivel.

## Verificación conjunta con el problema 18

La copia local de QA parte de `a882b8d` (PR #29) y aplica el cambio de código de `7effef4` (este PR) sin crear commits ni fusionar ramas. Se comprobó que `CGEntities.cs` y el arnés de recorrido coinciden por SHA-256 con esta rama; la diferencia adicional en `CGWorld.cs` es exclusivamente la posición del segundo fragmento de N1 y N8.

Los **diez recorridos principales terminaron correctamente**, con 3/3 fragmentos, victoria/final y progreso registrado. El arnés conduce al personaje desde su aparición mediante el teclado del juego. Recoge por triggers, salta los huecos, usa el checkpoint, ataca y entra al portal. **No recoloca al personaje, no desactiva enemigos y no concede fragmentos, vida, terminal ni muerte del jefe.**

[Resultados de física](VictorIssue19/fisica.txt) · [Recorridos N1–N9 y EXTRA](VictorIssue19/recorridos.txt).

Capturas reales de las plataformas elevadas: [N1](VictorIssue19/N1_plataformas.png), [N2](VictorIssue19/N2_plataformas.png), [N8](VictorIssue19/N8_plataformas.png). Victoria: [N1](VictorIssue19/N1_victoria.png), [N2](VictorIssue19/N2_victoria.png), [N8](VictorIssue19/N8_victoria.png) y [final N9](VictorIssue19/N9_final.png).

En una prueba visual independiente se colocó el personaje a y=8.34 para comprobar exclusivamente la cámara: esta siguió hasta y=7.34 y mantuvo pies/cabeza dentro del encuadre (coordenadas verticales 0.59 y 0.77). [Captura de cámara superior](VictorIssue19/camara_superior.png). Esta colocación controlada no se usó en los diez recorridos ni demuestra haber recorrido todas las rutas superiores.

La terminal utiliza el manejador existente de su respuesta correcta; la interfaz de ratón/teclado del quiz pertenece a #21. Por tanto, el recorrido comprueba el movimiento y la misión, pero no constituye una validación física de esa interfaz.

## Reproducción y revisión

1. Integrar primero #29 en una copia de QA y aplicar este cambio, manteniendo los commits de cada rama.
2. Abrir Unity 6000.3.16f1 y esperar a que compile.
3. Ejecutar `Campus Glitch > QA > Verificar caida lateral y salto (Issue 19)` con Play detenido.
4. Ejecutar `Campus Glitch > QA > Recorrer Nivel 1 con teclado (Issue 19)`. Para otro nivel se dispone de `CGRouteRegression.Run(carpetaNueva, indice)` con índices 0..9.
5. Comprobar manualmente el roce lateral, salto parado/carrera, cara inferior, aterrizaje, huecos y recorrido N1/N2/N8. Revisar el seguimiento de cámara al subir.

El arnés de recorrido respalda y restaura `.json`, `.bak` y `.tmp`, incluyendo la ausencia original de cada archivo. Sus copias de seguridad permanecen fuera del repositorio y no forman parte de la entrega.

Al terminar se compararon presencia y SHA-256 de los tres nombres de guardado con el respaldo inicial: el progreso personal quedó intacto.

## Integración y límites

`CGEntities.cs` también se modifica en los PR #27 y #28. Este cambio afecta material del collider, salto y cámara; no modifica animación, daño ni derrota. Vicente debe repetir las pruebas después de integrar esas ramas.

Las escenas originales conservan advertencias de componentes sin script. No se regeneraron escenas ni animadores. El build y recorrido gráfico de Windows, mando físico y Android siguen fuera de estas pruebas y corresponden a #16. Los recorridos elevados opcionales no se consideran todos validados por una ruta principal completada.

No se realizó merge, no se modificó `main`/`develop` y no se cerraron Issues.
