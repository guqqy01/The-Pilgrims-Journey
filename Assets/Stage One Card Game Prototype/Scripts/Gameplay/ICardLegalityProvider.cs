// ICardLegalityProvider.cs — new
using System.Collections.Generic;
using Tiles;

namespace TPJ.Gameplay
{
    public interface ICardLegalityProvider
    {
        TileGridBase GetGridFor(DeckHolder holder);
        IEnumerable<TileBase> GetTargetableTiles(DeckHolder holder);
    }
}