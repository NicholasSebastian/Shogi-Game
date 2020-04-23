using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // TODO: Remove all the extra prefabs for every piece. Just use one.
    // TODO: UI.
    // TODO: Checkmate Detection: Check for overlap between possible king moves and enemy moves.
    // - If all possible king moves are overlapped, checkmate. Else, move the king to avoid checkmate.
    // TODO: Dropping???

    public static bool game = true;
    public static bool multiplayer = false;
    public static bool playersTurn = true;

    public static float enemyOffensiveLevel = 0.8f;
    public static float EnemyMinThinkTime = 2.0f;

    private int turnCounter;

    private static GameObject boardPrefab;
    public static GameObject facePrefab;
    public static GameObject piecePrefab;

    private Board board;

    void Awake()
    {
        boardPrefab = (GameObject)Resources.Load("Board");
        facePrefab = (GameObject)Resources.Load("Face");
        piecePrefab = (GameObject)Resources.Load("Piece");
    }

    void Start()
    {
        board = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0))
                .GetComponent<Board>();
        turnCounter = 0;

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
            turnCounter++;
        }
    }

    private void endGame(DeathType deathType, bool playerWins)
    {
        game = false;
        switch (deathType)
        {
            case DeathType.Checkmate:
                break;

            case DeathType.KingKilled:
                Debug.Log(
                    playerWins ?
                    "Player Wins! Enemy's King has been killed." :
                    "Enemy Wins! Player's King has been killed."
                );
                break;

            case DeathType.NoMoves:
                Debug.Log(
                    playerWins ?
                    "Player Wins! Enemy's King has been killed." :
                    "Enemy Wins! Player's King has been killed."
                );
                break;

            case DeathType.NoPieces:
                Debug.Log(
                    playerWins ?
                    "Player Wins! Enemy's King has been killed." :
                    "Enemy Wins! Player's King has been killed."
                );
                break;

            default:
                break;
        }
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

        while (playersTurn == false)
        {
            yield return new WaitForSeconds(
                Time.time - startTime < EnemyMinThinkTime ?
                0.2f : 0.0f
            );
            // Initialize lists for the different targets.
            List<Tile[]> playerTargets = new List<Tile[]>();
            List<Tile[]> spaceTargets = new List<Tile[]>();
            List<Tile[]> pawnSpaceTargets = new List<Tile[]>();

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
                                endGame(DeathType.KingKilled, false);
                                break;
                            }
                            // add to a list of playerTargets.
                            else playerTargets.Add(new Tile[2] { currentTile, targetTile });
                        }
                        // If the target is empty, add to a list of spaceTargets.
                        else if (targetTile.getState() == PieceType.None)
                        {
                            spaceTargets.Add(new Tile[2] { currentTile, targetTile });
                            if (currentTile.getState() == PieceType.Pawn)
                                pawnSpaceTargets.Add(new Tile[2] { currentTile, targetTile });
                        }
                    }
                }
                if (game == false) break;
            }
            // If it turns out the enemy has no pieces, declare the loss.
            if (numberOfPieces == 0) endGame(DeathType.NoPieces, true);

            Tile[] moveTarget = new Tile[2];
            // If there is/are player piece(s) in range of attack, high chance to attack:
            if (playerTargets.Count > 0 && Random.value < enemyOffensiveLevel)
                moveTarget = playerTargets[Random.Range(0, playerTargets.Count - 1)];

            // If there is/are empty space(s) in range of movement, move:
            else if (spaceTargets.Count > 0)
                moveTarget = (
                    turnCounter < enemyOffensiveLevel * 20 &&
                    Random.value < enemyOffensiveLevel ?
                    pawnSpaceTargets[Random.Range(0, pawnSpaceTargets.Count - 1)] :
                    spaceTargets[Random.Range(0, spaceTargets.Count - 1)]
                );

            // Else declare that there are no available moves and lose.
            else endGame(DeathType.NoMoves, true);

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
                    endGame(DeathType.KingKilled, true);
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
        if (numberOfPieces == 0) endGame(DeathType.NoPieces, false);
        if (numberOfMoves == 0) endGame(DeathType.NoMoves, false);
    }

    private enum DeathType
    {
        Checkmate, KingKilled, NoMoves, NoPieces
    };
}
