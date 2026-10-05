using System;

namespace TPJ.Gameplay
{
    [Serializable]
    public abstract class CardBase
    {
        public DeckHolder Owner { get; internal set; }
        public bool HasBeenPlayed { get; protected set; }
        public int CardActionCost { get; internal set; }

        /// <summary>Whether the card is moved to the discard pile after being played. Cards that
        /// stay on the board (e.g. tile cards) override this to false.</summary>
        public virtual bool GoesToDiscardWhenPlayed => true;

        public abstract bool TryPlayCard<TPlayContext>(TPlayContext context) where TPlayContext : ICardPlayContext;
        public abstract bool HasAnyLegalPlay(ICardLegalityProvider provider);
        public abstract IDescriptable GetDisplayInfo();

        /// <summary>
        /// Resets the card's play state. Call this when a card is recycled back into the deck
        /// after being discarded or reshuffled.
        /// </summary>
        public void ResetPlayState()
        {
            HasBeenPlayed = false;
        }

        public override string ToString() => GetType().Name;
    }
}