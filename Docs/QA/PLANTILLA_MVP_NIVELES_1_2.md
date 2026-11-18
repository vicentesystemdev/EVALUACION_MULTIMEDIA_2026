# QA de aceptación — MVP nivel 1 y nivel 2

**Versión:** [SHA de develop/release]  **Unity:** 6000.3.16f1  **Responsable:** [nombre]  **Fecha:** [ISO]  **Plataforma:** [Windows Editor / Windows exe / Android real]

| ID | Prueba | Resultado Pasa/Falla/No ejecutado | Evidencia / Issue |
|---|---|---|---|
| M01 | Splash, menú, selección N1 y navegación con teclado | | |
| M02 | Movimiento/carrera y salto; plataformas realmente alcanzables | | |
| M03 | Mordida y enemigo N1 / N2 | | |
| M04 | 3 fragmentos recolectables N1, incluido el segundo | | |
| M05 | 3 fragmentos recolectables N2 | | |
| M06 | Terminal/quizz navegable con teclado y sin ratón | | |
| M07 | Portal y victoria; siguiente nivel y volver al menú | | |
| M08 | Progreso se conserva al CANCELAR «Borrar progreso» | | |
| M09 | Borrado explícito solo tras confirmar | | |
| M10 | Vida y daño; Game Over a cero corazones | | |
| M11 | Reintentar, menú, caída y checkpoint | | |
| M12 | Guardar/cerrar/reabrir y desbloqueo de N2 | | |
| M13 | Pausa, audio, feedback visual y HUD | | |
| M14 | Señalética e información de orientación antes del quiz | | |
| M15 | Windows exe con render/cámara, controles y resolución | | |
| M16 | Android APK real: mover+saltar/correr/habilidad/quiz/safe area | | |

**Regla:** No marcar "Pasa" sin ejecutar esa prueba concreta. `BUG_*` de auditoría pasa cuando el bug existe: sustituir por regresión positiva después del fix. Adjuntar capturas/logs pertinentes en PR o ruta documental liviana, no subir binarios pesados innecesariamente.
