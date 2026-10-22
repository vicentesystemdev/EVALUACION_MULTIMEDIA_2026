# Campus Glitch – Exportación Android (Evaluación Día 2)

**Proyecto:** CAMPUS_GLITCH · **Unity:** 6000.3.16f1 · **Tipo:** videojuego 2D (URP 2D, Input System) · **Rama:** `feature/evaluacion-dia2-builds`
**Responsable Android:** Vicente Oscar Claros Mamani (la exportación Windows la hace otro integrante del equipo y no se incluye aquí).

## 1. Objetivo
Generar un APK funcional de *Campus Glitch* jugable en un teléfono Android sin teclado ni mando, y documentar el proceso con evidencias reales.

## 2. Configuración utilizada
| Parámetro | Valor | Dónde se aplica |
|---|---|---|
| Nombre de la app | Campus Glitch | Solo durante la build del APK (`productName` es compartido con Windows y se restaura) |
| Identificador | `com.unifranz.campusglitch` | Player Settings > Android > Identification |
| Formato | APK (Build App Bundle desactivado) | `EditorUserBuildSettings.buildAppBundle = false` |
| Backend de scripting | IL2CPP | Player Settings > Android > Configuration |
| Arquitectura | ARM64 | Target Architectures |
| Orientación | Horizontal (Landscape Left/Right, autorrotación solo horizontal) | Resolution and Presentation |
| Min SDK | 25 (Android 7.1), valor existente del proyecto | Other Settings |
| Escenas | 18 escenas, `SC_Bootstrap` primero | Build Profiles > Scene List |

Toda la configuración se aplica con el menú **Campus Glitch > Android > 1. Configurar Android** (`Assets/CampusGlitch/Editor/CGAndroidBuild.cs`).

## 3. Adaptación de controles táctiles
El juego solo leía teclado y mando (`CGControl`). Se añadió `CGTouch.cs`, que lee el `Touchscreen` del **Input System** y solo se activa en plataformas móviles (`Application.isMobilePlatform`), así que en Windows no cambia nada.

- **IZQ / DER:** movimiento continuo mientras se mantienen pulsados (multitáctil: se puede saltar a la vez).
- **SALTAR, ATACAR, USAR (interactuar):** se integran en `CGControl.Jump/Attack/Interact`, por lo que `CGHero` no se modificó.
- **PAUSA:** botón en la parte superior central; para reanudar se usa *CONTINUAR* del menú de pausa o el botón Atrás de Android.
- **Splash:** «TOCA LA PANTALLA PARA JUGAR».
- **Menús:** usan los botones IMGUI existentes (toque = clic). En la pantalla Controles se añadió «< VOLVER» en móvil.
- **Relación de aspecto:** en móvil la UI se escala de forma uniforme y centrada (sin estirarse en 19.5:9 / 20:9), y los botones se anclan al *safe area* del teléfono.

## 4. Proceso de exportación del APK
1. Instalar desde Unity Hub los módulos *Android Build Support*, *OpenJDK* y *Android SDK & NDK Tools* para 6000.3.16f1.
2. Abrir el proyecto en Unity y ejecutar **Campus Glitch > Android > 1. Configurar Android**.
3. Ejecutar **Campus Glitch > Android > 2. Generar APK**. Este paso valida las 18 escenas, cambia a Android, compila con IL2CPP y genera `Builds/Android/CampusGlitch.apk`, además de `Docs/Evaluacion_Dia2/Android/Reporte_Build_Android.txt`.
4. Instalar en el teléfono con `Tools/ANDROID_INSTALAR_APK.bat` (depuración USB) o copiando el APK al teléfono.

## 5. Problemas encontrados y soluciones
| Problema (detectado en la auditoría) | Solución |
|---|---|
| El Editor 6000.3.16f1 no tenía Android Build Support (`Data/PlaybackEngines` solo contenía Windows) | Instalación de módulos desde Unity Hub |
| No existían controles táctiles: sin teclado era imposible pasar del Splash o jugar | `CGTouch` integrado en `CGControl` y `CGGame` |
| La UI IMGUI se estiraba a 1280x720 en cualquier relación de aspecto | Escala uniforme centrada solo en móvil |
| La pantalla Controles no tenía botón de salida y el reasignado de teclas esperaba un teclado | Botón «VOLVER» en móvil; un toque cancela el reasignado |
| Orientación en autorrotación con vertical permitido; sin identificador Android | Solo horizontal; `com.unifranz.campusglitch` |
| `productName` = `CAMPUS_GLITCH_TEMP` (compartido con Windows) | El APK usa «Campus Glitch» sin alterar la configuración de Windows |
| _[PENDIENTE: errores reales que aparezcan al compilar el APK]_ | _[PENDIENTE]_ |

## 6. Resultados de las pruebas
| Prueba | Resultado |
|---|---|
| Compilación de scripts en el Editor | _[PENDIENTE]_ |
| Generación del APK | _[PENDIENTE – ver Reporte_Build_Android.txt]_ |
| Instalación en teléfono (modelo / versión Android) | _[PENDIENTE]_ |
| Splash → menú con un toque | _[PENDIENTE]_ |
| Menú principal y mapa por toque | _[PENDIENTE]_ |
| Movimiento IZQ/DER mantenido, salto, ataque, interacción | _[PENDIENTE]_ |
| Pausa / continuar / botón Atrás | _[PENDIENTE]_ |
| Orientación horizontal y legibilidad | _[PENDIENTE]_ |

## 7. Evidencias
| # | Evidencia | Archivo |
|---|---|---|
| 1 | Configuración Android en Unity | ![](Evidencias/01_Configuracion_Android.png) _[PENDIENTE]_ |
| 2 | Escenas en Build Profiles | ![](Evidencias/02_Escenas_Build_Profiles.png) _[PENDIENTE]_ |
| 3 | Orientación horizontal | ![](Evidencias/03_Orientacion_Landscape.png) _[PENDIENTE]_ |
| 4 | Compilación exitosa | ![](Evidencias/04_Build_Exitoso.png) _[PENDIENTE]_ |
| 5 | APK generado | ![](Evidencias/05_APK_Generado.png) _[PENDIENTE]_ |
| 6 | Juego instalado en el teléfono | ![](Evidencias/06_Juego_Instalado_Android.png) _[PENDIENTE]_ |
| 7 | Controles táctiles en juego | ![](Evidencias/07_Controles_Tactiles.png) _[PENDIENTE]_ |

## 8. Control de versiones
Rama `feature/evaluacion-dia2-builds`; los commits anteriores se conservan, sin push forzado. `Builds/` y `*.apk` están excluidos por `.gitignore`.
- _[Commit: controles táctiles Android]_
- _[Commit: configuración y exportación APK]_
- _[Commit: evidencias e informe]_

## 9. Conclusión
_[PENDIENTE: completar cuando el APK se haya generado y probado en el teléfono.]_
