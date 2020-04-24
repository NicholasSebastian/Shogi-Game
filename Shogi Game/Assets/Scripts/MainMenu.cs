using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool mainMenu = true;

    public GameObject gameOver;
    private Button playButton;
    private Button quitButton;

    void Awake()
    {
        if (mainMenu)
        {
            this.gameObject.SetActive(true);
            gameOver.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
            gameOver.SetActive(true);
        }
    }

    void Start()
    {
        playButton = transform.GetChild(1).GetComponent<Button>();
        quitButton = transform.GetChild(2).GetComponent<Button>();
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void PlayGame()
    {
        mainMenu = false;
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
