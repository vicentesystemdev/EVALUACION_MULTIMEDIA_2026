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
    enum Page{Splash,Main,Map,Customization,Settings,Controls,Help,Credits,Playing,Paused,Quiz,Victory,Ending,ConfirmReset,Loading,GameOver,Intro}
    Page page=Page.Splash;
    CGWorld world;int selection=0,mapSelection=0,chips=0;bool[] taken=new bool[3];bool terminalSolved=false,bossDead=true;string toast="",message="";float toastUntil=0;bool toastCritical;
    int rebinding=-1;bool padMode=false;float nextQuizAnswerTime=0;
    int gameOverEnteredFrame=-1;
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
    readonly string[] mainItems={"JUGAR / CONTINUAR","MAPA DEL CAMPUS","PERSONALIZACIÓN","CONFIGURACIÓN","AYUDA Y CONTROLES","CRÉDITOS","SALIR"};
    void Start(){
      // Editor previews exist so scenes are visible in 2D before pressing Play.
      // They are decorative only: runtime generates the playable scene with physics.
      foreach(var sceneRoot in gameObject.scene.GetRootGameObjects())if(sceneRoot.name=="CG_EDITOR_PREVIEW_2D")Destroy(sceneRoot);
      CGStore.Load();if(Camera.main==null){GameObject obj=new GameObject("Camera_UI");obj.tag="MainCamera";Camera camera=obj.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5.7f;if(FindFirstObjectByType<AudioListener>()==null)obj.AddComponent<AudioListener>();}
      CGCamera2DRuntime.PrepareForPlay();
      if(GetComponent<CGAudio>()==null)gameObject.AddComponent<CGAudio>();CGSettingsRuntime.ApplySaved();
      if(sceneLevel>=0){CreateLevel();}else{
        if(ShowCreditsOnBoot)page=Page.Credits;
        else if(!Enum.TryParse<Page>(initialPage,true,out page))page=Page.Splash;
        ShowCreditsOnBoot=false;
        if(page==Page.Controls)BeginControls();
        if(page==Page.Credits)BeginCredits();
        if(page==Page.Settings)BeginSettings();
        if(page==Page.Help)BeginHelp();
      }
    }
    void CreateLevel(){page=Page.Playing;chips=0;taken=new bool[3];terminalSolved=false;bossDead=string.IsNullOrEmpty(CGLevels.All[sceneLevel].boss);
      GameObject root=new GameObject("LEVEL_"+(sceneLevel+1));world=root.AddComponent<CGWorld>();world.Setup(this,sceneLevel);levelEntered=Time.unscaledTime;Hint(CGLevels.All[sceneLevel].objective);
      if(IntroPending())BeginIntro();
    }
    void LateUpdate(){
      if(lastMenuPage!=(int)page){menuNavigation.Reset();lastMenuPage=(int)page;lastMenuSelection=selection;}
      else if(selection!=lastMenuSelection&&page!=Page.Playing&&page!=Page.Loading){CGAudio.Menu();lastMenuSelection=selection;focusChangedAt=Time.unscaledTime;}
      menuMotion.Tick((int)page,selection,CGStore.Data!=null&&CGSettingsRuntime.ReduceFlash);
    }
    void Update(){CGTouch.UpdateInput();pointerMoved=Mouse.current!=null&&Mouse.current.delta.ReadValue().sqrMagnitude>1;
      if(page==Page.Intro){HandleIntro();return;}
      if(page==Page.Controls){HandleControls();return;}
      if(page==Page.Settings){CGSettingsWorldView.Tick();HandleSettings();return;}
      if(page==Page.Help){HandleHelp();return;}
      if(page==Page.Loading){if(loadingLevel&&(CGControl.Confirm()||CGTouch.AnyTap()))skipAccessAnimation=true;return;}
      if(page==Page.Map&&HandleMapShortcuts())return;
      if(page==Page.Customization){HandleCustomization();return;}
      if(page==Page.Credits){HandleCredits();return;}
      if(page==Page.GameOver){
        if(Time.frameCount<=gameOverEnteredFrame)return;
        selection=menuNavigation.Read(selection,2,false);selection=(selection%2+2)%2;
        if(CGControl.Back()){GameOverChoice(1);return;}
        if(CGControl.Confirm()){CGAudio.Menu(true);GameOverChoice(selection);}
        return;
      }
      if(page==Page.Playing){if(CGControl.Pause()){page=Page.Paused;Time.timeScale=0;selection=0;}return;}
      if(page==Page.Splash){if(CGControl.Confirm()||CGTouch.AnyTap()){page=Page.Main;selection=0;}return;}
      if(page==Page.Quiz){
        if(CGControl.Back()){CloseQuiz();return;}
        selection=menuNavigation.Read(selection,4,false);selection=(selection%4+4)%4;
        if(CGControl.Confirm()){CGAudio.Menu(true);if(selection==3)CloseQuiz();else QuizAnswer(selection);}
        return;
      }
      int n=page==Page.Main?mainItems.Length:page==Page.Controls?8:page==Page.Paused?3:page==Page.Settings?6:page==Page.Customization?4:page==Page.Map?10:page==Page.Victory||page==Page.Ending||page==Page.ConfirmReset||page==Page.Help?2:1;
      selection=menuNavigation.Read(selection,n,page==Page.Map);
      selection=(selection%n+n)%n;
      if(page==Page.Map){mapSelection=selection;}
      if(CGControl.Confirm()){
       CGAudio.Menu(true);
       switch(page){case Page.Main:MainSelect(selection);break;case Page.Map:ChooseLevel(mapSelection);break;case Page.Paused:PauseChoice(selection);break;
         case Page.Victory:HandleVictorySelection(selection,AfterVictory,ToMain);break;
         case Page.Ending:HandleEndingSelection(selection,OpenEndingCredits,ToMain);break;
         case Page.ConfirmReset:ConfirmResetChoice(selection);break;

         case Page.Help:if(selection==0)OpenUIScreen("SC_Controls",Page.Controls);else ToMain();break;
         case Page.Controls:rebinding=selection;break;
         default:ToMain();break;
       }
      }
      if(CGControl.Back()){if(page==Page.Paused){page=Page.Playing;Time.timeScale=1;}else if(page!=Page.Main)ToMain();}
    }
    public void Hint(string msg,bool always=false){toast=msg;toastCritical=always;toastUntil=Time.unscaledTime+4f;}
    void MainSelect(int i){switch(i){case 0: ChooseLevel(Mathf.Min(CGStore.Data.latestUnlocked,8));break;case 1:OpenUIScreen("SC_LevelMap",Page.Map);break;
      case 2:OpenUIScreen("SC_Customization",Page.Customization);break;case 3:OpenUIScreen("SC_Settings",Page.Settings);break;case 4:OpenUIScreen("SC_Help",Page.Help);break;
      case 5:OpenUIScreen("SC_Credits",Page.Credits);break;case 6:Application.Quit();break;}}
    // Shared decision for keyboard, gamepad and pointer input (Carla).
    public static void HandleResetSelection(int selected,Action resetProgress,Action returnToMenu){
      if(selected==0)resetProgress();
      returnToMenu();
    }
    void ConfirmResetChoice(int selected){HandleResetSelection(selected,CGStore.Reset,ToMain);}
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
      Time.timeScale=1;string name=CGLevels.Scene(i);levelEnteredFromMenu=true;
      if(Application.CanStreamedLevelBeLoaded(name))StartCoroutine(TransitionToScene(name,true));
      else{sceneLevel=i;CreateLevel();}}
    IEnumerator TransitionToScene(string next,bool levelEntry=false){
      page=Page.Loading;loadProgress=0;loadingLevel=levelEntry;skipAccessAnimation=false;accessAnimationStarted=Time.realtimeSinceStartup;
      loadingCaption=next=="SC_MainMenu"?"VOLVIENDO AL MENÚ":next=="SC_Settings"?"ABRIENDO CONFIGURACIÓN":next=="SC_LevelMap"?"ABRIENDO MAPA DEL CAMPUS":next=="SC_Customization"?"ABRIENDO PERSONALIZACIÓN":next=="SC_Help"?"ABRIENDO AYUDA":next=="SC_Credits"?"ABRIENDO CRÉDITOS":"ABRIENDO CONTROLES";
      float minimum=ShowCampusAccess?(CGMenuTheme.Design!=null?CGMenuTheme.Design.entranceDuration:1.25f):(CGMenuTheme.Design!=null?CGMenuTheme.Design.screenTransition:.22f);
      var operation=SceneManager.LoadSceneAsync(next);operation.allowSceneActivation=false;
      while(operation.progress<.9f||!skipAccessAnimation&&Time.realtimeSinceStartup-accessAnimationStarted<minimum){loadProgress=Mathf.Clamp01(operation.progress/.9f);yield return null;}
      loadProgress=1;operation.allowSceneActivation=true;
    }
    void PauseChoice(int i){if(i==0){page=Page.Playing;Time.timeScale=1;resumedInputFrame=Time.frameCount;}else if(i==1){page=Page.Playing;Time.timeScale=1;resumedInputFrame=Time.frameCount;Respawn();}else ToMain();}
    public static float VolumeAfterStep(float value,float delta){return Mathf.Clamp01(Mathf.Round((value+delta)*10f)/10f);}
    void EquipPurple(bool on){if(on&&!CGStore.Data.purpleUnlocked){Hint("Morado: se desbloquea al terminar el nivel 2");return;}CGStore.Data.purpleEquipped=on;CGStore.Save();Hint(on?"Lobo morado equipado":"Lobo clásico equipado");}

    public void TakeChip(int id){if(!Playing||id<0||id>=3||taken[id])return;taken[id]=true;chips++;CGAudio.Play("Pickup");Hint("Fragmento recuperado: "+chips+"/3");}
    public void Attack(Vector3 pos){AttackPulse(pos,.82f,1);}
    readonly System.Collections.Generic.List<Collider2D> attackTargets=new System.Collections.Generic.List<Collider2D>();
    readonly System.Collections.Generic.List<RaycastHit2D> attackObstacles=new System.Collections.Generic.List<RaycastHit2D>();
    readonly System.Collections.Generic.HashSet<CGEnemy> attacked=new System.Collections.Generic.HashSet<CGEnemy>();
    public void AttackPulse(Vector3 pos,float radius,int damage){if(!Playing)return;
      var physics=gameObject.scene.GetPhysicsScene2D();var filter=ContactFilter2D.noFilter;attackTargets.Clear();attacked.Clear();physics.OverlapCircle(pos,radius,filter,attackTargets);
      Vector2 origin=Hero!=null?(Hero.Body!=null?Hero.Body.position:(Vector2)Hero.transform.position)+Vector2.up*.8f:(Vector2)pos;
      foreach(var c in attackTargets){
        var e=c.GetComponentInParent<CGEnemy>();if(e==null||!attacked.Add(e))continue;
        if(Hero!=null&&(e.transform.position.x-origin.x)*(Hero.facesRight?1:-1)<-.05f)continue;
        attackObstacles.Clear();physics.Linecast(origin,e.transform.position,filter,attackObstacles);bool blocked=false;
        foreach(var h in attackObstacles)if(h.collider!=null&&!h.collider.isTrigger&&h.collider.GetComponent<CGSurface>()!=null){blocked=true;break;}
        if(!blocked)e.Hurt(damage);
      }
    }
    public void BossDown(){if(!Playing)return;bossDead=true;Hint("¡Jefe derrotado! El portal está disponible.");}
    public void KillHero(){
      if(!Playing||Hero==null)return;
      Hero.hearts=0;Hero.StopActions();page=Page.GameOver;selection=0;
      gameOverEnteredFrame=Time.frameCount;rebinding=-1;toast="";toastUntil=0;
      Time.timeScale=0;CGAudio.Play("Damage");
    }
    public void HandleHeroFall(){
      if(!Playing||Hero==null)return;
      if(Hero.DamageFromFall()&&Playing){Respawn();Hint("Perdiste un corazón. Regresas al checkpoint.");}
    }
    void Respawn(bool restoreHearts=false){
      if(Hero==null||world==null)return;
      Hero.RespawnAt(new Vector3(world.checkpointX+0.5f,.45f,0),restoreHearts);
    }
    void GameOverChoice(int choice){
      if(page!=Page.GameOver||Time.frameCount<=gameOverEnteredFrame)return;
      if(choice==0){
        Respawn(true);page=Page.Playing;Time.timeScale=1;selection=0;resumedInputFrame=Time.frameCount;
        Hint("Nueva oportunidad. Continúas desde el checkpoint.");
      }else if(choice==1)ToMain();
    }
    public void InteractNearPlayer(){if(!Playing||Hero==null||world==null)return;
      float x=Hero.transform.position.x;
      if(Mathf.Abs(x-8)<1.7f){
        var guide=world.root.GetComponentInChildren<CGCampusNPC>();
        if(guide!=null)guide.Speak();else{message=world.spec.dialogue;Hint(message,true);}return;}
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
    void CompleteLevel(){if(!Playing)return;toast="";toastUntil=0;CGStore.Data.completed[sceneLevel]=true;CGStore.Data.stars[sceneLevel]=3; if(sceneLevel<8)CGStore.Data.latestUnlocked=Mathf.Max(CGStore.Data.latestUnlocked,sceneLevel+1);
      if(sceneLevel==1)CGStore.Data.purpleUnlocked=true;if(sceneLevel==4)CGStore.Data.extraUnlocked=true;CGAudio.Play("Win");
      CGStore.Save();Hero.GetComponent<CGGuardianVisual>()?.Celebrate();page=sceneLevel==8?Page.Ending:Page.Victory;selection=0;
    }
    public static void HandleVictorySelection(int selected,Action nextLevel,Action menu){if(selected==0)nextLevel();else menu();}
    public static void HandleEndingSelection(int selected,Action credits,Action menu){if(selected==0)credits();else menu();}
    void OpenEndingCredits(){OpenUIScreen("SC_Credits",Page.Credits);}
    void AfterVictory(){int next=sceneLevel==9?0:sceneLevel+1;if(next==9){ToMain();return;}ChooseLevel(next);}
    public static int MoveQuizSelection(int current,int direction){return ((current+direction)%4+4)%4;}
    void CloseQuiz(){page=Page.Playing;Time.timeScale=1;resumedInputFrame=Time.frameCount;}
    void QuizAnswer(int selected){if(page!=Page.Quiz||selected<0||selected>2||Time.unscaledTime<nextQuizAnswerTime)return;nextQuizAnswerTime=Time.unscaledTime+.2f;if(selected==world.spec.right){CGAudio.Play("Checkpoint");terminalSolved=true;Hint("¡Respuesta correcta! Terminal restaurada.");CloseQuiz();}else{CGAudio.Play("Denied");Hint("Esa opción no recupera los datos. Intenta otra vez.");}}
    void RectColor(Rect r,Color color){Color old=CGUIDraw.Color;color.a*=old.a;CGUIDraw.Color=color;CGUIDraw.Texture(r,Texture2D.whiteTexture);CGUIDraw.Color=old;}
    void Outline(Rect r,Color color,int b=3){RectColor(new Rect(r.x,r.y,r.width,b),color);RectColor(new Rect(r.x,r.yMax-b,r.width,b),color);RectColor(new Rect(r.x,r.y,b,r.height),color);RectColor(new Rect(r.xMax-b,r.y,b,r.height),color);}
    void Panel(Rect r,Color? accent=null){var design=CGMenuTheme.Design;RectColor(r,design!=null?design.ink:new Color(.022f,.035f,.075f,.94f));Outline(r,accent??(design!=null?design.amber:new Color(.92f,.34f,.11f,.96f)),2);}
    void Styles(){
      var design=CGMenuTheme.Design;
      int buttonSize=design!=null?design.buttonSize:28,titleSize=design!=null?design.titleSize:42,bodySize=design!=null?design.bodySize:22;
      if(buttonStyle!=null&&buttonStyle.font==CGMenuTheme.Interface&&buttonStyle.fontSize==buttonSize&&titleStyle!=null&&titleStyle.font==CGMenuTheme.Title&&titleStyle.fontSize==titleSize&&textStyle!=null&&textStyle.font==CGMenuTheme.Body&&textStyle.fontSize==bodySize&&tinyStyle!=null&&tinyStyle.fontSize>0){titleStyle.normal.textColor=design!=null?design.amber:new Color(1f,.52f,.10f);return;}
      buttonStyle=new GUIStyle(GUIStyle.none){font=CGMenuTheme.Interface,fontSize=buttonSize,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleCenter,wordWrap=true};
      buttonStyle.normal.textColor=Color.white;buttonStyle.hover.textColor=new Color(1f,.75f,.34f);
      titleStyle=new GUIStyle(GUIStyle.none){font=CGMenuTheme.Title,fontSize=titleSize,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleCenter,wordWrap=true};titleStyle.normal.textColor=design!=null?design.amber:new Color(1f,.52f,.10f);
      textStyle=new GUIStyle(GUIStyle.none){font=CGMenuTheme.Body,fontSize=bodySize,alignment=TextAnchor.MiddleCenter,wordWrap=true};textStyle.normal.textColor=new Color(.96f,.97f,1f);
      tinyStyle=new GUIStyle(textStyle){fontSize=16};
    }
    void HeroPortrait(float x,float y,float w,float h,bool purple=false){
      if(!CGStore.Data.useOriginalWolf&&CGGuardianMenuArt.Draw(new Rect(x,y,w,h),purple))return;
      Sprite sprite=!CGStore.Data.useOriginalWolf&&CGGuardianArt.Available?CGGuardianArt.Get(purple,0,(int)(CGMenuTheme.Clock*8)%8):CGArt.Player(purple,(int)(CGMenuTheme.Clock*5)%2,true);
      CGGuardianArt.DrawFit(new Rect(x,y,w,h),sprite);
    }
    bool Button(string label,int order,float x,float y,float w=520,float h=42){
      Rect rect=new Rect(x,y,w,h);bool active=selection==order;
      if(!editorMenuPreview&&pointerMoved&&rect.Contains(Event.current.mousePosition)){selection=order;active=true;}
      float focus=editorMenuPreview?(active?1:0):menuMotion.Focus(order);rect.x-=focus*4;rect.width+=focus*8;
      float pulse=CGSettingsRuntime.ReduceFlash ? .5f : .5f+.5f*Mathf.Sin(CGMenuTheme.Clock*5f);
      RectColor(rect,active?new Color(.18f+.13f*pulse,.073f+.04f*pulse,.06f,.99f):new Color(.025f,.045f,.085f,.96f));
      Color accent=CGMenuTheme.Design!=null?CGMenuTheme.Design.amber:new Color(1,.52f,.14f,1);
      Outline(rect,active?accent:new Color(.28f,.42f,.57f,.83f),active?3:2);
      if(active&&!CGSettingsRuntime.ReduceFlash)RectColor(new Rect(rect.x+2,rect.yMax-4,rect.width*(.35f+.15f*pulse),2),new Color(1,.79f,.4f,.9f));
      if(active){RectColor(new Rect(x+7,y+8,5,h-16),new Color(1,.58f,.17f,1));RectColor(new Rect(x+w-11,y+8,5,h-16),new Color(1,.58f,.17f,1));}
      bool clicked=Click(rect,label,buttonStyle);if(clicked){selection=order;CGAudio.Menu(true);}return clicked;
    }
    bool MainButton(string label,int order,Rect rect){
      if(!editorMenuPreview&&pointerMoved&&rect.Contains(Event.current.mousePosition))selection=order;
      bool active=selection==order;float focus=editorMenuPreview?(active?1:0):menuMotion.Focus(order);
      rect.x-=focus*6;rect.width+=focus*6;
      var design=CGMenuTheme.Design;Color accent=design!=null?design.amber:new Color(1,.56f,.12f);
      if(active){var tint=CGUIDraw.Color;CGUIDraw.Color=new Color(accent.r,accent.g,accent.b,tint.a*.16f);CGUIDraw.Texture(new Rect(rect.x-5,rect.y-4,rect.width+10,rect.height+8),CGMenuArtwork.Button(true,accent));CGUIDraw.Color=tint;}
      var previousTint=CGUIDraw.Color;CGUIDraw.Color=new Color(previousTint.r,previousTint.g,previousTint.b,previousTint.a*(active?1:design!=null?design.buttonOpacity:.78f));
      CGUIDraw.Texture(rect,CGMenuArtwork.Button(active,accent));CGUIDraw.Color=previousTint;
      if(active)DrawGlitchFocus(rect,accent);
      bool icon=design==null||design.showButtonIcons;
      if(icon){var texture=design!=null&&design.mainIcons!=null&&order<design.mainIcons.Length?design.mainIcons[order]:null;CGUIDraw.Texture(new Rect(rect.x+18,rect.center.y-18,36,36),texture!=null?texture:CGMenuArtwork.Icon(order),ScaleMode.ScaleToFit);}
      var style=new GUIStyle(buttonStyle){alignment=TextAnchor.MiddleLeft};
      CGUIDraw.Text(new Rect(rect.x+(icon?72:24),rect.y+2,rect.width-(icon?104:56),rect.height-4),label,style);
      if(Click(rect,"",GUIStyle.none)){selection=order;CGAudio.Menu(true);return true;}return false;
    }
    bool Click(Rect rect,string text,GUIStyle style){if(editorMenuPreview){CGUIDraw.Text(rect,text,style);return false;}return GUI.Button(rect,text,style);}
    void Label(string t,float y,float height=43,bool head=false){CGUIDraw.Text(new Rect(190,y,900,height),t,head?titleStyle:textStyle);}
    void Header(string t){
      RectColor(new Rect(0,0,1280,98),new Color(.025f,.038f,.085f,.99f));
      RectColor(new Rect(0,94,1280,4),new Color(.98f,.39f,.08f,1));
      var design=CGMenuTheme.Design;var titleRect=design!=null?design.titleRect:new Rect(35,7,620,59);string title=design!=null?design.title:"CAMPUS GLITCH";
      var shadow=titleRect;shadow.position+=new Vector2(2,3);var shadowStyle=new GUIStyle(titleStyle);shadowStyle.normal.textColor=new Color(.1f,.025f,.01f);CGUIDraw.Text(shadow,title,shadowStyle);CGUIDraw.Text(titleRect,title,titleStyle);
      CGUIDraw.Text(new Rect(45,70,560,21),design!=null?design.campus:"UNIFRANZ LA PAZ · INNOVACIÓN · APRENDE HACIENDO",tinyStyle);
      CGUIDraw.Text(new Rect(650,25,590,47),t,textStyle);
    }
    void MainHeader(){
      var design=CGMenuTheme.Design;var tint=CGUIDraw.Color;
      CGUIDraw.Color=new Color(1,1,1,tint.a*(design!=null?design.headerShade:.45f));CGUIDraw.Texture(new Rect(0,0,1280,135),CGMenuArtwork.Shade);CGUIDraw.Color=tint;
      var rect=design!=null?design.titleRect:new Rect(35,7,620,59);string title=design!=null?design.title:"CAMPUS GLITCH";
      var shadow=rect;shadow.position+=new Vector2(2,3);var style=new GUIStyle(titleStyle);style.normal.textColor=new Color(.02f,.025f,.045f,.95f);CGUIDraw.Text(shadow,title,style);CGUIDraw.Text(rect,title,titleStyle);
      string campus=design!=null?design.campus:"UNIFRANZ LA PAZ · INNOVACIÓN · APRENDE HACIENDO";var subtitle=new Rect(rect.x+16,rect.yMax+3,rect.width-32,29);
      RectColor(new Rect(rect.x+5,rect.yMax,rect.width-10,35),new Color(.012f,.02f,.045f,.88f));
      RectColor(new Rect(rect.x+5,rect.yMax,3,35),design!=null?design.amber:new Color(1,.56f,.12f));
      CGUIDraw.Text(subtitle,campus,new GUIStyle(textStyle){font=CGMenuTheme.Interface,fontSize=21,alignment=TextAnchor.MiddleCenter});
    }
    void OnGUI(){DrawInterface(Screen.width,Screen.height);}
#if UNITY_EDITOR
    public void DrawEditorMenu(bool entrance=false){
      var originalPage=page;int originalSelection=selection;float originalProgress=loadProgress,originalAccess=accessAnimationStarted;bool originalLevel=loadingLevel;
      editorMenuPreview=true;Enum.TryParse(initialPage,true,out page);selection=page==Page.Map?mapSelection:0;
      if(entrance){page=Page.Loading;loadingLevel=true;loadProgress=Mathf.PingPong(CGMenuTheme.Clock*.22f,1);accessAnimationStarted=CGMenuTheme.Clock-loadProgress*(CGMenuTheme.Design!=null?CGMenuTheme.Design.entranceDuration:1.25f);}
      try{DrawInterface(1280,720);}finally{page=originalPage;selection=originalSelection;loadProgress=originalProgress;loadingLevel=originalLevel;accessAnimationStarted=originalAccess;editorMenuPreview=false;}
    }
#endif
    void DrawInterface(float viewportWidth,float viewportHeight){CGStore.Load();Styles();var old=CGUIDraw.Matrix;var oldColor=CGUIDraw.Color;
      if(!editorMenuPreview&&Event.current.isKey)Event.current.Use(); // Input System handles actions once.
      Rect content=CGMenuViewport.Content(viewportWidth,viewportHeight,editorMenuPreview?new Rect(0,0,viewportWidth,viewportHeight):Screen.safeArea);
      float desktopUnit=content.width/1280f;
      Vector3 guiScale=new Vector3(desktopUnit,desktopUnit,1),guiOffset=new Vector3(content.x,content.y,0);
      if(page!=Page.Playing)RectColor(new Rect(0,0,viewportWidth,viewportHeight),page==Page.Paused||page==Page.Quiz||page==Page.GameOver?new Color(.005f,.012f,.037f,.84f):new Color(.012f,.02f,.045f,1));
      CGUIDraw.Matrix=Matrix4x4.TRS(guiOffset,Quaternion.identity,guiScale);
      if(page==Page.Playing){DrawSettingsBrightness(new Rect(0,0,1280,720),CGSettingsRuntime.Options.brightness);DrawHUD();if(Time.unscaledTime<toastUntil&&(CGStore.Data.subtitles||toastCritical)){float ty=CGTouch.Enabled?160:630,tx=CGTouch.Enabled?280:350,tw=CGTouch.Enabled?720:630;Panel(new Rect(tx,ty,tw,55),new Color(.96f,.5f,.15f,1));CGUIDraw.Text(new Rect(tx+25,ty+4,tw-50,43),toast,new GUIStyle(textStyle){fontSize=new[]{20,23,26}[CGSettingsRuntime.Options.hintSize]});}if(!CGSettingsRuntime.ReduceFlash)RectColor(new Rect(0,0,1280,720),new Color(.015f,.025f,.055f,1-Mathf.Clamp01((Time.unscaledTime-levelEntered)/.25f)));CGUIDraw.Matrix=old;CGUIDraw.Color=oldColor;CGTouch.Draw();return;}
      if(page==Page.Paused||page==Page.Quiz||page==Page.GameOver){if(!CGTouch.Enabled)RectColor(new Rect(0,0,1280,720),new Color(.005f,.012f,.037f,.84f));}
      else{
        var design=CGMenuTheme.Design;
        var image=(page==Page.Map?(design!=null?design.mapBackground:null):design!=null?design.menuBackground:null)??(page==Page.Map?Resources.Load<Texture2D>("CG/VisualV2/MapCampus"):null)??Resources.Load<Texture2D>("CG/VisualV2/MenuCampus")??CGCampusVisual.Background(0);
        if(page==Page.Credits&&CreditsDesign!=null&&CreditsDesign.background!=null)image=CreditsDesign.background;
        if(page==Page.Settings&&SettingsDesign!=null&&SettingsDesign.background!=null)image=SettingsDesign.background;
        if(page==Page.Help&&HelpDesign!=null&&HelpDesign.background!=null)image=HelpDesign.background;
        float drift=CGSettingsRuntime.ReduceFlash?0:Mathf.Sin(CGMenuTheme.Clock*.16f)*(design!=null?design.backgroundDrift:5);
        if(image!=null)CGUIDraw.Texture(new Rect(-12+drift,-8,1304,736),image,ScaleMode.ScaleAndCrop);
        else RectColor(new Rect(0,0,1280,720),new Color(.02f,.045f,.08f,1));
        DrawSettingsBrightness(new Rect(0,0,1280,720),CGSettingsRuntime.Options.brightness);
        // Lightweight animated energy motes: entirely 2D UI, independent of camera.
        if(!CGSettingsRuntime.ReduceFlash){
          for(int k=0;k<(page==Page.Main?6:25);k++){
            float y=140f+(k*53f)%515f;
            float x=Mathf.Repeat(k*163f+CGMenuTheme.Clock*(9f+(k%3)*5f),1280f);
            float a=.13f+.12f*(1f+Mathf.Sin(CGMenuTheme.Clock*2f+k));
            RectColor(new Rect(x,y,3+(k%3),3+(k%3)),new Color(k%2==0?1f:.67f,.23f,k%2==0?.23f:1f,a));
          }
        }
        if(page!=Page.Customization&&page!=Page.Controls&&page!=Page.Credits&&page!=Page.Settings&&page!=Page.Help&&page!=Page.Intro){RectColor(new Rect(0,676,1280,44),new Color(.02f,.03f,.055f,.95f));
        CGUIDraw.Text(new Rect(28,684,1235,28),page==Page.Map?"MOVER: DIRECCIONES / STICK   ·   A / ENTER: JUGAR   ·   Y / TAB: DETALLES   ·   LB / RB: NIVEL   ·   B / ESC: VOLVER":"MOVER: DIRECCIONES / STICK     ·     A / ENTER: ELEGIR     ·     B / ESC: VOLVER     ·     CAMPUS 2.5D",tinyStyle);}
      }
      float entry=editorMenuPreview?1:menuMotion.Entry(CGSettingsRuntime.ReduceFlash);
      CGUIDraw.Color=new Color(oldColor.r,oldColor.g,oldColor.b,oldColor.a*entry);
      CGUIDraw.Matrix=Matrix4x4.TRS(guiOffset+new Vector3((1-entry)*(CGMenuTheme.Design!=null?CGMenuTheme.Design.screenSlide:16)*guiScale.x,0,0),Quaternion.identity,guiScale);
      if(page==Page.Loading){
        if(ShowCampusAccess)DrawCampusAccess(1,"INGRESANDO A LA UNIVERSIDAD",AccessProgress,true);
        else DrawMenuLoading();
      }
      if(page==Page.Splash){
        Header("EL CAMPUS TE NECESITA");
        HeroPortrait(195,232,270,385);
        Panel(new Rect(708,205,493,294));
        CGUIDraw.Text(new Rect(735,220,443,112),"LOBO\nGUARDIÁN",titleStyle);
        CGUIDraw.Text(new Rect(747,332,416,80),"Recupera el campus, supera los bugs y descubre sus laboratorios.",textStyle);
        Panel(new Rect(738,535,432,73),new Color(1,.61f,.13f,1));
        if(Click(new Rect(753,547,402,45),CGTouch.Enabled?"TOCA LA PANTALLA PARA JUGAR":"PULSA ENTER / A PARA JUGAR",textStyle)){page=Page.Main;selection=0;}
      }
      if(page==Page.Main){
        MainHeader();
        var design=CGMenuTheme.Design;var hero=design!=null?design.heroRect:new Rect(138,220,360,410);
        CGUIDraw.Texture(new Rect(hero.center.x-hero.width*.34f,hero.yMax-9,hero.width*.68f,21),CGMenuArtwork.Shadow);
        DrawMainGuardian(hero);
        RectColor(design!=null?design.mainPanel:new Rect(720,133,514,522),new Color(.018f,.025f,.06f,design!=null?design.mainPanelOpacity:.06f));
        RectColor(new Rect(805,106,475,570),new Color(.015f,.025f,.055f,design!=null?design.rightShade:.16f));
        DrawGlitchHighlights();
        RectColor(new Rect(853,32,381,82),new Color(.012f,.02f,.045f,.82f));
        CGUIDraw.Text(new Rect(867,39,353,36),"EQUIPADO · GUARDIÁN "+(CGStore.Data.purpleEquipped?"MORADO":"ROJO"),new GUIStyle(textStyle){font=CGMenuTheme.Interface,fontSize=22});
        Vector2 start=design!=null?design.firstButton:new Vector2(745,150),size=design!=null?design.buttonSizePixels:new Vector2(463,60);float spacing=design!=null?design.buttonSpacing:70;
        for(int i=0;i<mainItems.Length;i++){string label=design!=null&&design.mainLabels!=null&&i<design.mainLabels.Length?design.mainLabels[i]:mainItems[i];if(i==0){bool first=CGStore.Data.latestUnlocked==0&&!CGStore.Data.completed[0];label=first?(design!=null?design.newGameLabel:"Iniciar aventura"):(design!=null?design.continueLabel:"Continuar · Nivel {0}").Replace("{0}",(Mathf.Min(CGStore.Data.latestUnlocked,8)+1).ToString());}if(MainButton(label,i,new Rect(start.x,start.y+i*spacing,size.x,size.y)))MainSelect(i);}
        var captionStyle=new GUIStyle(textStyle){font=CGMenuTheme.Interface,fontSize=23,alignment=TextAnchor.MiddleLeft};string caption=design!=null?design.mainCaption:"EXPLORA · APRENDE · RESTAURA EL CAMPUS";
        RectColor(new Rect(42,123,570,39),new Color(.012f,.02f,.045f,.84f));CGUIDraw.Text(new Rect(55,125,545,34),caption,captionStyle);
        CGUIDraw.Texture(new Rect(878,82,24,24),CGMenuArtwork.Icon(1),ScaleMode.ScaleToFit);
        CGUIDraw.Text(new Rect(913,79,287,30),design!=null?design.location:"UNIFRANZ · SEDE LA PAZ",new GUIStyle(tinyStyle){font=CGMenuTheme.Interface,fontSize=19,alignment=TextAnchor.MiddleLeft});
        if(design!=null&&design.mainDescriptions!=null&&selection<design.mainDescriptions.Length)CGUIDraw.Text(new Rect(start.x,632,size.x,37),design.mainDescriptions[selection],tinyStyle);
      }
      if(page==Page.Intro)DrawIntro();
      if(page==Page.Map)DrawCampusMap();
      if(page==Page.Customization)DrawCustomization();
      if(page==Page.Settings)DrawSettings();
      if(page==Page.Controls)DrawControls();
      if(page==Page.Help)DrawHelp();
      if(page==Page.Credits)DrawCredits();
      if(page==Page.Paused){Panel(new Rect(330,220,620,300));Header("JUEGO EN PAUSA");if(Button("CONTINUAR",0,375,260,530,65))PauseChoice(0);
       if(Button("REINICIAR DESDE CHECKPOINT",1,375,342,530,65))PauseChoice(1);
       if(Button("SALIR AL MENÚ",2,375,425,530,65))PauseChoice(2);}
      if(page==Page.GameOver){
       Header("GAME OVER");Panel(new Rect(290,210,700,380));
       Label("PERDISTE LA SEÑAL",245,60);Label("Reintenta desde el checkpoint con tres corazones.",320,60);
       bool enabled=GUI.enabled;GUI.enabled=enabled&&Time.frameCount>gameOverEnteredFrame;
       if(Button("REINTENTAR",0,375,420,530,65))GameOverChoice(0);
       if(Button("MENÚ PRINCIPAL",1,375,505,530,60))GameOverChoice(1);
       GUI.enabled=enabled;
      }
      if(page==Page.Quiz){Panel(new Rect(240,200,800,440));Header("TERMINAL / MISIÓN DE APRENDIZAJE");Label(world.spec.quiz,230,75);
       string[] options={world.spec.option1,world.spec.option2,world.spec.option3};for(int i=0;i<3;i++)if(Button(options[i],i,340,333+i*80,600,67))QuizAnswer(i);
       if(Button("CERRAR TERMINAL",3,420,581,440,48))CloseQuiz();
       Label("ESC / B para cerrar la terminal",646,29);}
      if(page==Page.Victory){Header("¡NIVEL RESTAURADO!");Label("Recuperaste los tres fragmentos y activaste la conexión.",265);
       DrawAbilityReward();if(sceneLevel==1)Label("Variante morada disponible en Personalización",294,30);
       if(Button("SIGUIENTE NIVEL",0,375,453,530,66))HandleVictorySelection(0,AfterVictory,ToMain);if(Button("MENÚ PRINCIPAL",1,375,535,530,60))HandleVictorySelection(1,AfterVictory,ToMain);}
      if(page==Page.Ending){Header("¡CAMPUS RESTAURADO!");Label("CORE GLITCH HA SIDO DERROTADO",260);DrawAbilityReward();
       Label("UNIFRANZ LA PAZ — FIN DE LA AVENTURA",440,35);
       if(Button("VER CRÉDITOS",0,390,501,500,65))HandleEndingSelection(0,OpenEndingCredits,ToMain);
       if(Button("VOLVER AL MENÚ",1,390,583,500,55))HandleEndingSelection(1,OpenEndingCredits,ToMain);}
      if(page==Page.ConfirmReset){Header("¿BORRAR PROGRESO?");Label("Se eliminarán los niveles completados y las skins desbloqueadas.",311);
       if(Button("SÍ, BORRAR",0,320,431,300,58))ConfirmResetChoice(0);if(Button("CANCELAR",1,660,431,300,58))ConfirmResetChoice(1);}
      if(Time.unscaledTime<toastUntil && page!=Page.Playing&&page!=Page.Customization&&page!=Page.Controls&&page!=Page.Settings&&page!=Page.Credits&&page!=Page.Map&&page!=Page.Help){CGUIDraw.Text(new Rect(180,page==Page.Quiz?132:628,920,35),toast,tinyStyle);}
      CGUIDraw.Matrix=old;
      CGUIDraw.Color=oldColor;
    }
    void DrawHUD(){
      if(CGStore.Data.subtitles&&Time.unscaledTime<CGAudio.CaptionUntil){Panel(new Rect(390,570,500,46));CGUIDraw.Text(new Rect(402,575,476,36),CGAudio.Caption,tinyStyle);}
      if(world==null||Hero==null)return;
      // Touch controls reserve the original x=690..820 gap for the pause button.
      if(CGGuardianArt.Available&&!CGTouch.Enabled){CGCampusHUD.Draw(world,Hero,chips,taken,terminalSolved,bossDead);return;}
      Rect r=new Rect(12,12,625,72);Panel(r,new Color(.97f,.43f,.12f,1));
      CGUIDraw.Text(new Rect(24,18,595,24),world.spec.name,tinyStyle);
      CGUIDraw.Text(new Rect(24,44,600,27),"VIDAS "+Mathf.Max(0,Hero.hearts)+"/3   ·   DATOS "+chips+"/3   ·   "+(terminalSolved?"TERMINAL OK":"TERMINAL PENDIENTE"),tinyStyle);
      Panel(new Rect(12,91,340,53),new Color(.25f,.8f,1));CGUIDraw.Text(new Rect(24,94,315,23),CGAbilities.Names[CGStore.Data.equippedAbility]+" · "+Mathf.FloorToInt(Hero.Skills.Energy)+"/100",tinyStyle);RectColor(new Rect(24,126,315*Hero.Skills.Energy/100,6),new Color(.25f,.8f,1));
      Rect rr=new Rect(866,12,403,50);Panel(rr,new Color(.34f,.67f,1f,1));
      CGUIDraw.Text(new Rect(876,19,383,33),CGTouch.Enabled?"TOCA USAR PARA INTERACTUAR":"E / Y  INTERACTUAR   ·   ESC / START  PAUSA",tinyStyle);
      if(world.boss!=null&&!bossDead){Panel(new Rect(390,97,500,52),new Color(1,.21f,.25f,1));CGUIDraw.Text(new Rect(404,106,474,36),"JEFE: "+world.boss.kind+"   |   VIDA: "+world.boss.hp,textStyle);}
    }
}
}
