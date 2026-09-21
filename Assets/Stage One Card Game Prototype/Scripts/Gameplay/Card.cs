using System;

namespace TPJ.Gameplay
{
    [Serializable]
    public abstract class BaseCard
    {
        public DeckHolder Owner { get; internal set; }
        public bool HasBeenPlayed { get; private set; }

        public abstract IDescriptable GetDisplayInfo();

        public bool TryPlayCard(CardPlayContext context)
        {
            if (HasBeenPlayed)
                return false;

            if (!CanPlay(context))
                return false;

            ProtectedPlayCard(context);
            HasBeenPlayed = true;
            return true;
        }

        public abstract bool HasAnyLegalPlay(ICardLegalityProvider provider);

        public abstract bool CanPlay(CardPlayContext context);
        protected abstract void ProtectedPlayCard(CardPlayContext context);

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

    [Serializable]
    public abstract class Card<T> : BaseCard where T : ICardable
    {
        public T CardableRef { protected set; get; }

        protected Card(T cardableRef)
        {
            CardableRef = cardableRef;
        }

        public override string ToString() => $"{GetType().Name}({CardableRef})";
    }
}