#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.Editor {
[CustomEditor(typeof(CGGuardianVisual))]
public class CGGuardianVisualEditor:UnityEditor.Editor {
    public override bool RequiresConstantRepaint(){return Application.isPlaying;}
    public override void OnInspectorGUI(){
        var visual=(CGGuardianVisual)target;
        var hero=visual.GetComponent<CGHero>();
        var view=visual.GetComponentInChildren<SpriteRenderer>();
        using(new EditorGUI.DisabledScope(true)){
            EditorGUILayout.TextField("Animación actual",Application.isPlaying?visual.CurrentAnimation:"Idle · vista previa");
            if(hero!=null){
                EditorGUILayout.Toggle("En suelo",hero.Grounded);
                EditorGUILayout.FloatField("Entrada horizontal",hero.moveInput);
                EditorGUILayout.Toggle("Corriendo",hero.running);
                if(hero.Body!=null)EditorGUILayout.Vector2Field("Velocidad física",hero.Body.linearVelocity);
            }
            if(view!=null&&view.sprite!=null){
                EditorGUILayout.ObjectField("Fotograma",view.sprite,typeof(Sprite),false);
                EditorGUILayout.EnumPopup("Filtro de textura",view.sprite.texture.filterMode);
            }
        }
        EditorGUILayout.HelpBox("El controlador selecciona Idle, caminar, correr, salto, caída/aterrizaje, ataque, daño y victoria. La presentación no modifica la cápsula ni el movimiento.",MessageType.Info);
    }
}
}
#endif
