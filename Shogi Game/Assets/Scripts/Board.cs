﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game Manager script.
// TODO: Piece Promotion.
// TODO: Piece Face Textures.
// TODO: Checkmate Detection.
// TODO: Sounds.
// TODO: UI.

public class Board : MonoBehaviour
{
    private AudioSource boardSound;

    public static readonly int boardSize = 9;
    private Tile[,] board = new Tile[boardSize, boardSize];
    private Tile selectedTile;

    private bool game = true;
    private bool playersTurn = true;

    // The higher this value, the longer the load time.
    public static float enemyOffensiveLevel = 0.8f;
    public static float EnemyMinThinkTime = 2.0f;

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

            GameCheck();
        }
    }

    private void GameCheck()
    {
        int playerPieces = 0, enemyPieces = 0;
        bool playerKing = false, enemyKing = false;

        foreach (Tile tile in board)
        {
            if (tile.isEnemy()) enemyPieces++;
            else playerPieces++;

            if (tile.getState() == PieceType.King)
            {
                if (tile.isEnemy()) enemyKing = true;
                else playerKing = true;
            }
        }

        if (enemyPieces == 0 || enemyKing == false)
        {
            Debug.Log("PLAYER WINS");
            game = false;
        }
        else if (playerPieces == 0 || playerKing == false)
        {
            Debug.Log("ENEMY WINS");
            game = false;
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
                        if (clickedTile.getState() != PieceType.None)
                            if (clickedTile.isEnemy() == false)
                            {
                                if (this.selectedTile) deselectPiece();
                                this.selectedTile = clickedTile;
                                selectPiece();
                            }
                            else movePiece(clickedTile);
                        else movePiece(clickedTile);
                    else deselectPiece();
                }
                else deselectPiece();
            }
            yield return null;
        }
    }

    private void selectPiece()
    {
        this.selectedTile.raised();
        foreach (int[] possibleMove in this.selectedTile.getMoves(this.board))
        {
            int x = possibleMove[0], y = possibleMove[1];
            if (x >= 0 && x < boardSize && y >= 0 && y < boardSize)
                board[x, y].highlightEnable();
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
                StartCoroutine(playerMovement(targetTile));
                playersTurn = false;
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

    private IEnumerator EnemyControls()
    {
        Debug.Log("Enemy's Turn");
        float startTime = Time.time;

        while (playersTurn == false)
        {
            yield return new WaitForSeconds(
                Time.time - startTime < EnemyMinThinkTime ?
                0.2f : 0.0f
            );

            // List all the enemy pieces on the board.
            List<Tile> enemyTiles = new List<Tile>();
            foreach (Tile tile in board)
                if (tile.getState() != PieceType.None && tile.isEnemy())
                    enemyTiles.Add(tile);

            // Choose one of the pieces, preferably one that is placed closer to the front.
            int forwardBiasedSelector = (
                Random.value < enemyOffensiveLevel ?
                Random.Range(0, Mathf.RoundToInt((enemyTiles.Count - 1) / 2)) :
                Random.Range(Mathf.RoundToInt((enemyTiles.Count - 1) / 2), enemyTiles.Count - 1)
            );
            Tile selectedEnemyTile = enemyTiles[forwardBiasedSelector];

            // List all the possible moves of this piece.
            List<int[]> possibleMoves = selectedEnemyTile.getMoves(this.board);

            // Remove all invalid moves.
            for (int i = possibleMoves.Count - 1; i >= 0; i--)
            {
                int[] possibleMove = possibleMoves[i];
                int x = possibleMove[0], y = possibleMove[1];
                if (x < 0 || x >= boardSize || y < 0 || y >= boardSize
                    || board[x, y].isEnemy())
                    possibleMoves.Remove(possibleMove);
            }

            // Proceed if it has valid moves, otherwise, go find another piece.
            if (possibleMoves.Count > 0)
            {
                // Select a random move from this list.
                int[] targetMove = possibleMoves[Random.Range(0, possibleMoves.Count - 1)];
                Tile targetTile = board[targetMove[0], targetMove[1]];

                // If it does not attack the player, high chance to have to find another piece.
                if (!(targetTile.getState() != PieceType.None &&
                    targetTile.isEnemy() == false))
                    if (Random.value < enemyOffensiveLevel) continue;

                // Execute the move, play the sound effect, and end the turn.
                yield return
                    selectedEnemyTile.StartCoroutine(
                        selectedEnemyTile.moveState(targetTile)
                    );
                boardSound.PlayOneShot(boardSound.clip);
                playersTurn = true;
            }
        }
    }
}
