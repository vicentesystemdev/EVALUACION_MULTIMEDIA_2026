# Campus Glitch — aportes y verificación de Carla

**Responsable:** Carla (@VarlaCdev20)  
**Rama de trabajo:** `feature/carla`  
**Base inicial:** `origin/develop` @ `048b3e3`  
**Unity requerido:** `6000.3.16f1`  
**Destino del PR:** `develop`, sin integración por parte de Carla.

> La rama agrupa los Issues #20, #21, #22, #11 y #12 por solicitud de Carla. Es una excepción al protocolo común (una rama por Issue) que Vicente debe revisar y aceptar.

## Cambios de código por Issue

| Issue | Modificación | Ruta |
|---|---|---|
| #20 | Respetar la opción elegida al borrar progreso, reiniciar selección al abrir y usar una sola acción para teclado/ratón/mando | `Assets/CampusGlitch/Scripts/CGGame.cs` |
| #21 | Permitir navegar por tres respuestas y confirmar con teclado/mando, cerrar con Escape/B y botón visible, y mantener `Time.timeScale` coherente | `CGGame.cs` |
| #22 | Diferenciar Siguiente nivel de Menú principal; distinguir Créditos de Volver al menú | `CGGame.cs` |
| #11 | Ajustar el número de opciones navegables por pantalla, permitir seleccionar el modo de controles sin ratón, actualizar el foco al hacer clic y evitar estiramiento de UI en formatos distintos de 16:9 | `CGGame.cs` |
| #11 | Limitar el volumen general al intervalo 0–100 % | `CGGame.cs` |
| #12 | Reiniciar fotogramas en cambios de estado del lobo y dar feedback visual al recibir daño (respetando reducir destellos) | `CGEntities.cs` |
| #12 | Reproducir el sonido de selección disponible al activar botones de menú | `CGGame.cs` |

La regresión en `Assets/CampusGlitch/Editor/CGMenuRegression.cs` expone tres comandos del menú del editor **Campus Glitch > QA**: Comprobar borrado de progreso, Comprobar navegacion de terminal y Comprobar victoria y creditos. Es una prueba positiva de lógica de selección con delegados simulados, **no** una prueba de guardado real ni una ejecución de Play Mode.

## Evidencia realmente obtenida

- Revisión del código fuente de `develop`, del Issue #20, y de las instrucciones de equipo.
- Comprobación estática de delimitadores `{}`, `()` y `[]` balanceados en `CGGame.cs`, `CGEntities.cs` y `CGMenuRegression.cs`.
- **No se ha compilado ni abierto Unity en esta sesión.** No hay capturas, logs de consola, APK, vídeo ni verificación real de guardado.
- **Los comandos de QA del editor existen pero aún no se ejecutaron.** Sus mensajes de éxito no se presentan como evidencia.

## Pruebas pendientes para aceptar el PR

1. **Respaldo obligatorio:** seguir `Docs/QA/QA_PR_ISSUES_13_18_20_2026-11-06.md` §1.3 antes de tocar el JSON real de progreso. Los worktrees comparten `Application.persistentDataPath`. Restaurar con hashes y comprobar el mapa al terminar.
2. **#20:** ejecutar R1, R2, R4, R5, R6, R7 y R8 en PC; si hay mando, R3 y R4b; si hay APK, R9. Verificar que reiniciar la aplicación después de cancelar mantiene los desbloqueos. Ejecutar el comando de QA correspondiente en el Editor.
3. **#21:** resolver terminales en N1 y N2 con teclado, probar 3 respuestas con flechas/W/S y Enter; repetir con mando si está disponible. Probar error, respuesta correcta, Escape/B, clic/tacto sobre CERRAR TERMINAL, continuidad del personaje y `Time.timeScale=1`. Probar N8 y jefe si la progresión lo permite.
4. **#22:** en victoria, confirmar primero SIGUIENTE NIVEL y luego MENÚ PRINCIPAL con teclado y ratón; comprobar menú, desbloqueos y créditos después del nivel final con teclado/mando. Ejecutar el comando de QA de victoria y créditos.
5. **#11:** recorrer menú, controles, ajustes, ayuda, créditos y mapa con teclado y ratón. Comprobar 1280x720, 1280x1024 y móvil horizontal; verificar que el modo de controles se activa con teclado y que el volumen no supera 100 %.
6. **#12:** observar idle, caminar, correr, salto, caída, ataque y daño con lobo original y alternativo; confirmar tintado de daño, opción de reducir destellos y sonido de selección por ratón y teclado. Registrar capturas/logs y cualquier recurso ausente.
7. Confirmar consola sin errores nuevos y Play Mode sin bloqueos. Repetir los casos críticos después de integrar los cambios compartidos con Marcelo.

**Estado de las pruebas en Unity:** NO EJECUTADAS. Su ejecución y resultado deben quedar en el PR antes del merge.

## Revisión educativa de N1 y N2

El código actual identifica N1 como «Exterior / acceso principal» y N2 como «Planta baja / lobby», con preguntas breves sobre una credencial y la recepción. Las prioridades del MVP indican orientación del campus para N1 y vida universitaria para N2, con información explicativa antes de evaluar.

**Pendiente de validar con equipo/docente:** comprobar que exigir una credencial para entrar representa una regla institucional real; que la ubicación y el rol de recepción son correctos; y que N2 enseña hábitos de adaptación académica además de orientación espacial. No se han cambiado preguntas ni se ha inventado ningún servicio. El GDD original en Excel debe revisarse directamente por el equipo antes de aprobar cambios de contenido.

## Riesgos e integración

- `CGGame.cs` también es necesario para Marcelo (Issue #13). Requiere revisar conflicto de líneas y repetir QA después de integrar sus cambios.
- Se utilizó una sola rama para cinco Issues por elección de Carla; requiere conformidad de Vicente por apartarse del protocolo habitual.
- No se alteraron escenas `.unity`, prefabs, `ProjectSettings`, recursos gráficos ni el archivo original GDD.
- El script de QA se incluyó con su archivo `.meta` para conservar el GUID.
- La regresión de lógica no sustituye pruebas reales de entrada, Play Mode, guardado ni hardware móvil.

**Solicitud:** revisión técnica y QA por Vicente, sin merge hasta comprobar las pruebas y resolver observaciones.
