#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.Editor {
public static class CGGuardianArtRegression {
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    [MenuItem("Campus Glitch/QA/Verificar arte del Guardian")]
    public static void RunAll() {
        Run(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CampusGlitchQA", "Guardian_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")));
    }
    public static void Run(string directory) {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new InvalidOperationException("Detener Play y esperar la compilación.");
        string output = Path.GetFullPath(directory);
        if (Directory.Exists(output) || output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Elegir una carpeta nueva fuera del proyecto.");
        Directory.CreateDirectory(output);
        int pass = 0, fail = 0;
        Action<bool, string> check = (ok, label) => { if (ok) pass++; else fail++; File.AppendAllText(Path.Combine(output, "results.txt"), (ok ? "PASS " : "FAIL ") + label + "\n"); };
        CGProgress original = CGStore.Data;
        GameObject fixture = null;
        try {
            foreach (bool purple in new[] { false, true }) {
                bool valid = true, anchored = true;
                for (int row = 0; row < 8; row++) for (int frame = 0; frame < 8; frame++) {
                    Sprite sprite = CGGuardianArt.Get(purple, row, frame);
                    valid &= sprite != null && sprite.textureRect.width > 0 && sprite.textureRect.height > 0 && sprite.textureRect.width < sprite.texture.width / 4f;
                    anchored &= sprite != null && Mathf.Abs(sprite.bounds.min.y) < .0001f;
                }
                check(valid, (purple ? "Morado" : "Rojo") + ": 64 frames independientes, retratos limitados a su rectángulo");
                check(anchored, "Pivotes de pies sin desplazamiento vertical entre los 64 recortes");
                var idle = CGGuardianArt.Get(purple, 0, 0);
                check(idle != null && Mathf.Abs(idle.bounds.size.y * CGGuardianArt.Scale(purple) - 1.88f) < .001f, "Altura de espera 1.88; escala uniforme por variante");
            }
            bool propsValid=true;
            for(int i=0;i<16;i++){var prop=CGCampusVisual.Object(i);propsValid&=prop!=null&&prop.textureRect.width>0&&prop.textureRect.height>0;}
            check(propsValid,"16 objetos del campus independientes, incluidos suelo y plataforma");
            foreach (string file in new[] { "GuardianV2/Guardian_Rojo", "GuardianV2/Guardian_Morado", "VisualV2/CampusExterior", "VisualV2/CampusInterior", "VisualV2/CampusObjects" }) {
                var importer = AssetImporter.GetAtPath("Assets/CampusGlitch/Resources/CG/" + file + ".png") as TextureImporter;
                check(importer != null && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.alphaIsTransparency, "Importación pixel art: " + file);
            }
            CGStore.Data = new CGProgress();
            fixture = new GameObject("GuardianArtFixture") { hideFlags = HideFlags.HideAndDontSave };
            var game = fixture.AddComponent<CGGame>(); game.enabled = false;
            var player = new GameObject("Hero"); player.transform.SetParent(fixture.transform);
            var hero = player.AddComponent<CGHero>(); hero.Setup(game); hero.enabled = false;
            var collider = hero.GetComponent<CapsuleCollider2D>();
            var visual = hero.GetComponent<CGGuardianVisual>();
            var view = hero.GetComponentInChildren<SpriteRenderer>();
            Action render = () => typeof(CGGuardianVisual).GetMethod("LateUpdate", Private).Invoke(visual, null);
            hero.Grounded = true; render();
            check(view.sprite == CGGuardianArt.Get(false, 0, 0) && view.transform.localPosition == Vector3.zero, "Espera: pies en el origen del controlador");
            hero.moveInput = 1; render(); check(view.sprite == CGGuardianArt.Get(false, 1, 0), "Movimiento parado a caminar");
            hero.running = true; render(); check(view.sprite == CGGuardianArt.Get(false, 2, 0), "Caminar a correr");
            hero.Grounded = false; hero.Body.linearVelocity = Vector2.up; render(); check(view.sprite == CGGuardianArt.Get(false, 3, 0), "Ascenso del salto real");
            hero.Body.linearVelocity = Vector2.down; render(); check(view.sprite == CGGuardianArt.Get(false, 4, 0), "Caída del salto real");
            hero.attackUntil = Time.time + 1; render(); check(view.sprite == CGGuardianArt.Get(false, 5, 0), "Respuesta visual al ataque");
            hero.hurtUntil = Time.time + 1; render(); check(view.sprite == CGGuardianArt.Get(false, 6, 0), "Daño tiene prioridad sobre ataque");
            visual.Celebrate(); render(); check(view.sprite == CGGuardianArt.Get(false, 7, 0), "Victoria");
            hero.facesRight = false; CGStore.Data.purpleEquipped = true; render();
            check(view.flipX && view.sprite == CGGuardianArt.Get(true, 7, 0), "Cambio de dirección y variante morada");
            CGStore.Data.useOriginalWolf = true;
            hero.GetComponent<CGFrameAnimator>().Update();
            check(view.sprite != null && view.sprite.name.StartsWith("Classic"), "Modelo clásico continúa disponible");
            check(collider.size == new Vector2(.76f, 1.64f) && collider.offset == new Vector2(0, .82f) && collider.sharedMaterial.friction == 0 && hero.Body.gravityScale == 3.2f && hero.transform.localScale == Vector3.one, "Cambiar arte no altera cápsula, fricción, gravedad ni escala física");
        } catch (Exception ex) { check(false, ex.ToString()); }
        finally { if (fixture != null) UnityEngine.Object.DestroyImmediate(fixture); CGStore.Data = original; }
        File.AppendAllText(Path.Combine(output, "results.txt"), "TOTAL PASS=" + pass + " FAIL=" + fail + "\n");
        Debug.Log("GuardianArt PASS=" + pass + " FAIL=" + fail + " | " + output);
    }
}
}
#endif
