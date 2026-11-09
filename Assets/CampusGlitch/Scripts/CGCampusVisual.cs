using UnityEngine;

namespace CampusGlitch {
public static class CGCampusVisual {
    [System.Serializable] class ObjectFrames { public CGGuardianArt.Frame[] frames; }
    static Sprite[] objects;
    static readonly Sprite[] backdrops = new Sprite[2];
    public static Sprite Object(int index) {
        if(objects==null) {
            var texture=Resources.Load<Texture2D>("CG/VisualV2/CampusObjects");
            var data=Resources.Load<TextAsset>("CG/VisualV2/CampusObjects_Frames");
            if(texture==null||data==null)return null;
            var atlas=JsonUtility.FromJson<ObjectFrames>(data.text);
            if(atlas==null||atlas.frames==null||atlas.frames.Length!=16)return null;
            objects=new Sprite[16];
            for(int i=0;i<16;i++) {
                var f=atlas.frames[i];
                objects[i]=Sprite.Create(texture,new Rect(f.x,f.y,f.w,f.h),new Vector2(.5f,.5f),128,0,SpriteMeshType.FullRect);
                objects[i].name="Campus_Object_"+i;
            }
        }
        return objects[Mathf.Clamp(index,0,15)];
    }
    public static Sprite Prop(string name) {
        if(name=="CheckpointPillar")return Object(8);
        if(name=="TerminalHacking")return Object(9);
        if(name=="PortalTransition")return Object(10);
        if(name.Contains("Bench"))return Object(0);
        if(name.Contains("Planter"))return Object(1);
        if(name.Contains("Pole")||name=="StudyLamp")return Object(2);
        if(name.Contains("Totem")||name=="SecurityBooth")return Object(3);
        if(name.Contains("Server")||name.Contains("Router")||name.Contains("Network")||name.Contains("Systems")||name=="HackTerminal")return Object(4);
        if(name.Contains("Bookshelf")||name.Contains("Bookcase"))return Object(5);
        if(name.Contains("Health")||name.Contains("Medical")||name.Contains("Experiment")||name.Contains("Anatomy")||name.Contains("Nutrition")||name.Contains("LabCurtain"))return Object(6);
        if(name.Contains("Sofa")||name.Contains("Chair")||name.Contains("Stool"))return Object(7);
        return null;
    }
    public static Texture2D Background(int level) {
        return Resources.Load<Texture2D>("CG/VisualV2/" + (level == 0 || level == 7 ? "CampusExterior" : "CampusInterior"));
    }
    public static bool Build(CGWorld world) {
        if(world.index==0&&CGLevelOneStage.Build(world))return true;
        int kind = world.index == 0 || world.index == 7 ? 0 : 1;
        var texture = Background(world.index);
        if (texture == null) return false;
        if (backdrops[kind] == null) backdrops[kind] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 128);
        float height = 11.4f, width = height * texture.width / texture.height;
        // The painted walkway meets y=0. Art stays behind every solid surface and actor.
        for (int i = -1; i < 5; i++) {
            var obj = new GameObject("Campus_Panorama_" + i);
            obj.transform.SetParent(world.root, false);
            obj.transform.position = new Vector3(8 + i * width, height * .35f, 0);
            obj.transform.localScale = Vector3.one * (height / backdrops[kind].bounds.size.y);
            var view = obj.AddComponent<SpriteRenderer>();
            view.sprite = backdrops[kind]; view.sortingOrder = -100;
            var parallax = obj.AddComponent<CGParallax>();
            parallax.cameraTransform = world.ViewCamera.transform;
            parallax.startX = obj.transform.position.x - world.ViewCamera.transform.position.x * .06f;
            parallax.factor = .06f;
        }
        return true;
    }
}
}
