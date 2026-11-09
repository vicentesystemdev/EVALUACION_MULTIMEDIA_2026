using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace CampusGlitch {
public partial class CGGame:MonoBehaviour {
    public static bool ShowCreditsOnBoot=false;
    public string initialPage="Splash";
    public int sceneLevel=-1; // -1 = menu, 0..8 main, 9 = extra
    enum Page{Splash,Main,Map,Customization,Settings,Controls,Help,Credits,Playing,Paused,Quiz,Victory,Ending,ConfirmReset,Loading}
    Page page=Page.Splash;
    CGWorld world;int selection=0,mapSelection=0,chips=0;bool[] taken=new bool[3];bool terminalSolved=false,bossDead=true;string toast="",message="";float toastUntil=0;
    int rebinding=-1;bool padMode=false;
    public CGHero Hero {get {return world!=null?world.hero:null;}}
    public bool Playing {get {return page==Page.Playing;}}
    GUIStyle buttonStyle,titleStyle,textStyle,tinyStyle;
    readonly CGMenuNavigation menuNavigation=new CGMenuNavigation();
    readonly CGMenuMotion menuMotion=new CGMenuMotion();
    int lastMenuPage=-1,lastMenuSelection=-1;
    bool pointerMoved;float loadProgress,levelEntered;
    int resumedInputFrame=-1;
    bool editorMenuPreview;
    public bool AcceptsPlayerInput {get{return Playing&&Time.frameCount!=resumedInputFrame;}}
    readonly string[] mainItems={"CONTINUAR / JUGAR","MAPA DE NIVELES","PERSONALIZAR LOBO","AJUSTES","CONTROLES","AYUDA","CRÉDITOS","BORRAR PROGRESO","SALIR"};
    void Start(){
      // Editor previews exist so scenes are visible in 2D before pressing Play.
      // They are decorative only: runtime generates the playable scene with physics.
      foreach(var sceneRoot in gameObject.scene.GetRootGameObjects())if(sceneRoot.name=="CG_EDITOR_PREVIEW_2D")Destroy(sceneRoot);
      CGStore.Load();if(Camera.main==null){GameObject obj=new GameObject("Camera_UI");obj.tag="MainCamera";Camera camera=obj.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5.7f;if(FindFirstObjectByType<AudioListener>()==null)obj.AddComponent<AudioListener>();}
      CGCamera2DRuntime.PrepareForPlay();
      if(GetComponent<CGAudio>()==null)gameObject.AddComponent<CGAudio>();Application.targetFrameRate=60;AudioListener.volume=CGStore.Data.masterVolume;
      if(sceneLevel>=0){CreateLevel();}else{
        if(ShowCreditsOnBoot)page=Page.Credits;
        else if(!Enum.TryParse<Page>(initialPage,true,out page))page=Page.Splash;
        ShowCreditsOnBoot=false;
      }
    }
    void CreateLevel(){page=Page.Playing;chips=0;taken=new bool[3];terminalSolved=false;bossDead=string.IsNullOrEmpty(CGLevels.All[sceneLevel].boss);
      GameObject root=new GameObject("LEVEL_"+(sceneLevel+1));world=root.AddComponent<CGWorld>();world.Setup(this,sceneLevel);levelEntered=Time.unscaledTime;Hint(CGLevels.All[sceneLevel].objective);
    }
    void LateUpdate(){
      if(lastMenuPage!=(int)page){menuNavigation.Reset();lastMenuPage=(int)page;lastMenuSelection=selection;}
      else if(selection!=lastMenuSelection&&page!=Page.Playing&&page!=Page.Loading){CGAudio.Menu();lastMenuSelection=selection;}
      menuMotion.Tick((int)page,selection,CGStore.Data!=null&&CGStore.Data.reduceFlash);
    }
    void Update(){pointerMoved=Mouse.current!=null&&Mouse.current.delta.ReadValue().sqrMagnitude>1;if(rebinding>=0){
        if(CGControl.Back()){rebinding=-1;return;}
        if(CGTouch.AnyTap()){rebinding=-1;return;} // no physical keyboard on touch devices
        if(padMode){string b=CGControl.NextGamepadButton();if(!string.IsNullOrEmpty(b)){CGStore.Data.padBindings[rebinding]=b;CGStore.Save();rebinding=-1;Hint("Botón de mando actualizado");}}
        else{Key? k=CGControl.NextKeyboardKey();if(k.HasValue){if(k.Value!=Key.Escape){CGStore.Data.actionKeys[rebinding]=k.Value.ToString();CGStore.Save();Hint("Control actualizado");}rebinding=-1;}}
        return;}
      if(page==Page.Loading)return;
      if(page==Page.Map&&HandleMapShortcuts())return;
      if(page==Page.Controls&&((Keyboard.current!=null&&Keyboard.current.tabKey.wasPressedThisFrame)||(Gamepad.current!=null&&Gamepad.current.leftShoulder.wasPressedThisFrame))){padMode=!padMode;CGAudio.Menu();return;}
      if(page==Page.Playing){if(CGControl.Pause()){page=Page.Paused;Time.timeScale=0;selection=0;}return;}
      if(page==Page.Splash){if(CGControl.Confirm()||CGTouch.AnyTap()){page=Page.Main;selection=0;}return;}
      if(page==Page.Quiz){
        if(CGControl.Back()){page=Page.Playing;Time.timeScale=1;}
        else {selection=menuNavigation.Read(selection,3,false);if(CGControl.Confirm()){CGAudio.Menu(true);QuizAnswer(selection);}}
        return;
      }
      int n=page==Page.Main?mainItems.Length:page==Page.Controls?8:page==Page.Paused?3:page==Page.Settings?4:page==Page.Customization?4:page==Page.Map?10:page==Page.Victory||page==Page.Ending||page==Page.ConfirmReset?2:1;
      selection=menuNavigation.Read(selection,n,page==Page.Map);
      selection=(selection%n+n)%n;
      if(page==Page.Map){mapSelection=selection;}
      if(CGControl.Confirm()){
       CGAudio.Menu(true);
       switch(page){case Page.Main:MainSelect(selection);break;case Page.Map:ChooseLevel(mapSelection);break;case Page.Paused:PauseChoice(selection);break;
         case Page.Victory:if(selection==0)AfterVictory();else ToMain();break;
         case Page.Ending:if(selection==0){ShowCreditsOnBoot=true;StartCoroutine(TransitionToScene("SC_Bootstrap"));}else ToMain();break;
         case Page.ConfirmReset:if(selection==0)CGStore.Reset();ToMain();break;
         case Page.Customization:if(selection==0)EquipPurple(false);else if(selection==1)EquipPurple(true);
           else if(selection==2){CGStore.Data.useOriginalWolf=!CGStore.Data.useOriginalWolf;CGStore.Save();Hint(CGStore.Data.useOriginalWolf?"Usando el lobo clásico":"Usando el Guardián acorazado");}
           else ToMain();break;
         case Page.Settings:if(selection==0)AdjustVolume(.1f);else if(selection==1){CGStore.Data.reduceFlash=!CGStore.Data.reduceFlash;CGStore.Save();}else if(selection==2){CGStore.Data.subtitles=!CGStore.Data.subtitles;CGStore.Save();}else ToMain();break;
         case Page.Controls:rebinding=selection;break;
         default:ToMain();break;
       }
      }
      if(CGControl.Back()){if(page==Page.Paused){page=Page.Playing;Time.timeScale=1;}else if(page!=Page.Main)ToMain();}
    }
    public void Hint(string msg){toast=msg;toastUntil=Time.unscaledTime+4f;}
    void MainSelect(int i){switch(i){case 0: ChooseLevel(Mathf.Min(CGStore.Data.latestUnlocked,8));break;case 1:OpenUIScreen("SC_LevelMap",Page.Map);break;
      case 2:OpenUIScreen("SC_Customization",Page.Customization);break;case 3:OpenUIScreen("SC_Settings",Page.Settings);break;case 4:OpenUIScreen("SC_Controls",Page.Controls);break;
      case 5:OpenUIScreen("SC_Help",Page.Help);break;case 6:OpenUIScreen("SC_Credits",Page.Credits);break;case 7:page=Page.ConfirmReset;selection=1;break;case 8:Application.Quit();break;}}
    void ToMain(){Time.timeScale=1;
       if(Application.CanStreamedLevelBeLoaded("SC_MainMenu"))StartCoroutine(TransitionToScene("SC_MainMenu"));
       else if(sceneLevel>=0)StartCoroutine(TransitionToScene("SC_Bootstrap"));
       else{page=Page.Main;selection=0;}
    }
    void OpenUIScreen(string name,Page fallback){
       selection=0;Time.timeScale=1;
       if(Application.CanStreamedLevelBeLoaded(name))StartCoroutine(TransitionToScene(name));
       else page=fallback;
    }
    void ChooseLevel(int i){if(i<9&&i>CGStore.Data.latestUnlocked){Hint("Nivel bloqueado");return;}if(i==9&&!CGStore.Data.extraUnlocked){Hint("Se desbloquea después del nivel 5");return;}
      Time.timeScale=1;string name=CGLevels.Scene(i);
      if(Application.CanStreamedLevelBeLoaded(name))StartCoroutine(TransitionToScene(name));
      else{sceneLevel=i;CreateLevel();}}
    IEnumerator TransitionToScene(string next){
      page=Page.Loading;loadProgress=0;float began=Time.unscaledTime;
      var operation=SceneManager.LoadSceneAsync(next);operation.allowSceneActivation=false;
      while(operation.progress<.9f||Time.unscaledTime-began<.35f){loadProgress=Mathf.Clamp01(operation.progress/.9f);yield return null;}
      loadProgress=1;operation.allowSceneActivation=true;
    }
    void PauseChoice(int i){if(i==0){page=Page.Playing;Time.timeScale=1;resumedInputFrame=Time.frameCount;}else if(i==1){page=Page.Playing;Time.timeScale=1;resumedInputFrame=Time.frameCount;Respawn();}else ToMain();}
    void EquipPurple(bool on){if(on&&!CGStore.Data.purpleUnlocked){Hint("Morado: se desbloquea al terminar el nivel 2");return;}CGStore.Data.purpleEquipped=on;CGStore.Save();Hint(on?"Lobo morado equipado":"Lobo clásico equipado");}
    void AdjustVolume(float n){CGStore.Data.masterVolume=Mathf.Repeat(CGStore.Data.masterVolume+n,1.1f);AudioListener.volume=CGStore.Data.masterVolume;CGStore.Save();}
    public void TakeChip(int id){if(id<0||id>=3||taken[id])return;taken[id]=true;chips++;CGAudio.Play("Pickup");Hint("Fragmento recuperado: "+chips+"/3");}
    public void Attack(Vector3 pos){foreach(var c in Physics2D.OverlapCircleAll(pos,.82f)){CGEnemy e=c.GetComponent<CGEnemy>();if(e!=null)e.Hurt(1);}}
    public void BossDown(){bossDead=true;Hint("¡Jefe derrotado! El portal está disponible.");}
    public void KillHero(){if(Hero==null)return;CGAudio.Play("Damage");Hint("Restaurando desde el checkpoint...");Respawn();}
    void Respawn(){if(Hero==null||world==null)return;Hero.transform.position=new Vector3(world.checkpointX+0.5f,.45f,0);Hero.Body.linearVelocity=Vector2.zero;Hero.hearts=3;}
    public void InteractNearPlayer(){if(Hero==null||world==null)return;
      float x=Hero.transform.position.x;
      if(Mathf.Abs(x-8)<1.7f){
        var guide=world.root.GetComponentInChildren<CGCampusNPC>();
        if(guide!=null)guide.Speak();else{message=world.spec.dialogue;Hint(message);}return;}
      if(Mathf.Abs(x-35)<1.5f){world.checkpoint=true;world.checkpointX=35;CGAudio.Play("Checkpoint");Hint("Checkpoint activado. ¡Continúa!");return;}
      if(Mathf.Abs(x-40)<1.6f){page=Page.Quiz;Time.timeScale=0;selection=0;return;}
      if(Mathf.Abs(x-69)<2f){if(chips<3){Hint("Necesitas reunir los tres fragmentos de datos.");return;}
         if(!terminalSolved){Hint("Primero resuelve la terminal del nivel.");return;}
         if(!bossDead){Hint("Derrota al jefe antes de salir.");return;}
         CompleteLevel();return;}
      foreach(var npc in world.root.GetComponentsInChildren<CGCampusNPC>()){
        if(Mathf.Abs(x-npc.transform.position.x)<1.4f&&Mathf.Abs(Hero.transform.position.y-npc.transform.position.y)<1.5f){npc.Speak();return;}
      }
      Hint("Acércate a un NPC, terminal, checkpoint o portal para interactuar.");
    }
    void CompleteLevel(){CGStore.Data.completed[sceneLevel]=true;CGStore.Data.stars[sceneLevel]=3; if(sceneLevel<8)CGStore.Data.latestUnlocked=Mathf.Max(CGStore.Data.latestUnlocked,sceneLevel+1);
      if(sceneLevel==1)CGStore.Data.purpleUnlocked=true;if(sceneLevel==4)CGStore.Data.extraUnlocked=true;CGAudio.Play("Win");
      CGStore.Save();Hero.GetComponent<CGGuardianVisual>()?.Celebrate();page=sceneLevel==8?Page.Ending:Page.Victory;selection=0;
    }
    void AfterVictory(){int next=sceneLevel==9?0:sceneLevel+1;if(next==9){ToMain();return;}ChooseLevel(next);}
    void QuizAnswer(int selected){if(selected==world.spec.right){CGAudio.Play("Checkpoint");terminalSolved=true;Hint("¡Respuesta correcta! Terminal restaurada.");page=Page.Playing;Time.timeScale=1;resumedInputFrame=Time.frameCount;}else{CGAudio.Play("Denied");Hint("Esa opción no recupera los datos. Intenta otra vez.");}}
    void RectColor(Rect r,Color color){Color old=GUI.color;color.a*=old.a;GUI.color=color;CGUIDraw.Texture(r,Texture2D.whiteTexture);GUI.color=old;}
    void Outline(Rect r,Color color,int b=3){RectColor(new Rect(r.x,r.y,r.width,b),color);RectColor(new Rect(r.x,r.yMax-b,r.width,b),color);RectColor(new Rect(r.x,r.y,b,r.height),color);RectColor(new Rect(r.xMax-b,r.y,b,r.height),color);}
    void Panel(Rect r,Color? accent=null){RectColor(r,new Color(.022f,.035f,.075f,.94f));Outline(r,accent??new Color(.92f,.34f,.11f,.96f),2);}
    void Styles(){
      if(buttonStyle!=null)return;
      buttonStyle=new GUIStyle(GUIStyle.none){font=CGMenuTheme.Interface,fontSize=24,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleCenter,wordWrap=true};
      buttonStyle.normal.textColor=Color.white;buttonStyle.hover.textColor=new Color(1f,.75f,.34f);
      titleStyle=new GUIStyle(GUI.skin.label){font=CGMenuTheme.Title,fontSize=38,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleCenter,wordWrap=true};titleStyle.normal.textColor=new Color(1f,.52f,.10f);
      textStyle=new GUIStyle(GUI.skin.label){font=CGMenuTheme.Body,fontSize=22,alignment=TextAnchor.MiddleCenter,wordWrap=true};textStyle.normal.textColor=new Color(.96f,.97f,1f);
      tinyStyle=new GUIStyle(textStyle){fontSize=16};
    }
    void HeroPortrait(float x,float y,float w,float h,bool purple=false){
      Sprite sprite=!CGStore.Data.useOriginalWolf&&CGGuardianArt.Available?CGGuardianArt.Get(purple,0,(int)(CGMenuTheme.Clock*8)%8):CGArt.Player(purple,(int)(CGMenuTheme.Clock*5)%2,true);
      CGGuardianArt.DrawFit(new Rect(x,y,w,h),sprite);
    }
    bool Button(string label,int order,float x,float y,float w=520,float h=42){
      Rect rect=new Rect(x,y,w,h);bool active=selection==order;
      if(!editorMenuPreview&&pointerMoved&&rect.Contains(Event.current.mousePosition)){selection=order;active=true;}
      float focus=editorMenuPreview?(active?1:0):menuMotion.Focus(order);rect.x-=focus*4;rect.width+=focus*8;
      float pulse=CGStore.Data.reduceFlash ? .5f : .5f+.5f*Mathf.Sin(CGMenuTheme.Clock*5f);
      RectColor(rect,active?new Color(.18f+.13f*pulse,.073f+.04f*pulse,.06f,.99f):new Color(.025f,.045f,.085f,.96f));
      Outline(rect,active?new Color(1,.52f,.14f,1):new Color(.28f,.42f,.57f,.83f),active?3:2);
      if(active){RectColor(new Rect(x+7,y+8,5,h-16),new Color(1,.58f,.17f,1));RectColor(new Rect(x+w-11,y+8,5,h-16),new Color(1,.58f,.17f,1));}
      bool clicked=Click(rect,label,buttonStyle);if(clicked){selection=order;CGAudio.Menu(true);}return clicked;
    }
    bool Click(Rect rect,string text,GUIStyle style){if(editorMenuPreview){CGUIDraw.Text(rect,text,style);return false;}return GUI.Button(rect,text,style);}
    void Label(string t,float y,float height=43,bool head=false){CGUIDraw.Text(new Rect(190,y,900,height),t,head?titleStyle:textStyle);}
    void Header(string t){
      RectColor(new Rect(0,0,1280,98),new Color(.025f,.038f,.085f,.99f));
      RectColor(new Rect(0,94,1280,4),new Color(.98f,.39f,.08f,1));
      CGUIDraw.Text(new Rect(35,7,620,59),"CAMPUS GLITCH",new GUIStyle(titleStyle){fontSize=44});
      CGUIDraw.Text(new Rect(45,70,520,21),"UNIFRANZ LA PAZ  |  INNOVACIÓN  ·  APRENDE HACIENDO",tinyStyle);
      CGUIDraw.Text(new Rect(650,25,590,47),t,textStyle);
    }
    void OnGUI(){DrawInterface(Screen.width,Screen.height);}
#if UNITY_EDITOR
    public void DrawEditorMenu(){
      var originalPage=page;int originalSelection=selection;
      editorMenuPreview=true;Enum.TryParse(initialPage,true,out page);selection=0;
      try{DrawInterface(1280,720);}finally{page=originalPage;selection=originalSelection;editorMenuPreview=false;}
    }
#endif
    void DrawInterface(float viewportWidth,float viewportHeight){CGStore.Load();Styles();var old=GUI.matrix;var oldColor=GUI.color;
      if(!editorMenuPreview&&Event.current.isKey)Event.current.Use(); // Input System handles actions once.
      float desktopUnit=Mathf.Min(viewportWidth/1280f,viewportHeight/720f);
      Vector3 guiScale=new Vector3(desktopUnit,desktopUnit,1),guiOffset=new Vector3((viewportWidth-1280*desktopUnit)*.5f,(viewportHeight-720*desktopUnit)*.5f,0);
      if(CGTouch.Enabled){
        // Mobile: uniform scale centered on screen so wide phones (19.5:9, 20:9) do not stretch the UI.
        float u=CGTouch.Unit;guiScale=new Vector3(u,u,1);guiOffset=new Vector3((Screen.width-1280*u)*.5f,(Screen.height-720*u)*.5f,0);
        if(page==Page.Paused||page==Page.Quiz)RectColor(new Rect(0,0,Screen.width,Screen.height),new Color(.005f,.012f,.037f,.84f));
        else if(page!=Page.Playing)RectColor(new Rect(0,0,Screen.width,Screen.height),new Color(.02f,.045f,.08f,1));
      }
      GUI.matrix=Matrix4x4.TRS(guiOffset,Quaternion.identity,guiScale);
      if(page==Page.Playing){DrawHUD();if(Time.unscaledTime<toastUntil){float ty=CGTouch.Enabled?160:630;Panel(new Rect(280,ty,720,55),new Color(.96f,.5f,.15f,1));CGUIDraw.Text(new Rect(305,ty+4,670,43),toast,textStyle);}if(!CGStore.Data.reduceFlash)RectColor(new Rect(0,0,1280,720),new Color(.015f,.025f,.055f,1-Mathf.Clamp01((Time.unscaledTime-levelEntered)/.25f)));GUI.matrix=old;GUI.color=oldColor;CGTouch.Draw();return;}
      if((page==Page.Paused||page==Page.Quiz)&&!CGTouch.Enabled){RectColor(new Rect(0,0,1280,720),new Color(.005f,.012f,.037f,.84f));}
      else{
        var image=(page==Page.Map?Resources.Load<Texture2D>("CG/VisualV2/MapCampus"):null)??Resources.Load<Texture2D>("CG/VisualV2/MenuCampus")??CGCampusVisual.Background(0);
        float drift=CGStore.Data.reduceFlash?0:Mathf.Sin(CGMenuTheme.Clock*.16f)*5;
        if(image!=null)CGUIDraw.Texture(new Rect(-12+drift,-8,1304,736),image,ScaleMode.ScaleAndCrop);
        else RectColor(new Rect(0,0,1280,720),new Color(.02f,.045f,.08f,1));
        // Lightweight animated energy motes: entirely 2D UI, independent of camera.
        if(!CGStore.Data.reduceFlash){
          for(int k=0;k<25;k++){
            float y=140f+(k*53f)%515f;
            float x=Mathf.Repeat(k*163f+CGMenuTheme.Clock*(9f+(k%3)*5f),1280f);
            float a=.13f+.12f*(1f+Mathf.Sin(CGMenuTheme.Clock*2f+k));
            RectColor(new Rect(x,y,3+(k%3),3+(k%3)),new Color(k%2==0?1f:.67f,.23f,k%2==0?.23f:1f,a));
          }
        }
        RectColor(new Rect(0,676,1280,44),new Color(.02f,.03f,.055f,.95f));
        CGUIDraw.Text(new Rect(28,684,1235,28),page==Page.Map?"MOVER: DIRECCIONES / STICK   ·   A / ENTER: JUGAR   ·   Y / TAB: DETALLES   ·   LB / RB: NIVEL   ·   B / ESC: VOLVER":"MOVER: DIRECCIONES / STICK     ·     A / ENTER: ELEGIR     ·     B / ESC: VOLVER     ·     CAMPUS 2.5D",tinyStyle);
      }
      float entry=editorMenuPreview?1:menuMotion.Entry(CGStore.Data.reduceFlash);
      GUI.color=new Color(oldColor.r,oldColor.g,oldColor.b,oldColor.a*entry);
      GUI.matrix=Matrix4x4.TRS(guiOffset+new Vector3((1-entry)*16*guiScale.x,0,0),Quaternion.identity,guiScale);
      if(page==Page.Loading){
        Panel(new Rect(360,280,560,156));
        CGUIDraw.Text(new Rect(398,310,484,45),"RESTAURANDO CONEXIÓN...",textStyle);
        RectColor(new Rect(400,389,480,10),new Color(.12f,.18f,.28f));
        RectColor(new Rect(400,389,480*loadProgress,10),new Color(.89f,.45f,.12f));
      }
      if(page==Page.Splash){
        Header("EL CAMPUS TE NECESITA");
        HeroPortrait(195,232,270,385);
        Panel(new Rect(708,205,493,294));
        CGUIDraw.Text(new Rect(735,220,443,112),"LOBO\nGUARDIÁN",titleStyle);
        CGUIDraw.Text(new Rect(747,332,416,80),"Recupera el campus, supera los bugs y descubre sus laboratorios.",textStyle);
        Panel(new Rect(738,535,432,73),new Color(1,.61f,.13f,1));
        CGUIDraw.Text(new Rect(753,547,402,45),CGTouch.Enabled?"TOCA LA PANTALLA PARA JUGAR":"PULSA ENTER / A PARA JUGAR",textStyle);
      }
      if(page==Page.Main){
        Header("MENÚ PRINCIPAL");
        HeroPortrait(169,263,285,332,CGStore.Data.purpleEquipped);
        Panel(new Rect(706,137,516,536));
        for(int i=0;i<mainItems.Length;i++){string label=i==0?(CGStore.Data.latestUnlocked==0&&!CGStore.Data.completed[0]?"INICIAR AVENTURA":"CONTINUAR NIVEL "+(Mathf.Min(CGStore.Data.latestUnlocked,8)+1)):mainItems[i];if(Button(label,i,733,155+i*54,463,47))MainSelect(i);}
        Panel(new Rect(85,168,475,65));CGUIDraw.Text(new Rect(100,177,445,45),"GUARDIÁN DEL CAMPUS  ·  EXPLORA LA PAZ",new GUIStyle(textStyle){font=CGMenuTheme.Interface,fontSize=23});
      }
      if(page==Page.Map)DrawCampusMap();
      if(page==Page.Customization){Header("PERSONALIZACIÓN DEL LOBO");Panel(new Rect(190,205,900,60));Label("Lobo Guardián: armadura digital, identidad del campus y dos variantes.",215);
       HeroPortrait(70,278,230,290,false);HeroPortrait(980,278,230,290,true);
       if(Button("LOBO ROJO",0,355,310,570,58))EquipPurple(false);
       if(Button(CGStore.Data.purpleUnlocked?"LOBO MORADO - DESBLOQUEADO":"LOBO MORADO - COMPLETA NIVEL 2",1,355,380,570,58))EquipPurple(true);
       if(Button("MODELO: "+(CGStore.Data.useOriginalWolf?"CLÁSICO":"GUARDIÁN ACORAZADO"),2,355,452,570,58)){
         CGStore.Data.useOriginalWolf=!CGStore.Data.useOriginalWolf;CGStore.Save();}
       if(Button("VOLVER",3,355,520,570,55))ToMain();
       Panel(new Rect(238,590,804,52));Label("Equipado: "+(CGStore.Data.purpleEquipped?"MORADO":"ROJO")+"  |  "+(CGStore.Data.useOriginalWolf?"CLÁSICO":"GUARDIÁN ACORAZADO"),600);}
      if(page==Page.Settings){Header("AJUSTES");
       if(Button("VOLUMEN GENERAL: "+Mathf.RoundToInt(CGStore.Data.masterVolume*100)+" %",0,340,242,600,65))AdjustVolume(.1f);
       if(Button("REDUCIR DESTELLOS: "+(CGStore.Data.reduceFlash?"SÍ":"NO"),1,340,321,600,65)){CGStore.Data.reduceFlash=!CGStore.Data.reduceFlash;CGStore.Save();}
       if(Button("SUBTÍTULOS: "+(CGStore.Data.subtitles?"SÍ":"NO"),2,340,400,600,65)){CGStore.Data.subtitles=!CGStore.Data.subtitles;CGStore.Save();}
       if(Button("VOLVER",3,340,500,600,65))ToMain();}
      if(page==Page.Controls){Header("CONTROLES - TECLADO / MANDO");
       Panel(new Rect(390,165,500,43));if(Click(new Rect(400,171,480,32),padMode?"EDITANDO MANDO - CAMBIAR A TECLADO":"EDITANDO TECLADO - CAMBIAR A MANDO",buttonStyle)){padMode=!padMode;rebinding=-1;}
       for(int i=0;i<8;i++)if(Button(CGControl.Names[i]+":  "+(rebinding==i?"PULSA UN BOTÓN":padMode?CGStore.Data.padBindings[i]:CGStore.Data.actionKeys[i]),i,375,215+i*51,530,43))rebinding=i;
       CGUIDraw.Text(new Rect(70,645,1140,40),"TAB / LB: teclado o mando   ·   B / ESC: cancelar asignación   ·   START: pausa al jugar",tinyStyle);
       if(CGTouch.Enabled&&Click(new Rect(40,171,200,32),"< VOLVER",buttonStyle))ToMain();}
      if(page==Page.Help){Header("GUÍA DEL JUGADOR");Panel(new Rect(180,205,920,290));Label("Corre, salta, recupera 3 fragmentos y resuelve una terminal por nivel.",249);
       Label("E / Y: hablar con NPC, activar checkpoint, terminal y portal de salida.",306);
       Label("J / X: mordida; evita enemigos y derrota a los jefes de fase.",363);
       Label("La skin morada llega tras el NIVEL 2; Subsuelo extra tras el NIVEL 5.",424);
       if(Button("VOLVER",0,395,550,490,55))ToMain();}
      if(page==Page.Credits){Header("CRÉDITOS - DESARROLLO");Panel(new Rect(180,205,920,415));Label("VICTOR GROVER ASTURIZAGA PLATA",233);Label("Terrenos, assets, diseño de niveles y colisiones",265,32);
       Label("CARLA VALERIA ENCINAS CANO",321);Label("Animación del protagonista y sonidos",350,31);
       Label("VICENTE OSCAR CLAROS MAMANI",405);Label("Integración, menús, conexiones y acciones",435,31);
       Label("MARCELO JOSUE ESCOBAR CHIPANA",492);Label("Villanos, enemigos, jefes y apoyo visual",521,31);
       Label("Agradecimiento: Noemi Moller Chuquimia",573,29);
       if(Button("VOLVER",0,470,629,340,45))ToMain();}
      if(page==Page.Paused){Header("JUEGO EN PAUSA");if(Button("CONTINUAR",0,375,260,530,65))PauseChoice(0);
       if(Button("REINICIAR DESDE CHECKPOINT",1,375,342,530,65))PauseChoice(1);
       if(Button("SALIR AL MENÚ",2,375,425,530,65))PauseChoice(2);}
      if(page==Page.Quiz){Header("TERMINAL / MISIÓN DE APRENDIZAJE");Label(world.spec.quiz,230,75);
       string[] options={world.spec.option1,world.spec.option2,world.spec.option3};for(int i=0;i<3;i++)if(Button(options[i],i,340,333+i*80,600,67))QuizAnswer(i);
       Label("ESC / B para cerrar la terminal",635,30);}
      if(page==Page.Victory){Header("¡NIVEL RESTAURADO!");Label("Recuperaste los tres fragmentos y activaste la conexión.",265);
       if(sceneLevel==1)Label("¡PERSONALIZACIÓN MORADA DESBLOQUEADA!",352,57);
       if(Button("SIGUIENTE NIVEL",0,375,453,530,66))AfterVictory();if(Button("MENÚ PRINCIPAL",1,375,535,530,60))ToMain();}
      if(page==Page.Ending){Header("¡CAMPUS RESTAURADO!");Label("CORE GLITCH HA SIDO DERROTADO",260);Label("INNOVACIÓN · APRENDE HACIENDO",349,48);
       Label("UNIFRANZ LA PAZ — FIN DE LA AVENTURA",415);
       if(Button("VER CRÉDITOS",0,390,501,500,65)){ShowCreditsOnBoot=true;StartCoroutine(TransitionToScene("SC_Bootstrap"));}
       if(Button("VOLVER AL MENÚ",1,390,583,500,55))ToMain();}
      if(page==Page.ConfirmReset){Header("¿BORRAR PROGRESO?");Label("Se eliminarán los niveles completados y las skins desbloqueadas.",311);
       if(Button("SÍ, BORRAR",0,320,431,300,58)){CGStore.Reset();ToMain();}if(Button("CANCELAR",1,660,431,300,58))ToMain();}
      if(Time.unscaledTime<toastUntil && page!=Page.Playing){CGUIDraw.Text(new Rect(180,628,920,35),toast,tinyStyle);}
      GUI.matrix=old;
      GUI.color=oldColor;
    }
    void DrawHUD(){
      if(world==null||Hero==null)return;
      // Touch controls reserve the original x=690..820 gap for the pause button.
      if(CGGuardianArt.Available&&!CGTouch.Enabled){CGCampusHUD.Draw(world,Hero,chips,taken,terminalSolved,bossDead);return;}
      Rect r=new Rect(12,12,625,72);Panel(r,new Color(.97f,.43f,.12f,1));
      CGUIDraw.Text(new Rect(24,18,595,24),world.spec.name,tinyStyle);
      CGUIDraw.Text(new Rect(24,44,600,27),"VIDAS "+Mathf.Max(0,Hero.hearts)+"/3   ·   DATOS "+chips+"/3   ·   "+(terminalSolved?"TERMINAL OK":"TERMINAL PENDIENTE"),tinyStyle);
      Rect rr=new Rect(866,12,403,50);Panel(rr,new Color(.34f,.67f,1f,1));
      CGUIDraw.Text(new Rect(876,19,383,33),"E / Y  INTERACTUAR   ·   ESC / START  PAUSA",tinyStyle);
      if(world.boss!=null&&!bossDead){Panel(new Rect(390,97,500,52),new Color(1,.21f,.25f,1));CGUIDraw.Text(new Rect(404,106,474,36),"JEFE: "+world.boss.kind+"   |   VIDA: "+world.boss.hp,textStyle);}
    }
}
}
