// Put inside Assets/Editor/CampusGlitchAnimations.cs
// Menu: Tools > Campus Glitch > Construir clips PNG
// Converts per-frame PNGs into animation clips (SpriteRenderer.sprite), no runtime dependencies.
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public static class CampusGlitchAnimations
{
    [MenuItem("Tools/Campus Glitch/Construir clips PNG")]
    public static void BuildClips()
    {
        const string root = "Assets/Art";
        if (!AssetDatabase.IsValidFolder(root))
        {
            Debug.LogWarning("No existe Assets/Art/Animaciones. Copia la carpeta Assets del ZIP dentro de tu proyecto.");
            return;
        }
        // Frame PNGs are separated in Art/Animaciones and Art/PNG/02_Animados.
        string[] dirs = Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
            .Where(d => d.Contains("/Animaciones/") || d.Contains("\\Animaciones\\") ||
                        d.Contains("/PNG/02_Animados/") || d.Contains("\\PNG\\02_Animados\\"))
            .ToArray();
        int made = 0;
        foreach (string dir in dirs)
        {
            var files = Directory.GetFiles(dir, "*.png", SearchOption.TopDirectoryOnly)
                .Where(x => !x.EndsWith("_spritesheet.png"))
                .OrderBy(x => x).ToArray();
            if (files.Length < 2) continue;
            // Folders are named IDLE, HABLAR, etc. For new sprites, group by prefix before _00.png.
            var groups = files.GroupBy(x => Path.GetFileNameWithoutExtension(x).Substring(0,
                Path.GetFileNameWithoutExtension(x).LastIndexOf('_')));
            foreach (var group in groups)
            {
                var sprites = group.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
                if (sprites.Any(s => s == null) || sprites.Length < 2) continue;
                var clip = new AnimationClip { frameRate = 8f };
                var curve = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
                var keyframes = sprites.Select((sp,i) => new ObjectReferenceKeyframe { time = i/8f, value = sp }).ToArray();
                AnimationUtility.SetObjectReferenceCurve(clip,curve,keyframes);
                string path = (dir + "/" + group.Key + ".anim").Replace("\\","/");
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(clip,path); made++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Campus Glitch: " + made + " clips de animación creados.");
    }
}
