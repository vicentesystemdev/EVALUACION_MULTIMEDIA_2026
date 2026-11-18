using UnityEngine;
namespace CampusGlitch {
[CreateAssetMenu(menuName="Campus Glitch/Diseño del mapa",fileName="CampusMap")]
public sealed class CGMapDesign:ScriptableObject {
    public string title="MAPA DEL CAMPUS",subtitle="ELIGE TU DESTINO · DESCUBRE SUS ACTIVIDADES";
    public Vector2 cardOrigin=new Vector2(30,181),cardSize=new Vector2(268,118),cardGap=new Vector2(14,16);
    public Color panel=new Color(.025f,.04f,.085f,1);
    [Range(0,6)]public float selectionLift=3;
    public bool animateRoutes=true;
    void OnValidate(){cardSize=new Vector2(Mathf.Clamp(cardSize.x,250,268),Mathf.Clamp(cardSize.y,110,118));cardGap=new Vector2(Mathf.Clamp(cardGap.x,12,14),Mathf.Clamp(cardGap.y,14,16));cardOrigin.x=Mathf.Clamp(cardOrigin.x,24,862-3*cardSize.x-2*cardGap.x);cardOrigin.y=Mathf.Clamp(cardOrigin.y,174,568-3*cardSize.y-2*cardGap.y);panel.a=1;}
}
}
