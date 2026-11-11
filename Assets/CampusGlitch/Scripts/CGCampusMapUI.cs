using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
public partial class CGGame {
    bool mapDetails;int renderedMap=-1;float mapChangedAt;CGMapDesign mapDesign;
    CGMapDesign MapDesign {get{if(mapDesign==null)mapDesign=Resources.Load<CGMapDesign>("CG/Presentation/CampusMap");return mapDesign;}}
    Color MapInk {get{return MapDesign!=null?MapDesign.panel:new Color(.025f,.04f,.085f,1);}}
    void MapPanel(Rect r,Color accent){CGSettingsArt.Frame(r,accent,MapInk);}
    public Rect CampusMapCardRect(int index){var d=MapDesign;var origin=d!=null?d.cardOrigin:new Vector2(30,181);var size=d!=null?d.cardSize:new Vector2(268,118);var gap=d!=null?d.cardGap:new Vector2(14,16);return index==9?new Rect(312,586,268,68):new Rect(origin.x+(index%3)*(size.x+gap.x),origin.y+(index/3)*(size.y+gap.y),size.x,size.y);}
    static readonly Color MapGreen=new Color(.45f,.95f,.42f),MapCyan=new Color(.26f,.9f,1),MapAmber=new Color(1,.6f,.12f),MapLocked=new Color(.42f,.49f,.6f);
    static readonly string[] Risk={"INTRODUCCIÓN","EXPLORACIÓN","RUTAS ALTAS","EXPLORACIÓN","REDES Y NODOS","JEFE: SECUENCIAS","RUTAS ALTAS","JEFE: IF / ELSE","JEFE: CORE GLITCH","RUTA OPCIONAL"};
    static readonly string[] Guides={"GUARDIA U + ESTUDIANTE + TÉCNICA","RECEPCIÓN","DIRECCIÓN","DOCENTE DE LABORATORIO","COORDINACIÓN DE SISTEMAS","DIRECCIÓN","GUÍA DE BIBLIOTECA","ESTUDIANTE","GUÍA DEL NÚCLEO","DOCENTE DE ANATOMÍA"};
    bool Unlocked(int index){return index==9?CGStore.Data.extraUnlocked:index<=CGStore.Data.latestUnlocked;}
    string MapState(int index){return CGStore.Data.completed[index]?"COMPLETADO":Unlocked(index)?"DISPONIBLE":"BLOQUEADO";}
    string MapGate(int index){return index==9?"Completa N05 para abrir el subsuelo.":index==0?"Acceso inicial disponible.":"Completa N"+index.ToString("00")+" para abrir este nivel.";}
    string MapReward(int index){return index==9?"ETAPA EXTRA RESTAURADA":CGAbilities.Rewards[index]+(index==1?" · VARIANTE MORADA":index==4?" · ACCESO AL SUBSUELO":index==8?" · FINAL DEL RECORRIDO":"");}
    bool HandleMapShortcuts(){
        var pad=Gamepad.current;var keyboard=Keyboard.current;
        if(pad!=null&&pad.startButton.wasPressedThisFrame||keyboard!=null&&keyboard.mKey.wasPressedThisFrame){ToMain();return true;}
        if(keyboard!=null&&keyboard.pKey.wasPressedThisFrame||pad!=null&&pad.buttonWest.wasPressedThisFrame){OpenUIScreen("SC_Customization",Page.Customization);return true;}
        if(keyboard!=null&&keyboard.cKey.wasPressedThisFrame||pad!=null&&pad.selectButton.wasPressedThisFrame){OpenUIScreen("SC_Settings",Page.Settings);return true;}
        bool toggle=pad!=null&&pad.buttonNorth.wasPressedThisFrame||keyboard!=null&&keyboard.tabKey.wasPressedThisFrame;
        if(toggle){mapDetails=!mapDetails;CGAudio.Menu();menuNavigation.Reset();return true;}
        if(mapDetails&&CGControl.Back()){mapDetails=false;CGAudio.Menu();menuNavigation.Reset();return true;}
        int step=0;
        if(pad!=null){if(pad.leftShoulder.wasPressedThisFrame)step=-1;if(pad.rightShoulder.wasPressedThisFrame)step=1;}
        if(keyboard!=null){if(keyboard.pageUpKey.wasPressedThisFrame)step=-1;if(keyboard.pageDownKey.wasPressedThisFrame)step=1;}
        if(step==0)return false;selection=mapSelection=(selection+step+10)%10;CGAudio.Menu();return true;
    }
    GUIStyle MapText(int size,TextAnchor align=TextAnchor.MiddleLeft,Color? color=null,bool body=false){
        var style=new GUIStyle(textStyle){font=body?CGMenuTheme.Body:CGMenuTheme.Interface,fontSize=size,alignment=align,wordWrap=true};style.normal.textColor=color??Color.white;return style;
    }
    void MapLabel(Rect r,string text,int size=20,Color? color=null,bool body=false,TextAnchor anchor=TextAnchor.MiddleLeft){CGUIDraw.Text(r,text,MapText(size,anchor,color,body));}
    void MapImage(Rect r,int index){
        var texture=Resources.Load<Texture2D>("CG/MapPreviews/Level"+index.ToString("00"));
        if(texture!=null)CGUIDraw.Texture(r,texture,ScaleMode.ScaleAndCrop);
    }
    void DrawMapHeader(){
        RectColor(new Rect(0,0,1280,82),new Color(.018f,.028f,.065f,1));RectColor(new Rect(0,79,1280,3),MapAmber);
        var style=MapText(29);style.font=CGMenuTheme.Title;CGUIDraw.Text(new Rect(30,9,650,40),"CAMPUS GLITCH",style);
        MapLabel(new Rect(32,49,650,25),"UNIFRANZ LA PAZ · EXPLORA · APRENDE HACIENDO",17);
        string[] text={"MENÚ · M / START","GUARDIÁN · P / X","AJUSTES · C / VIEW"};
        for(int i=0;i<3;i++){var r=new Rect(745+i*171,21,160,43);MapPanel(r,MapCyan);MapLabel(r,text[i],19,null,false,TextAnchor.MiddleCenter);if(Click(r,"",GUIStyle.none)){if(i==0)ToMain();else OpenUIScreen(i==1?"SC_Customization":"SC_Settings",i==1?Page.Customization:Page.Settings);}}
    }
    void MapLine(Vector2 from,Vector2 to,Color color){
        if(Mathf.Abs(from.x-to.x)<.1f)RectColor(new Rect(from.x-2,Mathf.Min(from.y,to.y),4,Mathf.Abs(to.y-from.y)),color);
        else if(Mathf.Abs(from.y-to.y)<.1f)RectColor(new Rect(Mathf.Min(from.x,to.x),from.y-2,Mathf.Abs(to.x-from.x),4),color);
        else{MapLine(from,new Vector2(to.x,from.y),color);MapLine(new Vector2(to.x,from.y),to,color);}
    }
    void DrawCampusMap(){
        DrawMapHeader();if(renderedMap!=selection){renderedMap=selection;mapChangedAt=CGMenuTheme.Clock;}
        if(mapDetails){DrawMapDetail();return;}
        RectColor(new Rect(24,91,844,79),new Color(.02f,.03f,.065f,.85f));MapLabel(new Rect(30,95,830,43),MapDesign!=null?MapDesign.title:"MAPA DEL CAMPUS",33);
        bool notice=Time.unscaledTime<toastUntil;MapLabel(new Rect(32,139,830,28),notice?toast:MapDesign!=null?MapDesign.subtitle:"ELIGE TU DESTINO · DESCUBRE SUS ACTIVIDADES",18,notice?MapAmber:new Color(.79f,.86f,.97f));
        for(int i=0;i<8;i++){
            var a=CampusMapCardRect(i);var b=CampusMapCardRect(i+1);Color route=CGStore.Data.completed[i]?MapAmber:MapLocked;
            if(i%3<2){MapLine(new Vector2(a.xMax,a.center.y),new Vector2(b.x,b.center.y),route);if(CGStore.Data.completed[i]&&!CGSettingsRuntime.ReduceFlash&&(MapDesign==null||MapDesign.animateRoutes)){float t=Mathf.Repeat(CGMenuTheme.Clock*.5f+i*.19f,1);RectColor(new Rect(Mathf.Lerp(a.xMax,b.x,t)-2,a.center.y-4,4,8),Color.white);}}
            else {float y=a.yMax+8;MapLine(new Vector2(a.xMax-18,a.yMax),new Vector2(a.xMax-18,y),route);MapLine(new Vector2(a.xMax-18,y),new Vector2(b.x+18,y),route);MapLine(new Vector2(b.x+18,y),new Vector2(b.x+18,b.y),route);}
        }
        for(int index=0;index<10;index++)DrawMapCard(index);
        DrawMapSummary(new Rect(892,102,360,552),selection);
        MapPanel(new Rect(30,586,268,68),MapCyan);CGGuardianArt.DrawPortrait(new Rect(39,592,55,55),CGArt.Player(CGStore.Data.purpleEquipped,0,CGStore.Data.useOriginalWolf));
        MapLabel(new Rect(105,590,183,27),"TU GUARDIÁN",20);MapLabel(new Rect(105,618,183,24),CGStore.Data.purpleEquipped?"VARIANTE MORADA":"VARIANTE ROJA",17,MapAmber);
        MapPanel(new Rect(594,586,268,68),MapCyan);MapLabel(new Rect(607,590,242,25),"DISPONIBLE · COMPLETADO",17,MapGreen);MapLabel(new Rect(607,620,242,24),"BLOQUEADO: SIGUE EL RECORRIDO",15,MapLocked);
        DrawCampusProgress();
    }
    void DrawCampusProgress(){
        int done=0;foreach(bool complete in CGStore.Data.completed)if(complete)done++;
        MapLabel(new Rect(30,657,515,21),"CAMPUS RESTAURADO · "+done+" / 10 NIVELES",15,new Color(.87f,.9f,1));
        for(int i=0;i<10;i++)RectColor(new Rect(581+i*67,663,61,6),CGStore.Data.completed[i]?MapAmber:new Color(.2f,.28f,.4f));
    }
    void DrawMapCard(int index){
        var rect=CampusMapCardRect(index);bool selected=selection==index,unlocked=Unlocked(index),complete=CGStore.Data.completed[index];Color accent=complete?MapGreen:unlocked?MapCyan:MapLocked;
        float lift=MapDesign!=null?MapDesign.selectionLift:3;rect.y-=editorMenuPreview?(selected?lift:0):menuMotion.Focus(index)*lift;
        MapPanel(rect,selected?MapAmber:accent);if(selected)RectColor(new Rect(rect.x+2,rect.y+15,4,rect.height-30),MapAmber);
        if(index==9){MapImage(new Rect(rect.x+10,rect.y+8,89,rect.height-16),index);MapLabel(new Rect(rect.x+109,rect.y+3,151,29),"EXTRA · ANATOMÍA",21);MapLabel(new Rect(rect.x+109,rect.y+34,151,26),MapState(index),18,accent);}
        else{
            var badge=new Rect(rect.x+10,rect.y+7,43,25);RectColor(badge,MapAmber);MapLabel(badge,"N"+(index+1).ToString("00"),20,new Color(.02f,.03f,.06f),false,TextAnchor.MiddleCenter);
            MapLabel(new Rect(rect.x+61,rect.y+2,rect.width-69,35),CGMenuTheme.MapNames[index],19);
            var old=CGUIDraw.Color;if(!unlocked)CGUIDraw.Color=new Color(.5f,.55f,.63f,old.a);MapImage(new Rect(rect.x+10,rect.y+40,rect.width-20,45),index);CGUIDraw.Color=old;
            MapLabel(new Rect(rect.x+10,rect.y+89,rect.width-20,25),MapState(index),19,accent,false,TextAnchor.MiddleCenter);
        }
        if(!editorMenuPreview&&pointerMoved&&rect.Contains(Event.current.mousePosition))selection=mapSelection=index;
        if(Click(rect,"",GUIStyle.none)){selection=mapSelection=index;CGAudio.Menu();}
    }
    void DrawMapActivity(Rect r,string label,bool complete,int number){
        CGSettingsArt.Frame(r,complete?MapGreen:new Color(.25f,.36f,.48f),MapInk);MapLabel(new Rect(r.x+8,r.y+2,25,r.height-4),complete?"✓":number.ToString(),19,complete?MapGreen:MapAmber,false,TextAnchor.MiddleCenter);MapLabel(new Rect(r.x+41,r.y+1,r.width-49,r.height-2),label,19);
    }
    void DrawMapSummary(Rect rect,int index){
        bool complete=CGStore.Data.completed[index],unlocked=Unlocked(index);Color accent=complete?MapGreen:unlocked?MapCyan:MapLocked;
        MapPanel(rect,MapAmber);MapLabel(new Rect(rect.x+16,rect.y+9,rect.width-32,51),(index==9?"EXTRA":"N"+(index+1).ToString("00"))+" · "+CGMenuTheme.MapNames[index],27);
        var color=CGUIDraw.Color;float entry=editorMenuPreview||CGSettingsRuntime.ReduceFlash?1:Mathf.SmoothStep(.65f,1,Mathf.Clamp01((CGMenuTheme.Clock-mapChangedAt)/.18f));CGUIDraw.Color=new Color(color.r,color.g,color.b,color.a*entry);MapImage(new Rect(rect.x+16,rect.y+67,rect.width-32,121),index);CGUIDraw.Color=color;
        MapLabel(new Rect(rect.x+16,rect.y+194,rect.width-32,25),MapState(index)+" · "+Risk[index],18,accent);
        MapLabel(new Rect(rect.x+16,rect.y+224,rect.width-32,47),CGLevels.All[index].objective,18,null,true);
        MapLabel(new Rect(rect.x+16,rect.y+275,rect.width-32,24),complete?"ACTIVIDADES COMPLETADAS":"ACTIVIDADES DEL NIVEL",20,MapAmber);
        DrawMapActivity(new Rect(rect.x+16,rect.y+306,rect.width-32,30),"Recuperar los tres fragmentos",complete,1);
        DrawMapActivity(new Rect(rect.x+16,rect.y+342,rect.width-32,30),"Resolver el reto de la terminal",complete,2);
        DrawMapActivity(new Rect(rect.x+16,rect.y+378,rect.width-32,30),string.IsNullOrEmpty(CGLevels.All[index].boss)?"Abrir el portal y continuar":"Vencer al jefe y abrir el portal",complete,3);
        MapLabel(new Rect(rect.x+16,rect.y+413,rect.width-32,18),complete?"RECOMPENSA CONSEGUIDA":"AL COMPLETAR",15,complete?MapGreen:MapCyan);
        MapLabel(new Rect(rect.x+16,rect.y+433,rect.width-32,38),MapReward(index),18,Color.white);
        var play=new Rect(rect.x+16,rect.y+481,rect.width-32,43);CGSettingsArt.Frame(play,unlocked?MapAmber:MapLocked,unlocked?new Color(.38f,.13f,.025f,1):MapInk);MapLabel(play,unlocked?(complete?"VOLVER A JUGAR  /  A":"JUGAR NIVEL  /  A"):"NIVEL BLOQUEADO",25,null,false,TextAnchor.MiddleCenter);
        if(Click(play,"",GUIStyle.none)&&unlocked)ChooseLevel(index);
        string bottom=!unlocked?MapGate(index):mapDetails?"Y / TAB · VOLVER AL RECORRIDO":"Y / TAB · VER NIVEL Y GUÍAS";
        var more=new Rect(rect.x+16,rect.y+525,rect.width-32,25);MapLabel(more,bottom,17,!unlocked?MapLocked:MapCyan,false,TextAnchor.MiddleCenter);if(unlocked&&Click(more,"",GUIStyle.none)){mapDetails=!mapDetails;CGAudio.Menu();}
    }
    void DrawMapDetail(){
        int index=selection;RectColor(new Rect(24,91,818,57),new Color(.02f,.03f,.065f,.85f));MapLabel(new Rect(30,96,793,47),Time.unscaledTime<toastUntil?toast:"DESCUBRE EL NIVEL",Time.unscaledTime<toastUntil?25:33);
        var scene=new Rect(30,163,806,310);MapPanel(scene,MapAmber);MapImage(new Rect(scene.x+8,scene.y+8,scene.width-16,scene.height-16),index);
        MapPanel(new Rect(30,488,806,78),MapCyan);MapLabel(new Rect(47,495,772,28),CGMenuTheme.MapNames[index],25);MapLabel(new Rect(47,529,772,27),"CONOCE A: "+Guides[index]+" · "+Risk[index],19,new Color(.83f,.9f,1));
        for(int i=0;i<10;i++){var r=new Rect(30+i*81,585,77,68);MapPanel(r,i==index?MapAmber:Unlocked(i)?MapCyan:MapLocked);MapImage(new Rect(r.x+5,r.y+5,67,34),i);MapLabel(new Rect(r.x+4,r.y+41,69,22),i==9?"EXTRA":"N"+(i+1).ToString("00"),17,null,false,TextAnchor.MiddleCenter);if(Click(r,"",GUIStyle.none)){selection=mapSelection=i;CGAudio.Menu();}}
        DrawMapSummary(new Rect(862,102,390,552),index);DrawCampusProgress();
    }
#if UNITY_EDITOR
    public void PreviewCampusMap(int level,bool details){selection=mapSelection=Mathf.Clamp(level,0,9);mapDetails=details;renderedMap=selection;mapChangedAt=CGMenuTheme.Clock;}
#endif
}
}
