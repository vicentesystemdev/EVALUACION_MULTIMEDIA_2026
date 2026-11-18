#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
namespace CampusGlitch.Editor {
[InitializeOnLoad]public static class CGSettingsWorldLifecycle {
    static CGSettingsWorldLifecycle(){EditorApplication.update+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode)CGSettingsWorldView.Tick();};EditorSceneManager.sceneClosing+=(scene,removing)=>{if(!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorSceneManager.IsPreviewScene(scene))CGSettingsWorldView.Cleanup();};AssemblyReloadEvents.beforeAssemblyReload+=CGSettingsWorldView.Cleanup;EditorApplication.quitting+=CGSettingsWorldView.Cleanup;EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.ExitingEditMode||state==PlayModeStateChange.ExitingPlayMode)CGSettingsWorldView.Cleanup();};}
}
}
#endif
