using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    None, Pawn, Bishop, Rook, Lance, Knight, Silver, Gold, King
};

public class Piece : MonoBehaviour
{
    private static readonly float hoverHeight = 0.3f;
    private static readonly float movementSpeed = 10.0f;

    private PieceType piece = PieceType.None;
    private bool promoted = false;
    private bool enemy = false;

    public PieceType getPiece()
    {
        return piece;
    }

    public void setPiece(PieceType piece)
    {
        this.piece = piece;
    }

    public bool isEnemy()
    {
        return enemy;
    }

    public void setSide(bool enemy)
    {
        this.enemy = enemy;
    }

    public void raised()
    {
        transform.Translate(0, hoverHeight, 0);
    }

    public void deselected()
    {
        transform.Translate(0, -hoverHeight, 0);
    }

    public void promotion()
    {
        this.promoted = true;
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

    public List<int[]> pieceMoves(int row, int col, Tile[,] board)
    {
        List<int[]> possibleMoves = new List<int[]>();
        if (promoted == false)
            switch (this.piece)
            {
                case PieceType.Pawn:
                    possibleMoves.Add(new int[2] { row + 1, col });
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

                case PieceType.Lance:
                    lanceMoves(possibleMoves, row, col, board);
                    break;

                case PieceType.Bishop:
                    bishopMoves(possibleMoves, row, col, board);
                    break;

                case PieceType.Rook:
                    rookMoves(possibleMoves, row, col, board);
                    break;

                default:
                    break;
            }
        else
            switch (this.piece)
            {
                case PieceType.Bishop:
                    bishopMoves(possibleMoves, row, col, board);
                    possibleMoves.Add(new int[2] { row + 1, col });
                    possibleMoves.Add(new int[2] { row, col - 1 });
                    possibleMoves.Add(new int[2] { row, col + 1 });
                    possibleMoves.Add(new int[2] { row - 1, col });
                    break;

                case PieceType.Rook:
                    rookMoves(possibleMoves, row, col, board);
                    possibleMoves.Add(new int[2] { row + 1, col - 1 });
                    possibleMoves.Add(new int[2] { row + 1, col + 1 });
                    possibleMoves.Add(new int[2] { row - 1, col - 1 });
                    possibleMoves.Add(new int[2] { row - 1, col + 1 });
                    break;

                default:
                    possibleMoves.Add(new int[2] { row + 1, col });
                    possibleMoves.Add(new int[2] { row + 1, col - 1 });
                    possibleMoves.Add(new int[2] { row + 1, col + 1 });
                    possibleMoves.Add(new int[2] { row, col - 1 });
                    possibleMoves.Add(new int[2] { row, col + 1 });
                    possibleMoves.Add(new int[2] { row - 1, col });
                    break;
            }
        return
            clearSpecificCollision(
                sideInverse(possibleMoves, row),
                board
            );
    }

    private List<int[]> clearSpecificCollision(List<int[]> possibleMoves, Tile[,] board)
    {
        if (enemy == false)
            for (int i = possibleMoves.Count - 1; i >= 0; i--)
            {
                int[] possibleMove = possibleMoves[i];
                int x = possibleMove[0], y = possibleMove[1];
                if (x >= 0 && x < Board.boardSize && y >= 0 && y < Board.boardSize)
                {
                    Tile checkedTile = board[x, y];
                    if (checkedTile.getState() != PieceType.None &&
                        checkedTile.isEnemy() == false)
                        possibleMoves.Remove(possibleMove);
                }
            }
        return possibleMoves;
    }

    private List<int[]> sideInverse(List<int[]> possibleMoves, int row)
    {
        if (enemy)
            foreach (int[] possibleMove in possibleMoves)
                if (possibleMove[0] != row)
                    possibleMove[0] = (
                        possibleMove[0] < row ?
                        row + (row - possibleMove[0]) :
                        row - (possibleMove[0] - row)
                    );
        return possibleMoves;
    }

    private void lanceMoves(List<int[]> possibleMoves, int row, int col, Tile[,] board)
    {
        for (int i = row + 1; i < Board.boardSize; i++)
        {
            if (board[i, col].getState() == PieceType.None ||
                board[i, col].isEnemy())
            {
                possibleMoves.Add(new int[2] { i, col });
                if (board[i, col].isEnemy()) break;
            }
            else break;
        }
    }

    private void bishopMoves(List<int[]> possibleMoves, int row, int col, Tile[,] board)
    {
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row + i > 8 || col + i > 8) break;
            if (board[row + i, col + i].getState() == PieceType.None ||
                board[row + i, col + i].isEnemy())
            {
                possibleMoves.Add(new int[2] { row + i, col + i });
                if (board[row + i, col + i].isEnemy()) break;
            }
            else break;
        }
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row + i > 8 || col - i < 0) break;
            if (board[row + i, col - i].getState() == PieceType.None ||
                board[row + i, col - i].isEnemy())
            {
                possibleMoves.Add(new int[2] { row + i, col - i });
                if (board[row + i, col - i].isEnemy()) break;
            }
            else break;
        }
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row - i < 0 || col + i > 8) break;
            if (board[row - i, col + i].getState() == PieceType.None ||
                board[row - i, col + i].isEnemy())
            {
                possibleMoves.Add(new int[2] { row - i, col + i });
                if (board[row - i, col + i].isEnemy()) break;
            }
            else break;
        }
        for (int i = 1; i < Board.boardSize; i++)
        {
            if (row - i < 0 || col - i < 0) break;
            if (board[row - i, col - i].getState() == PieceType.None ||
                board[row - i, col - i].isEnemy())
            {
                possibleMoves.Add(new int[2] { row - i, col - i });
                if (board[row - i, col - i].isEnemy()) break;
            }
            else break;
        }
    }

    private void rookMoves(List<int[]> possibleMoves, int row, int col, Tile[,] board)
    {
        for (int i = row + 1; i < Board.boardSize; i++)
        {
            if (board[i, col].getState() == PieceType.None ||
                board[i, col].isEnemy())
            {
                possibleMoves.Add(new int[2] { i, col });
                if (board[i, col].isEnemy()) break;
            }
            else break;
        }
        for (int i = row - 1; i >= 0; i--)
        {
            if (board[i, col].getState() == PieceType.None ||
                board[i, col].isEnemy())
            {
                possibleMoves.Add(new int[2] { i, col });
                if (board[i, col].isEnemy()) break;
            }
            else break;
        }
        for (int i = col + 1; i < Board.boardSize; i++)
        {
            if (board[row, i].getState() == PieceType.None ||
                board[row, i].isEnemy())
            {
                possibleMoves.Add(new int[2] { row, i });
                if (board[row, i].isEnemy()) break;
            }
            else break;
        }
        for (int i = col - 1; i >= 0; i--)
        {
            if (board[row, i].getState() == PieceType.None ||
                board[row, i].isEnemy())
            {
                possibleMoves.Add(new int[2] { row, i });
                if (board[row, i].isEnemy()) break;
            }
            else break;
        }
    }
}
