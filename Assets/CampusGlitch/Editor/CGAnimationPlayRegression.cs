#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
// Real input drives Update/FixedUpdate/LateUpdate; no animation state is assigned by the test.
[InitializeOnLoad]
public static class CGAnimationPlayRegression {
    const string Pending="CampusGlitch.AnimationPlay.Output";
    static Keyboard keyboard;static string output;static float started,peak;
    static bool damaged,celebrated,finished;static int passed,failed;
    static readonly HashSet<string> states=new HashSet<string>(),idleFrames=new HashSet<string>();
    static CGAnimationPlayRegression(){EditorApplication.playModeStateChanged+=OnMode;}
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Detener Play y esperar compilación.");
        if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("La escena contiene cambios sin guardar.");
        output=Path.GetFullPath(directory);
        if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva fuera del proyecto.");
        Directory.CreateDirectory(output);
        SessionState.SetString(Pending,output);SessionState.SetString(Pending+".Scene",SceneManager.GetActiveScene().path);
        SessionState.SetInt(Pending+".Background",(int)InputSystem.settings.backgroundBehavior);
        SessionState.SetBool(Pending+".RunBackground",Application.runInBackground);
        EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/SC_Level01.unity");EditorApplication.isPlaying=true;
    }
    static void OnMode(PlayModeStateChange mode){
        output=SessionState.GetString(Pending,"");if(string.IsNullOrEmpty(output))return;
        if(mode==PlayModeStateChange.EnteredPlayMode){
            CGStore.Data=new CGProgress();keyboard=InputSystem.AddDevice<Keyboard>();
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
            states.Clear();idleFrames.Clear();started=Time.unscaledTime;peak=0;damaged=celebrated=finished=false;passed=failed=0;
            InputSystem.onAfterUpdate+=Drive;EditorApplication.update+=Observe;
        }else if(mode==PlayModeStateChange.ExitingPlayMode){
            InputSystem.onAfterUpdate-=Drive;EditorApplication.update-=Observe;
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);keyboard=null;
            InputSystem.settings.backgroundBehavior=(InputSettings.BackgroundBehavior)SessionState.GetInt(Pending+".Background",0);
            Application.runInBackground=SessionState.GetBool(Pending+".RunBackground",false);
        }else if(mode==PlayModeStateChange.EnteredEditMode){
            string scene=SessionState.GetString(Pending+".Scene","");SessionState.EraseString(Pending);
            if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);
        }
    }
    static void Keys(params Key[] keys){keyboard.MakeCurrent();InputState.Change(keyboard,new KeyboardState(keys));}
    static void Drive(){
        if(finished||keyboard==null||InputState.currentUpdateType!=InputUpdateType.Dynamic)return;
        float t=Time.unscaledTime-started;
        if(t>=.9f&&t<1.6f)Keys(Key.D);
        else if(t>=2&&t<2.65f)Keys(Key.D,Key.LeftShift);
        else if(t>=3&&t<3.08f)Keys(Key.Space);
        else if(t>=4.5f&&t<4.58f)Keys(Key.J);
        else if(t>=7.5f&&t<7.9f)Keys(Key.A);
        else Keys();
    }
    static void Check(bool ok,string label){if(ok)passed++;else failed++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
    static void Observe(){
        if(finished||!EditorApplication.isPlaying)return;
        try{
            var game=UnityEngine.Object.FindFirstObjectByType<CGGame>();if(game==null||game.Hero==null)return;
            var hero=game.Hero;var visual=hero.GetComponent<CGGuardianVisual>();var view=hero.GetComponentInChildren<SpriteRenderer>();
            float t=Time.unscaledTime-started;states.Add(visual.CurrentAnimation);
            if(t<.8f)peak=Mathf.Max(peak,hero.transform.position.y);
            if(visual.CurrentAnimation=="Idle"&&view.sprite!=null)idleFrames.Add(view.sprite.name);
            if(t>=6&&!damaged){damaged=true;hero.Damage(1);}
            if(t>=8.5f&&!celebrated){
                Check(visual.CurrentAnimation=="Idle"&&hero.Grounded,"Volver a Idle después de caminar, correr, saltar, atacar, daño y giro");
                Check(!hero.facesRight&&view.flipX,"Giro izquierdo del controlador reflejado en el sprite");
                celebrated=true;visual.Celebrate(); // Explicit victory signal, not a level completion.
            }
            if(t<9.2f)return;
            Check(peak<.08f,"Spawn sin salto automático ni Space: pico="+peak.ToString("F4"));
            Check(idleFrames.Count>=4,"Idle avanza con entrada neutra: frames distintos="+idleFrames.Count);
            foreach(string state in new[]{"Idle","Caminar","Correr","Saltar","Caer / aterrizar","Atacar","Daño","Victoria"})Check(states.Contains(state),"Estado observado: "+state);
            Check(view.sprite.texture.filterMode==FilterMode.Point&&view.sprite.texture.mipmapCount==1&&view.sprite.texture.anisoLevel==0,"Atlas en Point, sin mipmaps ni filtrado anisotrópico");
            Check(hero.GetComponent<CapsuleCollider2D>().size==new Vector2(.76f,1.64f)&&Mathf.Abs(hero.Body.gravityScale-3.2f)<.001f,"Geometría física del protagonista preservada");
            Check(GameObject.Find("CG_EDITOR_LIVE_PREVIEW")==null,"Preview temporal excluida de Play");
            finished=true;Keys();File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+passed+" FAIL="+failed+"\n");EditorApplication.isPlaying=false;
        }catch(Exception ex){Check(false,ex.ToString());finished=true;EditorApplication.isPlaying=false;}
    }
}
}
#endif
