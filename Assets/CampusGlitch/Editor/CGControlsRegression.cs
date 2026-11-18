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
public static class CGControlsRegression {
    const string Pending="CampusGlitch.ControlsRegression";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static Mouse mouse;static GamepadState desired;static Key key;static int pass,fail;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){var info=typeof(CGGame).GetField(field,Private);return (T)(info!=null?info.GetValue(Game):typeof(CGGame).GetProperty(field,Private).GetValue(Game));}
    static void Set(string field,object value){typeof(CGGame).GetField(field,Private).SetValue(Game,value);}
    static object Call(string name,params object[] args){return typeof(CGGame).GetMethod(name,Private).Invoke(Game,args);}
    static CGControlsRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar escena y detener Play.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Controls.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();desired=default;key=Key.None;pass=fail=0;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;InputSystem.onAfterUpdate+=Drive;
            new GameObject("CustomizationQA").AddComponent<CGControlsDriver>().StartCoroutine(Guard(Suite()));
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
    static IEnumerator Trigger(bool left){desired=left?new GamepadState{leftTrigger=1}:new GamepadState{rightTrigger=1};yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Suite(){
        yield return new WaitForSecondsRealtime(.5f);Check(Get<object>("page").ToString()=="Controls","Abre Controles");
        Check((string)Call("PadCaption","buttonSouth")=="A","Xbox: botón inferior A");Call("SelectControllerFamily",1);Check((string)Call("PadCaption","buttonSouth")=="CRUZ"&&(string)Call("PadCaption","leftTrigger")=="L2","PlayStation: símbolos y gatillos correctos");Call("SelectControllerFamily",0);
        Set("selection",0);Call("ControlsMode",false);var original=JsonUtility.ToJson(CGStore.Data);yield return Tap(GamepadButton.South);Check(Get<int>("rebinding")==0,"A inicia captura de la acción seleccionada: captura="+Get<int>("rebinding")+", selección="+Get<int>("selection"));yield return Press(Key.K);Check(Get<int>("rebinding")==-1&&Get<string[]>("controlsKeys")[0]=="K","Tecla se prepara en el borrador");Check(CGStore.Data.actionKeys[0]=="A","Asignar no guarda automáticamente");
        Call("StartControlCapture",0);yield return new WaitForSecondsRealtime(.12f);yield return Press(Key.D);Check(Get<object>("controlsDialog").ToString()=="Conflict"&&Get<int>("controlsDialogChoice")==1,"Duplicado abre conflicto con Cancelar seleccionado");yield return Tap(GamepadButton.South);Check(Get<string[]>("controlsKeys")[0]=="K"&&Get<string[]>("controlsKeys")[1]=="D","Cancelar conflicto conserva ambas asignaciones");
        Call("StartControlCapture",0);yield return new WaitForSecondsRealtime(.12f);yield return Press(Key.D);yield return Tap(GamepadButton.DpadUp);yield return Tap(GamepadButton.South);Check(Get<string[]>("controlsKeys")[0]=="D"&&Get<string[]>("controlsKeys")[1]=="K","Confirmar intercambia ambas teclas sin duplicarlas");Check((bool)Call("SaveControls"),"Aplicar guarda borradores válidos");CGStore.Data=null;CGStore.Load();Check(CGStore.Data.actionKeys[0]=="D"&&CGStore.Data.actionKeys[1]=="K","Teclas persisten al cargar el archivo real");
        Call("ControlAction",8);Call("FinishControlDialog",false);Check(Get<string[]>("controlsKeys")[0]=="D","Cancelar restauración mantiene el borrador");Call("ControlAction",8);Call("FinishControlDialog",true);Check(Get<string[]>("controlsKeys")[0]=="A"&&CGStore.Data.actionKeys[0]=="D","Restaurar prepara valores originales sin guardarlos");Check((bool)Call("SaveControls"),"Aplicar restaura los valores originales");
        yield return Press(Key.Tab);Check(Get<bool>("padMode"),"Tab selecciona mando");Call("StartControlCapture",6);yield return new WaitForSecondsRealtime(.12f);yield return Trigger(true);Check(Get<string[]>("controlsPads")[6]=="leftTrigger","Gatillo analógico se puede asignar");Check((bool)Call("SaveControls"),"Asignación de gatillo se guarda");
        Set("controlsPractice",true);desired=new GamepadState{leftTrigger=1};yield return new WaitForSecondsRealtime(.04f);Check(CGControl.PadHeld(6),"Gatillo asignado responde mientras se mantiene");Check((float)Call("PadGlow","leftTrigger")>.8f,"Pulsación enciende el gatillo en el esquema");yield return new WaitForSecondsRealtime(.1f);Check(!CGControl.Ability(),"Mantener gatillo no repite la activación de habilidad");desired=default;yield return new WaitForSecondsRealtime(.12f);
        yield return Tap(GamepadButton.North);Check(Get<bool>("controlsPractice"),"Y dentro de prueba no edita ni abandona el diagnóstico");yield return Tap(GamepadButton.East);Check(!Get<bool>("controlsPractice"),"B termina la prueba");
        Call("StartControlCapture",6);yield return new WaitForSecondsRealtime(.12f);yield return Tap(GamepadButton.East);Check(Get<int>("rebinding")==-1&&Get<string[]>("controlsPads")[6]=="leftTrigger","B cancela captura sin guardar otro botón");
        Call("ControlsMode",false);Call("StartControlCapture",2);Set("captureStarted",Time.unscaledTime-21);yield return null;Check(Get<int>("rebinding")==-1,"La captura caduca sin alterar controles");
        Get<string[]>("controlsKeys")[0]="D";Check(!(bool)Call("SaveControls"),"Guardar rechaza duplicados introducidos externamente");Call("BeginControls");Get<string[]>("controlsKeys")[0]="L";Call("LeaveControls");Check(Get<object>("controlsDialog").ToString()=="Discard"&&Get<int>("controlsDialogChoice")==1,"Salir con borrador selecciona Seguir editando");Call("FinishControlDialog",false);Check(Get<string[]>("controlsKeys")[0]=="L","Seguir editando conserva el borrador");
        var sony=InputSystem.AddDevice<UnityEngine.InputSystem.DualShock.DualShockGamepad>();sony.MakeCurrent();Call("SyncControlDevice");Check(Get<bool>("ControlsPlayStation"),"Mando PlayStation detectado cambia el esquema sin opción Auto");InputSystem.RemoveDevice(sony);pad.MakeCurrent();
        var xbox=InputSystem.AddDevice<UnityEngine.InputSystem.XInput.XInputController>();xbox.MakeCurrent();Call("SyncControlDevice");Check(!Get<bool>("ControlsPlayStation"),"Mando Xbox detectado cambia el esquema");InputSystem.RemoveDevice(xbox);pad.MakeCurrent();
        Call("ControlsMode",false);Set("controlsPractice",true);key=Key.J;yield return new WaitForSecondsRealtime(.06f);Check((float)Call("KeyboardGlow","J")>.8f&&CGStore.Data.actionKeys[4]=="J","Tecla real anima su botón y conserva la acción");key=Key.None;yield return new WaitForSecondsRealtime(.07f);Check((float)Call("KeyboardGlow","J")>0&&(float)Call("KeyboardGlow","J")<1,"Animación de teclado decae al soltar");Set("controlsPractice",false);
        Call("StartControlCapture",4);Call("ChooseKeyboardKey","U");Check(Get<int>("rebinding")==-1&&Get<string[]>("controlsKeys")[4]=="U"&&CGStore.Data.actionKeys[4]=="J","Elegir tecla en el esquema prepara un borrador");
        Call("StartControlCapture",4);Call("ChooseKeyboardKey","Escape");Check(Get<int>("rebinding")==-1&&Get<string[]>("controlsKeys")[4]=="U","Escape del esquema cancela sin reasignar");
        Check(CGControlsArt.KeyboardBody!=null&&CGControlsArt.KeyboardStates!=null&&CGControlsArt.KeyboardKeys.Length>60,"Teclado carga carcasa, estados y distribución editable");
        Check(CGControlsArt.Controller(false)!=null&&CGControlsArt.Controller(true)!=null&&CGControlsArt.ButtonStates!=null,"Arte de carcasa y estados cargado en Unity");
        var reader=UnityEditor.U2D.Aseprite.AsepriteReader.ReadFile(Path.GetFullPath("ArtSource/Controles/CampusController.aseprite"));Check(reader!=null,"Lector oficial de Unity acepta el Aseprite");if(reader!=null){Check(reader.width==512&&reader.height==300&&reader.noOfFrames==8,"Aseprite conserva dimensiones y ocho estados");int layers=0,cels=0;foreach(var frame in reader.frameData)foreach(var chunk in frame.chunks){if(chunk is UnityEditor.U2D.Aseprite.LayerChunk)layers++;if(chunk is UnityEditor.U2D.Aseprite.CellChunk)cels++;}Check(layers==8&&cels>=50,"Aseprite contiene ocho capas editables y cels");reader.Dispose();}
        reader=UnityEditor.U2D.Aseprite.AsepriteReader.ReadFile(Path.GetFullPath("ArtSource/Controles/CampusKeyboard.aseprite"));Check(reader!=null&&reader.noOfFrames==4,"Lector oficial acepta el teclado Aseprite y sus cuatro estados");if(reader!=null)reader.Dispose();
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);while(stack.Count>0){object next=null;bool ended=false;Exception error=null;try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGControlsDriver:MonoBehaviour {}
}
#endif
