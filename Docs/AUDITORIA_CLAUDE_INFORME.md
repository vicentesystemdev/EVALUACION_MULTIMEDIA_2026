# Auditoría técnica independiente — Campus Glitch

| Campo | Valor |
|---|---|
| Repositorio | `vicentesystemdev/EVALUACION_MULTIMEDIA_2026` |
| Rama auditada | `origin/develop` @ `9257b4e` («Preparar exportación APK de Android», 2026-10-22) |
| Unity | 6000.3.16f1 (`a56f230f6470`), URP 2D, Input System 1.19.0 |
| Fecha | 2026-11-05 |
| Modo | Solo lectura. Sin commits, push, merge, reset ni rebase. Árbol local limpio (`feature/build-multiplataforma-dia2` @ `8c48d0a`), sin cambios descartados. El stash local `2630813` («Respaldo antes de evaluacion dia 2») sigue intacto. |
| Método | Análisis estático de Git, GitHub API pública, C#, YAML de escenas, `.meta`/GUID y `Resources`; **pruebas PlayMode reales** en una copia desechable de `develop` (`D:\CGAUDIT`, fuera del repo). |

Convención de evidencia: **[VERIFICADO-EJECUCIÓN]** = reproducido en Unity por prueba automática; **[VERIFICADO-ESTÁTICO]** = demostrado leyendo código/datos (causa y efecto inequívocos); **[NO VERIFICADO]** = requiere prueba manual o hardware.

---

## 1. Resumen ejecutivo

1. **El juego existe y es sustancial**: 9 niveles + subsuelo extra generados por código (`CGWorld`), 18 escenas en Build Settings, menús IMGUI, guardado JSON, controles teclado/mando/táctil. Todas las referencias de script y GUID de las escenas resuelven (0 GUID rotos, 0 GUID duplicados en 4 270 assets).
2. **El juego no se puede completar**: el fragmento de datos 2 del **Nivel 1** (y del Nivel 8) no se puede recoger porque una plataforma inalcanzable tapa al héroe (E6, medido en Unity). Hay además otros defectos de jugabilidad que bloquean la evaluación:
   - no existe Game Over ni pérdida real de vidas;
   - con teclado o mando, la opción «CANCELAR» de *Borrar progreso* **borra el progreso**;
   - la terminal (quiz) no se puede responder con teclado ni mando;
   - el salto (1,37 u) no alcanza ninguna plataforma elevada (tope mínimo 1,86 u).

   Ver §3; los resultados de ejecución están en §10.
