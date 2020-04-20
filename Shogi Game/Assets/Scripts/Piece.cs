using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    None,
    Pawn, Bishop, Rook, Lance, Knight, Silver, Gold, King,
    ProPawn, ProBishop, ProRook, ProLance, ProKnight, ProSilver, ProGold, ProKing,
    EnemyPawn, EnemyBishop, EnemyRook, EnemyLance, EnemyKnight, EnemySilver, EnemyGold, EnemyKing,
    EnemyProPawn, EnemyProBishop, EnemyProRook, EnemyProLance, EnemyProKnight, EnemyProSilver, EnemyProGold, EnemyProKing
};

public class Piece : MonoBehaviour
{
    private static readonly float hoverHeight = 0.3f;
    private PieceType piece = PieceType.None;

    public PieceType getPiece()
    {
        return piece;
    }

    public void setPiece(PieceType piece)
    {
        this.piece = piece;
    }

    public int[][] selected(int row, int col)
    {
        transform.Translate(0, hoverHeight, 0);

        switch (this.piece)
        {
            case PieceType.Pawn:
                return new int[][]
                {
                    new int[2] {row + 1, col}
                };
            // add other piece cases here.
            default:
                return null;
        }
    }

    public void deselected()
    {
        transform.Translate(0, -hoverHeight, 0);
    }
}
