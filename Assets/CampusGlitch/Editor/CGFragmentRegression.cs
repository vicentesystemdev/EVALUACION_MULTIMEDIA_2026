#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
// Positive physics checks for #18. Teleports isolate each approach; they are not a full playthrough.
[InitializeOnLoad]
public static class CGFragmentRegression {
    const string Pending = "CampusGlitch.FragmentRegression.Output";
    const string PreviousScene = "CampusGlitch.FragmentRegression.Scene";
    static readonly string[] SaveNames = { "campus_glitch_save_v1.json", "campus_glitch_save_v1.json.bak", "campus_glitch_save_v1.json.tmp" };
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static Keyboard keyboard;
    static InputSettings.BackgroundBehavior background;
    static string output;
    static int passed, failed;
    static bool collected;

    static CGFragmentRegression() { EditorApplication.playModeStateChanged += OnPlayMode; }

    [MenuItem("Campus Glitch/QA/Verificar fragmentos (Issue 18)")]
    public static void RunAll() {
        Run(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CampusGlitchQA", "Issue18_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")));
    }

    public static void Run(string directory) {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Detener Play y esperar la compilacion antes de ejecutar la prueba.");
        if (Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Guardar los cambios de escena antes de ejecutar la prueba.");
        output = Path.GetFullPath(directory);
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
        if (output.StartsWith(project, StringComparison.OrdinalIgnoreCase) || Directory.Exists(output))
            throw new InvalidOperationException("Elegir una carpeta nueva fuera del proyecto para evidencias y respaldo.");
        Directory.CreateDirectory(output);
        foreach (string name in SaveNames) {
            string source = Path.Combine(Application.persistentDataPath, name);
            if (File.Exists(source)) File.Copy(source, Path.Combine(output, name));
        }
        SessionState.SetString(PreviousScene, SceneManager.GetActiveScene().path);
        SessionState.SetString(Pending, output);
        File.WriteAllText(Path.Combine(output, "results.txt"), "Issue #18 | Unity " + Application.unityVersion + " | " + DateTime.Now.ToString("O") + "\n");
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Level01.unity");
        EditorApplication.isPlaying = true;
    }

    static void OnPlayMode(PlayModeStateChange state) {
        string directory = SessionState.GetString(Pending, "");
        if (string.IsNullOrEmpty(directory)) return;
        output = directory;
        if (state == PlayModeStateChange.EnteredPlayMode) {
            EditorApplication.delayCall += () => {
                CGGame game = UnityEngine.Object.FindFirstObjectByType<CGGame>();
                if (game == null) { Record(false, "CGGame presente en SC_Level01"); EditorApplication.isPlaying = false; return; }
                game.StartCoroutine(Guard(Suite(game)));
            };
        } else if (state == PlayModeStateChange.EnteredEditMode) {
            // Restore exact bytes, including absence, even when Play was interrupted.
            foreach (string name in SaveNames) {
                string saved = Path.Combine(output, name), target = Path.Combine(Application.persistentDataPath, name);
                if (File.Exists(saved)) { Directory.CreateDirectory(Application.persistentDataPath); File.Copy(saved, target, true); }
                else if (File.Exists(target)) File.Delete(target);
            }
            File.AppendAllText(Path.Combine(output, "results.txt"), "Guardado original restaurado.\n");
            SessionState.EraseString(Pending);
            string scene = SessionState.GetString(PreviousScene, "");
            if (!string.IsNullOrEmpty(scene)) EditorSceneManager.OpenScene(scene);
            SessionState.EraseString(PreviousScene);
        }
    }

    static IEnumerator Guard(IEnumerator suite) {
        passed = failed = 0;
        background = InputSystem.settings.backgroundBehavior;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        keyboard = InputSystem.AddDevice<Keyboard>();
        var stack = new Stack<IEnumerator>(); stack.Push(suite);
        bool broken = false;
        while (stack.Count > 0 && !broken) {
            object next = null; bool moved = false;
            try { moved = stack.Peek().MoveNext(); if (moved) next = stack.Peek().Current; }
            catch (Exception ex) { Record(false, "EXCEPCION " + ex); broken = true; }
            if (broken) break;
            if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
            if (next is IEnumerator nested) stack.Push(nested);
            else yield return next;
        }
        while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
        InputSystem.RemoveDevice(keyboard); keyboard = null;
        InputSystem.settings.backgroundBehavior = background;
        Time.timeScale = 1;
        File.AppendAllText(Path.Combine(output, "results.txt"), "TOTAL PASS=" + passed + " FAIL=" + failed + "\n");
        Debug.Log("[Issue18] PASS=" + passed + " FAIL=" + failed + " | " + output);
        EditorApplication.isPlaying = false;
    }

    static IEnumerator Suite(CGGame game) {
        yield return Wait(.3f);
        CGStore.Data = new CGProgress();
        yield return Wait(.05f, Key.D);
        Record(CGControl.Axis() > .9f, "Control del arnes: entrada D reconocida");
        yield return Wait(.05f);
        for (int level = 0; level < 10; level++) {
            for (int mode = 0; mode < 2; mode++) {
                CGWorld old = Get<CGWorld>(game, "world");
                if (old != null) UnityEngine.Object.Destroy(old.gameObject);
                yield return null;
                game.sceneLevel = level;
                Call(game, "CreateLevel");
                yield return Wait(.2f);
                CGWorld world = Get<CGWorld>(game, "world");
                // Local reachability isolates enemy damage. Combat is a separate check.
                foreach (CGEnemy enemy in world.root.GetComponentsInChildren<CGEnemy>()) enemy.enabled = false;
                string label = "N" + (level + 1) + (mode == 0 ? " parado" : " carrera");
                CGPickup[] pickups = world.root.GetComponentsInChildren<CGPickup>().OrderBy(p => p.id).ToArray();
                Record(pickups.Length == 3, label + " genera tres fragmentos");
                Place(game.Hero, 69);
                game.InteractNearPlayer();
                Record(game.Playing, label + " portal rechaza sin fragmentos");
                for (int id = 0; id < pickups.Length; id++) {
                    CGPickup pickup = pickups[id];
                    Vector3 position = pickup.transform.position;
                    int before = Get<int>(game, "chips");
                    if (id == 1 && (level == 0 || level == 1 || level == 7)) {
                        Place(game.Hero, position.x);
                        yield return Wait(.12f);
                        Capture("N" + (level + 1) + "_" + mode + "_fragmento2");
                        Log(label + " fragmento2=" + position + " | plataformas=" + string.Join(";", world.root.GetComponentsInChildren<BoxCollider2D>().Where(c => c.GetComponent<CGSurface>() != null && c.bounds.min.y > 0 && Mathf.Abs(c.bounds.center.x - position.x) < 2.5f).Select(c => c.bounds.ToString())));
                    }
                    collected = false;
                    float[] approaches = mode == 0 ? new[] { 0f, -.3f, .3f, -.6f, .6f, -.9f, .9f, -1.2f, 1.2f } : new[] { -1.8f, 1.8f, -2.1f, 2.1f, -1.5f, 1.5f };
                    foreach (float approach in approaches) {
                        Place(game.Hero, position.x + approach);
                        yield return Wait(.16f);
                        Key direction = approach > 0 ? Key.A : Key.D;
                        yield return Wait(.04f, mode == 0 ? new[] { Key.Space } : new[] { direction, Key.LeftShift, Key.Space });
                        yield return Wait(.8f, mode == 0 ? Array.Empty<Key>() : new[] { direction, Key.LeftShift });
                        if (Get<bool[]>(game, "taken")[id]) { collected = true; break; }
                    }
                    yield return Wait(.04f);
                    Record(collected && Get<int>(game, "chips") == before + 1, label + " recoge fragmento " + (id + 1) + " por trigger (+1 exacto)");
                    if (collected) { game.TakeChip(id); Record(Get<int>(game, "chips") == before + 1, label + " fragmento " + (id + 1) + " no cuenta dos veces"); }
                }
                Record(Get<int>(game, "chips") == 3, label + " contador final 3/3");
                Place(game.Hero, 69); game.InteractNearPlayer();
                Record(game.Playing, label + " portal rechaza sin terminal");
                Place(game.Hero, 40); game.InteractNearPlayer();
                Record(Get<object>(game, "page").ToString() == "Quiz", label + " abre terminal");
                // Calls the existing answer handler; mouse/keyboard quiz UI is outside #18.
                Call(game, "QuizAnswer", world.spec.right);
                Record(Get<bool>(game, "terminalSolved") && game.Playing, label + " respuesta correcta restaura terminal");
                if (world.boss != null) {
                    Place(game.Hero, 69); game.InteractNearPlayer();
                    Record(game.Playing, label + " portal rechaza con jefe vivo");
                    // Exercise actual attack overlap and Hurt, rather than invoking BossDown.
                    for (int attacks = 0; attacks < 20 && world.boss != null && !Get<bool>(game, "bossDead"); attacks++) {
                        game.Attack(world.boss.transform.position);
                        yield return null;
                    }
                    Record(Get<bool>(game, "bossDead"), label + " ataques eliminan jefe");
                }
                Place(game.Hero, 69); game.InteractNearPlayer();
                bool victory = Get<object>(game, "page").ToString() == (level == 8 ? "Ending" : "Victory");
                Record(victory && CGStore.Data.completed[level], label + " portal y progreso requieren objetivos reales");
                if (victory && level < 8) Record(CGStore.Data.latestUnlocked >= level + 1, label + " desbloquea siguiente nivel");
                Time.timeScale = 1;
            }
        }
    }

    static void Place(CGHero hero, float x) {
        hero.transform.position = new Vector3(x, .03f, 0);
        hero.Body.position = new Vector2(x, .03f);
        hero.Body.linearVelocity = Vector2.zero;
        typeof(CGHero).GetField("queuedJump", Private).SetValue(hero, -999f);
        typeof(CGHero).GetField("lastGrounded", Private).SetValue(hero, -999f);
        Physics2D.SyncTransforms();
    }

    static IEnumerator Wait(float seconds, params Key[] keys) {
        float end = Time.unscaledTime + seconds;
        do { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); yield return null; } while (Time.unscaledTime < end);
    }

    static void Capture(string name) {
        Camera camera = Camera.main;
        if (camera == null) return;
        RenderTexture target = RenderTexture.GetTemporary(1280, 720, 24);
        RenderTexture previous = RenderTexture.active, previousTarget = camera.targetTexture;
        var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try {
            camera.transform.position = new Vector3(31, 2.2f, -10);
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG());
        } finally { camera.targetTexture = previousTarget; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.Destroy(texture); }
    }

    static T Get<T>(object target, string name) { return (T)target.GetType().GetField(name, Private).GetValue(target); }
    static void Call(object target, string name, params object[] args) { target.GetType().GetMethod(name, Private).Invoke(target, args); }
    static void Log(string message) { File.AppendAllText(Path.Combine(output, "results.txt"), message + "\n"); }
    static void Record(bool success, string label) { if (success) passed++; else failed++; Log((success ? "PASS " : "FAIL ") + label); }
}
}
#endif
