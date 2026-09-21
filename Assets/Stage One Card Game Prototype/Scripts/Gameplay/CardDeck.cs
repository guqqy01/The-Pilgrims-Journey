using System;
using UnityEngine;
namespace TPJ.Gameplay
{
    [Serializable]
    public class CardDeck
    {
        [SerializeField]
        public CardPile WholeDeck = new CardPile();
        [SerializeField]
        public CardPile ActivePile = new CardPile();
        [SerializeField]
        public CardPile DiscardPile = new CardPile();
        public void Reset()
        {
            ActivePile.Copy(WholeDeck);
            ActivePile.Shuffle();
        }

        public void AddCardToDeck(BaseCard card)
        {
            WholeDeck.AddCard(card);
        }

        public void RemoveCardFromDeck(BaseCard card)
        {
            WholeDeck.RemoveCard(card);
            ActivePile.RemoveCard(card);
            DiscardPile.RemoveCard(card);
        }

        public CardPile DrawFromActive(int amountToDraw)
        {
            return ActivePile.Draw(amountToDraw);
        }

        public void AddPileToDiscard(CardPile pile)
        {
            DiscardPile.AddPile(pile);
        }

        public void DiscardCard(BaseCard card)
        {
            DiscardPile.AddCard(card);
        }
    }
}
