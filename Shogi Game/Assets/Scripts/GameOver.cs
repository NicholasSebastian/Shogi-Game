using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public GameObject mainMenu;

    private Button backButton;
    private bool playersWin;
    private string endComment;

    void Start()
    {
        playersWin = GameController.playersWin;
        endComment = GameController.endComment;
        transform.GetChild(0).GetComponent<Text>().text = (
            playersWin ?
            "YOU WIN!!!" :
            "YOU LOSE!!!"
        );
        transform.GetChild(1).GetComponent<Text>().text = endComment;

        backButton = transform.GetChild(2).GetComponent<Button>();
        backButton.onClick.AddListener(BackToMain);
    }

    void BackToMain()
    {
        MainMenu.mainMenu = true;
        mainMenu.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
