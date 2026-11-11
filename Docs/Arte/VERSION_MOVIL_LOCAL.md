# Campus Glitch para Android — distribución local

El lateral de Ayuda incluye **Versión móvil**. La pantalla sigue tres pasos: generar la versión actual, guardar la APK donde se elija y enviarla al teléfono seleccionado. Conserva naranja, azul oscuro, blanco y cian. Las acciones aún no disponibles aparecen atenuadas y no reciben foco. El selector muestra los teléfonos encontrados y permite cancelar sin perder la elección anterior. Las operaciones de copia y conexión se realizan sin bloquear el menú.

## Uso en esta computadora

1. Guarda los cambios y abre Ayuda → Versión móvil → Generar / actualizar APK. Desde Unity también se puede usar Campus Glitch → Android → Generar APK. Si hay Play activo, termina antes de exportar. La exportación cambia temporalmente a Android y vuelve al destino anterior.
2. Cuando termine, pulsa Guardar APK en… para elegir carpeta y nombre. Cancelar no crea un archivo. Una copia incompleta o alterada se rechaza y no reemplaza el destino anterior.
3. Conecta el teléfono mediante un cable USB de datos. Activa la depuración USB y autoriza esta computadora desde el teléfono. Pulsa Detectar teléfonos USB, abre el selector y elige el equipo. Los estados pendiente y sin conexión no permiten enviar.
4. Enviar a Descargas copia `Campus Glitch.apk` a la carpeta Download del teléfono. Abre ese archivo en el teléfono para instalar o actualizar. La pantalla no instala aplicaciones silenciosamente ni elige un teléfono sin intervención.

La primera generación requiere el módulo Android de Unity 6000.3.16f1 con SDK, NDK y OpenJDK. El archivo generado está en `Builds/Android/CampusGlitch.apk`; la ficha de versión y el reporte también están allí. Una ficha privada de esta computadora permite localizar la última APK desde el juego de Windows. Las rutas de esta ficha, paquetes y firmas quedan fuera del repositorio. No hay publicaciones ni descargas en Internet.

## Actualizaciones del equipo

Cada APK conserva `com.unifranz.campusglitch`, el nombre visible **Campus Glitch**, su icono y un número de edición mayor. La ficha registra fecha, resumen del proyecto, revisión de origen y SHA-256 de la APK. La pantalla distingue APK actual y cambios pendientes. Comprueba la integridad antes de copiar o enviar.

Después de la primera exportación, Campus Glitch → Android → **Actualizar APK tras cada commit** está activado en esta computadora. Unity comprueba el nuevo commit, compara el contenido del proyecto y genera la actualización cuando está detenido, sin compilar y con la escena guardada. No inicia compilaciones durante Play o mientras se edita una escena sin guardar. Se puede desactivar esa opción desde el mismo menú. Los cambios todavía sin commit se señalan como pendientes y también pueden exportarse manualmente.

Las aportaciones de otras ramas llegan a la app cuando se integran en esta copia respetando el flujo de revisión del equipo. Este sistema actualiza la APK; no sincroniza partidas entre computadoras y teléfonos ni instala actualizaciones sin confirmación. Para actualizar conservando la partida se debe mantener identificador y firma, e instalar encima de la app anterior. La exportación local usa la firma configurada en esta computadora: conservarla. Una firma de distribución del equipo se configurará antes de publicar fuera de esta computadora; nunca subir claves ni contraseñas al repositorio.

## Controles del teléfono

En Ayuda → Versión móvil se puede elegir **Botones** o **Gestos**; la preferencia se guarda en ese dispositivo.

| Modo | Acciones |
|---|---|
| Botones | Izquierda, derecha, correr, saltar, atacar, poder, usar y pausa. Admite movimiento y acciones con dedos distintos. |
| Gestos | Arrastra en la mitad izquierda para mover; más distancia permite correr. En la mitad derecha, toca para atacar, desliza arriba para saltar y lateralmente para activar el poder. Usar y Pausa mantienen botones visibles. |

Los dedos conservan su acción al deslizarse; al perder foco o salir de una escena se limpia la entrada. Los controles respetan el área segura de pantalla. La interfaz conserva su proporción dentro del espacio disponible; Android se configura horizontal y permite ambos sentidos. El mando y teclado conectados siguen disponibles. Las teclas del escritorio no sustituyen los botones táctiles.

## Fuentes

- [Conexión, dispositivos y copia por USB — Android Developers](https://developer.android.com/tools/adb).
- [Firma y actualización de aplicaciones — Android Developers](https://developer.android.com/studio/publish/app-signing).
- [Entrada en Android — Unity 6000.3](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/developing/input).
- [Lectura táctil — Unity Input System](https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/touch-polling.md).
