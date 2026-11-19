using UnityEngine;

namespace CampusGlitch {
// Native, cached artwork: crisp symbols and circular bezels rather than stretched menu bars.
public static class CGTouchArt {
    static readonly Texture2D[,] faces=new Texture2D[8,2];
    static readonly Texture2D[] symbols=new Texture2D[8];
    static readonly Color Ink=new Color(.025f,.045f,.09f,.94f);
    public static Color Accent(int button){
        if(button==CGTouch.Jump||button==CGTouch.Run)return new Color(1f,.61f,.18f);
        if(button==CGTouch.Attack)return new Color(1f,.36f,.22f);
        if(button==CGTouch.Ability)return new Color(.73f,.51f,1f);
        return new Color(.3f,.85f,1f);
    }
    static Texture2D Make(string name,int size,Color[] pixels){
        var t=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};
        t.SetPixels(pixels);t.Apply(false,true);return t;
    }
    public static Texture2D Face(int button,bool down){
        int state=down?1:0;if(faces[button,state]!=null)return faces[button,state];
        const int size=128;var pixels=new Color[size*size];Color accent=Accent(button);
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            float radius=new Vector2(x-63.5f,y-63.5f).magnitude;
            float alpha=Mathf.Clamp01(61.5f-radius);if(alpha<=0)continue;
            Color fill=down?Color.Lerp(accent,Ink,.25f):Color.Lerp(Ink,new Color(.1f,.16f,.25f,.96f),y/127f);
            if(radius>57)fill=Color.Lerp(accent,Color.white,down?.32f:.05f);
            else if(radius>53)fill=Color.Lerp(Ink,accent,.3f);
            else if(radius>49&&y>64)fill=Color.Lerp(fill,accent,.45f);
            fill.a*=alpha;pixels[y*size+x]=fill;
        }
        return faces[button,state]=Make("Tactil_Bisel_"+button+"_"+state,size,pixels);
    }
    static bool Line(Vector2 p,float ax,float ay,float bx,float by,float width=2.4f){
        var a=new Vector2(ax,ay);var d=new Vector2(bx-ax,by-ay);
        return (p-a-d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)).magnitude<width;
    }
    public static Texture2D Symbol(int button){
        if(symbols[button]!=null)return symbols[button];const int size=64;var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            var p=new Vector2(x,y);bool on=false;
            switch(button){
                case CGTouch.Left:on=Line(p,40,12,20,32)||Line(p,20,32,40,52)||Line(p,20,32,49,32);break;
                case CGTouch.Right:on=Line(p,24,12,44,32)||Line(p,44,32,24,52)||Line(p,15,32,44,32);break;
                case CGTouch.Jump:on=Line(p,14,34,32,52)||Line(p,32,52,50,34)||Line(p,32,52,32,15)||Line(p,16,10,48,10,1.6f);break;
                case CGTouch.Attack:on=Line(p,18,10,48,48,3)||Line(p,48,48,46,35)||Line(p,48,48,35,46)||Line(p,13,27,29,14)||Line(p,12,7,20,14);break;
                case CGTouch.Interact:on=Line(p,13,13,13,51)||Line(p,13,51,49,51)||Line(p,49,51,49,13)||Line(p,49,13,13,13)||Line(p,24,31,40,31)||Line(p,34,25,40,31)||Line(p,40,31,34,37);break;
                case CGTouch.Pause:on=(x>=20&&x<=26||x>=38&&x<=44)&&y>=14&&y<=50;break;
                case CGTouch.Ability:on=Line(p,36,55,18,29)||Line(p,18,29,33,29)||Line(p,33,29,28,9)||Line(p,28,9,47,37)||Line(p,47,37,32,37);break;
                case CGTouch.Run:on=Line(p,12,15,29,32)||Line(p,29,32,12,49)||Line(p,32,15,49,32)||Line(p,49,32,32,49);break;
            }
            if(on)pixels[y*size+x]=Color.white;
        }
        return symbols[button]=Make("Tactil_Icono_"+button,size,pixels);
    }
    public static void Draw(Rect rect,int button,bool down,string label,GUIStyle style){
        var original=GUI.color;Color accent=Accent(button);bool wide=rect.width>rect.height*1.5f;
        // Solid contrast and restrained translucency keep the control legible over every level.
        GUI.color=original;
        if(wide)CGSettingsArt.Frame(rect,down?Color.white:accent,down?Color.Lerp(Ink,accent,.3f):Ink);
        else GUI.DrawTexture(rect,Face(button,down));
        float iconSize=wide?rect.height*.57f:rect.width*.43f;
        Rect icon=wide?new Rect(rect.x+rect.height*.22f,rect.center.y-iconSize*.5f,iconSize,iconSize):new Rect(rect.center.x-iconSize*.5f,rect.y+rect.height*.16f,iconSize,iconSize);
        if(down)icon.y+=rect.height*.02f;
        GUI.color=original*(down?Color.white:accent);GUI.DrawTexture(icon,Symbol(button));GUI.color=original;
        Rect text=wide?new Rect(rect.x+rect.height,rect.y,rect.width-rect.height-8,rect.height):new Rect(rect.x+8,rect.y+rect.height*.61f,rect.width-16,rect.height*.24f);
        GUI.Label(text,label,style);GUI.color=original;
    }
}
}
