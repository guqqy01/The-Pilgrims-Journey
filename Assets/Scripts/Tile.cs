using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Tile
{
    public bool locked;

    public Unit unit;

    public TileShape shape;

    public List<Vector2Int> Shape => shape.shape.v2IntList;
}
