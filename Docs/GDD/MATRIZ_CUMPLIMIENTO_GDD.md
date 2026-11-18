# Matriz de cumplimiento del GDD — Campus Glitch

**Documento de referencia (no editado):** `Docs/GDD/Guia_JARVIS_v1.0_PRESENTADA.xlsx`, hoja *Preguntas*. La columna **Ref.** indica el número de pregunta (`P#`) y la celda de la respuesta (`C#`).
**Corte:** 2026-11-06, `develop` @ `4b729e3`. El código de juego no ha cambiado desde `9257b4e` (auditoría del 5–6/11).
**Fuentes de estado:** `Docs/AUDITORIA_CLAUDE_INFORME.md` (§2 inventario, §3 errores, §10 ejecución) y lectura estática del código en `develop`.
**Propuestas de ajuste:** `Docs/GDD/PROPUESTAS_AJUSTE_v1.1.md`. Están **pendientes de aprobación docente** y no cambian el compromiso original.

## Leyenda

| Estado | Significado |
|---|---|
| ✅ Cumple · verificado | Comprobado ejecutando el juego o pruebas PlayMode |
| 🟢 Cumple · implementado | Presente en el código; falta una prueba ejecutada |
| 🟡 Parcial | Cumple solo en parte o con defectos conocidos |
| ❌ No cumple | Ausente, o un defecto confirmado impide cumplirlo |
| ➕ Excede | Hay más de lo prometido (ampliación, no reemplaza al MVP) |
| ⏳ Pendiente | Requiere trabajo o prueba fuera del código (p. ej., dispositivo o personas reales) |

**Prioridad:** **MVP** = obligatorio según P10 (C17), P59 (C76) y P112 (C140); **Opc.** = el propio GDD lo declara opcional.

### Tipo de incumplimiento

El estado (columna **Estado**) se acompaña, cuando corresponde, de una marca que distingue la **naturaleza** de la desviación:

| Marca | Tipo | Efecto sobre el MVP |
|---|---|---|
| 🛑 | **Defecto que impide cumplir el MVP**: el juego no permite hacer lo comprometido (bug confirmado o función ausente) | Bloquea la entrega mientras no se corrija |
| 🔀 | **Desviación del orden de desarrollo o del proceso**: el GDD pedía hacer algo después o de otra forma, pero el resultado jugable no queda impedido por ello | No bloquea por sí sola; debe explicarse al docente |
| ⚖️ | **Pendiente de decisión docente**: existe una propuesta en `PROPUESTAS_AJUSTE_v1.1.md`, **no aprobada** | Mientras no se apruebe, rige el **compromiso original** y el estado se mantiene |

## 1. Alcance del MVP (P10 · C17)

