# Auditoría inicial — Campus Glitch / EVALUACION_MULTIMEDIA_2026
**Fecha de revisión:** 2026-11-05  
**Alcance:** inspección estática de GitHub (árbol, ramas, commits, PR, README, scripts, build settings y documentación); **no constituye prueba de ejecución dentro de Unity**.  
**Fuente de desarrollo:** `develop` SHA `9257b4ee01eafb63abdc040c2405ec3cc172c33e`.  
**Fuente de entrega anterior:** `main` SHA `4865a1d22e3ce4ff95a6a889c1b6454117fde657`.

## 1. Estado comprobable
- `main` tiene 34 commits en su historial (todos fechados 2026-10-15).
- `develop` tiene 38 commits, 4 por delante de `main` y 0 por detrás; los commits están fechados 2026-10-15 (34) y 2026-10-22 (4). No se encontraron nuevos commits posteriores al 22/10 durante esta consulta.
- 7 ramas visibles; sin protección activada en `main` y `develop`. 8 PR fusionados; ninguno con revisión formal registrada.
- PR #8 (2026-10-22) importó el juego completo, con **10.644 archivos modificados** y 955.601 líneas añadidas según GitHub; priorizar cambios incrementales pequeños desde ahora.
- En `develop`: 18 escenas habilitadas en `ProjectSettings/EditorBuildSettings.asset` (8 pantallas + 9 niveles + subsuelo extra); Unity `6000.3.16f1`; URP2D e Input System.
- Juego integrado en `Assets/CampusGlitch/`: lógica de menús, nivel, enemigos, fragmentos, preguntas, progreso JSON, controles teclado/mando/táctil. Encontrar código **no** prueba que compila o funciona.
- Android: `CGAndroidBuild.cs`, `CGTouch.cs` e informe de exportación existentes. La sección de resultados y evidencias del informe está pendiente; no dar APK por validado.
- Windows: `CGBuildScenes.BuildWindows` escribe en `Build_Windows/`; inicialmente no estaba ignorado.
- README de `develop` aún presenta el prototipo antiguo de tres escenas, contradiciendo el juego completo. Credenciales/responsabilidades deben revisarse contra el estado actual y autorías.
- No se observó una suite de tests del videojuego ni workflow CI dentro del árbol de `develop`; existe validador de recursos/editor `CGProjectValidator` y auditor `CGLevelAuditV5`. Su presencia no sustituye Play Mode ni pruebas en dispositivos.

## 2. Riesgos técnicos
| Prioridad | Hallazgo | Verificación/corrección |
|---|---|---|
| P0 | Falta evidencia de builds y pruebas reproducibles | Ejecutar Unity, capturar consola, Windows/Android y resultados por caso |
| P0 | `CGGame.KillHero` hace `Respawn()` y restaura 3 corazones; escena `GameOver.unity` anterior no se incluye en 18 escenas | Validar rúbrica/diseño y corregir si Game Over es requisito |
| P0 | Menús, niveles y progreso aún no tienen informe de QA de ejecución | Matriz de pruebas con evidencia por responsable |
| P1 | `main` no representa el juego de `develop` | Probar release antes de realizar PR a `main` |
| P1 | PR gigantes y sin revisión formal | Una rama/Issue/PR por unidad de trabajo, revisión cruzada |
| P1 | Arte repartido entre varias carpetas y directorio de recursos completo | Inventario, análisis de referencias/GUID antes de mover o eliminar |
| P1 | Herramienta 'Reparar escenas' abre y guarda las escenas existentes | No ejecutarla sin respaldo, control del diff y consenso de editores |
| P1 | Documentación no refleja la arquitectura actual | Actualizar README solo con estados implementados y probados diferenciados |
| P2 | Faltan checks automáticos | Añadir validación CI estática cuando la base esté estable; tests reales de Unity cuando sea viable |

## 3. Reparto inicial (Issues)
- Vicente — [#9 Línea base, Gitflow y documentación](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/9); [#10 Android y evidencias](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/10).
- Carla — [#11 Menús y UX](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/11); [#12 Animación y audio](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/12).
- Marcelo — [#13 Derrota y Game Over](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/13); [#14 Enemigos y progresión](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/14).
- Víctor — [#15 Inventario y GUID](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/15); [#16 Escenas y build Windows](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/16).

## 4. Criterio de 'limpio'
**No** borrar historial, rehacer fechas, recrear commits de compañeros, forzar push a `main` ni eliminar assets por intuición. 'Limpio' significa estructura validada, control de ramas, unidades de cambio pequeñas, Issues/PR revisados, documentación vigente y builds verificadas. Si Unity falla, documentar el error y conservar el SHA exacto.

## 5. Criterios de salida
1. Base `develop` ejecutada sin errores bloqueantes (o registro reproducible de fallos).
2. Flujos Splash > menú > nivel > pausa > progreso > salida probados.
3. Resultado real de build y prueba Windows; resultado real de build y prueba Android.
4. README, CHANGELOG y reportes por fecha, autor, Issue y commit SHA.
5. Revisión de PR por alguien distinto al autor; integración a `develop`; release aprobado hacia `main`.

**Nota de atribución:** esta auditoría de GitHub y su documentación inicial no acreditan trabajo de código de los otros integrantes. Cada persona debe realizar y firmar sus cambios reales.
