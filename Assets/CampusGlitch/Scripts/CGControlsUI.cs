using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
public partial class CGGame {
    static readonly string[] ControlActions={"MOVER IZQUIERDA","MOVER DERECHA","SALTAR","CORRER · MANTENER","ATAQUE BÁSICO","INTERACTUAR","PODER EQUIPADO","PAUSA"};
    enum ControlDialog{None,Conflict,Discard,Restore}
    bool controlsReady,controlsPractice,captureReleased;string[] controlsKeys,controlsPads;float captureStarted;int controlsDialogChoice=1,conflictAction=-1,conflictTarget=-1;string pendingControl;
    ControlDialog controlsDialog;
    int controlsFamily; // 0 Xbox, 1 PlayStation. Presentation is chosen explicitly.
    int observedControlDevice=-1;string controlsDeviceName="MANDO NO DETECTADO";
    bool ControlsPlayStation {get{return controlsFamily==1;}}
    void SyncControlDevice(){
        var device=Gamepad.current;if(device==null){observedControlDevice=-1;controlsDeviceName="MANDO NO DETECTADO";return;}
        string identity=device.GetType().FullName+" "+device.description.manufacturer+" "+device.description.product+" "+device.displayName;
        bool sony=identity.IndexOf("Dual",StringComparison.OrdinalIgnoreCase)>=0||identity.IndexOf("Sony",StringComparison.OrdinalIgnoreCase)>=0||identity.IndexOf("PlayStation",StringComparison.OrdinalIgnoreCase)>=0;
        bool xbox=identity.IndexOf("XInput",StringComparison.OrdinalIgnoreCase)>=0||identity.IndexOf("Xbox",StringComparison.OrdinalIgnoreCase)>=0;
        bool activity=false;foreach(string binding in CGControl.GamepadButtons)if(CGControl.PadButton(binding)?.wasPressedThisFrame==true){activity=true;break;}
        if((device.leftStick.ReadValue()-device.leftStick.ReadValueFromPreviousFrame()).sqrMagnitude>.01f||(device.rightStick.ReadValue()-device.rightStick.ReadValueFromPreviousFrame()).sqrMagnitude>.01f)activity=true;
        if(observedControlDevice!=device.deviceId||activity){if(sony)controlsFamily=1;else if(xbox)controlsFamily=0;observedControlDevice=device.deviceId;}
        controlsDeviceName=(sony?"PLAYSTATION":xbox?"XBOX":"MANDO COMPATIBLE")+" · "+device.displayName;
    }
    string PadCaption(string binding){
        if(!ControlsPlayStation)return binding=="selectButton"?"VIEW":binding=="startButton"?"MENU":ControlCaption(binding);
        switch(binding){case "buttonSouth":return "CRUZ";case "buttonEast":return "CÍRCULO";case "buttonNorth":return "TRIÁNGULO";case "buttonWest":return "CUADRADO";case "leftShoulder":return "L1";case "rightShoulder":return "R1";case "leftTrigger":return "L2";case "rightTrigger":return "R2";case "selectButton":return "CREATE";case "startButton":return "OPTIONS";default:return ControlCaption(binding);}
    }
    string KeyboardCaption(string binding){
        string fallback=ControlCaption(binding);if(!Enum.TryParse<Key>(binding,true,out var key)||key==Key.None||Keyboard.current==null||!Enum.IsDefined(typeof(Key),key))return fallback;
        if(binding.Length==1||binding.StartsWith("Digit",StringComparison.Ordinal)||key==Key.Semicolon||key==Key.Quote||key==Key.LeftBracket||key==Key.RightBracket||key==Key.Backslash){string local=Keyboard.current[key].displayName;if(!string.IsNullOrWhiteSpace(local))return local.ToUpperInvariant();}
        return fallback;
    }
    void SelectControllerFamily(int family){controlsFamily=(family%2+2)%2;CGAudio.Menu();}
    readonly Dictionary<string,float> controlPressTimes=new Dictionary<string,float>();
    bool ControlsDirty {get{return !SameControls(controlsKeys,CGStore.Data.actionKeys)||!SameControls(controlsPads,CGStore.Data.padBindings);}}
    static bool SameControls(string[] first,string[] second){if(first==null||second==null||first.Length!=second.Length)return false;for(int i=0;i<first.Length;i++)if(first[i]!=second[i])return false;return true;}
    void BeginControls(){CGStore.Load();controlsKeys=(string[])CGStore.Data.actionKeys.Clone();controlsPads=(string[])CGStore.Data.padBindings.Clone();controlsReady=true;rebinding=-1;controlsDialog=ControlDialog.None;controlsPractice=false;}
    void ControlsMode(bool gamepad){padMode=gamepad;selection=Mathf.Clamp(selection,0,7);menuNavigation.Reset();CGAudio.Menu();}
    void StartControlCapture(int action){if(action<0||action>=8)return;controlsPractice=false;rebinding=action;selection=action;captureStarted=Time.unscaledTime;captureReleased=false;CGAudio.Menu(true);}
    void SubmitControlBinding(string binding){
        if(rebinding<0||string.IsNullOrEmpty(binding))return;var draft=padMode?controlsPads:controlsKeys;int action=rebinding;rebinding=-1;
        if(draft[action]==binding){Hint("La acción ya utiliza ese control.");return;}
        for(int i=0;i<draft.Length;i++)if(i!=action&&draft[i]==binding){conflictAction=action;conflictTarget=i;pendingControl=binding;controlsDialog=ControlDialog.Conflict;controlsDialogChoice=1;return;}
        draft[action]=binding;Hint("Asignación preparada. Pulsa Aplicar para guardarla.");CGAudio.Menu(true);
    }
    bool SaveControls(){
        if(!ControlsDirty){Hint("Los controles ya están aplicados.");return true;}
        var keys=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var pads=new HashSet<string>(StringComparer.Ordinal);
        for(int i=0;i<8;i++){if(!Enum.TryParse<Key>(controlsKeys[i],true,out var key)||key==Key.None||!Enum.IsDefined(typeof(Key),key)||!keys.Add(controlsKeys[i])||Array.IndexOf(CGControl.GamepadButtons,controlsPads[i])<0||!pads.Add(controlsPads[i])){Hint("Revisa las asignaciones duplicadas o restaura los valores originales.");return false;}}
        string[] oldKeys=CGStore.Data.actionKeys,oldPads=CGStore.Data.padBindings;CGStore.Data.actionKeys=(string[])controlsKeys.Clone();CGStore.Data.padBindings=(string[])controlsPads.Clone();
        if(!CGStore.Save()){CGStore.Data.actionKeys=oldKeys;CGStore.Data.padBindings=oldPads;Hint("No se pudo guardar. Las asignaciones siguen disponibles para reintentar.");return false;}
        Hint("Controles aplicados y guardados.");CGAudio.Menu(true);return true;
    }
    void LeaveControls(){if(ControlsDirty){controlsDialog=ControlDialog.Discard;controlsDialogChoice=1;}else{rebinding=-1;controlsPractice=false;ToMain();}}
    void ControlAction(int action){
        if(action<8){StartControlCapture(action);return;}
        if(action==8){controlsDialog=ControlDialog.Restore;controlsDialogChoice=1;}else if(action==9){controlsPractice=!controlsPractice;Hint(controlsPractice?"Prueba activa: mueve y pulsa controles. Esc / B termina la prueba.":"Prueba terminada. No se alteraron asignaciones.");}else if(action==10)SaveControls();else LeaveControls();
    }
    void FinishControlDialog(bool accept){
        var dialog=controlsDialog;controlsDialog=ControlDialog.None;if(!accept)return;
        if(dialog==ControlDialog.Conflict){var draft=padMode?controlsPads:controlsKeys;string previous=draft[conflictAction];draft[conflictAction]=pendingControl;draft[conflictTarget]=previous;Hint("Asignaciones intercambiadas. Pulsa Aplicar para guardar.");}
        else if(dialog==ControlDialog.Restore){var defaults=new CGProgress();controlsKeys=defaults.actionKeys;controlsPads=defaults.padBindings;Hint("Valores originales preparados. Pulsa Aplicar para guardar.");}
        else if(dialog==ControlDialog.Discard){BeginControls();ToMain();}
    }
    void HandleControls(){
        if(!controlsReady)BeginControls();SyncControlDevice();var k=Keyboard.current;var p=Gamepad.current;
        if(controlsDialog!=ControlDialog.None){controlsDialogChoice=menuNavigation.Read(controlsDialogChoice,2,false);if(CGControl.Back())FinishControlDialog(false);else if(CGControl.Confirm())FinishControlDialog(controlsDialogChoice==0);return;}
        if(rebinding>=0){
            if(CGControl.Back()||Time.unscaledTime-captureStarted>20){rebinding=-1;Hint("Asignación cancelada. Tus controles se conservan.");return;}
            if(!captureReleased){bool held=false;if(padMode){foreach(string name in CGControl.GamepadButtons){var button=CGControl.PadButton(name);if(button!=null&&button.isPressed){held=true;break;}}}else held=k!=null&&k.anyKey.isPressed;if(!held)captureReleased=true;return;}
            if(padMode){string name=CGControl.NextGamepadButton();if(name!=null)SubmitControlBinding(name);}else{Key? pressed=CGControl.NextKeyboardKey();if(pressed.HasValue&&pressed.Value!=Key.Escape)SubmitControlBinding(pressed.Value.ToString());}return;
        }
        if(controlsPractice){if(CGControl.Back()){controlsPractice=false;Hint("Prueba terminada.");}return;}
        bool test=k!=null&&k.tKey.wasPressedThisFrame||p!=null&&p.buttonNorth.wasPressedThisFrame;
        if(test){ControlAction(9);return;}
        bool mode=k!=null&&k.tabKey.wasPressedThisFrame||p!=null&&(p.leftShoulder.wasPressedThisFrame||p.rightShoulder.wasPressedThisFrame);
        if(mode){ControlsMode(!padMode);return;}
        if(k!=null&&k.fKey.wasPressedThisFrame||p!=null&&p.selectButton.wasPressedThisFrame){SelectControllerFamily(controlsFamily+1);return;}
        selection=menuNavigation.Read(selection,12,false);if(CGControl.Confirm())ControlAction(selection);else if(CGControl.Back())LeaveControls();
    }
    static bool KeyboardBindingHeld(string binding){if(Keyboard.current==null||!Enum.TryParse<Key>(binding,true,out var key)||key==Key.None||!Enum.IsDefined(typeof(Key),key))return false;return Keyboard.current[key].isPressed;}
    bool DraftActionHeld(int action){return padMode?CGControl.PadButton(controlsPads[action])?.isPressed==true:KeyboardBindingHeld(controlsKeys[action]);}
    bool ControlsBlocked {get{return controlsDialog!=ControlDialog.None||rebinding>=0;}}
    void ControlLabel(Rect rect,string label,int size=21,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft,bool body=false){MapLabel(rect,label,size,color,body,align);}
    void ControlPanel(Rect rect){Panel(rect,new Color(.38f,.39f,.65f));}
    bool ControlButton(Rect rect,string label,int action){
        bool focus=selection==action;var amber=CGMenuTheme.Design!=null?CGMenuTheme.Design.amber:new Color(1,.56f,.12f);CGUIDraw.Texture(rect,CGMenuArtwork.Button(focus,amber));ControlLabel(rect,label,23,null,TextAnchor.MiddleCenter);
        if(!editorMenuPreview&&!ControlsBlocked&&!controlsPractice&&pointerMoved&&rect.Contains(Event.current.mousePosition))selection=action;
        if(!ControlsBlocked&&(!controlsPractice||action==9)&&Click(rect,"",GUIStyle.none)){selection=action;ControlAction(action);return true;}return false;
    }
    void DrawControls(){
        if(!controlsReady)BeginControls();SyncControlDevice();var design=CGMenuTheme.Design;var amber=design!=null?design.amber:new Color(1,.56f,.12f);var cyan=design!=null?design.cyan:new Color(.3f,.87f,1);
        RectColor(new Rect(0,0,1280,98),new Color(.015f,.025f,.06f,.96f));RectColor(new Rect(0,95,1280,3),amber);
        ControlLabel(new Rect(28,10,485,47),"CAMPUS GLITCH",36,amber);ControlLabel(new Rect(30,61,505,26),"UNIFRANZ LA PAZ · APRENDE HACIENDO",19);
        CGUIDraw.Text(new Rect(610,17,635,58),"AJUSTES / CONTROLES",new GUIStyle(titleStyle){fontSize=31});
        for(int i=0;i<2;i++){var r=new Rect(24+i*218,113,207,45);CGUIDraw.Texture(r,CGMenuArtwork.Button(padMode==(i==1),amber));ControlLabel(r,i==0?"TECLADO":"MANDO",26,null,TextAnchor.MiddleCenter);if(!ControlsBlocked&&Click(r,"",GUIStyle.none))ControlsMode(i==1);}
        bool connected=padMode?Gamepad.current!=null:Keyboard.current!=null;
        ControlLabel(new Rect(478,111,766,29),controlsPractice?"PRUEBA DE ENTRADA ACTIVA":padMode?controlsDeviceName:connected?"TECLADO DETECTADO":"TECLADO NO DETECTADO",22,connected?new Color(.4f,1,.68f):cyan,TextAnchor.MiddleRight);
        ControlLabel(new Rect(477,140,767,24),ControlsDirty?"CAMBIOS PENDIENTES DE APLICAR":"ASIGNACIONES GUARDADAS",18,ControlsDirty?amber:new Color(.72f,.8f,.9f),TextAnchor.MiddleRight);
        var left=design!=null?design.controlsActionsPanel:new Rect(24,177,420,390);var right=design!=null?design.controlsDevicePanel:new Rect(460,177,796,390);
        ControlPanel(left);ControlPanel(right);ControlLabel(new Rect(left.x+15,left.y+9,left.width-30,32),padMode?"REMAPEO DE MANDO":"REMAPEO DE TECLADO",27,amber);
        float pitch=(left.height-53)/8;
        for(int i=0;i<8;i++){
            var r=new Rect(left.x+12,left.y+49+i*pitch,left.width-24,pitch-3);bool held=DraftActionHeld(i);RectColor(r,held?new Color(.08f,.32f,.37f,.95f):selection==i?new Color(.3f,.12f,.045f,.9f):new Color(.035f,.05f,.1f,.92f));if(selection==i)Outline(r,amber,2);
            ControlLabel(new Rect(r.x+10,r.y,r.width-128,r.height),ControlActions[i],20,held?cyan:Color.white);var key=new Rect(r.xMax-109,r.y+3,100,r.height-6);RectColor(key,new Color(.08f,.12f,.22f));ControlLabel(key,padMode?PadCaption(controlsPads[i]):KeyboardCaption(controlsKeys[i]),21,held?cyan:Color.white,TextAnchor.MiddleCenter);
            if(!editorMenuPreview&&!ControlsBlocked&&!controlsPractice&&pointerMoved&&r.Contains(Event.current.mousePosition))selection=i;
            if(!ControlsBlocked&&!controlsPractice&&Click(r,"",GUIStyle.none)){selection=i;StartControlCapture(i);}
        }
        ControlLabel(new Rect(right.x+16,right.y+9,right.width-355,31),padMode?"MANDO · ENTRADA EN VIVO":"TECLADO · ENTRADA EN VIVO",23,cyan);
        if(padMode){string[] families={"XBOX","PLAYSTATION"};for(int i=0;i<2;i++){var r=new Rect(right.xMax-275+i*131,right.y+8,123,36);CGUIDraw.Texture(r,CGMenuArtwork.Button(controlsFamily==i,amber));ControlLabel(r,families[i],20,null,TextAnchor.MiddleCenter);if(!ControlsBlocked&&!controlsPractice&&Click(r,"",GUIStyle.none))SelectControllerFamily(i);}}
        else ControlLabel(new Rect(right.xMax-320,right.y+9,300,31),"TECLAS SEGÚN TU DISTRIBUCIÓN",18,new Color(.75f,.82f,.93f),TextAnchor.MiddleRight);
        float padHeight=right.height-110,padWidth=Mathf.Min(right.width-240,padHeight*512/300);var padArea=new Rect(right.center.x-padWidth*.5f,right.y+49,padWidth,padHeight);if(padMode){DrawGamepad(padArea);DrawControlCallouts(right);}else DrawKeyboard(new Rect(right.x+18,right.y+49,right.width-36,right.height-87));
        ControlLabel(new Rect(right.x+15,right.yMax-31,right.width-30,24),controlsPractice?padMode?"PULSA BOTONES Y MUEVE LOS STICKS · ESC / B: TERMINAR":"PULSA LAS TECLAS · ESC: TERMINAR LA PRUEBA":"T / Y: PROBAR SIN CAMBIAR ASIGNACIONES",18,new Color(.75f,.82f,.93f),TextAnchor.MiddleCenter);
        RectColor(new Rect(24,576,1232,31),new Color(.015f,.025f,.055f,.94f));string context=Time.unscaledTime<toastUntil?toast:controlsPractice?"Los brillos muestran la entrada real. La prueba no mueve al personaje ni guarda ajustes.":"Elige una acción y pulsa un nuevo control. Los duplicados se intercambian con confirmación.";ControlLabel(new Rect(36,576,1208,31),context,19,amber);
        ControlButton(new Rect(24,620,231,44),"RESTAURAR VALORES",8);ControlButton(new Rect(271,620,281,44),controlsPractice?"TERMINAR PRUEBA":"PROBAR ENTRADA",9);ControlButton(new Rect(568,620,382,44),ControlsDirty?"APLICAR CONTROLES":"CONTROLES APLICADOS",10);ControlButton(new Rect(966,620,290,44),"VOLVER",11);
        RectColor(new Rect(0,676,1280,44),new Color(.012f,.02f,.045f,1));ControlLabel(new Rect(20,683,1240,28),"↑↓: ACCIÓN · TAB / "+PadCaption("leftShoulder")+": TECLADO / MANDO · ENTER / "+PadCaption("buttonSouth")+": ASIGNAR · T / "+PadCaption("buttonNorth")+": PROBAR · F / "+PadCaption("selectButton")+": ESQUEMA · ESC: VOLVER",16,null,TextAnchor.MiddleCenter);
        if(rebinding>=0)DrawControlCapture();if(controlsDialog!=ControlDialog.None)DrawControlDialog();
    }
    void DrawControlCapture(){
        if(!padMode){
            RectColor(new Rect(0,0,1280,720),new Color(0,0,0,.75f));ControlPanel(new Rect(280,145,720,455));
            ControlLabel(new Rect(302,158,676,45),"ASIGNAR · "+ControlActions[rebinding],29,null,TextAnchor.MiddleCenter);
            ControlLabel(new Rect(302,209,676,36),"PULSA UNA TECLA O ELÍGELA EN EL TECLADO",23,new Color(.4f,.9f,1),TextAnchor.MiddleCenter);
            DrawKeyboard(new Rect(300,249,680,245),true);
            ControlLabel(new Rect(303,508,674,25),"ESC: CANCELAR · "+Mathf.CeilToInt(20-(Time.unscaledTime-captureStarted))+" s",18,null,TextAnchor.MiddleCenter);
            var cancel=new Rect(494,544,292,40);CGUIDraw.Texture(cancel,CGMenuArtwork.Button(false,new Color(1,.56f,.12f)));ControlLabel(cancel,"CANCELAR ASIGNACIÓN",23,null,TextAnchor.MiddleCenter);if(Click(cancel,"",GUIStyle.none))rebinding=-1;return;
        }
        RectColor(new Rect(0,0,1280,720),new Color(0,0,0,.68f));ControlPanel(new Rect(300,231,680,260));
        ControlLabel(new Rect(324,249,632,45),"ASIGNAR · "+ControlActions[rebinding],29,null,TextAnchor.MiddleCenter);
        ControlLabel(new Rect(331,304,618,57),captureReleased?padMode?"PULSA UN BOTÓN O GATILLO DEL MANDO":"PULSA LA NUEVA TECLA":"SUELTA EL CONTROL PARA COMENZAR",25,new Color(.4f,.9f,1),TextAnchor.MiddleCenter);
        if(padMode){string[] names={"buttonSouth","buttonEast","buttonWest","buttonNorth","leftShoulder","rightShoulder","leftTrigger","rightTrigger"};for(int i=0;i<names.Length;i++){var chip=new Rect(323+i*80,365,74,38);CGUIDraw.Texture(chip,CGMenuArtwork.Button(false,new Color(.3f,.8f,1)));ControlLabel(chip,PadCaption(names[i]),ControlsPlayStation?15:21,null,TextAnchor.MiddleCenter);if(Click(chip,"",GUIStyle.none))SubmitControlBinding(names[i]);}}
        ControlLabel(new Rect(332,padMode?404:373,616,padMode?20:39),"ESC / B: CANCELAR · "+Mathf.CeilToInt(20-(Time.unscaledTime-captureStarted))+" s",padMode?16:21,null,TextAnchor.MiddleCenter);
        var r=new Rect(494,426,292,44);CGUIDraw.Texture(r,CGMenuArtwork.Button(false,new Color(1,.56f,.12f)));ControlLabel(r,"CANCELAR ASIGNACIÓN",23,null,TextAnchor.MiddleCenter);if(Click(r,"",GUIStyle.none))rebinding=-1;
    }
    void DrawControlDialog(){
        RectColor(new Rect(0,0,1280,720),new Color(0,0,0,.75f));ControlPanel(new Rect(280,225,720,275));
        string title=controlsDialog==ControlDialog.Conflict?"CONTROL YA ASIGNADO":controlsDialog==ControlDialog.Restore?"¿RESTAURAR CONTROLES?":"¿SALIR SIN APLICAR?";
        string description=controlsDialog==ControlDialog.Conflict?(padMode?PadCaption(pendingControl):ControlCaption(pendingControl))+" ya se usa en "+ControlActions[conflictTarget]+". Puedes intercambiar ambas asignaciones.":controlsDialog==ControlDialog.Restore?"Se prepararán los valores originales de teclado y mando. Se guardarán al pulsar Aplicar.":"Se conservarán las asignaciones guardadas. Puedes seguir editando el borrador.";
        ControlLabel(new Rect(304,241,672,45),title,31,null,TextAnchor.MiddleCenter);ControlLabel(new Rect(312,297,656,81),description,21,null,TextAnchor.MiddleCenter,true);
        for(int i=0;i<2;i++){var r=new Rect(310+i*340,409,320,58);CGUIDraw.Texture(r,CGMenuArtwork.Button(controlsDialogChoice==i,new Color(1,.56f,.12f)));ControlLabel(r,i==0?controlsDialog==ControlDialog.Conflict?"INTERCAMBIAR":controlsDialog==ControlDialog.Restore?"RESTAURAR":"DESCARTAR CAMBIOS":controlsDialog==ControlDialog.Discard?"SEGUIR EDITANDO":"CANCELAR",23,null,TextAnchor.MiddleCenter);if(Click(r,"",GUIStyle.none))FinishControlDialog(i==0);}
    }
    void DrawGamepad(Rect rect){
        var design=CGMenuTheme.Design;bool ps=ControlsPlayStation;Texture body=ps?design!=null?design.controllerPlayStation:null:design!=null?design.controllerXbox:null;
        if(body==null)body=CGControlsArt.Controller(ps);if(body==null)body=CGControlsArt.Body(design!=null?design.controllerBody:new Color(.065f,.08f,.18f),design!=null?design.controllerEdge:new Color(.62f,.35f,1));CGUIDraw.Texture(rect,body);
        PadStick(rect,ps?new Vector2(190,198):new Vector2(145,120),false);PadStick(rect,ps?new Vector2(322,198):new Vector2(305,195),true);
        PadNode(rect,new Vector2(389,92),"Y","buttonNorth",ps?new Color(.3f,1,.7f):new Color(1,.86f,.24f),18);PadNode(rect,new Vector2(424,126),"B","buttonEast",new Color(1,.34f,.36f),18);PadNode(rect,new Vector2(389,160),"A","buttonSouth",ps?new Color(.45f,.72f,1):new Color(.35f,1,.58f),18);PadNode(rect,new Vector2(354,126),"X","buttonWest",ps?new Color(.95f,.4f,1):new Color(.3f,.8f,1),18);
        var dpad=ps?new Vector2(128,119):new Vector2(210,195);
        PadNode(rect,dpad+new Vector2(0,-20),"↑","dpadUp",Color.white,12);PadNode(rect,dpad+new Vector2(0,20),"↓","dpadDown",Color.white,12);PadNode(rect,dpad+new Vector2(-20,0),"←","dpadLeft",Color.white,12);PadNode(rect,dpad+new Vector2(20,0),"→","dpadRight",Color.white,12);
        PadNode(rect,ps?new Vector2(166,78):new Vector2(232,116),"V","selectButton",new Color(.7f,.77f,.94f),12);PadNode(rect,ps?new Vector2(346,78):new Vector2(280,116),"II","startButton",new Color(.7f,.77f,.94f),12);
        PadShoulder(rect,new Rect(108,5,80,28),PadCaption("leftTrigger"),"leftTrigger");PadShoulder(rect,new Rect(332,5,80,28),PadCaption("rightTrigger"),"rightTrigger");PadShoulder(rect,new Rect(102,37,92,25),PadCaption("leftShoulder"),"leftShoulder");PadShoulder(rect,new Rect(326,37,92,25),PadCaption("rightShoulder"),"rightShoulder");
        string live=padMode&&selection<8?PadCaption(controlsPads[selection])+" · "+ControlActions[selection]:"STICK IZQUIERDO · MOVIMIENTO HORIZONTAL";
        foreach(string binding in CGControl.GamepadButtons)if(CGControl.PadButton(binding)?.isPressed==true){live=PadCaption(binding)+" · "+BoundAction(binding);break;}
        ControlLabel(new Rect(rect.x-40,rect.yMax+1,rect.width+80,23),live,18,new Color(.55f,.85f,1),TextAnchor.MiddleCenter);
    }
    void DrawControlCallouts(Rect panel){
        string[] bindings={"leftTrigger","leftShoulder","rightTrigger","buttonSouth","buttonWest","buttonNorth"};
        for(int i=0;i<6;i++){bool left=i<3;int row=i%3;string binding=bindings[i];var r=new Rect(left?panel.x+13:panel.xMax-143,panel.y+86+row*72,130,59);RectColor(r,new Color(.025f,.04f,.085f,.94f));Outline(r,new Color(.32f,.38f,.58f),1);ControlLabel(new Rect(r.x+5,r.y+2,r.width-10,25),PadCaption(binding),21,new Color(1,.64f,.28f),TextAnchor.MiddleCenter);ControlLabel(new Rect(r.x+4,r.y+27,r.width-8,29),BoundAction(binding),15,Color.white,TextAnchor.MiddleCenter);}
    }
    float KeyboardGlow(string binding){float now=CGMenuTheme.Clock;if(KeyboardBindingHeld(binding)){controlPressTimes["key:"+binding]=now;return 1;}return !CGStore.Data.reduceFlash&&controlPressTimes.TryGetValue("key:"+binding,out var at)?Mathf.Clamp01(1-(now-at)*6):0;}
    void ChooseKeyboardKey(string binding){
        if(rebinding>=0&&!padMode){if(binding=="Escape"){rebinding=-1;return;}SubmitControlBinding(binding);return;}
        for(int i=0;i<8;i++)if(controlsKeys[i]==binding){StartControlCapture(i);return;}
        Hint("Elige primero la acción que quieres asignar a "+KeyboardCaption(binding)+".");
    }
    void DrawKeyboard(Rect area,bool capture=false){
        var design=CGMenuTheme.Design;var body=design!=null?design.keyboardBackground:null;if(body==null)body=CGControlsArt.KeyboardBody;if(body!=null)CGUIDraw.Texture(area,body);
        var caps=design!=null?design.keyboardKeyStates:null;if(caps==null)caps=CGControlsArt.KeyboardStates;
        string live=selection<8?KeyboardCaption(controlsKeys[selection])+" · "+ControlActions[selection]:"PULSA UNA TECLA PARA VER SU RESPUESTA";
        foreach(var tile in CGControlsArt.KeyboardKeys){
            bool held=KeyboardBindingHeld(tile.binding);float pulse=KeyboardGlow(tile.binding);
            int action=-1;for(int i=0;i<8;i++)if(string.Equals(controlsKeys[i],tile.binding,StringComparison.OrdinalIgnoreCase)){action=i;break;}
            bool focus=selection==action&&action>=0;var r=PadRect(area,new Rect(tile.x,tile.y,tile.w,tile.h));if(pulse>0&&!CGStore.Data.reduceFlash)r.y+=2*pulse;
            if(caps!=null)CGUIDraw.Uv(r,caps,new Rect((pulse>.1f?2:focus||action>=0?1:0)/3f,0,1/3f,1));else RectColor(r,new Color(.06f,.1f,.18f));
            string label=tile.binding=="UpArrow"?"↑":tile.binding=="DownArrow"?"↓":tile.binding=="LeftArrow"?"←":tile.binding=="RightArrow"?"→":tile.label;if(tile.binding.Length==1||tile.binding.StartsWith("Digit",StringComparison.Ordinal)||tile.binding=="Semicolon"||tile.binding=="Quote"||tile.binding=="LeftBracket"||tile.binding=="RightBracket"||tile.binding=="Backslash")label=KeyboardCaption(tile.binding);
            ControlLabel(r,label,label.Length>4?12:label.Length>2?14:18,held?new Color(.4f,1,1):action>=0?new Color(1,.76f,.4f):new Color(.8f,.88f,1),TextAnchor.MiddleCenter);
            if(held)live=label+" · "+(action>=0?ControlActions[action]:"SIN ACCIÓN DE JUEGO ASIGNADA");
            if((capture&&controlsDialog==ControlDialog.None||!ControlsBlocked&&!controlsPractice)&&Click(r,"",GUIStyle.none))ChooseKeyboardKey(tile.binding);
        }
        if(!capture)ControlLabel(new Rect(area.x,area.yMax-22,area.width,22),live,18,new Color(.55f,.85f,1),TextAnchor.MiddleCenter);
    }
    static Rect PadRect(Rect area,Rect source){return new Rect(area.x+source.x/512*area.width,area.y+source.y/300*area.height,source.width/512*area.width,source.height/300*area.height);}
    string BoundAction(string binding){for(int i=0;i<8;i++)if(controlsPads[i]==binding)return ControlActions[i];return "SIN ASIGNAR";}
    float PadGlow(string name){float now=CGMenuTheme.Clock;bool held=CGControl.PadButton(name)?.isPressed==true;if(held)controlPressTimes[name]=now;if(held)return 1;if(CGStore.Data.reduceFlash||CGMenuTheme.Design!=null&&!CGMenuTheme.Design.animateControlPresses)return 0;return controlPressTimes.TryGetValue(name,out var at)?Mathf.Clamp01(1-(now-at)*5):0;}
    void PadClick(Rect rect,string binding){if(controlsDialog!=ControlDialog.None||controlsPractice||rebinding>=0)return;if(Click(rect,"",GUIStyle.none)){for(int i=0;i<8;i++)if(controlsPads[i]==binding){padMode=true;StartControlCapture(i);return;}Hint("Elige una acción de mando para asignarle este botón.");}}
    void PadNode(Rect area,Vector2 point,string label,string binding,Color color,float radius){
        float glow=PadGlow(binding);bool selected=padMode&&selection<8&&controlsPads[selection]==binding;var r=PadRect(area,new Rect(point.x-radius,point.y-radius,radius*2,radius*2));float scale=CGMenuTheme.Design!=null?CGMenuTheme.Design.controllerPressScale:.07f;if(!CGStore.Data.reduceFlash){r.x-=r.width*glow*scale*.5f;r.y-=r.height*glow*scale*.5f;r.size*=1+glow*scale;}
        var tint=CGUIDraw.Color;CGUIDraw.Color=new Color(color.r,color.g,color.b,(selected?.55f:0)+glow*.55f);var halo=r;halo.x-=9;halo.y-=9;halo.width+=18;halo.height+=18;CGUIDraw.Texture(halo,CGControlsArt.Glow);CGUIDraw.Color=tint;
        var cap=CGMenuTheme.Design!=null?CGMenuTheme.Design.controllerButtonStates:null;if(cap==null)cap=CGControlsArt.ButtonStates;
        if(cap!=null){int state=glow>.4f?2:selected?1:0;CGUIDraw.Uv(r,cap,new Rect(state/3f,0,1/3f,1));}else{CGUIDraw.Color=new Color(.07f,.1f,.18f);CGUIDraw.Texture(r,CGControlsArt.Circle);CGUIDraw.Color=tint;}
        if(ControlsPlayStation&&binding.StartsWith("button",StringComparison.Ordinal)){int shape=binding=="buttonSouth"?0:binding=="buttonEast"?1:binding=="buttonNorth"?2:3;CGUIDraw.Color=color;var glyph=r;glyph.x+=r.width*.15f;glyph.y+=r.height*.15f;glyph.width*=.7f;glyph.height*=.7f;CGUIDraw.Texture(glyph,CGControlsArt.FaceShape(shape));CGUIDraw.Color=tint;}
        else ControlLabel(r,label,(int)(radius>14?25:18),radius>14?color:Color.white,TextAnchor.MiddleCenter);PadClick(r,binding);
    }
    void PadStick(Rect area,Vector2 point,bool right){var p=Gamepad.current;Vector2 value=p==null?Vector2.zero:right?p.rightStick.ReadValue():p.leftStick.ReadValue();var r=PadRect(area,new Rect(point.x-31,point.y-31,62,62));var tint=CGUIDraw.Color;CGUIDraw.Color=new Color(.08f,.12f,.2f);CGUIDraw.Texture(r,CGControlsArt.Circle);CGUIDraw.Color=tint;PadNode(area,point+new Vector2(value.x,-value.y)*11,right?"R3":"L3",right?"rightStickButton":"leftStickButton",new Color(.48f,.67f,1),22);}
    void PadShoulder(Rect area,Rect source,string label,string binding){var r=PadRect(area,source);float glow=PadGlow(binding);RectColor(r,Color.Lerp(new Color(.07f,.12f,.24f),new Color(.3f,.65f,.9f),glow));Outline(r,new Color(.5f,.65f,1),1);ControlLabel(r,label,19,null,TextAnchor.MiddleCenter);var control=CGControl.PadButton(binding);float value=control!=null?control.ReadValue():0;RectColor(new Rect(r.x,r.yMax-3,r.width*value,3),new Color(.3f,.9f,1));PadClick(r,binding);}
#if UNITY_EDITOR
    public void PreviewControlsMode(bool gamepad){if(!controlsReady)BeginControls();padMode=gamepad;}
    public void PreviewControllerFamily(int family){SelectControllerFamily(family);}
#endif
}
}
