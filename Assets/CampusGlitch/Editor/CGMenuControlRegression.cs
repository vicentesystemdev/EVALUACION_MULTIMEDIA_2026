#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
[InitializeOnLoad]
public static class CGMenuControlRegression {
    const string Pending="CampusGlitch.MenuControl.Output";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static GamepadState desired;
    static bool loadingSeen;static int passed,failed,changes,lastSelection;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){return (T)typeof(CGGame).GetField(field,Private).GetValue(Game);}
    static string Page {get{return Game==null?"none":Get<object>("page").ToString();}}
    static CGMenuControlRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Detener Play y esperar compilación.");
        if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar cambios de escena.");
        output=Path.GetFullPath(directory);
        if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);
        foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_MainMenu.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();desired=default;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
            passed=failed=changes=0;lastSelection=-1;loadingSeen=false;InputSystem.onAfterUpdate+=Drive;
            var runner=new GameObject("MenuControlQA");UnityEngine.Object.DontDestroyOnLoad(runner);runner.AddComponent<CGMenuControlDriver>().StartCoroutine(Guard(Suite()));
        }else if(mode==PlayModeStateChange.ExitingPlayMode){
            InputSystem.onAfterUpdate-=Drive;if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);pad=null;keyboard=null;
            InputSystem.settings.backgroundBehavior=(InputSettings.BackgroundBehavior)SessionState.GetInt(Pending+".Background",0);Application.runInBackground=SessionState.GetBool(Pending+".RunBackground",false);Time.timeScale=1;
        }else if(mode==PlayModeStateChange.EnteredEditMode){
            foreach(string name in Saves){string copy=Path.Combine(output,name),target=Path.Combine(Application.persistentDataPath,name);if(File.Exists(copy))File.Copy(copy,target,true);else if(File.Exists(target))File.Delete(target);}
            string scene=SessionState.GetString(Pending+".Scene","");SessionState.EraseString(Pending);EditorSceneManager.OpenScene(scene);
            File.AppendAllText(Path.Combine(output,"results.txt"),"Guardado original restaurado.\n");
        }
    }
    static void Drive(){
        if(pad==null||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;
        keyboard.MakeCurrent();InputState.Change(keyboard,new KeyboardState());pad.MakeCurrent();InputState.Change(pad,desired);
        if(Page=="Loading")loadingSeen=true;
        if(Game!=null&&Get<int>("selection")!=lastSelection){changes++;lastSelection=Get<int>("selection");}
    }
    static void Check(bool ok,string label){if(ok)passed++;else failed++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
    static IEnumerator WaitPage(string expected){float until=Time.realtimeSinceStartup+20;while(Page!=expected&&Time.realtimeSinceStartup<until)yield return null;Check(Page==expected,"Pantalla "+expected);if(Page!=expected)throw new Exception("No se alcanzó "+expected);yield return new WaitForSecondsRealtime(.25f);}
    static IEnumerator Tap(GamepadButton button){desired=new GamepadState().WithButton(button);yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Select(int index){for(int i=0;i<10&&Get<int>("selection")!=index;i++)yield return Tap(GamepadButton.DpadDown);Check(Get<int>("selection")==index,"Selección con cruceta "+index);}
    static IEnumerator Suite(){
        yield return WaitPage("Main");
        Check(CGMenuTheme.Title!=null&&CGMenuTheme.Interface!=null&&CGMenuTheme.Body!=null,"Tres familias tipográficas cargadas");
        yield return Tap(GamepadButton.DpadDown);Check(Get<int>("selection")==1,"Cruceta mueve una opción por pulsación");
        yield return Tap(GamepadButton.South);yield return WaitPage("Map");
        yield return Tap(GamepadButton.DpadRight);Check(Get<int>("selection")==1,"Mapa: derecha selecciona la tarjeta contigua");
        desired=new GamepadState{leftStick=Vector2.up};yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);
        Check(Get<int>("selection")==7,"Stick: arriba recorre la columna con envoltura");
        int before=changes;desired=new GamepadState{leftStick=Vector2.down};yield return new WaitForSecondsRealtime(.7f);desired=default;yield return new WaitForSecondsRealtime(.15f);
        Check(changes-before>=3&&changes-before<=6,"Repetición analógica controlada: pasos="+(changes-before));
        yield return Tap(GamepadButton.North);Check(Get<bool>("mapDetails"),"Y abre detalle del nivel seleccionado");
        yield return Tap(GamepadButton.RightShoulder);Check(Get<int>("selection")>=0&&Get<int>("selection")<10,"RB cambia el nivel desde detalle");
        yield return Tap(GamepadButton.East);Check(Page=="Map"&&!Get<bool>("mapDetails"),"B cierra detalle sin abandonar el mapa");
        yield return Tap(GamepadButton.East);yield return WaitPage("Main");
        yield return Select(3);yield return Tap(GamepadButton.South);yield return WaitPage("Settings");
        yield return Tap(GamepadButton.RightShoulder);yield return Tap(GamepadButton.RightShoulder);yield return Tap(GamepadButton.RightShoulder);
        yield return Tap(GamepadButton.DpadDown);yield return Tap(GamepadButton.South);
        Check(Get<int>("settingsDialog")==4&&Get<int>("settingsChoice")==1,"Borrar avance abre con Cancelar seleccionado");var progress=CGStore.Data;
        yield return Tap(GamepadButton.South);Check(Page=="Settings"&&Get<int>("settingsDialog")==0&&ReferenceEquals(progress,CGStore.Data),"Confirmar Cancelar conserva el progreso y vuelve a Configuración");
        yield return Tap(GamepadButton.East);yield return WaitPage("Main");
        yield return Select(4);yield return Tap(GamepadButton.South);yield return WaitPage("Help");
        yield return Select(6);yield return Tap(GamepadButton.South);yield return WaitPage("Controls");
        yield return Tap(GamepadButton.LeftShoulder);Check(Get<bool>("padMode"),"LB cambia a edición de mando");
        string binding=CGStore.Data.padBindings[0];yield return Tap(GamepadButton.South);Check(Get<int>("rebinding")==0,"A inicia asignación del control elegido");
        yield return Tap(GamepadButton.East);Check(Get<int>("rebinding")==-1&&binding==CGStore.Data.padBindings[0],"B cancela asignación sin guardar otro botón");
        yield return Tap(GamepadButton.East);yield return WaitPage("Main");
        yield return Select(5);yield return Tap(GamepadButton.South);yield return WaitPage("Credits");yield return Tap(GamepadButton.East);yield return WaitPage("Main");
        yield return Tap(GamepadButton.South);yield return WaitPage("Playing");Check(loadingSeen,"Carga asíncrona muestra su transición real");
        var hero=Game.Hero;float start=hero.transform.position.x;
        desired=new GamepadState{leftStick=Vector2.right};yield return new WaitForSecondsRealtime(.5f);desired=default;yield return new WaitForSecondsRealtime(.12f);
        float walked=hero.transform.position.x-start;Check(walked>1.3f&&walked<2.4f,"Stick mueve al personaje a velocidad de caminar");start=hero.transform.position.x;
        desired=new GamepadState{leftStick=Vector2.right,rightTrigger=1};yield return new WaitForSecondsRealtime(.45f);desired=default;yield return new WaitForSecondsRealtime(.12f);
        Check(hero.transform.position.x-start>2.2f,"Gatillo derecho activa carrera");
        float until=Time.realtimeSinceStartup+3;desired=new GamepadState{leftStick=Vector2.right};while(hero.transform.position.x<7.5f&&Time.realtimeSinceStartup<until)yield return null;desired=default;yield return new WaitForSecondsRealtime(.15f);
        yield return Tap(GamepadButton.North);Check(Get<string>("toast").StartsWith("GUARDIA U"),"Y habla con el NPC en su posición real");
        yield return Tap(GamepadButton.South);float peak=hero.transform.position.y;until=Time.realtimeSinceStartup+1;while(Time.realtimeSinceStartup<until){peak=Mathf.Max(peak,hero.transform.position.y);yield return null;}
        Check(peak>2.8f&&hero.Grounded,"A ejecuta ascenso, caída y aterrizaje");
        yield return Tap(GamepadButton.West);Check(hero.GetComponent<CGGuardianVisual>().CurrentAnimation=="Atacar","X conecta con ataque y animación");yield return new WaitForSecondsRealtime(.4f);
        yield return Tap(GamepadButton.Start);yield return WaitPage("Paused");Vector3 paused=hero.transform.position;yield return new WaitForSecondsRealtime(.2f);Check(Time.timeScale==0&&hero.transform.position==paused,"START pausa la simulación");
        yield return Tap(GamepadButton.South);yield return WaitPage("Playing");Check(Time.timeScale==1&&hero.Grounded&&hero.transform.position.y<.1f,"A reanuda sin salto involuntario");
        desired=default;
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);
        while(stack.Count>0){object next=null;bool ended=false;Exception error=null;
            try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}
            if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}
            if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;
        }
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+passed+" FAIL="+failed+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGMenuControlDriver:MonoBehaviour {}
}
#endif
