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
public static class CGCustomizationRegression {
    const string Pending="CampusGlitch.CustomizationRegression";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static GamepadState desired;static Key key;static int pass,fail;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){var info=typeof(CGGame).GetField(field,Private);return (T)(info!=null?info.GetValue(Game):typeof(CGGame).GetProperty(field,Private).GetValue(Game));}
    static void Set(string field,object value){typeof(CGGame).GetField(field,Private).SetValue(Game,value);}
    static object Call(string name,params object[] args){return typeof(CGGame).GetMethod(name,Private).Invoke(Game,args);}
    static CGCustomizationRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar escena y detener Play.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Customization.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();desired=default;key=Key.None;pass=fail=0;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;InputSystem.onAfterUpdate+=Drive;
            new GameObject("CustomizationQA").AddComponent<CGCustomizationDriver>().StartCoroutine(Guard(Suite()));
        }else if(mode==PlayModeStateChange.ExitingPlayMode){
            InputSystem.onAfterUpdate-=Drive;if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);pad=null;keyboard=null;
            InputSystem.settings.backgroundBehavior=(InputSettings.BackgroundBehavior)SessionState.GetInt(Pending+".Background",0);Application.runInBackground=SessionState.GetBool(Pending+".RunBackground",false);Time.timeScale=1;
        }else if(mode==PlayModeStateChange.EnteredEditMode){
            foreach(string name in Saves){string copy=Path.Combine(output,name),target=Path.Combine(Application.persistentDataPath,name);if(File.Exists(copy))File.Copy(copy,target,true);else if(File.Exists(target))File.Delete(target);}
            string scene=SessionState.GetString(Pending+".Scene","");SessionState.EraseString(Pending);CGStore.Data=null;EditorSceneManager.OpenScene(scene);File.AppendAllText(Path.Combine(output,"results.txt"),"Guardado original restaurado.\n");
        }
    }
    static void Drive(){if(pad==null||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;keyboard.MakeCurrent();InputState.Change(keyboard,key==Key.None?new KeyboardState():new KeyboardState(key));pad.MakeCurrent();InputState.Change(pad,desired);}
    static void Check(bool ok,string label){if(ok)pass++;else fail++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
    static IEnumerator Tap(GamepadButton button){desired=new GamepadState().WithButton(button);yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Press(Key value){key=value;yield return new WaitForSecondsRealtime(.08f);key=Key.None;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Suite(){
        yield return new WaitForSecondsRealtime(.5f);Check(Get<object>("page").ToString()=="Customization","Abre la escena de personalización");
        yield return Press(Key.V);Check(Get<bool>("customPadControls"),"V selecciona instrucciones de mando");yield return Tap(GamepadButton.North);Check(!Get<bool>("customPadControls"),"Y selecciona instrucciones de teclado");
        for(int id=0;id<4;id++)Check(CGAbilityArt.Card(id)!=CGAbilityArt.Icon(id),"Ilustración de poder cargada "+id);
        string initial=JsonUtility.ToJson(CGStore.Data);var files=new Dictionary<string,byte[]>();foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);files[name]=File.Exists(path)?File.ReadAllBytes(path):null;}
        yield return Tap(GamepadButton.RightShoulder);Check(Get<int>("customSection")==1,"RB abre habilidades");
        yield return Tap(GamepadButton.DpadDown);Check(Get<int>("customAbility")==1,"Cruceta selecciona ráfaga glitch");yield return Tap(GamepadButton.South);Check(Get<int>("draftAbility")==0,"Ráfaga bloqueada no se equipa");
        yield return Tap(GamepadButton.DpadDown);yield return Tap(GamepadButton.South);Check(Get<int>("draftAbility")==0&&!CGAbilities.Unlocked(CGStore.Data,2),"Escudo bloqueado no se equipa");
        yield return Tap(GamepadButton.West);Check(Get<int>("demonstration")==2&&Get<CGAbilityState>("demonstrationState").Active(2,CGMenuTheme.Clock),"X demuestra escudo sin desbloquearlo");
        Call("CustomAction",5);Check(Get<bool>("impact")&&!Get<CGAbilityState>("demonstrationState").Active(2,CGMenuTheme.Clock),"Impacto consume la protección demostrada");
        Set("customAbility",1);Set("selection",1);float energy=Get<CGAbilityState>("demonstrationState").Energy;yield return Press(Key.T);Check(Get<int>("demonstration")==1&&Get<CGAbilityState>("demonstrationState").Energy<energy,"T demuestra ráfaga y consume carga");energy=Get<CGAbilityState>("demonstrationState").Energy;yield return Press(Key.T);Check(Get<CGAbilityState>("demonstrationState").Energy==energy,"La recarga evita disparar repetidamente en la demostración");
        yield return Press(Key.Tab);Check(Get<int>("customSection")==2,"Tab abre evolución sin categoría Efectos");Call("CustomAction",8);Call("DemoAbility");Check(Get<int>("customMilestone")==9&&Get<int>("customAbility")==3,"N9 muestra el corte élite");
        yield return Tap(GamepadButton.RightShoulder);Check(Get<int>("customSection")==0,"RB recorre las tres categorías");
        Check(JsonUtility.ToJson(CGStore.Data)==initial,"Demostraciones e hitos futuros no modifican progreso");
        bool untouched=true;foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);byte[] previous=files[name];if(previous==null){untouched&=!File.Exists(path);}else untouched&=File.Exists(path)&&Convert.ToBase64String(previous)==Convert.ToBase64String(File.ReadAllBytes(path));}Check(untouched,"Observar y probar no escribe archivos de guardado");
        Call("CustomCategory",0);Call("CustomAction",1);Check(!Get<bool>("draftPurple")&&Get<bool>("inspectPurple"),"Variante bloqueada solo se observa");Call("CustomAction",3);Check(!CGStore.Data.useOriginalWolf&&Get<bool>("CustomDirty") ,"Elegir modelo mantiene cambios como borrador");
        yield return Tap(GamepadButton.East);Check(Get<bool>("customDiscard")&&Get<int>("discardChoice")==1,"Volver protege cambios con Seguir editando seleccionado");yield return Tap(GamepadButton.South);Check(!Get<bool>("customDiscard")&&Get<object>("page").ToString()=="Customization","A conserva el borrador al seguir editando");
        Call("BeginCustomization");CGStore.Data.completed[1]=true;CGStore.Data.latestUnlocked=2;CGStore.Data.purpleUnlocked=true;Call("BeginCustomization");Call("CustomCategory",1);Call("CustomAction",2);
        Check(CGStore.Data.equippedAbility==0,"Elegir poder no guarda automáticamente");Check((bool)Call("ApplyCustomization"),"Aplicar confirma el guardado");CGStore.Data=null;CGStore.Load();Check(CGStore.Data.equippedAbility==2,"Poder equipado persiste al cargar el archivo real");
        Call("BeginCustomization");Call("CustomCategory",1);Set("customAbility",1);Call("CustomAction",6);Check(CGStore.Data.equippedAbility==1,"Equipar usa el poder inspeccionado aunque no se pulsara la tarjeta");CGStore.Data=null;CGStore.Load();Check(CGStore.Data.equippedAbility==1,"Ráfaga equipada persiste al cargar");
        CGStore.Data=new CGProgress{equippedAbility=1};CGStore.Save();CGStore.Data=null;CGStore.Load();Check(CGStore.Data.equippedAbility==0,"Carga corrige un poder todavía bloqueado");
        CGStore.Data.equippedAbility=99;CGStore.Save();CGStore.Data=null;CGStore.Load();Check(CGStore.Data.equippedAbility==0,"Carga normaliza identificadores inválidos");
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);while(stack.Count>0){object next=null;bool ended=false;Exception error=null;try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGCustomizationDriver:MonoBehaviour {}
}
#endif
