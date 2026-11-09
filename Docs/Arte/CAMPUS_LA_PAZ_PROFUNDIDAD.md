# Campus La Paz: ambientes, actores y profundidad

Revisión: 2026-11-09. Recorrido de nueve niveles y subsuelo conservado. La propuesta visual utiliza la sede La Paz como referencia institucional y mantiene el mapa del equipo; no constituye un plano arquitectónico del edificio.

## Referencias comprobadas

- [Sede La Paz](https://unifranz.edu.bo/sedes/la-paz/): Héroes del Acre esquina Landaeta, 1855; la página ofrece un recorrido virtual. El visor público requiere ejecución interactiva y no se utilizó para deducir pisos ni medidas.
- [Centro de Desarrollo de Sistemas, La Paz](https://unifranz.edu.bo/blog/unifranz-la-paz-inaugura-el-centro-de-desarrollo-el-nuevo-corazon-tecnologico-de-ingenieria-de-sistemas/): taller con mesas móviles, ensamblaje de electrónica, software, hardware y proyectos integradores. Orienta el laboratorio de Sistemas.
- [Biblioteca de la sede La Paz](https://bibliovirtual.unifranz.edu.bo/cgi-bin/koha/opac-library.pl?branchcode=BLP): consulta, investigación, colecciones y computadoras. La dirección publicada indica **sexto piso**. El mapa del equipo denomina su etapa «Piso 5 / Biblioteca e Innova»; se conserva su secuencia de juego y se registra esta discrepancia para confirmar la señalética física con el equipo.
- [Clínica Odontológica de La Paz](https://unifranz.edu.bo/blog/la-clinica-odontologica-unifranz-emprende-una-cruzada-de-salud-contra-la-caries/): formación clínica y atención a la comunidad, base contextual de los consultorios.
- [Sistemas e Innovación Digital](https://unifranz.edu.bo/carreras/ingenieria-de-sistemas-e-innovacion-digital/): disponible en La Paz; formación en redes, producción de sistemas, calidad de software e innovación. La ambientación acompaña los objetivos de recuperar nodos, registros y conexiones existentes.

Los demás pisos, áreas de atención, terraza y subsuelo proceden del mapa previo del equipo. Sus ubicaciones exactas y denominaciones requieren confirmación local. No se presentan como observaciones del recorrido virtual.

## Composición de los diez mapas

| Escena | Ambiente y recorrido visible |
|---|---|
| SC_Level01 | Entrada, plaza, laboratorios, pasarela glitch y lobby de salida |
| SC_Level02 | Recepción, servicios estudiantiles, espera y conexión |
| SC_Level03 | Mezanine, atención, oficinas y dirección |
| SC_Level04 | Laboratorio de salud, muestras, investigación y registros |
| SC_Level05 | Centro de Desarrollo, hardware, redes y Sistemas |
| SC_Level06 | Aulas dinámicas, aprendizaje, proyectos y reto de secuencias |
| SC_Level07 | Biblioteca, consulta, estudio colaborativo e innovación |
| SC_Level08 | Terraza, lounge, mirador de La Paz y enlace exterior |
| SC_Level09 | Clínica odontológica, consultorios, registros y núcleo digital |
| SC_Extra_Subsuelo | Anatomía, osteoteca, colección docente y pasillo secreto |

La perspectiva sigue siendo lateral y ortográfica. La profundidad se representa con arquitectura en perspectiva, planos de fondo, desplazamiento relativo y mobiliario detrás del recorrido. El jugador conserva movimiento X/Y con físicas 2D. Las plataformas transitables mantienen sus colliders propios y una franja de contacto visible; los huecos reales se marcan en los bordes. El mobiliario de fondo no introduce superficies de apoyo falsas.

La organización de planos y prioridad de sprites sigue las herramientas descritas en el [flujo 2D de Unity](https://docs.unity3d.com/6000.1/Documentation/Manual/2d-game-creation-wokflow.html). Los colores y tamaños separan el plano de acción de la arquitectura, sin aplicar desenfoque al personaje.

## Recursos y pautas de diseño

Todos los archivos siguientes están en `Assets/CampusGlitch/Resources/CG/ActorsV2/`. Se conservan los PNG originales; los recortes se describen por rectángulos y pivotes, sin modificar los píxeles del protagonista.

| Recurso | Contenido e indicaciones de composición |
|---|---|
| CampusNPC.png | 4 × 4: guardia, estudiante, técnica de laboratorio y asistente. Poses de espera, conversación, explicación y descanso; misma silueta y pies por fila. Azul oscuro, naranja y cian; transparencia y contorno claro. |
| CampusStaff.png | 4 × 4: recepción, dirección, docente y coordinación. Personajes ilustrados ficticios, sin retratar personas reales. Chaquetas azul oscuro, identificación naranja, carpeta/libro/tableta y cuatro poses por rol. |
| CampusEnemies.png | 8 × 3: bug terrestre rojo, saltador blanco/rojo y dron de vigilancia ámbar. Siluetas diferenciadas y fotogramas separados; comportamiento vinculado a los controladores existentes. Los jefes conservan su arte anterior. |
| LevelOneZones.png | 2 × 2: plaza, laboratorios, pasarela glitch y lobby. Profundidad arquitectónica, carril frontal despejado, sin personajes, HUD ni plataformas falsas pintadas. Se combina con la entrada previamente aprobada. |
| CampusAmbientes_A.png | 2 × 2: recepción, mezanine, salud y desarrollo de Sistemas. Cristal, hormigón claro, mesas móviles y equipamiento al fondo. Base de suelo al 92 % de cada panel. |
| CampusAmbientes_B.png | 2 × 2: aulas, biblioteca, odontología y anatomía. Áreas profundas y mobiliario reconocible; azul/cian, luz cálida y acentos naranja; sin obstáculos pintados en el carril frontal. |
| CampusTerraza.png | Panorama elevado con pérgola, lounge, jardines, ciudad y montañas; no reutiliza la entrada como terraza. Carril frontal despejado y línea de suelo al 85 % de la imagen. |

Los archivos `*_Frames.json` contienen los recortes de 16, 16 y 24 fotogramas de actores. La máscara de render de los actores descarta el halo de alfa bajo; no se utiliza para cambiar colisiones. Importación Point, sin mipmaps, sin compresión destructiva; máximo 4096 para el panorama de terraza de 2172 píxeles de ancho.

El Guardián rojo y morado se conservan. El filtro Point se fija también al cargar; las ocho animaciones existentes se seleccionan por entrada, suelo, velocidad, ataque, daño y victoria. El Inspector muestra el estado y fotograma efectivos.

## Scene y ejecución

`CGEditorWorldPreview` construye la vista desde el mismo `CGWorld.Setup` usado al jugar. Su jerarquía `CG_EDITOR_LIVE_PREVIEW` es temporal, sin guardado en escena ni inclusión en build. Los cuerpos y controladores permanecen desactivados en edición; solo se muestrean poses visuales. Al iniciar Play se retira la vista temporal y se genera el mundo jugable.

El preview serializado anterior se oculta temporalmente y se restaura al limpiar la vista. No se ejecutaron regeneradores ni se reescribieron escenas. Los sprites creados en memoria se conservan y se recuperan si Unity descarga una referencia al abrir otra escena.

## Rutas y geometría

Se mantuvieron salto, gravedad y cápsula. Se desplazaron accesos que sellaban verticalmente su propia plataforma de lanzamiento en N2, N6, N8 y extra. Se añadieron apoyos intermedios en N3, N7 y N8 para llegar a las rutas altas. No se eliminaron plataformas.

`CGPlatformRouteRegression` registra saltos y aterrizajes positivos sobre los colliders generados en una escena física aislada, con velocidades 3,5 y 5,8. Los puntos de lanzamiento del suelo se complementan con los recorridos de teclado desde spawn, que atraviesan los huecos, recogen fragmentos y completan el portal. Esta combinación no sustituye la revisión independiente ni una prueba manual de todas las variantes de movimiento.
