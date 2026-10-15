# Prefabs gráficos compartidos

Disponible: `FondoCampus.prefab`. Los paneles, botones, mensajes y HUD
se prepararán en la siguiente parte.

## FondoCampus

1. Arrastrar el prefab bajo un Canvas y colocarlo como primer hijo.
2. El RectTransform raíz se estira al contenedor, con offsets cero.
3. `ImagenFondo` utiliza `MenuBackdrop.png` y un AspectRatioFitter
   Envelope Parent de 16:9: llena el área sin deformar; recorta bordes cuando cambia la proporción.
4. RectMask2D limita el fondo al contenedor. Sus imágenes tienen Raycast Target desactivado.
5. `VeloLegibilidad` empieza con alfa cero. Carla o Marcelo pueden oscurecer
   el fondo en su instancia, manteniendo títulos y botones por encima.

Configuración de Canvas recomendada para las futuras referencias:
CanvasScaler → Scale With Screen Size, referencia 1280 × 720,
Screen Match Mode Expand y Reference Pixels Per Unit 32. El prefab también
puede estirarse dentro de otro contenedor UI.

Comprobado en Unity en 1280 × 720 y 1024 × 768. Mantener textos y controles
en objetos separados: parte del arte lateral queda fuera en 4:3.

Los prefabs referencian los PNG existentes, sin copiarlos. Los futuros botones
quedarán sin acciones de navegación. Cada responsable los integrará en su escena:
Carla → MenuPrincipal; Marcelo → GameOver; Vicente → Nivel01.

Consultar `Docs/MAPA_CARPETAS_RECURSOS_VICTOR.md` en la raíz del repositorio.
