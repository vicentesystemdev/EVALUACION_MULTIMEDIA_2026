#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CampusGlitch.Editor {
/// <summary>Scene uses the runtime world builder; generated objects are never saved into scene YAML.</summary>
[InitializeOnLoad]
public static class CGEditorWorldPreview {
    const string PreviewName="CG_EDITOR_LIVE_PREVIEW";
    const HideFlags Transient=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
    static GameObject preview,legacy;
    static bool legacyHidden;
    static readonly Dictionary<Renderer,bool> renderStates=new Dictionary<Renderer,bool>();
    static bool scheduled,building;
    static bool previewClassic,previewPurple;
    static double nextFrame;

    static CGEditorWorldPreview(){
        EditorSceneManager.sceneOpened+=(scene,mode)=>Schedule();
        EditorSceneManager.sceneClosing+=(scene,removing)=>Cleanup();
        EditorSceneManager.activeSceneChangedInEditMode+=(oldScene,newScene)=>Schedule();
        EditorApplication.projectChanged+=Schedule;
        EditorApplication.update+=AnimatePreview;
        EditorApplication.playModeStateChanged+=state=>{
            if(state==PlayModeStateChange.ExitingEditMode)Cleanup();
            if(state==PlayModeStateChange.EnteredEditMode)Schedule();
        };
        AssemblyReloadEvents.beforeAssemblyReload+=Cleanup;
        EditorApplication.quitting+=Cleanup;
        Schedule();
    }
    static void Schedule(){if(scheduled)return;scheduled=true;EditorApplication.delayCall+=()=>{scheduled=false;Refresh();};}

    [MenuItem("Campus Glitch/Vista del nivel/Actualizar mundo en Scene")]
    public static void Refresh(){
        if(building||EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer)return;
        var scene=SceneManager.GetActiveScene();
        if(!scene.IsValid()||!scene.isLoaded||EditorSceneManager.IsPreviewScene(scene))return;
        CGGame controller=null;
        foreach(var root in scene.GetRootGameObjects()){
            var candidate=root.GetComponent<CGGame>();
            if(candidate!=null){controller=candidate;break;}
        }
        Cleanup();
        if(controller==null||controller.sceneLevel<0||controller.sceneLevel>=CGLevels.All.Length)return;
        building=true;
        var originalData=CGStore.Data;
        try{
            CGStore.Load();
            foreach(var root in scene.GetRootGameObjects())if(root.name=="CG_EDITOR_PREVIEW_2D"){
                legacy=root;
                legacyHidden=SceneVisibilityManager.instance.IsHidden(legacy);
                SceneVisibilityManager.instance.Hide(legacy,true);
                foreach(var renderer in legacy.GetComponentsInChildren<Renderer>(true)){
                    renderStates[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;
                }
                break;
            }
            preview=new GameObject(PreviewName){hideFlags=Transient};
            SceneManager.MoveGameObjectToScene(preview,scene);
            var world=preview.AddComponent<CGWorld>();
            world.Setup(controller,controller.sceneLevel,true);
            // Display the approved idle artwork, without running a player controller in edit mode.
            var view=world.hero.GetComponentInChildren<SpriteRenderer>();
            bool classic=CGStore.Data.useOriginalWolf,purple=CGStore.Data.purpleEquipped;
            previewClassic=classic;previewPurple=purple;
            if(!classic&&CGGuardianArt.Available){
                view.sprite=CGGuardianArt.Get(purple,0,0);
                view.transform.localPosition=Vector3.zero;
                view.transform.localScale=Vector3.one*CGGuardianArt.Scale(purple);
            }else if(view.sprite!=null){
                view.transform.localPosition=new Vector3(0,.82f,0);
                view.transform.localScale=Vector3.one*(1.85f/view.sprite.bounds.size.y);
            }
            foreach(var body in preview.GetComponentsInChildren<Rigidbody2D>(true))body.simulated=false;
            foreach(var behaviour in preview.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            foreach(var camera in preview.GetComponentsInChildren<Camera>(true))camera.enabled=false;
            foreach(var tr in preview.GetComponentsInChildren<Transform>(true)){
                tr.gameObject.hideFlags=Transient;
                foreach(var component in tr.GetComponents<Component>())if(component!=null)component.hideFlags=Transient;
            }
            SceneView.RepaintAll();
        }catch(System.Exception error){Cleanup();Debug.LogException(error);}
        finally{CGStore.Data=originalData;building=false;}
    }
    public static void Cleanup(){
        if(preview!=null)Object.DestroyImmediate(preview);
        preview=null;
        foreach(var item in renderStates)if(item.Key!=null)item.Key.forceRenderingOff=item.Value;
        renderStates.Clear();
        if(legacy!=null&&!legacyHidden)SceneVisibilityManager.instance.Show(legacy,true);
        legacy=null;
    }
    static void AnimatePreview(){
        if(preview==null||EditorApplication.isPlayingOrWillChangePlaymode)return;
        foreach(SceneView sceneView in SceneView.sceneViews){
            if(sceneView==null||sceneView.camera==null)continue;
            sceneView.camera.allowMSAA=false;
            sceneView.sceneViewState.showImageEffects=false;
            if(sceneView.camera.targetTexture!=null)sceneView.camera.targetTexture.filterMode=FilterMode.Point;
        }
        double now=EditorApplication.timeSinceStartup;
        if(now<nextFrame)return;nextFrame=now+.125;
        var world=preview.GetComponent<CGWorld>();
        if(world!=null&&world.hero!=null&&!previewClassic&&CGGuardianArt.Available){
            var view=world.hero.GetComponentInChildren<SpriteRenderer>();
            if(view!=null)view.sprite=CGGuardianArt.Get(previewPurple,0,(int)(now*8)%8);
        }
        foreach(var actor in preview.GetComponentsInChildren<CGCampusActor>())actor.SamplePreview((float)now);
        SceneView.RepaintAll();
    }
    [MenuItem("Campus Glitch/Vista del nivel/Mostrar nivel completo")]
    public static void FrameLevel(){
        var view=SceneView.lastActiveSceneView;if(view==null)return;
        view.in2DMode=true;view.orthographic=true;view.LookAtDirect(new Vector3(36.5f,3.5f,0),Quaternion.identity,24f);view.Focus();
        view.Repaint();
    }
    [MenuItem("Campus Glitch/Vista del nivel/Enfocar entrada")]
    public static void FrameEntrance(){
        var view=SceneView.lastActiveSceneView;if(view==null)return;
        view.in2DMode=true;view.orthographic=true;view.LookAtDirect(new Vector3(8,2.8f,0),Quaternion.identity,8.8f);view.Focus();
        view.Repaint();
    }
}
}
#endif
