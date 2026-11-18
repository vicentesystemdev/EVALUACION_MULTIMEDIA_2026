using UnityEngine;
namespace CampusGlitch {
public static class CGAbilityArt {
    static readonly Texture2D[] icons=new Texture2D[4];static Sprite slash;
    static readonly Texture2D[] cards=new Texture2D[4];
    static readonly string[] cardNames={"Dash","EnergyBurst","Shield","EnergyCut"};
    public static Texture2D Card(int id){id=Mathf.Clamp(id,0,3);if(cards[id]==null)cards[id]=Resources.Load<Texture2D>("CG/Abilities/"+cardNames[id]);return cards[id]!=null?cards[id]:Icon(id);}
    static float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
    public static Texture2D Icon(int id){
        id=Mathf.Clamp(id,0,3);if(icons[id]!=null)return icons[id];const int s=96;var pixels=new Color[s*s];
        for(int y=0;y<s;y++)for(int x=0;x<s;x++){
            var p=new Vector2(x,y);float radius=Vector2.Distance(p,new Vector2(48,48));bool on=false;
            if(id==0)on=Segment(p,new Vector2(25,25),new Vector2(57,48))<4||Segment(p,new Vector2(57,48),new Vector2(25,71))<4||Segment(p,new Vector2(47,25),new Vector2(79,48))<4||Segment(p,new Vector2(79,48),new Vector2(47,71))<4;
            if(id==1)on=Mathf.Abs(Vector2.Distance(p,new Vector2(60,48))-19)<4||Segment(p,new Vector2(9,35),new Vector2(39,40))<3||Segment(p,new Vector2(9,61),new Vector2(39,56))<3;
            if(id==2)on=Mathf.Abs(radius-36)<3||(Mathf.Abs(x-48)<3&&y>29&&y<66)||(Mathf.Abs(y-48)<3&&x>29&&x<66);
            if(id==3){float r=Vector2.Distance(p,new Vector2(28,48));on=x>37&&y>12&&y<84&&(Mathf.Abs(r-22)<3||Mathf.Abs(r-32)<3||Mathf.Abs(r-42)<3);}
            if(on)pixels[y*s+x]=Color.white;
        }
        var tex=new Texture2D(s,s,TextureFormat.RGBA32,false){name="Habilidad_"+id,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};tex.SetPixels(pixels);tex.Apply(false,true);return icons[id]=tex;
    }
    public static Sprite Effect(bool cut){
        if(!cut)return FxSprite(0);
        if(slash!=null)return slash;
        var sprite=Sprite.Create(Icon(3),new Rect(0,0,96,96),new Vector2(.5f,.5f),48);sprite.hideFlags=HideFlags.HideAndDontSave;return slash=sprite;
    }
    static readonly Texture2D[] fxTextures=new Texture2D[3];static readonly Sprite[] fxSprites=new Sprite[3];
    public static Texture2D FxTexture(int kind){
        if(fxTextures[kind]!=null)return fxTextures[kind];const int size=192;var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            var p=new Vector2((x-95.5f)/96,(y-95.5f)/96);float r=p.magnitude,alpha=0;Color color=Color.white;
            if(kind==0){
                if(r<.89f){alpha=.045f;int row=Mathf.RoundToInt(y/21f),col=Mathf.RoundToInt((x-(row%2)*12.125f)/24.25f);var center=new Vector2(col*24.25f+(row%2)*12.125f,row*21f);float edge=100;
                    for(int side=0;side<6;side++){float a=(side*60+30)*Mathf.Deg2Rad,b=((side+1)*60+30)*Mathf.Deg2Rad;edge=Mathf.Min(edge,Segment(new Vector2(x,y),center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*14,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*14));}
                    if(edge<.8f)alpha=.22f;
                }
                alpha=Mathf.Max(alpha,Mathf.Exp(-Mathf.Abs(r-.89f)*95)*.9f);if(r>.98f)alpha=0;
            }else if(kind==1){
                float orb=Vector2.Distance(p,new Vector2(.3f,0));float core=Mathf.Clamp01(1-orb/.3f),rim=Mathf.Exp(-Mathf.Abs(orb-.34f)*30);float trail=p.x<.3f?Mathf.Clamp01(1-Mathf.Abs(p.y)/(.035f+(.3f-p.x)*.17f))*Mathf.Clamp01((p.x+1)/1.2f):0;
                alpha=Mathf.Max(core,Mathf.Max(rim*.8f,trail*.75f));color=Color.Lerp(new Color(.65f,.15f,1),new Color(.25f,.95f,1),rim);if(core>.5f)color=Color.Lerp(color,Color.white,(core-.5f)*2);
            }else{float spokes=Mathf.Pow(Mathf.Abs(Mathf.Cos(Mathf.Atan2(p.y,p.x)*6)),12);alpha=Mathf.Clamp01(1-r)*spokes+Mathf.Exp(-Mathf.Abs(r-.5f)*35)*.7f;}
            pixels[y*size+x]=new Color(color.r,color.g,color.b,Mathf.Clamp01(alpha));
        }
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Poder_Visual_"+kind,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};texture.SetPixels(pixels);texture.Apply(false,true);return fxTextures[kind]=texture;
    }
    public static Sprite FxSprite(int kind){if(fxSprites[kind]!=null)return fxSprites[kind];var sprite=Sprite.Create(FxTexture(kind),new Rect(0,0,192,192),new Vector2(.5f,.5f),128);sprite.hideFlags=HideFlags.HideAndDontSave;return fxSprites[kind]=sprite;}
}
public class CGAbilityVisual:MonoBehaviour {
    CGHero hero;SpriteRenderer shield,cut,source;readonly SpriteRenderer[] trail=new SpriteRenderer[4];float nextTrail,shieldImpactUntil;
    public void ShieldImpact(){shieldImpactUntil=Time.time+.22f;CGPowerImpact.Spawn(transform.parent,(Vector3)hero.Body.position+new Vector3(hero.facesRight?.7f:-.7f,1,0),new Color(.3f,.85f,1));}
    public void Configure(CGHero owner){hero=owner;source=transform.Find("LoboGuardián_Sprite").GetComponent<SpriteRenderer>();shield=Create("Escudo_Digital",CGAbilityArt.Effect(false));cut=Create("Corte_Energia",CGAbilityArt.Effect(true));for(int i=0;i<4;i++)trail[i]=Create("Rastro_"+i,null);}
    SpriteRenderer Create(string label,Sprite sprite){var obj=new GameObject(label);obj.transform.SetParent(transform,false);var sr=obj.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=27;sr.enabled=false;return sr;}
    void LateUpdate(){
        if(hero==null||hero.Body==null)return;Color color=CGAbilities.ColorFor(CGStore.Data.purpleEquipped?2:0);
        bool impact=Time.time<shieldImpactUntil;shield.enabled=hero.Skills.Active(2,Time.time)||impact;shield.transform.localPosition=new Vector3(0,1.02f,0);float pulse=CGStore.Data.reduceFlash?1:1+Mathf.Sin(Time.time*3)*.015f;shield.transform.localScale=new Vector3(1.5f,1.8f,1)*pulse;shield.color=new Color(.24f,.82f,1,impact?.95f:.72f);
        cut.enabled=hero.Skills.Active(3,Time.time);cut.transform.localPosition=new Vector3(hero.facesRight?1.1f:-1.1f,.85f,0);cut.transform.localScale=Vector3.one*(CGAbilities.Stats(CGStore.Data,3).range/1.3f);cut.flipX=!hero.facesRight;cut.color=color;
        if(hero.Skills.Active(0,Time.time)&&Time.time>=nextTrail){nextTrail=Time.time+.05f;for(int i=3;i>0;i--){trail[i].sprite=trail[i-1].sprite;trail[i].transform.position=trail[i-1].transform.position;trail[i].transform.localScale=trail[i-1].transform.localScale;trail[i].flipX=trail[i-1].flipX;}trail[0].sprite=source.sprite;trail[0].transform.position=source.transform.position;trail[0].transform.localScale=source.transform.localScale;trail[0].flipX=source.flipX;}
        for(int i=0;i<4;i++){float alpha=hero.Skills.Active(0,Time.time)?(.24f-i*.045f):0;trail[i].enabled=alpha>0&&!CGStore.Data.reduceFlash;trail[i].color=new Color(color.r,color.g,color.b,alpha);trail[i].sortingOrder=23;}
    }
}
}
