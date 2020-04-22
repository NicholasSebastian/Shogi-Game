using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private int row;
    private int col;
    private Piece piece;

    private GameObject highlight;

    void Awake()
    {
        highlight = transform.GetChild(1).gameObject;
        highlight.SetActive(false);
    }

    public void setPosition(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public void raised()
    {
        if (this.piece)
            this.piece.raised();
    }

    public void deselected()
    {
        if (this.piece)
            this.piece.deselected();
    }

    public void highlightEnable()
    {
        highlight.SetActive(true);
    }

    public void highlightDisable()
    {
        highlight.SetActive(false);
    }

    public bool isHighlighted()
    {
        return highlight.activeSelf;
    }

    public bool isEnemy()
    {
        return (
            this.piece != null ?
            this.piece.isEnemy() :
            false
        );
    }

    public PieceType getState()
    {
        return (
            this.piece != null ?
            this.piece.getPiece() :
            PieceType.None
        );
    }

    public List<int[]> getMoves(Tile[,] board)
    {
        return piece.pieceMoves(this.row, this.col, board);
    }

    public void setState(PieceType state, bool enemy)
    {
        if (state == PieceType.None) removePiece();
        else
        {
            if (this.piece == null) addPiece(state, enemy);
            else
            {
                Debug.Log(
                    (this.piece.isEnemy() ? "Player's " : "Enemy's ") + state
                    + " killed " +
                    (this.piece.isEnemy() ? "Enemy's " : "Player's ") + this.piece.getPiece()
                );
                removePiece();
                addPiece(state, enemy);
            }
        }
    }

    public IEnumerator moveState(Tile targetTile)
    {
        PieceType temp = this.piece.getPiece();
        bool tempSide = this.piece.isEnemy();
        yield return
            this.piece.StartCoroutine(
                this.piece.moveAnimation(targetTile.transform.position)
            );
        setState(PieceType.None, false);
        targetTile.setState(temp, tempSide);
        targetTile.checkPromotion();
    }

    private void addPiece(PieceType state, bool enemy)
    {
        switch (state)
        {
            case PieceType.Pawn:
                this.piece =
                    Instantiate(
                        GameController.pawnPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Bishop:
                this.piece =
                    Instantiate(
                        GameController.bishopPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Rook:
                this.piece =
                    Instantiate(
                        GameController.rookPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Lance:
                this.piece =
                    Instantiate(
                        GameController.lancePrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Knight:
                this.piece =
                    Instantiate(
                        GameController.knightPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Silver:
                this.piece =
                    Instantiate(
                        GameController.silverPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Gold:
                this.piece =
                    Instantiate(
                        GameController.goldPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.King:
                this.piece =
                    Instantiate(
                        GameController.kingPrefab,
                        transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            default:
                break;
        }
        if (enemy) this.piece.gameObject.transform.GetChild(0).Rotate(0, 180, 0, Space.Self);
        this.piece.setPiece(state);
        this.piece.setSide(enemy);
    }

    private void removePiece()
    {
        if (this.piece)
        {
            Destroy(this.piece.gameObject);
            this.piece = null;
        }
    }

    private void checkPromotion()
    {
        if (this.piece.getPiece() == PieceType.King ||
            this.piece.getPiece() == PieceType.Gold)
            return;

        if (this.piece.isEnemy() == false &&
            this.row >= Board.boardSize - 3)
            this.piece.promotion();

        else if (this.piece.isEnemy() == true &&
            this.row < 3)
            this.piece.promotion();
    }
}
