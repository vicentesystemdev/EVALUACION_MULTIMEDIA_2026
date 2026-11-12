#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.EditorQA {
// MVP 2026-11-12: runs the existing Campus Glitch regressions one after another from the command line.
//   Unity.exe -batchmode -projectPath <p> -executeMethod CampusGlitch.EditorQA.CGMvpBatchQA.RunFromCommandLine
//             -cgOut <new folder outside the project> -cgSuite route:0,fragment,...
// Set the environment variable CG_QA_SAVE_DIR to an empty temporary folder: every CGStore.Save() of the
// session then goes there, so the player's personal save is never written. It is a controlled harness,
// not a manual playthrough.
[InitializeOnLoad]
public static class CGMvpBatchQA {
    const string Key = "CampusGlitch.MvpBatchQA";
    const double TestTimeout = 900, NeverPlayedTimeout = 90;

    static CGMvpBatchQA() {
        string isolated = Environment.GetEnvironmentVariable("CG_QA_SAVE_DIR");
        if (!string.IsNullOrEmpty(isolated)) {
            Directory.CreateDirectory(isolated);
            typeof(CGStore).GetField("TestSaveDirectory", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, isolated);
        }
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += state => {
            // Batch mode has no focused Game View: queued keyboard events would be routed to the editor.
            // Use an in-memory copy of the settings so the project's InputSystem asset is never modified.
            if (state != PlayModeStateChange.EnteredPlayMode || string.IsNullOrEmpty(Output) || !Application.isBatchMode) return;
            var settings = UnityEngine.Object.Instantiate(UnityEngine.InputSystem.InputSystem.settings);
            settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.InputSystem.InputSystem.settings = settings;
        };
    }

    public static void RunFromCommandLine() {
        string[] args = Environment.GetCommandLineArgs();
        string Arg(string name) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        string output = Path.GetFullPath(Arg("-cgOut") ?? throw new ArgumentException("Falta -cgOut"));
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
        if (output.StartsWith(project, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("-cgOut debe estar fuera del proyecto");
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CG_QA_SAVE_DIR"))) throw new ArgumentException("Definir CG_QA_SAVE_DIR para aislar el guardado");
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "summary.txt"), "Campus Glitch MVP batch QA | Unity " + Application.unityVersion + " | " + DateTime.Now.ToString("O") + "\n");
        SessionState.SetString(Key + ".Out", output);
        SessionState.SetString(Key + ".Queue", Arg("-cgSuite") ?? "");
        SessionState.SetInt(Key + ".Index", 0);
        SessionState.SetInt(Key + ".Phase", 0);
    }

    static string[] Queue => SessionState.GetString(Key + ".Queue", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    static string Output => SessionState.GetString(Key + ".Out", "");
    static void Summary(string line) { File.AppendAllText(Path.Combine(Output, "summary.txt"), line + "\n"); Debug.Log("[MVP QA] " + line); }

    static void Tick() {
        if (string.IsNullOrEmpty(Output) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string[] queue = Queue; int index = SessionState.GetInt(Key + ".Index", 0);
        if (index >= queue.Length) { if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish(); return; }
        string test = queue[index]; string folder = Path.Combine(Output, index.ToString("00") + "_" + test.Replace(':', '_'));
        double now = EditorApplication.timeSinceStartup;
        int phase = SessionState.GetInt(Key + ".Phase", 0);
        if (phase == 0) {
            SessionState.SetFloat(Key + ".Started", (float)now); SessionState.SetBool(Key + ".Played", false);
            SessionState.SetInt(Key + ".Phase", 1);
            try { if (Start(test, folder)) { Summary(Evaluate(test, folder)); Next(index); } }
            catch (Exception e) { Summary("FAIL " + test + " (no inicia): " + (e.InnerException ?? e).Message); Next(index); }
            return;
        }
        if (EditorApplication.isPlaying) SessionState.SetBool(Key + ".Played", true);
        double started = SessionState.GetFloat(Key + ".Started", (float)now);
        bool played = SessionState.GetBool(Key + ".Played", false);
        if (now - started > TestTimeout || (!played && now - started > NeverPlayedTimeout && !EditorApplication.isPlayingOrWillChangePlaymode)) {
            Summary("FAIL " + test + " (tiempo agotado " + (int)(now - started) + " s)");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            Next(index); return;
        }
        if (played && !EditorApplication.isPlayingOrWillChangePlaymode) {
            // Give the harness one more editor tick to write its last lines after leaving Play Mode.
            if (SessionState.GetFloat(Key + ".Left", 0) == 0) { SessionState.SetFloat(Key + ".Left", (float)now); return; }
            if (now - SessionState.GetFloat(Key + ".Left", 0) < 2) return;
            SessionState.EraseFloat(Key + ".Left");
            Summary(Evaluate(test, folder)); Next(index);
        }
    }

    static void Next(int index) { SessionState.SetInt(Key + ".Index", index + 1); SessionState.SetInt(Key + ".Phase", 0); }

    static Type T(string name) => typeof(CGGame).Assembly.GetType(name) ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null) ?? throw new Exception("No existe " + name);
    static void Invoke(string type, string method, params object[] args) {
        var m = T(type).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).First(x => x.Name == method && x.GetParameters().Length == args.Length);
        m.Invoke(null, args);
    }

