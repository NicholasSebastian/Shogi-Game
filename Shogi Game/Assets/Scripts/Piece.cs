using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Pieces
{
    None, Pawn, Bishop, Rook, Lance, Knight, Silver, Gold, King,
    EnemyPawn, EnemyBishop, EnemyRook, EnemyLance, EnemyKnight, EnemySilver, EnemyGold, EnemyKing
};

public class Piece : MonoBehaviour
{
    private float hoverHeight = 0.3f;

    public void selected()
    {
        transform.Translate(0, hoverHeight, 0);
    }

    public void deselected()
    {
        transform.Translate(0, -hoverHeight, 0);
    }
}
