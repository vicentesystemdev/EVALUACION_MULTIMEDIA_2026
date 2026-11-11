using UnityEngine;
namespace CampusGlitch {
[CreateAssetMenu(menuName="Campus Glitch/Diseño de configuración",fileName="CampusSettings")]
public sealed class CGSettingsDesign:ScriptableObject {
    public string title="CONFIGURACIÓN",subtitle="AJUSTA EL CAMPUS A TU MANERA";
    public string[] sections={"Pantalla","Audio","Accesibilidad","Partida"};
    public Texture2D background;
    public bool useCampusPalette=true;
    public Color accent=new Color(.78f,.35f,1),active=new Color(1,.56f,.12f),panel=new Color(.035f,.02f,.09f,1);
    public Rect sidebar=new Rect(24,147,211,471),content=new Rect(251,147,1005,471);
    [Range(.1f,.4f)]public float sectionTransition=.2f;
    public bool animateFocus=true;
    void OnValidate(){sidebar.x=Mathf.Clamp(sidebar.x,12,30);sidebar.width=Mathf.Clamp(sidebar.width,195,218);sidebar.y=Mathf.Clamp(sidebar.y,135,160);sidebar.height=Mathf.Clamp(sidebar.height,470,478);content.x=Mathf.Clamp(content.x,sidebar.xMax+12,267);content.width=1256-content.x;content.y=sidebar.y;content.height=sidebar.height;}
}
}
