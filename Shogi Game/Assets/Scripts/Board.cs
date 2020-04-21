using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game Manager script.
// TODO: Enemy Movement.
// TODO: Piece Promotion.
// TODO: Sounds.
// TODO: UI.

public class Board : MonoBehaviour
{
    public static readonly int boardSize = 9;
    private Tile[,] board = new Tile[boardSize, boardSize];
    private Tile selectedTile;
    private bool game = true;
    private bool playersTurn = true;

    void Start()
    {
        initializeBoard();
        prepareBoard();
        StartCoroutine(GameLoop());
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

    private IEnumerator GameLoop()
    {
        while (game)
        {
            if (playersTurn)
                yield return
                StartCoroutine(PlayerControls());
            else
                yield return
                StartCoroutine(EnemyControls());
        }
    }

    private IEnumerator EnemyControls()
    {
        Debug.Log("Enemy's Turn");
        while (playersTurn == false)
        {
            if (Random.value < 0.3f) Debug.Log("Thinking...");
            else
            {
                Debug.Log("Pretend that enemy has moved.");
                playersTurn = true;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator PlayerControls()
    {
        Debug.Log("Your Turn");
        while (playersTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Transform clicked = hit.transform;
                    Tile clickedTile = clicked.GetComponent<Tile>();

                    if (clicked.CompareTag("Tile"))
                    {
                        if (clickedTile.getState() != PieceType.None)
                        {
                            // Select if player's piece.
                            if (clickedTile.isEnemy() == false)
                            {
                                // Swap selection if already holding a piece.
                                if (this.selectedTile) deselectPiece();
                                this.selectedTile = clickedTile;
                                selectPiece();
                            }
                            // Attack if enemy's piece.
                            else movePiece(clickedTile);
                        }
                        // Move if empty.
                        else movePiece(clickedTile);
                    }
                    else deselectPiece();
                }
                else deselectPiece();
            }
            yield return null;
        }
    }

    private void selectPiece()
    {
        foreach (int[] possibleMoves in this.selectedTile.selected(this.board))
        {
            if (possibleMoves[0] >= 0 && possibleMoves[0] < boardSize &&
                possibleMoves[1] >= 0 && possibleMoves[1] < boardSize
            )
                board[possibleMoves[0], possibleMoves[1]].highlightEnable();
        }
    }

    private void deselectPiece()
    {
        if (this.selectedTile)
        {
            this.selectedTile.deselected();
            this.selectedTile = null;
            foreach (Tile tile in board) tile.highlightDisable();
        }
    }

    private void movePiece(Tile targetTile)
    {
        if (targetTile.isHighlighted())
        {
            if (this.selectedTile)
            {
                StartCoroutine(movement(targetTile));
                playersTurn = false;
            }
        }
        else deselectPiece();
    }

    private IEnumerator movement(Tile targetTile)
    {
        yield return
            this.selectedTile.StartCoroutine(
                this.selectedTile.moveState(targetTile)
            );
        deselectPiece();
    }
}
