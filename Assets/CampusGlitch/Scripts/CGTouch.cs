using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch=UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase=UnityEngine.InputSystem.TouchPhase;
namespace CampusGlitch {
public static class CGTouch {
    public const int Left=0,Right=1,Jump=2,Attack=3,Interact=4,Pause=5,Ability=6,Run=7;
    static readonly string[] Labels={"IZQ","DER","SALTAR","ATACAR","USAR","PAUSA","PODER","CORRER"};
    struct Finger {public Vector2 start;public int button;public bool fired;}
    static readonly Dictionary<int,Finger> fingers=new Dictionary<int,Finger>();static readonly List<int> ended=new List<int>();
    static int taps,held,frame=-1;static readonly float[] expires=new float[8];static float axis;static bool running;
    #if UNITY_EDITOR
    public static bool EditorSimulation;
#endif
    public static bool Enabled {get{return Application.isMobilePlatform
#if UNITY_EDITOR
        ||EditorSimulation
#endif
        ;}}
    public static float Unit {get{return Mathf.Min(Screen.safeArea.width/1280f,Screen.safeArea.height/720f);}}
    public static float Axis {get{return Enabled?axis:0;}}
    public static bool Running {get{return Enabled&&running;}}
    public static Rect Layout(int b,float width,float height,Rect safe){
        float u=Mathf.Min(safe.width/1280f,safe.height/720f),m=18*u,s=102*u,left=safe.xMin,right=safe.xMax,bottom=height-safe.yMin;
        if(b==Left)return new Rect(left+m,bottom-m-s,s,s);if(b==Right)return new Rect(left+2*m+s,bottom-m-s,s,s);
        if(b==Run)return new Rect(left+m,bottom-2*m-2*s,2*s+m,66*u);
        if(b==Jump)return new Rect(right-m-s,bottom-m-s,s,s);if(b==Attack)return new Rect(right-2*m-2*s,bottom-m-s,s,s);
        if(b==Ability)return new Rect(right-3*m-3*s,bottom-m-s,s,s);if(b==Interact)return new Rect(right-m-s,bottom-2*m-2*s,s,s);
        var content=CGMenuViewport.Content(width,height,safe);return new Rect(content.x+690*u,content.y+12*u,130*u,50*u);
    }
    public static Rect ButtonRect(int b){return Layout(b,Screen.width,Screen.height,Screen.safeArea);}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Initialize(){Reset();if(!EnhancedTouchSupport.enabled)EnhancedTouchSupport.Enable();Application.focusChanged-=Focus;Application.focusChanged+=Focus;}
    static void Focus(bool focused){if(!focused)Reset();}
    public static void Reset(){fingers.Clear();taps=held=0;axis=0;running=false;frame=-1;}
    public static int Gesture(Vector2 delta,float duration,float unit){if(duration>1.2f)return -1;float threshold=55*unit;if(delta.y < -threshold&&Mathf.Abs(delta.y)>Mathf.Abs(delta.x)*1.1f)return Jump;if(Mathf.Abs(delta.x)>threshold&&Mathf.Abs(delta.x)>Mathf.Abs(delta.y)*1.1f)return Ability;return delta.magnitude<20*unit&&duration<.35f?Attack:-1;}
    public static void UpdateInput(){
        if(!Enabled||frame==Time.frameCount)return;frame=Time.frameCount;held=0;axis=0;running=false;for(int b=0;b<8;b++)if(Time.unscaledTime>=expires[b])taps&=~(1<<b);
        if(!EnhancedTouchSupport.enabled)EnhancedTouchSupport.Enable();CGStore.Load();bool gestures=CGStore.Data.touchGestures;ended.Clear();
        foreach(var touch in Touch.activeTouches){int id=touch.touchId;var point=new Vector2(touch.screenPosition.x,Screen.height-touch.screenPosition.y);
            if(touch.phase==TouchPhase.Began){int button=-1;for(int b=0;b<Labels.Length;b++)if((!gestures||b==Interact||b==Pause)&&ButtonRect(b).Contains(point)){button=b;break;}if(gestures&&button<0)button=point.x<Screen.safeArea.center.x?-2:-3;fingers[id]=new Finger{start=point,button=button};if(button>=0)QueueTap(button);}
            if(!fingers.TryGetValue(id,out var finger))continue;
            if(touch.phase==TouchPhase.Canceled){ended.Add(id);continue;}
            if(touch.phase==TouchPhase.Ended){if(finger.button==-3&&!finger.fired){int action=Gesture(point-finger.start,(float)(touch.time-touch.startTime),Unit);if(action>=0)QueueTap(action);}ended.Add(id);continue;}
            if(finger.button>=0)held|=1<<finger.button;
            if(finger.button==-2){float x=Mathf.Clamp((point.x-finger.start.x)/(85*Unit),-1,1);if(Mathf.Abs(x)>.16f)axis=x;running=Mathf.Abs(x)>.85f;}
            if(finger.button==-3&&!finger.fired){int action=Gesture(point-finger.start,(float)(touch.time-touch.startTime),Unit);if(action==Jump||action==Ability){QueueTap(action);finger.fired=true;fingers[id]=finger;}}
        }
        foreach(int id in ended)fingers.Remove(id);if((held&(1<<Left))!=0)axis=-1;if((held&(1<<Right))!=0)axis=1;running|=(held&(1<<Run))!=0;
    }
    static void QueueTap(int b){taps|=1<<b;expires[b]=Time.unscaledTime+.18f;}
    public static bool Held(int b){return Enabled&&(held&(1<<b))!=0;}
    public static bool Tapped(int b){if(!Enabled||(taps&(1<<b))==0)return false;taps&=~(1<<b);return true;}
    public static bool AnyTap(){if(!Enabled)return false;foreach(var touch in Touch.activeTouches)if(touch.phase==TouchPhase.Began)return true;return false;}
    static GUIStyle style;
    public static void Draw(){
        if(!Enabled)return;if(style==null){style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,wordWrap=true};style.normal.textColor=Color.white;}style.font=CGMenuTheme.Interface;style.fontSize=Mathf.Max(12,Mathf.RoundToInt(20*Unit));CGStore.Load();
        var color=GUI.color;for(int b=0;b<Labels.Length;b++){if(CGStore.Data.touchGestures&&b!=Interact&&b!=Pause)continue;Rect r=ButtonRect(b);GUI.color=Held(b)?new Color(1,.5f,.12f,.8f):new Color(.02f,.035f,.075f,.65f);GUI.DrawTexture(r,CGMenuArtwork.Button(Held(b),new Color(1,.5f,.12f)));GUI.color=color;GUI.Label(r,Labels[b],style);}
        if(CGStore.Data.touchGestures){var r=ButtonRect(Run);GUI.color=new Color(1,.6f,.18f,.65f);GUI.DrawTexture(r,CGMenuArtwork.Button(false,new Color(1,.5f,.12f)));GUI.color=color;GUI.Label(r,"ARRASTRA PARA MOVER",style);}
        GUI.color=color;
    }
}
}
