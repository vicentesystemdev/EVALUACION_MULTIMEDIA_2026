# Estado de las tareas de Víctor

Revisión: 2026-11-07, Unity 6000.3.16f1. Base de esta comprobación: `feature/victor-integracion` en `359160c`, coincidente con la rama remota.

Este documento distingue la implementación comprobada de los criterios todavía pendientes. No declara cerradas las cuatro tareas.

| Tarea | Comprobado | Pendiente para aceptación completa |
|---|---|---|
| #18: fragmento inaccesible en N1/N8 | Corrección mínima de la posición del segundo fragmento; verificación final de 289 PASS / 0 FAIL en diez niveles, parado y en carrera; diez recorridos principales con 3/3 datos, terminal, portal y progreso | Revisión independiente del PR #29 y validación manual complementaria |
| #19: salto y geometría | Material sin fricción, plataformas elevadas sólidas, salto medido de aproximadamente 3,164 unidades; 56 comprobaciones de física y diez recorridos principales | Verificar todas las rutas elevadas opcionales desde inicio/checkpoints y revisión independiente del PR #30; una ruta principal completada no demuestra todos los accesos superiores |
| #15: inventario y referencias | Recursos visuales nuevos con metadatos, pautas de importación, comprobación de GUID duplicados y conservación de los assets anteriores | **No completada:** falta `Docs/Arte/INVENTARIO_REFERENCIAS.md` con inventario global, clasificación mantener/duplicado candidato/no tocar y comprobación de referencias; falta su rama/PR específico |
| #16: QA de escenas y Windows | Capturas reales de niveles seleccionados, recorridos automatizados en diez niveles, build de las 18 escenas habilitadas con 0 errores y arranque sin gráficos durante 25 segundos | **Parcial:** checklist visual de los 18 assets de escena y prueba manual del .exe con gráficos, teclado y mando; falta su rama/PR específico |

## Presentación del Guardián y campus

La mejora visual está implementada y comprobada: variantes roja/morada, 128 fotogramas en total, estados de animación vinculados al controlador, retratos, fondos exterior/interior, mobiliario y HUD de progreso real. Las verificaciones de arte registran 23 PASS / 0 FAIL. Se conserva el modelo clásico, el desbloqueo morado y la geometría física.

[Informe y límites](GUARDIAN_VISUAL_2026_11_07.md) · [Pruebas de arte](GuardianVisual/arte.txt) · [Física](GuardianVisual/fisica.txt) · [Diez recorridos](GuardianVisual/recorridos.txt) · [Build Windows](GuardianVisual/build_windows.txt).

La repetición final del arnés de fragmentos sobre el código integrado (`8250846`, sin cambios de ejecución respecto de `359160c`) produjo [289 PASS / 0 FAIL](GuardianVisual/fragmentos_final.txt). Incluye aproximaciones paradas y en carrera, tres triggers por nivel y condiciones del portal. Esta prueba aísla aproximaciones recolocando al personaje y desactivando enemigos; no se presenta como recorrido integral. Los diez recorridos principales citados arriba son una prueba diferente, sin esas ayudas. Al terminar se verificaron presencia y hashes de los guardados contra el respaldo tomado inmediatamente antes de la prueba.

## Comprobación en ejecución

En el Nivel 1 se inspeccionaron el estado del controlador, el sprite activo, la cápsula, la gravedad y las plataformas del escenario generado. Se observaron diez plataformas elevadas con collider habilitado, sólido y sin efector. La cápsula mantiene tamaño 0,76 × 1,64, offset (0; 0,82), fricción 0 y gravedad 3,2. El controlador informa suelo y velocidad vertical cero al descansar.

Al cargar la escena se registraron 27 advertencias de scripts faltantes. No se consideran resueltas por el rediseño. En la inspección de la jerarquía jugable generada no aparecieron componentes Missing; corresponde revisar por separado la serialización de los previews y su carga. La consola anterior a esta inspección no mostraba errores de compilación ni errores de juego. Los avisos deben incluirse en el cierre de #15/#16, sin borrar componentes o regenerar escenas arbitrariamente.

## Alcance del PR de integración

El PR de `feature/victor-integracion` presenta los cambios terminados con sus evidencias y estos pendientes explícitos. Incluye las correcciones propias de #18/#19 mientras sus PR separados continúen abiertos; coordinar la revisión para evitar duplicar integraciones. No fusiona a develop/main ni sustituye la revisión de otra persona.

La siguiente mejora de NPCs, enemigos, ayudante y las cinco zonas del Nivel 1 todavía no forma parte de este estado. Los conceptos preparados se mantienen fuera de los recursos del proyecto hasta iniciar esa fase. El Guardián aprobado se conserva.
