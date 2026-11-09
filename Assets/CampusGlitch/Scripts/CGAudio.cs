using UnityEngine;
namespace CampusGlitch {
public class CGAudio:MonoBehaviour {
 public static CGAudio Instance;AudioSource music,fx;
 void Awake(){Instance=this;music=gameObject.AddComponent<AudioSource>();fx=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=true;fx.playOnAwake=false;
   music.clip=Resources.Load<AudioClip>("CG/Sound/Music");music.volume=.4f;if(music.clip!=null)music.Play();}
 public static void Play(string name){if(Instance==null)return;AudioClip clip=Resources.Load<AudioClip>("CG/Sound/"+name);if(clip!=null)Instance.fx.PlayOneShot(clip,.8f);}
 static AudioClip menuMove,menuConfirm;
 public static void Menu(bool confirm=false){
   if(Instance==null)return;var clip=confirm?menuConfirm:menuMove;
   if(clip==null){int count=confirm?2205:1102;var samples=new float[count];
     for(int i=0;i<count;i++){float t=i/22050f,envelope=Mathf.Pow(1-i/(float)count,2);samples[i]=Mathf.Sin(2*Mathf.PI*(confirm?880:560)*t)*envelope*.14f;}
     clip=AudioClip.Create(confirm?"Menu_Confirmar":"Menu_Navegar",count,1,22050,false);clip.SetData(samples,0);clip.hideFlags=HideFlags.HideAndDontSave;
     if(confirm)menuConfirm=clip;else menuMove=clip;
   }
   Instance.fx.PlayOneShot(clip,.6f);
 }
 void OnDestroy(){if(Instance==this)Instance=null;}
}
}
