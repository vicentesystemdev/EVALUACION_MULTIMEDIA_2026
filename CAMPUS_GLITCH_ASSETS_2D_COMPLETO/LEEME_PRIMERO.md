# CAMPUS GLITCH — PAQUETE DE ASSETS 2D PARA UNITY 6

## Entrega
- **930 entradas principales en inventario** (no equivale a sprites totalmente diferentes).
- **625 sprites ambientales originales de biblioteca previa**, con PNG individuales y .aseprite editables nuevos de una capa.
- **270 sprites modulares adicionales nuevos**, PNG RGBA y .aseprite.
- **2 variantes moradas animadas** desbloqueables tras completar nivel 2, y **15 objetos interactivos animados** con archivos editables y PNG por fotograma.
- **3 .aseprite originales aportados por el usuario** SIN modificaciones: 17 + 17 + 9 fotogramas; tags respetados.
- **13 personajes/enemigos nuevos animados**, .aseprite con tags y frames PNG individuales.
- Incluye previsualizaciones y guías por nivel, manifiestos y scripts auxiliares Unity.

## IMPORTANTE: alcance real
Esta es una **biblioteca de producción inicial**, NO un juego completo ni 625+ escenas terminadas.
Los sprites nuevos están hechos en pixel art lateral simple, orientados a ser refinados. Los PNG no animados son assets base.
Los `.aseprite` generados contienen una capa RGBA por sprite; no equivalen a archivos multicapa de arte final.
Las láminas de `Previsualizaciones` son solo referencia; no están listas para cortarse como sprites.
No se ha probado el proyecto en el Editor de Unity, pues aquí no está instalado. Verifica animaciones y colisiones con la versión exacta de tu equipo.

## Uso en Unity 6 (6000.x)
1. Crea proyecto **2D Core** con versión igual en todos los equipos (verifica Help > About Unity).
2. Copia la carpeta `Assets` de este ZIP dentro de la carpeta `Assets` de tu proyecto.
3. Selecciona cada sprite importado: Texture Type Sprite (2D and UI), **PPU 32**, Filter Point, Compression None, Mip Maps off.
   La clase en `Assets/Editor/CampusGlitchTextureImporter.cs` lo aplica a los PNG alojados en Art.
4. Tus personajes originales: `Originales_Usuario/`, sus frames listos en `Assets/Art/Animaciones/Originales_Usuario`.
5. Para crear clips por carpetas usa **Tools > Campus Glitch > Construir clips PNG**.
   Luego crea/ajusta un Animator Controller por personaje y configura transiciones manualmente.
6. Para colisión: crea Tilemap de suelo con TilemapCollider2D (y opcional CompositeCollider2D); props de colisión con BoxCollider2D.
7. Para plataformas que se pueden atravesar desde abajo: PlatformEffector2D.
8. Separar Sorting Layers: `Background`, `Architecture`, `Platforms`, `PropsBack`, `Characters`, `PropsFront`, `VFX`, `UI`.
9. La prueba `CGPlayerSide2D.cs` es un script auxiliar básico con entrada antigua; no tiene adaptador del nuevo Input System todavía.

## Estructura
- `Assets/Art/PNG/00_Biblioteca_Previa_625/`: tiles y props de biblioteca ya existente
- `Assets/Art/PNG/01_Nuevos_Modulares/`: props exclusivos, UI y piezas de progresión
- `Assets/Art/PNG/02_Animados/`: sprite sheets y PNG individual de enemigos y NPCs nuevos
- `Assets/Art/Aseprite/`: ficheros editables correspondientes a las secciones anteriores
- `Assets/Art/Animaciones/Originales_Usuario`: fotogramas separados por tags del lobo y asistente
- `Originales_Usuario/`: 3 archivos .aseprite originales (sin modificación) y sus spritesheets originales
- `Documentacion/`: inventario CSV/JSON, mapa de niveles y animaciones
- `Previsualizaciones/`: ejemplos y tableros (solo referencia)

