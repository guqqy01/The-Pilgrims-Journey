using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public static class TileCardTypeAdjacency
    {
        private static IEnumerable<TileBase> GetTilesAdjacentToCell(TileBase tile, Vector2Int cell)
        {
            TileGridBase grid = tile.TileGrid;
            if (grid == null) yield break;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Vector2Int neighbor = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (grid.TryGetTileAtPosition(neighbor, out TileBase candidate) && candidate != tile)
                        yield return candidate;
                }
            }
        }

        /// <summary>Finds an adjacent tile of the given type owned by someone else, next to this
        /// one specific cell of `tile` — rather than the tile's footprint as a whole.</summary>
        public static bool TryFindOpposingAdjacentToCell(TileBase tile, Vector2Int cell, TileCardType type, out TileBase found)
        {
            found = null;
            DeckHolder owner = TileOwnership.Get(tile);

            foreach (TileBase adjacent in GetTilesAdjacentToCell(tile, cell))
            {
                if (!adjacent.TileCardType.HasFlag(type)) continue;
                if (TileOwnership.Get(adjacent) == owner) continue;
                found = adjacent;
                return true;
            }
            return false;
        }

        /// <summary>Whether this one specific cell of `tile` has a same-owner Guard adjacent to it.</summary>
        public static bool IsCellDefended(TileBase tile, Vector2Int cell)
        {
            DeckHolder owner = TileOwnership.Get(tile);

            foreach (TileBase adjacent in GetTilesAdjacentToCell(tile, cell))
            {
                if (adjacent.TileCardType.HasFlag(TileCardType.Guard) && TileOwnership.Get(adjacent) == owner)
                    return true;
            }
            return false;
        }

        /// <summary>All cells of `target` that are actually touching `from` — a tile can
        /// contact a neighbor at more than one cell, so a targeting UI that wants every valid
        /// option (rather than just "any one will do") should use this over FindContactCell.</summary>
        public static IEnumerable<Vector2Int> FindContactCells(TileBase from, TileBase target)
        {
            foreach (Vector2Int cell in target.ActivePlacedShape)
            {
                if (GetTilesAdjacentToCell(target, cell).Contains(from))
                    yield return cell;
            }
        }

        /// <summary>The first cell of `target` found touching `from` — convenience for callers
        /// (like the AI's automatic Brigade targeting) that just need any one valid cell rather
        /// than every option. Falls back to the target's first active cell if somehow none are
        /// directly touching (shouldn't normally happen for a tile found via GetAdjacentTiles).</summary>
        public static Vector2Int FindContactCell(TileBase from, TileBase target)
        {
            foreach (Vector2Int cell in FindContactCells(from, target))
                return cell;

            return target.ActivePlacedShape.FirstOrDefault();
        }

        /// <summary>Finds an adjacent tile of the given type owned by someone else — checked across
        /// the tile's whole (still-active) footprint, a hit anywhere on it counts.</summary>
        public static bool TryFindOpposingAdjacent(TileBase tile, TileCardType type, out TileBase found)
        {
            found = null;
            DeckHolder owner = TileOwnership.Get(tile);
            if (tile.TileGrid == null) return false;

            foreach (TileBase adjacent in tile.TileGrid.GetAdjacentTiles(tile))
            {
                if (!adjacent.TileCardType.HasFlag(type)) continue;
                if (TileOwnership.Get(adjacent) == owner) continue;
                found = adjacent;
                return true;
            }
            return false;
        }

        /// <summary>Whether this tile has a same-owner Guard adjacent to it anywhere on its
        /// (still-active) footprint.</summary>
        public static bool IsDefended(TileBase tile)
        {
            DeckHolder owner = TileOwnership.Get(tile);
            if (tile.TileGrid == null) return false;

            foreach (TileBase adjacent in tile.TileGrid.GetAdjacentTiles(tile))
            {
                if (adjacent.TileCardType.HasFlag(TileCardType.Guard) && TileOwnership.Get(adjacent) == owner)
                    return true;
            }
            return false;
        }
    }
}