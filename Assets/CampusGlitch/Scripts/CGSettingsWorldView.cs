using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace CampusGlitch {
// Uses the same world builder and sprites as gameplay, with no input or physics.
public static class CGSettingsWorldView {
    static Scene scene;static GameObject root;static CGWorld world;static Camera camera;static RenderTexture target;static CGGame owner;static CGCampusActor[] actors;static SpriteRenderer hero;static float nextFrame;
    static UniversalRenderPipeline.SingleCameraRequest request;
    static HideFlags Flags {get{return Application.isPlaying?HideFlags.None:HideFlags.HideAndDontSave;}}
    public static bool Visible {get;set;}
    public static int Level {get{return world!=null?world.index:-1;}}
    public static int RenderedFrames {get;private set;}
    public static CGWorld World {get{return world;}}
    public static Texture Get(CGGame game){
        int level=Mathf.Clamp(CGStore.Data.latestUnlocked,0,8);
        if(root==null||owner!=game||world.index!=level)Build(game,level);
        Visible=true;return target;
    }
    public static void Tick(){
        if(!Visible||root==null||camera==null||target==null)return;
        if(CGMenuTheme.Clock>=nextFrame){
            nextFrame=CGMenuTheme.Clock+.125f;
            bool purple=CGStore.Data.purpleEquipped,classic=CGStore.Data.useOriginalWolf;
            if(hero!=null){hero.sprite=classic?CGArt.Player(purple,(int)(CGMenuTheme.Clock*3)%2,true):CGGuardianArt.Get(purple,0,(int)(CGMenuTheme.Clock*8)%8);hero.transform.localPosition=classic?Vector3.up*.82f:Vector3.zero;hero.transform.localScale=Vector3.one*(classic?1.85f/hero.sprite.bounds.size.y:CGGuardianArt.Scale(purple));}
            foreach(var actor in actors)if(actor!=null)actor.SamplePreview(CGMenuTheme.Clock);
            if(Application.isPlaying)camera.enabled=true;
            else {var previous=RenderTexture.active;try{if(GraphicsSettings.currentRenderPipeline!=null)RenderPipeline.SubmitRenderRequest(camera,request);else camera.Render();RenderedFrames++;}finally{RenderTexture.active=previous;}}
        }
    }
    static void Rendered(ScriptableRenderContext context,Camera rendered){if(rendered==camera){camera.enabled=false;RenderedFrames++;}}
    static void BuiltInRendered(Camera rendered){if(GraphicsSettings.currentRenderPipeline==null&&rendered==camera){camera.enabled=false;RenderedFrames++;}}
    static void Build(CGGame game,int level){
        Cleanup();owner=game;
#if UNITY_EDITOR
        if(!Application.isPlaying)scene=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();else
#endif
        scene=SceneManager.CreateScene("Campus_Settings_View_"+System.Guid.NewGuid().ToString("N"),new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        root=new GameObject("Campus_Vista_Del_Juego"){hideFlags=Flags};SceneManager.MoveGameObjectToScene(root,scene);
        world=root.AddComponent<CGWorld>();world.Setup(game,level,true);
        foreach(var body in root.GetComponentsInChildren<Rigidbody2D>(true))body.simulated=false;
        foreach(var script in root.GetComponentsInChildren<MonoBehaviour>(true))script.enabled=false;
        actors=root.GetComponentsInChildren<CGCampusActor>();hero=world.hero.GetComponentInChildren<SpriteRenderer>();world.hero.Grounded=true;
        var sourceLight=Object.FindFirstObjectByType<Light2D>();var lightObject=new GameObject("Luz_de_la_vista");lightObject.transform.SetParent(root.transform,false);var light=lightObject.AddComponent<Light2D>();light.lightType=Light2D.LightType.Freeform;light.SetShapePath(new[]{new Vector3(-100,-100),new Vector3(200,-100),new Vector3(200,100),new Vector3(-100,100)});light.shapeLightFalloffSize=0;light.intensity=sourceLight!=null?sourceLight.intensity:1.15f;light.color=sourceLight!=null?sourceLight.color:Color.white;
        camera=world.ViewCamera;CGCamera2DRuntime.ConfigureCamera(camera);camera.tag="Untagged";camera.enabled=false;camera.cullingMask=1<<31;camera.orthographicSize=5.7f;camera.transform.position=new Vector3(8,2.2f,-10);
#if UNITY_EDITOR
        if(!Application.isPlaying)camera.scene=scene;
#endif
        // Keep the display camera and regular scene geometry completely independent.
        root.transform.position=new Vector3(1000,1000,0);
        foreach(var tr in root.GetComponentsInChildren<Transform>(true)){tr.gameObject.layer=31;tr.gameObject.hideFlags=Flags;foreach(var component in tr.GetComponents<Component>())if(component!=null)component.hideFlags=Flags;}
        target=new RenderTexture(640,360,24,RenderTextureFormat.ARGB32){name="Campus_Nivel_Actual",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};target.Create();camera.targetTexture=target;camera.aspect=16f/9;request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};nextFrame=0;RenderedFrames=0;
        RenderPipelineManager.endCameraRendering+=Rendered;Camera.onPostRender+=BuiltInRendered;
    }
    public static void Cleanup(){
        RenderPipelineManager.endCameraRendering-=Rendered;Camera.onPostRender-=BuiltInRendered;
        if(camera!=null){camera.enabled=false;camera.targetTexture=null;}if(root!=null){root.SetActive(false);if(Application.isPlaying)Object.Destroy(root);else Object.DestroyImmediate(root);}
        if(target!=null){target.Release();if(Application.isPlaying)Object.Destroy(target);else Object.DestroyImmediate(target);}target=null;world=null;camera=null;hero=null;actors=null;owner=null;request=null;
        if(scene.IsValid()&&scene.isLoaded){
#if UNITY_EDITOR
            if(UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene))UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);else
#endif
            SceneManager.UnloadSceneAsync(scene);
        }
        root=null;scene=default;Visible=false;
    }
}
}
