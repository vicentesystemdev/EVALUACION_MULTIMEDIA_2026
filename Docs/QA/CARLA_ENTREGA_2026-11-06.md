# Entrega de Carla — 6 de noviembre de 2026

Rama: feature/carla. PR: #27 hacia develop. Issues: #11, #12, #20, #21, #22.

## Cambios finales

- El escenario de PC ya no queda cubierto por el fondo opaco de los menus; pausa y terminal oscurecen el mundo sin sustituirlo por el fondo del menu.
- El escalado toma en cuenta el area segura de pantalla.
- Portada accesible con raton/toque y titulo del guardian ajustado para evitar recortes.
- Mapa y controles tienen botones Volver; teclado/mando pueden seleccionarlos.
- B/Escape cancelan la reasignacion sin cambiar el control guardado.
- Confirmacion de borrado abre sobre Cancelar y solo borra con seleccion explicita.
- Terminal: tres respuestas y cierre seleccionables; se restaura el tiempo al cerrar/acertar y se filtra la doble activacion.
- Volumen bidireccional por teclado, cruceta, raton y toque, limitado a 0–100 %.
- Dano no mortal con sonido; subtitulos de sonidos visibles en HUD cuando la opcion esta activada.
- Animaciones conservan el fotograma inicial de cada estado y respetan reduccion de destellos.

## Evidencia ejecutada

Unity 6000.3.16f1, compilacion y pruebas desde Campus Glitch > QA > Comprobar entradas y persistencia aislada.

| Comprobacion | Resultado y alcance |
|---|---|
| Cancelar con Enter, Escape, B; borrado explicito con A | PASS, entradas simuladas; JSON temporal escrito y recargado, guardado personal intacto |
| Terminales N1, N8 y N6 (nivel con jefe) | PASS, incorrecta/correcta/cierre y retorno a Playing con escala temporal 1; sin completar recorridos |
| Regreso desde mapa/controles, B en reasignacion | PASS, entradas simuladas |
| Victoria al menu y final a creditos | PASS, entradas simuladas; siguiente nivel tambien cubierto por prueba de decisiones |
| Volumen | PASS, limites y pulsaciones izquierda/derecha |
| Animacion | PASS, 28 combinaciones: idle, caminar, correr, ascenso, caida, ataque, dano por cuatro modelos/skins; inspeccion de fotograma inicial, no video completo |
| Recursos | PASS, todos los fotogramas de cuatro modelos/skins y nueve clips de sonido cargados |
| Subtitulos | PASS, generacion de mensaje de dano; la respuesta auditiva requiere escucha humana |
| Play Mode | Portada entra con raton, menu carga SC_Level01; escenario y HUD visibles |

Captura real de SC_Level01 en Play Mode: [nivel y HUD](CarlaCapturas/20261106-213101.png).

## Pendiente de revision y pruebas externas

- Mando fisico y Android/APK: no disponibles en esta comprobacion. Las entradas simuladas no equivalen a certificacion en dispositivos.
- Recorridos completos, respuesta auditiva y pruebas visuales en distintas resoluciones moviles: pendientes.
- Advertencia de componente con script ausente al cargar el nivel; no se modificaron escenas compartidas. Revisar con Vicente.
- La primera importacion mostro fallos .uss/.uxml del paquete com.unity.2d.tooling; no impidieron las pruebas.
- Repetir la matriz tras integrar cambios de Marcelo en CGGame.cs.
- Vicente conserva la revision y la decision de merge. No se cierra ningun Issue como validado al 100 % con estas limitaciones.

## Reproducir

Abrir CAMPUS_GLITCH_CARLA_QA en Unity. Ejecutar Campus Glitch > QA > Comprobar entradas y persistencia aislada fuera de Play Mode. La prueba restaura las escenas registradas, escala temporal, datos en memoria y modo de actualizacion de entrada; elimina exclusivamente su directorio temporal. Usa mecanismos internos del Input System fijados a la version del proyecto.
