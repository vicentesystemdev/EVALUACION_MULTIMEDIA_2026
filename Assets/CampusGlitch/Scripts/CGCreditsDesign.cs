using System;
using UnityEngine;

namespace CampusGlitch {
[Serializable]public class CGCreditProfile {
    public string name,role,theme;
    [TextArea(2,5)]public string detail;
    public Texture2D portrait;public Color accent=new Color(1,.56f,.12f);public bool arachnidTheme;
    [Range(0,1)]public float portraitFocus=1;
}
[CreateAssetMenu(menuName="Campus Glitch/Diseño de créditos",fileName="CampusCredits")]
public sealed class CGCreditsDesign:ScriptableObject {
    public string title="CRÉDITOS",subtitle="LAS PERSONAS DETRÁS DEL CAMPUS",footer="Gracias por ser parte de Campus Glitch";
    public Texture2D background;
    public Rect firstCard=new Rect(31,184,282,391);
    [Range(8,30)]public float cardGap=24;
    [Range(.65f,1)]public float panelOpacity=.98f;
    [Range(0,8)]public float hoverLift=4;
    [Range(.1f,.5f)]public float modalTransition=.24f;
    public bool animatePortraits=true;
    public CGCreditProfile[] team={
        new CGCreditProfile{name="VÍCTOR GROVER\nASTURIZAGA PLATA",role="Escenarios, dibujos y caminos del campus",theme="PROGRAMACIÓN · ANIMACIÓN",detail="Dio forma a los escenarios, las plataformas y los caminos del campus. Mejoró los menús, el mapa y la presentación de los poderes del Guardián. Su estilo mezcla Spider-Man, programación y animación.",accent=new Color(1,.24f,.3f),arachnidTheme=true},
        new CGCreditProfile{name="CARLA VALERIA\nENCINAS CANO",role="Menús, movimiento y sonidos",theme="ANIMACIÓN · SONIDO",detail="Creó el primer menú de inicio: organizó el título, las opciones y el botón para jugar. Su trabajo en el equipo se centra en los menús, el movimiento de los personajes y los sonidos.",accent=new Color(.81f,.43f,1)},
        new CGCreditProfile{name="VICENTE OSCAR\nCLAROS MAMANI",role="Unión del juego y controles en pantalla",theme="PANTALLAS · JUEGO EN TELÉFONO",detail="Unió las partes del juego y preparó el cambio entre pantallas. Añadió controles en pantalla para jugar en el teléfono y organizó la revisión del trabajo del equipo.",accent=new Color(.3f,.85f,1)},
        new CGCreditProfile{name="MARCELO JOSUE\nESCOBAR CHIPANA",role="Enemigos y regreso a la partida",theme="DESAFÍOS · NUEVOS INTENTOS",detail="Creó la primera pantalla de derrota y sus opciones de regreso. Su trabajo se centra en los enemigos, el combate y volver a jugar desde el último punto alcanzado.",accent=new Color(1,.4f,.62f)}
    };
    public CGCreditProfile[] acknowledgements={
        new CGCreditProfile{name="Ing. Noemí Moller\nChuquimia",role="Directora de la carrera\nIngeniería de Sistemas",theme="DIRECCIÓN ACADÉMICA",detail="Gracias por su apoyo, confianza y guía en el desarrollo de Campus Glitch.",accent=new Color(.8f,.4f,1)},
        new CGCreditProfile{name="Ing. Rubén Soria",role="Docente de\nProgramación Multimedia",theme="APRENDE HACIENDO",detail="Gracias por su orientación, dedicación y aportes a la elaboración de este proyecto.",accent=new Color(.3f,.85f,1)}
    };
    [TextArea(4,9)]public string resources="TIPOGRAFÍA\nAudiowide · Barlow Condensed · Questrial\nFuentes y licencias incluidas en el proyecto.\n\nARTE Y DESARROLLO\nCampus, Guardián, actores e interfaces de Campus Glitch.\n\nPROYECTO ACADÉMICO\nUNIFRANZ · Sede La Paz · Programación Multimedia";
    void OnValidate(){firstCard.width=Mathf.Clamp(firstCard.width,240,282);firstCard.height=Mathf.Clamp(firstCard.height,350,400);cardGap=Mathf.Clamp(cardGap,8,24);firstCard.x=Mathf.Clamp(firstCard.x,16,1264-4*firstCard.width-3*cardGap);firstCard.y=Mathf.Clamp(firstCard.y,168,580-firstCard.height);}
}
}
