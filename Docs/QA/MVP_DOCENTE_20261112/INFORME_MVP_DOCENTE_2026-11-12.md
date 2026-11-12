# Campus Glitch — Informe técnico de la candidata MVP docente

**Fecha:** 2026-11-12 · **Unity:** 6000.3.16f1 · **Rama candidata:** `qa/mvp-docente-20261112` (worktree aislado `D:\Unity\EVALUACION_MULTIMEDIA_2026_MVP`)

> **Lectura de estados.** **PASS MANUAL**: jugado físicamente por una persona. **PASS AUTOMATIZADO**: comprobado por arneses en Play Mode con física y entrada simulada (teclado/mando), **no** sustituye una partida humana. **SMOKE**: solo arranque. **FAIL**: defecto reproducido. **BLOCKED**: impedido por otro defecto. **PENDIENTE**: no ejecutado.
>
> Los 41/41 PASS MANUAL de la integración `31daa0f` **no se trasladan automáticamente**: el PR #31 modificó `CGGame.cs`, `CGEntities.cs` y la UI. Deben repetirse sobre esta candidata (anexo 1).

---

## A. Estado de la integración

### Rama base y SHAs (verificados tras `git fetch --all --prune` y refs `pull/*/head`)

| Referencia | SHA | Relación con la candidata |
|---|---|---|
| Base QA validada `qa/integracion-mvp-20261108` | `31daa0f` | Primer padre de la candidata |
| PR #27 `feature/carla` | `00d3e68` | Ya contenido en `31daa0f` |
| PR #28 `feature/marcelo/13-game-over` | `041e0fb` | Ya contenido en `31daa0f` |
| PR #29 `fix/victor/18-fragmento-inaccesible` | `d2835ac` | Ya contenido en `31daa0f` |
| PR #30 `fix/victor/19-salto-plataformas` | `b1cbd06` | Ya contenido en `31daa0f` y en #31 |
| PR #31 `feature/victor-integracion` | `6f77471` | **Integrado ahora** (64 commits propios; #29/#30 no se duplican) |
| `origin/develop` | `048b3e3` | Ancestro de la candidata → avance rápido posible |
| `origin/main` | `4865a1d` | Ancestro de la candidata → avance rápido posible |

### Cambios recibidos por integrante

| Integrante | PR | Aporte | Commits |
|---|---|---|---|
| Carla (`VarlaC_dev_20`) | #27 | Terminal con teclado/mando y CERRAR, victoria/créditos, volumen, borrado seguro, animación, sonidos y subtítulos, arneses QA | 19 |
| Marcelo (`Marcelo Escobar`) | #28 | Game Over → REINTENTAR/MENÚ, caída con pérdida de corazón, bloqueo de entrada | 4 |
| Víctor (`VictorDev`) | #29/#30 | Fragmentos accesibles (#18), salto y plataformas (#19) | 8 + previos |
| Víctor (`VictorDev`) | #31 | Menú y mapa nuevos, configuración, controles y remapeo, ayuda, créditos, personalización, poderes (habilidades, proyectil, escudo), actores/NPC, ambientación de 10 niveles, móvil/Android | 64 |

Autoría e historial preservados: merge `--no-ff`, sin squash ni rebase.

### Conflictos y resoluciones (merge `0173aa6`)

Criterio: **arquitectura de UI de Víctor como base** y reaplicación explícita de los comportamientos probados de Carla y Marcelo.

| Archivo | Conflicto | Resolución |
|---|---|---|
| `CGGame.cs` (11 bloques) | Víctor reestructuró `Update`/`OnGUI` en pantallas parciales; Marcelo añadió `Page.GameOver`; Carla cambió terminal, victoria, borrado y volumen | Se conserva Game Over (REINTENTAR/MENÚ, `timeScale 0`, guardas `!Playing` en fragmentos/ataque/jefe/interacción/victoria), terminal de 4 opciones con CERRAR y antirrebote, helpers estáticos de Carla (`HandleVictorySelection`, `HandleEndingSelection`, `HandleResetSelection`, `MoveQuizSelection`, `VolumeAfterStep`), subtítulos de sonido en HUD; todo el retorno al juego usa `resumedInputFrame` de Víctor |
| `CGEntities.cs` (3) | Habilidades/escudo de Víctor frente a `ApplyDamage`/`StopActions`/`RespawnAt` de Marcelo | Unión: el escudo absorbe golpes de enemigos pero **no** caídas; `StopActions` cancela habilidades |
| `CGAudio.cs` (1) | Mezcladores por canal de Víctor frente a subtítulos de Carla | Unión: `Play` describe el sonido y usa los mezcladores |
| `.gitignore` (1) | Listas distintas | Unión de ambas |

