#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
// Positive witnesses against actual generated colliders, not a distance-only reachability formula.
// Ground launch sites are checked separately by the full keyboard route regression.
[InitializeOnLoad]
public static class CGPlatformRouteRegression {
    const string Pending="CampusGlitch.PlatformRoutes.Output";
    static CGPlatformRouteRegression(){EditorApplication.playModeStateChanged+=state=>{
        if(state==PlayModeStateChange.EnteredPlayMode&&!string.IsNullOrEmpty(SessionState.GetString(Pending,"")))Execute();
    };}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Detener Play y esperar compilación.");
        if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar cambios de escena.");
        string output=Path.GetFullPath(directory);
        if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir una carpeta nueva externa.");
        Directory.CreateDirectory(output);SessionState.SetString(Pending,output);EditorApplication.isPlaying=true;
    }
    static void Execute(){
        string output=SessionState.GetString(Pending,"");SessionState.EraseString(Pending);
        var original=CGStore.Data;CGStore.Data=new CGProgress();int pass=0,fail=0;
        try{
            for(int level=0;level<10;level++)foreach(float speed in new[]{3.5f,5.8f}){
                var scene=SceneManager.CreateScene("PlatformRoute_"+level+"_"+Guid.NewGuid().ToString("N"),new CreateSceneParameters(LocalPhysicsMode.Physics2D));
                var root=new GameObject("RouteFixture");SceneManager.MoveGameObjectToScene(root,scene);
                try{
                    var game=root.AddComponent<CGGame>();game.enabled=false;
                    var world=root.AddComponent<CGWorld>();world.Setup(game,level,true);
                    foreach(var script in root.GetComponentsInChildren<MonoBehaviour>())script.enabled=false;
                    var hero=world.hero;var body=hero.Body;var capsule=hero.GetComponent<CapsuleCollider2D>();
                    foreach(var collider in root.GetComponentsInChildren<Collider2D>())if(collider!=capsule&&collider.GetComponent<CGSurface>()==null)collider.enabled=false;
                    var physics=scene.GetPhysicsScene2D();Physics2D.SyncTransforms();
                    var surfaces=root.GetComponentsInChildren<BoxCollider2D>().Where(c=>c.GetComponent<CGSurface>()!=null).ToArray();
                    var reachable=new HashSet<BoxCollider2D>(surfaces.Where(c=>c.name=="Ground"));
                    var pending=surfaces.Where(c=>c.name=="Plataforma_Elevada").OrderBy(c=>c.bounds.max.y).ToList();
                    bool changed=true;
                    while(changed&&pending.Count>0){
                        changed=false;
                        foreach(var target in pending.ToArray()){
                            string witness=null;
                            foreach(var source in reachable.ToArray()){
                                float rise=target.bounds.max.y-source.bounds.max.y;
                                if(rise<.1f||rise>3.22f||Mathf.Abs(target.bounds.center.x-source.bounds.center.x)>7)continue;
                                if(TryJump(physics,hero,capsule,source,target,speed,out witness))break;
                            }
                            if(witness==null)continue;
                            reachable.Add(target);pending.Remove(target);changed=true;
                            File.AppendAllText(Path.Combine(output,"witnesses.txt"),"N"+(level+1)+" speed="+speed+" "+witness+"\n");
                        }
                    }
                    bool ok=pending.Count==0;if(ok)pass++;else fail++;
                    File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+"N"+(level+1)+" speed="+speed+" plataformas="+surfaces.Count(c=>c.name=="Plataforma_Elevada")+" sin ruta="+string.Join("; ",pending.Select(c=>c.bounds.center.ToString()))+"\n");
                }finally{UnityEngine.Object.DestroyImmediate(root);SceneManager.UnloadSceneAsync(scene);}
            }
        }catch(Exception ex){fail++;File.AppendAllText(Path.Combine(output,"results.txt"),"FAIL "+ex+"\n");}
        finally{CGStore.Data=original;File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");EditorApplication.isPlaying=false;}
    }
    static bool TryJump(PhysicsScene2D physics,CGHero hero,CapsuleCollider2D capsule,BoxCollider2D source,BoxCollider2D target,float speed,out string witness){
        witness=null;var from=source.bounds;var to=target.bounds;
        float min=from.min.x+.39f,max=from.max.x-.39f;if(min>max)return false;
        float jump=(float)typeof(CGHero).GetField("JumpSpeed",BindingFlags.NonPublic|BindingFlags.Static).GetRawConstantValue();
        var launches=new[]{min,max,from.center.x,Mathf.Clamp(to.min.x-.55f,min,max),Mathf.Clamp(to.max.x+.55f,min,max),Mathf.Clamp(to.min.x-1.2f,min,max),Mathf.Clamp(to.max.x+1.2f,min,max)}.Distinct();
        foreach(float launch in launches)foreach(float delay in new[]{0f,.12f,.24f,.34f,.4f}){
            hero.Body.position=new Vector2(launch,from.max.y+.015f);hero.Body.linearVelocity=new Vector2(0,jump);hero.Body.angularVelocity=0;
            Physics2D.SyncTransforms();
            for(int step=0;step<70;step++){
                float dx=to.center.x-hero.Body.position.x;
                hero.Body.linearVelocity=new Vector2(step*.02f<delay?0:Mathf.Abs(dx)>.06f?Mathf.Sign(dx)*Mathf.Min(speed,Mathf.Abs(dx)/.02f):0,hero.Body.linearVelocity.y);
                physics.Simulate(.02f);
                var pos=hero.Body.position;
                if(step>8&&Mathf.Abs(pos.y-to.max.y)<.055f&&Mathf.Abs(hero.Body.linearVelocity.y)<.1f&&capsule.IsTouching(target)){
                    witness="origen="+from.center+" lanzamiento="+launch+" espera="+delay+" destino="+to.center+" aterrizaje="+pos;return true;
                }
                if(pos.y<from.max.y-.3f)break;
            }
        }
        return false;
    }
}
}
#endif
