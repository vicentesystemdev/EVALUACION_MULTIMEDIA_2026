using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
public partial class CGGame {
    CGSettingsDesign settingsDesign;CGSettingsDraft settingsDraft,settingsBaseline;
    int settingsSection,settingsDialog,settingsChoice=1,settingsLeaveTarget,settingsAxis;
    float settingsRepeat,settingsChangedAt,settingsConfirmUntil;string settingsNotice="";
    CGSettingsOptions displayBackup;bool displayTrial,settingsDialogReady;
    readonly List<Vector2Int> settingsResolutions=new List<Vector2Int>();readonly float[] settingsSpectrum=new float[32];
    static readonly string[] SettingsSections={"Pantalla","Audio","Accesibilidad","Partida"};
    static readonly string[] SettingsSamples={"Salto","Ataque","Interacción","Menú","Alerta"};
    static readonly string[] SettingsClips={"Jump","Attack","Checkpoint","Click","Denied"};
    CGSettingsDesign SettingsDesign {get{if(settingsDesign==null)settingsDesign=Resources.Load<CGSettingsDesign>("CG/Presentation/CampusSettings");return settingsDesign;}}
    int SettingsRows {get{return settingsSection==1?5:settingsSection==3?2:4;}}
    int SettingsFooter {get{return 4+SettingsRows+(settingsSection==1?5:0);}}
    bool SettingsDirty {get{return settingsDraft!=null&&!settingsDraft.Same(settingsBaseline);}}
    void OpenSettingsDialog(int kind){settingsDialog=kind;settingsChoice=1;settingsDialogReady=false;menuNavigation.Reset();}
    void BeginSettings(){
        CGStore.Load();settingsDraft=CGSettingsDraft.Read(CGStore.Data);settingsResolutions.Clear();
        foreach(var res in Screen.resolutions)if(res.width>=640&&res.height>=360){var v=new Vector2Int(res.width,res.height);if(!settingsResolutions.Contains(v))settingsResolutions.Add(v);}
        if(settingsDraft.options.width==0){settingsDraft.options.width=Application.isEditor?Screen.currentResolution.width:Screen.width;settingsDraft.options.height=Application.isEditor?Screen.currentResolution.height:Screen.height;}
        if(settingsDraft.options.width<640||settingsDraft.options.height<360){settingsDraft.options.width=1280;settingsDraft.options.height=720;}
        if(settingsDraft.options.screenMode<0)settingsDraft.options.screenMode=Application.isEditor?0:CGSettingsRuntime.ModeIndex(Screen.fullScreenMode);
        var current=new Vector2Int(settingsDraft.options.width,settingsDraft.options.height);if(!settingsResolutions.Contains(current))settingsResolutions.Add(current);
        settingsBaseline=settingsDraft.Copy();settingsDialog=0;settingsChoice=1;selection=4;settingsNotice="";settingsAxis=0;menuNavigation.Reset();SyncSettingsPreview();
    }
    void SyncSettingsPreview(){if(Application.isPlaying){CGSettingsRuntime.Preview=settingsDraft;CGSettingsRuntime.Audio();}}
    void OnDisable(){CGSettingsWorldView.Cleanup();CGHelpArt.Cleanup();if(settingsDraft!=null){if(displayTrial)RestoreSettingsDisplay();if(ReferenceEquals(CGSettingsRuntime.Preview,settingsDraft)){CGSettingsRuntime.Preview=null;CGSettingsRuntime.Audio();}}}
    void SettingsSection(int section){settingsSection=(section+4)%4;selection=4;settingsNotice="";settingsChangedAt=CGMenuTheme.Clock;settingsAxis=0;menuNavigation.Reset();CGAudio.Menu();}
    bool SettingsToggle(int row){return settingsSection==0&&row==3||settingsSection==1&&row==0||settingsSection==2&&row!=1;}
    void SettingsChange(int row,int direction){
        var s=settingsDraft.options;
        if(settingsSection==0){
            if(row==0)s.screenMode=(s.screenMode+direction+3)%3;
            if(row==1){int index=settingsResolutions.IndexOf(new Vector2Int(s.width,s.height));var res=settingsResolutions[(index+direction+settingsResolutions.Count)%settingsResolutions.Count];s.width=res.x;s.height=res.y;}
            if(row==2)s.brightness=Mathf.Clamp(Mathf.Round((s.brightness+direction*.05f)*20)/20,.55f,1.2f);
            if(row==3)s.verticalSync=!s.verticalSync;
        }else if(settingsSection==1){
            if(row==0)s.muted=!s.muted;
            if(row==1)settingsDraft.masterVolume=Mathf.Clamp01(settingsDraft.masterVolume+direction*.05f);
            if(row==2)s.musicVolume=Mathf.Clamp01(s.musicVolume+direction*.05f);
            if(row==3)s.effectsVolume=Mathf.Clamp01(s.effectsVolume+direction*.05f);
            if(row==4)s.interfaceVolume=Mathf.Clamp01(s.interfaceVolume+direction*.05f);
        }else if(settingsSection==2){
            if(row==0)settingsDraft.showHints=!settingsDraft.showHints;
            if(row==1)s.hintSize=(s.hintSize+direction+3)%3;
            if(row==2)s.solidText=!s.solidText;
            if(row==3)settingsDraft.reduceFlash=!settingsDraft.reduceFlash;
        }else if(row==0)LeaveSettings(1);else OpenSettingsDialog(4);
        SyncSettingsPreview();
    }
    void SettingsAction(int index){
        if(index<4){SettingsSection(index);return;}
        CGAudio.Menu(true);
        if(index<4+SettingsRows){SettingsChange(index-4,1);return;}
        if(index<SettingsFooter){int sample=index-4-SettingsRows;if(sample==3){CGAudio.Menu(true);settingsNotice="Escuchando: Menú";}else settingsNotice=CGAudio.Play(SettingsClips[sample])?"Escuchando: "+SettingsSamples[sample]:"Este sonido todavía no está disponible.";return;}
        if(index==SettingsFooter)LeaveSettings(0);else if(index==SettingsFooter+1)OpenSettingsDialog(2);else RequestSettingsSave();
    }
    void HandleSettings(){
        if(settingsDraft==null)BeginSettings();var k=Keyboard.current;var p=Gamepad.current;
        if(settingsDialog!=0){
            if(settingsDialog==3&&Time.unscaledTime>=settingsConfirmUntil){FinishSettingsDialog(false);return;}
            if(CGControl.Back()){FinishSettingsDialog(false);return;}
            if(!settingsDialogReady){bool held=k!=null&&(k.enterKey.isPressed||k.spaceKey.isPressed||k.upArrowKey.isPressed||k.downArrowKey.isPressed||k.leftArrowKey.isPressed||k.rightArrowKey.isPressed||k.wKey.isPressed||k.sKey.isPressed);held|=p!=null&&(p.buttonSouth.isPressed||p.buttonEast.isPressed||p.dpad.ReadValue().sqrMagnitude>.1f||p.leftStick.ReadValue().sqrMagnitude>.1f);settingsDialogReady=!held;return;}
            int choice=menuNavigation.Read(settingsChoice,2,false);if(k!=null&&(k.leftArrowKey.wasPressedThisFrame||k.rightArrowKey.wasPressedThisFrame)||p!=null&&(p.dpad.left.wasPressedThisFrame||p.dpad.right.wasPressedThisFrame))choice=1-choice;settingsChoice=choice;
            if(CGControl.Confirm())FinishSettingsDialog(settingsChoice==0);return;
        }
        if(k!=null&&k.tabKey.wasPressedThisFrame){SettingsSection(settingsSection+(k.shiftKey.isPressed?-1:1));return;}
        if(p!=null&&(p.leftShoulder.wasPressedThisFrame||p.rightShoulder.wasPressedThisFrame)){SettingsSection(settingsSection+(p.leftShoulder.wasPressedThisFrame?-1:1));return;}
        selection=menuNavigation.Read(selection,SettingsFooter+3,false);
        float x=0;if(k!=null){if(k.leftArrowKey.isPressed)x=-1;if(k.rightArrowKey.isPressed)x=1;}if(p!=null&&Mathf.Abs(x)<.1f){x=p.dpad.ReadValue().x;if(Mathf.Abs(x)<.55f)x=p.leftStick.ReadValue().x;}
        int direction=x>.55f?1:x<-.55f?-1:0;if(direction==0)settingsAxis=0;else if(direction!=settingsAxis||Time.unscaledTime>=settingsRepeat){bool first=settingsAxis!=direction;settingsAxis=direction;settingsRepeat=Time.unscaledTime+(first?.34f:.12f);if(selection>=4&&selection<4+SettingsRows&&settingsSection!=3&&(first||!SettingsToggle(selection-4)))SettingsChange(selection-4,direction);}
        if(CGControl.Confirm())SettingsAction(selection);else if(CGControl.Back())LeaveSettings(0);
    }
    void LeaveSettings(int destination){settingsLeaveTarget=destination;if(SettingsDirty)OpenSettingsDialog(1);else SettingsNavigate();}
    void SettingsNavigate(){CGSettingsRuntime.Preview=null;CGSettingsRuntime.Audio();if(settingsLeaveTarget==1)OpenUIScreen("SC_Controls",Page.Controls);else ToMain();}
    bool SettingsDisplayChanged {get{return settingsDraft.options.width!=settingsBaseline.options.width||settingsDraft.options.height!=settingsBaseline.options.height||settingsDraft.options.screenMode!=settingsBaseline.options.screenMode;}}
    void RequestSettingsSave(){
        if(!SettingsDirty){settingsNotice="Todo está guardado.";return;}
        if(SettingsDisplayChanged&&CGSettingsRuntime.CanChangeDisplay){displayBackup=new CGSettingsOptions{width=Screen.width,height=Screen.height,screenMode=CGSettingsRuntime.ModeIndex(Screen.fullScreenMode)};displayTrial=true;CGSettingsRuntime.Display(settingsDraft.options);OpenSettingsDialog(3);settingsConfirmUntil=Time.unscaledTime+15;return;}
        SaveSettings();
    }
    bool SaveSettings(){
        var p=CGStore.Data;var original=p.settings;float master=p.masterVolume;bool flashes=p.reduceFlash,hints=p.subtitles;
        p.settings=settingsDraft.options.Copy();if(!SettingsDisplayChanged){p.settings.width=original.width;p.settings.height=original.height;p.settings.screenMode=original.screenMode;}p.masterVolume=settingsDraft.masterVolume;p.reduceFlash=settingsDraft.reduceFlash;p.subtitles=settingsDraft.showHints;
        if(!CGStore.Save()){p.settings=original;p.masterVolume=master;p.reduceFlash=flashes;p.subtitles=hints;if(displayTrial)RestoreSettingsDisplay();settingsNotice="No se pudo guardar. Tus cambios siguen aquí para intentarlo de nuevo.";return false;}
        displayTrial=false;settingsBaseline=settingsDraft.Copy();CGSettingsRuntime.ApplySaved();settingsNotice="Cambios guardados.";CGAudio.Menu(true);return true;
    }
    void RestoreSettingsDisplay(){if(displayBackup!=null){CGSettingsRuntime.Display(displayBackup);settingsDraft.options.width=displayBackup.width;settingsDraft.options.height=displayBackup.height;settingsDraft.options.screenMode=displayBackup.screenMode;var res=new Vector2Int(displayBackup.width,displayBackup.height);if(!settingsResolutions.Contains(res))settingsResolutions.Add(res);}displayTrial=false;}
    void FinishSettingsDialog(bool accept){
        int dialog=settingsDialog;settingsDialog=0;
        if(dialog==1&&accept){settingsDraft=settingsBaseline.Copy();CGSettingsRuntime.Preview=null;CGSettingsRuntime.Audio();SettingsNavigate();}
        else if(dialog==2&&accept){var screen=settingsDraft.options;settingsDraft=new CGSettingsDraft();settingsDraft.options.width=screen.width;settingsDraft.options.height=screen.height;settingsDraft.options.screenMode=screen.screenMode;settingsNotice="Valores originales preparados. Pulsa Guardar cambios para conservarlos.";SyncSettingsPreview();}
        else if(dialog==3){if(accept)SaveSettings();else {RestoreSettingsDisplay();settingsNotice="Se recuperó la pantalla anterior.";}}
        else if(dialog==4&&accept){
            var old=CGStore.Data;var fresh=new CGProgress{masterVolume=old.masterVolume,reduceFlash=old.reduceFlash,subtitles=old.subtitles,settings=old.settings.Copy(),actionKeys=(string[])old.actionKeys.Clone(),padBindings=(string[])old.padBindings.Clone()};CGStore.Data=fresh;
            if(!CGStore.Save()){CGStore.Data=old;settingsNotice="No se pudo borrar el avance.";}else settingsNotice="Avance borrado. Se conservaron tus opciones y botones.";
        }
        menuNavigation.Reset();
    }
    void SettingsLabel(Rect r,string text,int size=22,Color? color=null,bool body=false,TextAnchor align=TextAnchor.MiddleLeft){MapLabel(r,text,size,color,body,align);}
    void SettingsHeading(Rect r,string text,int size,TextAnchor align=TextAnchor.MiddleLeft){var style=MapText(size,align,Color.white);style.font=CGMenuTheme.Title;CGUIDraw.Text(r,text,style);}
    Color SettingsAccent {get{return SettingsDesign!=null&&!SettingsDesign.useCampusPalette?SettingsDesign.accent:CGMenuTheme.Design!=null?CGMenuTheme.Design.cyan:new Color(.3f,.87f,1);}}
    Color SettingsActive {get{return SettingsDesign!=null&&!SettingsDesign.useCampusPalette?SettingsDesign.active:CGMenuTheme.Design!=null?CGMenuTheme.Design.amber:new Color(1,.56f,.12f);}}
    Color SettingsInk {get{var color=SettingsDesign!=null&&!SettingsDesign.useCampusPalette?SettingsDesign.panel:CGMenuTheme.Design!=null?CGMenuTheme.Design.ink:new Color(.025f,.04f,.085f);color.a=1;return color;}}
    void SettingsPanel(Rect r){CGSettingsArt.Frame(r,SettingsActive,SettingsInk);}
    bool SettingsHit(Rect r,int index){if(settingsDialog!=0)return false;if(!editorMenuPreview&&pointerMoved&&r.Contains(Event.current.mousePosition))selection=index;if(Click(r,"",GUIStyle.none)){selection=index;return true;}return false;}
    void SettingsButton(Rect r,string text,int index){bool active=selection==index,primary=index==SettingsFooter+2;CGSettingsArt.Frame(r,active||primary?SettingsActive:SettingsAccent,active||primary?Color.Lerp(SettingsInk,SettingsActive,active?.3f:.19f):SettingsInk);SettingsLabel(r,text,primary?26:24,null,false,TextAnchor.MiddleCenter);if(SettingsHit(r,index))SettingsAction(index);}
    void SettingsRow(Rect r,string label,string value,int row,bool toggle=false,bool enabled=false,float slider=-1){
        int index=4+row;bool active=selection==index;CGSettingsArt.Frame(r,active?SettingsActive:Color.Lerp(SettingsInk,SettingsAccent,.55f),active?Color.Lerp(SettingsInk,SettingsActive,.12f):SettingsInk);if(active&&!settingsDraft.reduceFlash&&(SettingsDesign==null||SettingsDesign.animateFocus)){float t=Mathf.Repeat(CGMenuTheme.Clock*.28f,1);RectColor(new Rect(r.x+14,r.yMax-4,(r.width-28)*(.2f+.55f*t),2),SettingsActive);}int icon=settingsSection==0?(row==2?4:0):settingsSection==1?(row>1?6:1):row==1?7:row==3?5:2;CGUIDraw.Texture(new Rect(r.x+14,r.center.y-15,30,30),CGSettingsArt.Icon(icon));SettingsLabel(new Rect(r.x+54,r.y+3,234,r.height-6),label,22);
        var valueRect=new Rect(r.xMax-244,r.y+6,230,r.height-12);
        if(toggle){var previous=CGUIDraw.Color;CGUIDraw.Color=enabled?SettingsActive:Color.Lerp(SettingsInk,SettingsAccent,.2f);CGUIDraw.Texture(new Rect(r.xMax-91,r.center.y-15,69,30),CGSettingsArt.Track);CGUIDraw.Color=previous;CGUIDraw.Texture(new Rect(r.xMax-(enabled?49:89),r.center.y-14,28,28),CGSettingsArt.Knob);SettingsLabel(new Rect(r.xMax-220,r.y+3,118,r.height-6),enabled?"Activado":"Desactivado",18,null,false,TextAnchor.MiddleRight);}
        else if(slider>=0){var track=new Rect(r.xMax-245,r.center.y-5,155,10);var previous=CGUIDraw.Color;CGUIDraw.Color=Color.Lerp(SettingsInk,SettingsAccent,.4f);CGUIDraw.Texture(track,CGSettingsArt.Track);CGUIDraw.Color=SettingsActive;if(slider>0)CGUIDraw.Texture(new Rect(track.x,track.y,track.width*slider,10),CGSettingsArt.Track);CGUIDraw.Color=previous;CGUIDraw.Texture(new Rect(track.x+slider*track.width-12,track.center.y-12,24,24),CGSettingsArt.Knob);SettingsLabel(new Rect(r.xMax-80,r.y+3,65,r.height-6),value,21,null,false,TextAnchor.MiddleRight);
            if(!editorMenuPreview&&settingsDialog==0){float next=GUI.HorizontalSlider(new Rect(track.x-4,r.y+7,track.width+8,r.height-14),slider,0,1,GUIStyle.none,GUIStyle.none);if(Mathf.Abs(next-slider)>.001f){selection=index;SetSettingsSlider(row,next);}}
        }else {CGSettingsArt.Frame(valueRect,active?SettingsActive:SettingsAccent,SettingsInk);SettingsLabel(valueRect,"‹  "+value+"  ›",21,null,false,TextAnchor.MiddleCenter);if(settingsDialog==0){var a=new Rect(valueRect.x,valueRect.y,32,valueRect.height);var b=new Rect(valueRect.xMax-32,valueRect.y,32,valueRect.height);if(Click(a,"",GUIStyle.none)){selection=index;SettingsChange(row,-1);CGAudio.Menu();return;}if(Click(b,"",GUIStyle.none)){selection=index;SettingsChange(row,1);CGAudio.Menu();return;}}}
        if(slider<0&&SettingsHit(r,index))SettingsAction(index);
        if(slider>=0&&!editorMenuPreview&&settingsDialog==0&&pointerMoved&&r.Contains(Event.current.mousePosition))selection=index;
    }
    void SetSettingsSlider(int row,float value){if(settingsSection==0)settingsDraft.options.brightness=.55f+value*.65f;else if(row==1)settingsDraft.masterVolume=value;else if(row==2)settingsDraft.options.musicVolume=value;else if(row==3)settingsDraft.options.effectsVolume=value;else if(row==4)settingsDraft.options.interfaceVolume=value;SyncSettingsPreview();}
    void DrawSettings(){
        if(settingsDraft==null)BeginSettings();var design=SettingsDesign;var side=design!=null?design.sidebar:new Rect(24,147,211,471);var content=design!=null?design.content:new Rect(251,147,1005,471);
        RectColor(new Rect(0,0,1280,112),new Color(.015f,.015f,.05f,.97f));SettingsHeading(new Rect(30,13,660,48),"CAMPUS GLITCH",35);SettingsLabel(new Rect(32,65,700,29),"UNIFRANZ LA PAZ · APRENDE HACIENDO",21);SettingsHeading(new Rect(790,22,451,48),design!=null?design.title:"CONFIGURACIÓN",28,TextAnchor.MiddleRight);SettingsLabel(new Rect(790,73,451,26),design!=null?design.subtitle:"AJUSTA EL CAMPUS A TU MANERA",17,new Color(.75f,.85f,.97f),false,TextAnchor.MiddleRight);RectColor(new Rect(0,110,1280,3),SettingsAccent);
        SettingsPanel(side);SettingsPanel(content);
        for(int i=0;i<4;i++){var r=new Rect(side.x+10,side.y+17+i*58,side.width-20,48);bool current=settingsSection==i;CGSettingsArt.Frame(r,current?SettingsActive:SettingsAccent,current?Color.Lerp(SettingsInk,SettingsActive,.3f):SettingsInk);CGUIDraw.Texture(new Rect(r.x+10,r.y+10,28,28),CGSettingsArt.Icon(i));SettingsLabel(new Rect(r.x+47,r.y,r.width-54,r.height),design!=null&&design.sections!=null&&design.sections.Length>i?design.sections[i]:SettingsSections[i],24);if(editorMenuPreview?settingsSection==i:selection==i)Outline(new Rect(r.x+6,r.y+5,r.width-12,r.height-10),Color.white,1);if(SettingsHit(r,i))SettingsSection(i);}
        CGGuardianMenuArt.Draw(new Rect(side.x+29,side.y+268,side.width-58,158),CGStore.Data.purpleEquipped);SettingsLabel(new Rect(side.x+7,side.yMax-38,side.width-14,29),"TU CAMPUS · TU ESTILO",18,new Color(1,.64f,.26f),false,TextAnchor.MiddleCenter);
        CGSettingsArt.Frame(new Rect(content.x+12,content.y+10,content.width-24,46),SettingsAccent,Color.Lerp(SettingsInk,SettingsAccent,.08f));CGUIDraw.Texture(new Rect(content.x+24,content.y+17,30,30),CGSettingsArt.Icon(settingsSection));SettingsLabel(new Rect(content.x+68,content.y+10,560,46),SettingsSections[settingsSection].ToUpperInvariant(),30,new Color(1,.64f,.26f));SettingsLabel(new Rect(content.xMax-310,content.y+14,285,36),SettingsDirty?"CAMBIOS SIN GUARDAR":"OPCIONES GUARDADAS",18,SettingsDirty?new Color(1,.67f,.25f):new Color(.4f,1,.7f),false,TextAnchor.MiddleRight);
        float entry=editorMenuPreview||settingsDraft.reduceFlash?1:Mathf.Clamp01((CGMenuTheme.Clock-settingsChangedAt)/(design!=null?design.sectionTransition:.2f));float y=content.y+73+(1-entry)*8;
        var left=new Rect(content.x+20,y,548,330);var right=new Rect(content.x+590,y,391,330);var s=settingsDraft.options;
        if(settingsSection==0){
            SettingsRow(new Rect(left.x,left.y,left.width,64),"Modo de pantalla",s.screenMode==0?"Ventana":s.screenMode==1?"Completa sin bordes":"Pantalla completa",0);
            SettingsRow(new Rect(left.x,left.y+76,left.width,64),"Resolución",s.width+" × "+s.height,1);
            SettingsRow(new Rect(left.x,left.y+152,left.width,64),"Brillo del ambiente",Mathf.RoundToInt(s.brightness*100)+"%",2,false,false,(s.brightness-.55f)/.65f);
            SettingsRow(new Rect(left.x,left.y+228,left.width,64),"Sincronizar imagen","",3,true,s.verticalSync);
            DrawSettingsExample(right);
        }else if(settingsSection==1){
            SettingsRow(new Rect(left.x,left.y,left.width,49),"Silenciar todo","",0,true,s.muted);
            float[] values={settingsDraft.masterVolume,s.musicVolume,s.effectsVolume,s.interfaceVolume};string[] labels={"Volumen general","Música","Sonidos del juego","Sonidos de botones"};for(int i=0;i<4;i++)SettingsRow(new Rect(left.x,left.y+60+i*56,left.width,49),labels[i],Mathf.RoundToInt(values[i]*100)+"%",i+1,false,false,values[i]);
            DrawSettingsAudio(right);
        }else if(settingsSection==2){
            SettingsRow(new Rect(left.x,left.y,left.width,64),"Consejos en pantalla","",0,true,settingsDraft.showHints);SettingsRow(new Rect(left.x,left.y+76,left.width,64),"Tamaño de avisos",new[]{"Pequeño","Mediano","Grande"}[s.hintSize],1);SettingsRow(new Rect(left.x,left.y+152,left.width,64),"Fondos de texto sólidos","",2,true,s.solidText);SettingsRow(new Rect(left.x,left.y+228,left.width,64),"Reducir destellos","",3,true,settingsDraft.reduceFlash);DrawSettingsExample(right);
        }else {
            SettingsButton(new Rect(left.x,left.y,left.width,65),"ELEGIR TECLAS Y BOTONES",4);SettingsButton(new Rect(left.x,left.y+88,left.width,65),"BORRAR AVANCE…",5);
            SettingsLabel(new Rect(left.x+8,left.y+181,left.width-16,103),"Borrar el avance permite comenzar de nuevo. Tus opciones de sonido, pantalla y botones se conservan. Siempre te pediremos confirmación.",23,null,true);
            DrawSettingsExample(right);
        }
        string hint=settingsNotice;if(string.IsNullOrEmpty(hint))hint=settingsSection==1?"Puedes escuchar y ajustar el sonido antes de guardar.":settingsSection==2?"Las conversaciones y las opciones del juego siguen disponibles.":settingsSection==0&&Application.isEditor?"El modo y la resolución se aplicarán al abrir el juego exportado.":"Elige cómo quieres ver el campus.";
        CGSettingsArt.Frame(new Rect(content.x+16,content.yMax-61,content.width-32,49),Color.Lerp(SettingsInk,SettingsAccent,.5f),SettingsInk);SettingsLabel(new Rect(content.x+30,content.yMax-58,content.width-60,43),hint,19,new Color(.81f,.83f,1),true);
        SettingsButton(new Rect(24,644,240,48),"VOLVER",SettingsFooter);SettingsButton(new Rect(400,644,352,48),"RESTABLECER OPCIONES",SettingsFooter+1);SettingsButton(new Rect(773,644,483,48),"GUARDAR CAMBIOS",SettingsFooter+2);
        RectColor(new Rect(0,702,1280,18),new Color(.01f,.015f,.04f,1));SettingsLabel(new Rect(24,698,1230,22),"DIRECCIONES / STICK: ELEGIR · IZQ / DER: AJUSTAR · LB / RB / TAB: APARTADO · A / ENTER: ACEPTAR · B / ESC: VOLVER",14,null,false,TextAnchor.MiddleCenter);
        if(settingsDialog!=0)DrawSettingsDialog();
    }
    void DrawSettingsExample(Rect r){
        SettingsPanel(r);SettingsLabel(new Rect(r.x+12,r.y+9,r.width-104,32),"ASÍ SE VE AL JUGAR",22,SettingsActive);var picture=new Rect(r.x+12,r.y+49,r.width-24,(r.width-24)*9/16);var art=CGSettingsWorldView.Get(this);if(art!=null)CGUIDraw.Texture(picture,art,ScaleMode.ScaleToFit);DrawSettingsBrightness(picture,settingsDraft.options.brightness);SettingsLabel(new Rect(r.xMax-86,r.y+12,72,26),"NIVEL "+(CGSettingsWorldView.Level+1),16,SettingsAccent,false,TextAnchor.MiddleRight);
        SettingsLabel(new Rect(r.x+15,r.y+266,r.width-30,51),settingsDraft.showHints?"¡Bienvenido al campus!\nExplora nuevos caminos.":"Las conversaciones con personajes siempre se muestran.",new[]{18,21,24}[settingsDraft.options.hintSize],null,true,TextAnchor.MiddleCenter);
    }
    void DrawSettingsAudio(Rect r){
        CGSettingsWorldView.Visible=false;
        SettingsPanel(r);SettingsLabel(new Rect(r.x+14,r.y+7,r.width-28,34),"PRUEBA DE SONIDO",24,new Color(1,.65f,.25f));if(!editorMenuPreview)CGAudio.Spectrum(settingsSpectrum);
        for(int i=0;i<32;i++){float h=Mathf.Max(3,settingsSpectrum[i]*56);RectColor(new Rect(r.x+16+i*(r.width-32)/32,r.y+105-h,(r.width-37)/32,h),i%3==0?new Color(1,.56f,.12f):SettingsAccent);}
        for(int i=0;i<5;i++)SettingsButton(new Rect(r.x+12,r.y+119+i*40,r.width-24,34),"▷  "+SettingsSamples[i],4+SettingsRows+i);
    }
    void DrawSettingsDialog(){
        RectColor(new Rect(0,0,1280,720),new Color(.005f,.008f,.025f,.86f));var r=new Rect(293,212,694,299);SettingsPanel(r);
        string title=settingsDialog==1?"¿SALIR SIN GUARDAR?":settingsDialog==2?"¿RESTABLECER LAS OPCIONES?":settingsDialog==3?"¿SE VE BIEN LA PANTALLA?":"¿BORRAR EL AVANCE?";
        string detail=settingsDialog==1?"Puedes seguir ajustando tus opciones o volver con los valores que ya tenías guardados.":settingsDialog==2?"Se prepararán los valores originales de sonido y lectura. Tus niveles y botones se conservan.":settingsDialog==3?"Confirma en "+Mathf.CeilToInt(settingsConfirmUntil-Time.unscaledTime)+" segundos. Si no respondes, recuperaremos la pantalla anterior.":"Se borrarán los niveles completados y los poderes desbloqueados. Tus opciones y botones se conservarán.";
        SettingsLabel(new Rect(317,230,646,53),title,31,new Color(1,.65f,.25f),false,TextAnchor.MiddleCenter);SettingsLabel(new Rect(334,299,612,101),detail,23,null,true,TextAnchor.MiddleCenter);
        string yes=settingsDialog==1?"SALIR SIN GUARDAR":settingsDialog==2?"RESTABLECER":settingsDialog==3?"MANTENER":"BORRAR AVANCE";string no=settingsDialog==1?"SEGUIR AJUSTANDO":settingsDialog==3?"RECUPERAR":"CANCELAR";
        for(int i=0;i<2;i++){var b=new Rect(317+i*330,432,316,53);CGUIDraw.Texture(b,CGMenuArtwork.Button(settingsChoice==i,SettingsAccent));SettingsLabel(b,i==0?yes:no,23,null,false,TextAnchor.MiddleCenter);if(Click(b,"",GUIStyle.none)){settingsChoice=i;FinishSettingsDialog(i==0);}}
    }
    void DrawSettingsBrightness(Rect r,float brightness){if(brightness<1)RectColor(r,new Color(0,0,0,(1-brightness)*.7f));else if(brightness>1)RectColor(r,new Color(1,1,1,(brightness-1)*.35f));}
#if UNITY_EDITOR
    public void PreviewSettingsSection(int section){if(settingsDraft==null)BeginSettings();settingsSection=Mathf.Clamp(section,0,3);settingsDialog=0;settingsChangedAt=CGMenuTheme.Clock;}
#endif
}
}
