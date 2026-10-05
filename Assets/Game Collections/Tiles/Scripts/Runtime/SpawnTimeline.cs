using System;
using System.Collections.Generic;
using System.Linq;

namespace Tiles
{
    /// <summary>
    /// Phase-two spawn schedule, built from the finished card-game board.
    /// Owns its own cloned tiles, so the card game can reset freely afterwards.
    /// </summary>
    public class SpawnTimeline
    {
        private readonly List<TimelineEntry> entries = new();
        public IReadOnlyList<TimelineEntry> Entries => entries;
        public IEnumerable<TileBase> Tiles => entries.Select(e => e.Tile);

        /// <summary>Last occupied slot + 1 (0 when empty).</summary>
        public int Length => entries.Count == 0 ? 0 : entries.Max(e => e.LastSlot) + 1;

        public event Action<TileBase> TileAdded;
        public event Action<TileBase> TileRemoved;
        public event Action Changed;

        public void LoadFromGrid(TileGridBase source)
        {
            Clear();
            if (source == null)
                return;

            // Count cells per column straight off the grid array. This is exactly what the
            // old phase-two grid did, and it ignores cells destroyed during phase one.
            var cellsByTile = new Dictionary<TileBase, Dictionary<int, int>>();
            TileBase[,] cells = source.Tiles;

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    TileBase tile = cells[x, y];
                    if (tile is not (UnitTile or MonsterTileBase))
                        continue; // skips null and HeroTile

                    if (!cellsByTile.TryGetValue(tile, out var bySlot))
                        cellsByTile[tile] = bySlot = new Dictionary<int, int>();

                    bySlot[x] = bySlot.GetValueOrDefault(x) + 1;
                }
            }

            foreach (var (src, bySlot) in cellsByTile)
            {
                TileBase clone = TileCloner.Clone(src);
                if (clone == null)
                    continue;

                clone.ActionDestroyTile += HandleTileDestroyed;
                entries.Add(new TimelineEntry(clone, bySlot));
                TileAdded?.Invoke(clone);
            }

            Changed?.Invoke();
        }

        public void Clear()
        {
            List<TileBase> all = entries.Select(e => e.Tile).ToList();
            foreach (TileBase tile in all)
                RemoveEntry(tile);

            if (all.Count > 0)
                Changed?.Invoke();
        }

        public bool Remove(TileBase tile)
        {
            if (!RemoveEntry(tile))
                return false;

            Changed?.Invoke();
            return true;
        }

        private bool RemoveEntry(TileBase tile)
        {
            int index = entries.FindIndex(e => e.Tile == tile);
            if (index < 0)
                return false;

            entries.RemoveAt(index);
            tile.ActionDestroyTile -= HandleTileDestroyed;
            tile.OnDestroy();
            TileRemoved?.Invoke(tile);
            return true;
        }

        // Fired by UnitTile.OnUnitDie when a tile is spent (replaces TileGridBase.DestroyTile).
        private void HandleTileDestroyed(TileBase tile) => Remove(tile);

        public Dictionary<UnitTile, int> GetUnitTilesAt(int slot)
        {
            var result = new Dictionary<UnitTile, int>();
            foreach (TimelineEntry e in entries)
            {
                if (e.Tile is UnitTile unit && e.CellsAt(slot) is int c && c > 0)
                    result[unit] = c;
            }
            return result;
        }

        public Dictionary<MonsterTileBase, int> GetMonsterTilesAt(int slot)
        {
            var result = new Dictionary<MonsterTileBase, int>();
            foreach (TimelineEntry e in entries)
            {
                if (e.Tile is MonsterTileBase monster && e.CellsAt(slot) is int c && c > 0)
                    result[monster] = c;
            }
            return result;
        }

        /// <summary>True if any tile still has cells in a slot after this one.</summary>
        public bool HasTilesAfter(int slot) => entries.Any(e => e.LastSlot > slot);
    }
}