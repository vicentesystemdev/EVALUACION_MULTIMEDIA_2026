using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
public partial class CGGame {
    bool mapDetails;int renderedMap=-1;float mapChangedAt;
    static readonly Color MapGreen=new Color(.45f,.95f,.42f),MapCyan=new Color(.26f,.9f,1),MapAmber=new Color(1,.6f,.12f),MapLocked=new Color(.42f,.49f,.6f);
    static readonly string[] Risk={"INTRODUCCIÓN","EXPLORACIÓN","RUTAS ALTAS","EXPLORACIÓN","REDES Y NODOS","JEFE: SECUENCIAS","RUTAS ALTAS","JEFE: IF / ELSE","JEFE: CORE GLITCH","RUTA OPCIONAL"};
    static readonly string[] Guides={"GUARDIA U + ESTUDIANTE + TÉCNICA","RECEPCIÓN","DIRECCIÓN","DOCENTE DE LABORATORIO","COORDINACIÓN DE SISTEMAS","DIRECCIÓN","GUÍA DE BIBLIOTECA","ESTUDIANTE","GUÍA DEL NÚCLEO","DOCENTE DE ANATOMÍA"};
    bool Unlocked(int index){return index==9?CGStore.Data.extraUnlocked:index<=CGStore.Data.latestUnlocked;}
    string MapState(int index){return CGStore.Data.completed[index]?"COMPLETADO":Unlocked(index)?"DISPONIBLE":"BLOQUEADO";}
    string MapGate(int index){return index==9?"Completa N05 para abrir el subsuelo.":index==0?"Acceso inicial disponible.":"Completa N"+index.ToString("00")+" para abrir este nivel.";}
    string MapReward(int index){return index==1?"Al completar: variante morada del Guardián.":index==4?"Al completar: acceso al subsuelo de anatomía.":index==8?"Al completar: final de la aventura del campus.":index==9?"Al completar: etapa extra restaurada.":"Al completar: siguiente etapa del recorrido.";}
    bool HandleMapShortcuts(){
        var pad=Gamepad.current;var keyboard=Keyboard.current;
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
    static Rect MapCardRect(int index){return index==9?new Rect(320,586,280,68):new Rect(30+(index%3)*290,180+(index/3)*133,270,116);}
    void DrawMapHeader(){
        RectColor(new Rect(0,0,1280,82),new Color(.018f,.028f,.065f,.97f));RectColor(new Rect(0,79,1280,3),MapAmber);
        MapLabel(new Rect(30,4,460,46),"CAMPUS GLITCH",35,MapAmber);
        MapLabel(new Rect(32,47,550,27),"UNIFRANZ LA PAZ  |  INNOVACIÓN · APRENDE HACIENDO",15);
        if(Click(new Rect(763,21,149,40),"MENÚ PRINCIPAL",MapText(19,TextAnchor.MiddleCenter)))ToMain();
        if(Click(new Rect(921,21,172,40),"PERSONALIZACIÓN",MapText(19,TextAnchor.MiddleCenter)))OpenUIScreen("SC_Customization",Page.Customization);
        if(Click(new Rect(1102,21,146,40),"AJUSTES",MapText(19,TextAnchor.MiddleCenter)))OpenUIScreen("SC_Settings",Page.Settings);
        foreach(float x in new[]{758,916,1098})Outline(new Rect(x,19,x==916?180:155,44),new Color(.3f,.42f,.6f),1);
    }
    void MapLine(Vector2 from,Vector2 to,Color color){
        if(Mathf.Abs(from.x-to.x)<.1f)RectColor(new Rect(from.x-2,Mathf.Min(from.y,to.y),4,Mathf.Abs(to.y-from.y)),color);
        else if(Mathf.Abs(from.y-to.y)<.1f)RectColor(new Rect(Mathf.Min(from.x,to.x),from.y-2,Mathf.Abs(to.x-from.x),4),color);
        else{MapLine(from,new Vector2(to.x,from.y),color);MapLine(new Vector2(to.x,from.y),to,color);}
    }
    void DrawCampusMap(){
        DrawMapHeader();
        if(renderedMap!=selection){renderedMap=selection;mapChangedAt=CGMenuTheme.Clock;}
        if(mapDetails){DrawMapDetail();return;}
        MapLabel(new Rect(30,94,650,49),"MAPA DEL CAMPUS",35,Color.white);
        MapLabel(new Rect(32,140,825,28),"EXPLORA · RECUPERA DATOS · DESBLOQUEA EL CAMPUS",18,new Color(.79f,.86f,.97f));
        // Route lines are drawn behind cards and use actual completion/unlock state.
        for(int i=0;i<8;i++){
            var a=MapCardRect(i);var b=MapCardRect(i+1);Color route=CGStore.Data.completed[i]?MapAmber:new Color(.23f,.32f,.45f,.9f);
            if(i%3<2)MapLine(new Vector2(a.xMax,a.center.y),new Vector2(b.x,b.center.y),route);
            else{float y=a.yMax+8;MapLine(new Vector2(a.center.x,a.yMax),new Vector2(b.center.x,y),route);MapLine(new Vector2(b.center.x,y),new Vector2(b.center.x,b.y),route);}
        }
        MapLine(new Vector2(MapCardRect(4).center.x,MapCardRect(4).yMax),new Vector2(MapCardRect(9).center.x,MapCardRect(9).y),CGStore.Data.extraUnlocked?new Color(.82f,.37f,1):MapLocked);
        for(int index=0;index<10;index++)DrawMapCard(index);
        DrawMapSummary(new Rect(915,102,335,550),selection);
        Panel(new Rect(30,586,270,68),new Color(.3f,.42f,.62f));
        CGGuardianArt.DrawPortrait(new Rect(39,592,55,55),CGArt.Player(CGStore.Data.purpleEquipped,0,CGStore.Data.useOriginalWolf));
        MapLabel(new Rect(105,589,185,31),"GUARDIÁN DEL CAMPUS",17);
        MapLabel(new Rect(105,619,185,24),CGStore.Data.purpleEquipped?"EQUIPADO: MORADO":"EQUIPADO: ROJO",16,MapAmber);
        Panel(new Rect(610,586,270,68),new Color(.3f,.42f,.62f));
        MapLabel(new Rect(625,590,245,24),"COMPLETADO / DISPONIBLE",17,MapGreen);
        MapLabel(new Rect(625,621,245,24),"BLOQUEADO: COMPLETA EL ANTERIOR",15,MapLocked);
    }
    void DrawMapCard(int index){
        var rect=MapCardRect(index);bool selected=selection==index,unlocked=Unlocked(index),complete=CGStore.Data.completed[index];
        Color accent=complete?MapGreen:unlocked?MapCyan:MapLocked;if(index==9)accent=unlocked?new Color(.82f,.37f,1):MapLocked;
        float lift=editorMenuPreview?(selected?3:0):menuMotion.Focus(index)*3;rect.y-=lift;
        if(selected){Outline(new Rect(rect.x-4,rect.y-4,rect.width+8,rect.height+8),MapAmber,2);RectColor(new Rect(rect.x-4,rect.y-4,7,rect.height+8),MapAmber);}
        Panel(rect,accent);
        if(index==9){MapImage(new Rect(rect.x+3,rect.y+3,100,rect.height-6),index);MapLabel(new Rect(rect.x+113,rect.y+1,161,31),"EXTRA · ANATOMÍA",21);MapLabel(new Rect(rect.x+113,rect.y+32,161,29),MapState(index),18,accent);}
        else{
            Rect badge=new Rect(rect.x+8,rect.y+7,46,24);RectColor(badge,MapAmber);MapLabel(badge,"N"+(index+1).ToString("00"),20,new Color(.02f,.03f,.06f),false,TextAnchor.MiddleCenter);
            MapLabel(new Rect(rect.x+62,rect.y+3,200,34),CGMenuTheme.MapNames[index],19);
            var oldColor=CGUIDraw.Color;if(!unlocked)CGUIDraw.Color=new Color(.48f,.54f,.65f,oldColor.a);
            MapImage(new Rect(rect.x+7,rect.y+40,rect.width-14,47),index);CGUIDraw.Color=oldColor;
            MapLabel(new Rect(rect.x+8,rect.y+88,rect.width-16,23),MapState(index)+(complete?" · 3/3 DATOS":""),19,accent,false,TextAnchor.MiddleCenter);
        }
        if(!editorMenuPreview&&pointerMoved&&rect.Contains(Event.current.mousePosition))selection=mapSelection=index;
        if(Click(rect,"",GUIStyle.none)){selection=mapSelection=index;CGAudio.Menu();}
    }
    void DrawMapSummary(Rect rect,int index){
        bool complete=CGStore.Data.completed[index],unlocked=Unlocked(index);Color accent=complete?MapGreen:unlocked?MapCyan:MapLocked;
        Panel(rect,MapAmber);
        MapLabel(new Rect(rect.x+16,rect.y+10,rect.width-32,56),(index==9?"EXTRA":"N"+(index+1).ToString("00"))+" · "+CGMenuTheme.MapNames[index],27);
        var previousColor=CGUIDraw.Color;
        float imageEntry=editorMenuPreview||CGStore.Data.reduceFlash?1:Mathf.SmoothStep(.65f,1,Mathf.Clamp01((CGMenuTheme.Clock-mapChangedAt)/.18f));
        CGUIDraw.Color=new Color(previousColor.r,previousColor.g,previousColor.b,previousColor.a*imageEntry);
        MapImage(new Rect(rect.x+16,rect.y+78,rect.width-32,145),index);CGUIDraw.Color=previousColor;
        MapLabel(new Rect(rect.x+16,rect.y+226,rect.width-32,28),MapState(index)+"  /  "+Risk[index],19,accent);
        MapLabel(new Rect(rect.x+16,rect.y+262,rect.width-32,64),CGLevels.All[index].objective,18,null,true);
        MapLabel(new Rect(rect.x+16,rect.y+330,rect.width-32,50),complete?"REGISTRO GUARDADO\n3/3 datos · terminal y portal completados":"OBJETIVOS\n3 fragmentos · terminal · portal",18,complete?MapGreen:Color.white);
        MapLabel(new Rect(rect.x+16,rect.y+386,rect.width-32,48),MapReward(index),17,new Color(.84f,.89f,1),true);
        if(!unlocked)MapLabel(new Rect(rect.x+16,rect.y+435,rect.width-32,33),MapGate(index),16,MapLocked);
        var play=new Rect(rect.x+16,rect.y+473,rect.width-32,44);RectColor(play,unlocked?new Color(.62f,.22f,.025f):new Color(.13f,.17f,.23f));Outline(play,unlocked?MapAmber:MapLocked,2);
        if(Click(play,unlocked?"JUGAR NIVEL  /  A":"NIVEL BLOQUEADO",MapText(27,TextAnchor.MiddleCenter))){ChooseLevel(index);}
        if(Click(new Rect(rect.x+16,rect.y+519,rect.width-32,27),"Y / TAB · VER DETALLE",MapText(18,TextAnchor.MiddleCenter))){mapDetails=true;CGAudio.Menu();}
        int completed=0;foreach(bool done in CGStore.Data.completed)if(done)completed++;
        MapLabel(new Rect(925,656,215,22),"CAMPUS RESTAURADO  "+completed+" / 10",16);
        RectColor(new Rect(1140,662,110,9),new Color(.16f,.2f,.29f));RectColor(new Rect(1140,662,11*completed,9),MapAmber);
    }
    void DrawMapDetail(){
        int index=selection;
        MapLabel(new Rect(30,94,965,55),(index==9?"EXTRA":"N"+(index+1).ToString("00"))+" · "+CGMenuTheme.MapNames[index],34);
        if(Click(new Rect(1010,104,240,42),"B / ESC · VOLVER AL MAPA",MapText(20,TextAnchor.MiddleCenter))){mapDetails=false;CGAudio.Menu();}
        Panel(new Rect(28,168,715,347),MapAmber);MapImage(new Rect(35,175,701,333),index);
        Panel(new Rect(28,528,715,123),new Color(.3f,.42f,.62f));
        MapLabel(new Rect(44,538,681,52),CGLevels.All[index].objective,22,null,true);
        MapLabel(new Rect(44,598,681,39),"GUÍA: "+Guides[index]+"  ·  "+Risk[index],20,MapAmber);
        DrawMapSummary(new Rect(765,102,485,550),index);
    }
}
}