3. **Multiplataforma no demostrada**: el editor instalado **no tiene Android Build Support** (`PlaybackEngines` solo contiene `windowsstandalonesupport`), el informe Android tiene todos los resultados en `[PENDIENTE]` y la carpeta `Evidencias/` está vacía. Correr y la habilidad no tienen botón táctil.
4. **Gitflow y trazabilidad son el mayor riesgo frente al docente**: el juego entró en **un solo commit** (`8c48d0a`, PR #8: 10 644 archivos, +955 601 líneas) firmado por Vicente, con trabajo originado en otro repositorio (remoto `juego` = `CAMPUS_GLITCH`, commits de Víctor). Ese commit **eliminó** los aportes anteriores de Carla (`MenuPrincipal.unity`), Marcelo (`GameOver.unity`, `GameManager.cs`) y Vicente (`GestorEscenas.cs`). Los 8 PR fusionados tienen **0 revisiones** y todos fueron auto-fusionados; hay commits directos a `develop` y `main` sin PR.
5. **Peso y duplicación**: 4 573 de 6 193 archivos de arte (74 %) son duplicados byte a byte; 625 sprites de biblioteca no usados por el código están dentro de `Resources/` y se empaquetan en cada build, además sin compresión (forzado por `CGPixelImporter`).
6. El **README de `develop` describe el prototipo de 3 escenas** (`MenuPrincipal`, `Nivel01`, `GameOver`, `GestorEscenas`) que ya no existe.

**Veredicto**: base jugable en Windows a nivel de lógica, pero **no apta para entrega** sin corregir los P0 de §6 y sin rehacer la trazabilidad mediante trabajo incremental atribuible a cada integrante.

---

## 2. Inventario de funcionalidades

| Funcionalidad | Implementado en | Estado | Evidencia |
|---|---|---|---|
| Carga de las 18 escenas del build | `EditorBuildSettings.asset` | Ver §10 (Smoke) | VERIFICADO-EJECUCIÓN |
| Splash → menú (Enter/A/toque) | `CGGame.cs:43` | Implementado | ESTÁTICO |
| Menú principal (9 opciones) | `CGGame.cs:63-65,181-186` | Funcional con defectos (ver E1, E2) | ESTÁTICO + EJECUCIÓN |
| Mapa de niveles y bloqueo | `CGGame.cs:76,187-204` | Implementado | ESTÁTICO |
| Movimiento, carrera, salto con *coyote time*/buffer | `CGEntities.cs:57-72` | Funcional; salto corto (E5) | EJECUCIÓN |
| Ataque (mordida) | `CGGame.cs:89`, `CGEntities.cs:62` | Implementado | ESTÁTICO |
| Enemigos (Bug_Terrestre, Bug_Saltador, Centinela) | `CGEntities.cs:78-99` | Parcial: patrulla por `transform`, daño por distancia, sin física | ESTÁTICO |
| Jefes (Secuencias, IF_ELSE, Core_Glitch) | `CGEntities.cs:80,95` | **Parcial**: 9 PV, estáticos; el «pulso» solo muestra un texto (sin ataque real) | ESTÁTICO |
| Vidas / corazones | `CGEntities.cs:74-76` | **Defectuoso**: al llegar a 0 se restauran 3 | EJECUCIÓN (E4) |
| Muerte por caída + respawn | `CGEntities.cs:72`, `CGGame.cs:92` | Implementado | EJECUCIÓN |
| Checkpoint (pulsar E en x≈35) | `CGGame.cs:96` | Implementado (manual, no automático) | ESTÁTICO |
| Game Over | — | **No existe** (la escena anterior fue borrada en `8c48d0a`) | ESTÁTICO + EJECUCIÓN |
| Misión: 3 fragmentos + terminal + jefe + portal | `CGGame.cs:93-107` | Lógica funcional; alcanzabilidad física ver E6 | EJECUCIÓN |
| Terminal/quiz | `CGGame.cs:44,109,236-238` | **Solo ratón/toque** (E3) | EJECUCIÓN |
| Desbloqueos (morado tras N2, extra tras N5) | `CGGame.cs:104-105` | Implementado | EJECUCIÓN |
| Guardado JSON atómico con `.bak` | `CGData.cs:23-42` | Funcional; `.bak` nunca se usa al cargar | EJECUCIÓN + ESTÁTICO |
| Pausa / continuar / reiniciar checkpoint | `CGGame.cs:42,85` | Funcional; reiniciar restaura corazones | EJECUCIÓN |
| Ajustes (volumen, destellos, subtítulos) | `CGGame.cs:55,87,212-216` | Parcial: volumen supera 100 % (E8); «subtítulos» no tiene efecto en ningún sitio | ESTÁTICO + EJECUCIÓN |
| Reasignación de teclas / mando | `CGGame.cs:36-40`, `CGData.cs` | Implementado; permite duplicados | ESTÁTICO |
| Personalización del lobo (rojo/morado, 2 modelos) | `CGGame.cs:52-54,205-211` | Implementado | ESTÁTICO |
| Animación del héroe por frames (sprites) | `CGEntities.cs:5-43` | Implementado (no usa Animator; los `AnimatorController` de `Resources` no se usan en runtime) | ESTÁTICO |
| Audio (música + 7 SFX) | `CGAudio.cs` | Implementado; `Click.wav` sin uso | ESTÁTICO |
| Controles táctiles Android | `CGTouch.cs` | Parcial: sin Correr ni Habilidad; HUD muestra «E / Y … ESC / START» en móvil | ESTÁTICO; NO VERIFICADO en dispositivo |
| Build Windows x64 | `CGBuildScenes.cs:131-147` | Herramienta presente; ver §10 | — |
| Build Android (IL2CPP, ARM64, APK) | `CGAndroidBuild.cs` | **No verificable**: módulo Android ausente en el equipo | ESTÁTICO |
| Créditos / ayuda | `CGGame.cs:222-232` | Implementado | ESTÁTICO |

---

## 3. Errores confirmados

> Rutas relativas a la raíz del repo en `develop` @ `9257b4e`. Los resultados de ejecución están en §10.

### E1 — «CANCELAR» en *¿Borrar progreso?* borra el progreso (teclado/mando) · **P0**
- **Archivo/línea**: `Assets/CampusGlitch/Scripts/CGGame.cs:51` → `case Page.ConfirmReset:CGStore.Reset();ToMain();`
- **Causa**: el `switch` de `Confirm()` ignora `selection`; cualquier Enter/Espacio/A en esa página ejecuta `Reset()`.
- **Reproducción**: Menú → «BORRAR PROGRESO» → flecha abajo hasta «CANCELAR» (resaltado) → Enter → todo el progreso queda a cero y se guarda en disco.
- Mismo patrón: `case Page.Victory:AfterVictory();` (E2) y `case Page.Ending:ToMain();` (con teclado no se puede elegir «VER CRÉDITOS»).

### E2 — «MENÚ PRINCIPAL» en la pantalla de Victoria carga el siguiente nivel (teclado/mando) · **P1**
- **Archivo/línea**: `CGGame.cs:51`, `CGGame.cs:108`.
- **Reproducción**: completar un nivel → bajar a «MENÚ PRINCIPAL» → Enter → se carga el nivel siguiente.

### E3 — La terminal (quiz) no se puede responder con teclado ni mando · **P0**
- **Archivo/línea**: `CGGame.cs:44` (`if(page==Page.Quiz){if(CGControl.Back())…;return;}`) — retorna antes de procesar Arriba/Abajo/Confirmar. Las respuestas solo existen como `GUI.Button` (`CGGame.cs:237`).
- **Impacto**: con mando es imposible completar cualquier nivel; en Windows obliga a usar el ratón. En móvil no hay botón para cerrar la terminal (`CGGame.cs:238` solo menciona «ESC / B»).

### E4 — No existe Game Over ni pérdida real de vidas · **P0**
- **Archivo/línea**: `CGGame.cs:91-92` (`KillHero` → `Respawn` → `Hero.hearts=3`); `CGEntities.cs:76`; también `CGGame.cs:85` («REINICIAR DESDE CHECKPOINT» repone 3 corazones).
- **Reproducción**: recibir 3 golpes → el héroe reaparece en el checkpoint con 3 corazones, indefinidamente. No hay contador de intentos ni pantalla de derrota.
- **Contexto Git**: `GameOver.unity` y `GameManager.cs` (Marcelo, PR #4) fueron eliminados en `8c48d0a`.

### E5 — Las plataformas elevadas son inalcanzables con el salto · **P0** (ver medición §10)
- **Archivos/líneas**: salto `CGEntities.cs:61` (`vy=9.6`), gravedad `CGEntities.cs:49` (`gravityScale=3.2`) con `Physics2DSettings` `g=-9.81` ⇒ altura teórica `9.6² / (2·31.39) ≈ 1.47 u`. Plataformas: `CGWorld.cs:35` (`y = 1.7 + (x%3)·0.17`, alto 0.32 ⇒ tope 1.86–2.20 u), `CGWorld.cs:36` (tope ≈ 3.26 u), `CGWorld.cs:38,40` (≈ 4.1–4.6 u).
- **Impacto**: todo el diseño de «rutas elevadas» es decorativo; el nivel es un pasillo plano.

### E6 — El fragmento 2 no se puede recoger en el Nivel 1 ni en el Nivel 8: **el juego no se puede completar** · **P0 crítico** [VERIFICADO-EJECUCIÓN]
- **Causa**: el fragmento 2 está en `(31, 3.35)` (`CGWorld.cs:48`) y solo se alcanza saltando justo debajo (la cabeza del héroe llega a ≈ 3.0 u y el trigger baja hasta ≈ 2.88 u). Dos plataformas elevadas inalcanzables (E5) **tapan** al héroe (1.64 u de alto):
  - **Nivel 1** (`index 0`): plataforma en `x=32` (`CGWorld.cs:24`), que cubre 30.7–33.3 con la base a ≈ 1.88 u.
  - **Nivel 8** (`index 7`): plataforma en `x=31` (`CGWorld.cs:31`), que cubre 29.7–32.3 con la base a ≈ 1.71 u.
- **Medición**: barrido automático entre x 29,6 y 32,6, en pasos de 0,05 u, saltando parado con la velocidad real (`vy=9.6`). Nivel 1 → **ninguna** posición válida. Nivel 8 → **ninguna**. Control positivo, Nivel 2 → recogido en x=30,70.
- **Impacto**: sin 3 fragmentos el portal no abre (`CGGame.cs:98`). Al no poder superarse el Nivel 1, no se desbloquea ningún nivel jugando, ni el Nivel 9 ni el final.
- **Reproducción manual**: abrir `SC_Level01` → Play → caminar hasta x≈31 → saltar bajo el fragmento flotante desde cualquier posición: la cabeza choca con la plataforma y `DATOS` no pasa de 0/3 en ese fragmento.
- **Límite de la prueba**: solo se midió el salto parado. Por geometría, un salto en carrera queda frenado por el borde de la plataforma; conviene confirmarlo a mano (T07).

### E7 — `UNIFRANZ_Sign` no existe en ningún nivel · **P2**
- `CGWorld.cs:46` pide `CG/01_Nuevos_Modulares/<nivel>/UNIFRANZ_Sign`; el recurso no existe en ninguna de las 10 carpetas (verificado con script). `Prop()` retorna en silencio (`CGWorld.cs:121`).

### E8 — El volumen puede superar el 100 % · **P2**
- `CGGame.cs:87`: `Mathf.Repeat(v+0.1, 1.1f)` ⇒ 0.85 → 0.95 → **1.05** (la UI muestra «105 %»); no hay forma de bajar salvo dar la vuelta.

### E9 — Tras ganar el nivel EXTRA, «SIGUIENTE NIVEL» carga el Nivel 1 · **P2**
- `CGGame.cs:108`: `int next = sceneLevel==9 ? 0 : …`.

### E10 — Controles táctiles incompletos · **P1**
- `CGData.cs:75` (`HoldRun`) y `CGData.cs:79` (`Ability`) no consultan `CGTouch`; en Android no se puede correr ni usar la habilidad morada.
- `CGGame.cs:257`: el HUD en móvil muestra «E / Y INTERACTUAR · ESC / START PAUSA».

### E11 — `Build_Windows/` no está ignorado en `develop` · **P1**
- `.gitignore` ignora `/Build/` y `/Builds/`, pero `CGBuildScenes.BuildWindows` escribe en `Build_Windows/` (`CGBuildScenes.cs:135`). En el repositorio `CAMPUS_GLITCH` (remoto `juego`) ya se subieron ~90 MB de binarios (`UnityPlayer.dll`, `resources.assets.resS`, …) en `7c69f1c`. Lo corrige el PR #17 (abierto).

### E12 — Opción «Subtítulos» sin efecto · **P2**
- `CGStore.Data.subtitles` solo se escribe (`CGGame.cs:55,215`); ningún código lo lee.

### E13 — `.meta` huérfanos de carpetas vacías · **P2**
- Al importar, Unity avisa: `Assets/Art/Backgrounds`, `Assets/Art/Characters`, `Assets/Art/UI`, `Assets/Materials`, `Assets/Prefabs` tienen `.meta` pero no carpeta (ver log §10). Genera cambios espurios en cada clon.

---

## 4. Riesgos potenciales (no confirmados como fallo)

| ID | Riesgo | Dónde | Cómo confirmarlo |
|---|---|---|---|
| R1 | **IMGUI en cada frame**: `Styles()` crea 4 `GUIStyle` y hace `Resources.Load` por cada evento `OnGUI` (hasta 10 cargas por evento en el mapa) → GC y caídas de FPS en móvil | `CGGame.cs:140,152,196` | Profiler en dispositivo Android |
| R2 | En PC la UI se **estira sin conservar proporción** en 16:10, 4:3 o ultrawide (solo en móvil se usa escala uniforme) | `CGGame.cs:141-147` | Build Windows en ventana 1280×1024 |
| R3 | **Orientación** comprometida en `ProjectSettings` = AutoRotation con *portrait* permitido; solo se corrige si alguien ejecuta el menú «Configurar Android» antes de compilar | `ProjectSettings.asset:11,62-65` | Build Android sin ejecutar `Configure()` |
| R4 | `applicationIdentifier` Android no está comprometido; `productName=CAMPUS_GLITCH_TEMP`, `companyName=DefaultCompany`, Standalone `com.DefaultCompany.2D-URP` | `ProjectSettings.asset:15-16,169-170` | Revisar Player Settings |
| R5 | **Herramientas de editor destructivas**: «Reparar escenas» reabre y **sobrescribe las 18 escenas** y Build Settings, y `CGAnimationBuilder` **borra y recrea** `AnimatorController` (GUID nuevos). `BuildWindows` llama a `GenerateAll()` en silencio si hay < 18 escenas | `CGBuildScenes.cs:20-47,132`; `CGAnimationBuilder.cs:39,45,62` | Revisar diff tras ejecutarlas (no lo hice) |
| R6 | `CGWelcome2D` es `[InitializeOnLoad]` y abre `SC_Bootstrap` al cargar el editor; «Diagnosticar SIN modificar escenas» sí escribe `Docs/CAMARA_2D_DIAGNOSTICO.txt` | `CGWelcome2D.cs:7-18`; `CGCameraRepairTool.cs:190` | — |
| R7 | **Tamaño del build**: todo `Resources/CG` entra al build, incluidos 625 sprites de biblioteca y 23 controladores no usados, y todas las texturas se fuerzan a *Uncompressed* | `CGBuildScenes.cs:150-160` (`CGPixelImporter`) | Reporte de build (`Editor.log` → *Build Report*) |
| R8 | Paquete experimental `com.unity.pipeline 0.8.0-exp.1` (arrastra `test-framework 1.1.33`, `mono-cecil`) sin uso aparente; también `collab-proxy`, `multiplayer.center`, `visualscripting` | `Packages/manifest.json` | Quitar en rama y recompilar |
| R9 | Remoto local `juego` con historia **no relacionada** y binarios pesados; un `push` accidental de esas ramas a `origin` inflaría el repo evaluado. Además contiene `e13d459` (Marcelo, 22-oct, «MEJORAS EN LA CONFIGURACIÓN PORT WINDOWS») que **no está** en el repo evaluado | `git remote -v` | — |
| R10 | `CGStore.Load` no recupera desde `.bak` si el JSON se corrompe → pérdida total silenciosa | `CGData.cs:28` | Corromper el JSON a mano |
| R11 | La vista previa `CG_EDITOR_PREVIEW_2D` (con colliders) se destruye con `Destroy` diferido en `Start`; durante el primer paso de física coexiste con el nivel generado | `CGGame.cs:22-23` | Observar primer frame |
| R12 | Interacciones por **coordenadas X fijas** duplicadas en dos clases (8, 35, 40, 69); mover un objeto en `CGWorld` rompe la interacción sin error | `CGGame.cs:95-98` vs `CGWorld.cs:49,54-56` | — |
| R13 | El jefe puede derrotarse desde fuera de su radio de daño (alcance de mordida ≈ 2.6 u vs radio de daño 1.8 u) → jefes triviales | `CGGame.cs:89`, `CGEntities.cs:94` | Prueba manual de nivel 6 |
| R14 | Sin `.gitattributes` (UnityYAMLMerge / LFS); `.aseprite`, `.png`, `.wav` se versionan como blobs normales (pack local 82 MB) | raíz | — |

---

## 5. Problemas de arquitectura y organización

1. **Clase dios**: `CGGame` (261 líneas, muchas con 200+ caracteres) mezcla máquina de estados de UI, navegación de escenas, reglas de misión, HUD, guardado, audio y quiz. Estilo «minificado» (varias sentencias por línea) que dificulta revisión, *diff* y atribución en Git.
2. **Doble fuente de verdad del nivel**: posiciones de NPC/checkpoint/terminal/portal se escriben en `CGWorld.Setup` y se vuelven a escribir en `CGGame.InteractNearPlayer`. Existe `CGMarker` con `kind`, pero no se usa para interactuar.
3. **Escenas «falsas»**: las 10 escenas de nivel pesan ~700 KB cada una con una vista previa que se destruye en `Start`; el nivel real se genera por código. Editar la escena en Unity no cambia el juego (riesgo de confusión para el equipo y el docente).
4. **Todo en `Resources/`** con rutas en *strings* (`CGArt`, `CGWorld`, `CGAudio`): errores tipográficos fallan en silencio (ej. E7). Sin `ScriptableObject` de configuración de niveles.
5. **Input System sin Actions**: `InputSystem_Actions.inputactions` no se usa; `CGControl` consulta dispositivos directamente y la reasignación usa *strings* de `Key`.
6. **UI en IMGUI** (no uGUI/UI Toolkit): sin escalado por *anchors*, sin navegación accesible estándar, coste por frame alto.
7. **Sin asmdef ni tests** en el repo; las herramientas «validador»/«auditor» solo comprueban recursos.
8. **Árbol de assets caótico**: 5 copias de la biblioteca de 625 sprites (`Assets/Art/Tiles`, `CampusGlitch/Art/SourceAseprite`, `CampusGlitch/ArtSource/Aseprite`, `CampusGlitch/Resources/CG/00_Biblioteca_Previa_625`, `CAMPUS_GLITCH_ASSETS_2D_COMPLETO/…`). 2 813 archivos `.aseprite` dentro de `Assets/` alargan cada importación.
9. **Escenas huérfanas** fuera del build: `Assets/FASE.unity`, `Assets/Scenes/Nivel01.unity`.
10. **Documentación desalineada**: README (3 escenas), `Informe_Exportacion_Android.md` (cita la rama inexistente `feature/evaluacion-dia2-builds`, resultados pendientes), créditos del juego vs. evidencia de commits.

---

## 6. Mejoras recomendadas

### P0 — bloquean la entrega
1. **E1/E2/E3**: respetar `selection` en `ConfirmReset`, `Victory`, `Ending` y habilitar navegación/confirmación en `Quiz` (+ botón «CERRAR» táctil).
2. **E4**: definir con el docente la regla de derrota e implementar vidas + Game Over (reintentar / menú), integrado en `CGGame` (página `GameOver`) y no como escena aislada.
3. **E6 primero, luego E5**: el juego no se puede terminar mientras el fragmento 2 de N1/N8 siga tapado. Ajustar el salto (`vy≈11.5` ⇒ ≈ 2.1 u) o bajar las plataformas (tope ≤ 1.3 u), y/o generar las plataformas evitando la columna x∈[29.5, 32.5]. Añadir una prueba automática de alcanzabilidad de los 3 fragmentos en los 10 niveles.
4. **Trazabilidad**: desde ahora, una rama + Issue + PR pequeño por cambio, con **revisión de otro integrante** obligatoria y protección de ramas en `main`/`develop`.
5. **Android**: instalar módulo Android, generar APK reproducible, completar informe con evidencias reales (o declarar explícitamente que no se entrega Android).
6. **Fusionar PR #17** (ignora `Build_Windows/`, plantilla PR y protocolo) tras revisión de otro integrante.

### P1 — calidad visible
7. Botones táctiles Correr/Habilidad; HUD contextual por plataforma (E10).
8. README nuevo basado en el estado real y esta matriz de pruebas.
9. Mover la biblioteca de 625 sprites fuera de `Resources/` (tras demostrar que no se usa), quitar `Uncompressed` forzado, eliminar duplicados con inventario GUID (Issue #15).
10. Jefes con al menos un patrón de ataque real (proyectil/embestida) y distancia de daño coherente (R13).
11. Escalado IMGUI uniforme también en PC (R2) y caché de `GUIStyle`/texturas (R1).
12. Comprometer Player Settings correctos (nombre, compañía, ID Android, orientación horizontal).
13. Integrar o descartar formalmente `e13d459` (Marcelo, repo `CAMPUS_GLITCH`) vía PR para que su trabajo conste.

### P2 — deuda técnica
14. Separar `CGGame` en `UIRouter`, `MissionState`, `HUD`; reemplazar X fijas por triggers/`CGMarker`.
15. Volumen con límites 0–1 y −/+; implementar o retirar «Subtítulos»; recuperar `.bak`.
16. `UNIFRANZ_Sign`, `.meta` huérfanos, escenas huérfanas, paquetes sin uso, `.gitattributes`.
17. asmdef + tests EditMode/PlayMode en el repo (pueden partir de los de esta auditoría) y CI.

---

## 7. Propuesta de reparto técnico

Alineada con los Issues #9–#16 y con los créditos del juego, añadiendo los hallazgos nuevos. Cada persona trabaja en **sus** archivos para evitar conflictos de YAML/merge.

| Integrante | Responsabilidad | Archivos principales | Issues | Nuevos hallazgos asignados |
|---|---|---|---|---|
| **Vicente** (integración) | Línea base, Gitflow, protección de ramas, Android, revisión de PR | `CGTouch.cs`, `CGAndroidBuild.cs`, `ProjectSettings`, `.gitignore`, `.github/`, `README.md` | #9, #10 | E10, E11, R3, R4, R8, R9, P0-4/5/6 |
| **Carla** (UI/animación/audio) | Navegación de menús, quiz, ajustes, feedback | `CGGame.cs` (bloques `Update` de menús y `OnGUI`), `CGAudio.cs`, `CGEntities.cs` (`CGFrameAnimator`) | #11, #12 | E1, E2, E3, E8, E12, R1, R2 |
| **Marcelo** (enemigos/derrota) | Vidas, Game Over, checkpoints, jefes | `CGEntities.cs` (`CGHero.Damage`, `CGEnemy`), `CGGame.cs` (`KillHero`, `Respawn`, nueva página GameOver) | #13, #14 | E4, E9, R13, integrar `e13d459` |
| **Víctor** (niveles/assets) | Alcanzabilidad, colisiones, arte, limpieza, build Windows | `CGWorld.cs`, `Resources/CG`, importadores, escenas | #15, #16 | E5, E6, E7, E13, R5, R7, R11 |

> Coordinación obligatoria: `CGGame.cs` lo tocan Carla y Marcelo → acordar orden de PR o extraer primero la página GameOver a un archivo nuevo (`CGGameOver.cs`) para no pisarse.

---

## 8. Plan incremental diario (Gitflow)

**Reglas**: ramas `feature/<nombre>/<issue>-<tema>` desde `develop`; commits pequeños con Conventional Commits que citen el Issue (`fix(menu): respetar CANCELAR en borrar progreso (#11)`); PR a `develop` con al menos **1 revisión de otro integrante**; nunca fusionar el propio PR; `release/x.y` desde `develop` → `main` + tag; `hotfix/` desde `main`. Prohibido `push --force` y rebase de ramas compartidas.

| Día | Vicente | Carla | Marcelo | Víctor | Integración del día |
|---|---|---|---|---|---|
| 1 | Revisar y fusionar PR #17 (revisa Carla); activar protección de ramas; README mínimo con estado real | Rama `fix/carla/11-confirm-selection`: E1+E2 | Rama `feature/marcelo/13-reglas-derrota`: acordar regla con docente; esqueleto `CGGameOver` | Rama `fix/victor/16-alcance-salto`: medir y ajustar E5 | PR cruzados: Vicente↔Carla, Marcelo↔Víctor |
| 2 | Instalar módulo Android; primer APK; informe con log real | E3 quiz teclado/mando + botón cerrar táctil | Vidas + Game Over funcional + reintento | E6 fragmento 2 + prueba de alcanzabilidad | Smoke manual en Windows de los PR del día |
| 3 | E10 botones Correr/Habilidad; HUD por plataforma | E8 volumen, E12 subtítulos, R1 caché IMGUI | E9, R13 patrón de jefe | E7, E13, inventario de duplicados (sin borrar) | `release/1.1` candidata |
| 4 | Prueba APK en dispositivo físico + capturas (`ANDROID_CAPTURA_TELEFONO.bat`) | R2 escalado PC + revisión de animaciones | Integrar/descartar `e13d459` vía PR | Mover biblioteca fuera de `Resources/` (tras inventario aprobado) + build Windows | Matriz §9 completa en `Docs/QA_<fecha>.md` |
| 5 | `release/1.1` → `main`, tag `v1.1`, notas de versión | QA cruzado de Víctor | QA cruzado de Carla | QA cruzado de Marcelo | Entrega |

Evidencia diaria por persona: ≥ 1 commit propio con Issue, 1 revisión hecha a otro, entrada en `Docs/BITACORA.md` (fecha, SHA, qué se probó).

---

## 9. Matriz de pruebas y criterios de aceptación

| ID | Caso | Plataforma | Pasos | Criterio de aceptación | Estado actual |
|---|---|---|---|---|---|
| T01 | Carga de las 18 escenas | Editor | PlayMode `Smoke_SceneLoads` | Sin errores en consola; CGGame presente; héroe aterriza | **Pasa** (18/18) |
| T02 | Splash → Menú | Win/Android | Enter / toque | Llega a Menú | No ejecutado manual |
| T03 | Cancelar borrar progreso | Win (teclado y mando) | Menú → Borrar → CANCELAR → Enter | Progreso intacto | **Falla (E1)** |
| T04 | Victoria → Menú principal | Win | Completar nivel → MENÚ PRINCIPAL → Enter | Carga `SC_MainMenu` | **Falla (E2)** |
| T05 | Terminal con teclado/mando | Win | E en x≈40 → flechas → Enter | Se resuelve | **Falla (E3)** |
| T06 | Salto a plataforma elevada | Win | Saltar bajo cualquier plataforma | El héroe sube a la plataforma | **Falla (E5)** |
| T07 | Fragmento 2 recolectable | Todos los niveles | Saltar bajo x=31 (parado y en carrera) | `DATOS` sube | **Falla en N1 y N8 (E6)**; pasa en el resto |
| T08 | Completar los 10 niveles (lógica) | Editor | `Logic_CompleteLevel` | Victoria/Final; desbloqueos correctos | **Pasa** (10/10, solo lógica) |
| T09 | Derrota con 0 vidas | Win | Recibir 3 golpes | Pantalla Game Over con Reintentar/Menú | **Falla (E4)** |
| T10 | Caída al vacío | Win | Caer en x=20 | Reaparece en checkpoint | **Pasa** |
| T11 | Guardado persistente | Win/Android | Completar N1, cerrar app, abrir | N2 desbloqueado | **Pasa** ida y vuelta en disco; cierre real de app no probado |
| T12 | Pausa | Win/Android | Esc / botón PAUSA | Juego congelado; Continuar reanuda | **Pasa** con Esc (táctil no probado) |
| T13 | Táctil: mover+saltar simultáneo | Android | Mantener DER + tocar SALTAR | Salto en movimiento | NO VERIFICADO (sin APK) |
| T14 | Táctil: correr / habilidad | Android | — | Botones existen y funcionan | **Falla (E10)** |
| T15 | Orientación | Android | Girar teléfono | Solo horizontal | NO VERIFICADO |
| T16 | Safe area / notch | Android 20:9 | Ver botones | Nada recortado | NO VERIFICADO |
| T17 | Build Windows x64 | Win | `Campus Glitch > Crear build Windows x64` | `.exe` arranca y llega al menú | §10 |
| T18 | Build Android APK | Android | `Campus Glitch > Android > 2` | APK instalado y jugable | **Bloqueado**: módulo ausente |
| T19 | Volumen | Win | Ajustes → Volumen ×2 | Máximo 100 % | **Falla (E8)** |
| T20 | Resolución no 16:9 | Win | Ventana 1280×1024 | UI sin deformar | NO VERIFICADO (R2) |
| T21 | Rendimiento móvil | Android gama media | Profiler 2 min en nivel 5 | ≥ 30 FPS estables, GC < 1 KB/frame | NO VERIFICADO (R1) |

Prueba manual guiada (para T13–T16, T21): 1) instalar APK con `Tools\ANDROID_INSTALAR_APK.bat`; 2) en cada caso capturar con `Tools\ANDROID_CAPTURA_TELEFONO.bat T13_mover_saltar`; 3) anotar modelo, versión Android, resultado y SHA del build en el informe.

