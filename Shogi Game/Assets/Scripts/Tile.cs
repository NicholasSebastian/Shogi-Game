using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private int row;
    private int col;
    private Piece piece;

    private GameObject highlight;
    private GameObject pawnPrefab;

    void Start()
    {
        pawnPrefab = (GameObject)Resources.Load("Pawn");

        highlight = transform.GetChild(1).gameObject;
        highlight.SetActive(false);
    }

    public int getRow()
    {
        return row;
    }

    public int getCol()
    {
        return col;
    }

    public void setRow(int row)
    {
        this.row = row;
    }

    public void setCol(int col)
    {
        this.col = col;
    }

    public void setPosition(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public PieceType getState()
    {
        return (
            this.piece != null ?
            piece.getPiece() :
            PieceType.None
        );
    }

    public void setState(PieceType state)
    {
        if (state == PieceType.None)
        {
            removePiece();
        }
        else
        {
            if (this.piece == null)
            {
                addPiece(state);
            }
            else
            {
                replacePiece(state);
            }
        }
    }

    private void addPiece(PieceType state)
    {
        switch (state)
        {
            case PieceType.Pawn:
                this.piece =
                    Instantiate(pawnPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            // add other piece cases here.
            default:
                break;
        }
        this.piece.setPiece(state);
    }

    private void replacePiece(PieceType state)
    {
        // code here.
    }

    private void removePiece()
    {
        if (this.piece)
        {
            Destroy(this.piece.gameObject);
            this.piece = null;
        }
    }

    public int[][] selected()
    {
        return piece.selected(this.row, this.col);
    }

    public void deselected()
    {
        if (this.piece)
        {
            piece.deselected();
        }
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
}
