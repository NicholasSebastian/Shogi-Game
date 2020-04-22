using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static readonly int boardSize = 9;
    public Tile[,] board = new Tile[boardSize, boardSize];
    public Tile selectedTile;

    public AudioSource boardSound;

    void Awake()
    {
        initializeBoard();
        prepareBoard();
    }

    private void initializeBoard()
    {
        int row = 0, col = 0;
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            if (col == boardSize) { row++; col = 0; }
            tile.setPosition(row, col);
            board[row, col] = tile;
            col++;
        }
        boardSound = GetComponent<AudioSource>();
    }

    private void prepareBoard()
    {
        // Prepare the player's side.
        for (int i = 0; i < boardSize; i++)
        {
            board[2, i].setState(PieceType.Pawn, false);
        }
        board[1, 1].setState(PieceType.Bishop, false);
        board[1, 7].setState(PieceType.Rook, false);
        board[0, 0].setState(PieceType.Lance, false);
        board[0, 8].setState(PieceType.Lance, false);
        board[0, 1].setState(PieceType.Knight, false);
        board[0, 7].setState(PieceType.Knight, false);
        board[0, 2].setState(PieceType.Silver, false);
        board[0, 6].setState(PieceType.Silver, false);
        board[0, 3].setState(PieceType.Gold, false);
        board[0, 5].setState(PieceType.Gold, false);
        board[0, 4].setState(PieceType.King, false);

        // Prepare the enemy's side.
        for (int i = 0; i < boardSize; i++)
        {
            board[6, i].setState(PieceType.Pawn, true);
        }
        board[7, 7].setState(PieceType.Bishop, true);
        board[7, 1].setState(PieceType.Rook, true);
        board[8, 8].setState(PieceType.Lance, true);
        board[8, 0].setState(PieceType.Lance, true);
        board[8, 7].setState(PieceType.Knight, true);
        board[8, 1].setState(PieceType.Knight, true);
        board[8, 6].setState(PieceType.Silver, true);
        board[8, 2].setState(PieceType.Silver, true);
        board[8, 5].setState(PieceType.Gold, true);
        board[8, 3].setState(PieceType.Gold, true);
        board[8, 4].setState(PieceType.King, true);
    }

    public void selectPiece()
    {
        this.selectedTile.raised();
        foreach (int[] possibleMove in this.selectedTile.getMoves(this.board))
        {
            int x = possibleMove[0], y = possibleMove[1];
            if (x >= 0 && x < boardSize && y >= 0 && y < boardSize)
                board[x, y].highlightEnable();
        }
    }

    public void deselectPiece()
    {
        if (this.selectedTile)
        {
            this.selectedTile.deselected();
            this.selectedTile = null;
            foreach (Tile tile in board) tile.highlightDisable();
        }
    }

    public void movePiece(Tile targetTile)
    {
        if (targetTile.isHighlighted())
        {
            if (this.selectedTile)
            {
                StartCoroutine(playerMovement(targetTile));
                GameController.endTurn();
            }
        }
        else deselectPiece();
    }

    private IEnumerator playerMovement(Tile targetTile)
    {
        yield return
            this.selectedTile.StartCoroutine(
                this.selectedTile.moveState(targetTile)
            );
        boardSound.PlayOneShot(boardSound.clip);
        deselectPiece();
    }

    public void checkStatus()
    {
        // For all the possible targets of all player pieces present:
        int c = 0;
        foreach (Tile tile in board)
        {
            if (tile.getState() != PieceType.None && tile.isEnemy() == false)
                foreach (int[] possibleMove in tile.getMoves(this.board))
                {
                    // Skip invalid moves / moves that lead to another player piece.
                    int x = possibleMove[0], y = possibleMove[1];
                    if (x < 0 || x >= boardSize || y < 0 || y >= boardSize || (
                        board[x, y].getState() != PieceType.None &&
                        board[x, y].isEnemy() == false
                    )) continue;
                    c++;
                }
        }
        // Check if the player still has any valid moves.
        Debug.Log("No. of Possible Player Moves: " + c);
        if (c == 0)
            GameController.endGame(false);
    }
}
