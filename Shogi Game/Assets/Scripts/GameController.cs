using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // TODO: Checkmate Detection: Check for overlap between possible king moves and enemy moves.
    // - If all possible king moves are overlapped, checkmate. Else, move the king to avoid checkmate.
    // - Add Death/GameEnd function; Call when King is dead or possible moves / no. of pieces = 0.

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

    public static bool isPlayersTurn()
    {
        return playersTurn;
    }

    public static void endTurn()
    {
        playersTurn = !playersTurn;
    }

    public static void endGame(bool playerWin)
    {
        Debug.Log(
            playerWin ?
            "PLAYER WINS" :
            "ENEMY WINS"
        );
    }

    private IEnumerator PlayerControls()
    {
        Debug.Log("Your Turn");
        board.checkStatus();
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
                                    board.deselectPiece();
                                board.selectedTile = clickedTile;
                                board.selectPiece();
                            }
                            else board.movePiece(clickedTile);
                        else board.movePiece(clickedTile);
                    else board.deselectPiece();
                }
                else board.deselectPiece();
            }
            yield return null;
        }
    }

    public IEnumerator EnemyControls()
    {
        Debug.Log("Enemy's Turn");
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

            // For all the possible targets of all enemy pieces present:
            foreach (Tile currentTile in board.board)
            {
                if (currentTile.getState() != PieceType.None && currentTile.isEnemy())
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
                                yield return
                                    currentTile.StartCoroutine(
                                        currentTile.moveState(targetTile)
                                    );
                                board.boardSound.PlayOneShot(board.boardSound.clip);
                                GameController.endTurn();
                                break;
                            }
                            // add to a list of playerTargets.
                            else playerTargets.Add(new Tile[2] { currentTile, targetTile });
                        }
                        // If the target is empty, add to a list of spaceTargets.
                        else if (targetTile.getState() == PieceType.None)
                            spaceTargets.Add(new Tile[2] { currentTile, targetTile });
                    }
                if (GameController.isPlayersTurn() == true) break;
            }

            int c = playerTargets.Count + spaceTargets.Count;
            Debug.Log("No. of Possible Enemy Moves: " + c);

            Tile[] moveTarget = new Tile[2];
            // If there is/are player piece(s) in range of attack, high chance to attack:
            if (playerTargets.Count > 0 && Random.value < GameController.enemyOffensiveLevel)
                moveTarget = playerTargets[Random.Range(0, playerTargets.Count - 1)];

            // If there is/are empty space(s) in range of movement, move:
            else if (spaceTargets.Count > 0)
                moveTarget = spaceTargets[Random.Range(0, spaceTargets.Count - 1)];

            // If player has no more possible moves.
            else GameController.endGame(true);

            // Finally execute the enemy's move.
            Tile enemySelectedTile = moveTarget[0];
            Tile enemyTargetedTile = moveTarget[1];
            yield return
                enemySelectedTile.StartCoroutine(
                    enemySelectedTile.moveState(enemyTargetedTile)
                );
            board.boardSound.PlayOneShot(board.boardSound.clip);
            GameController.endTurn();
        }
    }
}
