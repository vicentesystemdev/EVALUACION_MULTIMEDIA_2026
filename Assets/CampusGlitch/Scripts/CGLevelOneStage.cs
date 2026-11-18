using UnityEngine;

namespace CampusGlitch {
/// <summary>Five connected campus zones; depth layers do not introduce false walking surfaces.</summary>
public static class CGLevelOneStage {
    static readonly Sprite[] panels=new Sprite[5];
    static Sprite white;
    static readonly string[] names={"ENTRADA PRINCIPAL","PLAZA UNIVERSITARIA","LABORATORIOS","PASARELA GLITCH","LOBBY / SALIDA"};
    public static string ZoneAt(float x){int zone=Mathf.Clamp((int)(x/14.6f),0,4);return (zone+1).ToString("00")+" · "+names[zone];}
    static Sprite Panel(int index){
        if(panels[index]!=null)return panels[index];
        var texture=index==0?CGCampusVisual.Background(0):Resources.Load<Texture2D>("CG/ActorsV2/LevelOneZones");
        if(texture==null)return null;
        Rect rect;
        if(index==0)rect=new Rect(0,0,texture.width,texture.height);
        else{
            int cell=index-1,width=texture.width/2,height=texture.height/2;
            rect=new Rect((cell%2)*width+1,texture.height-(cell/2+1)*height+1,width-2,height-2);
        }
        panels[index]=Sprite.Create(texture,rect,new Vector2(.5f,.5f),128,0,SpriteMeshType.FullRect);
        panels[index].name="N1_Zona_"+(index+1);panels[index].hideFlags=HideFlags.HideAndDontSave;return panels[index];
    }
    public static bool Build(CGWorld world){
        for(int i=0;i<5;i++)if(Panel(i)==null)return false;
        // Far city moves more slowly; near architecture stays aligned to the physical walkway.
        for(int i=-1;i<5;i++){
            var far=new GameObject("N1_Ciudad_Distante_"+i);far.transform.SetParent(world.root,false);
            var view=far.AddComponent<SpriteRenderer>();view.sprite=Panel(0);view.sortingOrder=-130;
            float height=12,width=height*view.sprite.bounds.size.x/view.sprite.bounds.size.y;
            far.transform.position=new Vector3(8+i*width,4.2f,0);
            far.transform.localScale=Vector3.one*(height/view.sprite.bounds.size.y);
            var parallax=far.AddComponent<CGParallax>();parallax.cameraTransform=world.ViewCamera.transform;parallax.factor=.035f;
            parallax.startX=far.transform.position.x-world.ViewCamera.transform.position.x*parallax.factor;
        }
        for(int zone=0;zone<5;zone++){
            var obj=new GameObject("N1_Zona_"+(zone+1)+"_"+names[zone]);obj.transform.SetParent(world.root,false);
            var view=obj.AddComponent<SpriteRenderer>();view.sprite=Panel(zone);view.sortingOrder=-70;
            float width=zone==0||zone==4?20.2f:14.6f;
            float scale=width/view.sprite.bounds.size.x,height=view.sprite.bounds.size.y*scale;
            obj.transform.localScale=Vector3.one*scale;
            float x=7.3f+zone*14.6f+(zone==4?2.8f:0);
            obj.transform.position=new Vector3(x,height*(zone==0?.35f:.455f),0);
        }
        return true;
    }
    public static void Decorate(CGWorld world){
        Place(world,3,4.5f,2.2f);Place(world,2,11.8f,3.2f);
        Place(world,0,17.8f,.85f);Place(world,1,25,1.3f);
        Place(world,4,30.2f,2.1f);Place(world,6,36.6f,1.4f);
        Place(world,1,58.2f,1.25f);Place(world,7,64.5f,1.1f);
        foreach(float gap in new[]{20.5f,44.5f}){
            Quad(world.root,"Hueco_Real_"+gap,new Vector3(gap,-6,0),new Vector2(1,12),new Color(.014f,.02f,.055f),8);
            Place(world,15,gap-1.3f,.8f);
            Quad(world.root,"Borde_Advertencia_I",new Vector3(gap-.54f,.035f,0),new Vector2(.08f,.07f),new Color(1,.63f,.12f),9);
            Quad(world.root,"Borde_Advertencia_D",new Vector3(gap+.54f,.035f,0),new Vector2(.08f,.07f),new Color(1,.63f,.12f),9);
        }
    }
    static void Place(CGWorld world,int sprite,float x,float height){
        var source=CGCampusVisual.Object(sprite);if(source==null)return;
        var obj=new GameObject("N1_Decoracion_Profundidad_"+sprite);obj.transform.SetParent(world.root,false);
        obj.transform.position=new Vector3(x,height*.5f+.12f,0);
        var view=obj.AddComponent<SpriteRenderer>();view.sprite=source;view.sortingOrder=-5;
        obj.transform.localScale=Vector3.one*(height/source.bounds.size.y);
    }
    public static void PlatformEdge(Transform parent,float width,float height,float x){
        var color=x>=43&&x<=58?new Color(.86f,.35f,1):new Color(1,.69f,.2f);
        Quad(parent,"Superficie_Solida_Marcada",new Vector3(0,height*.5f-.018f,0),new Vector2(width,.036f),color,7,true);
    }
    internal static void Quad(Transform parent,string name,Vector3 position,Vector2 size,Color color,int order,bool local=false){
        if(white==null){var t=new Texture2D(1,1){name="Campus_Color",hideFlags=HideFlags.HideAndDontSave};t.SetPixel(0,0,Color.white);t.Apply();white=Sprite.Create(t,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);white.hideFlags=HideFlags.HideAndDontSave;}
        var obj=new GameObject(name);obj.transform.SetParent(parent,false);
        if(local)obj.transform.localPosition=position;else obj.transform.position=position;
        obj.transform.localScale=new Vector3(size.x,size.y,1);
        var view=obj.AddComponent<SpriteRenderer>();view.sprite=white;view.color=color;view.sortingOrder=order;
    }
}
}
