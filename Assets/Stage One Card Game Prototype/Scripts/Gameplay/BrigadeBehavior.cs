using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class BrigadeBehavior : TileCardTypeBehavior
    {
        public override TileCardType HandledType => TileCardType.Brigade;

        public override void OnPlaced(DeckHolder owner, TileBase tile)
        {
            // Held until the destroy-target choice resolves at end of turn.
            tile.Locked = true;

            if (owner is CardPlayerManager)
            {
                // Hand off to the UI: BrigadeTargetHighlighter highlights adjacent tiles and
                // reports the chosen one back via TileCardTypeEffectManager.SetBrigadeTarget.
                CardGameEventsBus.RaiseOnBrigadePlaced(tile);
            }
            else
            {
                ChooseTargetAutomatically(owner, tile);
            }
        }

        private static void ChooseTargetAutomatically(DeckHolder owner, TileBase tile)
        {
            if (tile.TileGrid == null)
                return;

            List<TileBase> adjacent = tile.TileGrid.GetAdjacentTiles(tile);
            TileBase target = adjacent.FirstOrDefault(t => TileOwnership.Get(t) != owner)
                            ?? adjacent.FirstOrDefault();

            if (target == null)
            {
                TileCardTypeEffectManager.CancelBrigadeChoice(tile);
                tile.Locked = false;
                return;
            }

            Vector2Int cell = TileCardTypeAdjacency.FindContactCell(tile, target);
            TileCardTypeEffectManager.SetBrigadeTarget(tile, target, cell);
        }
    }
}