---

## 10. Resultados realmente ejecutados, comandos, logs y limitaciones

### 10.1 Entorno
- Windows 11, Unity `6000.3.16f1` en `D:\Unity\Editors\6000.3.16f1`. `PlaybackEngines` contiene **solo** `windowsstandalonesupport`: falta Android.
- `gh` no está instalado. PR, Issues y reseñas se consultaron con la API pública REST de GitHub (sin autenticar), por lo que **no se pudo comprobar la protección de ramas**.
- Copia de trabajo: `git archive origin/develop` (9257b4e) + `Library` copiada del proyecto, en **`D:\CGAUDIT`**. En esa copia se cambió `productName` a `CG_AUDIT_COPY` para aislar la ruta de guardado. El repositorio no se modificó.

### 10.2 Comandos ejecutados (resumen)
```bash
git status; git branch -a -vv; git fetch --all --prune      # solo actualiza refs remotas
git log --all --format='%h|%ad|%an|%ae|%s' --date=iso
git diff --name-status 4865a1d 8c48d0a                       # 2 357 borrados, 5 868 altas
git shortlog -sne --no-merges origin/develop
curl https://api.github.com/repos/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/{pulls,issues,pulls/N/reviews,commits/SHA}
python -I scan.py develop   # GUID/scripts por escena
python -I res.py develop    # rutas Resources.Load del código vs disco
md5sum (png/aseprite)       # duplicados exactos
Unity.exe -batchmode -nographics -projectPath D:/CGAUDIT -runTests -testPlatform PlayMode -testResults D:/CGAUDIT_results2.xml -logFile D:/CGAUDIT_playmode2.log
Unity.exe ... -runTests -testFilter Sweep_Chip2 -testResults D:/CGAUDIT_results4.xml -logFile D:/CGAUDIT_playmode4.log
Unity.exe -batchmode -nographics -projectPath D:/CGAUDIT -executeMethod CampusGlitch.Editor.CGBuildScenes.BuildWindows -quit -logFile D:/CGAUDIT_build_win.log
CampusGlitch.exe -batchmode -nographics -logFile D:/CGAUDIT_player.log   # 25 s y cierre
```

