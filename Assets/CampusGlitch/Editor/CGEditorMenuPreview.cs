#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CampusGlitch.Editor {
/// <summary>A real world-space Canvas in Scene, populated by the runtime menu's drawing stream.</summary>
[InitializeOnLoad]
public static class CGEditorMenuPreview {
    const HideFlags Transient=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
    static GameObject root,legacy;static CGGame controller;static RectTransform canvas;
    static readonly List<Graphic> elements=new List<Graphic>();
    static readonly Dictionary<Renderer,bool> renderStates=new Dictionary<Renderer,bool>();
    static bool legacyHidden;static double nextFrame;
    static CGEditorMenuPreview(){
        EditorSceneManager.sceneOpened+=(scene,mode)=>Schedule();
        EditorSceneManager.sceneClosing+=(scene,removing)=>Cleanup();
        EditorSceneManager.activeSceneChangedInEditMode+=(a,b)=>Schedule();
        EditorApplication.projectChanged+=Schedule;
        EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.ExitingEditMode)Cleanup();if(state==PlayModeStateChange.EnteredEditMode)Schedule();};
        AssemblyReloadEvents.beforeAssemblyReload+=Cleanup;EditorApplication.quitting+=Cleanup;
        EditorApplication.update+=Animate;Schedule();
    }
    static void Schedule(){EditorApplication.delayCall-=DeferredRefresh;EditorApplication.delayCall+=DeferredRefresh;}
    static void DeferredRefresh(){Refresh();}
    public static void Refresh(){
        EditorApplication.delayCall-=DeferredRefresh;
        if(Application.isBatchMode||EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer)return;
        Cleanup();var scene=SceneManager.GetActiveScene();if(!scene.IsValid()||!scene.isLoaded)return;
        foreach(var obj in scene.GetRootGameObjects()){var game=obj.GetComponent<CGGame>();if(game!=null){controller=game;break;}}
        if(controller==null||controller.sceneLevel>=0){controller=null;return;}
        foreach(var obj in scene.GetRootGameObjects())if(obj.name=="CG_EDITOR_PREVIEW_2D"){
            legacy=obj;legacyHidden=SceneVisibilityManager.instance.IsHidden(legacy);SceneVisibilityManager.instance.Hide(legacy,true);
            foreach(var renderer in legacy.GetComponentsInChildren<Renderer>(true)){renderStates[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}break;
        }
        root=new GameObject("CG_EDITOR_MENU_PREVIEW"){hideFlags=Transient};SceneManager.MoveGameObjectToScene(root,scene);
        var canvasObject=new GameObject("Menu_Actual_1280x720",typeof(RectTransform),typeof(Canvas),typeof(RectMask2D));canvasObject.transform.SetParent(root.transform,false);
        canvas=canvasObject.GetComponent<RectTransform>();canvas.sizeDelta=new Vector2(1280,720);canvas.localScale=Vector3.one*.01f;
        var display=canvasObject.GetComponent<Canvas>();display.renderMode=RenderMode.WorldSpace;display.sortingOrder=2000;
        Mark(root);RecordFrame();Frame();nextFrame=0;
    }
    static void Mark(GameObject obj){foreach(var transform in obj.GetComponentsInChildren<Transform>(true)){transform.gameObject.hideFlags=Transient;foreach(var component in transform.GetComponents<Component>())if(component!=null)component.hideFlags=Transient;}}
    static void Animate(){
        if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
        if(EditorApplication.timeSinceStartup<nextFrame)return;nextFrame=EditorApplication.timeSinceStartup+.125;
        if(root==null){var game=Object.FindFirstObjectByType<CGGame>();if(game!=null&&game.sceneLevel<0)Refresh();return;}
        foreach(SceneView view in SceneView.sceneViews){if(view==null||view.camera==null)continue;view.camera.allowMSAA=false;view.sceneViewState.showImageEffects=false;if(view.camera.targetTexture!=null)view.camera.targetTexture.filterMode=FilterMode.Point;}
        RecordFrame();
        SceneView.RepaintAll();
    }
    static void RecordFrame(){
        if(root==null||controller==null||EditorApplication.isPlayingOrWillChangePlaymode)return;
        var items=new List<CGUIDraw.Item>();var previous=CGUIDraw.Capture;
        try{CGUIDraw.Capture=items;CGUIDraw.ResetCaptureState();controller.DrawEditorMenu();}
        finally{CGUIDraw.Capture=previous;}
        if(root!=null&&canvas!=null)Apply(items);
    }
    static void Apply(List<CGUIDraw.Item> items){
        for(int i=0;i<items.Count;i++){
            var item=items[i];bool text=item.text!=null;
            if(i<elements.Count&&elements[i]!=null&&((elements[i] is Text)!=text)){if(Selection.activeGameObject==elements[i].gameObject)Selection.activeObject=controller;Object.DestroyImmediate(elements[i].gameObject);elements[i]=null;}
            if(i>=elements.Count)elements.Add(null);
            if(elements[i]==null){var obj=new GameObject(text?"Texto_"+i:"Imagen_"+i,typeof(RectTransform));obj.transform.SetParent(canvas,false);elements[i]=text?(Graphic)obj.AddComponent<Text>():obj.AddComponent<RawImage>();Mark(obj);}
            var graphic=elements[i];graphic.gameObject.SetActive(true);graphic.raycastTarget=false;graphic.color=item.color;
            var rect=graphic.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(item.rect.center.x-640,360-item.rect.center.y);rect.sizeDelta=item.rect.size;
            if(text){var label=(Text)graphic;label.text=item.text;label.font=item.font!=null?item.font:CGMenuTheme.Body;label.fontSize=item.fontSize;label.fontStyle=item.fontStyle;label.alignment=item.alignment;label.horizontalOverflow=item.wrap?HorizontalWrapMode.Wrap:HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Overflow;}
            else{var image=(RawImage)graphic;image.texture=item.texture;image.uvRect=item.uv;}
        }
        for(int i=items.Count;i<elements.Count;i++)if(elements[i]!=null)elements[i].gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases();
    }
    [MenuItem("Campus Glitch/Vista del menú/Mostrar menú actual en Scene")]
    public static void Frame(){if(root==null)return;var view=EditorWindow.GetWindow<SceneView>();view.in2DMode=true;view.orthographic=true;view.showGrid=false;view.drawGizmos=false;view.LookAtDirect(Vector3.zero,Quaternion.identity,3.6f);view.Focus();view.Repaint();}
    public static void Cleanup(){
        if(root!=null){if(Selection.activeTransform!=null&&Selection.activeTransform.IsChildOf(root.transform))Selection.activeObject=controller;Object.DestroyImmediate(root);}root=null;canvas=null;controller=null;elements.Clear();
        foreach(var pair in renderStates)if(pair.Key!=null)pair.Key.forceRenderingOff=pair.Value;renderStates.Clear();
        if(legacy!=null&&!legacyHidden)SceneVisibilityManager.instance.Show(legacy,true);legacy=null;
    }
}
}
#endif
