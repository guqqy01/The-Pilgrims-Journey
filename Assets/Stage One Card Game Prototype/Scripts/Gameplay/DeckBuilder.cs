using System.Collections.Generic;

namespace TPJ.Gameplay
{
    /// <summary>
    /// Generates cards directly from your tile/equipment content and loads them into a deck.
    /// </summary>
    public static class DeckBuilder
    {
        public static void BuildDeck(DeckHolder holder, IEnumerable<ICardable> sources)
        {
            foreach (ICardable source in sources)
            {
                holder.AddCardToDeck(source.CreateCard());
            }
        }
    }
}