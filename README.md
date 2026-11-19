<a id="inicio"></a>
<div align="center">
  <h1>🐺 CAMPUS GLITCH</h1>
  <h3>Videojuego 2D de plataformas para la orientación de estudiantes de primer semestre</h3>
  <p><strong>Programación Multimedia I · UNIFRANZ La Paz · 2026</strong></p>
  <p>
    <img alt="Unity 6000.3.16f1" src="https://img.shields.io/badge/Unity-6000.3.16f1-222222?style=for-the-badge&amp;logo=unity&amp;logoColor=white">
    <img alt="C#" src="https://img.shields.io/badge/C%23-Scripts-7A37A5?style=for-the-badge&amp;logo=csharp&amp;logoColor=white">
    <img alt="Windows" src="https://img.shields.io/badge/Windows-x64-0078D6?style=for-the-badge&amp;logo=windows&amp;logoColor=white">
    <img alt="Android" src="https://img.shields.io/badge/Android-APK-3DDC84?style=for-the-badge&amp;logo=android&amp;logoColor=white">
  </p>
  <p><a href="#descargas">⬇️ Descargas</a> · <a href="#juego">🎮 El juego</a> · <a href="#requisitos">✅ Requisitos</a> · <a href="#controles">🕹️ Controles</a> · <a href="#niveles">🗺️ Niveles</a> · <a href="#equipo">👥 Equipo</a> · <a href="#creditos">📜 Créditos</a> · <a href="#proyecto">🚀 Abrir el proyecto</a></p>
</div>

---

<a id="descargas"></a>
## ⬇️ Descargas

| Plataforma | Archivo | Enlace |
|---|---|---|
| **Windows** (x64) | `GAME_CAMPUS_GLITCH_WINDOWS.zip`: descomprimir y ejecutar `CampusGlitch.exe` | [Descargar ejecutable](ENLACE_DRIVE_WINDOWS) |
| **Android** (7.1 o superior, ARM64) | `CampusGlitch.apk`: instalar permitiendo orígenes desconocidos | [Descargar APK](ENLACE_DRIVE_APK) |

Ambas versiones se generaron desde la rama `main` de este repositorio con Unity 6000.3.16f1.

---

<a id="juego"></a>
## 🎮 El juego

Una **anomalía digital** altera el campus de UNIFRANZ La Paz: las señales se apagan, los accesos se bloquean, la información se desordena y aparecen criaturas llamadas **bugs**. El **Lobo Guardián** recorre la universidad zona por zona para recuperar la información perdida y restaurar cada espacio.

El juego acompaña a los estudiantes de primer semestre mientras conocen los espacios y servicios del campus: acceso principal, lobby, oficinas, laboratorios, Ingeniería de Sistemas, aulas, biblioteca, terraza y clínica odontológica.

**En cada nivel el jugador:**
1. Habla con el personaje del lugar, que explica para qué sirve ese espacio.
2. Recoge los **3 fragmentos de información** mientras evita o derrota a los bugs.
3. Activa el **checkpoint**.
4. Resuelve la **terminal**, una pregunta de orientación sobre el espacio del nivel.
5. Derrota al **jefe** en los niveles 6, 8 y 9.
6. Cruza el **portal** para restaurar la zona y desbloquear la siguiente.

**Sistemas principales:** 3 corazones de vida, daño con invulnerabilidad breve, Game Over con reintento desde el checkpoint, cuatro poderes desbloqueables (Dash, Ráfaga, Escudo y Corte de energía), personalización del lobo y guardado automático del avance.

---

<a id="requisitos"></a>
## ✅ Requisitos de la evaluación

