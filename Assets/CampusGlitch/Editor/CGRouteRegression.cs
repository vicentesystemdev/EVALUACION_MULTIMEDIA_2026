#if UNITY_EDITOR
using System;
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
// Exercises the real game through keyboard or gamepad input. Never relocates the hero or grants objectives.
[InitializeOnLoad]
public static class CGRouteRegression {
    const string Pending = "CampusGlitch.Route.Output";
    const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    static readonly string[] SaveNames = { "campus_glitch_save_v1.json", "campus_glitch_save_v1.json.bak", "campus_glitch_save_v1.json.tmp" };
    static Keyboard keyboard;
    static Gamepad gamepad;static bool useGamepad;
    static string output;
    static float started, jumpUntil, wonAt;
    static int lastChips;
    static bool finished;
    static float dropGoal;

    static CGRouteRegression() { EditorApplication.playModeStateChanged += OnPlayMode; }

    [MenuItem("Campus Glitch/QA/Recorrer Nivel 1 con teclado (Issue 19)")]
    public static void RunLevel1() {
        Run(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CampusGlitchQA", "Ruta_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")), 0);
    }

    public static void Run(string directory, int level,bool withGamepad=false) {
        if (level < 0 || level > 9 || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Seleccionar nivel 0..9 con Play detenido y scripts compilados.");
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Guardar las escenas antes de iniciar.");
        output = Path.GetFullPath(directory);
        if (Directory.Exists(output) || output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Elegir una carpeta nueva de evidencias fuera del proyecto.");
        Directory.CreateDirectory(output);
        foreach (string name in SaveNames) {
            string source = Path.Combine(Application.persistentDataPath, name);
            if (File.Exists(source)) File.Copy(source, Path.Combine(output, name));
        }
        SessionState.SetString(Pending, output);
        SessionState.SetBool(Pending+".Gamepad",withGamepad);
        SessionState.SetString(Pending + ".Scene", SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending + ".Background", (int)InputSystem.settings.backgroundBehavior);
        SessionState.SetBool(Pending + ".RunBackground", Application.runInBackground);
        File.WriteAllText(Path.Combine(output, "results.txt"), "Ruta N" + (level + 1) + " | Unity " + Application.unityVersion + " | " + DateTime.Now.ToString("O") + "\n");
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/" + CGLevels.Scene(level) + ".unity");
        EditorApplication.isPlaying = true;
    }

    static void OnPlayMode(PlayModeStateChange state) {
        output = SessionState.GetString(Pending, "");
        if (string.IsNullOrEmpty(output)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) {
            CGStore.Data = new CGProgress();
            keyboard = InputSystem.AddDevice<Keyboard>();
            gamepad=InputSystem.AddDevice<Gamepad>();useGamepad=SessionState.GetBool(Pending+".Gamepad",false);
            Log("Entrada: "+(useGamepad?"mando":"teclado")+"; terminal mediante controles reales.");
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.runInBackground = true;
            started = Time.unscaledTime; jumpUntil = wonAt = -1; lastChips = 0; finished = false; dropGoal = float.NaN;
            InputSystem.onAfterUpdate += Drive;
        } else if (state == PlayModeStateChange.ExitingPlayMode) {
            InputSystem.onAfterUpdate -= Drive;
            if (keyboard != null) { InputSystem.RemoveDevice(keyboard); keyboard = null; }
            if(gamepad!=null){InputSystem.RemoveDevice(gamepad);gamepad=null;}
            InputSystem.settings.backgroundBehavior = (InputSettings.BackgroundBehavior)SessionState.GetInt(Pending + ".Background", 0);
            Application.runInBackground = SessionState.GetBool(Pending + ".RunBackground", false);
        } else if (state == PlayModeStateChange.EnteredEditMode) {
            foreach (string name in SaveNames) {
                string copy = Path.Combine(output, name), target = Path.Combine(Application.persistentDataPath, name);
                if (File.Exists(copy)) { Directory.CreateDirectory(Application.persistentDataPath); File.Copy(copy, target, true); }
                else if (File.Exists(target)) File.Delete(target);
            }
            Log("Guardado original restaurado.");
            string scene = SessionState.GetString(Pending + ".Scene", "");
            SessionState.EraseString(Pending);
            if (!string.IsNullOrEmpty(scene)) EditorSceneManager.OpenScene(scene);
        }
    }

    static void Drive() {
        if (finished || keyboard == null || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        try { Step(); } catch (Exception ex) { Finish(false, "EXCEPCION " + ex); }
    }

    static void Step() {
        CGGame game = UnityEngine.Object.FindFirstObjectByType<CGGame>();
        if (game == null || game.Hero == null) return;
        CGHero hero = game.Hero;
        CGWorld world = Get<CGWorld>(game, "world");
        float now = Time.unscaledTime, x = hero.transform.position.x;
        int chips = Get<int>(game, "chips");
        if (chips != lastChips) {
            Log("DATOS " + chips + "/3 t=" + (now - started) + " posicion=" + hero.transform.position); lastChips = chips;
            if (chips == 2) ScreenCapture.CaptureScreenshot(Path.Combine(output, "plataformas.png"));
        }
        string page = Get<object>(game, "page").ToString();
        if (page == "Victory" || page == "Ending") {
            SetKeys();
            if (wonAt < 0) { wonAt = now; Log("Victoria real: datos=" + chips + " terminal=" + Get<bool>(game, "terminalSolved") + " jefe=" + Get<bool>(game, "bossDead")); ScreenCapture.CaptureScreenshot(Path.Combine(output, "victoria.png")); }
            if (now - wonAt > .4f) Finish(chips == 3 && CGStore.Data.completed[game.sceneLevel], "recorrido desde spawn, portal y progreso");
            return;
        }
        if (now - started > 120) { Finish(false, "tiempo agotado x=" + x + " datos=" + chips); return; }
        if (page == "Quiz") {
            if(now%.25f<.08f)SetKeys(Get<int>(game,"selection")==world.spec.right?Key.Enter:Key.DownArrow);
            else SetKeys();return;
        }
        if (!game.Playing) { SetKeys(); return; }
        var keys = new List<Key>();
        CGPickup pickup = world.root.GetComponentsInChildren<CGPickup>().OrderBy(p => p.id).FirstOrDefault();
        float goal = pickup != null ? pickup.transform.position.x : 69;
        if (pickup != null && pickup.transform.position.y < 2 && Mathf.Abs(x - pickup.transform.position.x) < 1 && hero.transform.position.y > .6f && float.IsNaN(dropGoal)) {
            var support = world.root.GetComponentsInChildren<BoxCollider2D>().FirstOrDefault(c => c.name == "Plataforma_Elevada" && x > c.bounds.min.x - .2f && x < c.bounds.max.x + .2f && Mathf.Abs(hero.transform.position.y - c.bounds.max.y) < .15f);
            if (support != null) dropGoal = support.bounds.max.x + .65f;
        }
        if (!float.IsNaN(dropGoal)) { if (hero.transform.position.y < .4f) dropGoal = float.NaN; else goal = dropGoal; }
        bool interact = false;
        if (!world.checkpoint && x > 32.5f && x < 37) { goal = 35; interact = Mathf.Abs(x - goal) < .6f; }
        else if (!Get<bool>(game, "terminalSolved") && x > 36 && x < 43) { goal = 40; interact = Mathf.Abs(x - goal) < .6f; }
        CGEnemy boss = world.boss;
        if (boss != null && !Get<bool>(game, "bossDead") && x > 55) goal = boss.transform.position.x + (x > boss.transform.position.x ? 2.2f : -2.2f);
        if (pickup == null && Get<bool>(game, "terminalSolved") && Get<bool>(game, "bossDead")) { goal = 69; interact = Mathf.Abs(x - goal) < 1; }
        float direction = Mathf.Abs(goal - x) > .13f ? Mathf.Sign(goal - x) : 0;
        CGEnemy enemy = world.root.GetComponentsInChildren<CGEnemy>().FirstOrDefault(e => e.hp > 0 && Mathf.Abs(e.transform.position.x - x) < (e.boss ? 2.4f : 2.1f) && Vector2.Distance(e.transform.position, hero.transform.position + new Vector3(e.transform.position.x > x ? .85f : -.85f, .7f, 0)) < (e.boss ? 1.77f : 1.32f));
        if (enemy != null) {
            float facing = Mathf.Sign(enemy.transform.position.x - x);
            direction = hero.facesRight == (facing > 0) ? 0 : facing;
            if (now % .55f < .09f) keys.Add(Key.J);
        }
        if (direction > 0) keys.Add(Key.D); else if (direction < 0) keys.Add(Key.A);
        keys.Add(Key.LeftShift);
        bool gap = direction > 0 && ((x > 19.15f && x < 19.8f) || (x > 43.15f && x < 43.8f));
        bool highPickup = pickup != null && pickup.transform.position.y > 2 && Mathf.Abs(goal - x) < .3f;
        if (hero.Grounded && (gap || highPickup)) jumpUntil = now + .06f;
        if (now < jumpUntil) keys.Add(Key.Space);
        if (interact && now % .25f < .08f) keys.Add(Key.E);
        SetKeys(keys.ToArray());
    }

    static void SetKeys(params Key[] keys) {
        keyboard.MakeCurrent();InputState.Change(keyboard,new KeyboardState(useGamepad?new Key[0]:keys));
        if(gamepad==null)return;gamepad.MakeCurrent();var state=new GamepadState();
        if(useGamepad){
            state.leftStick=new Vector2((keys.Contains(Key.D)?1:0)-(keys.Contains(Key.A)?1:0),0);
            state.rightTrigger=keys.Contains(Key.LeftShift)?1:0;
            if(keys.Contains(Key.Space)||keys.Contains(Key.Enter))state=state.WithButton(GamepadButton.South);
            if(keys.Contains(Key.J))state=state.WithButton(GamepadButton.West);
            if(keys.Contains(Key.E))state=state.WithButton(GamepadButton.North);
            if(keys.Contains(Key.DownArrow))state=state.WithButton(GamepadButton.DpadDown);
        }
        InputState.Change(gamepad,state);
    }
    static T Get<T>(object target, string field) { return (T)target.GetType().GetField(field, Private).GetValue(target); }
    static void Log(string text) { File.AppendAllText(Path.Combine(output, "results.txt"), text + "\n"); }
    static void Finish(bool success, string reason) { finished = true; SetKeys(); Log((success ? "PASS " : "FAIL ") + reason); EditorApplication.isPlaying = false; }
}
}
#endif
