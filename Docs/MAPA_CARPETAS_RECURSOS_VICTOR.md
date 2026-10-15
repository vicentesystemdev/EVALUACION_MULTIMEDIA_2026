# Mapa de recursos visuales — Víctor

## Por dónde empezar

Víctor prepara gráficos, importación y ejemplos visuales. Carla integra el menú;
Marcelo integra Game Over; Vicente integra el nivel, HUD y mecánicas.

**Estado actual:** estructura organizada e importación UI corregida en `40a5f1e`.
Esta entrega recupera `MenuBackdrop.png`, los tres neones y prepara `FondoCampus.prefab`.
Los paneles, botones, HUD y las escenas de referencia siguen pendientes.
No se han movido, renombrado ni duplicado imágenes que ya existían en esta copia.

## Qué contiene cada carpeta y para qué sirve

| Carpeta | Qué contiene / contendrá | Para qué y quién la utiliza |
| --- | --- | --- |
| `Assets/Art/Aseprite/` | Fuentes editables originales `.aseprite` | Víctor edita arte aquí; los compañeros usan los sprites importados, no necesitan copiar las fuentes a sus escenas. |
| `Assets/Art/PNG/00_Biblioteca_Previa_625/` | Biblioteca gráfica previa | Consulta y reutilización; conservar las rutas y los `.meta`. |
| `Assets/Art/PNG/01_Nuevos_Modulares/` | Piezas por ambientes y elementos globales | Selección de imágenes existentes para interfaz y nivel. |
| `Assets/Art/PNG/01_Nuevos_Modulares/Global_UI/` | Botones, paneles, corazones, energía e iconos | Fuente compartida de UI para Carla, Marcelo y Vicente. Mantener una sola copia. |
| `Assets/Art/PNG/01_Nuevos_Modulares/Global_Gameplay/` | Objetos y señales del juego | Vicente reutiliza sus imágenes; Víctor solo prepara ejemplos visuales. |
| `Assets/Art/PNG/02_Animados/` | PNG por fotograma y hojas de personajes | Buscar sprites decorativos; una hoja completa no equivale a un fotograma. |
| `Assets/Art/PNG/03_Interactivos_Animados/` | Hojas y fotogramas de objetos interactivos | Consulta visual; no implican que exista comportamiento programado. |
| `Assets/Art/Animaciones/` | PNG organizados por personaje y acción | Elegir fotogramas para referencias. No ejecutar el generador de clips sin revisar: el script local existente reemplaza clips. |
| `Assets/Art/PNG/04_Compartidos/Fondos/` | `MenuBackdrop.png`, recuperado con `.meta` original | Fondo compartido para menú y Game Over. |
| `Assets/Art/PNG/04_Compartidos/Efectos/` | `NeonBlue.png`, `NeonOrange.png`, `NeonPurple.png` | Acentos pixel art para las composiciones del equipo. |
| `Assets/Prefabs/UI/Compartidos/` | `FondoCampus.prefab`; paneles, botones y HUD pendientes | Arrastrar el fondo bajo un Canvas. Las futuras composiciones seguirán sin navegación ni mecánicas. |
| `Assets/Scenes/Referencias/` | Ubicación preparada para tres escenas de muestra | Carla, Marcelo y Vicente consultan y trasladan composiciones a sus propias escenas. No agregar referencias a Build Settings. |
| `Assets/Editor/` | Herramientas de importación/preparación del editor | Víctor mantiene las herramientas; no se adjuntan a objetos de juego. |
| `Assets/Scripts/` | Código de juego | Fuera de la entrega visual. Conservar los cambios locales de otros trabajos. |
| `Docs/` | Mapa, plan por partes y futura guía de integración | Consultar antes de trasladar los recursos. |

## De qué lado a qué lado: flujo de entrega

| Recurso de origen | Preparación de Víctor | Referencia pendiente | Destino que integra el compañero |
| --- | --- | --- | --- |
| `Global_UI/ButtonOrangeNormal.png`, `ButtonOrangeHover.png`, `ButtonOrangePressed.png` | Importación compatible y futuro prefab gráfico en `Assets/Prefabs/UI/Compartidos/` | Menú y Game Over | Carla: `Assets/Scenes/MenuPrincipal.unity`; Marcelo: `Assets/Scenes/GameOver.unity`. Ellos conectan las acciones. |
| `Global_UI/MenuPanel.png`, `DialogueFrame.png` | Composiciones de panel/mensaje; revisar el aspecto real antes de estirar | Menú, Game Over y HUD | Cada integrante reutiliza la composición adecuada. |
| `Global_UI/HeartFull.png`, `HeartEmpty.png`, `EnergyBarFill.png`, `EnergyBarEmpty.png` | Futuro HUD gráfico | `Referencia_HUD_Muerte_Victor.unity` | Vicente: `Assets/Scenes/Nivel01.unity`, con su lógica propia de vidas/daño. |
| `Global_UI/PauseIcon.png`, `SettingGear.png`, `MapIcon.png` | Iconos disponibles; no implican funcionalidades | Según la composición | El responsable de cada pantalla decide su integración funcional. |
| Fotograma en `Assets/Art/Animaciones/Originales_Usuario/` | Personaje decorativo sin controlador de movimiento | Menú y HUD | Carla/Vicente toman la representación visual que corresponda. |
| `Global_Gameplay/DamageSpikes.png` | Señal visual rotulada como ejemplo; no asumir que el dibujo sea un pincho | HUD y muertes | Vicente define peligros, colisiones y muerte dentro de `Nivel01`. |
| `CAMPUS_GLITCH: Assets/CampusGlitch/Resources/CG/UI/MenuBackdrop.png` | `04_Compartidos/Fondos/MenuBackdrop.png` → `Assets/Prefabs/UI/Compartidos/FondoCampus.prefab` | Menú y Game Over, pendientes | Carla y Marcelo reutilizan el mismo fondo bajo su Canvas, detrás del contenido. |
| `CAMPUS_GLITCH: Assets/CampusGlitch/Resources/CG/UI/NeonBlue.png`, `NeonOrange.png`, `NeonPurple.png` | Recuperados en `04_Compartidos/Efectos/` | Detalles de identidad, composiciones pendientes | Compartidos; Image con proporción conservada y Raycast Target desactivado para decoración. |

