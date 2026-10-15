# Entregas por partes — recursos de Víctor

Trabajar únicamente en `feature/victor-integracion`. Antes de cada commit,
mostrar al usuario los archivos exactos, el cambio realizado, las pruebas,
las limitaciones y una sugerencia de mensaje. **Esperar su indicación para hacer
ese commit.** No hacer push hasta recibir su indicación correspondiente.

| Parte | Entrega concreta | Verificación antes de proponer commit | Mensaje sugerido, sujeto al usuario |
| --- | --- | --- | --- |
| 1 | Estructura de carpetas y mapa origen → preparación → integrante | Carpetas reconocidas en Unity, rutas reales, ninguna imagen movida, GUID y escenas oficiales conservados | `Organizar carpetas y mapa de recursos visuales` |
| 2 | Corrección del importador y ajustes UI compatibles | Compilación, Point, PPU, compresión, recortes e identificadores conservados, vista de sprites | `Corregir importacion de recursos graficos de interfaz` |
| 3a | Recuperación de fondo/neones y prefab FondoCampus | Sin duplicados nuevos, `.meta` originales, aspecto en Unity en 16:9 y 4:3 | `Recuperar fondo y acentos visuales compartidos` |
| 3b | Prefabs gráficos de paneles, botones, mensajes y HUD | Referencias conservadas, estados distinguibles y sin acciones de navegación | `Preparar paneles y controles visuales compartidos` |
| 4 | Referencia del menú para Carla | Abrir/renderizar en Unity, sin eventos de navegación ni cambios a la escena oficial | `Agregar referencia visual del menu para Carla` |
| 5 | Referencia Game Over para Marcelo | Abrir/renderizar, sprites válidos, botones sin acciones conectadas | `Agregar referencia visual de Game Over para Marcelo` |
| 6 | Referencia de HUD/daño/muerte para Vicente | Abrir/renderizar, señales claras, sin lógica de vida, colisiones o transición | `Preparar referencia visual del HUD y las muertes` |
| 7 | `Docs/GUIA_RECURSOS_VISUALES_VICTOR.md` y evidencia de comprobaciones | Rutas y métodos reales, responsabilidades claras y pendientes honestos | `Documentar integracion de recursos visuales para el equipo` |

## Preparación de cada commit

Las partes 1 y 2 ya están incluidas en el commit del usuario `40a5f1e`
(`Corregir recursos graficos`). La parte 3a se entrega para que el usuario
ejecute los comandos de commit y push. No repetir los cambios ya publicados.

- Revisar `git status --short --branch` antes y después de cada parte.
- Seleccionar rutas explícitas; no usar `git add .` ni incluir cambios locales de otros trabajos.
- Mostrar `git diff --cached --name-status` y el diff de la parte si se preparó el área de staging.
- No borrar ni restablecer cambios ajenos para obtener un estado limpio.
- Si una corrección empezó antes de este acuerdo, presentarla por separado;
  no mezclarla con la organización de carpetas.
- No hacer merge a `develop`/`main`, force push, amend ni reescribir commits.

## Al terminar las partes aprobadas

Revisar `git status`, `git log --oneline`, `git diff develop --name-status` y
también la diferencia con `origin/develop`, porque la rama local `develop` puede
estar desactualizada. No actualizarla mediante merge dentro de esta tarea.
Publicar únicamente la rama `feature/victor-integracion` cuando el usuario
autorice el push de los avances ya verificados.

Mapa de trabajo: [carpetas y destinatarios](MAPA_CARPETAS_RECURSOS_VICTOR.md).
