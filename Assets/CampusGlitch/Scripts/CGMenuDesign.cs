using UnityEngine;

namespace CampusGlitch {
[CreateAssetMenu(menuName="Campus Glitch/Diseño del menú",fileName="CampusMenu")]
public class CGMenuDesign:ScriptableObject {
    [Header("Identidad y fondos")]
    public string title="CAMPUS GLITCH";
    public string campus="UNIFRANZ LA PAZ · INNOVACIÓN · APRENDE HACIENDO";
    public Texture2D menuBackground,mapBackground;
    public Color amber=new Color(1,.56f,.12f),cyan=new Color(.3f,.87f,1),ink=new Color(.025f,.04f,.085f,.96f);
    [Header("Tipografía")]
    public Font titleFont,buttonFont,bodyFont;
    [Range(24,60)]public int titleSize=42;
    [Range(20,38)]public int buttonSize=28;
    [Range(16,28)]public int bodySize=22;
    [Header("Distribución · base 1280 × 720")]
    public Rect titleRect=new Rect(35,7,620,59);
    public Rect heroRect=new Rect(138,220,360,410);
    public Rect mainPanel=new Rect(720,133,514,522);
    public Vector2 firstButton=new Vector2(745,150);
    public Vector2 buttonSizePixels=new Vector2(463,60);
    [Range(50,74)]public float buttonSpacing=70;
    public string[] mainLabels={"JUGAR / CONTINUAR","MAPA DEL CAMPUS","PERSONALIZACIÓN","CONFIGURACIÓN","AYUDA Y CONTROLES","CRÉDITOS","SALIR"};
    [Header("Movimiento y transiciones")]
    [Range(.1f,.5f)]public float screenTransition=.22f;
    [Range(2,12)]public float focusSpeed=8;
    [Range(0,8)]public float backgroundDrift=5;
    [Range(0,30)]public float screenSlide=16;
    void OnValidate(){
        buttonSpacing=Mathf.Max(buttonSpacing,buttonSizePixels.y+3);
        firstButton.x=Mathf.Clamp(firstButton.x,0,1280-Mathf.Max(100,buttonSizePixels.x));
        firstButton.y=Mathf.Clamp(firstButton.y,98,676-7*Mathf.Min(70,buttonSpacing));
    }
}
}
