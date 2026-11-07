using System;
using UnityEngine;

namespace CampusGlitch {
/// <summary>Eight authored animation strips, sliced without changing their source pixels.</summary>
public static class CGGuardianArt {
    [Serializable] public class Frame { public int x, y, w, h; public float pivotX; }
    [Serializable] class Atlas { public Frame[] frames; }
    static readonly Sprite[][] sprites = new Sprite[2][];
    static readonly float[] scales = new float[2];
    static readonly bool[] loaded = new bool[2];
    public static bool Available { get { return Get(false, 0, 0) != null; } }

    static void Load(bool purple) {
        int skin = purple ? 1 : 0;
        if (loaded[skin]) return;
        loaded[skin] = true;
        string name = purple ? "Morado" : "Rojo";
        var texture = Resources.Load<Texture2D>("CG/GuardianV2/Guardian_" + name);
        var data = Resources.Load<TextAsset>("CG/GuardianV2/GuardianFrames_" + name);
        if (texture == null || data == null) return;
        var atlas = JsonUtility.FromJson<Atlas>(data.text);
        if (atlas == null || atlas.frames == null || atlas.frames.Length != 64) return;
        foreach (var f in atlas.frames)
            if (f.w <= 0 || f.h <= 0 || f.x < 0 || f.y < 0 || f.x + f.w > texture.width || f.y + f.h > texture.height) return;
        sprites[skin] = new Sprite[64];
        for (int i = 0; i < 64; i++) {
            var f = atlas.frames[i];
            var sprite = Sprite.Create(texture, new Rect(f.x, f.y, f.w, f.h), new Vector2(f.pivotX / f.w, 0), 128, 0, SpriteMeshType.FullRect);
            sprite.name = "Guardian_" + name + "_" + (i / 8) + "_" + (i % 8);
            sprites[skin][i] = sprite;
        }
        // A single scale per skin preserves anticipation, squash and stride between frames.
        scales[skin] = 1.88f / sprites[skin][0].bounds.size.y;
    }
    public static Sprite Get(bool purple, int row, int frame) {
        Load(purple);
        var list = sprites[purple ? 1 : 0];
        return list == null ? null : list[Mathf.Clamp(row, 0, 7) * 8 + Mathf.Clamp(frame, 0, 7)];
    }
    public static float Scale(bool purple) { Load(purple); return scales[purple ? 1 : 0]; }
    public static Sprite Preview(bool purple, int oldFrame) {
        int row = oldFrame < 2 ? 0 : oldFrame < 6 ? 1 : oldFrame < 10 ? 2 : oldFrame < 13 ? 3 : oldFrame < 15 ? 4 : oldFrame < 19 ? 5 : 6;
        return Get(purple, row, oldFrame % 8);
    }
    public static void DrawFit(Rect area, Sprite sprite) {
        if (sprite == null) return;
        Rect uv = sprite.textureRect;
        float scale = Mathf.Min(area.width / uv.width, area.height / uv.height);
        Rect target = new Rect(area.center.x - uv.width * scale * .5f, area.center.y - uv.height * scale * .5f, uv.width * scale, uv.height * scale);
        GUI.DrawTextureWithTexCoords(target, sprite.texture, new Rect(uv.x / sprite.texture.width, uv.y / sprite.texture.height, uv.width / sprite.texture.width, uv.height / sprite.texture.height), true);
    }
}
}
