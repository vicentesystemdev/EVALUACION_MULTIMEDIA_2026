using System.Collections.Generic;
using UnityEngine;
namespace CampusGlitch {
public static class CGHelpArt {
    static readonly Dictionary<Texture2D,RenderTexture> clean=new Dictionary<Texture2D,RenderTexture>();static Material material;
    public static void Cleanup(){foreach(var t in clean.Values)if(t!=null){t.Release();if(Application.isPlaying)Object.Destroy(t);else Object.DestroyImmediate(t);}clean.Clear();if(material!=null){if(Application.isPlaying)Object.Destroy(material);else Object.DestroyImmediate(material);}material=null;}
    public static void Sprite(Rect area,Sprite sprite,bool trim=false){
        if(sprite==null)return;var source=sprite.texture;Texture texture=source;
        if(trim){
            if(!clean.TryGetValue(source,out var target)||target==null||!target.IsCreated()){
                var shader=Resources.Load<Shader>("CG/GuardianMenu/MenuPortrait");if(shader!=null){if(material==null)material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};target=new RenderTexture(source.width,source.height,0,RenderTextureFormat.ARGB32){name="Ayuda_Actores_Definidos",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};target.Create();var previous=RenderTexture.active;bool srgb=GL.sRGBWrite;try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(source,target,material);}finally{GL.sRGBWrite=srgb;RenderTexture.active=previous;}clean[source]=target;}
            }
            if(target!=null)texture=target;
        }
        var region=sprite.textureRect;float scale=Mathf.Min(area.width/region.width,area.height/region.height);var r=new Rect(area.center.x-region.width*scale*.5f,area.yMax-region.height*scale,region.width*scale,region.height*scale);CGUIDraw.Uv(r,texture,new Rect(region.x/source.width,region.y/source.height,region.width/source.width,region.height/source.height));
    }
}
}
