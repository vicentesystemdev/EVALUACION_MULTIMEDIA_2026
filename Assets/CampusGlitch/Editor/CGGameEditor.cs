#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.Editor {
[CustomEditor(typeof(CGGame))]
public class CGGameEditor:UnityEditor.Editor {
    UnityEditor.Editor designEditor,creditsEditor,settingsEditor,mapEditor,helpEditor;bool expanded=true,creditsExpanded=true,settingsExpanded=true,mapExpanded=true,helpExpanded=true;
    void OnDisable(){if(designEditor!=null)DestroyImmediate(designEditor);if(creditsEditor!=null)DestroyImmediate(creditsEditor);if(settingsEditor!=null)DestroyImmediate(settingsEditor);if(mapEditor!=null)DestroyImmediate(mapEditor);if(helpEditor!=null)DestroyImmediate(helpEditor);}
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
        if(game.initialPage=="Customization"){
            EditorGUILayout.Space();EditorGUILayout.LabelField("Personalización en Scene",EditorStyles.boldLabel);
            var labels=new[]{"Apariencia","Habilidades","Evolución"};
            for(int i=0;i<labels.Length;i++)if(GUILayout.Button("Mostrar "+labels[i])){game.SelectCustomizationSection(i);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar activación con teclado")){game.SelectCustomizationControls(false);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar activación con mando")){game.SelectCustomizationControls(true);CGEditorMenuPreview.Refresh();}
            EditorGUILayout.LabelField("Demostrar poder sin guardar progreso",EditorStyles.boldLabel);
            for(int id=0;id<CGAbilities.Names.Length;id++)if(GUILayout.Button(CGAbilities.Names[id])){game.PreviewCustomizationAbility(id);CGEditorMenuPreview.Refresh();}
        }
        if(game.initialPage=="Controls"){
            EditorGUILayout.Space();EditorGUILayout.LabelField("Controles en Scene",EditorStyles.boldLabel);
            if(GUILayout.Button("Mostrar controles de teclado")){game.PreviewControlsMode(false);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar controles de mando")){game.PreviewControlsMode(true);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar mando Xbox")){game.PreviewControlsMode(true);game.PreviewControllerFamily(0);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar mando PlayStation")){game.PreviewControlsMode(true);game.PreviewControllerFamily(1);CGEditorMenuPreview.Refresh();}
        }
        if(game.initialPage=="Help"){
            var help=Resources.Load<CGHelpDesign>("CG/Presentation/CampusHelp");EditorGUILayout.Space();helpExpanded=EditorGUILayout.Foldout(helpExpanded,"Contenido de Ayuda",true);
            if(help!=null&&helpExpanded){CreateCachedEditor(help,null,ref helpEditor);EditorGUI.BeginChangeCheck();helpEditor.OnInspectorGUI();if(EditorGUI.EndChangeCheck()){EditorUtility.SetDirty(help);CGEditorMenuPreview.Refresh();}}
            var chapters=new[]{"Cómo jugar","Objetivos","Iconos","Personajes","Guardado"};for(int i=0;i<chapters.Length;i++)if(GUILayout.Button("Mostrar "+chapters[i])){game.PreviewHelp(i);CGEditorMenuPreview.Refresh();}
        }
        if(game.initialPage=="Map"){
            var map=Resources.Load<CGMapDesign>("CG/Presentation/CampusMap");EditorGUILayout.Space();mapExpanded=EditorGUILayout.Foldout(mapExpanded,"Diseño del mapa y actividades",true);
            if(map!=null&&mapExpanded){CreateCachedEditor(map,null,ref mapEditor);EditorGUI.BeginChangeCheck();mapEditor.OnInspectorGUI();if(EditorGUI.EndChangeCheck()){EditorUtility.SetDirty(map);CGEditorMenuPreview.Refresh();}}
            if(GUILayout.Button("Mostrar recorrido del campus")){game.PreviewCampusMap(0,false);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar nivel y guías")){game.PreviewCampusMap(0,true);CGEditorMenuPreview.Refresh();}
        }
        if(game.initialPage=="Settings"){
            var settings=Resources.Load<CGSettingsDesign>("CG/Presentation/CampusSettings");EditorGUILayout.Space();settingsExpanded=EditorGUILayout.Foldout(settingsExpanded,"Diseño de Configuración",true);
            if(settings!=null&&settingsExpanded){CreateCachedEditor(settings,null,ref settingsEditor);EditorGUI.BeginChangeCheck();settingsEditor.OnInspectorGUI();if(EditorGUI.EndChangeCheck()){EditorUtility.SetDirty(settings);CGEditorMenuPreview.Refresh();}}
            var sections=new[]{"Pantalla","Audio","Accesibilidad","Partida"};for(int i=0;i<sections.Length;i++)if(GUILayout.Button("Mostrar "+sections[i])){game.PreviewSettingsSection(i);CGEditorMenuPreview.Refresh();}
        }
        if(game.initialPage=="Credits"){
            var credits=Resources.Load<CGCreditsDesign>("CG/Presentation/CampusCredits");EditorGUILayout.Space();creditsExpanded=EditorGUILayout.Foldout(creditsExpanded,"Diseño y contenido de Créditos",true);
            if(credits!=null&&creditsExpanded){CreateCachedEditor(credits,null,ref creditsEditor);EditorGUI.BeginChangeCheck();creditsEditor.OnInspectorGUI();if(EditorGUI.EndChangeCheck()){EditorUtility.SetDirty(credits);CGEditorMenuPreview.Refresh();}}
            if(GUILayout.Button("Mostrar equipo")){game.PreviewCredits(0);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar agradecimientos")){game.PreviewCredits(2);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar detalle de Víctor")){game.PreviewCredits(1,0);CGEditorMenuPreview.Refresh();}
            if(GUILayout.Button("Mostrar arte y recursos")){game.PreviewCredits(3);CGEditorMenuPreview.Refresh();}
        }
    }
}
}
#endif
