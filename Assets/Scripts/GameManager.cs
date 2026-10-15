using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reinciarButton;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (reinciarButton != null)
            reinciarButton.onClick.AddListener(ReiniciarEscena);

        if (menuButton != null)
            menuButton.onClick.AddListener(IrAlMenu);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOverActivo)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReinciarEscena();
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
            {
                IrAlMenu();
            }
        }
        
    }

    public void GameOver()
    {
        If (gameOverActivo) return;

        gameOverActivo = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n\nR - REINCIAR\nESC - MENÚ PRINCIPAL";
        }
        
    }

    public void ReinciarEscena()
    {
        Time.timeScale = 1f
        ScenaManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f
        ScenaManager.LoadScene("MENU");
    }

}
