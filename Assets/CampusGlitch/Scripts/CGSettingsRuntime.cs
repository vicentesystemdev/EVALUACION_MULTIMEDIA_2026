using System;
using UnityEngine;

namespace CampusGlitch {
[Serializable]public sealed class CGSettingsOptions {
    public float musicVolume=1,effectsVolume=1,interfaceVolume=1,brightness=1;
    public bool muted,verticalSync,solidText;
    public int width,height,screenMode=-1,hintSize=1;
    public CGSettingsOptions Copy(){return JsonUtility.FromJson<CGSettingsOptions>(JsonUtility.ToJson(this));}
    public bool Same(CGSettingsOptions s){return s!=null&&Mathf.Approximately(musicVolume,s.musicVolume)&&Mathf.Approximately(effectsVolume,s.effectsVolume)&&Mathf.Approximately(interfaceVolume,s.interfaceVolume)&&Mathf.Approximately(brightness,s.brightness)&&muted==s.muted&&verticalSync==s.verticalSync&&solidText==s.solidText&&width==s.width&&height==s.height&&screenMode==s.screenMode&&hintSize==s.hintSize;}
}
[Serializable]public sealed class CGSettingsDraft {
    public CGSettingsOptions options=new CGSettingsOptions();public float masterVolume=.85f;public bool reduceFlash,showHints=true;
    public CGSettingsDraft Copy(){return JsonUtility.FromJson<CGSettingsDraft>(JsonUtility.ToJson(this));}
    public bool Same(CGSettingsDraft s){return s!=null&&options.Same(s.options)&&Mathf.Approximately(masterVolume,s.masterVolume)&&reduceFlash==s.reduceFlash&&showHints==s.showHints;}
    public static CGSettingsDraft Read(CGProgress p){CGSettingsRuntime.Normalize(p);return new CGSettingsDraft{options=p.settings.Copy(),masterVolume=p.masterVolume,reduceFlash=p.reduceFlash,showHints=p.subtitles};}
}
public static class CGSettingsRuntime {
    public static CGSettingsDraft Preview;static bool displayInitialized;
    static float Valid(float v,float min,float max,float fallback){return float.IsNaN(v)||float.IsInfinity(v)?fallback:Mathf.Clamp(v,min,max);}
    public static void Normalize(CGProgress p){
        if(p.settings==null)p.settings=new CGSettingsOptions();var s=p.settings;
        s.musicVolume=Valid(s.musicVolume,0,1,1);s.effectsVolume=Valid(s.effectsVolume,0,1,1);s.interfaceVolume=Valid(s.interfaceVolume,0,1,1);s.brightness=Valid(s.brightness,.55f,1.2f,1);p.masterVolume=Valid(p.masterVolume,0,1,.85f);
        s.hintSize=Mathf.Clamp(s.hintSize,0,2);s.screenMode=Mathf.Clamp(s.screenMode,-1,2);if(s.width<640||s.height<360||s.width>7680||s.height>4320){s.width=s.height=0;}
    }
    public static CGSettingsOptions Options {get{return Preview!=null?Preview.options:CGStore.Data!=null&&CGStore.Data.settings!=null?CGStore.Data.settings:defaults;}}
    static readonly CGSettingsOptions defaults=new CGSettingsOptions();
    public static bool ReduceFlash {get{return Preview!=null?Preview.reduceFlash:CGStore.Data!=null&&CGStore.Data.reduceFlash;}}
    public static float Master {get{return Preview!=null?Preview.masterVolume:CGStore.Data!=null?CGStore.Data.masterVolume:.85f;}}
    public static void Audio(){if(Application.isPlaying)AudioListener.volume=Options.muted?0:Master;}
    public static bool CanChangeDisplay {get{return Application.isPlaying&&!Application.isEditor&&(Application.platform==RuntimePlatform.WindowsPlayer||Application.platform==RuntimePlatform.LinuxPlayer||Application.platform==RuntimePlatform.OSXPlayer);}}
    public static FullScreenMode Mode(int index){return index==2?FullScreenMode.ExclusiveFullScreen:index==1?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed;}
    public static int ModeIndex(FullScreenMode mode){return mode==FullScreenMode.ExclusiveFullScreen?2:mode==FullScreenMode.Windowed?0:1;}
    public static void Display(CGSettingsOptions s){if(CanChangeDisplay&&s.width>=640&&s.height>=360)Screen.SetResolution(s.width,s.height,Mode(s.screenMode));}
    public static void ApplySaved(){
        CGStore.Load();Normalize(CGStore.Data);Audio();if(!Application.isPlaying)return;QualitySettings.vSyncCount=CGStore.Data.settings.verticalSync?1:0;Application.targetFrameRate=CGStore.Data.settings.verticalSync?-1:60;
        if(!displayInitialized){displayInitialized=true;if(CGStore.Data.settings.screenMode>=0)Display(CGStore.Data.settings);}
    }
}
}
