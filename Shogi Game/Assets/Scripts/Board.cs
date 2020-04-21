using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game Manager script.

// TODO: Checkmate Detection: Check for overlap between king possible moves and enemy moves.
// TODO: Piece Promotion.
// TODO: Piece Face Textures.
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
        checkStatus();
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

    private void checkStatus()
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
            Debug.Log("No possible moves... Enemy Checkmate?");
    }

    private IEnumerator EnemyControls()
    {
        Debug.Log("Enemy's Turn");
        float startTime = Time.time;
        yield return new WaitForSeconds(0.8f);

        while (playersTurn == false)
        {
            yield return new WaitForSeconds(
                Time.time - startTime < EnemyMinThinkTime ?
                0.2f : 0.0f
            );
            // Initialize lists for the different targets.
            List<Tile[]> playerTargets = new List<Tile[]>();
            List<Tile[]> spaceTargets = new List<Tile[]>();

            // For all the possible targets of all enemy pieces present:
            foreach (Tile currentTile in board)
            {
                if (currentTile.getState() != PieceType.None && currentTile.isEnemy())
                    foreach (int[] possibleMove in currentTile.getMoves(this.board))
                    {
                        // Skip invalid moves / moves that lead to another enemy piece.
                        int x = possibleMove[0], y = possibleMove[1];
                        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize
                            || board[x, y].isEnemy()) continue;

                        Tile targetTile = board[x, y];
                        // If the target is the player's piece:
                        if (targetTile.getState() != PieceType.None &&
                            targetTile.isEnemy() == false)
                        {
                            // If the target is a king, then attack and stop checking.
                            if (targetTile.getState() == PieceType.King)
                            {
                                yield return
                                    currentTile.StartCoroutine(
                                        currentTile.moveState(targetTile)
                                    );
                                boardSound.PlayOneShot(boardSound.clip);
                                playersTurn = true;
                                break;
                            }
                            // add to a list of playerTargets.
                            else playerTargets.Add(new Tile[2] { currentTile, targetTile });
                        }
                        // If the target is empty, add to a list of spaceTargets.
                        else if (targetTile.getState() == PieceType.None)
                            spaceTargets.Add(new Tile[2] { currentTile, targetTile });
                    }
                if (playersTurn) break;
            }

            int c = playerTargets.Count + spaceTargets.Count;
            Debug.Log("No. of Possible Enemy Moves: " + c);

            Tile[] moveTarget = new Tile[2];
            // If there is/are player piece(s) in range of attack, high chance to attack:
            if (playerTargets.Count > 0 && Random.value < enemyOffensiveLevel)
                moveTarget = playerTargets[Random.Range(0, playerTargets.Count - 1)];

            // If there is/are empty space(s) in range of movement, move:
            else if (spaceTargets.Count > 0)
                moveTarget = spaceTargets[Random.Range(0, spaceTargets.Count - 1)];

            // If player has no more possible moves.
            else Debug.Log("No possible moves... Player Checkmate?");

            // Finally execute the enemy's move.
            Tile enemySelectedTile = moveTarget[0];
            Tile enemyTargetedTile = moveTarget[1];
            yield return
                enemySelectedTile.StartCoroutine(
                    enemySelectedTile.moveState(enemyTargetedTile)
                );
            boardSound.PlayOneShot(boardSound.clip);
            playersTurn = true;
        }
    }
}
