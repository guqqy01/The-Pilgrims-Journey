using System;
using System.Collections.Generic;
using Units;
using static UnityEngine.Rendering.DebugUI;
using Tiles;
using UnityEngine;

public class PrizeTilePage : TileGrid,  IPrizePage
{
    public void TileGridInit(List<TileAndUnit> tiles)
    {
        Clear();

        PlaceTiles(tiles);

        gameObject.SetActive(false);
    }
     
    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
