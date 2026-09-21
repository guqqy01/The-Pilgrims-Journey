using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class DeckHolder : MonoBehaviour
    {
        [SerializeField]
        private CardDeck deck;
        [SerializeField]
        private CardHand hand;

        public CardDeck Deck => deck;

        public IReadOnlyList<BaseCard> Hand => hand.Cards;
        public bool CurrentlyOurTurn;

        public event Action<CardPile> OnDraw;
        public event Action<BaseCard> OnDiscard;
        public event Action<BaseCard> OnPlay;

        public void Init()
        {
            deck.Reset();
        }

        public void AddCardToDeck(BaseCard card)
        {
            card.Owner = this;
            deck.AddCardToDeck(card);
        }

        public void RemoveCardFromDeck(BaseCard card)
        {
            deck.RemoveCardFromDeck(card);
        }

        public void DrawToHand(int amount)
        {
            CardPile drawn = deck.DrawFromActive(amount);

            // Reset play state for all drawn cards so they can be played this turn
            foreach (BaseCard card in drawn)
                card.ResetPlayState();

            hand.AddPile(drawn);

            OnDraw?.Invoke(drawn);
        }

        public bool PlayCard(BaseCard card, CardPlayContext context)
        {
            if (!hand.TryPlayCard(card, context))
                return false;

            OnPlay?.Invoke(card);

            deck.DiscardCard(card);
            OnDiscard?.Invoke(card);
            return true;
        }

        public virtual void CleanUp()
        {
            OnDraw = null;
            OnPlay = null;
            OnDiscard = null;
        }
    }
}