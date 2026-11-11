using UnityEngine;
namespace CampusGlitch {
public class CGAudio:MonoBehaviour {
 public static CGAudio Instance;AudioSource music,fx,ui;
 readonly float[] a=new float[128],b=new float[128],c=new float[128];
 void Awake(){Instance=this;music=gameObject.AddComponent<AudioSource>();fx=gameObject.AddComponent<AudioSource>();ui=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=true;fx.playOnAwake=ui.playOnAwake=false;
   music.clip=Resources.Load<AudioClip>("CG/Sound/Music");music.volume=.4f;if(music.clip!=null)music.Play();}
 void Update(){var options=CGSettingsRuntime.Options;music.volume=.4f*options.musicVolume;fx.volume=options.effectsVolume;ui.volume=options.interfaceVolume;CGSettingsRuntime.Audio();}
 public static bool Play(string name){if(Instance==null)return false;AudioClip clip=Resources.Load<AudioClip>("CG/Sound/"+name);if(clip==null)return false;Instance.fx.volume=CGSettingsRuntime.Options.effectsVolume;Instance.fx.PlayOneShot(clip,.8f);return true;}
 public static void Spectrum(float[] result){
   if(Instance==null){System.Array.Clear(result,0,result.Length);return;}var self=Instance;self.music.GetSpectrumData(self.a,0,FFTWindow.BlackmanHarris);self.fx.GetSpectrumData(self.b,0,FFTWindow.BlackmanHarris);self.ui.GetSpectrumData(self.c,0,FFTWindow.BlackmanHarris);
   for(int i=0;i<result.Length;i++){int bin=Mathf.Min(127,1+i*3);result[i]=CGSettingsRuntime.Options.muted?0:Mathf.Clamp01(Mathf.Sqrt(self.a[bin]+self.b[bin]+self.c[bin])*5*CGSettingsRuntime.Master);}
 }
 static AudioClip menuMove,menuConfirm;
 public static void Menu(bool confirm=false){
   if(Instance==null)return;var clip=confirm?menuConfirm:menuMove;
   if(clip==null){int count=confirm?2205:1102;var samples=new float[count];
     for(int i=0;i<count;i++){float t=i/22050f,envelope=Mathf.Pow(1-i/(float)count,2);samples[i]=Mathf.Sin(2*Mathf.PI*(confirm?880:560)*t)*envelope*.14f;}
     clip=AudioClip.Create(confirm?"Menu_Confirmar":"Menu_Navegar",count,1,22050,false);clip.SetData(samples,0);clip.hideFlags=HideFlags.HideAndDontSave;
     if(confirm)menuConfirm=clip;else menuMove=clip;
   }
   Instance.ui.volume=CGSettingsRuntime.Options.interfaceVolume;Instance.ui.PlayOneShot(clip,.6f);
 }
 void OnDestroy(){if(Instance==this)Instance=null;}
}
}