En las filas abreviadas, `Global_UI/` y `Global_Gameplay/` parten de
`Assets/Art/PNG/01_Nuevos_Modulares/`. Las rutas `04_Compartidos/` parten de
`Assets/Art/PNG/`. La guía de uso del fondo está en
`Assets/Prefabs/UI/Compartidos/LEEME.md`.

## Escenas: quién abre qué

| Integrante | Referencia que preparará Víctor | Escena oficial existente que modifica su responsable |
| --- | --- | --- |
| Carla | `Assets/Scenes/Referencias/Referencia_Menu_Victor.unity` — pendiente | `Assets/Scenes/MenuPrincipal.unity` |
| Marcelo | `Assets/Scenes/Referencias/Referencia_GameOver_Victor.unity` — pendiente | `Assets/Scenes/GameOver.unity` |
| Vicente | `Assets/Scenes/Referencias/Referencia_HUD_Muerte_Victor.unity` — pendiente | `Assets/Scenes/Nivel01.unity` |

Las referencias son muestras visuales: no sustituyen escenas oficiales, no
cargan otras escenas y no implementan colisiones, pérdida de vidas ni navegación.

## Hallazgos que condicionan las siguientes partes

- Unity inspeccionado mediante MCP: `6000.3.16f1`, proyecto correcto.
- La rama de trabajo es `feature/victor-integracion`.
- El inventario inicial tenía 1.286 PNG y 930 archivos Aseprite. Se agregaron
  cuatro PNG del original, sin coincidencias de contenido ni GUID previos.
  El análisis inicial de bytes detectó 828
  repeticiones PNG respecto de los archivos únicos: no eliminarlas, pueden tener
  rutas y referencias diferentes.
- Los tres PNG `ButtonOrange*` tienen contenido idéntico. No prometer estados
  visualmente distintos sin preparar y verificar una solución gráfica.
- Varios nombres son orientativos: `MenuPanel` se ve como una pantalla,
  `DialogueFrame` y `DamageSpikes` comparten un dibujo genérico con otros archivos.
  Hay que componer los ejemplos con el arte real, sin atribuirles detalle inexistente.
- Los 28 sprites de `Global_UI` tienen recortes Multiple existentes. Conservar sus
  identificadores internos además de los GUID; cambiar a Single sin revisar puede
  romper referencias. Verificados mediante MCP: Point, 32 PPU, Uncompressed,
  sin mipmaps; recortes e identificadores conservados respecto de `3c4604f`.
- El error del importador por `textureWrapMode` está corregido con `wrapMode`
  en `40a5f1e`. Unity ya no indica compilación fallida. Se conserva el historial
  de consola con el error anterior y timeouts de MCP; no se ha limpiado para ocultarlos.
- `FondoCampus.prefab` fue instanciado y renderizado mediante MCP en una escena
  de previsualización aislada, en 1280 × 720 y 1024 × 768. Fondo visible, sin
  componentes perdidos, con proporción conservada y recorte lateral en 4:3.
  Los tres neones también se renderizaron en Unity. Las capturas de comprobación
  quedan en `.codex/`, excluidas de Git.
- La primera captura de esta parte falló por ausencia de la cámara temporal:
  Unity impidió abrir una escena aditiva mientras la escena Untitled seguía
  sin guardar. Se mantuvo intacta y se repitió la comprobación con cámara propia
  en una escena de previsualización aislada; esas capturas sí se comprobaron.
  El mensaje del intento fallido permanece en consola.
- Al empezar, Build Settings solo incluía `Assets/Scenes/SampleScene.unity`.
  Se conserva esa configuración. El equipo debe integrar las escenas oficiales
  en un avance posterior, fuera de esta organización visual.
- `GestorEscenas.cs` no está en esta copia de la rama. Existe en
  `origin/develop`, en `Assets/Scripts/Core/GestorEscenas.cs`, inspeccionado tras
  actualizar las referencias remotas. No se ha fusionado ni copiado.

## Reglas al trasladar recursos

1. Arrastrar el sprite o prefab existente desde Project; no copiar PNG a nuevas carpetas por pantalla.
2. Conservar cada `.meta` junto a su recurso. Para mover archivos en futuras tareas, usar el editor de Unity y revisar referencias.
3. No mover ahora las bibliotecas originales; esta organización agrega lugares de entrega y un mapa de uso.
4. No incluir `Library`, `Temp`, `.codex`, configuraciones personales ni capturas de prueba en commits.
5. Víctor no modifica `MenuPrincipal.unity`, `GameOver.unity`, `Nivel01.unity` ni `GestorEscenas.cs`.

Continuación: [plan de entregas por partes](PLAN_ENTREGAS_VISUALES_VICTOR.md).
