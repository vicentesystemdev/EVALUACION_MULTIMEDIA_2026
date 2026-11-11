#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
[InitializeOnLoad]
public static class CGAbilityRegression {
    const string Pending="CampusGlitch.AbilityRegression";static string output;static int pass,fail;
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static CGAbilityRegression(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&!string.IsNullOrEmpty(SessionState.GetString(Pending,"")))Execute();};}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Detener Play y guardar cambios de escena.");
        output=Path.GetFullPath(directory);if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir una carpeta nueva externa.");
        Directory.CreateDirectory(output);SessionState.SetString(Pending,output);EditorApplication.isPlaying=true;
    }
    static void Check(bool valid,string name){if(valid)pass++;else fail++;File.AppendAllText(Path.Combine(output,"results.txt"),(valid?"PASS ":"FAIL ")+name+"\n");}
    static void Execute(){
        output=SessionState.GetString(Pending,"");SessionState.EraseString(Pending);pass=fail=0;var original=CGStore.Data;Scene fixture=default;GameObject root=null;
        try{
            var firstButton=CGMenuArtwork.Button(false,new Color(1,.56f,.12f));var alternateButton=CGMenuArtwork.Button(true,new Color(.8f,.35f,1));Check(firstButton!=null&&alternateButton!=null&&firstButton!=alternateButton,"Diferentes acentos conservan las texturas anteriores para Scene");Check(CGMenuArtwork.Button(false,new Color(1,.56f,.12f))==firstButton,"Botones reutilizan caché por acento");
            var progress=new CGProgress();Check(CGAbilities.Unlocked(progress,0),"Dash disponible al inicio");
            for(int id=1;id<4;id++)Check(!CGAbilities.Unlocked(progress,id),"Habilidad bloqueada al inicio "+id);
            Check(!CGAbilities.Unlocked(progress,-1)&&!CGAbilities.Unlocked(progress,4),"Identificadores inválidos rechazados");
            for(int level=1;level<=9;level++){
                progress.completed[level-1]=true;int id=CGAbilities.RewardAbility[level-1];Check(CGAbilities.Completed(progress,level),"Hito real N"+level);
                Check(CGAbilities.Unlocked(progress,id),"Recompensa disponible al completar N"+level);
            }
            var basic=new CGProgress();var elite=progress;
            Check(CGAbilities.Stats(elite,0).speed>CGAbilities.Stats(basic,0).speed&&CGAbilities.Stats(elite,0).cooldown<CGAbilities.Stats(basic,0).cooldown,"Dash mejora velocidad y recarga");
            Check(CGAbilities.Stats(elite,1).damage>CGAbilities.Stats(basic,1).damage&&CGAbilities.Stats(elite,1).range>CGAbilities.Stats(basic,1).range,"N7 mejora daño y alcance de la ráfaga");
            Check(CGAbilities.Stats(elite,2).duration>CGAbilities.Stats(basic,2).duration,"N5 amplía la protección");
            Check(CGAbilities.Stats(elite,3).damage==3&&CGAbilities.Stats(elite,3).range>CGAbilities.Stats(basic,3).range,"N6 y N9 mejoran daño y alcance");
            var state=new CGAbilityState();Check(!state.TryActivate(2,false,0),"No activa poderes bloqueados");Check(state.TryActivate(0,true,10),"Dash se activa");Check(state.Active(0,10.1f)&&!state.Active(0,10.23f),"Duración de dash limitada");Check(!state.TryActivate(0,true,11),"Recarga impide spam");Check(state.TryActivate(0,true,11.8f),"Recupera dash al terminar recarga");
            Check(state.TryActivate(2,true,20),"Escudo se activa");Check(state.Absorb(20.5f)&&!state.Absorb(20.6f),"Escudo absorbe un único impacto");Check(!state.TryActivate(2,true,21),"Escudo conserva recarga tras impacto");state.Cancel();Check(!state.Active(2,21)&&state.Remaining(2,21)>0,"Cancelar no borra recargas");
            Check(state.Energy==20,"Poderes consumen sus puntos de carga");Check(!state.TryActivate(2,true,30)&&state.Energy==20,"Carga insuficiente bloquea el poder sin gastar más energía");state.Tick(.5f);Check(state.Energy==20,"La regeneración espera tras utilizar un poder");state.Tick(2);Check(state.Energy>20&&state.Energy<100,"La carga se recupera gradualmente");state.Tick(100);Check(state.Energy==100,"La carga tiene un máximo de 100");
            var legacy=JsonUtility.FromJson<CGProgress>("{\"latestUnlocked\":2,\"purpleUnlocked\":true}");Check(legacy.equippedAbility==0,"Guardado anterior conserva valores por defecto");
            elite.equippedAbility=3;var copy=JsonUtility.FromJson<CGProgress>(JsonUtility.ToJson(elite));Check(copy.equippedAbility==3&&copy.completed[8],"Personalización y progreso conservados en serialización");
            Check(!CGAbilities.Completed(new CGProgress{latestUnlocked=8},9),"Desbloquear N9 no equivale a haberlo completado");
            CGStore.Data=new CGProgress{latestUnlocked=3,purpleUnlocked=true};
            fixture=SceneManager.CreateScene("AbilityFixture_"+Guid.NewGuid().ToString("N"),new CreateSceneParameters(LocalPhysicsMode.Physics2D));root=new GameObject("AbilityFixture");SceneManager.MoveGameObjectToScene(root,fixture);
            var game=root.AddComponent<CGGame>();game.enabled=false;var world=root.AddComponent<CGWorld>();world.hero=null;typeof(CGGame).GetField("world",Private).SetValue(game,world);var page=typeof(CGGame).GetField("page",Private);page.SetValue(game,Enum.Parse(page.FieldType,"Playing"));
            var heroObj=new GameObject("Hero");heroObj.transform.SetParent(root.transform,false);var hero=heroObj.AddComponent<CGHero>();hero.Setup(game);world.hero=hero;
            foreach(var script in hero.GetComponentsInChildren<MonoBehaviour>())script.enabled=false;
            Box(root,"Floor",new Vector2(0,-.1f),new Vector2(20,.2f));var wall=Box(root,"Wall",new Vector2(1.8f,1.2f),new Vector2(.2f,2.4f));var physics=fixture.GetPhysicsScene2D();hero.Body.position=new Vector2(0,.02f);Physics2D.SyncTransforms();
            Check(hero.ActivateAbility(0),"Controlador real activa dash");
            for(int i=0;i<24;i++){typeof(CGHero).GetMethod("FixedUpdate",Private).Invoke(hero,null);physics.Simulate(.02f);}
            Check(hero.Body.position.x<1.4f&&hero.Body.position.x>.6f,"Dash avanza y se detiene ante collider sólido");
            Check(hero.GetComponent<CapsuleCollider2D>().size==new Vector2(.76f,1.64f),"Cápsula aprobada conservada");
            hero.Skills.Cancel();hero.Body.position=new Vector2(0,.02f);hero.Body.linearVelocity=Vector2.zero;Check(hero.ActivateAbility(2),"Controlador activa escudo");typeof(CGAbilityVisual).GetMethod("LateUpdate",Private).Invoke(hero.GetComponent<CGAbilityVisual>(),null);var dome=hero.transform.Find("Escudo_Digital").GetComponent<SpriteRenderer>();Check(dome.enabled&&dome.bounds.size.y>1.64f&&dome.bounds.size.x>.76f,"Cúpula visual envuelve la cápsula del lobo");int life=hero.hearts;hero.Damage(1);Check(hero.hearts==life,"Escudo evita pérdida de vida");hero.Damage(1);Check(hero.hearts==life-1,"El siguiente impacto consume vida");
            var target=new GameObject("Target");target.transform.SetParent(root.transform,false);target.transform.position=new Vector3(1.2f,.8f,0);var enemy=target.AddComponent<CGEnemy>();enemy.Setup(game,"Bug_Terrestre",false);enemy.hp=9;enemy.enabled=false;target.AddComponent<BoxCollider2D>().isTrigger=true;hero.Body.position=new Vector2(0,.02f);hero.Body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
            Check(hero.ActivateAbility(3),"Controlador activa corte");Check(enemy.hp==7,"Dos colliders no duplican daño del corte");
            target.transform.position=new Vector3(2.1f,.8f,0);Physics2D.SyncTransforms();game.AttackPulse(new Vector3(1.1f,.8f,0),1.3f,2);Check(enemy.hp==7,"Ataque no atraviesa obstáculos sólidos");
            wall.enabled=false;Physics2D.SyncTransforms();game.AttackPulse(new Vector3(1.1f,.8f,0),1.3f,2);Check(enemy.hp==5,"Sin obstáculo el objetivo recibe daño");
            target.transform.position=new Vector3(-.2f,.8f,0);Physics2D.SyncTransforms();game.AttackPulse(new Vector3(1.1f,.8f,0),1.3f,2);Check(enemy.hp==5,"Corte frontal no golpea a espaldas del personaje");
            hero.Skills.Tick(30);enemy.hp=9;target.transform.position=new Vector3(3.5f,.95f,0);wall.enabled=true;Physics2D.SyncTransforms();Check(hero.ActivateAbility(1),"Controlador dispara la ráfaga real");var shot=root.GetComponentInChildren<CGPowerProjectile>();shot.Advance(.5f);Check(shot.Finished&&enemy.hp==9,"Proyectil se detiene ante un obstáculo");Check(hero.Skills.Energy==82,"Ráfaga consume 18 puntos de carga");
            wall.enabled=false;Physics2D.SyncTransforms();shot=CGPowerProjectile.Spawn(game,hero,CGAbilities.Stats(CGStore.Data,1));shot.Advance(.5f);shot.Advance(.2f);Check(shot.Finished&&enemy.hp==8,"Proyectil causa daño una vez aunque haya dos colliders");
            hero.facesRight=false;target.transform.position=new Vector3(-3,.95f,0);Physics2D.SyncTransforms();shot=CGPowerProjectile.Spawn(game,hero,CGAbilities.Stats(CGStore.Data,1));shot.Advance(.5f);Check(enemy.hp==7,"Ráfaga dispara hacia donde mira el lobo");hero.facesRight=true;target.transform.position=new Vector3(3.5f,.95f,0);Physics2D.SyncTransforms();shot=CGPowerProjectile.Spawn(game,hero,CGAbilities.Stats(elite,1));shot.Advance(.5f);Check(enemy.hp==5,"Ráfaga mejorada aplica dos puntos de daño");
            shot=CGPowerProjectile.Spawn(game,hero,CGAbilities.Stats(CGStore.Data,1));page.SetValue(game,Enum.Parse(page.FieldType,"Paused"));var paused=shot.transform.position;shot.Advance(1);Check(shot.transform.position==paused,"Proyectil se detiene durante la pausa");page.SetValue(game,Enum.Parse(page.FieldType,"Playing"));shot.Advance(.05f);Check(shot.transform.position!=paused,"Proyectil continúa al reanudar");
            page.SetValue(game,Enum.Parse(page.FieldType,"Paused"));Check(!hero.ActivateAbility(0),"Pausa bloquea activación de poderes");
        }catch(Exception ex){fail++;File.AppendAllText(Path.Combine(output,"results.txt"),"FAIL "+ex+"\n");}
        finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);if(fixture.IsValid())SceneManager.UnloadSceneAsync(fixture);CGStore.Data=original;File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;}
    }
    static BoxCollider2D Box(GameObject root,string name,Vector2 point,Vector2 size){var obj=new GameObject(name);obj.transform.SetParent(root.transform,false);obj.transform.position=point;obj.AddComponent<CGSurface>();var c=obj.AddComponent<BoxCollider2D>();c.size=size;return c;}
}
}
#endif
