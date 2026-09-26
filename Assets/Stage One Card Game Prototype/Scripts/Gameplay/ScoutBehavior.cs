using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class ScoutBehavior : TileCardTypeBehavior
    {
        public override TileCardType HandledType => TileCardType.Scout;

        public override void OnTurnBegin(TileBase tile)
        {
            DeckHolder owner = TileOwnership.Get(tile);
            if (owner == null) return;

            // Same per-cell split as Farmer/Bandit: each still-intact cell draws 1 card,
            // independently stolen by an adjacent undefended Spy or not.
            foreach (Vector2Int cell in tile.ActivePlacedShape)
            {
                if (!TileCardTypeAdjacency.IsCellDefended(tile, cell)
                    && TileCardTypeAdjacency.TryFindOpposingAdjacentToCell(tile, cell, TileCardType.Spy, out TileBase spy))
                {
                    TileOwnership.Get(spy)?.DrawToHand(1);
                }
                else
                {
                    owner.DrawToHand(1);
                }
            }
        }
    }
}