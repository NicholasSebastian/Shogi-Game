using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game Manager script.

public class Board : MonoBehaviour
{
    private static readonly int boardSize = 9;
    private Tile[,] board = new Tile[boardSize, boardSize];
    private Tile selectedTile;

    void Start()
    {
        initializeBoard();
        preparePlayer();
    }

    void Update()
    {
        SelectionManager();
    }

    private void initializeBoard()
    {
        int row = 0, col = 0;
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            if (col == boardSize)
            {
                row++;
                col = 0;
            }
            tile.setPosition(row, col);
            board[row, col] = tile;
            col++;
        }
    }

    private void preparePlayer()
    {
        // Row of pawns across the board.
        for (int i = 0; i < boardSize; i++)
        {
            board[2, i].setState(PieceType.Pawn);
        }
        // TODO: add the other pieces.
    }

    private void SelectionManager()
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
                        if (this.selectedTile)
                        {
                            deselectPiece();
                        }
                        this.selectedTile = clickedTile;
                        selectPiece();
                    }
                    else
                    {
                        if (clickedTile.isHighlighted())
                        {
                            movePiece(clickedTile);
                        }
                        else
                        {
                            deselectPiece();
                        }
                    }
                }
                else
                {
                    deselectPiece();
                }
            }
            else
            {
                deselectPiece();
            }
        }
    }

    private void selectPiece()
    {
        foreach (int[] possibleMoves in this.selectedTile.selected())
        {
            board[possibleMoves[0], possibleMoves[1]].highlightEnable();
        }
    }

    private void deselectPiece()
    {
        if (this.selectedTile)
        {
            this.selectedTile.deselected();
            this.selectedTile = null;

            foreach (Tile tile in board)
            {
                tile.highlightDisable();
            }
        }
    }

    private void movePiece(Tile targetTile)
    {
        if (this.selectedTile)
        {
            targetTile.setState(this.selectedTile.getState());
            this.selectedTile.setState(PieceType.None);
            deselectPiece();
        }
    }
}
