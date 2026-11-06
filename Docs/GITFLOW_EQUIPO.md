# Gitflow y evidencia diaria — Equipo Campus Glitch
**Vigente desde:** 2026-11-05. **Proyecto:** Unity 6000.3.16f1.

## 1. Ramas
- `main`: versiones demostradas/entregables; prohibido push directo. No reescribir historial.
- `develop`: integración de mejoras revisadas; también sin push directo.
- `feature/<persona>/<tema>`: funcionalidad pequeña desde `develop` (p. ej., `feature/carla/ux-menus`).
- `fix/<persona>/<tema>`: corrección de bug desde `develop`.
- `chore/<persona>/<tema>`: herramientas, documentación o configuración desde `develop`.
- `release/YYYY-MM-DD`: estabilización desde `develop`; PR a `main` tras QA.
- `hotfix/<tema>`: correcciones urgentes desde `main`; después integrar también en `develop`.

Las ramas heredadas del primer examen se conservan como histórico. No reutilizarlas para trabajo nuevo. No crear ramas permanentes de cada persona: crear ramas por tarea/Issue y cerrarlas tras merge.

## 2. Inicio de cada jornada (PowerShell o Git Bash)
```bash
git fetch --all --prune
git switch develop
git pull --ff-only origin develop
git switch -c feature/carla/ux-menus
```
Sustituir el ejemplo por la rama que corresponda a la persona y al Issue. Si ya existe una rama de la tarea: `git switch <rama>`; actualizarla con cuidado y confirmar que no haya cambios locales sin guardar.

## 3. Mientras se trabaja
```bash
git status
git diff --check
git diff --stat
git add Assets/CampusGlitch/Scripts/Archivo.cs
git commit -m "fix(ui): corregir regreso desde controles"
git push -u origin feature/carla/ux-menus
```
Hacer commits **atómicos, honestos y ligados a cambios reales**. Idealmente registrar al menos un avance verificable por cada jornada **en que se trabaje**; no fabricar commits vacíos ni falsear fechas. No mezclar varios módulos sin necesidad. No usar `git add .` sin revisar qué se añadirá. Conservar archivos Unity `.meta`.

Convención sugerida: `feat(scope): ...`, `fix(scope): ...`, `docs(scope): ...`, `test(scope): ...`, `chore(scope): ...`, `refactor(scope): ...`.

## 4. Pull Request y revisión
- Crear PR **rama -> develop**. Referenciar `Closes #N` cuando cierre realmente el Issue.
- Rellenar plantilla de PR con qué cambió, cómo se probó, capturas/registro y limitaciones.
- Pedir una revisión a integrante distinto del autor. No aprobar ni fusionar el propio PR sin revisión.
- Resolver conflictos en la rama del autor y repetir pruebas tras merge/update.
- Conservar los commits individuales (merge commit para integrar; evitar squash cuando el docente evalúe el progreso real por commit).
- No aprobar una revisión solo por el número de commits; leer diffs y comprobar ejecución cuando aplique.

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

## 8. Configuración de GitHub recomendada
Como administradores del repo, configurar en **Settings > Rules > Rulesets** (si la cuenta y el repositorio lo permiten):
- `main` y `develop`: requerir PR antes de merge, al menos una aprobación de otro integrante, bloquear force push y borrado de ramas.
- Requerir checks solo cuando exista una CI que funcione y esté publicada; no bloquear el equipo con checks inexistentes.
- Deshabilitar ediciones directas de `main` por costumbre incluso si las reglas del plan no permiten imponerlas.

## 9. Qué no hacer
No reiniciar el repositorio para simular días trabajados. No compartir credenciales GitHub. No atribuir commits hechos por una sola persona a otras cuentas. No fusionar importaciones masivas sin examen del diff. No afirmar que 'Android funciona' sin instalación y prueba física.
