using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    None,
    Pawn, Bishop, Rook, Lance, Knight, Silver, Gold, King
};

public class Piece : MonoBehaviour
{
    private static readonly float hoverHeight = 0.3f;

    private PieceType piece = PieceType.None;
    private bool promoted = false;
    private bool enemy = false;

    private List<int[]> possibleMoves = new List<int[]>();

    public PieceType getPiece()
    {
        return piece;
    }

    public void setPiece(PieceType piece)
    {
        this.piece = piece;
    }

    public bool getSide()
    {
        return enemy;
    }

    public void setSide(bool enemy)
    {
        this.enemy = enemy;
    }

    public List<int[]> selected(int row, int col)
    {
        transform.Translate(0, hoverHeight, 0);

        possibleMoves.Clear();
        switch (this.piece)
        {
            case PieceType.Pawn:
                possibleMoves.Add(new int[2] { row + 1, col });
                return possibleMoves;

            case PieceType.Bishop:
                for (int i = 1; i <= Board.boardSize; i++)
                {
                    possibleMoves.Add(new int[2] { row + i, col - i });
                    possibleMoves.Add(new int[2] { row + i, col + i });
                    possibleMoves.Add(new int[2] { row - i, col - i });
                    possibleMoves.Add(new int[2] { row - i, col + i });
                }
                return possibleMoves;

            case PieceType.Rook:
                for (int i = 1; i <= Board.boardSize; i++)
                {
                    possibleMoves.Add(new int[2] { row + i, col });
                    possibleMoves.Add(new int[2] { row - i, col });
                    possibleMoves.Add(new int[2] { row, col + i });
                    possibleMoves.Add(new int[2] { row, col - i });
                }
                return possibleMoves;

            case PieceType.Lance:
                for (int i = 1; i <= Board.boardSize; i++)
                {
                    possibleMoves.Add(new int[2] { row + i, col });
                }
                return possibleMoves;

            case PieceType.Knight:
                possibleMoves.Add(new int[2] { row + 2, col - 1 });
                possibleMoves.Add(new int[2] { row + 2, col + 1 });
                return possibleMoves;

            case PieceType.Silver:
                possibleMoves.Add(new int[2] { row + 1, col });
                possibleMoves.Add(new int[2] { row + 1, col - 1 });
                possibleMoves.Add(new int[2] { row + 1, col + 1 });
                possibleMoves.Add(new int[2] { row - 1, col - 1 });
                possibleMoves.Add(new int[2] { row - 1, col + 1 });
                return possibleMoves;

            case PieceType.Gold:
                possibleMoves.Add(new int[2] { row + 1, col });
                possibleMoves.Add(new int[2] { row + 1, col - 1 });
                possibleMoves.Add(new int[2] { row + 1, col + 1 });
                possibleMoves.Add(new int[2] { row, col - 1 });
                possibleMoves.Add(new int[2] { row, col + 1 });
                possibleMoves.Add(new int[2] { row - 1, col });
                return possibleMoves;

            case PieceType.King:
                possibleMoves.Add(new int[2] { row + 1, col });
                possibleMoves.Add(new int[2] { row - 1, col });
                possibleMoves.Add(new int[2] { row, col - 1 });
                possibleMoves.Add(new int[2] { row, col + 1 });
                possibleMoves.Add(new int[2] { row + 1, col - 1 });
                possibleMoves.Add(new int[2] { row + 1, col + 1 });
                possibleMoves.Add(new int[2] { row - 1, col - 1 });
                possibleMoves.Add(new int[2] { row - 1, col + 1 });
                return possibleMoves;

            default:
                return null;
        }
    }

    public void deselected()
    {
        transform.Translate(0, -hoverHeight, 0);
    }
}
