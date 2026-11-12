using UnityEngine;

namespace CampusGlitch {
public enum CGAbility {Dash,EnergyBurst,Shield,EnergyCut}
public struct CGAbilityStats {public int tier,damage,cost;public float duration,cooldown,speed,range;}
public static class CGAbilities {
    public static readonly string[] Names={"DASH GLITCH","RÁFAGA GLITCH","ESCUDO DIGITAL","CORTE DE ENERGÍA"};
    public static readonly string[] Descriptions={"Impulso rápido hacia delante para esquivar peligros.","Dispara un núcleo de energía que impacta al primer enemigo de su recorrido.","Cúpula de energía que absorbe un golpe.","Ataque frontal de energía: mayor alcance y 2 puntos de daño."};
    public static readonly int[] RequiredLevels={0,1,2,3};
    public static readonly float[] Durations={.22f,.35f,2,.25f},Cooldowns={1.8f,1.1f,6,2.5f};
    public static readonly int[] Costs={20,18,40,30};
    public static readonly int[] RewardAbility={1,2,3,0,2,3,1,0,3};
    public static readonly string[] Rewards={"RÁFAGA GLITCH · proyectil de energía","ESCUDO DIGITAL · absorbe un golpe","CORTE DE ENERGÍA · ataque de 2 de daño","DASH MEJORADO · recarga más rápida","ESCUDO MEJORADO · más duración","CORTE MEJORADO · 3 de daño","RÁFAGA MEJORADA · más daño y alcance","DASH ÉLITE · mayor impulso y menor recarga","CORTE ÉLITE · alcance y recarga mejorados"};
    public static bool Completed(CGProgress data,int level){return level==0||data!=null&&(data.latestUnlocked>=level||data.completed!=null&&data.completed.Length>=level&&data.completed[level-1]);}
    public static bool Unlocked(CGProgress data,int id){
        if(data==null||id<0||id>3)return false;int level=RequiredLevels[id];
        return Completed(data,level)||(id==2&&data.purpleUnlocked);
    }
    public static CGAbilityStats Stats(CGProgress data,int id,int previewLevel=0){
        id=Mathf.Clamp(id,0,3);bool Four=Completed(data,4)||previewLevel>=4,Five=Completed(data,5)||previewLevel>=5,Six=Completed(data,6)||previewLevel>=6,Seven=Completed(data,7)||previewLevel>=7,Eight=Completed(data,8)||previewLevel>=8,Nine=Completed(data,9)||previewLevel>=9;
        var stats=new CGAbilityStats{tier=1,duration=Durations[id],cooldown=Cooldowns[id],speed=10,range=1.3f,damage=2,cost=Costs[id]};
        if(id==0){stats.tier=Eight?3:Four?2:1;stats.speed=Eight?12:Four?11:10;stats.cooldown=Eight?1.2f:Four?1.5f:1.8f;}
        if(id==1){stats.tier=Seven?2:1;stats.speed=Seven?15:12;stats.range=Seven?9:7;stats.damage=Seven?2:1;stats.cooldown=Seven?.9f:1.1f;}
        if(id==2){stats.tier=Five?2:1;stats.duration=Five?2.5f:2;stats.cooldown=Five?5:6;}
        if(id==3){stats.tier=Nine?3:Six?2:1;stats.damage=Six?3:2;stats.range=Nine?1.6f:Six?1.45f:1.3f;stats.cooldown=Nine?2:Six?2.2f:2.5f;}
        return stats;
    }
    public static string Description(CGProgress data,int id,int previewLevel=0){var s=Stats(data,id,previewLevel);return id==1?"Proyectil frontal de "+s.damage+" de daño. Se detiene al impactar.":id==2?"Absorbe un golpe durante "+s.duration.ToString("0.0")+" s.":id==3?"Ataque frontal de "+s.damage+" de daño. Alcance "+(s.tier==3?"ampliado":"corto")+".":Descriptions[id];}
    public static string Gate(int id){return RequiredLevels[id]==0?"DISPONIBLE DESDE EL INICIO":"RESTAURA EL NIVEL "+RequiredLevels[id];}
    public static Color ColorFor(int choice){return choice==1?new Color(.3f,.9f,1):choice==2?new Color(.8f,.35f,1):new Color(1,.55f,.12f);}
}
// The controller and the customization demonstration share timing and activation rules.
public sealed class CGAbilityState {
    readonly float[] ready=new float[4],until=new float[4];float rechargeWait;
    public const float MaximumEnergy=100,RechargeRate=15;
    public float Energy {get;private set;}=MaximumEnergy;
    public void Tick(float seconds){seconds=Mathf.Max(0,seconds);float recover=Mathf.Max(0,seconds-rechargeWait);rechargeWait=Mathf.Max(0,rechargeWait-seconds);Energy=Mathf.Min(MaximumEnergy,Energy+recover*RechargeRate);}
    public bool Active(int id,float now){return id>=0&&id<4&&now<until[id];}
    public float ActiveRemaining(int id,float now){return Mathf.Max(0,until[Mathf.Clamp(id,0,3)]-now);}
    public float Remaining(int id,float now){return Mathf.Max(0,ready[Mathf.Clamp(id,0,3)]-now);}
    public bool TryActivate(int id,bool unlocked,float now){
        return TryActivate(id,unlocked,now,CGAbilities.Stats(null,id));
    }
    public bool TryActivate(int id,bool unlocked,float now,CGAbilityStats stats){
        if(id<0||id>3||!unlocked||now<ready[id]||Energy<stats.cost)return false;
        Energy-=stats.cost;rechargeWait=Mathf.Max(1.2f,stats.duration);until[id]=now+stats.duration;ready[id]=now+stats.cooldown;return true;
    }
    public bool Absorb(float now){if(!Active(2,now))return false;until[2]=0;return true;}
    public void Cancel(){for(int i=0;i<4;i++)until[i]=0;}
}
}