| Ref. | Compromiso | Prior. | Estado | Evidencia / referencia en código | Issue |
|---|---|---|---|---|---|
| P10 C17 · P59 C76 | 2 niveles cortos **completamente funcionales** | MVP | ❌ 🛑 | N1 no se puede completar: el fragmento 2 es inalcanzable (E6, prueba `Measure_Chip2ReachableByJump` y barrido `Sweep_Chip2` fallan en N1). La lógica de N2 está ✅ y el fragmento 2 de N2 se alcanza (control del barrido) | #18 |
| P10 C17 | Duración aprox. 5–7 min por nivel | MVP | ⏳ | No medido. Medir en QA manual | #16 |
| P10 C17 · P49 C65 | 1 personaje jugable (lobo) | MVP | 🟢 | `CGEntities.cs` (`CGHero`). La personalización (rojo/morado, 2 modelos) es ➕ | — |
| P10 C17 · P24 C33 | Movimiento, salto y mordida | MVP | 🟡 | Movimiento y carrera ✅; salto ✅ medido 1,37 u pero **no alcanza** plataformas elevadas desde 1,86 u (E5); mordida 🟢 | #19, #14 |
| P10 C17 · P66 C84 | Vida/daño básico | MVP | ❌ 🛑 | Al llegar a 0 corazones se restauran 3; no hay derrota (E4, `BUG_NoGameOver_OnZeroHearts`) | #13 |
| P10 C17 · P58 C74 | 1–2 tipos de enemigos | MVP | ➕ | 3 tipos (`Bug_Terrestre`, `Bug_Saltador`, `Centinela`) + 3 jefes en N6/N8/N9. Comportamiento sencillo, sin física | #14 |
| P10 C17 · P26 C35 | Interacción con información universitaria | MVP | 🟢 | NPC con diálogo y objetivo por nivel (`CGData.cs`, `CGLevels`). Falta validar el contenido institucional | #11 |
| P10 C17 · P35 C47 | ≥1 checkpoint por nivel | MVP | 🟡 ⚖️ | Checkpoint en x≈35 en cada nivel; reaparición por caída ✅. **Activación manual con E**, no automática (ver C48) | #13 |
| P10 C17 · P63 C80 | Desafío final sencillo por nivel | MVP | ❌ 🛑 | El quiz de la terminal **solo** responde a ratón o toque (E3, `BUG_Quiz_CannotBeAnsweredWithKeyboard`); el teclado es el dispositivo obligatorio (C99) | #21 |
| P10 C17 · P66 C84 | HUD básico | MVP | 🟡 | Vidas y fragmentos en HUD IMGUI 🟢; en móvil muestra textos de teclado | #10 |
| P10 C17 · P68 C87 | Menú principal, pausa | MVP | 🟡 | Pausa con Esc ✅. Menú 🟢 con defectos E1 (cancelar borra) y E2 (victoria ignora la opción) | #20, #22, #11 |
| P10 C17 · P79 C101 | Audio esencial | MVP | 🟢 | `CGAudio.cs`: música + 7 SFX | #12 |
| P10 C17 | Tercer nivel y extras solo tras terminar el MVP | MVP | ➕ 🔀 ⚖️ | Existen 9 niveles + extra **antes** de cerrar el MVP. Es una desviación del **orden** de desarrollo, no un defecto: los niveles extra no impiden jugar N1/N2; lo que lo impide son los 🛑 de esta tabla. Propuesta de clasificarlos como ampliación (C17/C76), pendiente | — |

## 2. Jugabilidad, mecánicas y modos

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P1 C7 · P2 C8 · P12 C20 | Plataformas 2D side-scrolling, vista lateral | MVP | ✅ | Escenas 2D cargan (Smoke ×18); mundo lateral generado por `CGWorld.cs` | — |
| P4 C10 · P14 C22 · P24 C33 | Agacharse | MVP | ❌ ⚖️ | No hay acción de agacharse. Sigue siendo compromiso del MVP: la propuesta de tratarla como deseable (C10/C22/C33) **no está aprobada** | — |
| P13 C21 | Desplazamiento horizontal con tramos verticales | MVP | 🟡 | Rutas elevadas inalcanzables con el salto actual (E5) | #19 |
| P15–17 C23–C25 · P25 C34 | Sin mejoras de capacidades en el MVP | MVP | 🟢 | Acción «Habilidad» (Q) existente; revisar que no sea requisito para avanzar | #14 |
| P18 C26 | Recuperar fragmentos y resolver el desafío para restaurar la zona | MVP | 🟡 | Lógica 3 fragmentos + terminal + portal ✅ (`Logic_CompleteLevel` ×10); bloqueada físicamente en N1/N8 (E6) y por el quiz (E3) | #18, #21 |
| P19–22 C28–C31 | Física arcade 2D (Rigidbody2D/Collider2D, gravedad) | MVP | ✅ | Gravedad y salto medidos en PlayMode (`Measure_JumpPhysicsDirect`) | — |
| P22 C31 | Retroceso al recibir daño | MVP | 🟢 | `CGHero.Damage` (`CGEntities.cs`). Sin prueba ejecutada | #13 |
| P26 C35 | Coleccionables por contacto; interacción con acción | MVP | 🟡 | Contacto ✅ por lógica; interacción por posiciones X fijas (8/35/40/69) | #18 |
| P28 C38 | Campaña de 1 jugador con 2 niveles, menú y pausa | MVP | 🟡 | Campaña ✅ en lógica; 2 niveles no completables (ver C17) | #18 |
| P29 C39 | Combate básico; mordida corta; daño por contacto | MVP | 🟢 | Mordida y daño por distancia; jefes estáticos (el «pulso» solo muestra un texto) | #14 |
| P30 C40 | Sin sistema económico | MVP | 🟢 | No hay moneda | — |

