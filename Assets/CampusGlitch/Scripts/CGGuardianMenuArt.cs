using System;
using UnityEngine;

namespace CampusGlitch {
/// <summary>High-resolution presentation atlas; the playable wolf keeps its original animation atlas.</summary>
public static class CGGuardianMenuArt {
    [Serializable]class Atlas {public CGGuardianArt.Frame[] frames;}
    static Texture2D source;static RenderTexture clean;static Material material;static CGGuardianArt.Frame[] frames;
    static RenderTexture processed;static Material glitchMaterial;static int processedTick=-1;
    static Texture GlitchTexture(float clock,float strength){
        var shader=Resources.Load<Shader>("CG/GuardianMenu/MenuGlitch");if(shader==null||!shader.isSupported)return clean;
        if(glitchMaterial==null)glitchMaterial=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
        if(processed==null||!processed.IsCreated()){
            processed=new RenderTexture(source.width,source.height,0,RenderTextureFormat.ARGB32){name="Guardian_Menu_Fallo",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};processed.Create();processedTick=-1;
        }
        int tick=Mathf.FloorToInt(clock*30);if(tick==processedTick)return processed;processedTick=tick;
        glitchMaterial.SetFloat("_Strength",strength);glitchMaterial.SetFloat("_Phase",clock);
        var target=RenderTexture.active;bool srgb=GL.sRGBWrite;
        try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(clean,processed,glitchMaterial);}
        finally{GL.sRGBWrite=srgb;RenderTexture.active=target;}
        return processed;
    }
    static bool Load(){
        if(clean!=null&&clean.IsCreated()&&frames!=null)return true;
        source=Resources.Load<Texture2D>("CG/GuardianMenu/GuardianMenuHD");var data=Resources.Load<TextAsset>("CG/GuardianMenu/GuardianMenuHD_Frames");
        var shader=Resources.Load<Shader>("CG/GuardianMenu/MenuPortrait");if(source==null||data==null||shader==null||!shader.isSupported)return false;
        var atlas=JsonUtility.FromJson<Atlas>(data.text);frames=atlas!=null?atlas.frames:null;if(frames==null||frames.Length!=8)return false;
        foreach(var f in frames)if(f.w<=0||f.h<=0||f.x<0||f.y<0||f.x+f.w>source.width||f.y+f.h>source.height)return false;
        source.filterMode=FilterMode.Point;source.anisoLevel=0;source.wrapMode=TextureWrapMode.Clamp;
        if(material==null)material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
        if(clean!=null){clean.Release();UnityEngine.Object.DestroyImmediate(clean);}
        clean=new RenderTexture(source.width,source.height,0,RenderTextureFormat.ARGB32){name="Guardian_Menu_Definido",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};clean.Create();
        var previousTarget=RenderTexture.active;bool previousSrgb=GL.sRGBWrite;
        try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(source,clean,material);}
        finally{GL.sRGBWrite=previousSrgb;RenderTexture.active=previousTarget;}
        return true;
    }
    public static bool Draw(Rect area,bool purple){
        if(!Load())return false;
        // Four deliberately subtle idle poses, held long enough for a calm menu presentation.
        int skin=purple?1:0,frame=(int)(CGMenuTheme.Clock*2)%4;var f=frames[skin*4+frame];var basis=frames[skin*4];
        float scale=Mathf.Min(area.width/basis.w,area.height/basis.h);
        var target=new Rect(area.center.x-f.w*scale*.5f,area.yMax-f.h*scale,f.w*scale,f.h*scale);
        CGUIDraw.Uv(target,clean,new Rect(f.x/(float)source.width,f.y/(float)source.height,f.w/(float)source.width,f.h/(float)source.height));return true;
    }
    public static bool DrawPortrait(Rect area,bool purple){
        if(!Load())return false;var f=frames[purple?4:0];float w=f.w*.7f,h=f.h*.46f,scale=Mathf.Min(area.width/w,area.height/h);
        var rect=new Rect(area.center.x-w*scale*.5f,area.center.y-h*scale*.5f,w*scale,h*scale);
        CGUIDraw.Uv(rect,clean,new Rect((f.x+f.w*.26f)/source.width,(f.y+f.h*.54f)/source.height,w/source.width,h/source.height));return true;
    }
    public static bool DrawGlitch(Rect area,bool equippedPurple,bool cycle,float seconds,float duration,float intensity){
        if(!Load())return false;
        float clock=CGMenuTheme.Clock,age=Mathf.Repeat(clock,Mathf.Max(4,seconds));int chapter=Mathf.FloorToInt(clock/Mathf.Max(4,seconds));
        bool next=cycle&&chapter%2!=0?!equippedPurple:equippedPurple;
        if(age>=duration)return Draw(area,next);
        bool previous=cycle?!next:next;float blend=Mathf.Clamp01(age/Mathf.Max(.25f,duration));
        int pose=(int)(clock*2)%4;var tint=CGUIDraw.Color;var texture=GlitchTexture(clock,intensity*Mathf.Sin(blend*Mathf.PI));
        for(int band=0;band<12;band++){
            bool reveal=(band*7%12)<blend*12;int skin=(reveal?next:previous)?1:0;
            var f=frames[skin*4+(reveal?pose:(pose+3)%4)];var basis=frames[skin*4];float scale=Mathf.Min(area.width/basis.w,area.height/basis.h);
            var rect=new Rect(area.center.x-f.w*scale*.5f,area.yMax-f.h*scale+f.h*scale*band/12,f.w*scale,f.h*scale/12+.1f);
            var uv=new Rect(f.x/(float)source.width,(f.y+f.h*(11-band)/12f)/source.height,f.w/(float)source.width,f.h/(12f*source.height));
            float tear=Mathf.Sin((band*13+(int)(age*22))*2.1f)*4*intensity*Mathf.Sin(blend*Mathf.PI);
            if(Mathf.Abs(tear)>.5f){var echo=rect;echo.x+=tear*2;CGUIDraw.Color=new Color(.25f,1,.9f,tint.a*.32f*intensity);CGUIDraw.Uv(echo,texture,uv);}
            rect.x+=tear;CGUIDraw.Color=tint;CGUIDraw.Uv(rect,texture,uv);
        }
        CGUIDraw.Color=tint;return true;
    }
}
}
