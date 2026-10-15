using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorEscenas : MonoBehaviour
{
    public void IniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Nivel01");
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Nivel01");
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}