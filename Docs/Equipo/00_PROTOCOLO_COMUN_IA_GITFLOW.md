# Protocolo obligatorio para el equipo y cualquier IA de programación

**Aplicación:** Codex, Claude Code, Cursor, Copilot Agent u otra herramienta. La IA es asistente del desarrollador; **el integrante sigue siendo responsable de sus cambios y de su autoría**.
**Repositorio:** `https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026` · Unity `6000.3.16f1` · rama base de desarrollo `origin/develop`.

## 1. Contexto que debes leer antes de tocar código

1. `Docs/AUDITORIA_CLAUDE_INFORME.md` (estado técnico auditado el 5/11; no confundir auditoría histórica con estado posterior a PRs).
2. `Docs/ESTADO_MVP_Y_PRIORIDADES.md` (prioridades y límites de las pruebas).
3. `Docs/GDD/Guia_JARVIS_v1.0_PRESENTADA.xlsx` (compromisos originales del docente; **NO editar**).
4. `Docs/GDD/PROPUESTAS_AJUSTE_v1.1.md` (solo propuestas, sujetas a aprobación).
5. `Docs/GITFLOW_EQUIPO.md` (en especial §0 *Roles y autoridad de integración*) y `.github/PULL_REQUEST_TEMPLATE.md`, integrados en `develop` con PR #17; si faltan en el clon, sigue este protocolo.
6. El Issue asignado en GitHub, sus comentarios y los PRs abiertos que afecten archivos comunes.

Si un archivo aún no está en el clon o no puede leerse, dilo explícitamente y pide al integrante su copia. No inventar referencias ni cambiar la guía oficial sin aprobación. Verifica el estado real del repo, porque los hallazgos se calcularon sobre `9257b4e`.

## 2. Proceso obligatorio por Issue

**Antes:** inspeccionar `git status`, `git branch -vv`, `git remote -v`, `git log -1`, `git config user.name` y `git config user.email`. No asumir que `develop` local está actualizado. Respetar modificaciones locales no registradas, archivos sin seguimiento y el stash existente `2630813`. Si el árbol no está limpio, no usar comandos destructivos: crear respaldo reversible o solicitar intervención del integrante.

**Inicio de rama** (si no existe una rama propia equivalente):

```powershell
git fetch origin --prune
git switch -c fix/usuario/NUM-objetivo origin/develop
```

Usar `feature/usuario/NUM-objetivo` para funcionalidades nuevas, `fix/` para errores, `docs/` para documentación y `test/` para pruebas. No crear varias ramas para el mismo cambio ni reutilizar una rama cuyo PR ya se fusionó.

**Desarrollo:** cambiar solo lo necesario para ese Issue. Si otro integrante modifica el mismo script, coordinar orden de PR; no combinar cambios en escenas y prefabs indiscriminadamente. En Unity, conservar archivos `.meta` y GUID y evitar generadores que sobrescriben 18 escenas. Jamás hacer cambios en `main` o `develop` directamente.

**Pruebas:** ejecutar en la medida disponible pruebas EditMode/PlayMode y comprobación visual del MVP; documentar exactamente qué se probó, entorno, resultado y limitaciones. Un PASS de `BUG_*` puede significar que el defecto sigue presente; para validar un fix escribir pruebas que fallen antes y pasen después. No inventar logs, capturas o resultados de Android.

**Commits:** mantener autor real, identidad propia y cambios coherentes, por ejemplo: `fix(menu): no borrar al cancelar (#20)`. No crear commits vacíos, fechas retroactivas ni reescribir la historia. Revisar `git diff --check`, `git status`, `git diff --cached`.

**Publicar:** `git push -u origin NOMBRE_RAMA`, abrir PR **hacia `develop`** con `Refs #N`, solicitar revisión a Vicente (revisor autorizado habitual; un revisor cruzado adicional es bienvenido), **avisar a Vicente** (ver §3) y **NO MERGEAR**. Si solo hay cambios parciales, abrir **Draft PR** y declararlo como tal. No usar `push --force` ni `git reset --hard` ni `git clean -fd`.

**Aprobación:** el PR de un compañero necesita **una aprobación (`Approve`) de un revisor autorizado** y todas las conversaciones resueltas; normalmente aprueba Vicente. Vicente integra tras comprobar evidencia, compatibilidad y, en cambios de Unity, los resultados de compilación, consola y Play Mode. Integrar **siempre con `Create a merge commit`** para conservar los commits y autores originales; no usar squash ni rebase merge. `main` solo recibe versión aceptada desde `release/1.1` mediante PR.

