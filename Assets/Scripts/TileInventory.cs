using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInventory : MonoBehaviour
{
    public Dictionary<Tile, int> tilesAndAmount = new Dictionary<Tile, int>();

    public delegate void OnChange();
    public OnChange onChange;

    public void AddTile(Tile tile)
    {
        if (tilesAndAmount.ContainsKey(tile))
        {
            tilesAndAmount[tile]++;
        }
        else
        {
            tilesAndAmount.Add(tile, 1);
        }

        onChange?.Invoke();
    }

    public void RemoveTile(Tile tile)
    {
        if (tilesAndAmount.ContainsKey(tile))
        {
            tilesAndAmount[tile]--;

            if (tilesAndAmount[tile] <= 0)
                tilesAndAmount.Remove(tile);

            onChange?.Invoke();
        }
    }
}
