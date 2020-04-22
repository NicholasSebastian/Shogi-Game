using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // TODO: Checkmate Detection: Check for overlap between possible king moves and enemy moves.
    // - If all possible king moves are overlapped, checkmate. Else, move the king to avoid checkmate.

    // TODO: Piece Promotion.
    // TODO: Piece Face Textures.
    // TODO: UI.
    // TODO: Add Japanese-style, Shamisen-like, Creative-Commons Background Music.

    public static bool game = true;
    public static bool multiplayer = false;
    public static bool playersTurn = true;

    public static float enemyOffensiveLevel = 0.8f;
    public static float EnemyMinThinkTime = 2.0f;

    private static GameObject boardPrefab;
    public static GameObject pawnPrefab;
    public static GameObject bishopPrefab;
    public static GameObject rookPrefab;
    public static GameObject lancePrefab;
    public static GameObject knightPrefab;
    public static GameObject silverPrefab;
    public static GameObject goldPrefab;
    public static GameObject kingPrefab;

    private Board board;

    void Awake()
    {
        boardPrefab = (GameObject)Resources.Load("Board");
        pawnPrefab = (GameObject)Resources.Load("Pawn");
        bishopPrefab = (GameObject)Resources.Load("Bishop");
        rookPrefab = (GameObject)Resources.Load("Rook");
        lancePrefab = (GameObject)Resources.Load("Lance");
        knightPrefab = (GameObject)Resources.Load("Knight");
        silverPrefab = (GameObject)Resources.Load("Silver");
        goldPrefab = (GameObject)Resources.Load("Gold");
        kingPrefab = (GameObject)Resources.Load("King");
    }

    void Start()
    {
        board = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0))
                .GetComponent<Board>();

        if (multiplayer == false)
            StartCoroutine(GameLoop());
        else if (multiplayer)
            Debug.Log("Two player local game not yet implemented.");
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

    private void endGame(DeathType deathType)
    {
        game = false;
        switch (deathType)
        {
            case DeathType.Checkmate:
                break;
            case DeathType.KingKilled:
                break;
            case DeathType.NoMoves:
                break;
            case DeathType.NoPieces:
                break;
            default:
                break;
        }
    }

    public static bool isPlayersTurn()
    {
        return playersTurn;
    }

    private IEnumerator PlayerControls()
    {
        playerStatus();
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
                                if (board.selectedTile)
                                    deselectPiece();
                                board.selectedTile = clickedTile;
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

    public IEnumerator EnemyControls()
    {
        float startTime = Time.time;
        yield return new WaitForSeconds(0.8f);

        while (isPlayersTurn() == false)
        {
            yield return new WaitForSeconds(
                Time.time - startTime < EnemyMinThinkTime ?
                0.2f : 0.0f
            );
            // Initialize lists for the different targets.
            List<Tile[]> playerTargets = new List<Tile[]>();
            List<Tile[]> spaceTargets = new List<Tile[]>();

            int numberOfPieces = 0;
            // For all the possible targets of all enemy pieces present:
            foreach (Tile currentTile in board.board)
            {
                if (currentTile.getState() != PieceType.None && currentTile.isEnemy())
                {
                    numberOfPieces++;
                    foreach (int[] possibleMove in currentTile.getMoves(board.board))
                    {
                        // Skip invalid moves / moves that lead to another enemy piece.
                        int x = possibleMove[0], y = possibleMove[1];
                        if (x < 0 || x >= Board.boardSize || y < 0 || y >= Board.boardSize
                            || board.board[x, y].isEnemy()) continue;

                        Tile targetTile = board.board[x, y];
                        // If the target is the player's piece:
                        if (targetTile.getState() != PieceType.None &&
                            targetTile.isEnemy() == false)
                        {
                            // If the target is a king, then attack and stop checking.
                            if (targetTile.getState() == PieceType.King)
                            {
                                yield return StartCoroutine(pieceMovement(currentTile, targetTile));
                                endGame(DeathType.KingKilled);
                                break;
                            }
                            // add to a list of playerTargets.
                            else playerTargets.Add(new Tile[2] { currentTile, targetTile });
                        }
                        // If the target is empty, add to a list of spaceTargets.
                        else if (targetTile.getState() == PieceType.None)
                            spaceTargets.Add(new Tile[2] { currentTile, targetTile });
                    }
                }
                if (isPlayersTurn()) break;
            }
            // If it turns out the enemy has no pieces, declare the loss.
            if (numberOfPieces == 0) endGame(DeathType.NoPieces);

            Tile[] moveTarget = new Tile[2];
            // If there is/are player piece(s) in range of attack, high chance to attack:
            if (playerTargets.Count > 0 && Random.value < enemyOffensiveLevel)
                moveTarget = playerTargets[Random.Range(0, playerTargets.Count - 1)];

            // If there is/are empty space(s) in range of movement, move:
            else if (spaceTargets.Count > 0)
                moveTarget = spaceTargets[Random.Range(0, spaceTargets.Count - 1)];

            // Else declare that there are no available moves and lose.
            else endGame(DeathType.NoMoves);

            // Finally execute the enemy's move.
            Tile enemySelectedTile = moveTarget[0];
            Tile enemyTargetedTile = moveTarget[1];
            yield return StartCoroutine(pieceMovement(enemySelectedTile, enemyTargetedTile));
        }
    }

    public void selectPiece()
    {
        board.selectedTile.raised();
        foreach (int[] possibleMove in board.selectedTile.getMoves(board.board))
        {
            int x = possibleMove[0], y = possibleMove[1];
            if (x >= 0 && x < Board.boardSize && y >= 0 && y < Board.boardSize)
                board.board[x, y].highlightEnable();
        }
    }

    public void deselectPiece()
    {
        if (board.selectedTile)
        {
            board.selectedTile.deselected();
            board.selectedTile = null;
            foreach (Tile tile in board.board) tile.highlightDisable();
        }
    }

    public void movePiece(Tile targetTile)
    {
        if (targetTile.isHighlighted())
        {
            if (board.selectedTile)
            {
                StartCoroutine(pieceMovement(board.selectedTile, targetTile));
                if (targetTile.getState() == PieceType.King)
                    endGame(DeathType.KingKilled);
            }
        }
        else deselectPiece();
    }

    private IEnumerator pieceMovement(Tile currentTile, Tile targetTile)
    {
        yield return
            currentTile.StartCoroutine(
                currentTile.moveState(targetTile)
            );
        board.boardSound.PlayOneShot(board.boardSound.clip);
        if (playersTurn) deselectPiece();
        playersTurn = !playersTurn;
    }

    public void playerStatus()
    {
        // For all the possible targets of all player pieces present:
        int numberOfPieces = 0, numberOfMoves = 0;
        foreach (Tile tile in board.board)
        {
            if (tile.getState() != PieceType.None && tile.isEnemy() == false)
            {
                numberOfPieces++;
                foreach (int[] possibleMove in tile.getMoves(board.board))
                {
                    // Skip invalid moves / moves that lead to another fellow piece.
                    int x = possibleMove[0], y = possibleMove[1];
                    if (x < 0 || x >= Board.boardSize ||
                        y < 0 || y >= Board.boardSize || (
                        board.board[x, y].getState() != PieceType.None &&
                        board.board[x, y].isEnemy() == false
                        ))
                        continue;
                    numberOfMoves++;
                }
            }
        }
        if (numberOfPieces == 0) endGame(DeathType.NoPieces);
        if (numberOfMoves == 0) endGame(DeathType.NoMoves);
    }

    private enum DeathType
    {
        Checkmate, KingKilled, NoMoves, NoPieces
    };
}
