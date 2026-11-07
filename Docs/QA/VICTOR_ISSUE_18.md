# Problema 18 — acceso al segundo fragmento

Víctor · 7 de noviembre de 2026 · Unity 6000.3.16f1.
Rama: `fix/victor/18-fragmento-inaccesible`, creada desde `origin/develop` (`048b3e3`). Refs #18.

## Corrección

El segundo fragmento se generaba en `(31, 3.35)`. En N1 la plataforma de x=32 tiene base en y=1.88; en N8 la de x=31 tiene base en y=1.71. Ambas interrumpen el salto desde abajo. La altura libre del salto se midió aproximadamente en 1.38 unidades.

`CGWorld.cs` coloca únicamente ese fragmento de N1 y N8 en `(31, 1.1)`, la misma altura de los otros dos fragmentos obligatorios. Las otras 28 posiciones de fragmentos, las plataformas, los colliders, las escenas, sus GUID, las reglas de misión y el guardado no cambian. El ajuste general del salto y el contacto lateral pertenecen a #19.

## Pruebas ejecutadas

- Compilación de scripts completada, sin errores.
- Reproducción anterior: N1 y N8 no permiten recoger el segundo fragmento parado ni en carrera; tampoco alcanzan 3/3 ni victoria. N8 produjo 23 comprobaciones correctas y 6 fallidas, correspondientes a estas condiciones en ambos modos.
- Control positivo N2, sin cambiar su geometría: 29 correctas, 0 fallidas.
- Después de la corrección: **289 correctas y 0 fallidas**, diez niveles × dos modos de aproximación. Se recogen los tres fragmentos por `OnTriggerEnter2D`, cada uno aumenta el contador exactamente una vez, se rechaza el portal sin fragmentos/terminal/jefe y se comprueba victoria, progreso y desbloqueo.
- Se respaldaron y restauraron los tres nombres posibles del guardado (`.json`, `.bak`, `.tmp`), conservando su presencia o ausencia. El JSON personal original coincide por SHA-256 antes/después.

[Resultados completos](VictorIssue18/resultados.txt). Capturas reales de cámara: [N1 antes](VictorIssue18/N1_antes.png), [N1 después](VictorIssue18/N1_despues.png), [N8 antes](VictorIssue18/N8_antes.png), [N8 después](VictorIssue18/N8_despues.png). Las capturas posteriores muestran el fragmento ya recogido; no incluyen la interfaz IMGUI.

## Reproducir

1. Abrir esta rama en Unity 6000.3.16f1 y esperar a que compile.
2. Con Play detenido y escenas guardadas, ejecutar `Campus Glitch > QA > Verificar fragmentos (Issue 18)`.
3. El arnés abre N1, genera los diez mundos con el código real y registra cada resultado en una carpeta nueva de `Documentos/CampusGlitchQA`. Al terminar sale de Play y restaura el guardado.
4. Para aceptación manual: recorrer N1, N2 y N8 desde el inicio, recoger 3/3, resolver la terminal con ratón, combatir cuando corresponda y salir por el portal. Comprobar además la caída junto a obstáculos manteniendo la dirección presionada.

## Alcance y pendientes

Las 289 comprobaciones **no equivalen a recorridos completos**: el arnés recoloca al personaje antes de cada aproximación y desactiva el daño de enemigos para aislar la alcanzabilidad. La terminal usa su manejador de respuesta existente y el jefe recibe llamadas reales de ataque; no se fuerza `taken`, `terminalSolved` ni `bossDead` para aprobar una condición.

El recorrido continuo, la caída lateral natural solicitada durante la revisión y las rutas elevadas requieren completar #19. El primer intento de automatizar un recorrido no llegó a mover al personaje con su arnés; se descartó como evidencia de jugabilidad. El PR se presenta como **Draft**, pendiente de aceptación completa y revisión de Vicente. Android, mando físico, build y recorrido gráfico de Windows quedan pendientes de #16.

La escena original genera advertencias de componentes sin script, ya observadas antes de la corrección. Se conservan para revisión del integrador; esta rama no regenera escenas. No se realizó merge ni se cerró el Issue.

## Revisión para Vicente

Revisar `CGWorld.cs`, ejecutar el menú de QA y comprobar manualmente N1/N2/N8 después de integrar los cambios de física correspondientes. Mantener el PR en borrador hasta completar esa aceptación. Los commits conservan la identidad Git existente de Víctor.
