using UnityEngine;

namespace CampusGlitch {
/// <summary>Layered side-view campus: scenery recedes behind the physical walking lane.</summary>
public static class CGCampusDepthStage {
    static readonly Sprite[] rooms=new Sprite[8];
    static Sprite city;
    static Sprite rooftop;
    static readonly string[][] areas={
        new[]{"ACCESO","PLAZA","LABORATORIOS","PASARELA","SALIDA"},
        new[]{"RECEPCIÓN","SERVICIOS ESTUDIANTILES","SALA DE ESPERA","CONEXIÓN"},
        new[]{"MEZANINE","ATENCIÓN","OFICINAS","DIRECCIÓN"},
        new[]{"LABORATORIO","MUESTRAS","INVESTIGACIÓN","REGISTROS"},
        new[]{"CENTRO DE DESARROLLO","HARDWARE","REDES","SISTEMAS"},
        new[]{"AULAS DINÁMICAS","APRENDIZAJE","PROYECTOS","RETO DE SECUENCIAS"},
        new[]{"BIBLIOTECA","CONSULTA","ESTUDIO COLABORATIVO","INNOVACIÓN"},
        new[]{"TERRAZA","LOUNGE","MIRADOR","ENLACE EXTERIOR"},
        new[]{"CLÍNICA ODONTOLÓGICA","CONSULTORIOS","REGISTROS","NÚCLEO DIGITAL"},
        new[]{"ANATOMÍA","OSTEOTECA","COLECCIÓN DOCENTE","PASILLO SECRETO"}
    };
    static readonly int[] themes={0,0,1,2,3,4,5,0,6,7};
    public static string AreaAt(int level,float x){
        level=Mathf.Clamp(level,0,9);if(level==0)return CGLevelOneStage.ZoneAt(x);
        int area=Mathf.Clamp((int)(x/18.25f),0,3);return (area+1).ToString("00")+" · "+areas[level][area];
    }
    static Sprite Room(int theme){
        if(rooms[theme]!=null)return rooms[theme];
        var texture=Resources.Load<Texture2D>("CG/ActorsV2/CampusAmbientes_"+(theme<4?"A":"B"));if(texture==null)return null;
        texture.filterMode=FilterMode.Point;texture.anisoLevel=0;texture.wrapMode=TextureWrapMode.Clamp;
        int cell=theme%4,w=texture.width/2,h=texture.height/2;
        rooms[theme]=Sprite.Create(texture,new Rect((cell%2)*w+2,texture.height-(cell/2+1)*h+2,w-4,h-4),new Vector2(.5f,.5f),128,0,SpriteMeshType.FullRect);
        rooms[theme].name="Ambiente_Campus_"+theme;rooms[theme].hideFlags=HideFlags.HideAndDontSave;return rooms[theme];
    }
    public static bool Build(CGWorld world){
        if(world.index==0)return false;
        bool terrace=world.index==7;
        var room=Room(themes[world.index]);if(room==null)return false;
        var sky=CGCampusVisual.Background(0);if(sky==null)return false;
        if(city==null){city=Sprite.Create(sky,new Rect(0,sky.height*.65f,sky.width,sky.height*.35f),new Vector2(.5f,.5f),128);city.hideFlags=HideFlags.HideAndDontSave;}
        var far=terrace?city:room;
        for(int i=-1;i<4;i++){
            var obj=new GameObject("Profundidad_Lejana_"+i);obj.transform.SetParent(world.root,false);
            var view=obj.AddComponent<SpriteRenderer>();view.sprite=far;view.sortingOrder=-140;view.color=terrace?Color.white:new Color(.28f,.34f,.5f);
            float height=terrace?26:12,width=height*far.bounds.size.x/far.bounds.size.y;
            obj.transform.position=new Vector3(8+i*width,terrace?7.8f:6.5f,0);obj.transform.localScale=Vector3.one*(height/far.bounds.size.y);
            var parallax=obj.AddComponent<CGParallax>();parallax.cameraTransform=world.ViewCamera.transform;parallax.factor=.035f;
            parallax.startX=obj.transform.position.x-world.ViewCamera.transform.position.x*parallax.factor;
        }
        if(!terrace){
            for(int bay=-1;bay<5;bay++){
                var obj=new GameObject("Ambiente_"+world.spec.key+"_"+bay);obj.transform.SetParent(world.root,false);
                var view=obj.AddComponent<SpriteRenderer>();view.sprite=room;view.sortingOrder=-70;view.flipX=(bay&1)==1;
                float width=18.25f,scale=width/room.bounds.size.x,height=room.bounds.size.y*scale;
                obj.transform.localScale=Vector3.one*scale;obj.transform.position=new Vector3(9.125f+bay*width,height*.42f,0);
            }
        }else{
            var texture=Resources.Load<Texture2D>("CG/ActorsV2/CampusTerraza");if(texture==null)return false;
            if(rooftop==null){rooftop=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),128);rooftop.hideFlags=HideFlags.HideAndDontSave;}
            for(int bay=-1;bay<3;bay++){
                var obj=new GameObject("Terraza_Lounge_Profundidad_"+bay);obj.transform.SetParent(world.root,false);
                var view=obj.AddComponent<SpriteRenderer>();view.sprite=rooftop;view.sortingOrder=-70;view.flipX=(bay&1)==1;
                float scale=36.5f/rooftop.bounds.size.x,height=rooftop.bounds.size.y*scale;
                obj.transform.localScale=Vector3.one*scale;obj.transform.position=new Vector3(18.25f+bay*36.5f,height*.35f,0);
            }
        }
        // Recessed door frames hide joins and establish depth behind the unobstructed front lane.
        for(int bay=0;bay<5;bay++){
            float x=bay*18.25f;
            if(!terrace){
                CGLevelOneStage.Quad(world.root,"Marco_Arquitectura_Profunda",new Vector3(x,5.5f,0),new Vector2(.24f,10.7f),new Color(.1f,.15f,.22f),-62);
                CGLevelOneStage.Quad(world.root,"Luz_De_Orientacion",new Vector3(x+.15f,3.5f,0),new Vector2(.035f,1.1f),new Color(1,.6f,.15f),-61);
            }
            var label=new GameObject("Señal_Ambiente_"+bay);label.transform.SetParent(world.root,false);label.transform.position=new Vector3(x+3.5f,4.1f,0);
            var text=label.AddComponent<TextMesh>();text.text=areas[world.index][Mathf.Min(bay,3)];text.fontSize=40;text.characterSize=.065f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(1,.8f,.43f);
            label.GetComponent<MeshRenderer>().sortingOrder=-40;
        }
        return true;
    }
    public static void Decorate(CGWorld world){
        foreach(float x in new[]{20.5f,44.5f}){
            CGLevelOneStage.Quad(world.root,"Hueco_Real_Visible",new Vector3(x,-6,0),new Vector2(1,12),new Color(.015f,.02f,.045f),8);
            foreach(float side in new[]{-.55f,.55f})CGLevelOneStage.Quad(world.root,"Advertencia_Borde_Real",new Vector3(x+side,.025f,0),new Vector2(.08f,.05f),new Color(1,.64f,.12f),9);
            Prop(world,15,x-1.35f,.75f);
        }
    }
    static void Prop(CGWorld world,int frame,float x,float height){
        var source=CGCampusVisual.Object(frame);if(source==null)return;
        var obj=new GameObject("Decoracion_Detras_Del_Recorrido");obj.transform.SetParent(world.root,false);obj.transform.position=new Vector3(x,.15f+height*.5f,0);
        var view=obj.AddComponent<SpriteRenderer>();view.sprite=source;view.sortingOrder=-6;obj.transform.localScale=Vector3.one*(height/source.bounds.size.y);
    }
}
}
