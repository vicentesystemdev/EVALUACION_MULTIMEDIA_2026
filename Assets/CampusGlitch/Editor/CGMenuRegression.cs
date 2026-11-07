using System;
using UnityEditor;
using UnityEngine;

namespace CampusGlitch.EditorQA {
public static class CGMenuRegression {
    // Runs without touching the player's real JSON save.
    [MenuItem("Campus Glitch/QA/Comprobar navegacion de terminal")]
    public static void CheckQuizNavigation() {
        if (CGGame.MoveQuizSelection(0, 1) != 1 ||
            CGGame.MoveQuizSelection(1, 1) != 2 ||
            CGGame.MoveQuizSelection(2, 1) != 0 ||
            CGGame.MoveQuizSelection(0, -1) != 2 ||
            CGGame.MoveQuizSelection(2, -1) != 1)
            throw new Exception("La seleccion de las tres respuestas no circula correctamente.");
        Debug.Log("[Campus Glitch QA] OK: desplazamiento entre respuestas 0, 1 y 2.");
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
