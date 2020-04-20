﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private int row;
    private int col;
    private Piece piece;

    private GameObject highlight;

    private GameObject pawnPrefab;
    private GameObject bishopPrefab;
    private GameObject rookPrefab;
    private GameObject lancePrefab;
    private GameObject knightPrefab;
    private GameObject silverPrefab;
    private GameObject goldPrefab;
    private GameObject kingPrefab;

    void Start()
    {
        pawnPrefab = (GameObject)Resources.Load("Pawn");
        bishopPrefab = (GameObject)Resources.Load("Bishop");
        rookPrefab = (GameObject)Resources.Load("Rook");
        lancePrefab = (GameObject)Resources.Load("Lance");
        knightPrefab = (GameObject)Resources.Load("Knight");
        silverPrefab = (GameObject)Resources.Load("Silver");
        goldPrefab = (GameObject)Resources.Load("Gold");
        kingPrefab = (GameObject)Resources.Load("King");

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
            case PieceType.Bishop:
                this.piece =
                    Instantiate(bishopPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Rook:
                this.piece =
                    Instantiate(rookPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Lance:
                this.piece =
                    Instantiate(lancePrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Knight:
                this.piece =
                    Instantiate(knightPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Silver:
                this.piece =
                    Instantiate(silverPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.Gold:
                this.piece =
                    Instantiate(goldPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
            case PieceType.King:
                this.piece =
                    Instantiate(kingPrefab, transform.position, Quaternion.identity)
                    .GetComponent<Piece>();
                break;
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

    public List<int[]> selected()
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
