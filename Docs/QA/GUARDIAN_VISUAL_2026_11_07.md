# Verificación del Guardián y mapa

Fecha: 2026-11-07. Unity 6000.3.16f1. Rama: `feature/victor-integracion`.

## Cambio comprobado

Nuevo Guardián acorazado rojo/morado, ocho secuencias de ocho fotogramas por variante, retratos con recorte del atlas, fondo exterior/interior del campus y 16 objetos modulares. El HUD de escritorio utiliza datos reales; el panel táctil anterior conserva su separación para pausa. El modelo clásico sigue seleccionable. No se añaden habilidades, monedas ni condiciones de progreso de las láminas de referencia.

Los pies del nuevo sprite se anclan al origen del controlador y la escala permanece uniforme por variante. La cápsula, gravedad, velocidades, material sin fricción y rutas físicas conservan los ajustes de #18 y #19. La imagen de las plataformas se dimensiona al collider; los pasamanos pertenecen a sus plataformas. No se ejecutaron regeneradores de escenas ni animadores.

## Resultados

| Verificación | Resultado | Evidencia |
|---|---|---|
| Arte: 128 recortes, pivotes, variantes, estados, importación, 16 props, modelo clásico e independencia física | 23 PASS / 0 FAIL | [arte.txt](GuardianVisual/arte.txt) |
| Caídas laterales, paredes sólidas, detención, ascenso/aterrizaje en tres alturas y geometría de diez niveles | 56 PASS / 0 FAIL | [fisica.txt](GuardianVisual/fisica.txt) |
| Recorridos desde spawn, tres fragmentos, checkpoint, terminal, enemigos, portal y progreso | 10/10 PASS | [recorridos.txt](GuardianVisual/recorridos.txt) |
| GUID duplicados en los `.meta` inspeccionados bajo Assets | 0 | Barrido por GUID; los ocho recursos nuevos de Resources conservan su `.meta` |
| Guardado personal tras los recorridos | Restaurado | Coinciden presencia y SHA-256 de JSON, respaldo y temporal con la copia previa |
| Build Windows x64 con las 18 escenas existentes | Succeeded, 0 errores, salida 0 | [build_windows.txt](GuardianVisual/build_windows.txt) |
| Arranque del ejecutable Windows sin gráficos | Activo durante 25 s, sin excepciones encontradas en su log | [build_windows.txt](GuardianVisual/build_windows.txt) |

Los recorridos reutilizan el arnés de #19: entrada de teclado mediante Input System, sin trasladar al héroe ni otorgar fragmentos o victorias. **Límite conservado:** el cuestionario de la base se responde mediante su manejador de respuesta correcta; no prueba navegación física por las opciones del cuestionario. El arnés de física incluye salto parado y en carrera; el recorrido integral utiliza carrera. Esto no equivale a una revisión manual completa de todas las rutas opcionales.

Capturas reales de PlayMode a 2560 × 1440:

- [N1: exterior y fragmento recuperado](GuardianVisual/N1_juego.png).
- [N2: lobby y salto](GuardianVisual/N2_juego.png).
- [N8: terraza](GuardianVisual/N8_juego.png).
- [Personalización: variantes roja y morada](GuardianVisual/variantes.png). La variante morada puede previsualizarse; equiparla sigue requiriendo completar N2. Esta captura utiliza progreso inicial temporal en memoria, sin guardar cambios en el perfil personal.

Se revisó la lectura de silueta, pies, retratos, contraste de textos y correspondencia entre plataforma visible y collider. El arte antiguo de enemigos y parte del mobiliario específico se conserva; esta revisión no sustituye todos los assets del equipo. Persisten avisos preexistentes de componentes Missing; no se presentan como resueltos por este cambio. La compilación de scripts finaliza sin errores.

El build final se produjo con `CGBuildScenes.BuildWindows` en modo batch, comprobando previamente que existieran las 18 escenas habilitadas. Tamaño de los archivos: 169.135.554 bytes (161,3 MiB); Unity informa 66,1 MiB de texturas. El coste de los nuevos atlas y fondos sin compresión requiere una revisión posterior de memoria si se apunta a móvil. El arranque sin gráficos comprueba ejecución, no entrada, cámaras ni legibilidad del ejecutable con gráficos. La validación gráfica aquí corresponde a PlayMode. El control de escritorio no completó la activación de la ventana, por lo que **continúa pendiente la comprobación manual del .exe con gráficos**, teclado y mando de #16.

Se completaron los metadatos de siete carpetas de animaciones que ya contenían recursos. No se eliminaron esos recursos ni se cambiaron sus GUID; los metadatos de carpeta son nuevos. Los temporales generados por el build y las configuraciones locales quedan excluidos de Git.

## Orden de integración

La rama existente conservó su historial. Se actualizó desde `origin/develop` (`048b3e3`) mediante merge normal y se incorporaron las ramas propias de #18 y #19 mediante merges normales. El prototipo antiguo de Víctor se conserva fuera de Assets en `CAMPUS_GLITCH_ASSETS_2D_COMPLETO/Prototipo_Victor`, con sus metadatos, para no reintroducirlo como escena jugable ni mezclar su controlador con el actual.

Los cambios visuales se registran en commits separados de recursos, animación del personaje, mobiliario, integración del mapa/HUD y ajustes de legibilidad. Los PR #29 y #30 continúan pendientes de revisión de otra persona; no se fusionan a develop ni main desde esta rama. Las configuraciones y evidencias privadas de la máquina quedan fuera de los archivos versionados.

La dirección y especificaciones de los recursos están en [LOBO_GUARDIAN_DIRECCION.md](../Arte/LOBO_GUARDIAN_DIRECCION.md). Los trabajos globales de inventario #15 y QA manual de las 18 escenas #16 no se consideran cerrados con estas pruebas.
