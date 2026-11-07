using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CampusGlitch.EditorQA {
[InitializeOnLoad]
public static class CGPlaySessionQA {
 const string Flag="Carla.PlayQA",Dir="Carla.PlayQADir",Previous="Carla.PreviousScene";
 static readonly int[] levels={0,7,5,8,9};
 static int index,stage;static double next,deadline;static bool ready;
 static BindingFlags fields=BindingFlags.Instance|BindingFlags.NonPublic;
 static string Output="Docs/QA/CarlaPlayMode";
 static CGPlaySessionQA(){EditorApplication.playModeStateChanged+=Changed;EditorApplication.update+=Tick;}
 [MenuItem("Campus Glitch/QA/Recorrer escenarios funcionales en Play Mode %#F9")]
 public static void Begin(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Salir de Play Mode antes de iniciar QA");
   if(SceneManager.GetActiveScene().isDirty)throw new Exception("Guardar la escena antes de iniciar QA");
   Directory.CreateDirectory(Output);File.WriteAllText(Output+"/resultados.txt","Unity 6000.3.16f1 — escenarios controlados, no recorrido manual ni prueba fisica de mando/Android.\n");
   SessionState.SetString(Previous,SceneManager.GetActiveScene().path);
   SessionState.SetString(Dir,Path.Combine(Path.GetTempPath(),"CarlaPlayQA-"+Guid.NewGuid().ToString("N")));
   SessionState.SetBool(Flag,true);
   EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Level01.unity");
   EditorApplication.isPlaying=true;
 }
 static void Changed(PlayModeStateChange state){
   if(!SessionState.GetBool(Flag,false))return;
   if(state==PlayModeStateChange.EnteredPlayMode){
     typeof(CGStore).GetField("TestSaveDirectory",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,SessionState.GetString(Dir,""));
     CGStore.Data=new CGProgress{latestUnlocked=8,extraUnlocked=true};CGStore.Save();
     index=0;stage=0;ready=true;next=EditorApplication.timeSinceStartup+1;deadline=next+20;
   }
   if(state==PlayModeStateChange.EnteredEditMode){
     SessionState.SetBool(Flag,false);ready=false;
     typeof(CGStore).GetField("TestSaveDirectory",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,null);CGStore.Data=null;
     string scratch=SessionState.GetString(Dir,"");
     if(Path.GetDirectoryName(Path.GetFullPath(scratch))!=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar))throw new Exception("Invalid QA scratch path");
     if(Directory.Exists(scratch))Directory.Delete(scratch,true);
     string previous=SessionState.GetString(Previous,"");if(File.Exists(previous))EditorSceneManager.OpenScene(previous);
   }
 }
 static object Get(CGGame g,string name){return typeof(CGGame).GetField(name,fields).GetValue(g);}
 static void Set(CGGame g,string name,object value){typeof(CGGame).GetField(name,fields).SetValue(g,value);}
 static void Call(CGGame g,string name,params object[] args){typeof(CGGame).GetMethod(name,fields).Invoke(g,args);}
 static void Check(bool value,string reason){if(!value)throw new Exception(reason);}
 static void Move(CGGame g,float x,float y=.2f){g.Hero.transform.position=new Vector3(x,y,0);g.Hero.Body.position=new Vector2(x,y);g.Hero.Body.linearVelocity=Vector2.zero;g.Hero.hurtUntil=Time.time+999;Physics2D.SyncTransforms();}
 static void Log(string message){File.AppendAllText(Output+"/resultados.txt",message+"\n");Debug.Log("[Carla Play QA] "+message);}
 static void Advance(){stage++;next=EditorApplication.timeSinceStartup+.55;deadline=next+20;}
 static void Tick(){
   if(!ready||!EditorApplication.isPlaying||EditorApplication.isPaused)return;
   double now=EditorApplication.timeSinceStartup;if(now<next)return;
   try{
     if(now>deadline)throw new Exception("Tiempo agotado esperando etapa "+stage);
     if(stage==10){
       string expected=index==0?"SC_Level02":levels[index]==8?"SC_Credits":"SC_MainMenu";
       if(SceneManager.GetActiveScene().name!=expected)return;
       Log("PASS N"+(levels[index]+1)+": transicion final a "+expected);
       if(++index==levels.Length){Log("PASS: cinco escenarios terminados; guardado personal sin cambios.");ready=false;EditorApplication.isPlaying=false;return;}
       stage=0;SceneManager.LoadScene(CGLevels.Scene(levels[index]));next=now+1;deadline=now+20;return;
     }
     var game=UnityEngine.Object.FindFirstObjectByType<CGGame>();
     if(game==null||game.sceneLevel!=levels[index]||game.Hero==null)return;
     var world=(CGWorld)Get(game,"world");
     switch(stage){
       case 0:Move(game,35);game.InteractNearPlayer();Check(world.checkpoint,"Checkpoint no se activa");game.Hero.hearts=1;game.KillHero();Check(game.Hero.hearts==3&&Mathf.Abs(game.Hero.transform.position.x-35.5f)<.1f,"Respawn no restaura checkpoint");break;
       case 1:Move(game,69);game.InteractNearPlayer();Check(game.Playing&&!(bool)Get(game,"terminalSolved"),"Portal no bloquea acceso incompleto");break;
       case 2:Move(game,14,.3f);break;
       case 3:Check((int)Get(game,"chips")==1,"No se recoge fragmento 1 por collider");Move(game,31,2.55f);break;
       case 4:Check((int)Get(game,"chips")==2,"No se recoge fragmento 2 por collider");Move(game,48,.3f);break;
       case 5:Check((int)Get(game,"chips")==3,"No se recoge fragmento 3 por collider");Move(game,40);game.InteractNearPlayer();Check(Get(game,"page").ToString()=="Quiz"&&Time.timeScale==0,"Terminal no pausa");break;
       case 6:Set(game,"nextQuizAnswerTime",-1f);Call(game,"QuizAnswer",(world.spec.right+1)%3);Check(Get(game,"page").ToString()=="Quiz","Respuesta incorrecta cierra terminal");Set(game,"nextQuizAnswerTime",-1f);Call(game,"QuizAnswer",world.spec.right);Check(game.Playing&&Time.timeScale==1,"Respuesta correcta no restaura partida");break;
       case 7:Move(game,69);game.InteractNearPlayer();if(!string.IsNullOrEmpty(world.spec.boss)){Check(game.Playing,"Portal no exige jefe");game.BossDown();game.InteractNearPlayer();}Check(CGStore.Data.completed[levels[index]],"Victoria no guarda nivel completado");CGStore.Data=null;CGStore.Load();Check(CGStore.Data.completed[levels[index]],"Victoria no persiste tras recarga");break;
       case 8:Check((string)Get(game,"toast")=="","Victoria conserva aviso anterior");Check(Get(game,"page").ToString()==(levels[index]==8?"Ending":"Victory"),"Pantalla final incorrecta");ScreenCapture.CaptureScreenshot(Output+"/N"+(levels[index]+1)+"_final.png");Log("PASS N"+(levels[index]+1)+": checkpoint, respawn, tres triggers de fragmentos, terminal incorrecta/correcta, puertas de acceso, victoria y recarga JSON temporal. Jefe liberado por callback de QA; no combate manual.");break;
       case 9:if(index==0)Call(game,"AfterVictory");else if(levels[index]==8)Call(game,"OpenEndingCredits");else Call(game,"ToMain");break;
     }
     Advance();
   }catch(Exception e){Log("FAIL: "+e);ready=false;Time.timeScale=1;EditorApplication.isPlaying=false;}
 }
}
}