| Requisito | Cómo se cumple |
|---|---|
| **Enlace al repositorio GitHub** | Este repositorio. La rama `main` contiene la versión entregada |
| **Ejecutable para Windows** | Ver [Descargas](#descargas) |
| **APK para Android** | Ver [Descargas](#descargas) |
| **Completamente jugable** | Los 9 niveles y el nivel extra se completan de principio a fin: fragmentos, terminal, jefes, portal, victoria, Game Over y reintento |
| **Introducción del contexto** | Al entrar al Nivel 1 se muestra una **introducción narrativa** de 4 pantallas: primer día en el campus, la anomalía digital, el Lobo Guardián y la misión. Se puede saltar con Esc / B |
| **Tutorial básico** | *Ayuda y controles → Cómo jugar* tiene un **tutorial animado** de movimiento, carrera, salto, ataque, poder, interacción y objetivo, adaptado a teclado, mando o pantalla táctil. Durante el juego aparecen mensajes contextuales (checkpoint, portal bloqueado, terminal) |
| **Al menos 3 niveles funcionales** | **9 niveles + 1 nivel extra** |
| **Créditos de recursos externos** | Ver [Créditos y atribuciones](#creditos) |

---

<a id="controles"></a>
## 🕹️ Controles

| Acción | Teclado | Mando | Táctil |
|---|---|---|---|
| Mover | A / D | Stick izquierdo / cruceta | Botones ◀ ▶ o arrastrar a la izquierda |
| Correr | Shift izquierdo | Gatillo derecho | Botón Correr o arrastre largo |
| Saltar | Espacio | A | Botón Saltar o deslizar hacia arriba |
| Atacar | J | X | Botón Atacar o toque a la derecha |
| Interactuar | E | Y | Botón Usar |
| Poder | Q | Bumper derecho | Botón Poder o deslizar lateral |
| Pausa | Esc | Start | Botón Pausa |
| Menús | Flechas o W/S, Enter, Esc | Cruceta, A, B | Toque |

Las teclas y botones se pueden reasignar en **Ayuda y controles**. En Android se elige entre **Botones** y **Gestos** desde *Ayuda → Versión móvil*.

---

<a id="niveles"></a>
## 🗺️ Niveles

| N.º | Zona del campus | Personaje | Jefe |
|---|---|---|---|
| 1 | Exterior / acceso principal | Guardia U | — |
| 2 | Planta baja / lobby | Recepcionista | — |
| 3 | Mezanine / conexión (ARCA, Cajas, Dirección) | Director | — |
| 4 | Piso 1 / laboratorios de salud | Docente | — |
| 5 | Piso 2 / Ingeniería de Sistemas | Noemí Moller | — |
| 6 | Piso 4 / aulas y pasillos | Director | Secuencias |
| 7 | Piso 5 / Biblioteca e Innova | Unifranzito | — |
| 8 | Terraza / lounge / solarium | Estudiante | IF_ELSE |
| 9 | Clínica odontológica / núcleo final | Noemí Moller | Core Glitch |
| Extra | Subsuelo de anatomía (Osteoteca); se desbloquea al completar el Nivel 5 | Docente | — |

Al completar un nivel se desbloquea el siguiente en el **Mapa del campus**. Al vencer a *Core Glitch* se muestran el final y los créditos.

---

<a id="equipo"></a>
## 👥 Equipo CAMPUS GLITCH

| Integrante | GitHub | Área principal | Aportes |
|---|---|---|---|
| **Víctor Grover Asturizaga Plata** | `Developer-vic1` | Programación, arte y escenarios | Escenarios y ambientación de los 10 niveles, plataformas y fragmentos accesibles, salto y física; menú, mapa, configuración, controles y remapeo, ayuda y tutorial, créditos, personalización y poderes; versión móvil final (botones, gestos y exportación del APK); filtros de física para Unity 6 |
| **Carla Valeria Encinas Cano** | `VarlaCdev20` | Interfaz, animación y sonido | Menú principal inicial; terminal con teclado y mando; pantallas de victoria y final; borrado seguro del progreso; control de volumen; transiciones de animación del lobo; efectos de sonido con subtítulos; pruebas de integración |
| **Marcelo Josué Escobar Chipana** | `Josue-EC16` | Derrota y reintento | Pantalla Game Over del prototipo; sistema de derrota del juego final con REINTENTAR / MENÚ, reintento desde el checkpoint con vida completa, pérdida de vida por caída y bloqueo de entrada durante la derrota |
| **Vicente Oscar Claros Mamani** | `vicentesystemdev` | Liderazgo, integración y control técnico | Coordinación del equipo y flujo Gitflow; integración y revisión de PR; QA de la versión candidata; controles táctiles iniciales y configuración Android; introducción narrativa; documentación y compilación de Windows |

La autoría de cada cambio se puede consultar en el historial de commits y en los Pull Requests.

**Pull Requests principales:**

| PR | Contenido | Autor |
|---|---|---|
| #27 | Terminal, victoria, borrado seguro, volumen, animación, sonidos, subtítulos y QA | Carla |
| #28 | Game Over y reintento desde el checkpoint | Marcelo |
| #29 | Corrección de fragmentos inaccesibles | Víctor |
| #30 | Salto y plataformas | Víctor |
| #31 | Menú, mapa, configuración, controles, ayuda, créditos, personalización, poderes, escenarios y versión móvil | Víctor |
| #32 / #33 | Integración y QA de la candidata MVP | Vicente |
| #34 | Filtros de física para Unity 6 | Víctor |
| #35 | Introducción narrativa del juego | Vicente |

---

<a id="creditos"></a>
## 📜 Créditos y atribuciones

### Código
- **Desarrollo:** Equipo CAMPUS GLITCH (ver [Equipo](#equipo)).
- **Repositorio de origen:** el juego se inició en el repositorio grupal [`vicentesystemdev/CAMPUS_GLITCH`](https://github.com/vicentesystemdev/CAMPUS_GLITCH), del mismo equipo, y se integró en este repositorio en el commit `8c48d0a`. **No se usó código de repositorios públicos de terceros.**

### Arte y sprites
- Personaje, enemigos, jefes, personajes del campus, escenarios, objetos e interfaz: **generados con inteligencia artificial** y luego editados, ajustados e integrados por el equipo en formato pixel art (archivos Aseprite).
- Retratos de la pantalla de créditos: **ilustraciones generadas con IA a partir de fotografías** aportadas por cada persona retratada.

### Audio
- Música y efectos (`Music`, `Click`, `Jump`, `Attack`, `Damage`, `Pickup`, `Checkpoint`, `Win`, `Denied`, en `Assets/CampusGlitch/Resources/CG/Sound/`): **generados con inteligencia artificial** para este proyecto.
- Sonidos de navegación del menú (`Menu_Navegar`, `Menu_Confirmar`): **sintetizados por código** en `CGAudio.cs`.

### Tipografías
[Audiowide](https://fonts.google.com/specimen/Audiowide), [Barlow Condensed](https://fonts.google.com/specimen/Barlow+Condensed) y [Questrial](https://fonts.google.com/specimen/Questrial), de Google Fonts, con licencia **SIL Open Font License 1.1**. Los archivos de licencia están en `Assets/CampusGlitch/Resources/CG/Typography/`.

### Inteligencia artificial
Durante el desarrollo se usaron estas herramientas:
- **Claude** (Anthropic) y **Codex** (OpenAI): apoyo en programación, revisión de código, pruebas automatizadas y documentación.
- **Herramientas de generación de imágenes y audio con IA**: arte, sprites, retratos y sonidos, como se indica arriba.

Todo el contenido generado con IA fue revisado, editado e integrado por el equipo.

### Herramientas
Unity 6000.3.16f1 (URP 2D, Input System), C#, Aseprite, Git y GitHub. Estas herramientas pertenecen a sus respectivos titulares.

### Agradecimientos especiales
- **Ing. Noemí Moller Chuquimia**: Directora de la carrera de Ingeniería de Sistemas.
- **Ing. Rubén Soria**: Docente de Programación Multimedia I.
- **Universidad Privada Franz Tamayo (UNIFRANZ) La Paz.**

---

<a id="proyecto"></a>
## 🚀 Abrir el proyecto

**Requisitos:** Unity Hub, **Unity 6000.3.16f1** con *Windows Build Support* (y *Android Build Support* para el APK), y Git.

```bash
git clone https://github.com/vicentesystemdev/EVALUACION_MULTIMEDIA_2026.git
```

1. En Unity Hub: **Add project from disk**, seleccionar la carpeta del repositorio.
2. Abrir `Assets/CampusGlitch/Scenes/SC_Bootstrap.unity` y pulsar **Play**.
3. **Windows:** menú *Campus Glitch → Crear build Windows x64* (se genera en `Build_Windows/`).
4. **Android:** menú *Campus Glitch → Android → Generar APK*.

> Los niveles se construyen por código al iniciar (`CGWorld.cs`); la vista previa del editor es solo decorativa.

### Tecnologías

| Tecnología | Uso |
|---|---|
| Unity 6000.3.16f1 · URP 2D | Motor, física 2D, iluminación 2D y compilación |
| C# | 46 scripts de juego y 34 herramientas de editor |
| Unity Input System 1.19 | Teclado, mando y pantalla táctil |
| JSON (`JsonUtility`) | Guardado del avance y la configuración |
| Unity Test Framework / scripts de editor | Pruebas automatizadas de niveles y sistemas |

### Estructura

```text
Assets/CampusGlitch/
├── Scenes/      # 18 escenas: arranque, menús, 9 niveles y extra
├── Scripts/     # Juego: CGGame, CGWorld, CGEntities, CGIntroUI, CGAudio, CGTouch...
├── Editor/      # Compilación Windows/Android y pruebas automatizadas
└── Resources/CG # Sprites, fondos, sonidos y tipografías
Docs/            # GDD, QA, informes y protocolo del equipo
```

### Organización del repositorio
- **Gitflow:** `main` (versión entregada) ← `develop` (integración) ← ramas `feature/`, `fix/` y `docs/` por tarea.
- Todo cambio entra por **Pull Request** con revisión y *merge commit*, para conservar la autoría de cada integrante.
- `main` y `develop` están protegidas: requieren PR y no permiten *force push*.
- Detalle: [`Docs/GITFLOW_EQUIPO.md`](Docs/GITFLOW_EQUIPO.md).

### Documentación
- Pruebas de la versión candidata: [`Docs/QA/MVP_DOCENTE_20261112/`](Docs/QA/MVP_DOCENTE_20261112/INFORME_MVP_DOCENTE_2026-11-12.md). Los 10 niveles se completan en recorridos automatizados.
- Documento de diseño (GDD): [`Docs/GDD/`](Docs/GDD/).
- Exportación Android: [`Docs/Evaluacion_Dia2/Android/`](Docs/Evaluacion_Dia2/Android/Informe_Exportacion_Android.md).

---

<div align="center">
<strong>GRACIAS POR JUGAR CAMPUS GLITCH</strong><br>
<sub>Equipo CAMPUS GLITCH · Programación Multimedia I · UNIFRANZ · La Paz, Bolivia · 2026</sub><br>
<a href="#inicio">⬆️ Volver al inicio</a>
</div>
