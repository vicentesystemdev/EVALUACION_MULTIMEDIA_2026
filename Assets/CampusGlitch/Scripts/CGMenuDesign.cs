using UnityEngine;
using UnityEngine.Serialization;

namespace CampusGlitch {
[CreateAssetMenu(menuName="Campus Glitch/Diseño del menú",fileName="CampusMenu")]
public class CGMenuDesign:ScriptableObject {
    [Header("Identidad y fondos")]
    public string title="CAMPUS GLITCH";
    public string campus="UNIFRANZ LA PAZ · INNOVACIÓN · APRENDE HACIENDO";
    public Texture2D menuBackground,mapBackground,entranceBackground;
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
    public Texture2D[] mainIcons=new Texture2D[7];
    public string[] mainDescriptions={"Retoma tu recorrido por el campus.","Explora ambientes, rutas y desbloqueos.","Elige la presentación de tu Guardián.","Adapta sonido, efectos y controles.","Consulta las acciones antes de jugar.","Conoce al equipo de Campus Glitch.","Hasta la próxima aventura."};
    public string newGameLabel="Iniciar aventura";
    public string continueLabel="Continuar · Nivel {0}";
    public bool showButtonIcons=true;
    [Header("Transparencia y contraste del menú principal")]
    [Range(0,1)]public float mainPanelOpacity=.06f;
    [Range(.35f,1)]public float buttonOpacity=.78f;
    [Range(0,.7f)]public float headerShade=.45f;
    [Range(0,.6f)]public float rightShade=.16f;
    public string location="UNIFRANZ · SEDE LA PAZ";
    public string mainCaption="EXPLORA · APRENDE · RESTAURA EL CAMPUS";
    [Header("Movimiento y transiciones")]
    [Range(.1f,.5f)]public float screenTransition=.22f;
    [Range(2,12)]public float focusSpeed=8;
    [Range(0,8)]public float backgroundDrift=5;
    [Range(0,30)]public float screenSlide=16;
    public bool campusEntrance=true;
    public Rect entranceDoorRect=new Rect(548,221,184,266);
    [Range(.6f,2)]public float entranceDuration=1.25f;
    [FormerlySerializedAs("matrixGlitch")]public bool glitchHighlights=true;
    [FormerlySerializedAs("matrixIntensity"),Range(0,1)]public float glitchIntensity=.65f;
    [FormerlySerializedAs("matrixSpeed"),Range(.3f,2)]public float glitchSpeed=1.2f;
    public bool guardianGlitch=true,cycleGuardianVariants=true;
    [Range(4,12)]public float guardianCycleSeconds=6;
    [Range(.25f,1)]public float guardianGlitchDuration=.6f;
    [Header("Controles · paneles y mando editable")]
    public Rect controlsActionsPanel=new Rect(24,177,420,390);
    public Rect controlsDevicePanel=new Rect(460,177,796,390);
    public Texture2D controllerXbox,controllerPlayStation,controllerButtonStates;
    public Texture2D keyboardBackground,keyboardKeyStates;
    public Color controllerBody=new Color(.065f,.08f,.18f),controllerEdge=new Color(.62f,.35f,1);
    [Range(0,.15f)]public float controllerPressScale=.07f;
    public bool animateControlPresses=true;
    void OnValidate(){
        buttonSizePixels.x=Mathf.Clamp(buttonSizePixels.x,220,1160);
        buttonSizePixels.y=Mathf.Clamp(buttonSizePixels.y,44,65);
        buttonSpacing=Mathf.Clamp(buttonSpacing,buttonSizePixels.y+3,75);
        firstButton.x=Mathf.Clamp(firstButton.x,12,1268-buttonSizePixels.x);
        firstButton.y=Mathf.Clamp(firstButton.y,100,664-6*buttonSpacing-buttonSizePixels.y);
        controlsActionsPanel.width=Mathf.Clamp(controlsActionsPanel.width,340,470);
        controlsActionsPanel.height=Mathf.Clamp(controlsActionsPanel.height,350,410);
        controlsActionsPanel.x=Mathf.Clamp(controlsActionsPanel.x,12,580-controlsActionsPanel.width);
        controlsActionsPanel.y=Mathf.Clamp(controlsActionsPanel.y,160,570-controlsActionsPanel.height);
        controlsDevicePanel.x=Mathf.Clamp(controlsDevicePanel.x,controlsActionsPanel.xMax+16,628);
        controlsDevicePanel.width=Mathf.Clamp(controlsDevicePanel.width,640,1268-controlsDevicePanel.x);
        controlsDevicePanel.height=Mathf.Clamp(controlsDevicePanel.height,350,410);
        controlsDevicePanel.y=Mathf.Clamp(controlsDevicePanel.y,160,570-controlsDevicePanel.height);
    }
}
}
