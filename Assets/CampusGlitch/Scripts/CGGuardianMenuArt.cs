using System;
using UnityEngine;

namespace CampusGlitch {
/// <summary>High-resolution presentation atlas; the playable wolf keeps its original animation atlas.</summary>
public static class CGGuardianMenuArt {
    [Serializable]class Atlas {public CGGuardianArt.Frame[] frames;}
    static Texture2D source;static RenderTexture clean;static Material material;static CGGuardianArt.Frame[] frames;
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
}
}
