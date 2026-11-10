using UnityEngine;

namespace CampusGlitch {
// Cached, code-drawn insignia. Its lines stay separate from the editable portraits.
public static class CGCreditsArt {
    static Texture2D web;
    static float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return (p-a-d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)).magnitude;}
    public static Texture2D Web {get{
        if(web!=null)return web;
        const int size=128;web=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Insignia_aracnida",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};var pixels=new Color32[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            var p=new Vector2(x-64,y-64);float radius=p.magnitude;if(radius>61)continue;var c=new Color(.02f,.035f,.09f,.98f);
            if(radius>58)c=new Color(.22f,.65f,1);
            for(int ray=0;ray<12;ray++){float angle=ray*Mathf.PI/6;var end=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*57;if(Segment(p,Vector2.zero,end)<.6f)c=new Color(.28f,.44f,.7f);}
            for(int ring=1;ring<4;ring++)if(Mathf.Abs(radius-ring*14)<.6f)c=new Color(.28f,.44f,.7f);
            bool spider=p.x*p.x/49+p.y*p.y/169<1||(p-new Vector2(0,16)).sqrMagnitude<34;
            for(int side=-1;side<=1;side+=2)for(int leg=0;leg<4;leg++){
                var a=new Vector2(side*5,12-leg*7);var b=new Vector2(side*(19+leg*2),22-leg*13);var d=new Vector2(side*(29+leg),35-leg*21);
                spider|=Segment(p,a,b)<1.5f||Segment(p,b,d)<1.5f;
            }
            if(spider)c=new Color(1,.19f,.28f);pixels[y*size+x]=c;
        }
        web.SetPixels32(pixels);web.Apply(false,true);return web;
    }}
}
}
