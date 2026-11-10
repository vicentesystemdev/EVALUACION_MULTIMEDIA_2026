using UnityEngine;

namespace CampusGlitch {
public enum CGAbility {Dash,DoubleJump,Shield,EnergyCut}
public struct CGAbilityStats {public int tier,damage;public float duration,cooldown,speed,range;}
public static class CGAbilities {
    public static readonly string[] Names={"DASH GLITCH","DOBLE SALTO","ESCUDO DIGITAL","CORTE DE ENERGÍA"};
    public static readonly string[] Descriptions={"Impulso frontal breve. Conserva las colisiones con obstáculos.","Un salto adicional en el aire; se recupera al aterrizar.","Protección durante 2 s que absorbe un golpe.","Ataque frontal de energía: mayor alcance y 2 puntos de daño."};
    public static readonly int[] RequiredLevels={0,1,2,3};
    public static readonly float[] Durations={.22f,.35f,2,.25f},Cooldowns={1.8f,0,6,2.5f};
    public static readonly int[] RewardAbility={1,2,3,0,2,3,1,0,3};
    public static readonly string[] Rewards={"DOBLE SALTO · un impulso extra en el aire","ESCUDO DIGITAL · absorbe un golpe","CORTE DE ENERGÍA · ataque de 2 de daño","DASH MEJORADO · recarga más rápida","ESCUDO MEJORADO · más duración","CORTE MEJORADO · 3 de daño","SALTO MEJORADO · mayor impulso aéreo","DASH ÉLITE · mayor impulso y menor recarga","CORTE ÉLITE · alcance y recarga mejorados"};
    public static bool Completed(CGProgress data,int level){return level==0||data!=null&&(data.latestUnlocked>=level||data.completed!=null&&data.completed.Length>=level&&data.completed[level-1]);}
    public static bool Unlocked(CGProgress data,int id){
        if(data==null||id<0||id>3)return false;int level=RequiredLevels[id];
        return Completed(data,level)||(id==2&&data.purpleUnlocked);
    }
    public static CGAbilityStats Stats(CGProgress data,int id,int previewLevel=0){
        id=Mathf.Clamp(id,0,3);bool Four=Completed(data,4)||previewLevel>=4,Five=Completed(data,5)||previewLevel>=5,Six=Completed(data,6)||previewLevel>=6,Seven=Completed(data,7)||previewLevel>=7,Eight=Completed(data,8)||previewLevel>=8,Nine=Completed(data,9)||previewLevel>=9;
        var stats=new CGAbilityStats{tier=1,duration=Durations[id],cooldown=Cooldowns[id],speed=id==1?11:10,range=1.3f,damage=2};
        if(id==0){stats.tier=Eight?3:Four?2:1;stats.speed=Eight?12:Four?11:10;stats.cooldown=Eight?1.2f:Four?1.5f:1.8f;}
        if(id==1){stats.tier=Seven?2:1;stats.speed=Seven?12:11;}
        if(id==2){stats.tier=Five?2:1;stats.duration=Five?2.5f:2;stats.cooldown=Five?5:6;}
        if(id==3){stats.tier=Nine?3:Six?2:1;stats.damage=Six?3:2;stats.range=Nine?1.6f:Six?1.45f:1.3f;stats.cooldown=Nine?2:Six?2.2f:2.5f;}
        return stats;
    }
    public static string Description(CGProgress data,int id,int previewLevel=0){var s=Stats(data,id,previewLevel);return id==2?"Absorbe un golpe durante "+s.duration.ToString("0.0")+" s.":id==3?"Ataque frontal de "+s.damage+" de daño. Alcance "+(s.tier==3?"ampliado":"corto")+".":Descriptions[id];}
    public static string Gate(int id){return RequiredLevels[id]==0?"DISPONIBLE DESDE EL INICIO":"RESTAURA EL NIVEL "+RequiredLevels[id];}
    public static Color ColorFor(int choice){return choice==1?new Color(.3f,.9f,1):choice==2?new Color(.8f,.35f,1):new Color(1,.55f,.12f);}
}
// The controller and the customization demonstration share timing and activation rules.
public sealed class CGAbilityState {
    readonly float[] ready=new float[4],until=new float[4];bool airJumpUsed;
    public bool Active(int id,float now){return id>=0&&id<4&&now<until[id];}
    public float Remaining(int id,float now){return Mathf.Max(0,ready[Mathf.Clamp(id,0,3)]-now);}
    public bool TryActivate(int id,bool unlocked,float now){
        return TryActivate(id,unlocked,now,CGAbilities.Stats(null,id));
    }
    public bool TryActivate(int id,bool unlocked,float now,CGAbilityStats stats){
        if(id<0||id>3||id==1||!unlocked||now<ready[id])return false;
        until[id]=now+stats.duration;ready[id]=now+stats.cooldown;return true;
    }
    public bool TryAirJump(bool unlocked){if(!unlocked||airJumpUsed)return false;airJumpUsed=true;return true;}
    public void Land(){airJumpUsed=false;}
    public bool Absorb(float now){if(!Active(2,now))return false;until[2]=0;return true;}
    public void Cancel(){for(int i=0;i<4;i++)until[i]=0;airJumpUsed=false;}
}
}