## Organización de niveles y kits
- **01_Exterior**: 00_Biblioteca_Previa_625/01_Common, 00_Biblioteca_Previa_625/02_Exterior, 01_Nuevos_Modulares/00_Global, 01_Nuevos_Modulares/01_Exterior
- **02_PlantaBaja**: 00_Biblioteca_Previa_625/01_Common, 00_Biblioteca_Previa_625/03_Hallway, 00_Biblioteca_Previa_625/16_Admin, 00_Biblioteca_Previa_625/17_CommonAreas, 01_Nuevos_Modulares/02_PlantaBaja
- **03_Mezanine**: 00_Biblioteca_Previa_625/03_Hallway, 00_Biblioteca_Previa_625/16_Admin, 01_Nuevos_Modulares/03_Mezanine
- **04_Piso1_Salud**: 00_Biblioteca_Previa_625/07_Health, 00_Biblioteca_Previa_625/09_Biochemistry, 00_Biblioteca_Previa_625/15_Gastronomy, 01_Nuevos_Modulares/04_Piso1_Salud
- **05_Piso2_Sistemas**: 00_Biblioteca_Previa_625/06_Systems, 00_Biblioteca_Previa_625/14_Design, 00_Biblioteca_Previa_625/12_Business, 01_Nuevos_Modulares/05_Piso2_Sistemas
- **06_Piso4**: 00_Biblioteca_Previa_625/04_Classroom, 00_Biblioteca_Previa_625/10_Law, 00_Biblioteca_Previa_625/11_Psychology, 01_Nuevos_Modulares/06_Piso4
- **07_Piso5_Biblioteca**: 00_Biblioteca_Previa_625/05_Library, 01_Nuevos_Modulares/07_Piso5_Biblioteca
- **08_Terraza_Lounge**: 00_Biblioteca_Previa_625/17_CommonAreas, 00_Biblioteca_Previa_625/02_Exterior, 01_Nuevos_Modulares/08_Terraza_Lounge
- **09_Odontologia**: 00_Biblioteca_Previa_625/08_Dentistry, 00_Biblioteca_Previa_625/07_Health, 01_Nuevos_Modulares/09_Odontologia
- **EXTRA_Subsuelo**: 00_Biblioteca_Previa_625/07_Health, 01_Nuevos_Modulares/EXTRA_Subsuelo

## Contexto de diseño
- **2D lateral plataformero**, no top-down ni 3D.
- Lobo Guardián = único avatar jugable inicial; su skin morada (`02_Animados/Skins_Desbloqueables`) se desbloquea después del Nivel 2.
- Unifranzito, Guardia U, Director y Noemi Moller Chuquimia = NPCs, no avatares del jugador.
- Subsuelo = nivel extra.
- Mensaje institucional mostrado en el juego: **INNOVACIÓN · APRENDE HACIENDO**.
- Créditos de desarrollo: Victor Grover Asturizaga Plata (assets/terreno/niveles/colisiones), Carla Valeria Encinas Cano (personaje/sonidos), Vicente Oscar Claros Mamani (conexión de niveles/menús/acciones), Marcelo Josue Escobar Chipana (villanos/enemigos). Asignación según roles de trabajo suministrados.

## Recomendación para el equipo
No modelar los niveles como una sola imagen. Usa un conjunto de sprites + Tilemaps por capas + triggers/doors y prefabs. Revisa el Tour 360 antes de la versión artística final; las propuestas son simplificaciones de diseño.

## Sobre la postura del lobo
Los archivos originales aportados muestran un lobo cuadrúpedo. Se conservan completos y sin alteraciones. Se incluyen **dos variantes bípedas opcionales nuevas**, roja y morada (21 frames cada una), para resolver el requisito anterior de avatar erguido. Su arte es una propuesta inicial, no una alteración de los originales.

## Nota de importación de Aseprite
Puedes utilizar los PNG sin plugin adicional. Para importar `.aseprite` como sprites de Unity, instala **2D Aseprite Importer** desde Package Manager si está disponible en tu versión. Los originales editables también se abren directamente con Aseprite/LibreSprite compatible.
