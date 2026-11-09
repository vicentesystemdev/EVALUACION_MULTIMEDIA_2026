using System;
using UnityEngine;

namespace CampusGlitch {
public static class CGCampusActors {
    [Serializable] class Atlas {public int columns,rows;public CGGuardianArt.Frame[] frames;}
    static readonly Sprite[][] sprites=new Sprite[2][];
    static readonly int[] columns={4,8};
    static Material material;
    public static Material Material {
        get {
            if(material==null){var shader=Resources.Load<Shader>("CG/ActorsV2/AtlasAlpha");if(shader!=null)material=new Material(shader){name="Campus_Actores_Alfa",hideFlags=HideFlags.HideAndDontSave};}
            return material;
        }
    }
    public static Sprite Frame(bool enemy,int row,int frame){
        int atlasIndex=enemy?1:0;
        if(sprites[atlasIndex]==null||sprites[atlasIndex][0]==null){
            string name=enemy?"CampusEnemies":"CampusNPC";
            var texture=Resources.Load<Texture2D>("CG/ActorsV2/"+name);
            var data=Resources.Load<TextAsset>("CG/ActorsV2/"+name+"_Frames");
            if(texture==null||data==null)return null;
            texture.filterMode=FilterMode.Point;texture.anisoLevel=0;texture.wrapMode=TextureWrapMode.Clamp;
            var atlas=JsonUtility.FromJson<Atlas>(data.text);
            if(atlas==null||atlas.frames==null||atlas.columns!=columns[atlasIndex])return null;
            var loaded=new Sprite[atlas.frames.Length];
            for(int i=0;i<loaded.Length;i++){
                var f=atlas.frames[i];
                if(f.w<=0||f.h<=0||f.x<0||f.y<0||f.x+f.w>texture.width||f.y+f.h>texture.height)return null;
                loaded[i]=Sprite.Create(texture,new Rect(f.x,f.y,f.w,f.h),new Vector2(f.pivotX/f.w,0),128,0,SpriteMeshType.FullRect);
                loaded[i].name=name+"_"+(i/atlas.columns)+"_"+(i%atlas.columns);
                loaded[i].hideFlags=HideFlags.HideAndDontSave;
            }
            sprites[atlasIndex]=loaded;
        }
        int index=row*columns[atlasIndex]+Mathf.Clamp(frame,0,columns[atlasIndex]-1);
        return index>=0&&index<sprites[atlasIndex].Length?sprites[atlasIndex][index]:null;
    }
    public static int NpcRow(string kind){return kind=="Guardia_U"?0:kind=="Estudiante"?1:kind=="Tecnica_Lab"?2:-1;}
    public static int EnemyRow(string kind){return kind=="Bug_Terrestre"?0:kind=="Bug_Saltador"?1:kind=="Centinela"?2:-1;}
    public static void Npc(GameObject obj,string kind,CGWorld world){
        int row=NpcRow(kind);if(row<0||Frame(false,row,0)==null)return;
        obj.AddComponent<CGCampusActor>().Configure(world,false,row,1.65f,.825f);
        if(world.index==0){
            var npc=obj.AddComponent<CGCampusNPC>();
            npc.Configure(world,kind);
        }
    }
    public static void Assistant(GameObject obj,CGWorld world){
        if(Frame(false,3,0)!=null)obj.AddComponent<CGCampusActor>().Configure(world,false,3,.72f,.36f);
    }
    public static void Enemy(GameObject obj,CGEnemy enemy,CGWorld world){
        int row=EnemyRow(enemy.kind);
        if(!enemy.boss&&row>=0&&Frame(true,row,0)!=null)obj.AddComponent<CGCampusActor>().Configure(world,true,row,.9f,.45f);
    }
}
}