## 3. Pantallas, opciones, guardado

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P31 C42 · P61 C78 | Flujo inicio → menú → N1 → checkpoint → desafío → N2 → … → finalización/créditos | MVP | 🟡 🛑 | Tras N2 el juego continúa a N3; no hay «cierre del MVP» tras N2. Victoria ignora la opción elegida (E2) | #22 |
| P68 C87 | Menú, juego, pausa, nivel completado/final, créditos | MVP | 🟢 | Páginas `Main`, `Playing`, `Paused`, `Victory`, `Ending`, `Credits`, `Help` en `CGGame.cs` | #11 |
| P32 C43 | Volumen general, **música y efectos**; pantalla completa/ventana; consultar controles | MVP | 🟡 ⚖️ | Solo volumen general (y llega a 105 %, E8); **no** hay volúmenes separados **ni** opción pantalla completa/ventana; controles ✅ consultables y reasignables (➕). Propuesta C43 **no aprobada**: rige el compromiso original | — |
| P32 C43 | Idioma español | MVP | 🟢 | Textos en español | — |
| P33 C45 · P37 C49 · P38 C50 | Guardado persistente opcional, 1 progreso como máximo | Opc. | ➕ | JSON persistente con un único progreso ✅ (`Save_RoundTrip`); `.bak` no se usa al cargar | — |
| P34 C46 · P38 C50 | Reiniciar devuelve al último checkpoint | MVP | 🟡 | «Reiniciar desde checkpoint» existe, pero restaura corazones (ligado a E4) | #13 |
| P36 C48 | Checkpoint **automático** al alcanzarlo, sin objetos | MVP | 🟡 ⚖️ | Sin objetos ✅; activación **manual** (E). Propuesta C47/C48 pendiente | #13 |
| P41 C54 | Funciones de depuración desactivadas en la entrega | MVP | ⏳ | No se encontraron trucos de jugador; revisar en la release | — |

## 4. Historia, mundo y niveles

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P3 C9 · P43 C57 | Anomalía digital en el campus; lobo recupera fragmentos | MVP | 🟢 | Objetivos y diálogos de `CGLevels` | — |
| P46 C60 | Enseñar primero y luego pedir aplicar la información | MVP | 🟡 | NPC antes del quiz; falta validar que el quiz evalúe lo mostrado | #11 |
| P48 C63 · P65 C82 | N1 «Conoce tu campus» (controles + espacios); N2 «Tu primer semestre» (organización y apoyo) | MVP | 🟡 ⚖️ | N1 actual = Exterior/acceso; N2 = Planta baja/lobby (recepción). Temática aproximada; N2 no trata organización académica. Propuesta C63/C81 pendiente | #11 |
| P60 C77 | Dificultad moderada creciente entre N1 y N2 | MVP | ⏳ | No evaluado | #16 |
| P62 C79 | Narrativa con paneles estáticos y texto breve | MVP | 🟢 | Diálogos y paneles IMGUI | — |
| P64 C81 | Camino principal claro | MVP | 🟡 | Camino de suelo con huecos en x=20 y x=44; rutas elevadas inalcanzables (E5) | #19 |
| P57 C73 · P55 C71 | Asistente digital / mensajes breves; NPC opcionales | MVP | 🟢 | Sprites `Assistant_*` y NPC animados (➕) | — |

