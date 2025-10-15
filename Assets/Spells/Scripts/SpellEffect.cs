using Map;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SpellEffect
{
    public virtual List<Vector2Int> Vector2Ints => Vector2ListWrapper.v2IntList;

    public Vector2ListWrapper Vector2ListWrapper;

    public LayerMask layerMask;

    [SerializeField]
    protected FX effectFX;
    [SerializeField]
    [Range(0f, 1f)]
    protected float fXGreyScaleFactor;
    [SerializeField]
    protected Color fXColor;
    [SerializeField]
    protected AudioClipAndSettings effectSound;

    public abstract void Effect(Vector2Int position);
    public abstract bool CanUseEffect(Vector2Int position);

    public List<Vector2Int> GetPositions(Vector2Int position)
    {
        return Vector2Ints.AddToAll(position);
    }

    public List<MapTile> GetMapTiles(Vector2Int position)
    {
        LocalMapGenerator localMapGenerator = LocalMapGenerator.Instance;

        List<Vector2Int> v2IntList = Vector2Ints;

        List<MapTile> returnMapTiles = new List<MapTile>();

        foreach (Vector2Int v2Int in v2IntList)
        {
            int x = position.x + v2Int.x;
            int y = position.y + v2Int.y;

            if (localMapGenerator.Tiles != null && x >= 0 && x < localMapGenerator.Tiles.GetLength(0) && y >= 0 && y < localMapGenerator.Tiles.GetLength(1))
            {
                returnMapTiles.Add(localMapGenerator.Tiles[x, y]);
            }
        }

        return returnMapTiles;
    }

    protected List<Collider2D> GetColliders2D(Vector2Int origin)
    {
        List<Collider2D> colliders = new List<Collider2D>();

        foreach (BoxVector2IntDimensions box in Vector2Ints.AddToAll(origin).GetVector2IntBoxDimensions())
        {
            colliders.AddRange(Physics2D.OverlapBoxAll(box.position, box.size - new Vector2(0.2f, 0.2f), 0f, layerMask));
            //Debug.Log($"Position: {box.position}, Size: {box.size}");
        }

        colliders.RemoveDuplicates();

        return colliders;
    }

    public virtual void Reset()
    {

    }
}
