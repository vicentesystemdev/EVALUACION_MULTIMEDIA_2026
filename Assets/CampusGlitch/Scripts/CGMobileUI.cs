using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace CampusGlitch {
public partial class CGGame {
    CGMobilePackage mobilePackage;List<CGMobilePhone> mobilePhones=new List<CGMobilePhone>();int mobilePhone,mobilePhoneChoice;bool mobilePhonePicker;
    string mobileMessage="Genera una APK para empezar.",mobileSource="";Task<string> mobileTask;Task<List<CGMobilePhone>> mobileDetect;
    bool MobileBusy {get{return mobileTask!=null||mobileDetect!=null;}}
    void BeginMobile(){mobilePackage=CGMobileLocal.Load();mobileSource=CGMobileLocal.SourceStatus!=null?CGMobileLocal.SourceStatus():"Las versiones nuevas se generan en la computadora del equipo.";mobileMessage=CGMobileLocal.Exists(mobilePackage)?"Elige dónde guardar o conecta tu teléfono por USB.":"Genera la primera versión para guardarla o enviarla.";}
    float mobileRefreshAt;
    void TickMobile(){
        if(helpSection==5&&Time.unscaledTime>=mobileRefreshAt){mobileRefreshAt=Time.unscaledTime+2;mobilePackage=CGMobileLocal.Load();if(CGMobileLocal.SourceStatus!=null)mobileSource=CGMobileLocal.SourceStatus();}
        if(mobileTask!=null&&mobileTask.IsCompleted){mobileMessage=mobileTask.IsFaulted?mobileTask.Exception.GetBaseException().Message:mobileTask.Result;mobileTask=null;}
        if(mobileDetect!=null&&mobileDetect.IsCompleted){if(mobileDetect.IsFaulted)mobileMessage=mobileDetect.Exception.GetBaseException().Message;else{mobilePhones=mobileDetect.Result;mobilePhone=0;mobileMessage=mobilePhones.Count==0?"Sin teléfonos. Conecta el cable USB y autoriza la conexión en el celular.":"Elige el teléfono al que quieres enviar la APK.";}mobileDetect=null;}
    }
    void MobileAction(int action){
        if(MobileBusy)return;try{
            if(action==12){string target=CGMobileLocal.ChooseDestination();if(!string.IsNullOrEmpty(target)){mobileMessage="Guardando APK…";mobileTask=CGMobileLocal.Copy(mobilePackage,target);}else mobileMessage="Copia cancelada.";}
            else if(action==13){mobileMessage="Buscando teléfonos USB…";mobileDetect=CGMobileLocal.Detect(mobilePackage);}
            else if(action==14){if(mobilePhones.Count>0){mobilePhonePicker=true;mobilePhoneChoice=mobilePhone;menuNavigation.Reset();}}
            else if(action==15){mobileMessage="Enviando APK…";mobileTask=CGMobileLocal.Send(mobilePackage,mobilePhones.Count>0?mobilePhones[mobilePhone]:null);}
            else if(action==16){if(CGMobileLocal.RequestBuild!=null){mobileMessage="Unity generará la nueva APK al salir de Play.";CGMobileLocal.RequestBuild();}else mobileMessage="Genera la versión actual desde Unity en la computadora del equipo.";}
            else if(action==17||action==18){bool before=CGStore.Data.touchGestures;CGStore.Data.touchGestures=action==18;if(!CGStore.Save()){CGStore.Data.touchGestures=before;mobileMessage="No se pudo guardar. Inténtalo de nuevo.";}else{CGTouch.Reset();mobileMessage=action==18?"Gestos activados. Los botones de uso y pausa siguen disponibles.":"Botones táctiles activados.";}}
        }catch(Exception error){mobileMessage=error.Message;}
    }
    bool MobileEnabled(int action){return !MobileBusy&&(action==12?CGMobileLocal.Exists(mobilePackage):action==13?mobilePackage!=null&&!string.IsNullOrEmpty(mobilePackage.adbPath):action==14?mobilePhones.Count>0:action==15?CGMobileLocal.Exists(mobilePackage)&&mobilePhones.Count>0&&mobilePhones[mobilePhone].Ready:action==16?CGMobileLocal.RequestBuild!=null:true);}
    int[] MobileNavigation(){var entries=new List<int>{0,1,2,3,4,5,6,7,8};for(int i=12;i<=18;i++)if(MobileEnabled(i))entries.Add(i);return entries.ToArray();}
    void MobileButton(Rect r,string label,int action){if(MobileEnabled(action))HelpButton(r,label,action,action==16);else{CGSettingsArt.Frame(r,new Color(.23f,.3f,.4f),HelpInk);HelpLabel(r,label,22,new Color(.48f,.55f,.64f),false,TextAnchor.MiddleCenter);}}
    void DrawMobileFooter(){MobileButton(new Rect(282,552,536,48),MobileBusy?"ESPERE…":"1 · GENERAR / ACTUALIZAR APK",16);HelpLabel(new Rect(850,548,374,56),mobileMessage,18,MapCyan,true);}
    bool HandleMobilePicker(){if(!mobilePhonePicker)return false;mobilePhoneChoice=menuNavigation.Read(mobilePhoneChoice,mobilePhones.Count+1,false);if(CGControl.Back()){mobilePhonePicker=false;menuNavigation.Reset();}else if(CGControl.Confirm()){mobilePhonePicker=false;if(mobilePhoneChoice<mobilePhones.Count)mobilePhone=mobilePhoneChoice;menuNavigation.Reset();}return true;}
    void DrawMobilePicker(){
        RectColor(new Rect(0,0,1280,720),new Color(.005f,.01f,.025f,.86f));HelpPanel(new Rect(340,162,600,410),true);HelpLabel(new Rect(365,178,550,39),"ELIGE EL TELÉFONO",28,MapAmber);int start=mobilePhoneChoice<mobilePhones.Count?(mobilePhoneChoice/4)*4:0;
        for(int i=start;i<Mathf.Min(start+4,mobilePhones.Count);i++){var r=new Rect(365,232+(i-start)*63,550,54);CGSettingsArt.Frame(r,mobilePhoneChoice==i?MapAmber:MapCyan,HelpInk);HelpLabel(new Rect(r.x+14,r.y,r.width-28,r.height),mobilePhones[i].Caption,21);if(Click(r,"",GUIStyle.none)){mobilePhone=i;mobilePhonePicker=false;menuNavigation.Reset();}}
        var close=new Rect(365,506,550,44);CGSettingsArt.Frame(close,mobilePhoneChoice==mobilePhones.Count?MapAmber:MapCyan,HelpInk);HelpLabel(close,"CANCELAR · ESC / B",21,null,false,TextAnchor.MiddleCenter);if(Click(close,"",GUIStyle.none)){mobilePhonePicker=false;menuNavigation.Reset();}
    }
    void DrawHelpMobile(Rect left,Rect right){
        HelpPanel(left);CGUIDraw.Texture(new Rect(left.x+18,left.y+15,74,74),Resources.Load<Texture2D>("CG/Presentation/CampusGlitchIcon"),ScaleMode.ScaleToFit);
        HelpLabel(new Rect(left.x+110,left.y+16,left.width-128,36),"CAMPUS GLITCH",28,MapAmber);bool ready=CGMobileLocal.Exists(mobilePackage);
        HelpLabel(new Rect(left.x+110,left.y+54,left.width-128,30),ready?"ANDROID · "+mobilePackage.versionName+" · EDICIÓN "+mobilePackage.versionCode:"ANDROID · PRIMERA VERSIÓN",19,MapCyan);
        CGSettingsArt.Frame(new Rect(left.x+18,left.y+108,left.width-36,61),MapAmber,Color.Lerp(HelpInk,MapAmber,.07f));HelpLabel(new Rect(left.x+31,left.y+114,left.width-62,48),string.IsNullOrEmpty(mobileSource)?"Genera la APK con los cambios actuales del juego.":mobileSource,19,null,true);
        HelpLabel(new Rect(left.x+18,left.y+185,left.width-36,30),"CÓMO JUGAR EN EL CELULAR",24,MapAmber);
        MobileButton(new Rect(left.x+18,left.y+226,238,45),CGStore.Data.touchGestures?"BOTONES":"✓ BOTONES",17);MobileButton(new Rect(left.x+273,left.y+226,245,45),CGStore.Data.touchGestures?"✓ GESTOS":"GESTOS",18);
        HelpLabel(new Rect(left.x+18,left.y+280,left.width-36,45),CGStore.Data.touchGestures?"Arrastra para mover. Toca para atacar. Desliza arriba para saltar; a un lado para usar el poder.":"Mueve y combina acciones. Cada botón responde al tocarlo. Usa Correr para acelerar.",17,null,true);
        if(Application.isMobilePlatform){HelpPanel(right);HelpLabel(new Rect(right.x+18,right.y+16,right.width-36,40),"JUEGA A TU MANERA",27,MapAmber);HelpLabel(new Rect(right.x+18,right.y+82,right.width-36,205),"Elige Botones o Gestos.\n\nTambién puedes conectar un mando.\n\nInstala las nuevas versiones del equipo sobre esta app para conservar tu avance.",23,null,true);return;}
        HelpLabel(new Rect(right.x,right.y,right.width,30),"2 · ELIGE DÓNDE GUARDAR",22,MapAmber);MobileButton(new Rect(right.x,right.y+40,right.width,48),"GUARDAR APK EN…",12);
        HelpLabel(new Rect(right.x,right.y+107,right.width,30),"3 · ENVÍA A TU TELÉFONO",22,MapAmber);MobileButton(new Rect(right.x,right.y+148,right.width,46),MobileBusy?"BUSCANDO…":"DETECTAR TELÉFONOS USB",13);
        string phone=mobilePhones.Count==0?"CONECTA Y AUTORIZA TU TELÉFONO":mobilePhones[mobilePhone].Caption+" ▾";
        MobileButton(new Rect(right.x,right.y+204,right.width,56),phone,14);MobileButton(new Rect(right.x,right.y+270,right.width,46),"ENVIAR A DESCARGAS",15);
    }
}
}
