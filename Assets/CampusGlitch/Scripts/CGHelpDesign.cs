using System;
using UnityEngine;
namespace CampusGlitch {
[Serializable]public sealed class CGHelpEntry {
    public string title;[TextArea(2,5)]public string body;
    public CGHelpEntry(string title,string body){this.title=title;this.body=body;}
}
[CreateAssetMenu(menuName="Campus Glitch/Diseño de ayuda",fileName="CampusHelp")]
public sealed class CGHelpDesign:ScriptableObject {
    public string title="GUÍA DEL CAMPUS",subtitle="EXPLORA · RESUELVE · AVANZA";
    public string[] sections={"Cómo jugar","Objetivos","Iconos del juego","Personajes","Guardado","Versión móvil"};
    public Texture2D background;
    public Color panel=new Color(.025f,.04f,.085f,1);
    [Range(.12f,.4f)]public float transition=.2f;
    [Range(3,8)]public float tutorialSeconds=4;
    public CGHelpEntry[] steps={
        new CGHelpEntry("MUÉVETE","Usa {move} para moverte. Mantén {run} para correr."),
        new CGHelpEntry("SALTA","Pulsa {jump} para saltar. Las franjas luminosas señalan las plataformas sólidas."),
        new CGHelpEntry("ATACA","Ataca con {attack}. Usa {ability} para activar tu poder. Revisa su energía antes de usarlo."),
        new CGHelpEntry("INTERACTÚA","Acércate y pulsa {interact} para hablar, activar un punto de regreso o abrir una terminal."),
        new CGHelpEntry("REÚNE Y RESUELVE","Reúne tres fragmentos. En la terminal, elige la respuesta con las direcciones y confirma con {confirm}."),
        new CGHelpEntry("CONTINÚA","Vence al jefe si lo hay. Acércate al portal y pulsa {interact} para completar el nivel.")
    };
    public CGHelpEntry[] questions={
        new CGHelpEntry("¿CUÁNDO SE GUARDA EL AVANCE?","El avance se guarda al completar un nivel. En Configuración y Controles, pulsa Guardar cambios o Aplicar."),
        new CGHelpEntry("¿PARA QUÉ SIRVE EL PUNTO DE REGRESO?","Actívalo con {interact} para regresar cerca si caes. Funciona durante la partida actual; no guarda el punto al cerrar el juego."),
        new CGHelpEntry("¿DÓNDE CAMBIO MIS BOTONES?","Abre Controles. Elige tus teclas y botones, pruébalos y pulsa Aplicar."),
        new CGHelpEntry("¿CÓMO ABRO NUEVOS CAMINOS?","Completa el nivel anterior. N02 abre la variante morada; N05, el subsuelo. Los niveles principales entregan o mejoran poderes.")
    };
}
}
