using System;
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.EditorQA {
public static class CGMenuRegression {
    public static void RunAll(){CheckQuizNavigation();CheckResetChoices();CheckVictoryAndEnding();CheckVolume();CheckInitialCancel();}
    [MenuItem("Campus Glitch/QA/Comprobar limites de volumen")]
    public static void CheckVolume(){
        if(CGGame.VolumeAfterStep(1f,.1f)!=1f || CGGame.VolumeAfterStep(0f,-.1f)!=0f ||
           Mathf.Abs(CGGame.VolumeAfterStep(.5f,-.1f)-.4f)>.001f)
            throw new Exception("Volumen fuera de rango o decremento incorrecto.");
        Debug.Log("[Campus Glitch QA] OK: volumen sube, baja y conserva limites.");
    }
    [MenuItem("Campus Glitch/QA/Comprobar seleccion segura inicial")]
    public static void CheckInitialCancel(){
        var go=new GameObject("QA temporary menu");
        try{
            var game=go.AddComponent<CGGame>();
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            typeof(CGGame).GetMethod("MainSelect",flags).Invoke(game,new object[]{7});
            int selected=(int)typeof(CGGame).GetField("selection",flags).GetValue(game);
            if(selected!=1)throw new Exception("El borrado no abre con Cancelar seleccionado.");
            Debug.Log("[Campus Glitch QA] OK: abrir borrado selecciona Cancelar.");
        }finally{UnityEngine.Object.DestroyImmediate(go);}
    }
    // Runs without touching the player's real JSON save.
    [MenuItem("Campus Glitch/QA/Comprobar victoria y creditos")]
    public static void CheckVictoryAndEnding() {
        int next = 0, menu = 0, credits = 0;
        CGGame.HandleVictorySelection(0, () => next++, () => menu++);
        CGGame.HandleVictorySelection(1, () => next++, () => menu++);
        CGGame.HandleEndingSelection(0, () => credits++, () => menu++);
        CGGame.HandleEndingSelection(1, () => credits++, () => menu++);
        if (next != 1 || credits != 1 || menu != 2)
            throw new Exception("Victoria o final ejecutó la opción equivocada.");
        Debug.Log("[Campus Glitch QA] OK: siguiente nivel, menu y creditos respetan seleccion.");
    }

    [MenuItem("Campus Glitch/QA/Comprobar navegacion de terminal")]
    public static void CheckQuizNavigation() {
        if (CGGame.MoveQuizSelection(0, 1) != 1 ||
            CGGame.MoveQuizSelection(1, 1) != 2 ||
            CGGame.MoveQuizSelection(2, 1) != 3 ||
            CGGame.MoveQuizSelection(3, 1) != 0 ||
            CGGame.MoveQuizSelection(0, -1) != 3 ||
            CGGame.MoveQuizSelection(2, -1) != 1)
            throw new Exception("La seleccion de respuestas y cierre no circula correctamente.");
        Debug.Log("[Campus Glitch QA] OK: respuestas y cierre accesibles con seleccion 0, 1, 2 y 3.");
    }

    [MenuItem("Campus Glitch/QA/Comprobar borrado de progreso")]
    public static void CheckResetChoices() {
        CGProgress progress = new CGProgress();
        progress.latestUnlocked = 2;
        progress.completed[0] = true;
        progress.completed[1] = true;
        string original = JsonUtility.ToJson(progress);
        int resets = 0;
        int returns = 0;

        CGGame.HandleResetSelection(1, () => {
            resets++;
            progress = new CGProgress();
        }, () => returns++);

        if (resets != 0 || returns != 1 || JsonUtility.ToJson(progress) != original)
            throw new Exception("Cancelar modificó el progreso o no regresó al menú.");

        CGGame.HandleResetSelection(0, () => {
            resets++;
            progress = new CGProgress();
        }, () => returns++);

        if (resets != 1 || returns != 2 || progress.latestUnlocked != 0)
            throw new Exception("Confirmar no reinició el progreso correctamente.");

        Debug.Log("[Campus Glitch QA] OK: cancelar conserva progreso; confirmar ejecuta el borrado.");
    }
}
}
