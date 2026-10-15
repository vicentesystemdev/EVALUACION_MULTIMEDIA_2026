# Fondos compartidos

Disponible: `MenuBackdrop.png`, fondo pixel art de 1280 × 720 recuperado de
CAMPUS_GLITCH con su `.meta` y GUID originales. Importado en Unity como Sprite
Single, Point, 32 PPU, sin compresión ni mipmaps.

Carla y Marcelo reutilizan el mismo recurso desde sus escenas oficiales;
no crear una copia por pantalla.

Para utilizarlo bajo un Canvas, arrastrar
`Assets/Prefabs/UI/Compartidos/FondoCampus.prefab` y colocarlo primero entre
los hijos, detrás de títulos y botones. El prefab cubre su contenedor y
conserva la proporción: en pantallas 4:3 recorta los bordes laterales.
Mantener los controles y el título en objetos separados del fondo.

El hijo `VeloLegibilidad` empieza transparente. Ajustar su alfa para oscurecer
el fondo en Game Over, sin modificar la imagen compartida.

Consultar `Docs/MAPA_CARPETAS_RECURSOS_VICTOR.md` en la raíz del repositorio.
