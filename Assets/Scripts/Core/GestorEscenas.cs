using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorEscenas : MonoBehaviour
{
    public void IniciarJuego()
    {
        // Restablece la velocidad normal antes de cargar el nivel.
        Time.timeScale = 1f;
        SceneManager.LoadScene("Nivel01");
    }

    public void MostrarGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    public void ReiniciarNivel()
    {
        // Reutiliza la carga del nivel de prueba.
        IniciarJuego();
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SalirDelJuego()
    {
        // UnityEditor solo se incluye al ejecutar desde el editor.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
