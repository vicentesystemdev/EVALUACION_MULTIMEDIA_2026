# Issue #13 — Implementé mi sistema de derrota y Game Over

- **Mi cuenta:** Marcelo Escobar (`Josue-EC16`).
- **Mi rama:** `feature/marcelo/13-game-over`.
- **Mi base:** `origin/develop` @ `048b3e3dd93f2240aa85e388ae2233f68dab1a7b`.
- **Mi motor y entorno:** Unity `6000.3.16f1`, Windows nativo.
- **Mi fecha de entrega:** 2026-11-07.
- **Mi estado:** implementé la corrección y superé mi QA de lógica; solicito revisión y conservo los límites indicados abajo.
- **Mi Issue:** [#13 — Derrota, vidas, Game Over y checkpoints](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/13).
- **Mi PR hacia `develop`:** [#28](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28).
- **Mi revisor e integrador:** Vicente (`vicentesystemdev`).

## 1. Qué corregí

Corregí E4: cuando perdía el último corazón, el juego me reaparecía con tres
corazones y continuaba sin derrota. Encontré que el daño mortal, la caída al
vacío y el reinicio desde pausa utilizaban la misma reaparición con curación.

Separé esas situaciones. Ahora, al llegar a cero, entro en Game Over y puedo
seleccionar REINTENTAR o MENÚ PRINCIPAL. No restauré las escenas ni los scripts
del prototipo anterior y no cambié el formato del guardado.

En esta sesión confirmé que ya podía completar la integración coordinada.
Consulté el PR #27 de Carla, que sigue abierto y comparte `CGGame.cs` y
`CGEntities.cs`. No fusioné su PR ni incorporé sus commits por mi cuenta;
preparé mi cambio sobre la base vigente y dejé indicada la adaptación de QA.

## 2. Las reglas que implementé

| Situación | Mi resultado |
|---|---|
| Recibo daño válido de un enemigo | Descuento corazones y respeto 1,1 s de invulnerabilidad. |
| Recibo daño cero, negativo o durante la derrota | Lo rechazo y conservo el estado. |
| Caigo al vacío | Descuento un corazón, incluso si todavía tenía invulnerabilidad. |
| Caigo y me queda vida | Reaparezco en el checkpoint con los corazones restantes. |
| Llego a cero | Abro Game Over una sola vez, detengo al héroe y congelo la partida. |
| Elijo REINTENTAR | Recupero tres corazones y vuelvo al checkpoint; sin checkpoint, al inicio. |
| Reinicio desde pausa | Me reposiciono sin recuperar corazones. |
| Elijo MENÚ PRINCIPAL o Escape/B | Salgo al menú con `Time.timeScale = 1`. |

Conservo los fragmentos recogidos, la terminal resuelta, los enemigos eliminados
y el estado del jefe durante el reintento. No reconstruyo el mundo ni duplico
recompensas. Ese avance sigue siendo temporal: no añadí persistencia dentro del
nivel ni cambié el desbloqueo de niveles ya superados.

Conservé la activación por interacción del checkpoint existente. No presento
como aprobado ningún ajuste pendiente del GDD ni cambio el alcance a enemigos
complejos; trabajaré el #14 en su propia rama después de integrar el #13.

## 3. Cómo lo implementé

| Archivo | Qué cambié |
|---|---|
| `Assets/CampusGlitch/Scripts/CGGame.cs` | Agregué `Page.GameOver`, la pantalla IMGUI, sus dos opciones, navegación, bloqueo del mismo fotograma y una entrada diferenciada para caídas. Quité la curación incondicional de la reaparición y protegí ataques, recogidas, interacciones y victoria fuera del juego activo. |
| `Assets/CampusGlitch/Scripts/CGEntities.cs` | Conservé `Damage(int)`, separé el daño de caída, limité la vida a cero y limpié movimiento, velocidad, salto almacenado, ataque y habilidad al detener/reaparecer. Evité que un trigger destruya un fragmento durante Game Over. |
| `.gitignore` | Agregué `/QA_Marcelo/` para excluir mis herramientas, copias, ejecutables y resultados temporales. |
| Este archivo | Actualicé mi expediente único del #13 y el aviso para Vicente. |

Reutilicé el audio `Damage`; no agregué recursos ni una animación nueva.
Al reaparecer aplico 1,1 s de protección y limpio la entrada pendiente para
que Espacio, usado al confirmar, no se convierta en un salto automático.

No escribo ni reinicio el guardado desde Game Over o Reintentar. No modifiqué
escenas, prefabs, paquetes, ProjectSettings ni los `.meta` ajenos de mi árbol
local. Conservé mis instrucciones personales fuera del versionado.

## 4. Qué probé realmente

### Mi entorno de QA

Preparé una copia temporal a partir de mi rama, con el producto aislado
`MARCELO_QA_13_20261107_082131`. Copié exactamente mis dos scripts corregidos y
comparé sus SHA-256 con los del proyecto original. Ejecuté Unity en esa copia,
no sobre mi guardado personal.

Conservé mi arnés, archivos de resultados, cachés auxiliares y builds en
`QA_Marcelo/issue13/` o en la copia temporal externa al repositorio. La carpeta
`QA_Marcelo/` está ignorada; esos archivos no forman parte del PR. En este
expediente registro cómo reproduzco los escenarios y los límites de la prueba.

### Mi regresión antes y después

- En la versión anterior a mi corrección obtuve cuatro comprobaciones correctas
  de carga/daño/invulnerabilidad y un fallo esperado: al llegar a cero, no entraba
  en Game Over. Conservé ese resultado en `baseline/resultados.json`.
- Con mi corrección obtuve **59 comprobaciones correctas y 0 fallidas** en
  `fixed-focus/resultados.json`. Incluí la carga de las **18 escenas**.
- Utilicé dispositivos simulados de Input System y su ciclo Dynamic. Configuré
  el foco del Game View solo en mi copia para que el modo batch procesara la
  entrada. Eso no equivale a un mando físico ni altera los ajustes del juego.

| Grupo | Qué comprobé | Mi resultado |
|---|---|---|
| Vida y daño | Comprobé 3 → 2 → 1 → 0, invulnerabilidad, daño excesivo y rechazo de daño no positivo. | Correcto en PlayMode. |
| Derrota | Comprobé Game Over sin curación, física detenida, rechazo de acciones y confirmación del mismo fotograma. | Correcto en PlayMode. |
| Caída | Moví al héroe debajo de `y = -9` y dejé actuar a `FixedUpdate`, con y sin checkpoint y con invulnerabilidad activa. | Desconté un corazón; a cero entré en Game Over. |
| Reintento | Confirmé con Espacio y con A de mando simulado; verifiqué vida, posición, tiempo y ausencia de salto almacenado. | Correcto en PlayMode. |
| Pausa | Pausé después de reaparecer y reinicié desde el checkpoint con un corazón. | Conservé ese corazón y reanudé el tiempo. |
| Navegación | Cambié entre las dos opciones con flechas; volví al menú con Enter, Escape y mando simulado. | Correcto en PlayMode. |
| Avance del intento | Recogí fragmentos con colisiones en posiciones controladas, resolví la terminal por su lógica y eliminé un enemigo. | Conservé esos estados al reintentar, sin regenerar objetos. |
| Progreso persistente | Comparé JSON, `.bak` y `.tmp` durante derrota/reintento/menú y recargué datos guardados. | Conservé el progreso anterior. |
| Combate de N2 | Apliqué mordida y ejecuté la lógica de contacto de un enemigo. | Desconté vida del enemigo y del héroe según correspondía. |
| Victoria de N2 | Completé los requisitos mediante callbacks controlados y recargué el JSON. | Conservé N1 y persistí N2 con el siguiente desbloqueo. |
| Carga | Abrí las ocho pantallas, nueve niveles y el extra. | Cargué las 18 escenas en el editor. |
| Estado del jefe y fragmentos | Reintenté con el jefe marcado como derrotado y provoqué un trigger durante Game Over. | Conservé el jefe y no destruí el fragmento durante la derrota. |

No presento estos escenarios automatizados como un recorrido físico completo:
utilicé posiciones controladas, callbacks y enemigos detenidos donde necesitaba
un escenario determinista. No corregí ni doy por resueltos los bloqueos de
fragmentos/plataformas de otros Issues.

### Mi compilación y mis límites visuales

Compilé mi copia Windows x64: **Succeeded, 0 errores, 209 advertencias,
140.584.379 bytes**. El ejecutable incluye un iniciador temporal de QA que no
está en el proyecto original ni en el PR. También repetí la compilación con
caché limpia; mantengo separado compilar de ejecutar correctamente.

Al intentar cargar `SC_Level01` desde ese ejecutable, encontré un fallo nativo:
`level8 is corrupted / Position out of bounds`. `level8` identifica el archivo
del índice 8 del build (`SC_Level01`), no el Nivel 8. Reproduje el mismo fallo
con los scripts de `6eb8e08`, anteriores a mi corrección, y con caché limpia.
En ambos casos el iniciador quedó en carga, antes de ejecutar Game Over.
Por esa comparación no lo presento como una regresión introducida por #13.
Conservé `player-baseline.log`, `player-clean.log` y los reportes de build en
mi carpeta ignorada. No reescribí las escenas compartidas para ocultarlo;
solicito revisar ese bloqueo con Vicente en la QA de Windows (#16).

Después ejecuté mi lógica en un reproductor QA que genera N1 desde Bootstrap,
solo dentro del iniciador temporal. Llegué a Game Over y ejecuté el reintento
por su callback; terminé en `Playing` con tres corazones, según
`Build_RenderLogic/ui-render.json`. Ese control no sustituye la carga de la
escena original ni una interacción real con ratón.

Inspeccioné las capturas del reproductor, tanto batch como fuera de batch:
salieron negras. No las acepté como evidencia visual y no marqué el render
ni el recorrido del `.exe` original como correctos. Tampoco pude
controlar la ventana con ratón: tanto computer-use como la alternativa integrada
informaron que el servicio nativo no estaba disponible. Conservo ese caso como
**No ejecutado**, no como un éxito de navegación.

En el editor registré una excepción de indexado `UnityEditor.Search` al iniciar
la copia y avisos de referencias a scripts incrustadas en escenas. No oculté
esos mensajes ni afirmo que toda la consola esté libre de incidencias. Mi suite
de lógica terminó sus 59 comprobaciones sin fallos.

También corregí limitaciones de mi arnés local: separé la captura del editor
batch, ajusté la API interna Dynamic de la versión instalada y habilité su foco
de entrada de prueba. No cambié el código del juego para ocultar esos fallos
del arnés.

**Conservo pendientes:** clic real con ratón, mando físico, Android, escucha
humana del audio y recorrido manual completo de N1/N2. Mi editor sí tiene
soporte Android instalado, pero no generé ni validé un APK para esta entrega.

## 5. Cómo reproduzco la aceptación del #13

1. Abro `SC_Level01` en Unity `6000.3.16f1` y verifico tres corazones.
2. Recibo tres golpes separados por la invulnerabilidad y compruebo que llego a
   Game Over sin reaparecer automáticamente.
3. Selecciono REINTENTAR con flechas y Enter/Espacio: compruebo tres corazones,
   posición inicial o checkpoint, tiempo normal y ausencia de salto inmediato.
4. Activo el checkpoint por interacción. Caigo con dos corazones y compruebo
   que reaparezco con uno; al caer de nuevo, compruebo Game Over.
5. Reinicio desde pausa con uno o dos corazones y compruebo que no me curo.
6. Reintento después de recoger datos, resolver la terminal y eliminar un
   enemigo; compruebo que conservo el avance de ese intento.
7. Selecciono MENÚ PRINCIPAL y compruebo que vuelvo con el tiempo normalizado.
   Cierro y reabro sobre un guardado aislado y compruebo mis niveles anteriores.
8. Repito el daño, reintento y salida en N2. Registro por separado cualquier
   prueba con ratón, mando físico o Android que todavía no haya ejecutado.

Para repetir mi arnés local utilizo una copia aislada y estos parámetros:

```powershell
& $unity -batchmode -force-d3d11 -projectPath $qaProject `
  -executeMethod CampusGlitch.MarceloLocalQA.CGIssue13QA.Begin `
  -marceloQaOutput $qaOutput -marceloQaPhase fixed -logFile $qaLog
```

Mi arnés es local e ignorado, por lo que esos comandos requieren su copia;
no remito a un archivo inexistente en GitHub. Mantengo los pasos manuales
anteriores para la revisión independiente de Vicente.

## 6. Cómo coordino la integración con Carla

Identifiqué que el caso 0 de `CGPlaySessionQA` del PR #27 espera que
`KillHero()` con un corazón me reaparezca curado. Esa expectativa corresponde
al comportamiento que corregí. Para integrar las dos entregas propongo
sustituir exclusivamente esa comprobación por una caída no mortal:

```csharp
Move(game, 35);
game.InteractNearPlayer();
Check(world.checkpoint, "Checkpoint no se activa");
game.Hero.hearts = 3;
game.HandleHeroFall();
Check(game.Hero.hearts == 2 &&
      Mathf.Abs(game.Hero.transform.position.x - 35.5f) < .1f,
      "Caída no mortal no conserva vida/checkpoint");
```

No añadí a mi rama el arnés de Carla, porque su PR sigue pendiente. Dejé este
ajuste concreto para coordinarlo con Vicente y repetir su QA después de la
integración. Tampoco doy por integradas sus correcciones de menús o terminal.

## 7. Mis commits y mi publicación

- [`6fc9810`](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/commit/6fc9810): documenté mi diagnóstico y QA preliminar.
- [`6eb8e08`](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/commit/6eb8e08): registré mi PR Draft y mi aviso inicial.
- `4998fb5`: implementé mi derrota y reintento desde checkpoint y excluí mi QA temporal.
- Mantengo el [historial completo de mi PR #28](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28/commits), incluidas las actualizaciones de este expediente.

Publico únicamente en mi rama y solicito revisión de Vicente. No fusiono mi PR,
no habilito auto-merge y no cierro el Issue por mi cuenta. No modifiqué `main`
ni `develop`. Mantengo los cambios locales de importación fuera de mis commits.

## 8. Mi aviso a Vicente — plantilla oficial

Adapto `Docs/Equipo/05_PLANTILLA_AVISO_PR.md` en este mismo expediente.
Dejo mi mensaje preparado para WhatsApp; no afirmo que ya lo haya enviado.

**Campus Glitch — implementé mi corrección del Issue #13 y solicito revisión**

- **Integrante:** soy Marcelo Escobar (@Josue-EC16).
- **Issue:** trabajé el #13 — Derrota, vidas, Game Over y checkpoints.
- **Rama:** trabajé en `feature/marcelo/13-game-over`.
- **Commits propios:** `4998fb5` contiene mi implementación; conservé `6fc9810`
  y `6eb8e08` como preparación. Registro las actualizaciones documentales en
  [los commits de mi PR](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28/commits).
- **URL del PR a `develop`:** actualizo https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/28.
- **Qué cambié:** implementé Game Over, reintento y retorno al menú. Separé
  caída, daño y reaparición; evité curarme desde pausa y conservé el progreso.
  También excluí mis archivos temporales de QA.
- **Qué debe probar Vicente:** propongo repetir los ocho pasos de §5,
  principalmente cero corazones, checkpoint, pausa, reintento y guardado.
  También solicito coordinar el ajuste de QA del PR #27 y revisar el fallo
  nativo que encontré al cargar la escena en mi ejecutable aislado.
- **Pruebas que ya ejecuté:** reproduje el fallo previo y obtuve 59
  comprobaciones correctas/0 fallidas con la corrección en PlayMode, incluidas
  18 escenas. Compilé Windows QA con 0 errores y registré sus advertencias.
- **No probado / riesgos:** no aprobé el render del `.exe`, clic real con
  ratón, mando físico, Android, escucha del audio ni recorrido manual completo.
  Comparto dos scripts con Carla y dejé identificada su expectativa antigua
  de reaparición. No oculto las incidencias del editor o del ejecutable QA.
- **Revisor solicitado:** solicito revisión a @vicentesystemdev.
- **Estado:** terminé mi implementación y mi QA de lógica; espero revisión y
  validación de los límites indicados. No está integrado en `develop`.

**Publiqué únicamente en mi rama. No hice merge ni modifiqué `main` o `develop`.**
