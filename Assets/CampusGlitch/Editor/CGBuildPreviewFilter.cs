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
    public static int Strip(Scene scene){int removed=0;foreach(var root in scene.GetRootGameObjects()){
        if(root.name=="CG_EDITOR_PREVIEW_2D"||root.name=="CG_EDITOR_LIVE_PREVIEW"||root.name=="CG_EDITOR_MENU_PREVIEW"){Object.DestroyImmediate(root);removed++;}
    }
        // Legacy scenes embed two CGCamera scripts without an asset GUID. The runtime
        // world configures a fresh follower and its target in CGWorld.Setup.
        foreach(var root in scene.GetRootGameObjects())foreach(var camera in root.GetComponentsInChildren<Camera>(true))
            foreach(var legacy in camera.GetComponents<CGCamera>())Object.DestroyImmediate(legacy);
        return removed;
    }
}
}
#endif
