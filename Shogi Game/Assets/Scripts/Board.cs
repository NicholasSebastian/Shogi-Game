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
            board[2, i].setState(PieceType.Pawn, false, false);
        }
        board[1, 1].setState(PieceType.Bishop, false, false);
        board[1, 7].setState(PieceType.Rook, false, false);
        board[0, 0].setState(PieceType.Lance, false, false);
        board[0, 8].setState(PieceType.Lance, false, false);
        board[0, 1].setState(PieceType.Knight, false, false);
        board[0, 7].setState(PieceType.Knight, false, false);
        board[0, 2].setState(PieceType.Silver, false, false);
        board[0, 6].setState(PieceType.Silver, false, false);
        board[0, 3].setState(PieceType.Gold, false, false);
        board[0, 5].setState(PieceType.Gold, false, false);
        board[0, 4].setState(PieceType.King, false, false);

        // Prepare the enemy's side.
        for (int i = 0; i < boardSize; i++)
        {
            board[6, i].setState(PieceType.Pawn, true, false);
        }
        board[7, 7].setState(PieceType.Bishop, true, false);
        board[7, 1].setState(PieceType.Rook, true, false);
        board[8, 8].setState(PieceType.Lance, true, false);
        board[8, 0].setState(PieceType.Lance, true, false);
        board[8, 7].setState(PieceType.Knight, true, false);
        board[8, 1].setState(PieceType.Knight, true, false);
        board[8, 6].setState(PieceType.Silver, true, false);
        board[8, 2].setState(PieceType.Silver, true, false);
        board[8, 5].setState(PieceType.Gold, true, false);
        board[8, 3].setState(PieceType.Gold, true, false);
        board[8, 4].setState(PieceType.King, true, false);
    }
}