Decisiones de comportamiento:
- **Borrar progreso** ya no está en el menú principal: Víctor lo movió a *Configuración → Guardado → BORRAR AVANCE…*, con **CANCELAR preseleccionado** (se mantiene la intención de Carla). `Page.ConfirmReset` queda como respaldo.
- **Créditos desde el final**: se usa `OpenUIScreen("SC_Credits")`, la corrección de Carla (`ae73422`).
- Las pantallas de mapa, controles y volumen de Carla quedaron reemplazadas por las de Víctor, que tienen sus propias regresiones.

### Commits creados en la candidata

| SHA | Tipo | Descripción |
|---|---|---|
| `0173aa6` | merge | PR #31 sobre `31daa0f` con la resolución descrita |
| `f1c613e` | test | Arneses de Carla: Game Over → REINTENTAR; borrado en Configuración |
| `fc0e94d` | test | Arneses de Carla: animación del Guardián y posición real del fragmento 2 |
| `6a65a55` | test | `CGMvpBatchQA`: ejecutor batch con guardado aislado |
| (siguiente) | test | Regresión de fragmentos: atacar al jefe de frente (combate del PR #31) |
| (siguiente) | docs | Este informe, evidencias y README |

**No se modificó código de juego fuera del merge.** No se regeneraron escenas, no se usaron `reset --hard`, `clean` ni force push, y no se publicó nada en el remoto.

---

## B. Matriz de los diez niveles

Fuentes: `evidencias/` (pasadas `results_run1..3`). Comandos en el anexo 3.

| Nivel | Escena | Arranque | Jugabilidad (ruta completa con entrada simulada) | Fragmentos 3/3 | Terminal | Jefe | Victoria | Desbloqueo | Checkpoint / Game Over | Estado final |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | SC_Level01 | PASS AUTO | PASS AUTO teclado **y mando** | PASS AUTO (parado y carrera) | PASS AUTO | — | PASS AUTO | N2 ✔ AUTO | PASS AUTO (Game Over → REINTENTAR) | **PASS AUTOMATIZADO** · PASS MANUAL previo (`31daa0f`) por revalidar |
| 2 | SC_Level02 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | — | PASS AUTO | N3 ✔ AUTO | checkpoint PASS AUTO | **PASS AUTOMATIZADO** · PASS MANUAL previo por revalidar |
| 3 | SC_Level03 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | — | PASS AUTO | N4 ✔ AUTO | checkpoint PASS AUTO | **PASS AUTOMATIZADO** · manual PENDIENTE |
| 4 | SC_Level04 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | — | PASS AUTO | N5 ✔ AUTO | checkpoint PASS AUTO | **PASS AUTOMATIZADO** · manual PENDIENTE |
| 5 | SC_Level05 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | — | PASS AUTO | N6 + Extra ✔ AUTO | checkpoint PASS AUTO | **PASS AUTOMATIZADO** · manual PENDIENTE |
| 6 | SC_Level06 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | Boss_Secuencias PASS AUTO | PASS AUTO | N7 ✔ AUTO | PASS AUTO (Game Over → REINTENTAR) | **PASS AUTOMATIZADO** · manual PENDIENTE |
| 7 | SC_Level07 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | — | PASS AUTO | N8 ✔ AUTO | checkpoint PASS AUTO | **PASS AUTOMATIZADO** · manual PENDIENTE |
| 8 | SC_Level08 | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | Boss_IF_ELSE PASS AUTO | PASS AUTO | N9 ✔ AUTO | PASS AUTO (Game Over → REINTENTAR) | **PASS AUTOMATIZADO** · manual PENDIENTE |
| 9 | SC_Level09 | PASS AUTO | PASS AUTO teclado **y mando** | PASS AUTO | PASS AUTO | Core_Glitch PASS AUTO | Final → Créditos PASS AUTO | — | PASS AUTO (Game Over → REINTENTAR) | **PASS AUTOMATIZADO** · manual PENDIENTE |
| Extra | SC_Extra_Subsuelo | PASS AUTO | PASS AUTO teclado | PASS AUTO | PASS AUTO | — | PASS AUTO | — | PASS AUTO (Game Over → REINTENTAR) | **PASS AUTOMATIZADO** · manual PENDIENTE |

Qué cubre cada arnés:
- **Ruta** (`CGRouteRegression`): abre la escena del nivel, entra en Play Mode y **conduce al lobo desde la salida** con teclas o mando simulados sobre la física real. Recoge 3 fragmentos, activa el checkpoint, resuelve la terminal navegando con flechas y Enter, ataca a enemigos y jefes con J y entra al portal. Comprueba victoria, `completed` y desbloqueo. Resultado: **12/12** (10 niveles con teclado, N1 y N9 con mando).
- **Fragmentos** (`CGFragmentRegression`, issue #18): en los 10 niveles, parado y en carrera, cada fragmento por trigger (+1 exacto, sin doble conteo), portal bloqueado sin objetivos, terminal y jefe. Resultado: **289/289**.
- **Plataformas** (`CGPlatformRouteRegression`, #19): 10 niveles × 2 velocidades, **20/20**. **Física** (`CGHeroPhysicsRegression`): **56/56**.
- **Sesión de Carla** (`CGPlaySessionQA`): niveles 1, 8, 6, 9 y Extra. Checkpoint, **última vida → Game Over (`timeScale 0`) → REINTENTAR con 3 corazones en el checkpoint**, triggers, terminal incorrecta/correcta, portal, victoria persistida tras recarga y transición final. **PASS**.
- **Limitación**: los arneses cargan cada escena directamente; el recorrido continuo Menú → Mapa → N1 → … → Créditos de una sola sesión **no** está automatizado. El desbloqueo y los destinos del mapa sí lo están (`CGMapRegression` 37/37).

### UI y sistemas (PASS AUTOMATIZADO)

| Área | Arnés | Resultado |
|---|---|---|
| Menú con teclado/mando | `CGMenuControlRegression` | 38/38 |
| Mapa del campus | `CGMapRegression` | 37/37 |
| Configuración (incl. borrar avance) | `CGSettingsRegression` | 45/45 |
| Controles y remapeo | `CGControlsRegression` | 39/39 |
| Ayuda y tutorial (incl. táctil) | `CGHelpRegression` | 66/66 |
| Créditos | `CGCreditsRegression` | 31/31 |
| Personalización | `CGCustomizationRegression` | 31/31 |
| Poderes / habilidades | `CGAbilityRegression` | 67/67 |
| Animación en Play Mode | `CGAnimationPlayRegression` | 15/15 |
| Arte del Guardián | `CGGuardianArtRegression` | 23/23 |
| Integración de Carla (entradas, JSON aislado, terminal, sprites, audio, subtítulos) | `CGIntegrationQA` | PASS |
| Menú de Carla (terminal, borrado, victoria, volumen, Cancelar por defecto) | `CGMenuRegression` | PASS |
| Vista previa de escenas en el editor | `CGSceneContentRegression` | **No ejecutable en batch** (`CGEditorWorldPreview` se desactiva con `Application.isBatchMode`). Solo afecta a la vista previa del editor, no al juego |

---

## C. Riesgos

### Defectos bloqueantes (P0)
**Ninguno reproducido** en compilación, arneses ni build.

### Problemas secundarios
| # | Prioridad | Descripción | Impacto |
|---|---|---|---|
| R1 | P2 | Las escenas de nivel contienen `MonoScript` incrustados sin GUID (rig de cámara antiguo): unos 27 avisos «referenced script missing» por entrada a Play Mode en el editor | Solo en el editor. El filtro de build de Víctor los elimina: el `Player.log` del .exe está limpio |
| R2 | P2 | Avisos CS0618 `ContactFilter2D.NoFilter()` obsoleto (3 sitios, código del PR #31) | Ninguno funcional |
| R3 | P2 | Unity 6 reserializa 16 `.meta` (espacios finales) y `TimeManager.asset` (formato) al abrir el proyecto | Cambio sin efecto funcional; **no** incluido en commits |
| R4 | P1 por validar | Rutas automatizadas superadas, pero la dificultad real (enemigos, saltos precisos) no se ha medido con una persona en N3–N9 y Extra | Requiere la pasada manual del anexo 1 |
| R5 | Info | `Extra → SIGUIENTE NIVEL` lleva al Nivel 1 (diseño heredado de `AfterVictory`) | No bloquea |

### Pruebas pendientes
- **PASS MANUAL** de N1–N9 y Extra sobre esta candidata (anexo 1).
- Persistencia **entre reinicios del editor/ejecutable** con la partida real (pendiente desde el 08/11). Automatizado: persistencia tras recarga de `CGStore` con JSON aislado.
- **Controles en el ejecutable**: no certificados. La entrada enviada al .exe no llegó porque el editor del proyecto QA, que se abrió durante la prueba, tomó el foco.
- Android/táctil en un dispositivo físico (fuera del alcance de la demo en Windows).

### Funciones no verificadas por mí
Android/APK y exportación USB (PR #31), gestos móviles reales, mandos físicos (solo mando virtual del Input System), rendimiento en el equipo del docente.

---

## D. Recomendación de release

| Elemento | Estado |
|---|---|
| Compilación C# (runtime + editor) | ✅ 0 errores |
| Niveles 1–9 + Extra | **PASS AUTOMATIZADO** completo. **Ninguno certificado como PASS MANUAL sobre esta candidata** |
| Ejecutable Windows x64 (`Build_Windows/CampusGlitch.exe`) | Build **Succeeded, 0 errores**. **SMOKE**: arranca con gráficos (portada 1280×720), proceso estable y `Player.log` sin errores. **Controles en el .exe: NO certificados** |
| Guardado personal | Sin cambios (SHA-256 idéntico antes y después de todas las pasadas) |

**Niveles listos para el docente:** técnicamente, los diez superan el recorrido automatizado de inicio a fin. Para la demostración recomiendo **certificar a mano al menos N1 → N3 y N9**, y si hay tiempo la secuencia completa del anexo 1.

**Condiciones antes de publicar en `main`:**
1. Ejecutar el anexo 1 (mínimo N1, N2, N3, N6 y N9) en el editor o el .exe y registrar las capturas.
2. Confirmar los controles de teclado en el .exe (anexo 2).
3. Aprobar el plan de integración del anexo 4 (decisión del líder; requiere autorización explícita).

---

## Anexo 1 — Pruebas manuales nivel por nivel (sin tocar la partida personal)

Preparación: en el editor, abrir `D:\Unity\EVALUACION_MULTIMEDIA_2026_MVP`. Antes de empezar, copiar la carpeta `%USERPROFILE%\AppData\LocalLow\DefaultCompany\CAMPUS_GLITCH_TEMP` (ya existe un respaldo verificado en `D:\Unity\QA_BACKUP_20261112\`). Al terminar, restaurar la copia. Iniciar desde `SC_Bootstrap` → Enter → *Configuración → Guardado → BORRAR AVANCE…* para partir de cero.

Para cada nivel N (captura en los puntos marcados con 📷):
1. Desde *MAPA DEL CAMPUS* elegir N (debe estar desbloqueado; el siguiente debe aparecer bloqueado) 📷.
2. A/D mover, Shift correr, Espacio saltar a una plataforma elevada.
3. Dejarse golpear una vez por un enemigo: pierde un corazón y queda invulnerable un momento.
4. En x≈35 pulsar E sobre el checkpoint; caer fuera del mapa: vuelve al checkpoint con un corazón menos.
5. Recoger los 3 fragmentos (HUD 3/3) 📷.
6. Terminal (x≈40, E): probar una respuesta incorrecta (sigue abierta), Escape (se cierra), reabrir y responder bien.
7. N6, N8 y N9: derrotar al jefe con J; el portal debe negar la salida mientras viva.
8. Portal (x≈69, E) → *¡NIVEL RESTAURADO!* 📷 → SIGUIENTE NIVEL.
9. Una vez (N1): perder las 3 vidas → GAME OVER 📷 → REINTENTAR → checkpoint con 3 corazones y fragmentos conservados.
10. N5: comprobar que el Extra aparece desbloqueado en el mapa. N9: final → VER CRÉDITOS 📷.
11. Cerrar y volver a abrir el juego o el editor: el progreso debe mantenerse 📷.

Anotar PASS/FAIL por paso; cualquier bloqueo es P0.

## Anexo 2 — Ejecutable Windows
`Build_Windows/CampusGlitch.exe` (carpeta ignorada por git; no copiar `CAMPUS_GLITCH_TEMP_BurstDebugInformation_DoNotShip`). Abrir, pulsar Enter en la portada, navegar el menú con flechas, entrar al Nivel 1, moverse, saltar y pausar con Escape. Usa la misma carpeta de guardado que el editor: respaldarla antes.

## Anexo 3 — Reproducir la QA automatizada
```
set CG_QA_SAVE_DIR=D:\Unity\QA_MVP_<fecha>\isolated_save
Unity.exe -batchmode -projectPath D:\Unity\EVALUACION_MULTIMEDIA_2026_MVP ^
  -executeMethod CampusGlitch.EditorQA.CGMvpBatchQA.RunFromCommandLine ^
  -cgOut D:\Unity\QA_MVP_<fecha>\results -cgSuite "route:0,route:1,...,route:9,route:0:pad,fragment,physics,playsession,play:CGMapRegression"
```
`-cgOut` debe ser una carpeta nueva **fuera** del proyecto. El resumen queda en `summary.txt`.

## Anexo 4 — Plan de integración hacia `develop` y release a `main` (no ejecutado)
`origin/develop` (`048b3e3`) y `origin/main` (`4865a1d`) son ancestros de la candidata, así que no hacen falta merges nuevos ni reescritura.

1. Publicar la candidata para revisión: `git push -u origin qa/mvp-docente-20261112`.
2. PR **`qa/mvp-docente-20261112` → `develop`** (merge commit, sin squash). Así se integran a la vez #27, #28, #29, #30 y #31 con su historial. Después, cerrar esos PR como «integrados vía la candidata MVP», para no duplicar commits.
3. Tras la aprobación y las pruebas manuales: PR **`develop` → `main`** como release (merge commit), con etiqueta `v1.0-mvp-docente`.
4. `main` pasa del prototipo de tres escenas al juego completo; la escena inicial es `SC_Bootstrap` (índice 0 en Build Settings).
