# Lobo Guardián y campus nocturno

## Dirección visual

Referencias facilitadas para esta revisión: lobo antropomorfo con armadura grafito, melena de energía, hocico plateado, ojos ámbar, cola grande y marca U en el hombro. Campus urbano de La Paz al anochecer, montañas, vidrio, árboles rosados y luces cálidas. La composición mantiene lectura lateral y profundidad por capas; la simulación sigue siendo 2D.

El modelo acorazado sustituye la presentación del bípedo alternativo. Se conserva el modelo clásico seleccionable y su arte anterior. Rojo es la variante inicial; morado conserva el desbloqueo al completar N2. Las referencias son una dirección artística, no una ampliación de habilidades, monedas o requisitos de progreso.

## Recursos entregados

| Recurso | Resolución | Uso |
|---|---:|---|
| Guardian_Rojo.png | 1254 × 1254 RGBA | Atlas de ocho filas por ocho columnas |
| Guardian_Morado.png | 1254 × 1254 RGBA | Variante de la misma identidad y secuencias |
| GuardianFrames_*.json | 64 rectángulos por variante | Recorte por alfa, pivote horizontal estable y pies en el origen |
| CampusExterior.png | 1672 × 941 | Exterior y terraza, sin personajes ni HUD pintados |
| CampusInterior.png | 1672 × 941 | Arquitectura común interior; utilería contextual de cada nivel |
| Conceptos/LoboGuardian_Rojo.png | 1024 × 1536 RGBA | Referencia de volumen y materiales, fuera del build |

Orden de las filas: espera, caminar, correr, ascenso, caída/aterrizaje, ataque, daño y victoria. Ocho fotogramas por fila: **64 por variante, 128 en total**. El recorte conserva los PNG originales, sin recolorear ni recomprimir sus píxeles. Unity utiliza Sprite.Create con 128 PPU y una escala uniforme por variante; no redimensiona cada pose individualmente. La altura de espera es 1,88 unidades. La cola es visual: el collider mantiene 0,76 × 1,64 y su material sin fricción.

Importación: Point, sin mipmaps ni compresión, alfa transparente, máximo 2048. No se sustituyen GUID de los recursos anteriores. Los atlas nuevos no son archivos .aseprite y no se presentan como tales.

## Especificaciones de diseño empleadas

1. Personaje maestro: “Full-body right-facing armored anthropomorphic wolf guardian; charcoal and graphite armor, silver muzzle and cheek fur, amber eyes, crimson energy mane and tail edges, restrained orange seams, U shoulder emblem. Detailed HD pixel-art, readable silhouette, transparent background, no labels or interface.”
2. Atlas rojo: “Same character and proportions in a strict 8 by 8 animation grid. Rows: idle, walk, run, jump ascent, fall and landing, attack, hurt, victory. Eight right-facing poses per row, consistent scale and cell spacing, transparent background, no text or panel borders.”
3. Atlas morado: “Preserve all 64 poses and grid composition. Replace crimson/orange energy accents with violet and magenta, retain graphite armor, silver fur and amber eyes.”
4. Exterior: “Wide side-scrolling campus panorama at blue hour, La Paz mountain skyline, modern glass UNIFRANZ entrance, navy architecture, warm amber lighting, pink flowering trees, empty straight walking lane around 85 percent image height. HD pixel-art 2.5D, no actors, HUD or floating platforms.”
5. Interior: “Wide modern university lobby with glass atrium, navy structural columns, warm amber lighting, restrained violet accents, city and flowering trees through windows. Empty continuous foreground walking lane around 85 percent image height; no actors, text, interface or false platforms.”

## Lectura y límites

El HUD muestra vidas, fragmentos, terminal, checkpoint y posición reales. No muestra poderes inexistentes. Los fondos y retratos se dibujan por separado del escenario físico. Las plataformas elevadas conservan sus colliders sólidos y pasamanos unidos; se excluyen los props llamados Floor/Rail del mobiliario decorativo. El panorama no añade superficies físicas. Las especialidades interiores siguen diferenciadas por mobiliario y nombre del nivel; no hay diez fondos exclusivos nuevos.

La opción de reducir destellos conserva su efecto sobre las partículas existentes; las animaciones nuevas no añaden parpadeo de pantalla. La validación y capturas de ejecución se registran en el informe de QA de esta revisión.