    // Returns true when the test already finished synchronously (edit-mode checks).
    static bool Start(string test, string folder) {
        string[] p = test.Split(':');
        switch (p[0]) {
            case "route": Invoke(FindType("CGRouteRegression"), "Run", folder, int.Parse(p[1]), p.Length > 2 && p[2] == "pad"); return false;
            case "fragment": Invoke(FindType("CGFragmentRegression"), "Run", folder, p.Length > 1 ? int.Parse(p[1]) : -1); return false;
            case "physics": Invoke(FindType("CGHeroPhysicsRegression"), "Run", folder); return false;
            case "playsession": Invoke(FindType("CGPlaySessionQA"), "Begin"); return false;
            case "edit":
                Directory.CreateDirectory(folder);
                Invoke(FindType(p[1]), "RunAll");
                File.WriteAllText(Path.Combine(folder, "results.txt"), "PASS " + p[1] + ".RunAll sin excepciones\n");
                return true;
            case "dir": Invoke(FindType(p[1]), "Run", folder); return true;
            case "play": Invoke(FindType(p[1]), "Run", folder); return false;
            default: throw new ArgumentException("Prueba desconocida " + test);
        }
    }
    static string FindType(string shortName) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
            .First(t => t.Name == shortName && t.Namespace != null && t.Namespace.StartsWith("CampusGlitch")).FullName;

    static string Evaluate(string test, string folder) {
        string file = test == "playsession" ? Path.GetFullPath("Docs/QA/CarlaPlayMode/resultados.txt") : Path.Combine(folder, "results.txt");
        if (!File.Exists(file)) return "FAIL " + test + " (sin results.txt)";
        string[] lines = File.ReadAllLines(file);
        if (test == "playsession") { Directory.CreateDirectory(folder); File.Copy(file, Path.Combine(folder, "results.txt"), true); }
        int fails = lines.Count(l => l.StartsWith("FAIL"));
        string total = lines.LastOrDefault(l => l.Contains("TOTAL PASS=")) ?? lines.LastOrDefault(l => l.StartsWith("PASS") || l.StartsWith("FAIL")) ?? "(sin veredicto)";
        bool ok = fails == 0 && (total.StartsWith("PASS") || total.Contains("FAIL=0"));
        return (ok ? "PASS " : "FAIL ") + test + " | " + total.Trim();
    }

    static void Finish() {
        string output = Output; SessionState.EraseString(Key + ".Out"); SessionState.EraseString(Key + ".Queue");
        File.AppendAllText(Path.Combine(output, "summary.txt"), "FIN " + DateTime.Now.ToString("O") + "\n");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
}
#endif
