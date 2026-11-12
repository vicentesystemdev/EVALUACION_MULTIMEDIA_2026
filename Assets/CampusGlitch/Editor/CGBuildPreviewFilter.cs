#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CampusGlitch.Editor {
// Only the build's scene copy is changed. Saved scenes keep their editor presentation.
public sealed class CGBuildPreviewFilter:IProcessSceneWithReport {
    public int callbackOrder {get{return -1000;}}
    public void OnProcessScene(Scene scene,BuildReport report){if(report!=null&&!EditorApplication.isPlaying)Strip(scene);}
    public static int Strip(Scene scene){
        var roots=scene.GetRootGameObjects();bool playable=false;int removed=0,cameras=0;
        foreach(var root in roots){var controller=root.GetComponent<CGGame>();if(controller!=null&&controller.sceneLevel>=0)playable=true;}
        foreach(var root in roots){
            if(root.name=="CG_EDITOR_PREVIEW_2D"||root.name=="CG_EDITOR_LIVE_PREVIEW"||root.name=="CG_EDITOR_MENU_PREVIEW"){Object.DestroyImmediate(root);removed++;}
            // CGGame.Start creates the level camera, and CGWorld.Setup assigns its follower.
            // Replacing the legacy rig avoids its embedded, GUID-less MonoScript entries.
            else if(playable&&root.name=="Main Camera (2D ortográfica)"&&root.GetComponent<Camera>()!=null){Object.DestroyImmediate(root);cameras++;}
        }
        if(BuildPipeline.isBuildingPlayer){System.IO.Directory.CreateDirectory("Builds/Android");System.IO.File.AppendAllText("Builds/Android/Filtro_Escenas.txt",scene.name+": maquetas "+removed+"; rig de cámara sustituido "+cameras+"\n");}
        return removed;
    }
}
}
#endif
