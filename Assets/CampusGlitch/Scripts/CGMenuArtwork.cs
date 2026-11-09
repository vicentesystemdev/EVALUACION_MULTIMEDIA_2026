using UnityEngine;

namespace CampusGlitch {
// Small cached UI textures keep beveled controls and symbols identical in Scene and Play.
public static class CGMenuArtwork {
    static Texture2D normal,focused;static readonly Texture2D[] icons=new Texture2D[7];
    static Color cachedAccent;
    static Texture2D shade,shadow;
    public static Texture2D Shadow {get{
        if(shadow!=null)return shadow;var pixels=new Color[128*32];
        for(int y=0;y<32;y++)for(int x=0;x<128;x++){float dx=(x-63.5f)/64,dy=(y-15.5f)/16;pixels[y*128+x]=new Color(.01f,.015f,.025f,Mathf.Pow(Mathf.Clamp01(1-dx*dx-dy*dy),2)*.6f);}
        return shadow=Make("Menu_Sombra_Guardian",128,32,pixels);
    }}
    public static Texture2D Shade {get{
        if(shade!=null)return shade;var pixels=new Color[64*64];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)pixels[y*64+x]=new Color(.012f,.02f,.045f,y/63f);
        return shade=Make("Menu_Velo_Gradual",64,64,pixels);
    }}
    public static Texture2D Button(bool selected,Color accent){
        if(normal==null||focused==null||cachedAccent!=accent){
            Release(normal);Release(focused);cachedAccent=accent;
            normal=CreateButton(false,accent);focused=CreateButton(true,accent);
        }
        return selected?focused:normal;
    }
    static void Release(Texture2D texture){if(texture!=null)Object.DestroyImmediate(texture);}
    static bool Inside(float x,float y,float w,float h,float cut){return x>=0&&y>=0&&x<=w&&y<=h&&x+y>=cut&&w-x+y>=cut&&x+h-y>=cut&&w-x+h-y>=cut;}
    static Texture2D CreateButton(bool selected,Color accent){
        const int w=512,h=80;var pixels=new Color[w*h];
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){
            if(!Inside(x,y,w-1,h-1,16))continue;
            bool inner=Inside(x-2,y-2,w-5,h-5,15);
            Color border=selected?Color.Lerp(accent,Color.white,.2f):new Color(.34f,.43f,.64f,.95f);
            Color fill=selected?Color.Lerp(new Color(.29f,.07f,.025f,.99f),new Color(.85f,.31f,.035f,.99f),y/(float)h):Color.Lerp(new Color(.025f,.035f,.08f,.98f),new Color(.055f,.075f,.15f,.98f),y/(float)h);
            pixels[y*w+x]=inner?fill:border;
        }
        return Make("Menu_Bisel_"+selected,w,h,pixels);
    }
    static Texture2D Make(string name,int w,int h,Color[] pixels){var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name,hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};texture.SetPixels(pixels);texture.Apply(false,true);return texture;}
    static float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
    static bool Line(Vector2 p,float ax,float ay,float bx,float by){return Segment(p,new Vector2(ax,ay),new Vector2(bx,by))<2;}
    static bool Ring(Vector2 p,float x,float y,float radius){return Mathf.Abs(Vector2.Distance(p,new Vector2(x,y))-radius)<2;}
    public static Texture2D Icon(int index){
        index=Mathf.Clamp(index,0,6);if(icons[index]!=null)return icons[index];var pixels=new Color[64*64];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){
            var p=new Vector2(x,y);bool on=false;
            switch(index){
                case 0:on=x>=19&&x<=48&&Mathf.Abs(y-32)<(48-x)*.7f;break;
                case 1:on=Line(p,10,15,24,20)||Line(p,24,20,40,14)||Line(p,40,14,54,19)||Line(p,10,47,24,52)||Line(p,24,52,40,46)||Line(p,40,46,54,51)||Line(p,10,15,10,47)||Line(p,24,20,24,52)||Line(p,40,14,40,46)||Line(p,54,19,54,51);break;
                case 2:on=Line(p,15,37,12,54)||Line(p,12,54,27,47)||Line(p,27,47,42,48)||Line(p,42,48,50,37)||Line(p,50,37,54,31)||Line(p,54,31,40,25)||Line(p,40,25,36,15)||Line(p,36,15,21,24)||Line(p,21,24,15,37)||Ring(p,36,36,2);break;
                case 3:on=Ring(p,32,32,16)||Ring(p,32,32,6);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));on|=Segment(p,new Vector2(32,32)+d*17,new Vector2(32,32)+d*25)<3;}break;
                case 4:on=Line(p,9,16,9,48)||Line(p,9,48,27,48)||Line(p,27,48,32,44)||Line(p,32,44,37,48)||Line(p,37,48,55,48)||Line(p,55,48,55,16)||Line(p,55,16,37,16)||Line(p,37,16,32,12)||Line(p,32,12,27,16)||Line(p,27,16,9,16)||Line(p,32,12,32,44);break;
                case 5:on=Ring(p,32,43,8)||Ring(p,12,39,6)||Ring(p,52,39,6)||Line(p,21,14,21,28)||Line(p,21,28,26,33)||Line(p,26,33,38,33)||Line(p,38,33,43,28)||Line(p,43,28,43,14)||Line(p,7,15,7,26)||Line(p,7,26,14,30)||Line(p,50,30,57,26)||Line(p,57,26,57,15);break;
                case 6:on=Line(p,31,12,13,12)||Line(p,13,12,13,52)||Line(p,13,52,31,52)||Line(p,30,32,55,32)||Line(p,55,32,44,43)||Line(p,55,32,44,21);break;
            }
            if(on)pixels[y*64+x]=Color.white;
        }
        return icons[index]=Make("Menu_Icono_"+index,64,64,pixels);
    }
}
}
