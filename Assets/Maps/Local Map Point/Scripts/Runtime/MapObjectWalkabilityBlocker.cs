using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapObjectWalkabilityBlocker : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D boxCollider;

        private readonly List<(MapTile tile, Rect rect)> _registeredRegions = new();

        private void Awake()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();

            RegisterBlockedTiles();
        }

        private void RegisterBlockedTiles()
        {
            Bounds worldBounds = boxCollider.bounds;

            int minX = Mathf.FloorToInt(worldBounds.min.x + 0.5f);
            int maxX = Mathf.CeilToInt(worldBounds.max.x - 0.5f);
            int minY = Mathf.FloorToInt(worldBounds.min.y + 0.5f);
            int maxY = Mathf.CeilToInt(worldBounds.max.y - 0.5f);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    MapTile tile = LocalMapGenerator.Instance.GetTileAt(x, y);
                    if (tile == null)
                        continue;

                    Rect relativeRect = ComputeRelativeRect(new Vector2Int(x, y), worldBounds);
                    tile.AddBlockedRegion(relativeRect);
                    _registeredRegions.Add((tile, relativeRect));
                }
            }
        }

        private static Rect ComputeRelativeRect(Vector2Int tilePos, Bounds worldBounds)
        {
            float tileMinX = tilePos.x - 0.5f;
            float tileMinY = tilePos.y - 0.5f;

            float overlapMinX = Mathf.Max(worldBounds.min.x, tileMinX);
            float overlapMaxX = Mathf.Min(worldBounds.max.x, tileMinX + 1f);
            float overlapMinY = Mathf.Max(worldBounds.min.y, tileMinY);
            float overlapMaxY = Mathf.Min(worldBounds.max.y, tileMinY + 1f);

            float relMinX = overlapMinX - tileMinX;
            float relMaxX = overlapMaxX - tileMinX;
            float relMinY = overlapMinY - tileMinY;
            float relMaxY = overlapMaxY - tileMinY;

            return new Rect(relMinX, relMinY, relMaxX - relMinX, relMaxY - relMinY);
        }

        private void OnDestroy()
        {
            foreach (var (tile, rect) in _registeredRegions)
            {
                if (tile != null)
                    tile.RemoveBlockedRegion(rect);
            }
        }
    }
}