## 5. Personaje, animación, HUD, cámara

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P52 C68 | Animaciones idle, caminar/correr, salto/caída, mordida, daño, **derrota** (Animator) | MVP | 🟡 🛑 ⚖️ | Animación por fotogramas en `CGEntities.cs`, sin Animator en tiempo de ejecución; **no hay derrota** (E4) 🛑. El uso de sprites en lugar de Animator es objeto de la propuesta C68 ⚖️, no aprobada | #12, #13 |
| P56 C72 | Vida, velocidad, daño, salto, fragmentos | MVP | 🟢 | Variables en `CGHero` / `CGGame` | — |
| P66 C84 · P67 C85 | HUD con vida y fragmentos; indicaciones de interacción | MVP | 🟢 | HUD IMGUI; en móvil muestra textos de teclado | #10 |
| P69 C89 · P70 C90 | Una cámara lateral que sigue al personaje dentro de límites | MVP | 🟢 | `CGCamera2DRuntime.cs`; verificación visual pendiente | #16 |
| P72 C92 | Presentación 2D clara; URP 2D y luces opcionales | Opc. | ➕ | URP 2D con Light2D en escenas | — |

## 6. Controles e Input System

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P9 C16 · P73 C94 | Unity Input System | MVP | 🟢 | Paquete Input System 1.19, `activeInputHandler: 1` | — |
| P75 C96 · P78 C99 | Acciones **lógicas** del Input System (no teclas directas en código) | MVP | ❌ ⚖️ | Lectura directa de dispositivos (`CGControl` en `CGData.cs`); `InputSystem_Actions.inputactions` no se usa. Propuesta C94/C96/C99 **no aprobada**: rige el compromiso original. No impide jugar con teclado | — |
| P76 C97 | Mapa: A/D o flechas, Espacio/W, S, J, E, Esc | MVP | 🟡 | Por defecto A/D, Espacio, J, E, Esc ✅ en lógica; sin S (agacharse); W/flechas solo en menús | — |
| P76 C97 | En desafíos, Move cambia la selección y Confirm acepta | MVP | ❌ 🛑 | El quiz no acepta teclado ni mando (E3) | #21 |
| P78 C99 | Teclado de PC obligatorio; mando y remapeo opcionales | MVP/Opc. | 🟡/➕ | Teclado con defectos E1–E3; mando y remapeo ➕. Táctil Android ➕, incompleto (E10) | #10, #20, #21 |

## 7. Audio, ayuda, IA, arte

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P80 C102 · P82 C104 | SFX de acción, daño, interacción, checkpoint, finalización + música | MVP | 🟢 | Música, Jump, Attack, Damage, Pickup, Checkpoint, Denied, Win. Sin SFX de derrota | #12 |
| P83 C105 · P88 C111 | Registrar autor y licencia del audio externo | MVP | ⏳ | No se encontró registro de licencias en `Docs/` | #12 |
| P93 C118 · P94 C119 | Ayuda contextual mínima en N1 | MVP | 🟢 | Página «Guía del jugador» y diálogos; validar contexto en N1 | #11 |
| P96 C122 · P98 C124 | IA de enemigos sencilla (patrullar, atacar, recibir daño) | MVP | 🟢 | `CGEnemy` por `transform` | #14 |
| P103–109 C130–C136 | Pixel art 2D del campus; guía visual mínima | MVP | 🟢 | Sprites en `Resources/CG`. 74 % de los archivos de arte son duplicados exactos (auditoría §5) | #15 |

## 8. Gestión y evaluación

