using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CampusGlitch {
public sealed class CGPowerProjectile:MonoBehaviour {
    readonly List<RaycastHit2D> hits=new List<RaycastHit2D>();
    CGGame game;CGHero owner;CGAbilityStats stats;Vector2 direction;float travelled;
    public bool Finished {get;private set;}
    public static CGPowerProjectile Spawn(CGGame game,CGHero owner,CGAbilityStats stats){
        var obj=new GameObject("Rafaga_Glitch");SceneManager.MoveGameObjectToScene(obj,owner.gameObject.scene);obj.transform.SetParent(owner.transform.parent,false);
        obj.transform.position=(Vector3)owner.Body.position+new Vector3(owner.facesRight?.3f:-.3f,.95f,0);
        var view=obj.AddComponent<SpriteRenderer>();view.sprite=CGAbilityArt.FxSprite(1);view.sortingOrder=29;view.flipX=!owner.facesRight;
        obj.transform.localScale=new Vector3(.8f,.55f,1);var shot=obj.AddComponent<CGPowerProjectile>();shot.game=game;shot.owner=owner;shot.stats=stats;shot.direction=owner.facesRight?Vector2.right:Vector2.left;return shot;
    }
    void Update(){Advance(Time.deltaTime);}
    public void Advance(float seconds){
        if(Finished||game==null||!game.Playing||seconds<=0)return;
        float distance=Mathf.Min(stats.speed*seconds,stats.range-travelled);hits.Clear();gameObject.scene.GetPhysicsScene2D().CircleCast(transform.position,.16f,direction,distance,new ContactFilter2D().NoFilter(),hits);
        foreach(var hit in hits){
            if(hit.collider==null||hit.collider.GetComponentInParent<CGHero>()==owner)continue;
            var enemy=hit.collider.GetComponentInParent<CGEnemy>();bool wall=!hit.collider.isTrigger&&hit.collider.GetComponent<CGSurface>()!=null;
            if(enemy==null&&!wall)continue;
            transform.position+=(Vector3)(direction*Mathf.Max(0,hit.distance));if(enemy!=null)enemy.Hurt(stats.damage);Finish(true);return;
        }
        transform.position+=(Vector3)(direction*distance);travelled+=distance;if(travelled>=stats.range)Finish(false);
    }
    void Finish(bool impact){Finished=true;if(impact)CGPowerImpact.Spawn(transform.parent,transform.position,new Color(.55f,.65f,1));Destroy(gameObject);}
}
public sealed class CGPowerImpact:MonoBehaviour {
    SpriteRenderer view;Color tint;float age;
    public static void Spawn(Transform parent,Vector3 point,Color color){
        var obj=new GameObject("Impacto_Energia");if(parent!=null)obj.transform.SetParent(parent,false);obj.transform.position=point;
        var effect=obj.AddComponent<CGPowerImpact>();effect.view=obj.AddComponent<SpriteRenderer>();effect.view.sprite=CGAbilityArt.FxSprite(2);effect.view.sortingOrder=31;effect.tint=color;effect.view.color=color;obj.transform.localScale=Vector3.one*.35f;
    }
    void Update(){age+=Time.deltaTime;float t=age/.32f;transform.localScale=Vector3.one*Mathf.Lerp(.35f,1.2f,t);view.color=new Color(tint.r,tint.g,tint.b,1-t);if(t>=1)Destroy(gameObject);}
}
}
