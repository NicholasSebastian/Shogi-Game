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
    private static readonly float movementSpeed = 10.0f;

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

    public List<int[]> selected(int row, int col, Tile[,] board)
    {
        transform.Translate(0, hoverHeight, 0);
        return pieceMoves(row, col, board);
    }

    public void deselected()
    {
        transform.Translate(0, -hoverHeight, 0);
    }

    public IEnumerator moveAnimation(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        for (float i = 0.0f, c = 0.0f; i <= 1.0f;
            c += movementSpeed * Time.deltaTime,
            i = c / distance)
        {
            transform.position = Vector3.Lerp(
                startPosition, targetPosition, i
            );
            yield return null;
        }
    }

    private List<int[]> pieceMoves(int row, int col, Tile[,] board)
    {
        possibleMoves.Clear();
        switch (this.piece)
        {
            case PieceType.Pawn:
                possibleMoves.Add(new int[2] { row + 1, col });
                break;

            case PieceType.Lance:
                for (int i = 1; i <= Board.boardSize; i++)
                {
                    possibleMoves.Add(new int[2] { row + i, col });
                }
                break;

            case PieceType.Knight:
                possibleMoves.Add(new int[2] { row + 2, col - 1 });
                possibleMoves.Add(new int[2] { row + 2, col + 1 });
                break;

            case PieceType.Silver:
                possibleMoves.Add(new int[2] { row + 1, col });
                possibleMoves.Add(new int[2] { row + 1, col - 1 });
                possibleMoves.Add(new int[2] { row + 1, col + 1 });
                possibleMoves.Add(new int[2] { row - 1, col - 1 });
                possibleMoves.Add(new int[2] { row - 1, col + 1 });
                break;

            case PieceType.Gold:
                possibleMoves.Add(new int[2] { row + 1, col });
                possibleMoves.Add(new int[2] { row + 1, col - 1 });
                possibleMoves.Add(new int[2] { row + 1, col + 1 });
                possibleMoves.Add(new int[2] { row, col - 1 });
                possibleMoves.Add(new int[2] { row, col + 1 });
                possibleMoves.Add(new int[2] { row - 1, col });
                break;

            case PieceType.King:
                possibleMoves.Add(new int[2] { row + 1, col });
                possibleMoves.Add(new int[2] { row - 1, col });
                possibleMoves.Add(new int[2] { row, col - 1 });
                possibleMoves.Add(new int[2] { row, col + 1 });
                possibleMoves.Add(new int[2] { row + 1, col - 1 });
                possibleMoves.Add(new int[2] { row + 1, col + 1 });
                possibleMoves.Add(new int[2] { row - 1, col - 1 });
                possibleMoves.Add(new int[2] { row - 1, col + 1 });
                break;

            case PieceType.Bishop:
                bishopMoves(row, col, board);
                break;

            case PieceType.Rook:
                rookMoves(row, col, board);
                break;

            default:
                break;
        }
        return possibleMoves;
    }

    private void bishopMoves(int row, int col, Tile[,] board)
    {
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row + i > 8 || col + i > 8) break;
            if (board[row + i, col + i].getState() == PieceType.None ||
                board[row + i, col + i].getSide())
            {
                possibleMoves.Add(new int[2] { row + i, col + i });
                if (board[row + i, col + i].getSide()) break;
            }
            else break;
        }
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row + i > 8 || col - i < 0) break;
            if (board[row + i, col - i].getState() == PieceType.None ||
                board[row + i, col - i].getSide())
            {
                possibleMoves.Add(new int[2] { row + i, col - i });
                if (board[row + i, col - i].getSide()) break;
            }
            else break;
        }
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row - i < 0 || col + i > 8) break;
            if (board[row - i, col + i].getState() == PieceType.None ||
                board[row - i, col + i].getSide())
            {
                possibleMoves.Add(new int[2] { row - i, col + i });
                if (board[row - i, col + i].getSide()) break;
            }
            else break;
        }
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row - i < 0 || col - i < 0) break;
            if (board[row - i, col - i].getState() == PieceType.None ||
                board[row - i, col - i].getSide())
            {
                possibleMoves.Add(new int[2] { row - i, col - i });
                if (board[row - i, col - i].getSide()) break;
            }
            else break;
        }
    }

    private void rookMoves(int row, int col, Tile[,] board)
    {
        for (int i = row + 1; i < Board.boardSize; i++)
        {
            if (board[i, col].getState() == PieceType.None ||
                board[i, col].getSide())
            {
                possibleMoves.Add(new int[2] { i, col });
                if (board[i, col].getSide()) break;
            }
            else break;
        }
        for (int i = row - 1; i >= 0; i--)
        {
            if (board[i, col].getState() == PieceType.None ||
                board[i, col].getSide())
            {
                possibleMoves.Add(new int[2] { i, col });
                if (board[i, col].getSide()) break;
            }
            else break;
        }
        for (int i = col + 1; i < Board.boardSize; i++)
        {
            if (board[row, i].getState() == PieceType.None ||
                board[row, i].getSide())
            {
                possibleMoves.Add(new int[2] { row, i });
                if (board[row, i].getSide()) break;
            }
            else break;
        }
        for (int i = col - 1; i >= 0; i--)
        {
            if (board[row, i].getState() == PieceType.None ||
                board[row, i].getSide())
            {
                possibleMoves.Add(new int[2] { row, i });
                if (board[row, i].getSide()) break;
            }
            else break;
        }
    }
}