| Ref. | Compromiso | Prior. | Estado | Evidencia | Issue |
|---|---|---|---|---|---|
| P9 C16 · P110 C138 | Unity 6000.3.16f1 para todo el equipo | MVP | ✅ | `ProjectVersion.txt`; auditoría ejecutada con 6000.3.16f1 | — |
| P110 C138 | Backlog, control de versiones con Git, registro de bugs | MVP | 🟢 | Issues #9–#22, ruleset en `main`/`develop`, plantilla de PR | #9 |
| P115 C143 | Mitigar el riesgo de exceder el alcance | MVP | 🟡 🔀 | Hay más niveles de los prometidos con el MVP incompleto | — |
| P116 C144 | Evaluación breve con estudiantes reales de primer semestre | MVP | ⏳ | No realizada. No simular encuestas | #9 / release |

## Compromisos pendientes de decisión docente (⚖️)

Ninguna modificación del GDD está aprobada. Mientras el docente no responda, **rige la versión 1.0 presentada** y estas filas conservan su estado de incumplimiento:

| Compromiso original | Ref. | Situación actual | Propuesta (no aprobada) |
|---|---|---|---|
| Acciones **lógicas** de Unity Input System (Move, Jump, Bite, Interact, Pause) | P75 C96 · P78 C99 · P73 C94 | Lectura directa de dispositivos en `CGControl`; `InputSystem_Actions` sin uso | Documentar la lectura existente y mantener el Input System como paquete |
| **Agacharse** (S / flecha abajo) | P4 C10 · P14 C22 · P24 C33 · P76 C97 | No implementado | Tratarlo como deseable, no esencial |
| **Opciones de configuración**: volumen general, música y efectos; pantalla completa/ventana; controles | P32 C43 | Solo volumen general (con E8) y controles; sin volúmenes separados ni modo de ventana | Priorizar volumen general y controles; el resto como mejora |
| Checkpoint automático | P36 C48 | Activación manual con E | Permitir activación por interacción visible |
| Animaciones con Animator | P52 C68 | Fotogramas por script | Describir la técnica actual (la derrota sigue siendo obligatoria) |
| Temas de N1 y N2 | P48 C63 · P64 C81 | Exterior/acceso y Planta baja/lobby | Revisar preguntas y señalética sin reconstruir niveles |
| Niveles 3–9 y extra | P10 C17 · P59 C76 | Ya existen | Clasificarlos como ampliación |

Si el docente rechaza una propuesta, el compromiso pasa a ser trabajo obligatorio del MVP y debe abrirse o actualizarse su Issue.

## Resumen

- **🛑 Defectos que impiden cumplir el MVP:** N1 no completable (E6, C17/C76), vida sin derrota (E4, C17/C68), desafío final sin teclado (E3, C80/C97), falta del cierre del MVP tras N2 (C42/C78). La evaluación con estudiantes (C144) no es un defecto del juego, pero es un compromiso pendiente antes de la entrega.
- **🔀 Desviaciones del orden de desarrollo:** se crearon 9 niveles + extra antes de cerrar el MVP (C17) y no se contuvo el alcance (C143). No bloquean el juego por sí mismas, pero deben explicarse al docente.
- **⚖️ Pendientes de decisión docente:** acciones lógicas del Input System, agacharse, opciones de configuración, checkpoint automático, Animator, temas de N1/N2 y clasificación de niveles extra. Siguen contando como incumplimientos hasta que haya respuesta.
- **Ya verificado (✅):** escenas, física y salto medidos, guardado, pausa, lógica de misión en los 10 niveles, uso de Unity 6000.3.16f1.
- **Por verificar manualmente (⏳):** duración de 5–7 min, coherencia educativa y cierre tras N2 (casos V1–V4 de `Docs/QA/QA_PR_ISSUES_13_18_20_2026-11-06.md` §6), dificultad, cámara con render y Android en dispositivo.

Actualizar esta matriz **solo** con pruebas ejecutadas después de integrar cada PR, indicando el SHA de `develop` utilizado.
