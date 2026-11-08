#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
[InitializeOnLoad]
public static class CGHeroPhysicsRegression {
    const string Pending = "CampusGlitch.HeroPhysics.Output";
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static string output;
    static int passed, failed;

    static CGHeroPhysicsRegression() {
        EditorApplication.playModeStateChanged += state => {
            if (state != PlayModeStateChange.EnteredPlayMode || string.IsNullOrEmpty(SessionState.GetString(Pending, ""))) return;
            Execute();
        };
    }

    [MenuItem("Campus Glitch/QA/Verificar caida lateral y salto (Issue 19)")]
    public static void RunAll() {
        Run(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CampusGlitchQA", "Issue19_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")));
    }

    public static void Run(string directory) {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new InvalidOperationException("Detener Play y esperar la compilacion.");
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Guardar las escenas antes de iniciar.");
        output = Path.GetFullPath(directory);
        if (Directory.Exists(output) || output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Usar una carpeta nueva fuera del proyecto.");
        Directory.CreateDirectory(output);
        SessionState.SetString(Pending, output);
        EditorApplication.isPlaying = true;
    }

    static void Execute() {
        output = SessionState.GetString(Pending, "");
        SessionState.EraseString(Pending);
        passed = failed = 0;
        File.WriteAllText(Path.Combine(output, "results.txt"), "Issue #19 | Unity " + Application.unityVersion + " | " + DateTime.Now.ToString("O") + "\n");
        CGProgress original = CGStore.Data;
        CGStore.Data = new CGProgress();
        try {
            foreach (float speed in new[] { 3.5f, 5.8f }) foreach (int direction in new[] { -1, 1 }) Edge(speed, direction);
            GroundStop();
            foreach (float speed in new[] { 3.5f, 5.8f }) { Landing(0, 2.96f, speed); Landing(2.96f, 5.48f, speed); Landing(5.48f, 8.34f, speed); }
            SolidUnderside(2.96f);
            for (int level = 0; level < 10; level++) Geometry(level);
        } catch (Exception ex) { Record(false, "EXCEPCION " + ex); }
        finally {
            CGStore.Data = original;
            Log("TOTAL PASS=" + passed + " FAIL=" + failed);
            Debug.Log("[Issue19] PASS=" + passed + " FAIL=" + failed + " | " + output);
            EditorApplication.isPlaying = false;
        }
    }

    // Each fixture has its own 2D physics scene; no save, scene asset or live level is edited.
    sealed class Fixture : IDisposable {
        public readonly Scene scene;
        public readonly PhysicsScene2D physics;
        public readonly GameObject root;
        public readonly CGHero hero;
        public Fixture(Vector2 position) {
            scene = SceneManager.CreateScene("HeroPhysics_" + Guid.NewGuid().ToString("N"), new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            physics = scene.GetPhysicsScene2D();
            root = new GameObject("PhysicsFixture"); SceneManager.MoveGameObjectToScene(root, scene);
            var game = root.AddComponent<CGGame>(); game.enabled = false;
            FieldInfo page = typeof(CGGame).GetField("page", Private); page.SetValue(game, Enum.Parse(page.FieldType, "Playing"));
            var player = new GameObject("Hero"); player.transform.SetParent(root.transform); player.transform.position = position;
            hero = player.AddComponent<CGHero>(); hero.Setup(game); hero.enabled = false;
        }
        public void Box(Vector2 position, Vector2 size) {
            var obstacle = new GameObject("Obstacle"); obstacle.transform.SetParent(root.transform); obstacle.transform.position = position;
            obstacle.AddComponent<BoxCollider2D>().size = size; obstacle.AddComponent<CGSurface>();
        }
        public void Platform(float top) {
            CGWorld world = root.GetComponent<CGWorld>();
            if (world == null) { world = root.AddComponent<CGWorld>(); world.root = root.transform; }
            typeof(CGWorld).GetMethod("Platform", Private).Invoke(world, new object[] { 0f, top - .16f, 2.6f, .32f, true });
        }
        public void Step(float move, float speed) {
            hero.moveInput = move; hero.running = speed > 4;
            // Execute the real velocity/grounding code; only input acquisition is isolated.
            typeof(CGHero).GetMethod("FixedUpdate", Private).Invoke(hero, null);
            physics.Simulate(.02f);
        }
        public void Jump() {
            hero.Grounded = true;
            typeof(CGHero).GetField("queuedJump", Private).SetValue(hero, Time.time);
            typeof(CGHero).GetMethod("Update", Private).Invoke(hero, null);
        }
        public void Dispose() { UnityEngine.Object.DestroyImmediate(root); SceneManager.UnloadSceneAsync(scene); }
    }

    static void Edge(float speed, int direction) {
        using (var fixture = new Fixture(new Vector2(-direction * 1.40f, 1.0f))) {
            fixture.Box(new Vector2(0, 1), new Vector2(2, 2));
            float start = fixture.hero.Body.position.y;
            for (int i = 0; i < 60 && fixture.hero.Body.position.y > -.5f; i++) fixture.Step(direction, speed);
            float end = fixture.hero.Body.position.y;
            Record(end < start - .8f, "cae manteniendo direccion=" + direction + " velocidad=" + speed + " y=" + start + " -> " + end);
            Record(direction * fixture.hero.Body.position.x < -1f, "conserva colision lateral direccion=" + direction + " velocidad=" + speed);
        }
    }

    static void GroundStop() {
        using (var fixture = new Fixture(new Vector2(-3, .03f))) {
            fixture.Box(new Vector2(0, -.5f), new Vector2(20, 1));
            for (int i = 0; i < 20; i++) fixture.Step(1, 5.8f);
            float x = fixture.hero.Body.position.x;
            for (int i = 0; i < 20; i++) fixture.Step(0, 5.8f);
            Record(Mathf.Abs(fixture.hero.Body.position.x - x) < .01f && Mathf.Abs(fixture.hero.Body.position.y) < .05f, "se detiene sobre suelo al soltar direccion, sin deslizar ni rebotar");
        }
    }

    static void SolidUnderside(float top) {
        using (var fixture = new Fixture(new Vector2(0, .03f))) {
            fixture.Box(new Vector2(0, -.5f), new Vector2(20, 1));
            fixture.Platform(top);
            for (int i = 0; i < 10; i++) fixture.Step(0, 3.5f);
            fixture.Jump();
            float peak = 0;
            for (int i = 0; i < 85; i++) {
                fixture.Step(0, 3.5f); peak = Mathf.Max(peak, fixture.hero.Body.position.y);
            }
            Record(peak + 1.64f < top - .32f + .03f && Mathf.Abs(fixture.hero.Body.position.y) < .05f, "plataforma solida por debajo=" + top + ", golpea y vuelve al suelo sin quedarse pegado, pico=" + peak);
        }
    }

    static void Geometry(int level) {
        using (var fixture = new Fixture(new Vector2(-100, 0))) {
            var world = fixture.root.AddComponent<CGWorld>();
            world.Setup(fixture.root.GetComponent<CGGame>(), level);
            fixture.physics.Simulate(.02f);
            var platforms = world.root.GetComponentsInChildren<BoxCollider2D>().Where(c => c.name == "Plataforma_Elevada").ToArray();
            Record(platforms.Length >= 10 && platforms.All(c => c.enabled && !c.isTrigger && !c.usedByEffector && c.GetComponent<PlatformEffector2D>() == null), "N" + (level + 1) + " todas las plataformas elevadas tienen colision solida (" + platforms.Length + ")");
            Record(platforms.All(c => {
                var renderer = c.GetComponentInChildren<SpriteRenderer>();
                return renderer != null && Vector2.Distance(c.bounds.center, renderer.bounds.center) < .002f && Vector2.Distance(c.bounds.size, renderer.bounds.size) < .002f;
            }), "N" + (level + 1) + " collider coincide con superficie visible");
            var rails = world.root.GetComponentsInChildren<SpriteRenderer>().Where(r => r.name == "Pasamanos_Elevado").ToArray();
            Record(rails.All(r => r.transform.parent.GetComponent<CGSurface>() != null), "N" + (level + 1) + " barandales unidos a plataformas fisicas, sin apoyos flotantes falsos");
            bool clearance = platforms.All(c => c.bounds.min.y >= 2.2f);
            foreach (var lower in platforms) foreach (var upper in platforms) {
                if (upper.bounds.min.y <= lower.bounds.max.y || upper.bounds.min.x >= lower.bounds.max.x || lower.bounds.min.x >= upper.bounds.max.x) continue;
                clearance &= upper.bounds.min.y - lower.bounds.max.y >= 2.1f;
            }
            Record(clearance, "N" + (level + 1) + " corredor de suelo y separacion vertical con espacio para el personaje");
        }
    }

    static void Landing(float floor, float top, float speed) {
        float approach = speed > 4 ? 2.4f : 1.6f;
        using (var fixture = new Fixture(new Vector2(-1.3f - approach, floor + .03f))) {
            fixture.Box(new Vector2(0, floor - .5f), new Vector2(20, 1));
            fixture.Platform(top);
            for (int i = 0; i < 10; i++) fixture.Step(0, speed);
            fixture.Jump();
            float peak = fixture.hero.Body.position.y;
            bool landed = false;
            for (int i = 0; i < 85; i++) {
                // Brake over the platform, exactly as releasing horizontal input would do.
                fixture.Step(fixture.hero.Body.position.x < -.1f ? 1 : 0, speed);
                Vector2 p = fixture.hero.Body.position;
                peak = Mathf.Max(peak, p.y);
                if (Mathf.Abs(p.x) < 1.2f && Mathf.Abs(p.y - top) < .06f && Mathf.Abs(fixture.hero.Body.linearVelocity.y) < .1f) { landed = true; break; }
            }
            Record(landed, "aterriza desde=" + floor + " hasta=" + top + " velocidad=" + speed + " pico=" + peak + " final=" + fixture.hero.Body.position);
        }
    }

    static void Log(string text) { File.AppendAllText(Path.Combine(output, "results.txt"), text + "\n"); }
    static void Record(bool success, string text) { if (success) passed++; else failed++; Log((success ? "PASS " : "FAIL ") + text); }
}
}
#endif
