using System.Collections.Generic;
using UnityEngine;

namespace CampusGlitch {
// Geometry and button caps remain separate so every input can animate and be selected.
public static class CGControlsArt {
    [System.Serializable]public class KeyTile{public string binding,label;public float x,y,w,h;}
    [System.Serializable]class KeyboardMap{public KeyTile[] keys;}
    static KeyTile[] keyboardKeys;static Texture2D keyboardBody,keyboardStates;
    public static KeyTile[] KeyboardKeys {get{if(keyboardKeys==null||keyboardKeys.Length==0){var json=Resources.Load<TextAsset>("CG/Controls/KeyboardLayout");keyboardKeys=json!=null?JsonUtility.FromJson<KeyboardMap>(json.text).keys:new KeyTile[0];}return keyboardKeys;}}
    public static Texture2D KeyboardBody {get{if(keyboardBody==null)keyboardBody=Resources.Load<Texture2D>("CG/Controls/Keyboard_Campus");return keyboardBody;}}
    public static Texture2D KeyboardStates {get{if(keyboardStates==null)keyboardStates=Resources.Load<Texture2D>("CG/Controls/Keyboard_KeyStates");return keyboardStates;}}
    static Texture2D circle,glow;
    static readonly Texture2D[] exportedBodies=new Texture2D[2];static Texture2D buttonStates;
    static readonly Texture2D[] shapes=new Texture2D[4];
    public static Texture2D Controller(bool playStation){int index=playStation?1:0;if(exportedBodies[index]==null)exportedBodies[index]=Resources.Load<Texture2D>("CG/Controls/Controller_"+(playStation?"PlayStation":"Xbox"));return exportedBodies[index];}
    public static Texture2D ButtonStates {get{if(buttonStates==null)buttonStates=Resources.Load<Texture2D>("CG/Controls/Controller_ButtonStates");return buttonStates;}}
    public static Texture2D FaceShape(int index){
        if(shapes[index]!=null)return shapes[index];const int s=64;var pixels=new Color[s*s];
        for(int y=0;y<s;y++)for(int x=0;x<s;x++){
            var p=new Vector2(x,y);bool on=false;
            if(index==0)on=Line(p,new Vector2(17,17),new Vector2(47,47))<2||Line(p,new Vector2(17,47),new Vector2(47,17))<2;
            if(index==1)on=Mathf.Abs(Vector2.Distance(p,new Vector2(32,32))-20)<2;
            if(index==2)on=Line(p,new Vector2(12,32-17),new Vector2(52,32-17))<2||Line(p,new Vector2(52,15),new Vector2(32,51))<2||Line(p,new Vector2(32,51),new Vector2(12,15))<2;
            if(index==3)on=(Mathf.Abs(x-12)<2||Mathf.Abs(x-52)<2)&&y>=12&&y<=52||(Mathf.Abs(y-12)<2||Mathf.Abs(y-52)<2)&&x>=12&&x<=52;
            if(on)pixels[y*s+x]=Color.white;
        }
        return shapes[index]=Make("Mando_Simbolo_"+index,s,s,pixels);
    }
    static float Line(Vector2 point,Vector2 start,Vector2 end){var d=end-start;return Vector2.Distance(point,start+d*Mathf.Clamp01(Vector2.Dot(point-start,d)/d.sqrMagnitude));}
    static readonly Dictionary<(Color32,Color32),Texture2D> bodies=new Dictionary<(Color32,Color32),Texture2D>();
    public static Texture2D Circle {get{if(circle==null)circle=Radial(false);return circle;}}
    public static Texture2D Glow {get{if(glow==null)glow=Radial(true);return glow;}}
    static Texture2D Radial(bool soft){const int s=96;var pixels=new Color[s*s];for(int y=0;y<s;y++)for(int x=0;x<s;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(47.5f,47.5f))/47.5f;float alpha=soft?Mathf.Pow(Mathf.Clamp01(1-r),2):Mathf.Clamp01((1-r)*48);pixels[y*s+x]=new Color(1,1,1,alpha);}return Make(soft?"Control_Brillo":"Control_Boton",s,s,pixels);}
    public static Texture2D Body(Color fill,Color edge){
        var key=((Color32)fill,(Color32)edge);if(bodies.TryGetValue(key,out var found)&&found!=null)return found;
        const int w=512,h=300;var points=new[]{new Vector2(95,51),new Vector2(146,32),new Vector2(366,32),new Vector2(417,51),new Vector2(465,218),new Vector2(451,273),new Vector2(414,282),new Vector2(365,235),new Vector2(147,235),new Vector2(98,282),new Vector2(61,273),new Vector2(47,218)};
        var pixels=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++){
            var p=new Vector2(x,y);bool inside=false;float distance=1000;
            for(int i=0,j=points.Length-1;i<points.Length;j=i++){
                var a=points[i];var b=points[j];if((a.y>y)!=(b.y>y)&&x<(b.x-a.x)*(y-a.y)/(b.y-a.y)+a.x)inside=!inside;
                var d=b-a;distance=Mathf.Min(distance,Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)));
            }
            if(!inside)continue;Color color=distance<2.2f?edge:Color.Lerp(fill,fill*1.8f,1-y/(float)h);color.a=1;pixels[(h-1-y)*w+x]=color;
        }
        var texture=Make("Mando_Campus",w,h,pixels);bodies[key]=texture;return texture;
    }
    static Texture2D Make(string name,int w,int h,Color[] pixels){var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};texture.SetPixels(pixels);texture.Apply(false,true);return texture;}
}
}
