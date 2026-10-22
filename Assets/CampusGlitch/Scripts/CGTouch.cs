using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CampusGlitch {
// On-screen touch controls for Android. Reads Touchscreen from the Input System and is
// only enabled on mobile platforms, so keyboard/gamepad behaviour on Windows is unchanged.
public static class CGTouch {
  public const int Left=0,Right=1,Jump=2,Attack=3,Interact=4,Pause=5;
  static readonly string[] Labels={"<\nIZQ",">\nDER","SALTAR","ATACAR","USAR","II  PAUSA"};
  public static bool Enabled {get {return Application.isMobilePlatform;}}
  // Uniform scale against the 1280x720 reference used by CGGame.OnGUI.
  public static float Unit {get {return Mathf.Min(Screen.width/1280f,Screen.height/720f);}}

  // Button rects in GUI screen pixels (top-left origin), anchored to the device safe area.
  public static Rect ButtonRect(int b){
    float u=Unit,m=24*u,s=150*u;
    Rect sa=Screen.safeArea;
    float left=sa.xMin,right=sa.xMax,bottom=Screen.height-sa.yMin;
    switch(b){
      case Left:return new Rect(left+m,bottom-m-s,s,s);
      case Right:return new Rect(left+2*m+s,bottom-m-s,s,s);
      case Jump:return new Rect(right-m-s,bottom-m-s,s,s);
      case Attack:return new Rect(right-2*m-2*s,bottom-m-s,s,s);
      case Interact:return new Rect(right-m-s,bottom-2*m-2*s,s,s);
      default:{ // Pause sits in the gap between the two HUD panels (reference x 690..820).
        float offX=(Screen.width-1280*u)*.5f,offY=(Screen.height-720*u)*.5f;
        return new Rect(offX+690*u,offY+12*u,130*u,50*u);}
    }
  }
  static Vector2 ToGUI(TouchControl t){Vector2 p=t.position.ReadValue();return new Vector2(p.x,Screen.height-p.y);}
  public static bool Held(int b){
    if(!Enabled||Touchscreen.current==null)return false;Rect r=ButtonRect(b);
    foreach(TouchControl t in Touchscreen.current.touches)if(t.press.isPressed&&r.Contains(ToGUI(t)))return true;
    return false;
  }
  public static bool Tapped(int b){
    if(!Enabled||Touchscreen.current==null)return false;Rect r=ButtonRect(b);
    foreach(TouchControl t in Touchscreen.current.touches)if(t.press.wasPressedThisFrame&&r.Contains(ToGUI(t)))return true;
    return false;
  }
  public static bool AnyTap(){
    if(!Enabled||Touchscreen.current==null)return false;
    foreach(TouchControl t in Touchscreen.current.touches)if(t.press.wasPressedThisFrame)return true;
    return false;
  }
  static GUIStyle style;
  // Called from CGGame.OnGUI with an identity GUI.matrix.
  public static void Draw(){
    if(!Enabled)return;
    if(style==null){style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,wordWrap=true};style.normal.textColor=Color.white;}
    style.fontSize=Mathf.RoundToInt(22*Unit);
    Color old=GUI.color;
    for(int b=0;b<Labels.Length;b++){
      Rect r=ButtonRect(b);bool on=Held(b);
      GUI.color=on?new Color(1f,.52f,.14f,.75f):new Color(.02f,.035f,.075f,.5f);GUI.DrawTexture(r,Texture2D.whiteTexture);
      GUI.color=new Color(1f,.58f,.17f,.9f);float k=Mathf.Max(2,3*Unit);
      GUI.DrawTexture(new Rect(r.x,r.y,r.width,k),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(r.x,r.yMax-k,r.width,k),Texture2D.whiteTexture);
      GUI.DrawTexture(new Rect(r.x,r.y,k,r.height),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(r.xMax-k,r.y,k,r.height),Texture2D.whiteTexture);
      GUI.color=old;GUI.Label(r,Labels[b],style);
    }
    GUI.color=old;
  }
}
}
