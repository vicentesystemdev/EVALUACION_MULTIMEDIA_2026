#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.Editor {
[CustomEditor(typeof(CGGame))]
public class CGGameEditor:UnityEditor.Editor {
    UnityEditor.Editor designEditor;bool expanded=true;
    void OnDisable(){if(designEditor!=null)DestroyImmediate(designEditor);}
    public override void OnInspectorGUI(){
        DrawDefaultInspector();var game=(CGGame)target;
        if(game.sceneLevel>=0){if(GUILayout.Button("Enfocar nivel actual en Scene"))CGEditorWorldPreview.FrameEntrance();return;}
        EditorGUILayout.Space();expanded=EditorGUILayout.Foldout(expanded,"Diseño compartido del menú",true);
        var design=CGMenuTheme.Design;if(design==null)return;
        if(expanded){
            EditorGUILayout.HelpBox("Fondo, fuentes, textos, distribución y animación se editan aquí. Scene y Play usan este mismo diseño. Los elementos de vista previa se reconstruyen automáticamente.",MessageType.Info);
            CreateCachedEditor(design,null,ref designEditor);EditorGUI.BeginChangeCheck();designEditor.OnInspectorGUI();
            if(EditorGUI.EndChangeCheck()){EditorUtility.SetDirty(design);SceneView.RepaintAll();}
        }
        if(GUILayout.Button("Enfocar menú actual en Scene"))CGEditorMenuPreview.Frame();
        if(GUILayout.Button("Alternar vista de entrada animada"))CGEditorMenuPreview.ToggleEntrance();
        if(GUILayout.Button("Seleccionar archivo de diseño")){Selection.activeObject=design;EditorGUIUtility.PingObject(design);}
    }
}
}
#endif
