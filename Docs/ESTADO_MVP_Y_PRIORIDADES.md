# Campus Glitch — Estado verificable del MVP y pendientes

**Corte técnico:** 6/11/2026. **Rama auditada:** `origin/develop` @ `9257b4ee01eafb63abdc040c2405ec3cc172c33e` (22/10/2026). Ver `AUDITORIA_CLAUDE_INFORME.md`; el estado debe actualizarse al integrar PR nuevos.

## Propósito y alcance comprometido

La guía presentada al docente (`GDD/Guia_JARVIS_v1.0_PRESENTADA.xlsx`, Preguntas hoja, C17 y C76) compromete un **MVP de dos niveles cortos totalmente funcionales**, junto con plataforma 2D, movimiento, salto, mordida, vida, enemigos, checkpoints, menús, pausa, HUD, audio y desafíos de orientación universitaria. El juego actual incluye nueve niveles y un extra: son **ampliación**, no motivo para dejar incompleto el MVP.

## Evidencia de la auditoría

- 67 pruebas PlayMode ejecutadas: 65 pasan y 2 fallan por inaccesibilidad del fragmento 2 en niveles 1 y 8.
- Advertencia: las pruebas llamadas `BUG_*` caracterizan y **confirman la presencia de errores**, aunque el test reporte PASS; no equivalen a comportamiento correcto.
- 18/18 escenas cargaron en Smoke PlayMode.
- Build Windows x64 compilado con 0 errores, 134,1 MB; arranque sin gráficos durante 25 segundos sin excepciones. **Falta verificar render real y recorrido manual del .exe**.
- Android no probado: el editor auditado carece del módulo Android Build Support. No hay APK ni capturas verificadas.

## Bloqueantes y asignación

| ID | Riesgo | Tarea existente | Responsable | Aceptación |
|---|---|---|---|---|
| E6 | Fragmento 2 inaccesible N1 y N8; no se puede completar N1 | [#18](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/18) | Víctor | 3 fragmentos y portal funcionales en N1 y N8, sin regresión |
| E5 | Salto 1,37 u vs plataformas desde 1,86 u | [#19](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/19) | Víctor | rutas elevadas alcanzables, sin alterar colisiones |
| E1 | Cancelar borrado elimina progreso | [#20](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/20) | Carla | cancelar conserva datos, confirmar sí borra |
| E3 | Quiz no responde teclado/mando | [#21](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/21) | Carla | quiz navegable y completable sin ratón |
| E4 | Sin estado real de Game Over | [#13](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/13) | Marcelo | derrota, reintento y retorno al menú consistentes |
| E2 | Victoria/Final ignora selección | [#22](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/22) | Carla | se respeta menú/siguiente/créditos |
| E10 | Sin correr/habilidad táctiles | [#10](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/10) | Vicente | controles implementados y verificados en APK real |
| README | Describe prototipo antiguo, no 18 escenas | [#9](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/issues/9) | Vicente | README actualizado, trazable y sin prometer pruebas no hechas |

**Secundarios:** #12 (animación/audio), #14 (enemigos/jefes), #15 (arte/duplicados), #16 (QA visual/Windows). Priorizar **dos niveles completos**, no cosmética de diez niveles.

## Integración y control

- PR [#17](https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026/pull/17) abre el protocolo de Gitflow; requiere revisión antes de merge.
- Los Issues son la fuente de tareas. No añadir aquí duplicados como lista desactualizada de Issues: mantener enlaces y fecha.
- Los PR de integrantes se dirigen a `develop`; el líder integra **solo después de revisión ajena y pruebas**. `main` se actualiza por PR desde una rama de release.
- La contribución individual se acredita con autor real de commit, Issue, PR, revisión, evidencia, y finalmente integración a `main` sin squash.
