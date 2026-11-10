using UnityEngine;

namespace CampusGlitch {
public partial class CGGame {
    float focusChangedAt=-99,accessAnimationStarted;
    bool loadingLevel,skipAccessAnimation;
    string loadingCaption="ABRIENDO DESTINO";
    bool ShowCampusAccess {get{return loadingLevel&&!CGStore.Data.reduceFlash&&CGMenuTheme.Design!=null&&CGMenuTheme.Design.campusEntrance;}}
    float AccessProgress {get{return Mathf.Clamp01((CGMenuTheme.Clock-accessAnimationStarted)/(CGMenuTheme.Design!=null?CGMenuTheme.Design.entranceDuration:1.25f));}}
    void DrawGlitchFocus(Rect rect,Color accent){
        var design=CGMenuTheme.Design;if(design==null||!design.glitchHighlights||CGStore.Data.reduceFlash)return;
        float clock=CGMenuTheme.Clock*design.glitchSpeed,intensity=design.glitchIntensity;
        float scan=Mathf.Repeat(clock*.38f,1);
        RectColor(new Rect(rect.x+12+scan*(rect.width-44),rect.y+2,21,2),new Color(.45f,1,.85f,.9f*intensity));
        RectColor(new Rect(rect.xMax-34-scan*(rect.width-44),rect.yMax-4,21,2),new Color(1,.75f,.25f,.9f*intensity));
        // Brief image tears affect the panel artwork, while the actual label stays stable.
        float age=editorMenuPreview?Mathf.Repeat(clock,4):Time.unscaledTime-focusChangedAt;
        if(age<.19f||Mathf.Repeat(clock,4.5f)<.12f){
            var tint=CGUIDraw.Color;CGUIDraw.Color=new Color(.3f,1,.83f,tint.a*.7f*intensity);
            CGUIDraw.Uv(new Rect(rect.x-3,rect.y+9,rect.width+6,3),CGMenuArtwork.Button(true,accent),new Rect(0,.7f,1,.04f));
            CGUIDraw.Uv(new Rect(rect.x+3,rect.yMax-10,rect.width-6,2),CGMenuArtwork.Button(true,accent),new Rect(0,.15f,1,.025f));CGUIDraw.Color=tint;
        }
    }
    void DrawGlitchHighlights(){
        var design=CGMenuTheme.Design;if(design==null||!design.glitchHighlights||CGStore.Data.reduceFlash)return;
        float clock=CGMenuTheme.Clock*design.glitchSpeed;
        for(int i=0;i<6;i++){
            float y=145+Mathf.Repeat(clock*(37+i*3)+i*79,470);
            float x=i%2==0?843:1242;
            RectColor(new Rect(x,y,2,18),new Color(.4f,1,.85f,design.glitchIntensity*.5f));
            RectColor(new Rect(x-4,y+18,10,2),new Color(1,.64f,.2f,design.glitchIntensity*.7f));
        }
    }
    void DrawMainGuardian(Rect area){
        var design=CGMenuTheme.Design;
        if(!CGStore.Data.useOriginalWolf&&design!=null&&design.guardianGlitch&&!CGStore.Data.reduceFlash&&CGGuardianMenuArt.DrawGlitch(area,CGStore.Data.purpleEquipped,design.cycleGuardianVariants,design.guardianCycleSeconds,design.guardianGlitchDuration,design.glitchIntensity))return;
        HeroPortrait(area.x,area.y,area.width,area.height,CGStore.Data.purpleEquipped);
    }
    void GlassRect(Rect rect,Rect aperture,Color color){
        var clipped=Rect.MinMaxRect(Mathf.Max(aperture.xMin,rect.xMin),Mathf.Max(aperture.yMin,rect.yMin),Mathf.Min(aperture.xMax,rect.xMax),Mathf.Min(aperture.yMax,rect.yMax));
        if(clipped.width>0&&clipped.height>0)RectColor(clipped,color);
    }
    void GlassLeaf(Rect leaf,Rect aperture,bool left){
        GlassRect(leaf,aperture,new Color(.55f,.75f,.84f,.17f));var frame=new Color(.035f,.045f,.055f,.78f);
        GlassRect(new Rect(leaf.x,leaf.y,3,leaf.height),aperture,frame);GlassRect(new Rect(leaf.xMax-3,leaf.y,3,leaf.height),aperture,frame);
        GlassRect(new Rect(leaf.x,leaf.y,leaf.width,3),aperture,frame);GlassRect(new Rect(leaf.x,leaf.yMax-3,leaf.width,3),aperture,frame);
        GlassRect(new Rect(leaf.x,leaf.y+leaf.height*.64f,leaf.width,5),aperture,new Color(1,.47f,.06f,.85f));
        GlassRect(new Rect(left?leaf.xMax-14:leaf.x+10,leaf.center.y-16,4,32),aperture,new Color(.8f,.85f,.88f,.88f));
    }
    void DrawCampusAccess(float alpha,string caption,float progress,bool entering){
        var design=CGMenuTheme.Design;Color accent=design!=null?design.amber:new Color(1,.56f,.12f);
        var original=CGUIDraw.Color;CGUIDraw.Color=new Color(original.r,original.g,original.b,original.a*alpha);
        var hall=design!=null?design.entranceBackground:null;
        float inset=Mathf.SmoothStep(0,.07f,progress);
        if(hall!=null){CGUIDraw.Uv(new Rect(0,0,1280,720),hall,new Rect(inset,inset,1-inset*2,1-inset*2));RectColor(new Rect(0,0,1280,720),new Color(.012f,.025f,.055f,.08f));}
        else RectColor(new Rect(0,0,1280,720),new Color(.012f,.025f,.055f,.98f));
        // A physical sliding-glass doorway, aligned to the architectural background.
        var baseDoor=design!=null?design.entranceDoorRect:new Rect(448,300,384,240);float zoom=1/(1-2*inset);
        var door=new Rect((baseDoor.x-inset*1280)*zoom,(baseDoor.y-inset*720)*zoom,baseDoor.width*zoom,baseDoor.height*zoom);
        if(hall==null){Panel(door,new Color(accent.r,accent.g,accent.b,.85f));var image=design!=null?design.menuBackground:null;if(image!=null)CGUIDraw.Uv(new Rect(door.x+8,door.y+8,door.width-16,door.height-16),image,new Rect(.35f,.04f,.24f,.39f));}
        float opening=entering?Mathf.SmoothStep(0,1,Mathf.Clamp01(progress*1.6f-.35f)):Mathf.Clamp01(progress);
        GlassLeaf(new Rect(door.x-door.width*.5f*opening,door.y,door.width*.5f,door.height),door,true);
        GlassLeaf(new Rect(door.center.x+door.width*.5f*opening,door.y,door.width*.5f,door.height),door,false);
        CGUIDraw.Color=new Color(original.r,original.g,original.b,original.a*alpha*.5f);CGUIDraw.Texture(new Rect(0,0,1280,180),CGMenuArtwork.Shade);CGUIDraw.Color=new Color(original.r,original.g,original.b,original.a*alpha);
        CGUIDraw.Text(new Rect(360,43,560,60),"UNIFRANZ · LA PAZ",new GUIStyle(titleStyle){fontSize=31});
        RectColor(new Rect(360,575,560,54),new Color(.012f,.025f,.055f,.7f));CGUIDraw.Text(new Rect(375,579,530,45),caption,textStyle);
        if(entering)CGUIDraw.Text(new Rect(360,642,560,32),"A / ENTER · OMITIR ENTRADA",tinyStyle);
        else{RectColor(new Rect(440,645,400,4),new Color(.16f,.25f,.32f));RectColor(new Rect(440,645,400*progress,4),accent);}
        CGUIDraw.Color=original;
    }
    void DrawMenuLoading(){
        Panel(new Rect(405,284,470,136));
        CGUIDraw.Text(new Rect(423,300,434,52),loadingCaption,new GUIStyle(buttonStyle){fontSize=25});
        RectColor(new Rect(445,383,390,4),new Color(.16f,.25f,.32f));RectColor(new Rect(445,383,390*loadProgress,4),CGMenuTheme.Design!=null?CGMenuTheme.Design.amber:new Color(1,.56f,.12f));
    }
}
}
