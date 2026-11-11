using System.Collections.Generic;
using UnityEngine;

namespace CampusGlitch {
// The menu has one drawing stream. Editor canvases consume the same layout as the running game.
public static class CGUIDraw {
    public sealed class Item {
        public Rect rect,uv;public Texture texture;public string text;public Font font;
        public int fontSize;public FontStyle fontStyle;public TextAnchor alignment;public bool wrap;public Color color;
    }
    public static List<Item> Capture;
    static Color captureColor=UnityEngine.Color.white;
    static Matrix4x4 captureMatrix=Matrix4x4.identity;
    public static Color Color {get{return Capture!=null?captureColor:GUI.color;}set{if(Capture!=null)captureColor=value;else GUI.color=value;}}
    public static Matrix4x4 Matrix {get{return Capture!=null?captureMatrix:GUI.matrix;}set{if(Capture!=null)captureMatrix=value;else GUI.matrix=value;}}
    public static void ResetCaptureState(){captureColor=UnityEngine.Color.white;captureMatrix=Matrix4x4.identity;}
    public static void Text(Rect rect,string text,GUIStyle style){
        if(string.IsNullOrEmpty(text))return;
        if(CGSettingsRuntime.Options.solidText){var tint=Color;var backing=new Rect(rect.x+12,rect.y+3,Mathf.Max(0,rect.width-24),Mathf.Max(0,rect.height-6));Color=new Color(.008f,.012f,.035f,tint.a*.96f);Uv(backing,Texture2D.whiteTexture,new Rect(0,0,1,1));Color=tint;}
        if(Capture==null){GUI.Label(rect,text,style);return;}
        Capture.Add(new Item{rect=rect,text=text,font=style.font,fontSize=style.fontSize,fontStyle=style.fontStyle,alignment=style.alignment,wrap=style.wordWrap,color=style.normal.textColor*Color});
    }
    public static void Uv(Rect rect,Texture texture,Rect uv,bool alpha=true){
        if(texture==null)return;
        if(Capture==null){GUI.DrawTextureWithTexCoords(rect,texture,uv,alpha);return;}
        Capture.Add(new Item{rect=rect,texture=texture,uv=uv,color=Color});
    }
    public static void Texture(Rect rect,Texture texture,ScaleMode mode=ScaleMode.StretchToFill){
        if(texture==null)return;
        if(Capture==null){GUI.DrawTexture(rect,texture,mode);return;}
        Rect uv=new Rect(0,0,1,1);
        float source=texture.width/(float)texture.height,target=rect.width/rect.height;
        if(mode==ScaleMode.ScaleAndCrop){if(source>target){uv.width=target/source;uv.x=(1-uv.width)*.5f;}else{uv.height=source/target;uv.y=(1-uv.height)*.5f;}}
        if(mode==ScaleMode.ScaleToFit){if(source>target){float h=rect.width/source;rect.y+=(rect.height-h)*.5f;rect.height=h;}else{float w=rect.height*source;rect.x+=(rect.width-w)*.5f;rect.width=w;}}
        Uv(rect,texture,uv);
    }
}
}
