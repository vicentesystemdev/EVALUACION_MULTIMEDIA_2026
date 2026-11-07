using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace CampusGlitch.EditorQA {
public static class CGIntegrationQA {
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Set(CGGame game,string field,object value){typeof(CGGame).GetField(field,flags).SetValue(game,value);}
 static object Get(CGGame game,string field){return typeof(CGGame).GetField(field,flags).GetValue(game);}
 static void Page(CGGame game,string value){Set(game,"page",Enum.Parse(typeof(CGGame).GetField("page",flags).FieldType,value));}
 static void Call(CGGame game,string method,params object[] args){typeof(CGGame).GetMethod(method,flags).Invoke(game,args);}
 static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
 static void Pump(){typeof(InputSystem).GetMethod("Update",BindingFlags.Static|BindingFlags.NonPublic,null,new[]{typeof(InputUpdateType)},null).Invoke(null,new object[]{InputUpdateType.Dynamic});}
 static void KeyPress(CGGame game,Keyboard keyboard,Key key){
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());Pump();
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));Pump();Call(game,"Update");
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());Pump();
 }
 static void PadPress(CGGame game,Gamepad pad,GamepadButton button){
   InputSystem.QueueStateEvent(pad,new GamepadState());Pump();
   InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));Pump();Call(game,"Update");
   InputSystem.QueueStateEvent(pad,new GamepadState());Pump();
 }
 [MenuItem("Campus Glitch/QA/Guardar captura de juego")]
 public static void Capture(){
   if(!Application.isPlaying)throw new Exception("La captura requiere Play Mode");
   Directory.CreateDirectory("Docs/QA/CarlaCapturas");
   ScreenCapture.CaptureScreenshot("Docs/QA/CarlaCapturas/"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png");
 }
 [MenuItem("Campus Glitch/QA/Comprobar entradas y persistencia aislada")]
 public static void RunAll(){
   var manager=typeof(InputSystem).GetField("s_Manager",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
   var editUpdates=manager.GetType().GetProperty("runPlayerUpdatesInEditMode",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
   var oldUpdates=editUpdates.GetValue(manager);editUpdates.SetValue(manager,true);
   var previous=CGStore.Data;var scenes=EditorBuildSettings.scenes;float scale=Time.timeScale;
   string scratch=Path.Combine(Path.GetTempPath(),"CampusCarlaQA-"+Guid.NewGuid().ToString("N"));
   var saveField=typeof(CGStore).GetField("TestSaveDirectory",BindingFlags.Static|BindingFlags.NonPublic);
   var oldDirectory=saveField.GetValue(null);
   var go=new GameObject("Isolated Carla QA");Keyboard keyboard=null;Gamepad pad=null;
   try{
     saveField.SetValue(null,scratch);EditorBuildSettings.scenes=new EditorBuildSettingsScene[0];
     var game=go.AddComponent<CGGame>();var world=go.AddComponent<CGWorld>();Set(game,"world",world);
     keyboard=InputSystem.AddDevice<Keyboard>();pad=InputSystem.AddDevice<Gamepad>();
     CGStore.Data=new CGProgress{latestUnlocked=3,purpleUnlocked=true};CGStore.Data.completed[0]=true;CGStore.Save();
     string file=Path.Combine(scratch,"campus_glitch_save_v1.json");string original=File.ReadAllText(file);
     Call(game,"MainSelect",7);KeyPress(game,keyboard,Key.Enter);Assert(Get(game,"page").ToString()=="Main","Enter no fue procesado");
     Assert(File.ReadAllText(file)==original,"Enter con Cancelar modifico el JSON");
     Call(game,"MainSelect",7);KeyPress(game,keyboard,Key.Escape);
     Assert(File.ReadAllText(file)==original,"Escape modifico el JSON");
     Call(game,"MainSelect",7);PadPress(game,pad,GamepadButton.East);
     Assert(File.ReadAllText(file)==original,"B modifico el JSON");
     CGStore.Data=null;CGStore.Load();Assert(CGStore.Data.latestUnlocked==3&&CGStore.Data.purpleUnlocked,"La cancelacion no persiste al recargar");
     Call(game,"MainSelect",7);Set(game,"selection",0);PadPress(game,pad,GamepadButton.South);
     CGStore.Data=null;CGStore.Load();Assert(CGStore.Data.latestUnlocked==0&&!CGStore.Data.purpleUnlocked,"Confirmar borrado no persiste");
     Debug.Log("[Carla integracion] OK: cancelar/Enter, Escape, B y borrado explicito con JSON temporal y recarga.");
     foreach(int level in new[]{0,7,5}){
       world.spec=CGLevels.All[level];Page(game,"Quiz");Set(game,"selection",(world.spec.right+1)%3);Set(game,"terminalSolved",false);Set(game,"nextQuizAnswerTime",-1f);Time.timeScale=0;
       KeyPress(game,keyboard,Key.Enter);Assert((string)Get(game,"page").ToString()=="Quiz"&&!(bool)Get(game,"terminalSolved"),"Respuesta incorrecta cerro la terminal");
       Set(game,"nextQuizAnswerTime",-1f);Set(game,"selection",world.spec.right);KeyPress(game,keyboard,Key.Enter);
       Assert(game.Playing&&Time.timeScale==1&&(bool)Get(game,"terminalSolved"),"Respuesta correcta no restauro la partida");
       Page(game,"Quiz");Set(game,"selection",2);Time.timeScale=0;KeyPress(game,keyboard,Key.DownArrow);
       Assert((int)Get(game,"selection")==3,"No se puede seleccionar Cerrar");KeyPress(game,keyboard,Key.Enter);
       Assert(game.Playing&&Time.timeScale==1,"Cerrar no restauro la partida");
       Page(game,"Quiz");Set(game,"selection",world.spec.right);Set(game,"nextQuizAnswerTime",-1f);Time.timeScale=0;PadPress(game,pad,GamepadButton.South);
       Assert(game.Playing&&Time.timeScale==1,"A no confirma la respuesta");
       Page(game,"Quiz");Time.timeScale=0;PadPress(game,pad,GamepadButton.East);Assert(game.Playing&&Time.timeScale==1,"B no cierra la terminal");
       Debug.Log("[Carla integracion] OK: terminal N"+(level+1)+" incorrecta/correcta/cierre, teclado y mando simulado.");
     }
     Page(game,"Map");Set(game,"selection",10);KeyPress(game,keyboard,Key.Enter);Assert(Get(game,"page").ToString()=="Main","Mapa no vuelve al menu");
     Page(game,"Controls");Set(game,"selection",9);KeyPress(game,keyboard,Key.Enter);Assert(Get(game,"page").ToString()=="Main","Controles no vuelve al menu");
     Page(game,"Controls");Set(game,"padMode",true);Set(game,"rebinding",0);string binding=CGStore.Data.padBindings[0];PadPress(game,pad,GamepadButton.East);
     Assert((int)Get(game,"rebinding")==-1&&CGStore.Data.padBindings[0]==binding,"B cambio la asignacion en lugar de cancelar");
     Page(game,"Victory");Set(game,"selection",1);PadPress(game,pad,GamepadButton.South);Assert(Get(game,"page").ToString()=="Main","Victoria ignora menu");
     Page(game,"Ending");Set(game,"selection",0);KeyPress(game,keyboard,Key.Enter);Assert(Get(game,"page").ToString()=="Credits","Final ignora creditos");
     Page(game,"Settings");Set(game,"selection",0);CGStore.Data.masterVolume=.5f;KeyPress(game,keyboard,Key.LeftArrow);Assert(Mathf.Abs(CGStore.Data.masterVolume-.4f)<.001f,"Izquierda no baja volumen");
     KeyPress(game,keyboard,Key.RightArrow);Assert(Mathf.Abs(CGStore.Data.masterVolume-.5f)<.001f,"Derecha no sube volumen");
     Debug.Log("[Carla integracion] OK: volver desde mapa/controles, cancelar reasignacion, victoria, creditos y volumen por entradas.");
     var heroObject=new GameObject("QA animation hero");heroObject.transform.SetParent(go.transform);var hero=heroObject.AddComponent<CGHero>();hero.Setup(game);
     var animator=heroObject.GetComponent<CGFrameAnimator>();
     foreach(bool originalWolf in new[]{false,true})foreach(bool purple in new[]{false,true}){
       CGStore.Data.useOriginalWolf=originalWolf;CGStore.Data.purpleEquipped=purple;
       for(int state=0;state<7;state++){
         hero.Grounded=state<3||state>4;hero.moveInput=state==1||state==2?1:0;hero.running=state==2;
         hero.Body.linearVelocity=new Vector2(0,state==3?1:state==4?-1:0);hero.attackUntil=state==5?Time.time+1:-1;hero.hurtUntil=state==6?Time.time+1:-1;
         animator.Update();int expected=state==0?0:state==1?2:state==2?6:state==3?10:state==4?(originalWolf?10:13):state==5?(originalWolf?13:15):(originalWolf?13:19);
         Assert(animator.view.sprite==CGArt.Player(purple,expected,originalWolf),"Estado de animacion incorrecto "+state);
       }
       CGStore.Data.reduceFlash=true;animator.Update();Assert(animator.view.color==Color.white,"Reducir destellos no evita tinte");CGStore.Data.reduceFlash=false;
     }
     Debug.Log("[Carla integracion] OK: 28 combinaciones de estado/modelo/skin y reduccion de destellos.");
     foreach(string prefix in new[]{"Classic_","ClassicPurple_","Red_","Purple_"}){
       int count=prefix.StartsWith("Classic")?17:21;
       for(int i=0;i<count;i++)Assert(Resources.Load<Sprite>("CG/Playable/"+prefix+i.ToString("00"))!=null,"Falta sprite "+prefix+i);
     }
     foreach(string sound in new[]{"Click","Music","Jump","Attack","Damage","Pickup","Checkpoint","Win","Denied"})Assert(Resources.Load<AudioClip>("CG/Sound/"+sound)!=null,"Falta audio "+sound);
     CGAudio.Describe("Damage");Assert(CGAudio.Caption=="[Golpe recibido]","Falta subtitulo de dano");
     Debug.Log("[Carla integracion] OK: sprites de los cuatro modelos/skins, nueve audios y subtitulo de dano.");
     CGMenuRegression.RunAll();
   }finally{
     editUpdates.SetValue(manager,oldUpdates);Time.timeScale=scale;CGStore.Data=previous;saveField.SetValue(null,oldDirectory);EditorBuildSettings.scenes=scenes;
     if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(pad!=null)InputSystem.RemoveDevice(pad);
     UnityEngine.Object.DestroyImmediate(go);
     // This directory was freshly created by this test and never points to a player's save.
     if(Path.GetDirectoryName(Path.GetFullPath(scratch))!=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar))throw new Exception("Unexpected QA directory");
     if(Directory.Exists(scratch))Directory.Delete(scratch,true);
   }
 }
}
}