### 10.3 Pruebas PlayMode (`D:\CGAUDIT\Assets\AuditTests\CGAuditPlayModeTests.cs`, solo en la copia)
Ejecución final: **67 pruebas, 65 superadas y 2 fallidas**, en 105 s. Las pruebas `BUG_*` pasan cuando el defecto **sigue presente** (son pruebas de caracterización).

| Prueba | Resultado | Salida `[AUDIT]` |
|---|---|---|
| `Harness_SimulatedKeyboardWorks` | Pasa | `Keyboard.current==kb True enter pressed=True` (valida el arnés) |
| `Smoke_SceneLoads` ×18 | 18/18 pasan | Sin errores; CGGame presente; vista previa destruida; héroe aterriza en las 10 escenas jugables |
| `Logic_CompleteLevel` ×10 | 10/10 pasan | Victoria (Final en N9); desbloqueo morado tras N2 y extra tras N5 |
| `Measure_JumpHeightVsRaisedPlatforms` ×10 (tecla Space) | 10/10 | `salto=1,37 u`; tope de la plataforma más baja 1,86 u (2,03 u en el extra) → **E5** |
| `Measure_JumpPhysicsDirect` ×10 | 10/10 | `salto físico=1,37 u` (coincide con el salto por tecla) |
| `Measure_Chip2ReachableByJump` ×10 | **8 pasan, 2 fallan** | N1 y N8: `alcanzable=False` → **E6** |
| `Sweep_Chip2` (N1, N2, N8) | Medición | N1 `ninguna` · N2 `30,70` (control) · N8 `ninguna` |
| `BUG_ConfirmReset_CancelWithKeyboardErasesProgress` | Pasa = bug | `latestUnlocked=0` → **E1** |
| `BUG_Victory_MainMenuOptionWithKeyboardLoadsNextLevel` | Pasa = bug | `SC_Level02` → **E2** |
| `BUG_Quiz_CannotBeAnsweredWithKeyboard` | Pasa = bug | `page=Quiz terminalSolved=False` → **E3** |
| `BUG_NoGameOver_OnZeroHearts` | Pasa = bug | `page=Playing hearts=3` → **E4** |
| `BUG_VolumeCanExceed100` | Pasa = bug | `1,05` → **E8** |
| `Fall_RespawnsAtCheckpoint` | Pasa | `pos=(2.50, 0.00)` |
| `Save_RoundTrip` | Pasa | `…/LocalLow/DefaultCompany/CG_AUDIT_COPY` |
| `Pause_EscapeTogglesPause` | Pasa | — |

