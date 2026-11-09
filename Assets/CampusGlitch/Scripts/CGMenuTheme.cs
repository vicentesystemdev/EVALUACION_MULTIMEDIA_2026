using UnityEngine;

namespace CampusGlitch {
public static class CGMenuTheme {
    public static float Clock {get{
#if UNITY_EDITOR
        if(!Application.isPlaying)return (float)UnityEditor.EditorApplication.timeSinceStartup;
#endif
        return Time.unscaledTime;
    }}
    static Font title,interfaceFont,body;static CGMenuDesign design;
    public static CGMenuDesign Design {get{if(design==null)design=Resources.Load<CGMenuDesign>("CG/Presentation/CampusMenu");return design;}}
    public static Font Title {get{if(Design!=null&&Design.titleFont!=null)return Design.titleFont;if(title==null)title=Resources.Load<Font>("CG/Typography/Audiowide-Regular");return title;}}
    public static Font Interface {get{if(Design!=null&&Design.buttonFont!=null)return Design.buttonFont;if(interfaceFont==null)interfaceFont=Resources.Load<Font>("CG/Typography/BarlowCondensed-SemiBold");return interfaceFont;}}
    public static Font Body {get{if(Design!=null&&Design.bodyFont!=null)return Design.bodyFont;if(body==null)body=Resources.Load<Font>("CG/Typography/Questrial-Regular");return body;}}
    public static readonly string[] MapNames={"ACCESO PRINCIPAL","RECEPCIÓN Y LOBBY","MEZANINE","LABORATORIOS DE SALUD","INGENIERÍA DE SISTEMAS","AULAS Y PASILLOS","BIBLIOTECA E INNOVA","TERRAZA / LOUNGE","CLÍNICA ODONTOLÓGICA","SUBSUELO / ANATOMÍA"};
}

// Unscaled, short transitions; reducing effects also removes decorative motion.
public sealed class CGMenuMotion {
    readonly float[] focus=new float[10];int page=-1;float entered;
    public void Tick(int current,int selection,bool reduced){
        if(page!=current){page=current;entered=Time.unscaledTime;for(int i=0;i<focus.Length;i++)focus[i]=0;}
        for(int i=0;i<focus.Length;i++)focus[i]=reduced?(i==selection?1:0):Mathf.MoveTowards(focus[i],i==selection?1:0,Time.unscaledDeltaTime*(CGMenuTheme.Design!=null?CGMenuTheme.Design.focusSpeed:8));
    }
    public float Focus(int index){return index>=0&&index<focus.Length?focus[index]:0;}
    public float Entry(bool reduced){return reduced?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-entered)/(CGMenuTheme.Design!=null?CGMenuTheme.Design.screenTransition:.22f)));}
}
}
