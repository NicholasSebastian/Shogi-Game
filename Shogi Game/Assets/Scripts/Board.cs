using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private static int boardSize = 9;

    private Transform[,] board = new Transform[boardSize, boardSize];
    private Tile[,] boardState = new Tile[boardSize, boardSize];

    private Transform selectedPiece;

    private GameObject pawnPrefab;

    private void initializeBoard()
    {
        int row = 0, col = 0;
        foreach (Transform tile in transform)
        {
            if (col == board.GetLength(0))
            {
                row++;
                col = 0;
            }
            board[row, col] = tile;
            boardState[row, col] = tile.GetComponent<Tile>();
            col++;
        }
    }

    private void loadResources()
    {
        pawnPrefab = (GameObject)Resources.Load("Pawn");
    }

    private void preparePlayer()
    {
        // Row of pawns across the board.
        for (int i = 0; i < board.GetLength(1); i++)
        {
            boardState[2, i].setState(Pieces.Pawn);
            Instantiate(pawnPrefab, board[2, i].position, Quaternion.identity);
        }
    }

    void Start()
    {
        loadResources();
        initializeBoard();
        preparePlayer();
    }

    void Update()
    {
        pieceSelection();
    }

    private void pieceSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Transform selectedPiece = hit.transform;
                if (selectedPiece.CompareTag("Piece"))
                {
                    if (this.selectedPiece)
                    {
                        this.selectedPiece.GetComponent<Piece>().deselected();
                    }
                    this.selectedPiece = selectedPiece;
                    this.selectedPiece.GetComponent<Piece>().selected();
                }
            }
            else
            {
                if (this.selectedPiece)
                {
                    this.selectedPiece.GetComponent<Piece>().deselected();
                    this.selectedPiece = null;
                }
            }
        }
    }
}
