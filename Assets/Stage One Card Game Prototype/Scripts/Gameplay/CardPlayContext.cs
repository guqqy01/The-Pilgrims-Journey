using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    /// <summary>
    /// Tile-placement context: grid + position (+ optional rotation for the hologram's
    /// current orientation), OR equipment-target context: the tile being equipped.
    /// Never both at once.
    /// </summary>
    public class CardPlayContext
    {
        public readonly TileGridBase Grid;
        public readonly Vector2Int GridPosition;
        public readonly float Rotation;

        public readonly TileBase TargetTile;

        public CardPlayContext(TileGridBase grid, Vector2Int gridPosition, float rotation = 0f)
        {
            Grid = grid;
            GridPosition = gridPosition;
            Rotation = rotation;
        }

        public CardPlayContext(TileBase targetTile)
        {
            TargetTile = targetTile;
        }
    }
}