using System.Collections.Generic;
using System.Linq;

namespace Tiles
{
    /// <summary>One tile on the timeline and how many of its cells sit in each slot (column).</summary>
    public sealed class TimelineEntry
    {
        public TileBase Tile { get; }
        public int FirstSlot { get; }
        public int LastSlot { get; }

        private readonly Dictionary<int, int> cellsBySlot;
        public IReadOnlyDictionary<int, int> CellsBySlot => cellsBySlot;

        public TimelineEntry(TileBase tile, Dictionary<int, int> cellsBySlot)
        {
            Tile = tile;
            this.cellsBySlot = cellsBySlot;
            FirstSlot = cellsBySlot.Keys.Min();
            LastSlot = cellsBySlot.Keys.Max();
        }

        public int CellsAt(int slot) => cellsBySlot.TryGetValue(slot, out int c) ? c : 0;
    }
}