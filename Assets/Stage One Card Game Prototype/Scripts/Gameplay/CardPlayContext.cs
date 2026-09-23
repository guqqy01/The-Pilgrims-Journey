using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class TileCardPlayContext : ICardPlayContext
    {
        public readonly TileGridBase Grid;
        public readonly Vector2Int GridPosition;
        public readonly float Rotation;

        public TileCardPlayContext(TileGridBase grid, Vector2Int gridPosition, float rotation = 0f)
        {
            Grid = grid;
            GridPosition = gridPosition;
            Rotation = rotation;
        }
    }

    public class EquipmentCardPlayContext : ICardPlayContext
    {
        public readonly TileBase TargetTile;

        public EquipmentCardPlayContext(TileBase targetTile)
        {
            TargetTile = targetTile;
        }
    }

    public interface ICardPlayContext
    {

    }
}