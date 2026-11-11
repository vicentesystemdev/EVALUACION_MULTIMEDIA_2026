using UnityEngine;

namespace CampusGlitch {
public class CGCampusNPC:MonoBehaviour {
    public string role,dialogue;
    CGWorld world;float talkingUntil;
    public bool Talking {get{return Time.time<talkingUntil;}}
    public void Configure(CGWorld owner,string kind){
        world=owner;
        role=kind=="Guardia_U"?"GUARDIA U":kind=="Estudiante"?"ESTUDIANTE":kind=="Tecnica_Lab"?"TÉCNICA DE LAB":kind=="Recepcionista"?"RECEPCIÓN":kind=="Director"?"DIRECCIÓN":kind=="Docente"?"DOCENTE":kind=="Unifranzito"?"GUÍA DE BIBLIOTECA":"COORDINACIÓN DE SISTEMAS";
        if(owner.index==8&&kind=="Noemi_Moller")role="GUÍA DEL NÚCLEO";
        dialogue=owner.index!=0||kind=="Guardia_U"?owner.spec.dialogue:kind=="Estudiante"?
            "Busca los tres fragmentos de datos. Las franjas luminosas marcan las plataformas sólidas.":
            "La terminal está a la derecha. Acércate y pulsa Interactuar para recuperar la conexión.";
        var label=new GameObject("Nombre_NPC");label.transform.SetParent(transform,false);
        label.transform.localPosition=new Vector3(0,1.03f,0);
        var text=label.AddComponent<TextMesh>();text.text=role;text.anchor=TextAnchor.LowerCenter;text.alignment=TextAlignment.Center;
        text.characterSize=.065f;text.fontSize=48;text.color=new Color(1,.8f,.4f);
        label.GetComponent<MeshRenderer>().sortingOrder=38;
    }
    public void Speak(){talkingUntil=Time.time+3;world.game.Hint(role+": "+dialogue,true);}
}
}
