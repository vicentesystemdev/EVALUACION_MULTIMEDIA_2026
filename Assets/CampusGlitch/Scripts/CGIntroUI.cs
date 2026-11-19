using UnityEngine;
namespace CampusGlitch {
// Introducción narrativa: se muestra la primera vez que se entra al Nivel 1.
// Teclado / mando / táctil: Enter, Espacio o A avanzan; Esc o B la saltan.
public partial class CGGame {
    int introSlide;float introSlideAt,introLastAdvance=-10;
    const float IntroCharsPerSecond=55f;
    static readonly string[] IntroTitles={"PRIMER DÍA EN EL CAMPUS","LA ANOMALÍA DIGITAL","EL LOBO GUARDIÁN","TU MISIÓN"};
    static readonly string[] IntroTexts={
      "Comienza el semestre en UNIFRANZ La Paz. Cientos de estudiantes nuevos llegan al campus buscando sus aulas, laboratorios, oficinas y servicios.",
      "De pronto, una anomalía digital altera la universidad: las señales se apagan, los accesos se bloquean y la información se desordena. Por los pasillos aparecen criaturas llamadas BUGS.",
      "Para devolver el orden despierta el Lobo Guardián. Recorrerá el campus zona por zona, desde la entrada principal hasta el núcleo final, para recuperar la información perdida.",
      "En cada zona: habla con la gente del campus, recoge los 3 fragmentos de información, resuelve la terminal y cruza el portal. Cada zona restaurada te acerca a conocer tu universidad."
    };

