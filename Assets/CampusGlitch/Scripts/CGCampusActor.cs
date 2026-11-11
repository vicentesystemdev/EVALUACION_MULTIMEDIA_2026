using UnityEngine;

namespace CampusGlitch {
/// <summary>Visuals follow existing actors; collision and combat remain on the controller.</summary>
public class CGCampusActor:MonoBehaviour {
    CGWorld world;SpriteRenderer view;bool enemy;int row;float scale,previousX;
    public void Configure(CGWorld owner,bool isEnemy,int animationRow,float height,float footOffset){
        world=owner;enemy=isEnemy;row=animationRow;
        var legacy=GetComponent<SpriteRenderer>();if(legacy!=null)legacy.enabled=false;
        var animator=GetComponent<CGFrameAnimator>();if(animator!=null)animator.enabled=false;
        transform.localScale=Vector3.one;
        var art=new GameObject("Arte_Campus_Actual");art.transform.SetParent(transform,false);
        art.transform.localPosition=new Vector3(0,-footOffset,0);
        view=art.AddComponent<SpriteRenderer>();view.sortingOrder=legacy!=null?legacy.sortingOrder:21;
        if(CGCampusActors.Material!=null)view.sharedMaterial=CGCampusActors.Material;
        scale=height/CGCampusActors.Frame(enemy,row,0).bounds.size.y;
        art.transform.localScale=Vector3.one*scale;previousX=transform.position.x;Render(0);
    }
    void Render(int frame){if(view!=null)view.sprite=CGCampusActors.Frame(enemy,row,frame);}
    public void SamplePreview(float time){Render(enemy?(int)(time*8)%8:row==3?(int)(time*3)%4:time%4<.18f?3:0);}
    void LateUpdate(){
        if(view==null)return;
        int frame;
        if(enemy){
            frame=(int)(Time.time*(row==2?7:10))%8;
            float delta=transform.position.x-previousX;
            if(Mathf.Abs(delta)>.0001f)view.flipX=delta>0;
            previousX=transform.position.x;
        }else if(row==3){frame=(int)(Time.time*3)%4;}
        else{
            var npc=GetComponent<CGCampusNPC>();
            frame=npc!=null&&npc.Talking?1+(int)(Time.time*3)%2:(Time.time%4>.0f&&Time.time%4<.18f?3:0);
            if(world!=null&&world.hero!=null)view.flipX=world.hero.transform.position.x<transform.position.x;
        }
        Render(frame);
    }
}
}
