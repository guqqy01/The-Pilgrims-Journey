using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class FarmerBehavior : TileCardTypeBehavior
    {
        public override TileCardType HandledType => TileCardType.Farmer;

        public override void OnTurnBegin(TileBase tile)
        {
            DeckHolder owner = TileOwnership.Get(tile);
            if (owner == null) return;

            // Generates 1 point per still-intact cell, decided independently per cell: a
            // raided section is stolen, everything else still pays out. A partially raided
            // multi-cell tile is no longer all-or-nothing.
            foreach (Vector2Int cell in tile.ActivePlacedShape)
            {
                if (!TileCardTypeAdjacency.IsCellDefended(tile, cell)
                    && TileCardTypeAdjacency.TryFindOpposingAdjacentToCell(tile, cell, TileCardType.Bandit, out TileBase bandit))
                {
                    TileOwnership.Get(bandit)?.CardGamePoints.ModifyPoints(1);
                }
                else
                {
                    owner.CardGamePoints.ModifyPoints(1);
                }
            }
        }
    }
}