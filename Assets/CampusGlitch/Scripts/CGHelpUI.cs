using UnityEngine;
using UnityEngine.InputSystem;
namespace CampusGlitch {
public partial class CGGame {
    CGHelpDesign helpDesign;int helpSection,helpAxis;readonly int[] helpItems=new int[5];bool helpPad,helpTutorial;float helpRepeat,helpChangedAt,helpTutorialAt;
    static readonly string[] HelpSections={"Cómo jugar","Objetivos","Iconos del juego","Personajes","Guardado"};
    static readonly string[] HelpKinds={"Guardia_U","Estudiante","Tecnica_Lab","Recepcionista","Director","Docente","Noemi_Moller","Unifranzito"};
    static readonly string[] HelpPeople={"GUARDIA U","ESTUDIANTE","TÉCNICA DE LAB","RECEPCIÓN","DIRECCIÓN","DOCENTE","COORDINACIÓN","GUÍA DE BIBLIOTECA"};
    static readonly string[] HelpPeopleText={"Te da la bienvenida al acceso principal y te orienta al comenzar el recorrido.","Comparte pistas sobre los tres fragmentos y las plataformas del campus.","Te indica dónde está la terminal y cómo recuperar la conexión.","Te orienta por el lobby y los servicios del campus.","Te acompaña en los recorridos de oficinas, aulas y pasillos.","Te orienta por los laboratorios y sus retos de aprendizaje.","Te acompaña en Ingeniería de Sistemas y en el tramo final del campus.","Te anima a explorar la biblioteca y compartir lo aprendido."};
    static readonly string[] HelpSymbols={"TRES FRAGMENTOS","PUNTO DE REGRESO","TERMINAL","PORTAL","ENEMIGOS","PODER EQUIPADO"};
    static readonly string[] HelpSymbolText={"Recupera tres fragmentos para abrir el portal.","Actívalo con {interact}. Volverás cerca de este punto si caes durante la partida.","Abre con {interact}. Elige una respuesta y confirma. Si fallas, puedes intentarlo de nuevo.","Reúne los datos, resuelve la terminal y vence al jefe si lo hay. Después pulsa {interact} en la salida.","Observa sus movimientos, evita el contacto y usa {attack} o tu poder para defenderte.","El indicador muestra la energía, el coste y el tiempo de espera del poder. Actívalo con {ability}."};
    CGHelpDesign HelpDesign {get{if(helpDesign==null)helpDesign=Resources.Load<CGHelpDesign>("CG/Presentation/CampusHelp");return helpDesign;}}
    Color HelpInk {get{return HelpDesign!=null?HelpDesign.panel:new Color(.025f,.04f,.085f,1);}}
    int HelpCount {get{return helpSection==0?HelpDesign.steps.Length:helpSection==1?10:helpSection==2?6:helpSection==3?8:HelpDesign.questions.Length;}}
    void BeginHelp(){helpSection=0;for(int i=0;i<helpItems.Length;i++)helpItems[i]=0;selection=0;helpTutorial=false;helpPad=Gamepad.current!=null;helpChangedAt=CGMenuTheme.Clock;menuNavigation.Reset();}
    string HelpBinding(int action){return helpPad?PadCaption(CGStore.Data.padBindings[action]):KeyboardCaption(CGStore.Data.actionKeys[action]);}
    string HelpText(string text){return text.Replace("{move}",HelpBinding(0)+" / "+HelpBinding(1)).Replace("{run}",HelpBinding(3)).Replace("{jump}",HelpBinding(2)).Replace("{attack}",HelpBinding(4)).Replace("{interact}",HelpBinding(5)).Replace("{ability}",HelpBinding(6)).Replace("{pause}",HelpBinding(7)).Replace("{confirm}",helpPad?PadCaption("buttonSouth"):"ENTER");}
    void HelpSection(int section){helpSection=(section+5)%5;selection=helpSection;helpTutorial=false;CGSettingsWorldView.Visible=helpSection==0||helpSection==4;helpChangedAt=CGMenuTheme.Clock;menuNavigation.Reset();CGAudio.Menu();}
    void HelpStep(int direction){helpItems[helpSection]=(helpItems[helpSection]+direction+HelpCount)%HelpCount;helpChangedAt=CGMenuTheme.Clock;helpTutorialAt=CGMenuTheme.Clock;CGAudio.Menu();}
    void HelpAction(int item){if(item<5)HelpSection(item);else if(item==5)OpenUIScreen("SC_Controls",Page.Controls);else if(item==6)OpenUIScreen("SC_LevelMap",Page.Map);else if(item==7)ToMain();else if(item==8||item==9){helpTutorial=false;HelpStep(item==8?-1:1);}else {HelpSection(0);helpItems[0]=0;helpTutorial=true;helpTutorialAt=CGMenuTheme.Clock;selection=10;}}
    void HandleHelp(){
        if(HelpDesign==null){if(CGControl.Back())ToMain();return;}SyncControlDevice();CGSettingsWorldView.Tick();
        if(helpTutorial&&CGMenuTheme.Clock-helpTutorialAt>=HelpDesign.tutorialSeconds){if(helpItems[0]>=HelpDesign.steps.Length-1)helpTutorial=false;else HelpStep(1);}
        var k=Keyboard.current;var p=Gamepad.current;if(k!=null&&k.anyKey.wasPressedThisFrame)helpPad=false;if(p!=null){foreach(string b in CGControl.GamepadButtons)if(CGControl.PadButton(b)?.wasPressedThisFrame==true){helpPad=true;break;}if(p.leftStick.ReadValue().sqrMagnitude>.2f)helpPad=true;}
        if(k!=null&&k.tabKey.wasPressedThisFrame){HelpSection(helpSection+(k.shiftKey.isPressed?-1:1));return;}if(p!=null&&(p.leftShoulder.wasPressedThisFrame||p.rightShoulder.wasPressedThisFrame)){HelpSection(helpSection+(p.leftShoulder.wasPressedThisFrame?-1:1));return;}
        selection=menuNavigation.Read(selection,11,false);float x=0;if(k!=null){if(k.leftArrowKey.isPressed)x=-1;if(k.rightArrowKey.isPressed)x=1;}if(p!=null&&Mathf.Abs(x)<.1f){x=p.dpad.ReadValue().x;if(Mathf.Abs(x)<.55f)x=p.leftStick.ReadValue().x;}
        int direction=x>.55f?1:x<-.55f?-1:0;if(direction==0)helpAxis=0;else if(direction!=helpAxis||Time.unscaledTime>=helpRepeat){bool first=direction!=helpAxis;helpAxis=direction;helpRepeat=Time.unscaledTime+(first?.34f:.16f);helpTutorial=false;HelpStep(direction);}
        if(CGControl.Confirm())HelpAction(selection);else if(CGControl.Back()){if(helpTutorial){helpTutorial=false;CGAudio.Menu();}else ToMain();}
    }
    void HelpLabel(Rect r,string text,int size=22,Color? color=null,bool body=false,TextAnchor align=TextAnchor.MiddleLeft){MapLabel(r,text,size,color,body,align);}
    void HelpPanel(Rect r,bool primary=false){CGSettingsArt.Frame(r,primary?MapAmber:MapCyan,HelpInk);}
    bool HelpHit(Rect r,int item){if(!editorMenuPreview&&pointerMoved&&r.Contains(Event.current.mousePosition))selection=item;if(Click(r,"",GUIStyle.none)){selection=item;CGAudio.Menu(true);return true;}return false;}
    void HelpButton(Rect r,string label,int item){
        bool focused=!editorMenuPreview&&selection==item;float focus=editorMenuPreview||CGSettingsRuntime.ReduceFlash?(focused?1:0):menuMotion.Focus(item);
        CGSettingsArt.Frame(r,Color.Lerp(MapCyan,MapAmber,focus),Color.Lerp(HelpInk,MapAmber,focus*.22f));HelpLabel(r,label,23,null,false,TextAnchor.MiddleCenter);
        if(focused)RectColor(new Rect(r.x+15,r.yMax-5,(r.width-30)*(.65f+.35f*focus),2),MapAmber);
        if(HelpHit(r,item))HelpAction(item);
    }
    void DrawHelp(){
        var d=HelpDesign;if(d==null){Header("AYUDA");Label("Cargando la guía del campus…",300);return;}
        RectColor(new Rect(0,0,1280,110),new Color(.018f,.028f,.065f,1));var title=MapText(30);title.font=CGMenuTheme.Title;CGUIDraw.Text(new Rect(30,12,650,44),"CAMPUS GLITCH",title);HelpLabel(new Rect(32,64,650,28),"UNIFRANZ LA PAZ · APRENDE HACIENDO",20);HelpLabel(new Rect(772,16,480,45),d.title,31,MapAmber,false,TextAnchor.MiddleRight);HelpLabel(new Rect(772,67,480,27),d.subtitle,17,null,false,TextAnchor.MiddleRight);RectColor(new Rect(0,109,1280,3),MapAmber);
        HelpPanel(new Rect(24,134,220,488),true);for(int i=0;i<5;i++){var r=new Rect(34,146+i*64,200,51);CGSettingsArt.Frame(r,i==helpSection?MapAmber:MapCyan,i==helpSection?Color.Lerp(HelpInk,MapAmber,.18f):HelpInk);CGUIDraw.Texture(new Rect(r.x+10,r.y+11,25,25),CGSettingsArt.Icon(i==0?3:i==1?5:i==2?4:i==3?2:6));HelpLabel(new Rect(r.x+44,r.y,r.width-48,r.height),d.sections!=null&&d.sections.Length>i?d.sections[i]:HelpSections[i],23);if((editorMenuPreview?helpSection:selection)==i)Outline(new Rect(r.x+6,r.y+4,r.width-12,r.height-8),Color.white,1);if(HelpHit(r,i))HelpSection(i);}
        CGGuardianMenuArt.Draw(new Rect(77,484,114,119),CGStore.Data.purpleEquipped);HelpPanel(new Rect(260,134,994,488),true);HelpLabel(new Rect(282,143,686,42),HelpSections[helpSection].ToUpperInvariant(),30,MapAmber);
        HelpLabel(new Rect(1016,150,214,28),helpPad?"MANDO":"TECLADO",19,MapCyan,false,TextAnchor.MiddleRight);
        int item=helpItems[helpSection];var left=new Rect(282,203,536,330);var right=new Rect(840,203,392,330);var tint=CGUIDraw.Color;var matrix=CGUIDraw.Matrix;float entry=editorMenuPreview||CGSettingsRuntime.ReduceFlash?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((CGMenuTheme.Clock-helpChangedAt)/d.transition));CGUIDraw.Color=new Color(tint.r,tint.g,tint.b,tint.a*entry);var shift=matrix;shift.m03+=(1-entry)*9*matrix.m00;CGUIDraw.Matrix=shift;
        if(helpSection==0)DrawHelpSteps(left,right,item);else if(helpSection==1)DrawHelpObjectives(left,right,item);else if(helpSection==2)DrawHelpSymbols(left,right,item);else if(helpSection==3)DrawHelpPeople(left,right,item);else DrawHelpSave(left,right,item);
        CGUIDraw.Color=tint;CGUIDraw.Matrix=matrix;HelpButton(new Rect(282,552,253,48),"ANTERIOR",8);HelpButton(new Rect(553,552,265,48),"SIGUIENTE",9);HelpButton(new Rect(840,552,392,48),helpTutorial?"TUTORIAL EN CURSO":"VER TUTORIAL",10);
        HelpButton(new Rect(24,643,360,48),"CONTROLES",5);HelpButton(new Rect(402,643,394,48),"MAPA DEL CAMPUS",6);HelpButton(new Rect(814,643,440,48),"VOLVER",7);
        RectColor(new Rect(0,703,1280,17),new Color(.012f,.02f,.045f,1));HelpLabel(new Rect(20,698,1240,22),"DIRECCIONES / STICK: ELEGIR · IZQ / DER: RECORRER · TAB / LB / RB: APARTADO · A / ENTER: ACEPTAR · B / ESC: VOLVER",14,null,false,TextAnchor.MiddleCenter);
    }
    void HelpScene(Rect r){var image=CGSettingsWorldView.Get(this);HelpPanel(r);if(image!=null)CGUIDraw.Texture(new Rect(r.x+9,r.y+9,r.width-18,r.height-18),image,ScaleMode.ScaleToFit);}
    void DrawHelpSteps(Rect left,Rect right,int item){
        for(int i=0;i<HelpDesign.steps.Length;i++){var r=new Rect(left.x,left.y+i*48,left.width,41);CGSettingsArt.Frame(r,i==item?MapAmber:new Color(.24f,.36f,.48f),HelpInk);HelpLabel(new Rect(r.x+12,r.y,32,r.height),(i+1).ToString(),22,MapAmber);HelpLabel(new Rect(r.x+52,r.y,r.width-62,r.height),HelpDesign.steps[i].title,22);if(Click(r,"",GUIStyle.none)){helpItems[0]=i;helpTutorial=false;helpChangedAt=CGMenuTheme.Clock;}}
        HelpScene(new Rect(right.x,right.y,right.width,200));HelpLabel(new Rect(right.x+13,right.y+204,right.width-26,26),"PASO "+(item+1)+" / "+HelpDesign.steps.Length,19,MapAmber);HelpLabel(new Rect(right.x+13,right.y+237,right.width-26,103),HelpText(HelpDesign.steps[item].body),19,null,true);
        if(helpTutorial&&!CGSettingsRuntime.ReduceFlash){float t=Mathf.Clamp01((CGMenuTheme.Clock-helpTutorialAt)/HelpDesign.tutorialSeconds);RectColor(new Rect(right.x+13,right.y+212,(right.width-26)*t,3),MapAmber);}
    }
    void DrawHelpObjectives(Rect left,Rect right,int item){
        HelpPanel(left);HelpLabel(new Rect(left.x+18,left.y+12,left.width-36,53),CGMenuTheme.MapNames[item],28,MapAmber);HelpLabel(new Rect(left.x+18,left.y+79,left.width-36,62),CGLevels.All[item].objective,23,null,true);
        string[] rows={"REÚNE LOS TRES FRAGMENTOS","RESUELVE LA TERMINAL",string.IsNullOrEmpty(CGLevels.All[item].boss)?"ABRE EL PORTAL":"VENCE AL JEFE Y ABRE EL PORTAL"};for(int i=0;i<3;i++)DrawMapActivity(new Rect(left.x+18,left.y+160+i*47,left.width-36,38),rows[i],false,i+1);
        HelpPanel(right);MapImage(new Rect(right.x+9,right.y+9,right.width-18,201),item);HelpLabel(new Rect(right.x+14,right.y+227,right.width-28,27),"AL COMPLETAR",20,MapCyan);HelpLabel(new Rect(right.x+14,right.y+265,right.width-28,60),MapReward(item),22,MapAmber);
    }
    Sprite HelpSymbol(int item){return item==0?CGArt.Get("01_Nuevos_Modulares/Global_UI/CoinIcon"):item==1?CGCampusVisual.Object(8):item==2?CGCampusVisual.Object(9):item==3?CGCampusVisual.Object(10):item==4?CGCampusActors.Frame(true,0,(int)(CGMenuTheme.Clock*7)%8):null;}
    void DrawHelpSymbols(Rect left,Rect right,int item){
        CGSettingsWorldView.Visible=false;HelpPanel(left);HelpLabel(new Rect(left.x+20,left.y+12,left.width-40,45),HelpSymbols[item],29,MapAmber);HelpLabel(new Rect(left.x+23,left.y+82,left.width-46,162),HelpText(HelpSymbolText[item]),25,null,true);
        HelpPanel(right);if(item==5)CGUIDraw.Texture(new Rect(right.x+81,right.y+47,230,200),CGAbilityArt.Card(CGStore.Data.equippedAbility),ScaleMode.ScaleToFit);else CGHelpArt.Sprite(new Rect(right.x+78,right.y+24,236,240),HelpSymbol(item),item==4);HelpLabel(new Rect(right.x+18,right.y+278,right.width-36,32),HelpSymbols[item],23,MapCyan,false,TextAnchor.MiddleCenter);
    }
    void DrawHelpPeople(Rect left,Rect right,int item){
        CGSettingsWorldView.Visible=false;for(int i=0;i<8;i++){var r=new Rect(left.x+i%4*136,left.y+i/4*159,128,148);CGSettingsArt.Frame(r,i==item?MapAmber:MapCyan,HelpInk);int row=CGCampusActors.NpcRow(HelpKinds[i]);CGHelpArt.Sprite(new Rect(r.x+20,r.y+10,88,95),CGCampusActors.Frame(false,row,(int)(CGMenuTheme.Clock*3)%4),true);HelpLabel(new Rect(r.x+7,r.y+111,r.width-14,31),HelpPeople[i],17,null,false,TextAnchor.MiddleCenter);if(Click(r,"",GUIStyle.none)){helpItems[3]=i;helpChangedAt=CGMenuTheme.Clock;}}
        HelpPanel(right);HelpLabel(new Rect(right.x+18,right.y+12,right.width-36,46),HelpPeople[item],27,MapAmber);HelpLabel(new Rect(right.x+19,right.y+72,right.width-38,133),HelpPeopleText[item],24,null,true);HelpLabel(new Rect(right.x+19,right.y+225,right.width-38,86),"Acércate y pulsa "+HelpBinding(5)+" para hablar.",22,MapCyan,true);
    }
    void DrawHelpSave(Rect left,Rect right,int item){
        HelpPanel(left);HelpLabel(new Rect(left.x+20,left.y+15,left.width-40,66),HelpDesign.questions[item].title,28,MapAmber);HelpLabel(new Rect(left.x+22,left.y+99,left.width-44,204),HelpText(HelpDesign.questions[item].body),25,null,true);HelpScene(new Rect(right.x,right.y,right.width,217));HelpLabel(new Rect(right.x+16,right.y+237,right.width-32,81),"Tu avance se conserva al completar cada nivel.",23,MapCyan,true,TextAnchor.MiddleCenter);
    }
#if UNITY_EDITOR
    public void PreviewHelp(int section,int item=0){helpSection=Mathf.Clamp(section,0,4);if(HelpDesign!=null)helpItems[helpSection]=Mathf.Clamp(item,0,HelpCount-1);CGSettingsWorldView.Visible=helpSection==0||helpSection==4;helpTutorial=false;helpChangedAt=CGMenuTheme.Clock;}
#endif
}
}