    // Solo cuando el jugador entra al Nivel 1 desde el menú o el mapa (ChooseLevel).
    // Las pruebas automatizadas abren la escena del nivel directamente y no la ven.
    // Se muestra una vez por cada apertura del juego (no se guarda en la partida).
    static bool levelEnteredFromMenu,introShownThisSession;
    bool IntroPending(){bool fromMenu=levelEnteredFromMenu;levelEnteredFromMenu=false;return fromMenu&&sceneLevel==0&&!introShownThisSession;}
    void BeginIntro(){page=Page.Intro;introSlide=0;introSlideAt=Time.unscaledTime;selection=0;Time.timeScale=0;toast="";toastUntil=0;}
    int IntroVisibleChars {get{string t=IntroTexts[introSlide];if(CGSettingsRuntime.ReduceFlash)return t.Length;return Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime-introSlideAt)*IntroCharsPerSecond),0,t.Length);}}
    void HandleIntro(){
      if(CGControl.Back()){FinishIntro();return;}
      if(CGControl.Confirm()||CGTouch.AnyTap())AdvanceIntro();
    }
    void AdvanceIntro(){
      if(Time.unscaledTime-introLastAdvance<.15f)return; // un toque puede llegar por táctil y por botón a la vez
      introLastAdvance=Time.unscaledTime;
      if(IntroVisibleChars<IntroTexts[introSlide].Length){introSlideAt=-1000;return;} // primero completa el texto
      CGAudio.Menu(true);
      if(++introSlide>=IntroTitles.Length){FinishIntro();return;}
      introSlideAt=Time.unscaledTime;
    }
    void FinishIntro(){
      if(page!=Page.Intro)return;
      introShownThisSession=true;
      page=Page.Playing;Time.timeScale=1;selection=0;resumedInputFrame=Time.frameCount;levelEntered=Time.unscaledTime;
      Hint(CGLevels.All[sceneLevel].objective);
    }
    void DrawIntro(){
      RectColor(new Rect(0,0,1280,720),new Color(.008f,.014f,.035f,.72f));
      Header("INTRODUCCIÓN · "+(introSlide+1)+" / "+IntroTitles.Length);
      Rect art=new Rect(70,140,500,440),text=new Rect(600,140,610,440);
      Panel(art,new Color(.25f,.8f,1f,1));Panel(text);
      DrawIntroArt(new Rect(art.x+6,art.y+6,art.width-12,art.height-12));
      var title=new GUIStyle(titleStyle){fontSize=36,alignment=TextAnchor.UpperLeft};
      CGUIDraw.Text(new Rect(text.x+32,text.y+28,text.width-64,52),IntroTitles[introSlide],title);
      RectColor(new Rect(text.x+32,text.y+86,120,4),new Color(1,.52f,.1f,1));
      var body=new GUIStyle(textStyle){fontSize=26,alignment=TextAnchor.UpperLeft};
      string full=IntroTexts[introSlide];
      CGUIDraw.Text(new Rect(text.x+32,text.y+112,text.width-64,300),full.Substring(0,IntroVisibleChars),body);
      for(int i=0;i<IntroTitles.Length;i++)RectColor(new Rect(text.x+32+i*30,text.yMax-38,i==introSlide?22:12,8),i==introSlide?new Color(1,.52f,.1f,1):new Color(.45f,.55f,.7f,.8f));
      bool last=introSlide==IntroTitles.Length-1;
      if(Button(last?"¡COMENZAR!":"CONTINUAR",0,text.xMax-300,text.yMax+22,300,58))AdvanceIntro();
      if(Click(new Rect(text.x,text.yMax+22,240,58),"SALTAR INTRO",tinyStyle))FinishIntro();
      RectColor(new Rect(0,676,1280,44),new Color(.02f,.03f,.055f,.95f));
      CGUIDraw.Text(new Rect(28,684,1235,28),CGTouch.Enabled?"TOCA PARA CONTINUAR   ·   SALTAR INTRO PARA EMPEZAR A JUGAR":"ENTER / A: CONTINUAR     ·     ESC / B: SALTAR INTRODUCCIÓN",tinyStyle);
    }
    void DrawIntroArt(Rect r){
      float t=Time.unscaledTime;int frame=(int)(t*6)%4;
      var campus=Resources.Load<Texture2D>("CG/VisualV2/MenuCampus")??Resources.Load<Texture2D>("CG/VisualV2/MapCampus");
      if(introSlide<=1&&campus!=null)CGUIDraw.Texture(r,campus,ScaleMode.ScaleAndCrop);
      else RectColor(r,new Color(.03f,.06f,.12f,1));
      switch(introSlide){
        case 0:
          RectColor(new Rect(r.x,r.yMax-60,r.width,60),new Color(.01f,.02f,.05f,.8f));
          CGUIDraw.Text(new Rect(r.x,r.yMax-55,r.width,50),"UNIFRANZ · SEDE LA PAZ",new GUIStyle(textStyle){fontSize=24});
          break;
        case 1:
          RectColor(r,new Color(.35f,0,.08f,.35f));
          if(!CGSettingsRuntime.ReduceFlash)for(int k=0;k<7;k++){float y=r.y+Mathf.Repeat(k*97f+t*140f,r.height-6);RectColor(new Rect(r.x,y,r.width,2+k%3),new Color(1,.2f,.3f,.35f));}
          string[] bugs={"Bug_Terrestre","Bug_Saltador","Centinela"};
          for(int k=0;k<3;k++){float bob=CGSettingsRuntime.ReduceFlash?0:Mathf.Sin(t*3+k)*8;CGGuardianArt.DrawFit(new Rect(r.x+25+k*155,r.y+250+bob-(k==2?90:0),130,130),CGArt.Enemy(bugs[k],frame));}
          CGUIDraw.Text(new Rect(r.x,r.y+20,r.width,40),"ERROR 404 · SEÑAL PERDIDA",new GUIStyle(textStyle){fontSize=26});
          break;
        case 2:
          HeroPortrait(r.x+r.width*.18f,r.y+20,r.width*.64f,r.height-40,CGStore.Data.purpleEquipped);
          break;
        default:
          string[] icons={"DataFragment","TerminalHacking","PortalTransition"};string[] names={"3 FRAGMENTOS","TERMINAL","PORTAL"};
          for(int k=0;k<3;k++){
            float y=r.y+25+k*138;
            CGGuardianArt.DrawFit(new Rect(r.x+40,y,110,110),CGArt.Get("01_Nuevos_Modulares/Global_Gameplay/"+icons[k]));
            CGUIDraw.Text(new Rect(r.x+170,y+30,r.width-200,50),(k+1)+". "+names[k],new GUIStyle(textStyle){fontSize=26,alignment=TextAnchor.MiddleLeft});
            if(k<2)RectColor(new Rect(r.x+93,y+118,4,16),new Color(1,.52f,.1f,1));
          }
          break;
      }
    }
}
}
