using System.Collections.Generic;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    /// <summary>
    /// Helpers for reasoning about where a tile's footprint would land *before* it's
    /// actually placed on the grid, so the enemy AI can score candidate placements
    /// without mutating real game state. Built entirely on TileGridBase/TileBase's
    /// existing API (Shape, Rotate, AddToAll, TryGetTileAtPosition) so it stays in
    /// sync with however placement/adjacency actually works.
    /// </summary>
    public static class GridFootprintMath
    {
        /// <summary>World cells a tile would occupy if placed at position/rotation right now.</summary>
        public static List<Vector2Int> GetHypotheticalFootprint(TileBase tile, Vector2Int position, float rotation)
        {
            return tile.Shape.Rotate(rotation).AddToAll(position);
        }

        /// <summary>
        /// The distinct already-placed tiles that would be adjacent (8-directional, matching
        /// TileGridBase.GetAdjacentTiles) to a hypothetical tile at position/rotation.
        /// </summary>
        public static IEnumerable<TileBase> GetHypotheticalAdjacentTiles(
            TileGridBase grid, TileBase tile, Vector2Int position, float rotation)
        {
            List<Vector2Int> footprint = GetHypotheticalFootprint(tile, position, rotation);
            var footprintSet = new HashSet<Vector2Int>(footprint);
            var seen = new HashSet<TileBase>();

            foreach (Vector2Int cell in footprint)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        Vector2Int neighbor = new Vector2Int(cell.x + dx, cell.y + dy);
                        if (footprintSet.Contains(neighbor)) continue;

                        if (grid.TryGetTileAtPosition(neighbor, out TileBase candidate) && seen.Add(candidate))
                            yield return candidate;
                    }
                }
            }
        }
    }
}
