# Mapa de recursos visuales — Víctor

## Por dónde empezar

Víctor prepara gráficos, importación y ejemplos visuales. Carla integra el menú;
Marcelo integra Game Over; Vicente integra el nivel, HUD y mecánicas.

**Estado de esta primera entrega:** estructura y orientación de carpetas.
Los prefabs y las escenas de referencia indicados como pendientes todavía no existen.
No se han movido, renombrado ni duplicado imágenes existentes.

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
| `Assets/Art/PNG/04_Compartidos/Fondos/` | Ubicación preparada para fondos recuperados del original | Pendiente: comprobar e incorporar `MenuBackdrop.png` con su `.meta`, sin duplicados. |
| `Assets/Art/PNG/04_Compartidos/Efectos/` | Ubicación preparada para acentos visuales | Pendiente: evaluar `NeonBlue.png`, `NeonOrange.png`, `NeonPurple.png` del original. |
| `Assets/Prefabs/UI/Compartidos/` | Ubicación preparada para composiciones UI reutilizables | Pendiente: paneles, botones y HUD gráficos, sin navegación ni mecánicas. |
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
| `CAMPUS_GLITCH: Assets/CampusGlitch/Resources/CG/UI/MenuBackdrop.png` | Recuperación pendiente hacia `04_Compartidos/Fondos/` | Menú y Game Over | Carla y Marcelo reutilizarán el mismo fondo, con composición distinta. |
| `CAMPUS_GLITCH: Assets/CampusGlitch/Resources/CG/UI/NeonBlue.png`, `NeonOrange.png`, `NeonPurple.png` | Evaluación pendiente hacia `04_Compartidos/Efectos/` | Detalles de identidad | Compartidos; no copiar imágenes ya presentes. |

En las filas abreviadas, `Global_UI/` y `Global_Gameplay/` parten de
`Assets/Art/PNG/01_Nuevos_Modulares/`. Los nombres de los futuros prefabs se
documentarán cuando se creen y comprueben en Unity.

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
- Hay 1.286 PNG y 930 archivos Aseprite. El análisis de bytes detectó 828
  repeticiones PNG respecto de los archivos únicos: no eliminarlas, pueden tener
  rutas y referencias diferentes.
- Los tres PNG `ButtonOrange*` tienen contenido idéntico. No prometer estados
  visualmente distintos sin preparar y verificar una solución gráfica.
- Varios nombres son orientativos: `MenuPanel` se ve como una pantalla,
  `DialogueFrame` y `DamageSpikes` comparten un dibujo genérico con otros archivos.
  Hay que componer los ejemplos con el arte real, sin atribuirles detalle inexistente.
- Los 28 sprites de `Global_UI` tienen recortes Multiple existentes. Conservar sus
  identificadores internos además de los GUID; cambiar a Single sin revisar puede
  romper referencias.
- La revisión inicial encontró un error de compilación del importador por
  `textureWrapMode`; su corrección y verificación pertenecen a la parte de importación.
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
