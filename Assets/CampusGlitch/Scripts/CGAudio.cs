using UnityEngine;
namespace CampusGlitch {
public class CGAudio:MonoBehaviour {
 public static string Caption {get;private set;}="";
 public static float CaptionUntil {get;private set;}
 public static void Describe(string name){
   string text=name=="Jump"?"[Salto]":name=="Attack"?"[Ataque]":name=="Damage"?"[Golpe recibido]":name=="Pickup"?"[Fragmento recuperado]":name=="Checkpoint"?"[Señal restaurada]":name=="Win"?"[Victoria]":name=="Denied"?"[Acceso denegado]":"";
   if(text.Length==0)return;Caption=text;CaptionUntil=Time.unscaledTime+1.8f;
 }
 public static CGAudio Instance;AudioSource music,fx;
 void Awake(){Instance=this;music=gameObject.AddComponent<AudioSource>();fx=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=true;fx.playOnAwake=false;
   music.clip=Resources.Load<AudioClip>("CG/Sound/Music");music.volume=.4f;if(music.clip!=null)music.Play();}
 public static void Play(string name){Describe(name);if(Instance==null)return;AudioClip clip=Resources.Load<AudioClip>("CG/Sound/"+name);if(clip!=null)Instance.fx.PlayOneShot(clip,.8f);}
 void OnDestroy(){if(Instance==this)Instance=null;}
}
}
