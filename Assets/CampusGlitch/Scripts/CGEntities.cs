using System;
using UnityEngine;
namespace CampusGlitch {
public class CGSurface:MonoBehaviour{}
public class CGFrameAnimator:MonoBehaviour {
 public SpriteRenderer view;
 public string npcKind="",enemyKind="",assistantMode="idle";
 public bool hero=false, assistant=false;
 public int forced=-1;
 float timer; string lastState="";
 public void Update(){if(view==null)return;timer+=Time.deltaTime; int frame=(int)(timer*9);
   Sprite sprite=null;
   if(hero){CGHero h=GetComponentInParent<CGHero>(); if(h!=null){
       bool official=CGStore.Data.useOriginalWolf;
       if(!official && CGGuardianArt.Available)return;
       int startFrame=0,count=2;
       bool hurt=h.hurtUntil>Time.time;
       bool attacking=h.attackUntil>Time.time;
       bool ascending=h.Body!=null&&h.Body.linearVelocity.y>0f;
       if(official){
         if(hurt||attacking){startFrame=13;count=4;}
         else if(!h.Grounded){startFrame=10;count=3;}
         else if(Mathf.Abs(h.moveInput)>.15f){startFrame=h.running?6:2;count=4;}
       }else{
         if(hurt){startFrame=19;count=2;}
         else if(attacking){startFrame=15;count=4;}
         else if(!h.Grounded){startFrame=ascending?10:13;count=ascending?3:2;}
         else if(Mathf.Abs(h.moveInput)>.15f){startFrame=h.running?6:2;count=4;}
       }
       Sprite next=CGArt.Player(CGStore.Data.purpleEquipped,startFrame+frame%count,official);
       if(next!=null)view.sprite=next;
       view.flipX=!h.facesRight;
       // Light body bounce: active when running, idle breathing. Discrete pixel art stays crisp.
       float bob=h.Grounded?(Mathf.Abs(h.moveInput)>.1f?Mathf.Round(Mathf.Sin(Time.time*(h.running?19:13)))*.023f:Mathf.Round(Mathf.Sin(Time.time*3f))*.012f):0f;
       view.transform.localPosition=new Vector3(0,(official ? .82f : 1.0f)+bob,0);
       float height=official?1.85f:2.05f;
       if(view.sprite!=null){float spriteH=Mathf.Max(.01f,view.sprite.bounds.size.y);float sc=height/spriteH;view.transform.localScale=Vector3.one*sc;}
   }}
   else if(assistant){sprite=CGArt.Get("Playable/Assistant_"+assistantMode+"_"+(frame%3).ToString("00"));}
   else if(!string.IsNullOrEmpty(npcKind)){sprite=CGArt.NPC(npcKind,frame%4);}
   else if(!string.IsNullOrEmpty(enemyKind)){sprite=CGArt.Enemy(enemyKind,frame%4);}
   if(sprite!=null)view.sprite=sprite;
 }
}
public class CGHero:MonoBehaviour {
 const float JumpSpeed=14.4f; // Clears the raised first tier (up to 3.00 units) with the existing gravity.
 static PhysicsMaterial2D movementMaterial;
 public Rigidbody2D Body {get;private set;}
 public float moveInput;public bool facesRight=true,running,Grounded;
 public int hearts=3; public float hurtUntil,attackUntil;float lastGrounded=-999f,queuedJump=-999f,lastAttack;
 public CGAbilityState Skills {get;private set;}=new CGAbilityState();
 readonly System.Collections.Generic.List<RaycastHit2D> groundHits=new System.Collections.Generic.List<RaycastHit2D>();
 CGGame game;CGFrameAnimator anim;
 public void Setup(CGGame g){game=g;Body=gameObject.AddComponent<Rigidbody2D>();Body.gravityScale=3.2f;Body.freezeRotation=true;Body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
   CapsuleCollider2D co=gameObject.AddComponent<CapsuleCollider2D>();co.size=new Vector2(0.76f,1.64f);co.offset=new Vector2(0,0.82f);
   // Horizontal input must not create enough wall friction to hold the body in the air.
   if(movementMaterial==null)movementMaterial=new PhysicsMaterial2D("Lobo_Movimiento"){friction=0f,bounciness=0f};
   co.sharedMaterial=movementMaterial;
   GameObject art=new GameObject("LoboGuardián_Sprite");art.transform.SetParent(transform,false);art.transform.localPosition=new Vector3(0,1.0f,0);
   SpriteRenderer sr=art.AddComponent<SpriteRenderer>();sr.sortingOrder=25;sr.sprite=CGArt.Player(CGStore.Data.purpleEquipped,0,CGStore.Data.useOriginalWolf);
   anim=gameObject.AddComponent<CGFrameAnimator>();anim.view=sr;anim.hero=true;
   gameObject.AddComponent<CGGuardianVisual>().Configure(this,sr);
   gameObject.AddComponent<CGAbilityVisual>().Configure(this);
   // Keep the artwork legible; collider geometry remains independent from the sprite.
   art.transform.localScale=Vector3.one;
 }
 void Update(){if(game==null||!game.AcceptsPlayerInput){moveInput=0;return;}
   moveInput=CGControl.Axis(); running=CGControl.HoldRun();if(Mathf.Abs(moveInput)>0.07f)facesRight=moveInput>0;
   if(Grounded)lastGrounded=Time.time;
   bool jumpPressed=CGControl.Jump();if(jumpPressed)queuedJump=Time.time;
   if(Time.time-queuedJump<0.15f&&Time.time-lastGrounded<0.14f){Body.linearVelocity=new Vector2(Body.linearVelocity.x,JumpSpeed);queuedJump=-999;lastGrounded=-999;CGAudio.Play("Jump");}
   else if(jumpPressed&&!Grounded&&Skills.TryAirJump(CGAbilities.Unlocked(CGStore.Data,1))){Body.linearVelocity=new Vector2(Body.linearVelocity.x,CGAbilities.Stats(CGStore.Data,1).speed);queuedJump=-999;CGAudio.Play("Jump");}
   if(CGControl.Attack() && Time.time-lastAttack>0.48f){lastAttack=Time.time;attackUntil=Time.time+0.23f;CGAudio.Play("Attack");game.Attack(transform.position+new Vector3(facesRight?0.85f:-0.85f,0.7f,0));}
   if(CGControl.Ability())ActivateAbility(CGStore.Data.equippedAbility);
   if(CGControl.Interact())game.InteractNearPlayer();
 }
 void FixedUpdate(){if(Body==null||game==null)return;
    Grounded=false;groundHits.Clear();gameObject.scene.GetPhysicsScene2D().Raycast(transform.position+new Vector3(0,0.08f,0),Vector2.down,0.23f,new ContactFilter2D().NoFilter(),groundHits);
    foreach(var hit in groundHits){if(hit.collider!=null&&!hit.collider.isTrigger&&hit.collider.GetComponent<CGSurface>()!=null){Grounded=true;break;}}
    if(Grounded&&Body.linearVelocity.y<=.1f)Skills.Land();
    if(!game.Playing)return;
    float speed=running?5.8f:3.5f;
    Body.linearVelocity=new Vector2(Skills.Active(0,Time.time)?(facesRight?1:-1)*CGAbilities.Stats(CGStore.Data,0).speed:moveInput*speed,Body.linearVelocity.y);
    if(transform.position.y<-9f)game.KillHero();
 }
 public bool ActivateAbility(int id){
   if(game==null||!game.AcceptsPlayerInput)return false;
   var stats=CGAbilities.Stats(CGStore.Data,id);
   if(!Skills.TryActivate(id,CGAbilities.Unlocked(CGStore.Data,id),Time.time,stats)){game.Hint(CGAbilities.Unlocked(CGStore.Data,id)?"Habilidad recargando":"Habilidad bloqueada: "+CGAbilities.Gate(Mathf.Clamp(id,0,3)));return false;}
   if(id==3){attackUntil=Time.time+.25f;game.AttackPulse(transform.position+new Vector3(facesRight?1.1f:-1.1f,.8f,0),stats.range,stats.damage);}
   CGAudio.Play(id==3?"Attack":"Checkpoint");game.Hint(CGAbilities.Names[id]);return true;
 }
 public void Damage(int n){if(Time.time<hurtUntil||game==null||!game.Playing)return;if(Skills.Absorb(Time.time)){game.Hint("Escudo: golpe absorbido");return;}hearts-=n;hurtUntil=Time.time+1.1f;
   Body.AddForce(new Vector2(facesRight?-3f:3f,4f),ForceMode2D.Impulse);
   game.Hint("¡Cuidado! Señal de vida reducida.");if(hearts<=0)game.KillHero();}
}
public class CGEnemy:MonoBehaviour {
  public string kind;public bool boss;public int hp=2;public float left,right;CGGame game;float direction=-1,phase,attackTimer;
  public void Setup(CGGame g,string k,bool isBoss){game=g;kind=k;boss=isBoss;hp=isBoss?9:2;phase=UnityEngine.Random.value*3f;
    var sr=gameObject.AddComponent<SpriteRenderer>();sr.sortingOrder=21;sr.sprite=CGArt.Enemy(kind,0);
    var anim=gameObject.AddComponent<CGFrameAnimator>();anim.view=sr;anim.enemyKind=kind;
    var col=gameObject.AddComponent<CircleCollider2D>();col.radius=isBoss?0.95f:0.5f;col.isTrigger=true;
    left=transform.position.x-2.3f;right=transform.position.x+2.3f;
  }
  void Update(){if(game==null||!game.Playing)return;
    phase+=Time.deltaTime; float x=transform.position.x;
    if(!boss){x+=direction*Time.deltaTime*(kind=="Bug_Saltador"?2:1.2f);if(x<left||x>right)direction*=-1;}
    float baseY=kind=="Centinela"?1.4f:boss?0.65f:0.45f;
    if(kind=="Bug_Saltador")baseY+=Mathf.Abs(Mathf.Sin(phase*3f))*1.1f;
    if(kind=="Centinela"||boss)baseY+=Mathf.Sin(phase*2.1f)*0.25f;
    transform.position=new Vector3(x,baseY,0);
    if(game.Hero!=null){float d=Vector2.Distance(game.Hero.transform.position+Vector3.up*0.6f,transform.position);
     if(d<(boss?1.8f:0.85f) && Time.time>attackTimer){attackTimer=Time.time+(boss?1.6f:1.1f);game.Hero.Damage(1);}
     if(boss && Time.time>attackTimer && d<7f && Mathf.RoundToInt(phase)%3==0){attackTimer=Time.time+1.6f;game.Hint("¡Esquiva el pulso del jefe!");}
    }
  }
  public void Hurt(int damage){hp-=damage;game.Hint(boss?"Núcleo dañado":"Bug eliminado");if(hp<=0){if(boss)game.BossDown();Destroy(gameObject);}}
}
public class CGPickup:MonoBehaviour {
 public int id;CGGame game;
 public void Setup(CGGame g,int number){game=g;id=number;var c=gameObject.AddComponent<CircleCollider2D>();c.isTrigger=true;c.radius=1.35f;
   var sr=gameObject.AddComponent<SpriteRenderer>();sr.sprite=CGArt.Get("01_Nuevos_Modulares/Global_UI/CoinIcon");sr.sortingOrder=20;transform.localScale=Vector3.one*.35f;
 }
 void Update(){transform.localScale=Vector3.one*(.35f+0.03f*Mathf.Sin(Time.time*4f));}
 void OnTriggerEnter2D(Collider2D c){if(game!=null&&c.GetComponent<CGHero>()!=null){game.TakeChip(id);Destroy(gameObject);}}
}
public class CGParallax:MonoBehaviour {
 public Transform cameraTransform;public float startX;public float factor=.24f;
 void LateUpdate(){if(cameraTransform!=null)transform.position=new Vector3(startX+cameraTransform.position.x*factor,transform.position.y,transform.position.z);}
}
public class CGCamera:MonoBehaviour {
 public Transform follow; public float levelWidth=73;
 void LateUpdate(){if(follow==null)return;float y=Mathf.Max(2.2f,follow.position.y-1f);Vector3 target=new Vector3(Mathf.Clamp(follow.position.x+3,8,levelWidth-8),y,-10);transform.position=Vector3.Lerp(transform.position,target,Time.deltaTime*4f);}
}
}
