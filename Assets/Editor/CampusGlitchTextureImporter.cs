// Put inside Assets/Editor/CampusGlitchTextureImporter.cs
// Applies 2D pixel art defaults automatically. Unity 6 6000.x, UnityEditor only.
using UnityEditor;
using UnityEngine;

public class CampusGlitchTextureImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/Art/PNG/") && !assetPath.Contains("/Art/Animaciones/")) return;
        TextureImporter imp = (TextureImporter)assetImporter;
        bool isNew = assetImporter.importSettingsMissing;
        bool isSharedUI = assetPath.Contains("/Global_UI/");
        // Recompiling a postprocessor causes Unity to revisit all textures.
        // Existing gameplay/animation textures belong to other integrations.
        if (!isNew && !isSharedUI) return;
        // Existing Multiple sprites have stable subasset IDs used by scenes/prefabs.
        // Only choose a mode for a newly imported texture; never discard its slices.
        if (isNew)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = assetPath.EndsWith("_spritesheet.png")
                ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 32;
        }
        // UI uses Canvas referencePixelsPerUnit = 32. Other existing assets keep
        // their scale, pivots and borders to avoid changing teammates' layouts.
        if (isSharedUI) imp.spritePixelsPerUnit = 32;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.wrapMode = TextureWrapMode.Clamp;
    }
}
