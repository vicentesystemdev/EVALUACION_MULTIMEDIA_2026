using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <-- 1. Añadido

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reiniciarButton;
    public Button menuButton;

    private bool gameOverActivo = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (reiniciarButton != null)
            reiniciarButton.onClick.AddListener(ReiniciarEscena);

        if (menuButton != null)
            menuButton.onClick.AddListener(IrAlMenu);
    }

    void Update()
    {
        if (gameOverActivo)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReiniciarEscena();
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
            {
                IrAlMenu();
            }
        }
    }

    public void GameOver()
    {
        if (gameOverActivo) return; // <-- 2. 'if' en minúscula

        gameOverActivo = true;

        // Opcional: pausar el juego al perder
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n\nR - REINICIAR\nESC - MENÚ PRINCIPAL";
        }
    }

    public void ReiniciarEscena() // <-- 5. Nombre corregido a 'ReiniciarEscena'
    {
        Time.timeScale = 1f; // <-- 3. Punto y coma añadido
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // <-- 4. SceneManager corregido
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f; // <-- 3. Punto y coma añadido
        SceneManager.LoadScene("MENU"); // <-- 4. SceneManager corregido
    }
}