**Incidencias del arnés, resueltas y declaradas:**
1. La primera ejecución desde `%TEMP%` abortó con `Could not read file Packages/com.unity.2d.animation/Editor/…asmdef` porque la ruta superaba MAX_PATH; se trasladó la copia a `D:\CGAUDIT`.
2. En la segunda ejecución todas las pruebas con teclado simulado dieron falsos negativos: en modo batch, sin foco, el Input System descarta la entrada. Se corrigió con `BackgroundBehavior.IgnoreFocus` y se añadió `Harness_SimulatedKeyboardWorks` como control.
3. Había dos errores propios del test, corregidos: `SC_LevelMap` coincidía con el prefijo `SC_Level` y la caída se lanzaba en x=20, sobre el borde del hueco.

### 10.4 Build Windows x64
- `CGBuildScenes.BuildWindows` → **`Build Finished, Result: Success` · errores 0**, salida en `D:\CGAUDIT\Build_Windows\CampusGlitch.exe`.
- Tamaño completo **134,1 MB**. Assets de usuario: 42,6 MB, de los que las **texturas suman 39,1 MB (91,9 %)**. El Build Report lista **625 entradas de `Resources/CG/00_Biblioteca_Previa_625`** y 38 de `AnimationControllers`/`03_Interactivos_Animados`, ninguna usada por el código (R7).
- El ejecutable se lanzó en modo headless (`-batchmode -nographics`): siguió vivo 25 s, inicializó Input System y PhysX y no registró excepciones (24 líneas de log).
- **No verificado**: el render real, la resolución, la navegación con ratón y mando ni la jugabilidad del `.exe`. Quedan como prueba manual (T17, T20).

### 10.5 Limitaciones
- **Android**: no se pudo generar el APK (falta el módulo) ni probar en dispositivo. T13–T16, T18 y T21 quedan sin verificar.
- **Táctil** (`CGTouch`), **IMGUI/ratón** y **mando**: no se ejecutaron. Con `-nographics` no se invoca `OnGUI`; el análisis de esas rutas es estático.
- No se evaluaron el render, la iluminación ni la legibilidad visual, que requieren revisión humana en pantalla.
- La protección de ramas y la configuración del repo en GitHub no se pudieron consultar sin autenticación.
- No se ejecutaron «Reparar escenas», `CGCameraRepairTool` ni `CGAnimationBuilder`, porque sobrescriben escenas o assets.
- El repositorio local no se modificó, salvo dos archivos nuevos sin seguimiento: `Docs/AUDITORIA_CLAUDE_PENDIENTE.md` (pedido por el usuario) y este informe.
