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
public static class CGCreditsRegression {
    const string Pending="CampusGlitch.CreditsRegression";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static Mouse mouse;static GamepadState desired;static Key key;static int pass,fail;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){var info=typeof(CGGame).GetField(field,Private);return (T)(info!=null?info.GetValue(Game):typeof(CGGame).GetProperty(field,Private).GetValue(Game));}
    static void Set(string field,object value){typeof(CGGame).GetField(field,Private).SetValue(Game,value);}
    static object Call(string name,params object[] args){return typeof(CGGame).GetMethod(name,Private).Invoke(Game,args);}
    static CGCreditsRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar escena y detener Play.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Credits.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();desired=default;key=Key.None;pass=fail=0;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;InputSystem.onAfterUpdate+=Drive;
            var driver=new GameObject("CreditsQA");UnityEngine.Object.DontDestroyOnLoad(driver);driver.AddComponent<CGCreditsDriver>().StartCoroutine(Guard(Suite()));
        }else if(mode==PlayModeStateChange.ExitingPlayMode){
            InputSystem.onAfterUpdate-=Drive;if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);pad=null;keyboard=null;mouse=null;
            InputSystem.settings.backgroundBehavior=(InputSettings.BackgroundBehavior)SessionState.GetInt(Pending+".Background",0);Application.runInBackground=SessionState.GetBool(Pending+".RunBackground",false);Time.timeScale=1;
        }else if(mode==PlayModeStateChange.EnteredEditMode){
            foreach(string name in Saves){string copy=Path.Combine(output,name),target=Path.Combine(Application.persistentDataPath,name);if(File.Exists(copy))File.Copy(copy,target,true);else if(File.Exists(target))File.Delete(target);}
            string scene=SessionState.GetString(Pending+".Scene","");SessionState.EraseString(Pending);CGStore.Data=null;EditorSceneManager.OpenScene(scene);File.AppendAllText(Path.Combine(output,"results.txt"),"Guardado original restaurado.\n");
        }
    }
    static void Drive(){if(pad==null||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;mouse.MakeCurrent();InputState.Change(mouse,new MouseState{position=new Vector2(-100,-100)});keyboard.MakeCurrent();InputState.Change(keyboard,key==Key.None?new KeyboardState():new KeyboardState(key));pad.MakeCurrent();InputState.Change(pad,desired);}
    static void Check(bool ok,string label){if(ok)pass++;else fail++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
    static IEnumerator Tap(GamepadButton button){desired=new GamepadState().WithButton(button);yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Press(Key value){key=value;yield return new WaitForSecondsRealtime(.08f);key=Key.None;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Suite(){
        yield return new WaitForSecondsRealtime(.5f);Check(Get<object>("page").ToString()=="Credits","Abre la escena de Créditos");
        var design=Resources.Load<CGCreditsDesign>("CG/Presentation/CampusCredits");Check(design!=null&&design.team.Length==4&&design.acknowledgements.Length==2,"Archivo editable contiene cuatro integrantes y dos agradecimientos");
        var portraits=new HashSet<Texture2D>();foreach(var p in design.team)portraits.Add(p.portrait);foreach(var p in design.acknowledgements)portraits.Add(p.portrait);Check(portraits.Count==6&&!portraits.Contains(null),"Los seis retratos son distintos y están conectados");
        Check(design.team[0].arachnidTheme&&design.team[0].detail.Contains("Spider-Man"),"Víctor conserva la temática arácnida junto con programación y animación");Check(CGCreditsArt.Web!=null,"Insignia arácnida se genera y se conserva en caché");
        string saved=JsonUtility.ToJson(CGStore.Data),scene=SceneManager.GetActiveScene().path;
        Set("selection",0);yield return Press(Key.Enter);Check(Get<int>("creditsOverlay")==1&&Get<int>("creditsMember")==0,"Enter abre el perfil de Víctor");
        yield return Tap(GamepadButton.RightShoulder);Check(Get<int>("creditsMember")==1,"RB cambia al siguiente integrante");yield return Tap(GamepadButton.LeftShoulder);Check(Get<int>("creditsMember")==0,"LB vuelve al integrante anterior");yield return Press(Key.LeftArrow);Check(Get<int>("creditsMember")==3,"Flecha izquierda recorre circularmente hasta Marcelo");
        yield return Tap(GamepadButton.East);Check(Get<int>("creditsOverlay")==0&&Get<int>("selection")==0,"B cierra el perfil y restaura el foco sin salir de Créditos");
        yield return Press(Key.RightArrow);Check(Get<int>("selection")==1,"Flecha derecha selecciona a Carla");yield return Tap(GamepadButton.South);Check(Get<int>("creditsOverlay")==1&&Get<int>("creditsMember")==1,"A abre el integrante seleccionado");yield return Press(Key.Escape);Check(Get<int>("creditsOverlay")==0&&Get<int>("selection")==1,"Escape cierra solamente el perfil");
        yield return Tap(GamepadButton.DpadRight);Check(Get<int>("selection")==2,"Cruceta derecha mueve el foco una tarjeta");
        Set("selection",4);yield return Tap(GamepadButton.South);Check(Get<int>("creditsOverlay")==2,"A abre Agradecimientos especiales");yield return Tap(GamepadButton.South);Check(Get<int>("creditsOverlay")==0&&Get<int>("selection")==4,"Confirmar cierra Agradecimientos y conserva la selección");
        Set("selection",5);yield return Press(Key.Enter);Check(Get<int>("creditsOverlay")==3,"Enter abre Arte y recursos");yield return Tap(GamepadButton.East);Check(Get<int>("creditsOverlay")==0&&Get<int>("selection")==5,"B cierra Recursos sin cambiar de escena");
        Set("selection",6);yield return Press(Key.RightArrow);Check(Get<int>("selection")==0,"Navegación horizontal vuelve al inicio");Set("selection",0);desired=new GamepadState{leftStick=new Vector2(1,0)};yield return new WaitForSecondsRealtime(.1f);desired=default;yield return new WaitForSecondsRealtime(.15f);Check(Get<int>("selection")==1,"Stick selecciona una tarjeta sin saltos repetidos");
        var items=new List<CGUIDraw.Item>();CGUIDraw.Capture=items;try{CGUIDraw.ResetCaptureState();Game.DrawEditorMenu();}finally{CGUIDraw.Capture=null;}int images=0;foreach(var item in items)if(item.texture!=null&&portraits.Contains(item.texture as Texture2D))images++;Check(images==4,"Scene usa los cuatro retratos del mismo menú de Play");Check(items.Exists(i=>i.text==design.team[3].name),"Nombre de Marcelo se muestra en la tarjeta");
        Call("OpenCreditsOverlay",2,0);items.Clear();CGUIDraw.Capture=items;try{CGUIDraw.ResetCaptureState();Game.DrawEditorMenu();}finally{CGUIDraw.Capture=null;}Check(items.Exists(i=>i.texture==design.acknowledgements[0].portrait)&&items.Exists(i=>i.texture==design.acknowledgements[1].portrait),"El modal muestra ambos retratos de agradecimiento antes de Play");
        CGStore.Data.reduceFlash=true;yield return Tap(GamepadButton.East);Set("selection",0);yield return Tap(GamepadButton.South);Check(Get<int>("creditsOverlay")==1,"La opción de reducir destellos conserva la interacción");yield return Tap(GamepadButton.East);CGStore.Data.reduceFlash=false;
        Check(SceneManager.GetActiveScene().path==scene&&Get<object>("page").ToString()=="Credits","Recorrer perfiles y modales conserva la escena y no inicia el acceso a niveles");Check(JsonUtility.ToJson(CGStore.Data)==saved,"Los créditos no modifican el progreso ni los controles");
        foreach(var size in new[]{new Vector2(1280,720),new Vector2(1024,768),new Vector2(2560,1080)}){var r=CGMenuViewport.Content(size.x,size.y,new Rect(0,0,size.x,size.y));Check(r.x>=-.01f&&r.y>=-.01f&&r.xMax<=size.x+.1f&&r.yMax<=size.y+.1f&&Mathf.Abs(r.width/r.height-16f/9)<.001f,"Encuadre completo y proporcionado en "+size.x+"x"+size.y);}
        ScreenCapture.CaptureScreenshot(Path.Combine(output,"Creditos_Play.png"));yield return new WaitForSecondsRealtime(.4f);
        key=Key.Escape;yield return new WaitForSecondsRealtime(.04f);Check(!(bool)Get<bool>("ShowCampusAccess"),"Volver al menú no abre la entrada de niveles");key=Key.None;yield return new WaitForSecondsRealtime(1.5f);Check(SceneManager.GetActiveScene().name=="SC_MainMenu"&&Get<object>("page").ToString()=="Main","Escape desde Créditos devuelve al menú principal");
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);while(stack.Count>0){object next=null;bool ended=false;Exception error=null;try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGCreditsDriver:MonoBehaviour {}
}
#endif
