# Gitflow y evidencia diaria — Equipo Campus Glitch
**Vigente desde:** 2026-11-05. **Actualizado:** 2026-11-06 (liderazgo y autorización de merges, Issue #9). **Proyecto:** Unity 6000.3.16f1.

## 0. Roles y autoridad de integración
- **Vicente (`vicentesystemdev`)** es administrador del repositorio y **responsable final de integrar los PR** en `develop` y `main`.
- **Carla, Marcelo y Víctor** trabajan en sus propias ramas, crean commits auténticos con su propia identidad y abren PR hacia `develop`. No fusionan PR.
- **PR de compañeros:** requieren **una aprobación (`Approve`) de un revisor autorizado** antes del merge. Normalmente la realiza Vicente.
- **PR propios de Vicente:** puede fusionarlos mediante la excepción administrativa del ruleset `Repository admin — Allow for pull requests only`, **solo después de documentar en el PR las pruebas ejecutadas y la autorización** (ver §4). Esta excepción **no constituye un `Approve` formal** del propio autor ni sustituye una revisión; si otro integrante revisa y aprueba, mejor.
- Nadie hace push directo a `main` ni a `develop`, incluido el administrador: **el PR es siempre obligatorio**.
- Nadie fusiona cambios de Unity sin verificar los resultados correspondientes (compilación, consola, Play Mode del flujo afectado y, si aplica, build).

## 1. Ramas
- `main`: versiones demostradas/entregables; prohibido push directo. No reescribir historial.
- `develop`: integración de mejoras revisadas; también sin push directo.
- `feature/<persona>/<NUM>-<tema>`: funcionalidad pequeña desde `develop` (p. ej., `feature/carla/11-ux-menus`).
- `fix/<persona>/<NUM>-<tema>`: corrección de bug desde `develop`.
- `docs/<persona>/<NUM>-<tema>` y `test/<persona>/<NUM>-<tema>`: documentación y pruebas desde `develop`.
- `chore/<persona>/<tema>`: herramientas o configuración desde `develop`.
- `release/YYYY-MM-DD`: estabilización desde `develop`; PR a `main` tras QA.
- `hotfix/<tema>`: correcciones urgentes desde `main`; después integrar también en `develop`.

Las ramas heredadas del primer examen se conservan como histórico. No reutilizarlas para trabajo nuevo. No crear ramas permanentes de cada persona: crear ramas por tarea/Issue y cerrarlas tras merge.

## 2. Inicio de cada jornada (PowerShell o Git Bash)
```bash
git fetch --all --prune
git switch develop
git pull --ff-only origin develop
git switch -c feature/carla/11-ux-menus
```
Sustituir el ejemplo por la rama que corresponda a la persona y al Issue. Si ya existe una rama de la tarea: `git switch <rama>`; actualizarla con cuidado y confirmar que no haya cambios locales sin guardar.

## 3. Mientras se trabaja
```bash
git status
git diff --check
git diff --stat
git add Assets/CampusGlitch/Scripts/Archivo.cs
git commit -m "fix(ui): corregir regreso desde controles"
git push -u origin feature/carla/11-ux-menus
```
Hacer commits **atómicos, honestos y ligados a cambios reales**. Idealmente registrar al menos un avance verificable por cada jornada **en que se trabaje**; no fabricar commits vacíos ni falsear fechas. No mezclar varios módulos sin necesidad. No usar `git add .` sin revisar qué se añadirá. Conservar archivos Unity `.meta`.

Convención sugerida: `feat(scope): ...`, `fix(scope): ...`, `docs(scope): ...`, `test(scope): ...`, `chore(scope): ...`, `refactor(scope): ...`.

## 4. Pull Request y revisión
- Crear PR **rama -> develop**. En PR hacia `develop` usar **`Refs #N`** (GitHub solo cierra Issues automáticamente al fusionar en la rama por defecto, `main`). Vincular el Issue en el lateral del PR; Vicente cierra el Issue manualmente tras validar, con enlace a la evidencia. `Closes #N` solo en el PR de release hacia `main` y cuando el Issue esté realmente terminado.
- Rellenar plantilla de PR con qué cambió, cómo se probó, capturas/registro y limitaciones.
- **Avisar a Vicente** cada vez que se publique o actualice un PR (nuevo push, respuesta a observaciones), con: Issue, rama, SHA de los commits, pruebas ejecutadas, riesgos y enlace al PR. Usar `Docs/Equipo/05_PLANTILLA_AVISO_PR.md`. Un push no equivale a merge.
- **PR de Carla, Marcelo o Víctor:** solicitar revisión a Vicente (revisor autorizado habitual). Se requiere **una aprobación** y todas las conversaciones resueltas antes del merge; el autor no fusiona su PR.
- **PR de Vicente:** solicitar revisión a un compañero cuando sea posible. Si se integra sin aprobación ajena, usar la excepción administrativa `Repository admin — Allow for pull requests only` solo tras dejar en el PR un comentario con: pruebas ejecutadas y resultado, riesgos, y la frase de autorización (p. ej., `Autorizo el merge por excepción administrativa: pruebas documentadas arriba`). Esa excepción no es un `Approve`.
- Resolver conflictos en la rama del autor y repetir pruebas tras merge/update.
- Integrar **siempre con `Create a merge commit`**, conservando los commits y autores originales. No usar `Squash and merge` ni `Rebase and merge`: el docente evalúa el progreso real por commit.
- No aprobar una revisión solo por el número de commits; leer diffs y comprobar ejecución cuando aplique. En cambios de Unity, no fusionar sin verificar compilación, consola y Play Mode del flujo afectado.

## 5. Evitar conflictos en Unity
1. Antes de editar, anunciar los archivos/escenas que se tocarán en el Issue.
2. No modificar simultáneamente la misma escena `.unity`, prefab, `ProjectSettings/EditorBuildSettings.asset` o `CGGame.cs`.
3. Dividir mejoras por script/objeto cuando sea posible; reservar ventanas de integración para escenas compartidas.
4. Nunca eliminar `.meta` para 'arreglar' una importación; revisar GUID y referencias.
5. No pulsar indiscriminadamente `Campus Glitch/Reparar escenas y mostrar en 2D`: puede reescribir escenas.
6. Antes de enviar PR: abrir Unity desde la rama correcta, comprobar consola y Play Mode, revisar `git status` para detectar archivos generados.
7. Los builds se entregan mediante enlace de release/artefacto o medio solicitado, no mezclados con Assets del proyecto.

## 6. Evidencias diarias exigidas por equipo
En un comentario del Issue o archivo de bitácora registrar:
- Fecha real, autor y usuario GitHub.
- Issue/objetivo del día.
- Commits SHA y enlace a PR (si está abierto).
- Evidencia del cambio: captura, log, vídeo breve o test reproducible.
- Estado: completado / en progreso / bloqueado y siguiente acción.

Ejemplo: `2026-11-05 — @VarlaCdev20 — #11 — commit abcd123 — verificado menú -> controles -> volver en Unity Play Mode — PR #xx en revisión`.
No inventar SHAs ni evidencias. Si no se completó la tarea, indicar lo pendiente.

## 7. Orden recomendado
**Etapa A — Estabilizar (primera jornada):** todos revisan `develop`, crean rama por Issue; Vicente documenta baseline; Víctor prepara inventario; Carla verifica menús; Marcelo reproduce derrota.
**Etapa B — Corregir (siguientes jornadas):** cambios P0 pequeños y revisados diariamente, evitando conflictos de escenas.
**Etapa C — Validar:** Windows + Android, matriz de QA y capturas reales; corregir regresiones.
**Etapa D — Release:** actualizar `develop`, crear `release/YYYY-MM-DD`, probar versión congelada, PR a `main`, etiqueta `v0.x.y` y evidencia de entrega.

## 8. Configuración de GitHub (ruleset activo)
En **Settings > Rules > Rulesets** está activo un ruleset sobre `main` y `develop` que:
- Exige PR antes de merge (no hay push directo).
- Exige **una aprobación** ordinaria.
- Exige que todas las conversaciones del PR estén resueltas.
- Bloquea force push y borrado de las ramas.
- Tiene como única excepción (*bypass*) al rol `Repository admin` en modo **`Allow for pull requests only`**: permite a Vicente fusionar sus propios PR sin aprobación ajena, pero no le permite hacer push directo. Su uso se rige por §0 y §4.

Requerir checks solo cuando exista una CI que funcione y esté publicada; no bloquear el equipo con checks inexistentes. Cualquier cambio del ruleset lo decide y aplica únicamente el administrador, y se comunica al equipo.

## 9. Qué no hacer
No reiniciar el repositorio para simular días trabajados. No compartir credenciales GitHub. No atribuir commits hechos por una sola persona a otras cuentas. No fusionar importaciones masivas sin examen del diff. No afirmar que 'Android funciona' sin instalación y prueba física.
