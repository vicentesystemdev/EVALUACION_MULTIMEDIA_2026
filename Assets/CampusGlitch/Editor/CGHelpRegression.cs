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
public static class CGHelpRegression {
    const string Pending="CampusGlitch.HelpRegression";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static Mouse mouse;static GamepadState desired;static Key key;static int pass,fail;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){var info=typeof(CGGame).GetField(field,Private);return (T)(info!=null?info.GetValue(Game):typeof(CGGame).GetProperty(field,Private).GetValue(Game));}
    static void Set(string field,object value){typeof(CGGame).GetField(field,Private).SetValue(Game,value);}
    static object Call(string name,params object[] args){return typeof(CGGame).GetMethod(name,Private).Invoke(Game,args);}
    static CGHelpRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar escena y detener Play.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".VSync",QualitySettings.vSyncCount);SessionState.SetInt(Pending+".FPS",Application.targetFrameRate);SessionState.SetFloat(Pending+".Volume",AudioListener.volume);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Help.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();desired=default;key=Key.None;pass=fail=0;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;InputSystem.onAfterUpdate+=Drive;
            var driver=new GameObject("HelpQA");UnityEngine.Object.DontDestroyOnLoad(driver);driver.AddComponent<CGHelpDriver>().StartCoroutine(Guard(Suite()));
        }else if(mode==PlayModeStateChange.ExitingPlayMode){
            InputSystem.onAfterUpdate-=Drive;if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);pad=null;keyboard=null;mouse=null;
            InputSystem.settings.backgroundBehavior=(InputSettings.BackgroundBehavior)SessionState.GetInt(Pending+".Background",0);Application.runInBackground=SessionState.GetBool(Pending+".RunBackground",false);Time.timeScale=1;
        }else if(mode==PlayModeStateChange.EnteredEditMode){
            foreach(string name in Saves){string copy=Path.Combine(output,name),target=Path.Combine(Application.persistentDataPath,name);if(File.Exists(copy))File.Copy(copy,target,true);else if(File.Exists(target))File.Delete(target);}
            string scene=SessionState.GetString(Pending+".Scene","");SessionState.EraseString(Pending);CGSettingsRuntime.Preview=null;QualitySettings.vSyncCount=SessionState.GetInt(Pending+".VSync",0);Application.targetFrameRate=SessionState.GetInt(Pending+".FPS",60);AudioListener.volume=SessionState.GetFloat(Pending+".Volume",.85f);CGTouch.EditorSimulation=false;CGTouch.Reset();CGStore.Data=null;EditorSceneManager.OpenScene(scene);File.AppendAllText(Path.Combine(output,"results.txt"),"Guardado original restaurado.\n");
        }
    }
    static void Drive(){if(pad==null||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;mouse.MakeCurrent();InputState.Change(mouse,new MouseState{position=new Vector2(-100,-100)});keyboard.MakeCurrent();InputState.Change(keyboard,key==Key.None?new KeyboardState():new KeyboardState(key));pad.MakeCurrent();InputState.Change(pad,desired);}
    static void Check(bool ok,string label){if(ok)pass++;else fail++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
    static IEnumerator Tap(GamepadButton button){desired=new GamepadState().WithButton(button);yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Press(Key value){key=value;yield return new WaitForSecondsRealtime(.08f);key=Key.None;yield return new WaitForSecondsRealtime(.16f);}
    static List<CGUIDraw.Item> Capture(int section,int item=0){Game.PreviewHelp(section,item);var list=new List<CGUIDraw.Item>();CGUIDraw.Capture=list;try{CGUIDraw.ResetCaptureState();Game.DrawEditorMenu();}finally{CGUIDraw.Capture=null;}return list;}
    static bool Contains(List<CGUIDraw.Item> items,string text){return items.Exists(i=>i.text!=null&&i.text.Contains(text));}
    static IEnumerator OpenHelp(){Call("ToMain");yield return new WaitForSecondsRealtime(1.1f);Call("MainSelect",4);yield return new WaitForSecondsRealtime(1.1f);}
    static IEnumerator Suite(){
        yield return new WaitForSecondsRealtime(.6f);Check(Get<object>("page").ToString()=="Help","Abre Ayuda con la misma interfaz que Scene");
        var design=Resources.Load<CGHelpDesign>("CG/Presentation/CampusHelp");Check(design!=null&&design.sections.Length==6&&Array.IndexOf(design.sections,"Controles rápidos")<0,"Seis apartados sin Controles rápidos en el lateral");
        Check(design.steps.Length==6&&design.questions.Length==4,"Seis pasos breves y cuatro respuestas editables");string saved=JsonUtility.ToJson(CGStore.Data);var items=Capture(0);Check(Contains(items,"CONTROLES")&&Contains(items,"MAPA DEL CAMPUS")&&!Contains(items,"Vista previa"),"Accesos directos a Controles y Mapa sin etiquetas de vista previa");
        Check(items.Exists(i=>i.texture is RenderTexture t&&t.name=="Campus_Nivel_Actual"),"La guía muestra el mundo real del juego");yield return new WaitForSecondsRealtime(.3f);Check(CGSettingsWorldView.World!=null&&CGSettingsWorldView.RenderedFrames>0,"El escenario ilustrativo actualiza sus animaciones");
        bool frozen=true;foreach(var body in CGSettingsWorldView.World.GetComponentsInChildren<Rigidbody2D>())frozen&=!body.simulated;Check(frozen&&!CGSettingsWorldView.World.hero.enabled,"Consultar la ayuda no simula la partida ni mueve al jugador");
        Set("helpPad",false);string previous=CGStore.Data.actionKeys[5];CGStore.Data.actionKeys[5]="U";Check(((string)Call("HelpText","Usa {interact}")).Contains("U"),"Las instrucciones usan la tecla reasignada");CGStore.Data.actionKeys[5]=previous;
        Set("helpPad",true);previous=CGStore.Data.padBindings[5];CGStore.Data.padBindings[5]="buttonWest";Check(((string)Call("HelpText","Usa {interact}")).Contains("X"),"Las instrucciones usan el botón de mando reasignado");CGStore.Data.padBindings[5]=previous;
        Call("HelpSection",0);yield return Press(Key.Tab);Check(Get<int>("helpSection")==1&&!Get<bool>("helpPad"),"Tab cambia de apartado y muestra teclado");yield return Tap(GamepadButton.LeftShoulder);Check(Get<int>("helpSection")==0&&Get<bool>("helpPad"),"LB vuelve al apartado anterior y muestra mando");yield return Tap(GamepadButton.LeftShoulder);Check(Get<int>("helpSection")==5,"LB recorre circularmente los seis apartados");yield return Tap(GamepadButton.RightShoulder);Check(Get<int>("helpSection")==0,"RB vuelve al primer apartado");
        yield return Press(Key.RightArrow);Check(Get<int[]>("helpItems")[0]==1,"Flecha derecha recorre los pasos");yield return Tap(GamepadButton.DpadLeft);Check(Get<int[]>("helpItems")[0]==0,"Cruceta izquierda vuelve al paso anterior");
        Call("HelpAction",9);Check(Get<int[]>("helpItems")[0]==5,"Anterior vuelve al último paso desde el primero");Call("HelpAction",10);Check(Get<int[]>("helpItems")[0]==0,"Siguiente vuelve al primer paso desde el último");
        for(int i=0;i<10;i++){items=Capture(1,i);Check(Contains(items,CGMenuTheme.MapNames[i])&&Contains(items,CGLevels.All[i].objective),"Destino "+(i+1)+": objetivo conectado a los datos reales del nivel");}
        var kinds=new[]{"Guardia_U","Estudiante","Tecnica_Lab","Recepcionista","Director","Docente","Noemi_Moller","Unifranzito"};bool sprites=true;foreach(string kind in kinds)sprites&=CGCampusActors.Frame(false,CGCampusActors.NpcRow(kind),0)!=null;Check(sprites,"Los ocho personajes usan las imágenes del campus");items=Capture(3,2);Check(Contains(items,"TÉCNICA DE LAB")&&Contains(items,"terminal"),"Elegir personaje muestra su función en el juego");
        bool icons=true;for(int i=0;i<5;i++)icons&=Call("HelpSymbol",i)!=null;Check(icons,"Fragmento, punto de regreso, terminal, portal y enemigo tienen imagen");items=Capture(2,5);Check(Contains(items,"PODER EQUIPADO")&&Contains(items,"energía"),"Los poderes explican energía y activación");items=Capture(4,1);Check(Contains(items,"no guarda el punto al cerrar"),"La guía distingue el punto temporal del avance guardado");
        Call("HelpAction",11);Check(Get<bool>("helpTutorial")&&Get<int[]>("helpItems")[0]==0,"Ver tutorial inicia el recorrido de seis pasos");Set("helpTutorialAt",CGMenuTheme.Clock-design.tutorialSeconds-.1f);yield return null;yield return null;Check(Get<int[]>("helpItems")[0]==1,"El tutorial avanza automáticamente al siguiente paso");yield return Tap(GamepadButton.East);Check(!Get<bool>("helpTutorial")&&Get<object>("page").ToString()=="Help","B detiene el tutorial conservando la ayuda");
        Call("HelpAction",11);Get<int[]>("helpItems")[0]=5;Set("helpTutorialAt",CGMenuTheme.Clock-design.tutorialSeconds-.1f);yield return null;yield return null;Check(!Get<bool>("helpTutorial")&&Get<int[]>("helpItems")[0]==5,"El tutorial termina en el último paso sin repetirse");
        Check(JsonUtility.ToJson(CGStore.Data)==saved,"Consultar apartados y tutorial conserva progreso y controles");CGStore.Data.reduceFlash=true;items=Capture(0,4);Check(Contains(items,"REÚNE Y RESUELVE")&&Contains(items,"CONTROLES"),"Reducir destellos conserva toda la guía");CGStore.Data.reduceFlash=false;
        foreach(var size in new[]{new Vector2(1280,720),new Vector2(1024,768),new Vector2(2560,1080)}){var r=CGMenuViewport.Content(size.x,size.y,new Rect(0,0,size.x,size.y));Check(r.x>=-.01f&&r.y>=-.01f&&r.xMax<=size.x+.1f&&r.yMax<=size.y+.1f&&Mathf.Abs(r.width/r.height-16f/9)<.001f,"Encuadre de ayuda completo en "+size.x+"x"+size.y);}
        Game.PreviewHelp(3,2);yield return new WaitForSecondsRealtime(.25f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"Ayuda_Play.png"));yield return new WaitForSecondsRealtime(.3f);
        var icon=Resources.Load<Texture2D>("CG/Presentation/CampusGlitchIcon");Check(icon!=null&&icon.width>=512,"El icono de Campus Glitch está conectado y tiene definición suficiente");
        var phones=CGMobileLocal.ParsePhones("List of devices attached\nABC123 device product:test model:Pixel_8 device:test\nXYZ789 unauthorized usb:1\nOFF123 offline\n* daemon started successfully *\n");Check(phones.Count==3&&phones[0].name=="Pixel 8"&&phones[0].Ready&&!phones[1].Ready&&!phones[2].Ready,"USB distingue teléfono autorizado, pendiente y desconectado");
        Check(CGMobileLocal.ParsePhones("List of devices attached\n").Count==0,"Sin dispositivos no inventa teléfonos detectados");
        Call("HelpAction",5);Check(Get<int>("helpSection")==5,"Versión móvil tiene su propio apartado lateral");items=Capture(5);Check(Contains(items,"2 · ELIGE DÓNDE GUARDAR")&&Contains(items,"3 · ENVÍA A TU TELÉFONO")&&!Contains(items,"ANTERIOR"),"La distribución móvil sigue guardar y enviar, sin acciones de tutorial ajenas");
        Set("mobilePhones",phones);Set("mobilePhone",0);Call("MobileAction",14);Check(Get<bool>("mobilePhonePicker"),"El selector abre la lista de teléfonos");yield return Tap(GamepadButton.DpadDown);yield return Tap(GamepadButton.East);Check(Get<int>("mobilePhone")==0&&!Get<bool>("mobilePhonePicker"),"Cancelar el selector conserva el teléfono elegido");Call("MobileAction",14);yield return Tap(GamepadButton.DpadDown);yield return Tap(GamepadButton.South);Check(Get<int>("mobilePhone")==1,"Confirmar elige otro teléfono de la lista");Check(!(bool)Call("MobileEnabled",15),"No permite enviar a un teléfono sin autorización");Set("mobilePhones",new List<CGMobilePhone>());Set("mobilePhone",0);
        string fixture=Path.Combine(output,"fixture.apk"),copy=Path.Combine(output,"copia.apk");File.WriteAllText(fixture,"Archivo de comprobación de copia, no es una aplicación.");var package=new CGMobilePackage{apkPath=fixture,sha256=CGMobileLocal.Hash(fixture)};var copying=CGMobileLocal.Copy(package,copy);while(!copying.IsCompleted)yield return null;Check(!copying.IsFaulted&&CGMobileLocal.Hash(copy)==package.sha256,"La copia conserva exactamente el archivo seleccionado y su integridad");File.AppendAllText(fixture,"cambio");copying=CGMobileLocal.Copy(package,copy);while(!copying.IsCompleted)yield return null;Check(copying.IsFaulted&&CGMobileLocal.Hash(copy)==package.sha256,"Una APK alterada se rechaza sin reemplazar la copia anterior");
        bool layout=true;foreach(var size in new[]{new Vector2(1280,720),new Vector2(2340,1080),new Vector2(1024,768)}){var safe=new Rect(42,16,size.x-84,size.y-32);var rects=new List<Rect>();for(int i=0;i<8;i++){var r=CGTouch.Layout(i,size.x,size.y,safe);layout&=r.xMin>=safe.xMin&&r.xMax<=safe.xMax&&r.yMin>=size.y-safe.yMax&&r.yMax<=size.y-safe.yMin;foreach(var other in rects)layout&=!r.Overlaps(other);rects.Add(r);}}Check(layout,"Ocho controles táctiles respetan márgenes y no se superponen en tres formatos");
        Check(CGTouch.Gesture(new Vector2(2,-100),.2f,1)==CGTouch.Jump&&CGTouch.Gesture(new Vector2(100,2),.2f,1)==CGTouch.Ability&&CGTouch.Gesture(new Vector2(2,2),.1f,1)==CGTouch.Attack,"Deslizar arriba, lateral y tocar corresponden a salto, poder y ataque");Check(CGTouch.Gesture(new Vector2(20,-20),.8f,1)==-1,"Un movimiento corto y lento no activa un poder por accidente");
        Call("MobileAction",18);Check(CGStore.Data.touchGestures,"Gestos se puede elegir y guardar");Call("MobileAction",17);Check(!CGStore.Data.touchGestures,"Botones se puede volver a elegir y guardar");
        yield return CheckTouches();
        Call("HelpSection",5);Set("selection",5);yield return Tap(GamepadButton.DpadDown);Check(Get<int>("selection")==6,"Cruceta selecciona el acceso independiente a Controles");yield return Tap(GamepadButton.South);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Controls","A abre la pantalla real de Controles");Check(CGSettingsWorldView.World==null,"Salir de Ayuda libera el escenario ilustrativo");Check(!Get<bool>("ShowCampusAccess"),"Abrir Controles no activa la entrada a los niveles");
        yield return OpenHelp();Set("selection",7);yield return Press(Key.Enter);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Map","Enter abre el mapa real del campus");yield return OpenHelp();Set("selection",8);yield return Tap(GamepadButton.South);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Main","Volver devuelve al menú principal");
    }
    static IEnumerator CheckTouches(){
        CGTouch.EditorSimulation=true;CGTouch.Reset();var screen=InputSystem.AddDevice<Touchscreen>();var left=CGTouch.ButtonRect(CGTouch.Left).center;var jump=CGTouch.ButtonRect(CGTouch.Jump).center;
        InputSystem.QueueStateEvent(screen,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=new Vector2(left.x,Screen.height-left.y)});InputSystem.QueueStateEvent(screen,new TouchState{touchId=2,phase=UnityEngine.InputSystem.TouchPhase.Began,position=new Vector2(jump.x,Screen.height-jump.y)});yield return null;yield return null;
        Check(CGTouch.Axis<0&&CGTouch.Held(CGTouch.Jump),"Dos dedos mantienen movimiento y salto al mismo tiempo");CGTouch.Reset();Check(CGTouch.Axis==0&&!CGTouch.Held(CGTouch.Jump),"Al reiniciar la entrada no quedan botones táctiles pegados");
        InputSystem.RemoveDevice(screen);CGTouch.Reset();screen=InputSystem.AddDevice<Touchscreen>();
        InputSystem.QueueStateEvent(screen,new TouchState{touchId=3,phase=UnityEngine.InputSystem.TouchPhase.Began,position=new Vector2(jump.x,Screen.height-jump.y)});yield return null;
        InputSystem.QueueStateEvent(screen,new TouchState{touchId=3,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=new Vector2(jump.x,Screen.height-jump.y)});yield return null;yield return null;
        Check(CGTouch.Tapped(CGTouch.Jump)&&!CGTouch.Tapped(CGTouch.Jump),"Un toque breve se conserva hasta leerlo y activa el salto una sola vez");
        InputSystem.RemoveDevice(screen);CGTouch.EditorSimulation=false;
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);while(stack.Count>0){object next=null;bool ended=false;Exception error=null;try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGHelpDriver:MonoBehaviour {}
}
#endif
