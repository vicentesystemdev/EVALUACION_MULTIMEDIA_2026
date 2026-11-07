# Issue #13 — Sistema de derrota y Game Over

- **Integrante:** Marcelo Escobar (`Josue-EC16`).
- **Motor:** Unity `6000.3.16f1` · Windows nativo.
- **Estado:** DRAFT — diagnóstico y diseño; implementación e integración pendientes.
- **Fecha de esta entrega:** 2026-11-07.
- **Rama:** `feature/marcelo/13-game-over`.
- **Base revisada:** `origin/develop` @ `048b3e3dd93f2240aa85e388ae2233f68dab1a7b`.
- **Issue:** [#13 — Derrota, vidas, Game Over y checkpoints](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/13).
- **Revisor e integrador:** Vicente (`vicentesystemdev`).
- **PR Draft a `develop`:** [#28 — Derrota, checkpoints y QA](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28).

Este es el expediente único del Issue #13. Se actualizará durante la
implementación, las pruebas y la revisión. No acredita que Game Over esté
implementado ni que la funcionalidad esté integrada en `develop`.

## 1. Diagnóstico verificado por lectura

En la base revisada:

- `CGHero.Damage(int)` descuenta corazones y concede aproximadamente 1,1 s
  de invulnerabilidad. Cuando la vida llega a cero, llama a `CGGame.KillHero()`.
- La caída por debajo de `y = -9` también llama a `KillHero()`.
- `KillHero()` ejecuta `Respawn()`, que reposiciona al héroe, anula su velocidad
  y siempre establece `hearts = 3`.
- La opción de pausa «REINICIAR DESDE CHECKPOINT» utiliza ese mismo método,
  por lo que también restaura corazones.
- No hay una página Game Over en la máquina de estados de `CGGame`.
- El checkpoint empieza en `x = 2`; el jugador activa el de `x = 35`
  mediante interacción. La reaparición actual usa `checkpointX + 0.5`.
- El estado del intento (fragmentos, terminal, enemigos y jefe) permanece
  en memoria. Los niveles completados y desbloqueos pertenecen al guardado JSON.

La auditoría histórica ya reprodujo E4 con PlayMode. Esta entrega confirma la
causa por lectura del código; no ha vuelto a ejecutar esas pruebas históricas.

## 2. Coordinación y condición de integración

El [PR #27 de Carla](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/27),
rama `feature/carla`, está abierto en la consulta realizada para esta entrega
y modifica tanto `CGGame.cs` como `CGEntities.cs`. Su head consultado es
`00d3e68ff8a27f73d8ff5a062fc6218eab9be3d9`.

**Marcelo confirmó que la coordinación con Vicente sigue pendiente y pidió
preparar la entrega y un PR Draft.** No se editarán los scripts compartidos
en esta fase. La integración de Game Over se realizará después de que el
trabajo de Carla esté incorporado en `develop` y Vicente confirme el orden.

Antes de implementar se volverán a consultar el Issue, el PR de Carla y la
base remota. No se fusionará el PR #27 desde esta rama por iniciativa propia.

## 3. Decisión de jugabilidad propuesta a Vicente

Las siguientes reglas fueron elegidas por Marcelo. **Quedan pendientes de
validación por Vicente y, si corresponde, de contraste con la rúbrica/docente.**

| Situación | Comportamiento acordado para implementar |
|---|---|
| Daño de enemigo | Descontar corazones y respetar 1,1 s de invulnerabilidad. |
| Caída al vacío | Descontar un corazón, incluso si había invulnerabilidad. |
| Caída con vida restante | Volver al checkpoint conservando los corazones restantes. |
| Cero corazones | Entrar una sola vez en Game Over; no reaparecer automáticamente. |
| REINTENTAR | Recuperar tres corazones y volver al checkpoint; sin checkpoint, al inicio. |
| Reiniciar desde pausa | Reposicionar sin recuperar corazones. |
| MENÚ PRINCIPAL | Salir con el tiempo normalizado y conservar el progreso persistente. |

Reaparición y reintento conservarán fragmentos, terminal resuelta, enemigos
eliminados y jefe derrotado del intento actual. No se añadirá guardado
persistente dentro del nivel: cerrar o abandonar el nivel no convierte ese
avance temporal en progreso guardado.

La activación automática del checkpoint y otros ajustes del GDD no se darán
por aprobados ni implementados como consecuencia de este diseño.

## 4. Arquitectura prevista — todavía no implementada

1. Añadir `Page.GameOver` a `CGGame` y reutilizar su presentación IMGUI,
   con REINTENTAR y MENÚ PRINCIPAL. No crear otra escena ni importar
   automáticamente `GameOver.unity` o `GameManager.cs` del prototipo.
2. Separar la entrada de caída, la reaparición y la derrota. Mantener
   `KillHero()` como entrada de derrota, y retirar de la reaparición la
   curación incondicional. Adaptar todos sus llamadores y pruebas.
3. Mantener compatible `Damage(int)` para los enemigos y añadir una vía
   explícita de daño por caída que omita la invulnerabilidad. Rechazar daño
   no positivo, vida negativa y daño fuera del juego activo.
4. Al entrar en Game Over, detener al héroe, limpiar acciones pendientes y
   establecer `Playing = false` y `Time.timeScale = 0`. Evitar repetir audio,
   procesar daño adicional o aceptar la confirmación del mismo fotograma.
5. Al reaparecer, conservar el mundo y el avance del intento; anular velocidades,
   salto almacenado, ataque y habilidad temporal; aplicar 1,1 s de protección.
   Reanudar con `Time.timeScale = 1` sin convertir la confirmación con Espacio
   en un salto inmediato.
6. Navegar con flechas/W/S, Enter/Espacio, equivalentes de mando y ratón.
   REINTENTAR será la selección inicial. Escape/B volverá al menú y no
   reanudará al personaje derrotado.
7. Bloquear recogidas, ataques e interacciones durante la derrota.
   Game Over y reintento no deberán escribir ni reiniciar el guardado.

### Compatibilidad con QA de Carla

El arnés `CGPlaySessionQA` del PR #27 utiliza actualmente `KillHero()` con
un corazón para comprobar una reaparición que restaura tres. Esa expectativa
representa el comportamiento que se quiere corregir. Después de la coordinación
se adaptará ese caso para simular una caída con tres corazones, esperando dos
y reaparición en el checkpoint. La derrota se cubrirá con casos propios.

## 5. Alcance real de esta entrega preliminar

**Realizado:**

- Diagnóstico del flujo de daño, caída, pausa y reaparición en la base revisada.
- Consulta por terminal de los Issues #13 y #14, comentarios y PR activos.
- Identificación del solapamiento con el PR #27 y su arnés de QA.
- Decisiones de jugabilidad con Marcelo y plan de integración y pruebas.
- Creación de este expediente en `Docs/MARCELO_ENTREGABLES/`.
- Regla en `.gitignore` para excluir instrucciones personales locales.

**Pendiente:** implementación de Game Over, adaptación de QA, compilación,
ejecución PlayMode y validación visual. El Issue #14 se trabajará en otra rama
solo después de integrar el #13.

No se han modificado escenas, scripts de juego, paquetes ni ProjectSettings
como parte de esta entrega. Hay cambios locales de importación en `.meta`
ajenos al alcance; se preservan y no se incluirán en los commits de este Issue.

## 6. Plan de pruebas y aceptación

### Comprobaciones ya ejecutadas

| Comprobación | Tipo | Resultado |
|---|---|---|
| Rama, remoto e identidad Git de Marcelo | Terminal | Verificados. |
| Actualización de referencias con `git fetch origin --prune` | Terminal | Completada. |
| Flujo de daño y reaparición de la base | Análisis estático | Causa de E4 identificada. |
| Issues #13/#14 y PR #27 | Consulta GitHub por terminal | Consultados; coordinación pendiente. |
| Scripts de juego del árbol local | Comparación Git | Sin cambios de esta entrega. |
| Formato y contenido del entregable | Validación estática | Campos del aviso presentes; 15 casos pendientes explícitos; sin espacios finales. |
| Alcance y espacios del commit inicial | Git | Solo dos rutas; `git diff --cached --check` sin errores. |
| Autor y destino del PR #28 | GitHub por terminal | `Josue-EC16`, rama propia hacia `develop`, abierto como Draft. |
| Revisión de Vicente | GitHub por terminal | Solicitud formal enviada a `vicentesystemdev`; revisión todavía pendiente. |

### Casos para ejecutar después de integrar la implementación

| ID | Caso | Resultado esperado | Estado |
|---|---|---|---|
| D1 | Daño sucesivo separado por la invulnerabilidad | Vida 3 → 2 → 1 → 0; nunca negativa. | No ejecutado |
| D2 | Daño repetido durante 1,1 s | No descuenta corazones adicionales. | No ejecutado |
| D3 | Llegar a cero | Game Over una vez; mundo detenido; sin reaparición automática. | No ejecutado |
| D4 | Caer sin checkpoint, con tres corazones | Reaparece al inicio con dos corazones. | No ejecutado |
| D5 | Activar checkpoint y caer con dos corazones | Reaparece en checkpoint con un corazón. | No ejecutado |
| D6 | Caer con un corazón, incluso durante invulnerabilidad | Cero corazones y Game Over. | No ejecutado |
| D7 | REINTENTAR con y sin checkpoint | Tres corazones, posición correcta, velocidad limpia y tiempo 1. | No ejecutado |
| D8 | Reintentar tras recoger datos, resolver terminal o eliminar enemigo | Conserva esos estados; no duplica objetos ni recompensas. | No ejecutado |
| D9 | Reiniciar desde pausa con uno o dos corazones | Reposiciona sin curación; pausa vuelve a funcionar. | No ejecutado |
| D10 | Confirmar Reintentar con Espacio | No salta ni ataca por la confirmación anterior. | No ejecutado |
| D11 | MENÚ PRINCIPAL con teclado y ratón | Vuelve al menú con tiempo 1 y progreso previo intacto. | No ejecutado |
| D12 | Escape/B y doble confirmación en Game Over | Sin reanudación inválida ni transiciones duplicadas. | No ejecutado |
| D13 | Guardado con niveles ya completados, derrota y recarga | Conserva completados, estrellas, desbloqueos y ajustes. | No ejecutado |
| D14 | Combate y retorno al juego en N1/N2 | Daño, mordida, HUD y victoria coherentes. | No ejecutado |
| D15 | Regresión de menús de Carla y carga de las 18 escenas | Sin regresiones nuevas atribuibles al #13. | No ejecutado |

Se prepararán regresiones automáticas de PlayMode y se documentarán por separado
las pruebas manuales de teclado/ratón, capturas y resultados. Mando físico y
Android permanecerán «No ejecutado» si no hay hardware o APK verificado.

Antes de ejecutar QA se usará aislamiento temporal de guardado, verificando
la restauración. El PR de Carla aporta `CGStore.TestSaveDirectory` para el
editor; se comprobará su disponibilidad tras integrar esa base. Otro clon
o worktree, por sí solo, no protege el guardado personal.

Los bloqueos de fragmentos/salto/quiz de otros Issues no se presentarán como
corregidos ni se sortearán para declarar un recorrido manual completo.

## 7. Publicación y siguientes pasos

- Commit inicial publicado: [`6fc9810` — diagnóstico, diseño y QA preliminar](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/commit/6fc9810).
- PR creado: [#28](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28),
  **Draft**, dirigido a `develop`, autor `Josue-EC16` y `Refs #13`.
- Historial completo de actualizaciones: [Commits del PR #28](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28/commits).
- Revisión formal solicitada a `vicentesystemdev` sobre diagnóstico, reglas
  y orden de integración. No hay aprobación ni integración que acreditar.
- No hacer merge, auto-merge, push a ramas protegidas ni cerrar el Issue.
- Tras coordinar e implementar, actualizar este mismo archivo con archivos
  modificados, evidencias, pruebas automáticas/manuales y limitaciones reales.

## 8. Aviso a Vicente — plantilla oficial adaptada

Basado en `Docs/Equipo/05_PLANTILLA_AVISO_PR.md`. Texto listo para que Marcelo
lo envíe por WhatsApp; no acredita que ya se haya enviado.

**Campus Glitch — Draft para coordinación del Issue #13**

- **Integrante:** Marcelo Escobar (@Josue-EC16).
- **Issue:** #13 — Derrota, vidas, Game Over y checkpoints.
- **Rama:** `feature/marcelo/13-game-over`.
- **Commits propios:** `6fc9810` — diagnóstico, diseño y QA preliminar (#13).
  Las actualizaciones documentales posteriores se consultan en
  [Commits del PR #28](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28/commits).
- **URL del PR a `develop`:** https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28.
- **Qué cambié:** documenté el diagnóstico y el diseño de Game Over, las reglas
  elegidas para caída/reintento/pausa y los casos de QA en este expediente.
  Todavía no modifiqué la lógica compartida de `CGGame.cs` o `CGEntities.cs`.
- **Qué debe probar Vicente:** en esta fase, revisar las reglas de §3 y confirmar
  el orden con Carla/PR #27. Después de implementar, ejecutar D1–D15 de §6,
  principalmente cero corazones → Game Over, reintento, menú y guardado intacto.
- **Pruebas que ya ejecuté:** verificaciones de Git, consulta de Issues/PR y
  análisis estático. No ejecuté nuevas pruebas de Unity ni builds en esta fase.
- **No probado / riesgos:** Game Over todavía no implementado; coordinación
  pendiente; ambos scripts compartidos y arnés de Carla requieren integración
  coordinada. PlayMode, teclado/ratón real, mando y Android pendientes.
- **Revisor solicitado:** @vicentesystemdev (solicitud formal registrada en GitHub).
- **Estado:** Draft — diagnóstico/diseño; no listo para integración funcional.

**El push es únicamente a mi rama. Este trabajo NO está integrado en `develop`
o `main`. No fusionar sin coordinación, implementación, revisión y QA.**
