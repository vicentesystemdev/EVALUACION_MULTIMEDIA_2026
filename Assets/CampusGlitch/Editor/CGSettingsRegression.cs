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
public static class CGSettingsRegression {
    const string Pending="CampusGlitch.SettingsRegression";
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Saves={"campus_glitch_save_v1.json","campus_glitch_save_v1.json.bak","campus_glitch_save_v1.json.tmp"};
    static string output;static Gamepad pad;static Keyboard keyboard;static Mouse mouse;static GamepadState desired;static Key key;static int pass,fail;
    static CGGame Game {get{return UnityEngine.Object.FindFirstObjectByType<CGGame>();}}
    static T Get<T>(string field){var info=typeof(CGGame).GetField(field,Private);return (T)(info!=null?info.GetValue(Game):typeof(CGGame).GetProperty(field,Private).GetValue(Game));}
    static void Set(string field,object value){typeof(CGGame).GetField(field,Private).SetValue(Game,value);}
    static object Call(string name,params object[] args){return typeof(CGGame).GetMethod(name,Private).Invoke(Game,args);}
    static CGSettingsRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar escena y detener Play.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);foreach(string name in Saves){string path=Path.Combine(Application.persistentDataPath,name);if(File.Exists(path))File.Copy(path,Path.Combine(output,name));}
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".VSync",QualitySettings.vSyncCount);SessionState.SetInt(Pending+".FPS",Application.targetFrameRate);SessionState.SetFloat(Pending+".Volume",AudioListener.volume);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Settings.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();desired=default;key=Key.None;pass=fail=0;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;InputSystem.onAfterUpdate+=Drive;
            var driver=new GameObject("SettingsQA");UnityEngine.Object.DontDestroyOnLoad(driver);driver.AddComponent<CGSettingsDriver>().StartCoroutine(Guard(Suite()));
        }else if(mode==PlayModeStateChange.ExitingPlayMode){
            InputSystem.onAfterUpdate-=Drive;if(pad!=null)InputSystem.RemoveDevice(pad);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);pad=null;keyboard=null;mouse=null;
            InputSystem.settings.backgroundBehavior=(InputSettings.BackgroundBehavior)SessionState.GetInt(Pending+".Background",0);Application.runInBackground=SessionState.GetBool(Pending+".RunBackground",false);Time.timeScale=1;
        }else if(mode==PlayModeStateChange.EnteredEditMode){
            foreach(string name in Saves){string copy=Path.Combine(output,name),target=Path.Combine(Application.persistentDataPath,name);if(File.Exists(copy))File.Copy(copy,target,true);else if(File.Exists(target))File.Delete(target);}
            string scene=SessionState.GetString(Pending+".Scene","");SessionState.EraseString(Pending);CGSettingsRuntime.Preview=null;QualitySettings.vSyncCount=SessionState.GetInt(Pending+".VSync",0);Application.targetFrameRate=SessionState.GetInt(Pending+".FPS",60);AudioListener.volume=SessionState.GetFloat(Pending+".Volume",.85f);CGStore.Data=null;EditorSceneManager.OpenScene(scene);File.AppendAllText(Path.Combine(output,"results.txt"),"Guardado original restaurado.\n");
        }
    }
    static void Drive(){if(pad==null||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;mouse.MakeCurrent();InputState.Change(mouse,new MouseState{position=new Vector2(-100,-100)});keyboard.MakeCurrent();InputState.Change(keyboard,key==Key.None?new KeyboardState():new KeyboardState(key));pad.MakeCurrent();InputState.Change(pad,desired);}
    static void Check(bool ok,string label){if(ok)pass++;else fail++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
    static IEnumerator Tap(GamepadButton button){desired=new GamepadState().WithButton(button);yield return new WaitForSecondsRealtime(.08f);desired=default;yield return new WaitForSecondsRealtime(.16f);}
    static IEnumerator Press(Key value){key=value;yield return new WaitForSecondsRealtime(.08f);key=Key.None;yield return new WaitForSecondsRealtime(.16f);}
    static CGSettingsDraft Draft {get{return Get<CGSettingsDraft>("settingsDraft");}}
    static bool Near(float a,float b){return Mathf.Abs(a-b)<.002f;}
    static IEnumerator Suite(){
        yield return new WaitForSecondsRealtime(.3f);Call("BeginSettings");yield return new WaitForSecondsRealtime(.3f);Check(Get<object>("page").ToString()=="Settings","Abre Configuración");Check(!Get<bool>("SettingsDirty"),"Abrir no genera cambios pendientes");
        yield return Press(Key.Tab);Check(Get<int>("settingsSection")==1,"Tab abre Audio");yield return Press(Key.Enter);Check(Draft.options.muted&&Near(AudioListener.volume,0)&&!CGStore.Data.settings.muted,"Silenciar actúa sin guardar");yield return Press(Key.Enter);Check(!Draft.options.muted&&Near(AudioListener.volume,.85f),"Desactivar silencio recupera el volumen");
        yield return Tap(GamepadButton.DpadDown);yield return Press(Key.LeftArrow);Check(Near(Draft.masterVolume,.8f)&&Near(CGStore.Data.masterVolume,.85f),"Teclado reduce volumen sin guardar automáticamente");
        Call("SetSettingsSlider",1,.25f);Call("SetSettingsSlider",2,.6f);Call("SetSettingsSlider",3,.2f);Call("SetSettingsSlider",4,.8f);yield return new WaitForSecondsRealtime(.12f);
        var audio=CGAudio.Instance;var music=(AudioSource)typeof(CGAudio).GetField("music",Private).GetValue(audio);var fx=(AudioSource)typeof(CGAudio).GetField("fx",Private).GetValue(audio);var ui=(AudioSource)typeof(CGAudio).GetField("ui",Private).GetValue(audio);
        Check(Near(AudioListener.volume,.25f)&&Near(music.volume,.24f)&&Near(fx.volume,.2f)&&Near(ui.volume,.8f),"General, música, juego y botones controlan salidas separadas");Check(CGAudio.Play("Attack")&&music.clip!=null&&music.isPlaying,"La prueba reproduce sonidos y música reales");yield return new WaitForSecondsRealtime(.12f);var spectrum=new float[32];CGAudio.Spectrum(spectrum);float sum=0;foreach(float f in spectrum)sum+=f;Check(sum>0,"El indicador recibe señal real de sonido");
        Call("SettingsChange",0,1);CGAudio.Spectrum(spectrum);sum=0;foreach(float f in spectrum)sum+=f;Check(Near(sum,0),"Silenciar deja el indicador sin señal");Call("SettingsChange",0,1);
        yield return Press(Key.Escape);Check(Get<int>("settingsDialog")==1&&Get<int>("settingsChoice")==1,"Salir con cambios selecciona Seguir ajustando");yield return Press(Key.Enter);Check(Get<int>("settingsDialog")==0&&Near(Draft.masterVolume,.25f),"Cancelar la salida conserva el borrador");Check((bool)Call("SaveSettings"),"Guardar confirma las opciones");CGStore.Data=null;CGStore.Load();Check(Near(CGStore.Data.masterVolume,.25f)&&Near(CGStore.Data.settings.musicVolume,.6f)&&Near(CGStore.Data.settings.interfaceVolume,.8f),"Opciones conservadas al volver a leer el archivo");
        Check(CGStore.Data.settings.width==0&&CGStore.Data.settings.screenMode==-1,"Guardar sonido conserva el modo de pantalla anterior");Call("BeginSettings");Check(!Get<bool>("SettingsDirty"),"Los valores guardados abren sin cambios pendientes");
        var legacy=JsonUtility.FromJson<CGProgress>("{\"masterVolume\":0.4,\"subtitles\":true}");CGSettingsRuntime.Normalize(legacy);Check(Near(legacy.masterVolume,.4f)&&Near(legacy.settings.musicVolume,1)&&Near(legacy.settings.brightness,1),"El progreso antiguo conserva su volumen y recibe opciones nuevas válidas");
        var invalid=new CGProgress{settings=new CGSettingsOptions{musicVolume=float.NaN,brightness=20,width=-1,height=2,hintSize=50}};CGSettingsRuntime.Normalize(invalid);Check(Near(invalid.settings.musicVolume,1)&&Near(invalid.settings.brightness,1.2f)&&invalid.settings.width==0&&invalid.settings.hintSize==2,"Opciones fuera de rango se corrigen antes de guardar");
        Call("SettingsSection",2);yield return Tap(GamepadButton.South);Check(!Draft.showHints&&CGStore.Data.subtitles,"A cambia consejos sin borrar el valor guardado");Call("SettingsChange",1,1);Call("SettingsChange",2,1);Call("SettingsChange",3,1);Check(Draft.options.hintSize==2&&CGSettingsRuntime.Options.solidText&&CGSettingsRuntime.ReduceFlash&&!CGStore.Data.reduceFlash,"Tamaño, fondos y reducción de destellos responden antes de guardar");Game.Hint("Conversación",true);Check(Get<bool>("toastCritical"),"Las conversaciones se conservan aunque los consejos estén apagados");
        var items=new List<CGUIDraw.Item>();CGUIDraw.Capture=items;try{CGUIDraw.ResetCaptureState();Game.DrawEditorMenu();}finally{CGUIDraw.Capture=null;}Check(items.Exists(i=>i.texture is RenderTexture t&&t.name=="Campus_Nivel_Actual"),"Scene dibuja una vista del nivel y no una imagen del menú");
        var world=CGSettingsWorldView.World;Check(world!=null&&world.index==0&&world.hero!=null&&world.GetComponentsInChildren<CGCampusNPC>().Length==3,"La vista genera el nivel real con el Guardián y sus personajes");bool still=true;foreach(var body in world.GetComponentsInChildren<Rigidbody2D>())still&=!body.simulated;Check(still&&!world.hero.enabled,"La vista mantiene la física y el control desactivados");Check(!world.gameObject.scene.GetPhysicsScene2D().Equals(Game.gameObject.scene.GetPhysicsScene2D()),"La vista tiene una escena física independiente");
        var main=Camera.main;var position=main.transform.position;float zoom=main.orthographicSize;int frames=CGSettingsWorldView.RenderedFrames;yield return new WaitForSecondsRealtime(.3f);Check(Camera.main==main&&main.transform.position==position&&Near(main.orthographicSize,zoom),"La vista no cambia la cámara de Configuración");Check(CGSettingsWorldView.RenderedFrames>frames,"El nivel se actualiza con las animaciones");
        var target=(RenderTexture)CGSettingsWorldView.Get(Game);var previous=RenderTexture.active;var probe=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);try{RenderTexture.active=target;probe.ReadPixels(new Rect(0,0,target.width,target.height),0,0);probe.Apply();var colors=new HashSet<Color32>();var pixels=probe.GetPixels32();for(int i=0;i<pixels.Length;i+=997)colors.Add(pixels[i]);int lit=0;foreach(var pixel in pixels)if(pixel.r+pixel.g+pixel.b>60)lit++;Check(colors.Count>24&&lit>pixels.Length/6,"La cámara del nivel muestra el escenario iluminado y no un cuadro vacío");File.WriteAllBytes(Path.Combine(output,"Vista_juego.png"),probe.EncodeToPNG());}finally{RenderTexture.active=previous;UnityEngine.Object.Destroy(probe);}
        Call("OpenSettingsDialog",2);Call("FinishSettingsDialog",false);Check(Draft.options.solidText,"Cancelar restablecimiento conserva el borrador");Call("OpenSettingsDialog",2);Call("FinishSettingsDialog",true);Check(Near(Draft.masterVolume,.85f)&&Near(CGStore.Data.masterVolume,.25f)&&!Draft.options.solidText,"Restablecer prepara opciones originales sin guardarlas ni borrar avance");
        CGStore.Data.completed[0]=true;CGStore.Data.latestUnlocked=1;CGStore.Data.actionKeys[0]="L";Call("SettingsSection",3);Set("selection",5);yield return Tap(GamepadButton.South);Check(Get<int>("settingsDialog")==4&&Get<int>("settingsChoice")==1,"Borrar avance abre una confirmación con Cancelar elegido");yield return Tap(GamepadButton.South);Check(CGStore.Data.completed[0]&&CGStore.Data.latestUnlocked==1,"Cancelar conserva el avance");
        Call("OpenSettingsDialog",4);Call("FinishSettingsDialog",true);Check(!CGStore.Data.completed[0]&&CGStore.Data.latestUnlocked==0,"Confirmar comienza de nuevo");Check(CGStore.Data.actionKeys[0]=="L"&&Near(CGStore.Data.masterVolume,.25f)&&Near(CGStore.Data.settings.musicVolume,.6f),"Borrar avance conserva sonido, pantalla y botones");
        Call("BeginSettings");string before=JsonUtility.ToJson(CGStore.Data);Set("displayBackup",new CGSettingsOptions{width=1280,height=720,screenMode=0});Set("displayTrial",true);Draft.options.width=1920;Draft.options.height=1080;Call("OpenSettingsDialog",3);Set("settingsConfirmUntil",Time.unscaledTime-1);yield return new WaitForSecondsRealtime(.12f);Check(Get<int>("settingsDialog")==0&&!Get<bool>("displayTrial")&&Draft.options.width==1280,"Caducar la confirmación recupera los valores anteriores de pantalla");Check(JsonUtility.ToJson(CGStore.Data)==before&&!CGSettingsRuntime.CanChangeDisplay,"La caducidad no guarda ni cambia la pantalla física en el editor");
        desired=new GamepadState().WithButton(GamepadButton.DpadUp);Call("OpenSettingsDialog",4);yield return new WaitForSecondsRealtime(.45f);Check(Get<int>("settingsChoice")==1,"Una dirección mantenida no confirma un borrado por arrastre");desired=default;yield return new WaitForSecondsRealtime(.12f);yield return Tap(GamepadButton.South);Check(Get<int>("settingsDialog")==0,"Confirmar la opción segura cierra el aviso");
        Call("BeginSettings");Call("SettingsSection",1);Set("selection",4);desired=new GamepadState().WithButton(GamepadButton.DpadRight);yield return new WaitForSecondsRealtime(.72f);desired=default;yield return new WaitForSecondsRealtime(.12f);Check(Draft.options.muted,"Mantener dirección cambia un interruptor una sola vez");
        Call("BeginSettings");Call("SettingsSection",0);Call("SetSettingsSlider",2,.4f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"Configuracion_Play.png"));yield return new WaitForSecondsRealtime(.35f);yield return Press(Key.Escape);Check(Get<int>("settingsDialog")==1,"Volver avisa sobre los cambios de pantalla pendientes");yield return Press(Key.LeftArrow);yield return Press(Key.Enter);yield return new WaitForSecondsRealtime(1.3f);Check(SceneManager.GetActiveScene().name=="SC_MainMenu"&&CGSettingsRuntime.Preview==null&&CGSettingsWorldView.World==null,"Salir sin guardar recupera el menú, el sonido y libera la vista del nivel");
        int orphan=0,global=0;foreach(var w in Resources.FindObjectsOfTypeAll<CGWorld>())if(w.name=="Campus_Vista_Del_Juego")orphan++;foreach(var light in Resources.FindObjectsOfTypeAll<UnityEngine.Rendering.Universal.Light2D>())if(light.isActiveAndEnabled&&light.lightType==UnityEngine.Rendering.Universal.Light2D.LightType.Global)global++;Check(orphan==0,"Salir no deja mundos temporales ocultos");Check(global==1,"Solo queda la luz global de la pantalla actual");
        Call("MainSelect",3);yield return new WaitForSecondsRealtime(1.2f);Check(Get<object>("page").ToString()=="Settings"&&!Get<bool>("SettingsDirty"),"Configuración vuelve a abrir con los valores guardados");Call("SettingsSection",3);yield return Tap(GamepadButton.South);yield return new WaitForSecondsRealtime(1.3f);Check(SceneManager.GetActiveScene().name=="SC_Controls"&&Get<object>("page").ToString()=="Controls","Elegir teclas y botones abre la pantalla real de Controles");
    }
    static IEnumerator Guard(IEnumerator suite){
        var stack=new Stack<IEnumerator>();stack.Push(suite);while(stack.Count>0){object next=null;bool ended=false;Exception error=null;try{if(!stack.Peek().MoveNext())ended=true;else next=stack.Peek().Current;}catch(Exception ex){error=ex;}if(error!=null){Check(false,error.ToString());break;}if(ended){stack.Pop();continue;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}
        File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;
    }
}
public class CGSettingsDriver:MonoBehaviour {}
}
#endif
