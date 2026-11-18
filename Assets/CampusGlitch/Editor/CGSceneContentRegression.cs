#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
public static class CGSceneContentRegression {
    public static void Run(string directory){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Detener Play y esperar compilación.");
        if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guardar cambios de escena.");
        var output=Path.GetFullPath(directory);
        if(Directory.Exists(output)||output.StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath,".."))+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Elegir carpeta nueva externa.");
        Directory.CreateDirectory(output);int pass=0,fail=0;
        Action<bool,string> check=(ok,label)=>{if(ok)pass++;else fail++;File.AppendAllText(Path.Combine(output,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");};
        string original=SceneManager.GetActiveScene().path;var data=CGStore.Data;CGStore.Data=new CGProgress();
        try{
            for(int level=0;level<10;level++){
                var scene=EditorSceneManager.OpenScene("Assets/CampusGlitch/Scenes/"+CGLevels.Scene(level)+".unity");
                var camera=Camera.main;string cameraState=EditorJsonUtility.ToJson(camera);Vector3 position=camera.transform.position;
                CGEditorWorldPreview.Refresh();
                var preview=GameObject.Find("CG_EDITOR_LIVE_PREVIEW");var world=preview.GetComponent<CGWorld>();
                check(world.index==level&&world.hero.GetComponentInChildren<SpriteRenderer>().sprite!=null,"N"+(level+1)+" protagonista presente tras cambiar de escena");
                check(Enumerable.Range(0,64).All(i=>CGGuardianArt.Get(false,i/8,i%8)!=null&&CGGuardianArt.Get(true,i/8,i%8)!=null),"N"+(level+1)+" cachés roja/morada válidas");
                var actors=world.GetComponentsInChildren<CGCampusActor>();
                check(actors.Length>=5&&actors.All(a=>a.GetComponentsInChildren<SpriteRenderer>().Any(v=>v.enabled&&v.sprite!=null&&v.sprite.texture.filterMode==FilterMode.Point)),"N"+(level+1)+" actores nuevos visibles y en Point");
                check(world.GetComponentsInChildren<CGCampusNPC>().All(n=>!string.IsNullOrEmpty(n.role)&&!string.IsNullOrEmpty(n.dialogue)),"N"+(level+1)+" roles y diálogos conectados");
                check(preview.GetComponentsInChildren<Transform>().All(t=>(t.gameObject.hideFlags&(HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild))==(HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild)),"N"+(level+1)+" preview excluido de serialización y build");
                check(!scene.isDirty&&cameraState==EditorJsonUtility.ToJson(camera)&&position==camera.transform.position,"N"+(level+1)+" escena y cámara original intactas");
            }
        }catch(Exception ex){check(false,ex.ToString());}
        finally{CGStore.Data=data;EditorSceneManager.OpenScene(original);CGEditorWorldPreview.Refresh();File.AppendAllText(Path.Combine(output,"results.txt"),"TOTAL PASS="+pass+" FAIL="+fail+"\n");}
    }
}
}
#endif
