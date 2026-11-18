using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
public partial class CGGame {
    CGCreditsDesign creditsDesign;int creditsOverlay,creditsMember,creditsReturnSelection;float creditsModalAt,creditsHorizontalAt;int creditsHorizontalDirection;
    CGCreditsDesign CreditsDesign {get{if(creditsDesign==null)creditsDesign=Resources.Load<CGCreditsDesign>("CG/Presentation/CampusCredits");return creditsDesign;}}
    int CreditsCount {get{return CreditsDesign!=null&&CreditsDesign.team!=null?Mathf.Min(4,CreditsDesign.team.Length):0;}}
    void BeginCredits(){creditsOverlay=creditsMember=selection=creditsHorizontalDirection=0;creditsHorizontalAt=0;menuNavigation.Reset();}
    void OpenCreditsOverlay(int kind,int member=0){creditsReturnSelection=selection;creditsOverlay=kind;creditsMember=Mathf.Clamp(member,0,Mathf.Max(0,CreditsCount-1));creditsModalAt=CGMenuTheme.Clock;CGAudio.Menu(true);}
    void CloseCreditsOverlay(){creditsOverlay=0;selection=creditsReturnSelection;menuNavigation.Reset();CGAudio.Menu();}
    void CreditsAction(int item){int count=CreditsCount;if(item<count)OpenCreditsOverlay(1,item);else if(item==count)OpenCreditsOverlay(2);else if(item==count+1)OpenCreditsOverlay(3);else ToMain();}
    void HandleCredits(){
        var k=Keyboard.current;var p=Gamepad.current;
        if(creditsOverlay!=0){
            if(CGControl.Back()||CGControl.Confirm()){CloseCreditsOverlay();return;}
            if(creditsOverlay==1){int step=0;if(k!=null){if(k.leftArrowKey.wasPressedThisFrame)step=-1;if(k.rightArrowKey.wasPressedThisFrame)step=1;}if(p!=null){if(p.leftShoulder.wasPressedThisFrame||p.dpad.left.wasPressedThisFrame)step=-1;if(p.rightShoulder.wasPressedThisFrame||p.dpad.right.wasPressedThisFrame)step=1;}if(step!=0&&CreditsCount>0){creditsMember=(creditsMember+step+CreditsCount)%CreditsCount;creditsModalAt=CGMenuTheme.Clock;CGAudio.Menu();}}return;
        }
        int total=CreditsCount+3;selection=menuNavigation.Read(selection,total,false);
        float x=0;if(k!=null){if(k.leftArrowKey.isPressed||k.aKey.isPressed)x=-1;if(k.rightArrowKey.isPressed||k.dKey.isPressed)x=1;}if(p!=null&&Mathf.Abs(x)<.1f){x=p.dpad.ReadValue().x;if(Mathf.Abs(x)<.5f)x=p.leftStick.ReadValue().x;}
        int direction=x>.55f?1:x<-.55f?-1:0;if(direction==0)creditsHorizontalDirection=0;else if(direction!=creditsHorizontalDirection||Time.unscaledTime>=creditsHorizontalAt){selection=(selection+direction+total)%total;creditsHorizontalDirection=direction;creditsHorizontalAt=Time.unscaledTime+.25f;}
        if(CGControl.Confirm())CreditsAction(selection);else if(CGControl.Back())ToMain();
    }
    void CreditsLabel(Rect rect,string text,int size=21,Color? tint=null,bool body=false,TextAnchor align=TextAnchor.MiddleCenter){MapLabel(rect,text,size,tint,body,align);}
    Color CreditInk {get{return new Color(.02f,.03f,.07f,CreditsDesign!=null?CreditsDesign.panelOpacity:.94f);}}
    void CreditPortrait(Rect area,CGCreditProfile profile){
        if(profile.portrait==null)return;float source=profile.portrait.width/(float)profile.portrait.height,target=area.width/area.height;var uv=new Rect(0,0,1,1);
        if(source>target){uv.width=target/source;uv.x=(1-uv.width)*.5f;}else{uv.height=source/target;uv.y=(1-uv.height)*profile.portraitFocus;}CGUIDraw.Uv(area,profile.portrait,uv);
    }
    void CreditPanel(Rect rect,Color accent,bool solid=false){RectColor(rect,solid?new Color(.02f,.03f,.07f,1):CreditInk);Outline(rect,accent,2);RectColor(new Rect(rect.x+10,rect.y,Mathf.Min(48,rect.width*.2f),3),new Color(1,.62f,.21f));}
    bool CreditsButton(Rect rect,string text,int index){
        bool selected=selection==index;CGUIDraw.Texture(rect,CGMenuArtwork.Button(selected,new Color(1,.56f,.12f)));CreditsLabel(rect,text,24);
        if(!editorMenuPreview&&creditsOverlay==0&&pointerMoved&&rect.Contains(Event.current.mousePosition))selection=index;
        if(creditsOverlay==0&&Click(rect,"",GUIStyle.none)){selection=index;CreditsAction(index);return true;}return false;
    }
    void DrawCredits(){
        var design=CreditsDesign;if(design==null){Header("CRÉDITOS");Label("Cargando la presentación del equipo...",330);if(Button("VOLVER",0,470,590,340,55))ToMain();return;}
        RectColor(new Rect(0,0,1280,720),new Color(.02f,.025f,.07f,.36f));RectColor(new Rect(0,0,1280,115),new Color(.012f,.022f,.055f,.93f));
        CreditsLabel(new Rect(28,13,470,47),"CAMPUS GLITCH",37,new Color(1,.56f,.12f),false,TextAnchor.MiddleLeft);CreditsLabel(new Rect(30,64,550,28),"UNIFRANZ LA PAZ · APRENDE HACIENDO",20,null,false,TextAnchor.MiddleLeft);
        CGUIDraw.Text(new Rect(750,21,490,66),design.title,new GUIStyle(titleStyle){fontSize=43});RectColor(new Rect(0,112,1280,3),new Color(1,.56f,.12f));
        CreditsLabel(new Rect(31,128,1218,38),design.subtitle,28,new Color(.72f,.8f,1));
        for(int i=0;i<CreditsCount;i++){
            var profile=design.team[i];if(profile==null)continue;var r=design.firstCard;r.x+=i*(r.width+design.cardGap);bool active=selection==i;float focus=editorMenuPreview?(active?1:0):menuMotion.Focus(i);r.y-=focus*(CGStore.Data.reduceFlash?0:design.hoverLift);
            CreditPanel(r,active?profile.accent:new Color(.32f,.35f,.53f));var portrait=new Rect(r.x+9,r.y+9,r.width-18,r.height*.55f);
            if(profile.arachnidTheme){RectColor(new Rect(r.x+2,r.y+2,4,r.height-4),profile.accent);if(!CGStore.Data.reduceFlash){float t=Mathf.Repeat(CGMenuTheme.Clock*.2f,1);RectColor(new Rect(r.x+3,r.y+12+(r.height-24)*t,3,11),new Color(.3f,.8f,1));}}
            if(profile.portrait!=null){float pulse=design.animatePortraits&&!CGStore.Data.reduceFlash?Mathf.Sin(CGMenuTheme.Clock*1.4f+i)*.7f:0;portrait.y+=pulse;CreditPortrait(portrait,profile);}
            CreditsLabel(new Rect(r.x+9,r.y+r.height*.57f,r.width-18,54),profile.name,24);
            RectColor(new Rect(r.x+16,r.y+r.height*.57f+60,r.width-32,1),profile.accent);
            CreditsLabel(new Rect(r.x+14,r.y+r.height*.57f+67,r.width-28,58),profile.role,18,null,true);
            CreditsLabel(new Rect(r.x+9,r.yMax-35,r.width-18,27),profile.theme,16,profile.accent);
            if(active&&!CGStore.Data.reduceFlash){float t=Mathf.Repeat(CGMenuTheme.Clock*.24f+i*.13f,1);RectColor(new Rect(r.x+4,r.y+12+(r.height-24)*t,3,7),profile.accent);}
            if(!editorMenuPreview&&creditsOverlay==0&&pointerMoved&&r.Contains(Event.current.mousePosition))selection=i;
            if(creditsOverlay==0&&Click(r,"",GUIStyle.none)){selection=i;CreditsAction(i);}
        }
        CreditsLabel(new Rect(33,584,1214,34),design.footer,24,new Color(.85f,.82f,1));
        int count=CreditsCount;CreditsButton(new Rect(31,628,460,45),"AGRADECIMIENTOS ESPECIALES",count);CreditsButton(new Rect(511,628,367,45),"ARTE Y RECURSOS",count+1);CreditsButton(new Rect(899,628,350,45),"VOLVER AL MENÚ",count+2);
        RectColor(new Rect(0,685,1280,35),new Color(.012f,.02f,.045f,.98f));CreditsLabel(new Rect(20,688,1240,28),"DIRECCIONES / STICK: ELEGIR · A / ENTER: VER DETALLE · B / ESC: VOLVER",17);
        if(creditsOverlay!=0)DrawCreditsOverlay();
    }
    void DrawCreditsOverlay(){
        var design=CreditsDesign;float entry=editorMenuPreview||CGStore.Data.reduceFlash?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((CGMenuTheme.Clock-creditsModalAt)/design.modalTransition));
        RectColor(new Rect(0,0,1280,720),new Color(.005f,.008f,.025f,.78f));var tint=CGUIDraw.Color;var matrix=CGUIDraw.Matrix;CGUIDraw.Color=new Color(tint.r,tint.g,tint.b,tint.a*entry);var shifted=matrix;shifted.m13+=(1-entry)*18*matrix.m11;CGUIDraw.Matrix=shifted;
        if(creditsOverlay==2)DrawAcknowledgements();else if(creditsOverlay==1)DrawCreditMember();else DrawCreditResources();
        CGUIDraw.Color=tint;CGUIDraw.Matrix=matrix;
    }
    bool CreditsModalClose(Rect rect,string label){CGUIDraw.Texture(rect,CGMenuArtwork.Button(true,new Color(.8f,.4f,1)));CreditsLabel(rect,label,24);if(Click(rect,"",GUIStyle.none)){CloseCreditsOverlay();return true;}return false;}
    void DrawAcknowledgements(){
        var r=new Rect(200,96,880,539);CreditPanel(r,new Color(.78f,.4f,1),true);CreditsLabel(new Rect(225,111,795,47),"AGRADECIMIENTOS ESPECIALES",34,new Color(1,.73f,.35f));CreditsModalClose(new Rect(1024,106,44,44),"×");
        var entries=CreditsDesign.acknowledgements;int count=entries!=null?Mathf.Min(2,entries.Length):0;
        for(int i=0;i<count;i++){
            var p=entries[i];if(p==null)continue;var card=new Rect(226+i*421,175,402,352);CreditPanel(card,p.accent,true);
            CreditPortrait(new Rect(card.x+74,card.y+10,254,190),p);
            RectColor(new Rect(card.x+10,card.y+204,card.width-20,48),new Color(.025f,.03f,.08f,.97f));CreditsLabel(new Rect(card.x+12,card.y+204,card.width-24,48),p.name,26);
            CreditsLabel(new Rect(card.x+16,card.y+258,card.width-32,41),p.role,21,p.accent);
            CreditsLabel(new Rect(card.x+16,card.y+302,card.width-32,42),p.detail,17,null,true);
        }
        CreditsModalClose(new Rect(475,556,330,51),"CONTINUAR");
    }
    void DrawCreditMember(){
        var profile=CreditsDesign.team[creditsMember];var r=new Rect(280,135,720,463);CreditPanel(r,profile.accent,true);CreditsModalClose(new Rect(944,147,44,44),"×");
        CreditPortrait(new Rect(304,191,226,283),profile);
        CreditsLabel(new Rect(547,184,419,76),profile.name,30);CreditsLabel(new Rect(548,270,418,47),profile.theme,20,profile.accent);
        CreditsLabel(new Rect(550,322,411,126),profile.detail,20,null,true);CreditsLabel(new Rect(304,485,672,35),"← / → / LB / RB: CAMBIAR INTEGRANTE",19,profile.accent);
        if(profile.arachnidTheme){CGUIDraw.Texture(new Rect(903,445,51,51),CGCreditsArt.Web);}
        CreditsModalClose(new Rect(474,535,332,43),"VOLVER AL EQUIPO");
    }
    void DrawCreditResources(){
        CreditPanel(new Rect(270,131,740,473),new Color(.3f,.85f,1),true);CreditsModalClose(new Rect(954,143,44,44),"×");CreditsLabel(new Rect(293,151,648,48),"ARTE Y RECURSOS",33,new Color(1,.7f,.3f));
        CreditsLabel(new Rect(319,214,642,308),CreditsDesign.resources,23,null,true,TextAnchor.UpperLeft);CreditsModalClose(new Rect(474,539,332,43),"VOLVER A CRÉDITOS");
    }
#if UNITY_EDITOR
    public void PreviewCredits(int overlay,int member=0){creditsOverlay=Mathf.Clamp(overlay,0,3);creditsMember=Mathf.Clamp(member,0,Mathf.Max(0,CreditsCount-1));creditsModalAt=CGMenuTheme.Clock;}
#endif
}
}