## 3. Entregables obligatorios del integrante por PR

- Enlace a Issue y PR, rama exacta, SHA de commits propios y autor/correo configurados.
- Resumen de lo cambiado, archivos afectados y por qué satisface el GDD.
- Pruebas reproducibles (pasos, comandos/Unity, resultados y capturas cuando proceda). Separar ejecutado de pendiente.
- Riesgos de integración: scripts compartidos, assets, escenas, controles; confirmar si requiere prueba cruzada.
- Captura/video si es visual, con fecha y rama/SHA identificables; evitar versionar vídeos grandes en Git sin acuerdo.
- Estado real: Draft / Listo para revisión / Cambios solicitados / Aprobado / Integrado / Validado.
- Mensaje al grupo y a Vicente **inmediatamente después del push, al crear el PR y cada vez que se actualice** (nuevos commits o respuesta a observaciones), usando `05_PLANTILLA_AVISO_PR.md`. Debe incluir como mínimo: **Issue, rama, SHA, pruebas, riesgos y enlace al PR**. Un push NO equivale a merge.

## 4. Revisión y trazabilidad de la evaluación individual

- Los PR de Carla, Marcelo y Víctor requieren **una aprobación de un revisor autorizado** (normalmente Vicente) antes del merge. GitHub no permite al autor aprobar su propio PR.
- Los PR propios de Vicente pueden fusionarse mediante la excepción administrativa `Repository admin — Allow for pull requests only`, siempre que antes quede documentado en el PR qué pruebas se ejecutaron, su resultado, los riesgos y la autorización explícita del merge. **Esta excepción no constituye un `Approve` formal** del autor; cuando sea posible, se pide igualmente revisión a un compañero.
- El ruleset activo de `main` y `develop` exige PR, una aprobación ordinaria, conversaciones resueltas, y bloquea force push y borrados. No hay pushes directos a ninguna de las dos ramas.
- Registrar revisión cruzada como evidencia adicional del trabajo individual, separada de la implementación.
- **No usar `Closes #N` como único método de seguimiento cuando el PR apunta a `develop`.** GitHub procesa el cierre automático al fusionar PRs hacia la rama predeterminada. Usar `Refs #N` en PR a develop y vincular Issue en el lateral/manual; Vicente cierra manualmente tras validar o al terminar la release, con enlace de evidencia.
- Para aparecer en Insights → Contributors, los commits deben llegar a **`main`**, rama por defecto, con correo vinculado a la cuenta GitHub. No hacer pushes individuales a main; llega mediante PR de release y merge commits conservando commits de cada autor.

## 5. Revisión de integración por Vicente (solo líder)

1. Confirmar que el PR se dirige a `develop`, usa `Refs #N`, el diff está acotado y el aviso incluye Issue, rama, SHA, pruebas, riesgos y enlace.
2. Revisar si afecta `CGGame.cs` / `CGEntities.cs` / escenas; verificar dependencias y conflictos.
3. Reproducir pruebas reportadas, principalmente del Nivel 1 y Nivel 2. Sin verificación de resultados no se fusionan cambios de Unity.
4. PR de compañero: dejar `Approve` (o `Request changes`), comprobar que las conversaciones están resueltas e integrar con `Create a merge commit`.
5. PR propio: pedir revisión a un compañero cuando sea posible; si se usa la excepción administrativa, publicar antes en el PR las pruebas, riesgos y la autorización, y después integrar con `Create a merge commit`. No presentar la excepción como aprobación.
6. Registrar resultado de aceptación y merge SHA en `develop`; avisar al autor del resultado; cerrar o actualizar el Issue manualmente con enlace a la evidencia.
7. No anunciar "integrado" cuando solo hay un push o un Draft PR.
8. Tras completar los P0 y la matriz MVP, crear `release/1.1` y PR a `main`, sin squash.

## 6. Qué no debe hacer una IA

- No hacer merges ni push a `main`/`develop`; no cambiar las reglas de protección sin autorización del administrador.
- No ejecutar el *bypass* administrativo del ruleset: su uso es una decisión exclusiva de Vicente sobre sus propios PR, tomada y ejecutada por él tras revisar las pruebas documentadas.
- No hacer commit o push sin que el integrante haya autorizado esa operación y esté conforme con el diff; al recibir permiso, realizar **solo push a su rama**.
- No cambiar `git config user.name` ni `git config user.email` para suplantar a otro desarrollador.
- No tocar otras tareas porque parezcan fáciles; abrir observación/Issue.
- No afirmar que hay tests que no se ejecutaron. No alterar el GDD original.
