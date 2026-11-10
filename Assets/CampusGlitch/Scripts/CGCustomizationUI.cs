using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
public partial class CGGame {
    int customSection,customAbility,customMilestone=1;bool customReady,draftPurple,draftClassic,inspectPurple,customDiscard;int draftAbility,discardChoice=1;
    float demonstrationAt=-99,secondJumpAt=-99;int demonstration=-1;bool impact,customPadControls;
    CGAbilityState demonstrationState=new CGAbilityState();
    static readonly string[] CustomSections={"APARIENCIA","HABILIDADES","EVOLUCIÓN"};
    bool CustomDirty {get{return draftPurple!=CGStore.Data.purpleEquipped||draftClassic!=CGStore.Data.useOriginalWolf||draftAbility!=CGStore.Data.equippedAbility;}}
    void BeginCustomization(){
        CGStore.Load();draftPurple=inspectPurple=CGStore.Data.purpleEquipped;draftClassic=CGStore.Data.useOriginalWolf;draftAbility=CGStore.Data.equippedAbility;customAbility=draftAbility;customReady=true;
    }
    void CustomCategory(int section){customSection=(section+3)%3;selection=0;menuNavigation.Reset();CGAudio.Menu();}
    void CustomLeave(){if(CustomDirty){customDiscard=true;discardChoice=1;}else ToMain();}
    bool ApplyCustomization(){
        if(!CustomDirty){Hint("Ya utilizas esta configuración.");return true;}
        if(draftPurple&&!CGStore.Data.purpleUnlocked){Hint("La variante morada requiere restaurar el nivel 2.");return false;}
        if(!CGAbilities.Unlocked(CGStore.Data,draftAbility)){Hint("Habilidad pendiente de desbloqueo.");return false;}
        string original=JsonUtility.ToJson(CGStore.Data);CGStore.Data.purpleEquipped=draftPurple;CGStore.Data.useOriginalWolf=draftClassic;CGStore.Data.equippedAbility=draftAbility;
        if(!CGStore.Save()){CGStore.Data=JsonUtility.FromJson<CGProgress>(original);Hint("No se pudo guardar. Tus cambios siguen disponibles para reintentar.");return false;}
        Hint("Personalización aplicada. Tu Guardián está preparado.");CGAudio.Menu(true);return true;
    }
    void DemoAbility(){
        float now=CGMenuTheme.Clock;
        if(customAbility==1&&demonstration==1&&now-demonstrationAt<1.3f&&now-demonstrationAt>.15f){
            if(demonstrationState.TryAirJump(true)){secondJumpAt=now;Hint("Segundo salto: un impulso adicional antes de aterrizar.");}return;
        }
        if(customAbility==1){demonstrationState.Land();}
        else if(!demonstrationState.TryActivate(customAbility,true,now,CGAbilities.Stats(CGStore.Data,customAbility,customSection==2?customMilestone:0))){Hint("Demostración recargando: "+demonstrationState.Remaining(customAbility,now).ToString("0.0")+" s");return;}
        demonstration=customAbility;demonstrationAt=now;secondJumpAt=-99;impact=false;CGAudio.Menu(true);
    }
    void CustomAction(int item){
        if(customSection==0){
            if(item<2){inspectPurple=item==1;if(!inspectPurple||CGStore.Data.purpleUnlocked)draftPurple=inspectPurple;else Hint("Puedes observar el morado. Se equipa tras restaurar el nivel 2.");}
            else if(item<4)draftClassic=item==3;
            else if(item==4)ApplyCustomization();else CustomLeave();
        }else if(customSection==1){
            if(item<4){customAbility=item;if(item==1)Hint("Doble salto es una habilidad pasiva; no ocupa la ranura activa.");else if(CGAbilities.Unlocked(CGStore.Data,item)){draftAbility=item;Hint("Habilidad seleccionada. Pulsa Aplicar para guardarla.");}else Hint(CGAbilities.Gate(item));}
            else if(item==4)DemoAbility();else if(item==5){if(demonstrationState.Absorb(CGMenuTheme.Clock)){impact=true;Hint("Escudo: impacto absorbido sin perder vida.");}else Hint("Activa la demostración del escudo para simular un impacto.");}
            else if(item==6)ApplyCustomization();else CustomLeave();
        }else if(item<9){customMilestone=item+1;customAbility=CGAbilities.RewardAbility[item];}
        else if(item==9)DemoAbility();else CustomLeave();
    }
    void HandleCustomization(){
        if(!customReady)BeginCustomization();var k=Keyboard.current;var p=Gamepad.current;
        if(customDiscard){discardChoice=menuNavigation.Read(discardChoice,2,false);if(CGControl.Confirm()){if(discardChoice==0){customDiscard=false;ToMain();}else customDiscard=false;}else if(CGControl.Back())customDiscard=false;return;}
        if((k!=null&&k.vKey.wasPressedThisFrame)||(p!=null&&p.buttonNorth.wasPressedThisFrame)){customPadControls=!customPadControls;CGAudio.Menu();}
        int category=0;if(k!=null&&k.tabKey.wasPressedThisFrame)category=k.leftShiftKey.isPressed?-1:1;
        if(p!=null){if(p.leftShoulder.wasPressedThisFrame)category=-1;if(p.rightShoulder.wasPressedThisFrame)category=1;}
        if(customSection!=2){if(k!=null){if(k.leftArrowKey.wasPressedThisFrame)category=-1;if(k.rightArrowKey.wasPressedThisFrame)category=1;}if(p!=null){if(p.dpad.left.wasPressedThisFrame)category=-1;if(p.dpad.right.wasPressedThisFrame)category=1;}}
        if(category!=0){CustomCategory(customSection+category);return;}
        int count=customSection==0?6:customSection==1?8:11;selection=menuNavigation.Read(selection,count,false);
        if(customSection==1&&selection<4)customAbility=selection;
        if(customSection==2&&selection<9){customMilestone=selection+1;customAbility=CGAbilities.RewardAbility[selection];}
        if((k!=null&&k.tKey.wasPressedThisFrame)||(p!=null&&p.buttonWest.wasPressedThisFrame))DemoAbility();
        if(CGControl.Confirm())CustomAction(selection);if(CGControl.Back())CustomLeave();
    }
    void CustomLabel(Rect rect,string text,int size=21,Color? color=null,bool body=false,TextAnchor align=TextAnchor.MiddleLeft){MapLabel(rect,text,size,color,body,align);}
    void CustomPanel(Rect rect){Panel(rect,new Color(.29f,.38f,.55f));}
    bool CustomButton(Rect rect,string label,int id,bool enabled=true){
        bool focus=selection==id;Color amber=CGMenuTheme.Design!=null?CGMenuTheme.Design.amber:new Color(1,.56f,.12f);
        float lift=editorMenuPreview?(focus?1:0):menuMotion.Focus(id);rect.x-=lift*2;rect.width+=lift*4;
        var tint=CGUIDraw.Color;CGUIDraw.Color=new Color(tint.r,tint.g,tint.b,tint.a*(enabled?1:.55f));CGUIDraw.Texture(rect,CGMenuArtwork.Button(focus,amber));CGUIDraw.Color=tint;
        CustomLabel(new Rect(rect.x+12,rect.y+2,rect.width-24,rect.height-4),label,23,enabled?Color.white:new Color(.64f,.7f,.8f),false,TextAnchor.MiddleCenter);
        if(!editorMenuPreview&&!customDiscard&&pointerMoved&&rect.Contains(Event.current.mousePosition))selection=id;
        if(!customDiscard&&Click(rect,"",GUIStyle.none)){selection=id;CustomAction(id);return true;}return false;
    }
    void DrawCustomization(){
        if(!customReady)BeginCustomization();var amber=CGMenuTheme.Design!=null?CGMenuTheme.Design.amber:new Color(1,.56f,.12f);Color accent=CGAbilities.ColorFor(inspectPurple?2:0);
        RectColor(new Rect(0,0,1280,97),new Color(.015f,.025f,.055f,.96f));RectColor(new Rect(0,94,1280,3),amber);
        CustomLabel(new Rect(29,10,470,49),"CAMPUS GLITCH",35,amber);CustomLabel(new Rect(32,60,568,25),"UNIFRANZ LA PAZ · INNOVACIÓN · APRENDE HACIENDO",18);
        CGUIDraw.Text(new Rect(637,19,610,57),"PERSONALIZA TU GUARDIÁN",new GUIStyle(titleStyle){fontSize=30});
        CustomPanel(new Rect(18,116,162,536));
        CGGuardianMenuArt.DrawPortrait(new Rect(52,131,92,88),inspectPurple);
        CustomLabel(new Rect(31,226,136,51),"TU ESTILO\nTU HABILIDAD",23,null,false,TextAnchor.MiddleCenter);
        for(int i=0;i<3;i++){
            var rect=new Rect(28,302+i*89,142,65);CGUIDraw.Texture(rect,CGMenuArtwork.Button(customSection==i,amber));CustomLabel(rect,CustomSections[i],21,null,false,TextAnchor.MiddleCenter);
            if(!customDiscard&&Click(rect,"",GUIStyle.none)){CustomCategory(i);}
        }
        CustomButton(new Rect(32,595,134,44),"VOLVER",customSection==0?5:customSection==1?7:10);
        CustomPanel(new Rect(195,116,296,417));
        CustomLabel(new Rect(211,127,265,39),CustomSections[customSection],26,amber);
        if(customSection==0)DrawAppearance(amber);
        else if(customSection==1)DrawAbilityDetails(accent);
        else DrawEvolutionDetails(amber);
        DrawCustomizationHero(new Rect(507,116,342,417),accent);
        if(customSection==2)DrawMilestoneCards(amber);else for(int i=0;i<4;i++)DrawAbilityCard(i,new Rect(867,116+i*101,394,91),amber);
        DrawActivationHelp(amber);
        if(CustomDirty)CustomLabel(new Rect(637,74,610,20),"CAMBIOS PENDIENTES · PULSA APLICAR",16,amber,false,TextAnchor.MiddleRight);
        CustomPanel(new Rect(867,526,394,126));
        int restored=0;for(int i=1;i<=9;i++)if(CGAbilities.Completed(CGStore.Data,i))restored++;
        CustomLabel(new Rect(884,535,360,27),"EVOLUCIÓN DEL GUARDIÁN",24,amber);
        for(int i=0;i<9;i++){bool complete=CGAbilities.Completed(CGStore.Data,i+1);var r=new Rect(883+i%3*120,569+i/3*23,111,20);RectColor(r,complete?new Color(.05f,.3f,.21f):new Color(.09f,.12f,.2f));CustomLabel(r,"N"+(i+1)+" · "+(complete?"LOGRADO":"PENDIENTE"),16,complete?new Color(.45f,1,.7f):new Color(.65f,.7f,.8f),false,TextAnchor.MiddleCenter);}
        CustomLabel(new Rect(884,637,358,16),restored+"/9 hitos · consulta Evolución",15,null,true);
        RectColor(new Rect(0,676,1280,44),new Color(.012f,.02f,.045f,.98f));
        CustomLabel(new Rect(20,682,1240,31),"↑↓ / STICK: OPCIÓN   ·   LB / RB / TAB: CATEGORÍA   ·   A / ENTER: ELEGIR   ·   X / T: PROBAR   ·   Y / V: TECLADO / MANDO   ·   B / ESC: VOLVER",16,null,false,TextAnchor.MiddleCenter);
        if(Time.unscaledTime<toastUntil){RectColor(new Rect(195,653,1066,23),new Color(.012f,.02f,.045f,.96f));CustomLabel(new Rect(207,652,1043,25),toast,17,amber);}
        if(customDiscard)DrawDiscardCustomization();
    }
    void DrawAppearance(Color amber){
        for(int i=0;i<2;i++){
            var rect=new Rect(211+i*132,179,120,112);RectColor(rect,new Color(.025f,.04f,.08f,.9f));Outline(rect,selection==i?amber:new Color(.32f,.4f,.56f),2);
            CGGuardianMenuArt.DrawPortrait(new Rect(rect.x+27,rect.y+6,66,65),i==1);
            CustomLabel(new Rect(rect.x+2,rect.y+76,116,29),i==0?"ROJO":CGStore.Data.purpleUnlocked?"MORADO":"MORADO · N2",19,null,false,TextAnchor.MiddleCenter);
            if(!customDiscard&&Click(rect,"",GUIStyle.none)){selection=i;CustomAction(i);}
        }
        CustomLabel(new Rect(211,303,260,28),"MODELO DEL PERSONAJE",21,amber);
        CustomButton(new Rect(211,340,264,45),(draftClassic?"":"✓ ")+"GUARDIÁN ACORAZADO",2);
        CustomButton(new Rect(211,394,264,45),(draftClassic?"✓ ":"")+"LOBO CLÁSICO",3);
        CustomButton(new Rect(211,465,264,46),CustomDirty?"APLICAR PERSONALIZACIÓN":"PERSONALIZACIÓN APLICADA",4);
    }
    void DrawAbilityDetails(Color accent){
        CGUIDraw.Texture(new Rect(287,167,112,112),CGAbilityArt.Card(customAbility),ScaleMode.ScaleToFit);
        CustomLabel(new Rect(212,283,262,39),CGAbilities.Names[customAbility],25,accent);
        CustomLabel(new Rect(212,328,262,62),CGAbilities.Description(CGStore.Data,customAbility),19,null,true);
        CustomLabel(new Rect(212,390,262,44),CGAbilities.Unlocked(CGStore.Data,customAbility)?customAbility==1?"PASIVA · UN SALTO EXTRA":"RANGO "+CGAbilities.Stats(CGStore.Data,customAbility).tier+" · RECARGA "+CGAbilities.Stats(CGStore.Data,customAbility).cooldown.ToString("0.0")+" s":CGAbilities.Gate(customAbility),19,CGAbilities.Unlocked(CGStore.Data,customAbility)?new Color(.45f,1,.7f):new Color(.77f,.8f,.87f));
        CustomButton(new Rect(211,435,127,44),"PROBAR",4);
        CustomButton(new Rect(347,435,128,44),"IMPACTO",5,customAbility==2&&demonstrationState.Active(2,CGMenuTheme.Clock));
        CustomButton(new Rect(211,485,264,44),draftAbility==CGStore.Data.equippedAbility?"HABILIDAD EQUIPADA":"APLICAR HABILIDAD",6);
    }
    static string ControlCaption(string binding){
        if(string.IsNullOrEmpty(binding))return "SIN ASIGNAR";
        switch(binding){case "Space":return "ESPACIO";case "LeftShift":return "SHIFT";case "RightShift":return "SHIFT DER.";case "buttonSouth":return "A";case "buttonEast":return "B";case "buttonWest":return "X";case "buttonNorth":return "Y";case "leftShoulder":return "LB";case "rightShoulder":return "RB";case "leftTrigger":return "LT";case "rightTrigger":return "RT";case "startButton":return "START";case "selectButton":return "SELECT";case "dpadUp":return "↑";case "dpadDown":return "↓";case "dpadLeft":return "←";case "dpadRight":return "→";case "leftStickButton":return "L3";case "rightStickButton":return "R3";default:return binding.ToUpperInvariant();}
    }
    void DrawActivationHelp(Color accent){
        int id=customSection==2?CGAbilities.RewardAbility[customMilestone-1]:customAbility,action=id==1?2:6;
        CustomPanel(new Rect(195,548,654,104));CustomLabel(new Rect(211,553,620,31),"CÓMO ACTIVAR · "+CGAbilities.Names[id],23,accent);
        for(int i=0;i<2;i++){var choice=new Rect(211+i*116,595,108,42);CGUIDraw.Texture(choice,CGMenuArtwork.Button(customPadControls==(i==1),accent));CustomLabel(choice,i==0?"TECLADO":"MANDO",21,null,false,TextAnchor.MiddleCenter);if(!customDiscard&&Click(choice,"",GUIStyle.none)){customPadControls=i==1;CGAudio.Menu();}}
        var key=new Rect(455,595,100,42);RectColor(key,new Color(.08f,.12f,.2f));Outline(key,new Color(.4f,.53f,.7f),1);CustomLabel(key,ControlCaption(customPadControls?CGStore.Data.padBindings[action]:CGStore.Data.actionKeys[action]),22,null,false,TextAnchor.MiddleCenter);
        string instruction=!CGAbilities.Unlocked(CGStore.Data,id)?CGAbilities.Gate(id):id==1?"SALTA Y PULSA OTRA VEZ EN EL AIRE":CGStore.Data.equippedAbility==id?"PULSA PARA ACTIVAR · EQUIPADA":"EQUÍPALA Y APLICA PARA UTILIZARLA";
        CustomLabel(new Rect(569,585,260,62),instruction,18,CGAbilities.Unlocked(CGStore.Data,id)?new Color(.45f,1,.7f):new Color(.76f,.8f,.86f),false);
    }
    void DrawEvolutionDetails(Color amber){
        bool complete=CGAbilities.Completed(CGStore.Data,customMilestone);int id=CGAbilities.RewardAbility[customMilestone-1];var stats=CGAbilities.Stats(CGStore.Data,id,customMilestone);
        CustomLabel(new Rect(212,178,262,43),"NIVEL "+customMilestone,32,amber);
        CGUIDraw.Texture(new Rect(287,217,112,112),CGAbilityArt.Card(id));
        CustomLabel(new Rect(212,327,262,78),CGAbilities.Rewards[customMilestone-1],23,null,true);
        CustomLabel(new Rect(212,406,262,44),complete?"HITO COMPLETADO · RANGO "+stats.tier:"RESTAURA ESTE NIVEL PARA OBTENERLO",19,complete?new Color(.45f,1,.7f):amber);
        CustomButton(new Rect(212,472,262,47),"DEMOSTRAR PODER",9);
    }
    void DrawMilestoneCards(Color amber){
        for(int i=0;i<9;i++){
            var r=new Rect(867+i%3*133,116+i/3*134,127,124);bool done=CGAbilities.Completed(CGStore.Data,i+1);int id=CGAbilities.RewardAbility[i];
            CustomPanel(r);if(customMilestone==i+1)Outline(r,amber,3);CustomLabel(new Rect(r.x+5,r.y+4,117,30),"NIVEL "+(i+1),24,null,false,TextAnchor.MiddleCenter);
            CGUIDraw.Color=done?Color.white:new Color(.58f,.64f,.76f);CGUIDraw.Texture(new Rect(r.x+38,r.y+34,51,51),CGAbilityArt.Card(id));CGUIDraw.Color=Color.white;
            CustomLabel(new Rect(r.x+3,r.y+85,121,31),done?"HITO LOGRADO":"POR RESTAURAR",17,done?new Color(.45f,1,.7f):new Color(.68f,.74f,.84f),false,TextAnchor.MiddleCenter);
            if(!customDiscard&&Click(r,"",GUIStyle.none)){selection=i;CustomAction(i);}
        }
    }
    void DrawAbilityCard(int id,Rect rect,Color amber){
        bool selected=customAbility==id,unlocked=CGAbilities.Unlocked(CGStore.Data,id);Color color=selected?amber:unlocked?new Color(.32f,.56f,.7f):new Color(.3f,.34f,.45f);
        CustomPanel(rect);if(selected)Outline(rect,color,2);
        CGUIDraw.Color=unlocked?Color.white:new Color(.5f,.55f,.65f);CGUIDraw.Texture(new Rect(rect.x+8,rect.y+8,75,75),CGAbilityArt.Card(id));CGUIDraw.Color=Color.white;
        CustomLabel(new Rect(rect.x+96,rect.y+7,rect.width-107,30),CGAbilities.Names[id],24);
        CustomLabel(new Rect(rect.x+96,rect.y+41,rect.width-107,41),unlocked?id==1?"PASIVA · SEGUNDO SALTO":draftAbility==id?CGStore.Data.equippedAbility==id?"EQUIPADA · LISTA PARA USAR":"SELECCIONADA · APLICAR PARA GUARDAR":"DISPONIBLE · ELEGIR PARA EQUIPAR":CGAbilities.Gate(id),17,unlocked?new Color(.45f,1,.7f):new Color(.7f,.75f,.83f));
        if(!editorMenuPreview&&!customDiscard&&pointerMoved&&customSection==1&&rect.Contains(Event.current.mousePosition)){selection=id;customAbility=id;}
        if(!customDiscard&&Click(rect,"",GUIStyle.none)){CustomCategory(1);selection=id;CustomAction(id);}
    }
    void DrawCustomizationHero(Rect rect,Color accent){
        RectColor(rect,new Color(.02f,.035f,.075f,.48f));
        CustomLabel(new Rect(rect.x+12,rect.y+8,rect.width-24,30),"LOBO GUARDIÁN",26,Color.white,false,TextAnchor.MiddleCenter);
        float now=CGMenuTheme.Clock,age=now-demonstrationAt;bool showing=demonstration>=0&&age<2.2f;float x=0,y=0;
        if(showing&&demonstration==0)x=age<.25f?age/.25f*64:Mathf.Lerp(64,0,Mathf.Clamp01((age-.5f)/.5f));
        if(showing&&demonstration==1){y=Mathf.Max(0,Mathf.Sin(Mathf.Clamp01(age/1.1f)*Mathf.PI)*68);if(secondJumpAt>0)y+=Mathf.Max(0,Mathf.Sin(Mathf.Clamp01((now-secondJumpAt)/.8f)*Mathf.PI)*58);}
        var area=new Rect(rect.x+36+x,rect.y+55-y,rect.width-72,280);
        CGUIDraw.Texture(new Rect(rect.x+39,rect.y+328,rect.width-78,22),CGMenuArtwork.Button(true,accent));CGUIDraw.Texture(new Rect(rect.x+63,rect.y+328,rect.width-126,19),CGMenuArtwork.Shadow);
        if(showing){int row=demonstration==0?2:demonstration==1?3:demonstration==3?5:0;CGGuardianArt.DrawFit(area,CGGuardianArt.Get(inspectPurple,row,(int)(age*14)%8));}
        else if(draftClassic)CGGuardianArt.DrawFit(area,CGArt.Player(inspectPurple,(int)(now*5)%2,true));else CGGuardianMenuArt.Draw(area,inspectPurple);
        if(showing&&demonstration==2&&demonstrationState.Active(2,now)){CGUIDraw.Color=new Color(accent.r,accent.g,accent.b,.7f);CGUIDraw.Texture(new Rect(rect.x+26,rect.y+58,rect.width-52,292),CGAbilityArt.Icon(2));CGUIDraw.Color=Color.white;}
        if(showing&&demonstration==3&&age<.5f){CGUIDraw.Color=accent;CGUIDraw.Texture(new Rect(rect.xMax-95,rect.y+137,78,133),CGAbilityArt.Icon(3));CGUIDraw.Color=Color.white;}
        if(showing&&demonstration==3){bool hit=age>.16f;CGUIDraw.Color=hit?new Color(1,1,1,.3f):Color.white;CGGuardianArt.DrawFit(new Rect(rect.xMax-68,rect.y+267,58,63),CGCampusActors.Frame(true,0,(int)(now*8)%8));CGUIDraw.Color=Color.white;RectColor(new Rect(rect.xMax-67,rect.y+254,55,4),new Color(.18f,.23f,.3f));if(!hit)RectColor(new Rect(rect.xMax-67,rect.y+254,55,4),new Color(.5f,1,.55f));CustomLabel(new Rect(rect.xMax-84,rect.y+231,84,21),hit?CGAbilities.Stats(CGStore.Data,3,customSection==2?customMilestone:0).damage+" DE DAÑO":"OBJETIVO",15,accent,false,TextAnchor.MiddleCenter);}
        float remain=demonstrationState.Remaining(customAbility,now);string message=showing?demonstration==1?secondJumpAt>0?"SEGUNDO SALTO ACTIVADO":"PROBAR OTRA VEZ: SEGUNDO SALTO":impact?"IMPACTO ABSORBIDO":CGAbilities.Names[demonstration]:"SELECCIONA Y PRUEBA UNA HABILIDAD";
        CustomLabel(new Rect(rect.x+10,rect.y+355,rect.width-20,31),message,17,accent,false,TextAnchor.MiddleCenter);
        CustomLabel(new Rect(rect.x+10,rect.y+388,rect.width-20,22),remain>0?"RECARGA "+remain.ToString("0.0")+" s":"T / X · DEMOSTRACIÓN SIN ALTERAR PROGRESO",15,null,false,TextAnchor.MiddleCenter);
    }
    void DrawDiscardCustomization(){
        RectColor(new Rect(0,0,1280,720),new Color(0,0,0,.76f));Panel(new Rect(320,245,640,230));
        CustomLabel(new Rect(345,260,590,48),"¿SALIR SIN APLICAR?",30,null,false,TextAnchor.MiddleCenter);
        CustomLabel(new Rect(345,313,590,43),"Tu configuración guardada se conservará.",21,null,true,TextAnchor.MiddleCenter);
        for(int i=0;i<2;i++){var r=new Rect(347+i*300,392,286,55);CGUIDraw.Texture(r,CGMenuArtwork.Button(discardChoice==i,new Color(1,.56f,.12f)));CustomLabel(r,i==0?"DESCARTAR CAMBIOS":"SEGUIR EDITANDO",23,null,false,TextAnchor.MiddleCenter);if(Click(r,"",GUIStyle.none)){customDiscard=false;if(i==0)ToMain();}}
    }
    void DrawAbilityReward(){
        if(sceneLevel<0||sceneLevel>8)return;
        int id=CGAbilities.RewardAbility[sceneLevel];var accent=CGAbilities.ColorFor(CGStore.Data.purpleEquipped?2:0);
        CustomPanel(new Rect(340,328,600,106));CGUIDraw.Texture(new Rect(353,343,76,76),CGAbilityArt.Card(id));
        CustomLabel(new Rect(431,336,492,27),"RECOMPENSA DEL NIVEL "+(sceneLevel+1),20,accent);
        CustomLabel(new Rect(431,367,492,56),CGAbilities.Rewards[sceneLevel],23,null,true);
    }
#if UNITY_EDITOR
    public void SelectCustomizationSection(int section){if(!customReady)BeginCustomization();customSection=Mathf.Clamp(section,0,2);selection=0;}
    public void SelectCustomizationControls(bool gamepad){customPadControls=gamepad;}
#endif
}
}
