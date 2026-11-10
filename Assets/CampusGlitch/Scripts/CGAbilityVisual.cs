using UnityEngine;
namespace CampusGlitch {
public static class CGAbilityArt {
    static readonly Texture2D[] icons=new Texture2D[4];static Sprite ring,slash;
    static readonly Texture2D[] cards=new Texture2D[4];
    static readonly string[] cardNames={"Dash","DoubleJump","Shield","EnergyCut"};
    public static Texture2D Card(int id){id=Mathf.Clamp(id,0,3);if(cards[id]==null)cards[id]=Resources.Load<Texture2D>("CG/Abilities/"+cardNames[id]);return cards[id]!=null?cards[id]:Icon(id);}
    static float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
    public static Texture2D Icon(int id){
        id=Mathf.Clamp(id,0,3);if(icons[id]!=null)return icons[id];const int s=96;var pixels=new Color[s*s];
        for(int y=0;y<s;y++)for(int x=0;x<s;x++){
            var p=new Vector2(x,y);float radius=Vector2.Distance(p,new Vector2(48,48));bool on=false;
            if(id==0)on=Segment(p,new Vector2(25,25),new Vector2(57,48))<4||Segment(p,new Vector2(57,48),new Vector2(25,71))<4||Segment(p,new Vector2(47,25),new Vector2(79,48))<4||Segment(p,new Vector2(79,48),new Vector2(47,71))<4;
            if(id==1)on=Segment(p,new Vector2(20,34),new Vector2(48,60))<4||Segment(p,new Vector2(48,60),new Vector2(76,34))<4||Segment(p,new Vector2(20,54),new Vector2(48,80))<4||Segment(p,new Vector2(48,80),new Vector2(76,54))<4;
            if(id==2)on=Mathf.Abs(radius-36)<3||(Mathf.Abs(x-48)<3&&y>29&&y<66)||(Mathf.Abs(y-48)<3&&x>29&&x<66);
            if(id==3){float r=Vector2.Distance(p,new Vector2(28,48));on=x>37&&y>12&&y<84&&(Mathf.Abs(r-22)<3||Mathf.Abs(r-32)<3||Mathf.Abs(r-42)<3);}
            if(on)pixels[y*s+x]=Color.white;
        }
        var tex=new Texture2D(s,s,TextureFormat.RGBA32,false){name="Habilidad_"+id,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};tex.SetPixels(pixels);tex.Apply(false,true);return icons[id]=tex;
    }
    public static Sprite Effect(bool cut){
        if(cut&&slash!=null)return slash;if(!cut&&ring!=null)return ring;
        var sprite=Sprite.Create(Icon(cut?3:2),new Rect(0,0,96,96),new Vector2(.5f,.5f),48);sprite.hideFlags=HideFlags.HideAndDontSave;
        if(cut)slash=sprite;else ring=sprite;return sprite;
    }
}
public class CGAbilityVisual:MonoBehaviour {
    CGHero hero;SpriteRenderer shield,cut,source;readonly SpriteRenderer[] trail=new SpriteRenderer[4];float nextTrail;
    public void Configure(CGHero owner){hero=owner;source=transform.Find("LoboGuardián_Sprite").GetComponent<SpriteRenderer>();shield=Create("Escudo_Digital",CGAbilityArt.Effect(false));cut=Create("Corte_Energia",CGAbilityArt.Effect(true));for(int i=0;i<4;i++)trail[i]=Create("Rastro_"+i,null);}
    SpriteRenderer Create(string label,Sprite sprite){var obj=new GameObject(label);obj.transform.SetParent(transform,false);var sr=obj.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=27;sr.enabled=false;return sr;}
    void LateUpdate(){
        if(hero==null||hero.Body==null)return;Color color=CGAbilities.ColorFor(CGStore.Data.purpleEquipped?2:0);
        shield.enabled=hero.Skills.Active(2,Time.time);shield.transform.localPosition=new Vector3(0,.9f,0);shield.transform.localScale=Vector3.one*1.23f;shield.color=new Color(color.r,color.g,color.b,.7f);
        cut.enabled=hero.Skills.Active(3,Time.time);cut.transform.localPosition=new Vector3(hero.facesRight?1.1f:-1.1f,.85f,0);cut.transform.localScale=Vector3.one*(CGAbilities.Stats(CGStore.Data,3).range/1.3f);cut.flipX=!hero.facesRight;cut.color=color;
        if(hero.Skills.Active(0,Time.time)&&Time.time>=nextTrail){nextTrail=Time.time+.05f;for(int i=3;i>0;i--){trail[i].sprite=trail[i-1].sprite;trail[i].transform.position=trail[i-1].transform.position;trail[i].transform.localScale=trail[i-1].transform.localScale;trail[i].flipX=trail[i-1].flipX;}trail[0].sprite=source.sprite;trail[0].transform.position=source.transform.position;trail[0].transform.localScale=source.transform.localScale;trail[0].flipX=source.flipX;}
        for(int i=0;i<4;i++){float alpha=hero.Skills.Active(0,Time.time)?(.24f-i*.045f):0;trail[i].enabled=alpha>0&&!CGStore.Data.reduceFlash;trail[i].color=new Color(color.r,color.g,color.b,alpha);trail[i].sortingOrder=23;}
    }
}
}
