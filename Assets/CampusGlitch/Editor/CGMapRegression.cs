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
public static class CGMapRegression {
    const string Pending="CampusGlitch.MapRegression";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static Mouse mouse;static GamepadState desired;static Key key;static int pass,fail;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){var info=typeof(CGGame).GetField(field,Private);return (T)(info!=null?info.GetValue(Game):typeof(CGGame).GetProperty(field,Private).GetValue(Game));}
    static void Set(string field,object value){typeof(CGGame).GetField(field,Private).SetValue(Game,value);}
    static object Call(string name,params object[] args){return typeof(CGGame).GetMethod(name,Private).Invoke(Game,args);}
    static CGMapRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar escena y detener Play.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_LevelMap.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();desired=default;key=Key.None;pass=fail=0;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;InputSystem.onAfterUpdate+=Drive;
            var driver=new GameObject("MapQA");UnityEngine.Object.DontDestroyOnLoad(driver);driver.AddComponent<CGMapDriver>().StartCoroutine(Guard(Suite()));
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
    static List<CGUIDraw.Item> Capture(int index,bool details){Game.PreviewCampusMap(index,details);var result=new List<CGUIDraw.Item>();CGUIDraw.Capture=result;try{CGUIDraw.ResetCaptureState();Game.DrawEditorMenu();}finally{CGUIDraw.Capture=null;}return result;}
    static bool Contains(List<CGUIDraw.Item> items,string fragment){return items.Exists(i=>i.text!=null&&i.text.Contains(fragment));}
    static IEnumerator Suite(){
        yield return new WaitForSecondsRealtime(.6f);Check(Get<object>("page").ToString()=="Map","Abre el mapa del campus");var original=JsonUtility.ToJson(CGStore.Data);
        var rects=new List<Rect>();for(int i=0;i<10;i++)rects.Add(Game.CampusMapCardRect(i));bool fit=true,overlap=false;foreach(var r in rects)fit&=r.x>=24&&r.y>=170&&r.xMax<=868&&r.yMax<=655;for(int i=0;i<10;i++)for(int j=i+1;j<10;j++)overlap|=rects[i].Overlaps(rects[j]);Check(fit&&!overlap,"Diez tarjetas con espacio propio, sin superposición ni salida del recorrido");
        for(int i=0;i<10;i++){var items=Capture(i,true);Check(Contains(items,CGMenuTheme.MapNames[i])&&Contains(items,"Recuperar los tres fragmentos")&&Contains(items,"Resolver el reto de la terminal"),"N"+(i+1)+": destino y actividades conectados al nivel");}
        var locked=Capture(4,true);Check(Contains(locked,"NIVEL BLOQUEADO")&&Contains(locked,"Completa N04")&&!Contains(locked,"ACTIVIDADES COMPLETADAS"),"Nivel bloqueado explica su acceso sin inventar actividades cumplidas");
        var first=Capture(0,false);Check(Contains(first,"RÁFAGA GLITCH")&&Contains(first,"AL COMPLETAR"),"Recompensa de N1 coincide con el poder que se desbloquea");var boss=Capture(5,true);Check(Contains(boss,"Vencer al jefe y abrir el portal"),"Nivel con jefe muestra el desafío correspondiente");
        var extra=Capture(9,true);Check(Contains(extra,"Completa N05")&&Contains(extra,"ETAPA EXTRA RESTAURADA"),"Subsuelo tiene acceso y recompensa propios");
        Check(JsonUtility.ToJson(CGStore.Data)==original,"Consultar los diez destinos no cambia el progreso");Game.PreviewCampusMap(0,false);Set("selection",0);yield return Tap(GamepadButton.DpadRight);Check(Get<int>("selection")==1,"Cruceta mueve a la tarjeta contigua");yield return Tap(GamepadButton.South);Check(Get<object>("page").ToString()=="Map"&&Get<string>("toast")=="Nivel bloqueado","A sobre un nivel bloqueado conserva el mapa y comunica el motivo");
        yield return Tap(GamepadButton.North);Check(Get<bool>("mapDetails"),"Y abre el nivel y sus guías");yield return Tap(GamepadButton.RightShoulder);Check(Get<int>("selection")==2,"RB cambia de destino en el detalle");yield return Tap(GamepadButton.LeftShoulder);Check(Get<int>("selection")==1,"LB vuelve al destino anterior");yield return Tap(GamepadButton.East);Check(Get<object>("page").ToString()=="Map"&&!Get<bool>("mapDetails"),"B cierra el detalle sin salir del mapa");yield return Press(Key.Tab);Check(Get<bool>("mapDetails"),"Tab abre el detalle con teclado");yield return Press(Key.Escape);Check(!Get<bool>("mapDetails"),"Escape cierra el detalle conservando la selección");
        Game.PreviewCampusMap(7,false);desired=new GamepadState{leftStick=Vector2.down};yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);Check(Get<int>("selection")==9,"Stick baja de la última fila a la ruta extra");yield return Tap(GamepadButton.DpadUp);Check(Get<int>("selection")==7,"Cruceta vuelve del extra a la última fila");
        CGStore.Data.latestUnlocked=3;for(int i=0;i<3;i++)CGStore.Data.completed[i]=true;var completed=Capture(1,true);Check(Contains(completed,"ACTIVIDADES COMPLETADAS")&&Contains(completed,"VOLVER A JUGAR")&&Contains(completed,"VARIANTE MORADA"),"Un nivel completado muestra actividades, repetición y recompensa conseguidas");Check(Contains(completed,"3 / 10 NIVELES"),"El progreso cuenta los niveles realmente completados");CGStore.Data.extraUnlocked=true;extra=Capture(9,true);Check(Contains(extra,"JUGAR NIVEL")&&!Contains(extra,"NIVEL BLOQUEADO"),"El acceso extra responde al desbloqueo guardado");
        CGStore.Data.reduceFlash=true;var reduced=Capture(3,false);Check(Contains(reduced,"JUGAR NIVEL")&&Contains(reduced,"DASH MEJORADO"),"Reducir destellos conserva el contenido y sus acciones");CGStore.Data.reduceFlash=false;Game.PreviewCampusMap(3,false);ScreenCapture.CaptureScreenshot(Path.Combine(output,"Mapa_Play.png"));yield return new WaitForSecondsRealtime(.35f);
        yield return Tap(GamepadButton.West);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Customization","X abre la pantalla real del Guardián");Call("ToMain");yield return new WaitForSecondsRealtime(1.1f);Call("MainSelect",1);yield return new WaitForSecondsRealtime(1.1f);yield return Tap(GamepadButton.Select);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Settings","View abre la pantalla real de Configuración");Check(!Get<bool>("ShowCampusAccess"),"Los accesos del mapa no activan la entrada de niveles");
        Call("ToMain");yield return new WaitForSecondsRealtime(1.1f);Call("MainSelect",1);yield return new WaitForSecondsRealtime(1.1f);yield return Press(Key.P);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Customization","P abre el Guardián con teclado");Call("ToMain");yield return new WaitForSecondsRealtime(1.1f);Call("MainSelect",1);yield return new WaitForSecondsRealtime(1.1f);yield return Press(Key.C);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Settings","C abre Configuración con teclado");Call("ToMain");yield return new WaitForSecondsRealtime(1.1f);Call("MainSelect",1);yield return new WaitForSecondsRealtime(1.1f);yield return Tap(GamepadButton.Start);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Main","Start devuelve al menú principal");
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);while(stack.Count>0){object next=null;bool ended=false;Exception error=null;try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGMapDriver:MonoBehaviour {}
}
#endif
