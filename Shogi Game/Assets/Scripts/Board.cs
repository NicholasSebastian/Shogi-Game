using System.Collections;
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
        foreach (int[] possibleMove in this.selectedTile.selected(this.board))
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
                Debug.Log(
                    this.selectedTile.getState() + " at " +
                    this.selectedTile.name + " moving to " +
                    targetTile.name
                );
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
        deselectPiece();
    }

    private IEnumerator EnemyControls()
    {
        Debug.Log("Enemy's Turn");
        while (playersTurn == false)
        {
            yield return new WaitForSeconds(0.2f);
            List<Tile> enemyTiles = new List<Tile>();
            foreach (Tile tile in board)
                if (tile.getState() != PieceType.None && tile.isEnemy())
                    enemyTiles.Add(tile);
            Tile selectedEnemyTile = enemyTiles[(
                Random.value < 0.8f ?
                Random.Range(0, Mathf.RoundToInt((enemyTiles.Count - 1) / 2)) :
                Random.Range(Mathf.RoundToInt((enemyTiles.Count - 1) / 2) + 1, enemyTiles.Count - 1)
            )];
            List<int[]> possibleMoves = selectedEnemyTile.selected(this.board);
            for (int i = possibleMoves.Count - 1; i >= 0; i--)
            {
                int[] possibleMove = possibleMoves[i];
                int x = possibleMove[0], y = possibleMove[1];
                if (x < 0 || x >= boardSize || y < 0 || y >= boardSize
                    || board[x, y].isEnemy())
                    possibleMoves.Remove(possibleMove);
            }
            if (possibleMoves.Count > 0)
            {
                int[] targetMove = (
                    possibleMoves.Count == 1 ?
                    possibleMoves[0] :
                    possibleMoves[Random.Range(0, possibleMoves.Count - 1)]
                );
                Tile targetTile = board[targetMove[0], targetMove[1]];
                Debug.Log(
                    selectedEnemyTile.getState() + " at " +
                    selectedEnemyTile.name + " moving to " +
                    targetTile.name
                );
                yield return
                    selectedEnemyTile.StartCoroutine(
                        selectedEnemyTile.moveState(targetTile)
                    );
                playersTurn = true;
            }
            else selectedEnemyTile.deselected();
        }
    }
}
