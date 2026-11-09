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
    public static void Text(Rect rect,string text,GUIStyle style){
        if(Capture==null){GUI.Label(rect,text,style);return;}
        Capture.Add(new Item{rect=rect,text=text,font=style.font,fontSize=style.fontSize,fontStyle=style.fontStyle,alignment=style.alignment,wrap=style.wordWrap,color=style.normal.textColor*GUI.color});
    }
    public static void Uv(Rect rect,Texture texture,Rect uv,bool alpha=true){
        if(Capture==null){GUI.DrawTextureWithTexCoords(rect,texture,uv,alpha);return;}
        Capture.Add(new Item{rect=rect,texture=texture,uv=uv,color=GUI.color});
    }
    public static void Texture(Rect rect,Texture texture,ScaleMode mode=ScaleMode.StretchToFill){
        if(Capture==null){GUI.DrawTexture(rect,texture,mode);return;}
        Rect uv=new Rect(0,0,1,1);
        float source=texture.width/(float)texture.height,target=rect.width/rect.height;
        if(mode==ScaleMode.ScaleAndCrop){if(source>target){uv.width=target/source;uv.x=(1-uv.width)*.5f;}else{uv.height=source/target;uv.y=(1-uv.height)*.5f;}}
        if(mode==ScaleMode.ScaleToFit){if(source>target){float h=rect.width/source;rect.y+=(rect.height-h)*.5f;rect.height=h;}else{float w=rect.height*source;rect.x+=(rect.width-w)*.5f;rect.width=w;}}
        Uv(rect,texture,uv);
    }
}
}
