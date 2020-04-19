using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Pieces state = Pieces.None;

    public Pieces getState()
    {
        return state;
    }

    public void setState(Pieces state)
    {
        this.state = state;
    }
}
