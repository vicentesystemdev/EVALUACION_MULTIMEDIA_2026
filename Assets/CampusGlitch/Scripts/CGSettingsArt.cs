using System.Collections.Generic;
using UnityEngine;
namespace CampusGlitch {
// Nine-slice panels preserve border width and cut corners at every menu size.
public static class CGSettingsArt {
    static readonly Dictionary<(Color32,Color32),Texture2D> frames=new Dictionary<(Color32,Color32),Texture2D>();static readonly Texture2D[] icons=new Texture2D[9];static readonly Vector4 UV=new Vector4(0,.25f,.75f,1);static Texture2D knob,track;
    static Texture2D Make(string name,int size,Color32[] pixels){var t=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};t.SetPixels32(pixels);t.Apply(false,true);return t;}
    public static void Frame(Rect r,Color edge,Color fill){
        var key=((Color32)edge,(Color32)fill);
        if(!frames.TryGetValue(key,out var t)||t==null){var p=new Color32[64*64];for(int y=0;y<64;y++)for(int x=0;x<64;x++){
            float d=Mathf.Min(Mathf.Min(x,63-x),Mathf.Min(y,63-y));d=Mathf.Min(d,Mathf.Min(Mathf.Min(x+y-12,63-x+y-12),Mathf.Min(x+63-y-12,126-x-y-12))*.7071f);if(d<0)continue;
            var c=Color.Lerp(fill*.75f,fill,y/63f);c.a=fill.a;if(d<2)c=edge;else if(d<4)c=Color.Lerp(c,edge,.28f);p[y*64+x]=c;
        }t=Make("Marco_Configuracion",64,p);frames[key]=t;}
        const float corner=16;var xs=new Vector4(r.x,r.x+corner,r.xMax-corner,r.xMax);var ys=new Vector4(r.y,r.y+corner,r.yMax-corner,r.yMax);
        for(int y=0;y<3;y++)for(int x=0;x<3;x++)CGUIDraw.Uv(Rect.MinMaxRect(xs[x],ys[y],xs[x+1],ys[y+1]),t,new Rect(UV[x],1-UV[y+1],UV[x+1]-UV[x],UV[y+1]-UV[y]));
    }
    public static Texture2D Knob {get{if(knob!=null)return knob;var p=new Color32[32*32];for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=new Vector2(x-15.5f,y-15.5f).magnitude;if(d>15.5f)continue;p[y*32+x]=d>12?new Color(1,.57f,.15f):Color.white;}return knob=Make("Ajuste_Indicador",32,p);}}
    public static Texture2D Track {get{if(track!=null)return track;var p=new Color32[32*32];for(int y=0;y<32;y++)for(int x=0;x<32;x++){if(new Vector2(x-15.5f,y-15.5f).magnitude<15.5f)p[y*32+x]=Color.white;}return track=Make("Ajuste_Pista",32,p);}}
    static bool Line(Vector2 p,float ax,float ay,float bx,float by){var a=new Vector2(ax,ay);var d=new Vector2(bx,by)-a;return (p-a-d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)).magnitude<1.6f;}
    public static Texture2D Icon(int id){
        id=Mathf.Clamp(id,0,8);if(icons[id]!=null)return icons[id];var pixels=new Color32[64*64];for(int y=0;y<64;y++)for(int x=0;x<64;x++){
            var p=new Vector2(x-32,y-32);bool on=false;
            if(id==0)on=(Mathf.Abs(p.x)<23&&Mathf.Abs(p.y-4)<15&&(Mathf.Abs(p.x)>20||Mathf.Abs(p.y-4)>12))||Line(p,0,-11,0,-23)||Line(p,-13,-23,13,-23);
            if(id==1)on=(p.x>=-23&&p.x<=-12&&Mathf.Abs(p.y)<7)||Line(p,-12,7,1,19)||Line(p,1,19,1,-19)||Line(p,1,-19,-12,-7)||(p.x>5&&Mathf.Abs(p.magnitude-14)<1.5f)||(p.x>10&&Mathf.Abs(p.magnitude-24)<1.5f);
            if(id==2)on=Mathf.Abs(p.magnitude-27)<1.5f||(p-new Vector2(0,15)).sqrMagnitude<17||Line(p,0,7,0,-6)||Line(p,-14,5,14,5)||Line(p,0,-6,-9,-19)||Line(p,0,-6,9,-19);
            if(id==3){float e=p.x*p.x/625+p.y*p.y/225;on=(e<1&&e>.73f)||Line(p,-18,0,-6,0)||Line(p,-12,-6,-12,6)||(p-new Vector2(13,4)).sqrMagnitude<8||(p-new Vector2(18,-3)).sqrMagnitude<8;}
            if(id==4){on=Mathf.Abs(p.magnitude-9)<1.6f;for(int a=0;a<8;a++){float t=a*Mathf.PI/4;on|=Line(p,Mathf.Cos(t)*16,Mathf.Sin(t)*16,Mathf.Cos(t)*24,Mathf.Sin(t)*24);}}
            if(id==5)on=Line(p,2,24,-14,-2)||Line(p,-14,-2,2,-2)||Line(p,2,-2,-2,-24)||Line(p,-2,-24,15,4)||Line(p,15,4,-1,4);
            if(id==6){for(int i=0;i<5;i++)on|=Mathf.Abs(p.x-(-20+i*10))<2.5f&&Mathf.Abs(p.y)<8+(i%3)*7;}
            if(id==7)on=Line(p,-23,19,-3,19)||Line(p,-13,19,-13,-19)||Line(p,4,9,23,9)||Line(p,14,9,14,-19);
            if(id==8)on=(Mathf.Abs(p.x)<15&&Mathf.Abs(p.y)<26&&(Mathf.Abs(p.x)>12||Mathf.Abs(p.y)>23))||Line(p,-5,-19,5,-19)||Line(p,-5,19,5,19);
            if(on)pixels[y*64+x]=Color.white;
        }return icons[id]=Make("Configuracion_Icono_"+id,64,pixels);
    }
}
}
