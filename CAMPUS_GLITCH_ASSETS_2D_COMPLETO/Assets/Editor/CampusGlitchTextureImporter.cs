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
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 32;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.textureWrapMode = TextureWrapMode.Clamp;
        imp.spritePivot = new Vector2(0.5f, 0.5f);
    }
}
