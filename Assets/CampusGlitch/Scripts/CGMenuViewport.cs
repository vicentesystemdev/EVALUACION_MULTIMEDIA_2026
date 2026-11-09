using UnityEngine;
namespace CampusGlitch {
public static class CGMenuViewport {
    // Screen.safeArea uses a bottom-left origin; IMGUI uses top-left.
    public static Rect Content(float width,float height,Rect safe){
        if(safe.width<=0||safe.height<=0)safe=new Rect(0,0,width,height);
        float unit=Mathf.Min(safe.width/1280f,safe.height/720f);
        return new Rect(safe.x+(safe.width-1280*unit)*.5f,height-safe.yMax+(safe.height-720*unit)*.5f,1280*unit,720*unit);
    }
}
}
