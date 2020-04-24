using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    private static Text text;

    void Start()
    {
        text = GetComponent<Text>();
        text.text = "";
    }

    public static void Log(bool isEnemy, PieceType killer, PieceType killed)
    {
        text.text = (
            (isEnemy ? "Player's " : "Enemy's ") + killer
            + " killed " +
            (isEnemy ? "Enemy's " : "Player's ") + killed
        );
    }

    public static void Clear()
    {
        text.text = "";
    }
}
