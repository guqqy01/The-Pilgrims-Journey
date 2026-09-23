using System;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class DeckResourceEventHandler : MonoBehaviour
    {
        [SerializeField]
        private DeckHolder targetDeckHolder;

        [SerializeField]
        private TileGridBase tileGrid;


    }

    public class DeckHolder : MonoBehaviour, ICardPointHolder
    {
        [SerializeField]
        private CardDeck deck;
        [SerializeField]
        private CardHand hand;

        public CardDeck Deck => deck;

        public IReadOnlyList<CardBase> Hand => hand.Cards;

        public List<CardBase> AbleToPlayCards => hand.Cards.Where(x => CardHasAnyLegalMove(x)).ToList();

        [SerializeField]
        private CardGamePoints _cardGamePoints;
        public CardGamePoints CardGamePoints => _cardGamePoints;

        public bool CurrentlyOurTurn { private set; get; }

        public event Action<CardPile> OnDraw;
        public event Action<CardBase> OnDiscard;
        public event Action<CardBase> OnPlay;

        public void Init()
        {
            deck.Reset();
        }

        public virtual void BeginTurn(int baseAmountOfCardToDraw, int baseAmountOfActionPoints)
        {
            CurrentlyOurTurn = true;
            DrawToHand(baseAmountOfCardToDraw);
            CardGameEventsBus.RaiseOnBeginTurn(this);
        }

        public virtual void EndTurn()
        {
            CurrentlyOurTurn = false;
            CardGameEventsBus.RaiseOnEndTurn(this);
        }

        public void AddCardToDeck(CardBase card)
        {
            card.Owner = this;
            deck.AddCardToDeck(card);
        }

        public void RemoveCardFromDeck(CardBase card)
        {
            deck.RemoveCardFromDeck(card);
        }

        public void DrawToHand(int amount)
        {
            CardPile drawn = deck.DrawFromActive(amount);

            // Reset play state for all drawn cards so they can be played this turn
            foreach (CardBase card in drawn)
                card.ResetPlayState();

            hand.AddPile(drawn);

            OnDraw?.Invoke(drawn);
        }

        public bool CardHasAnyLegalMove(CardBase card)
        {
            if(card.Owner != this)
            {
                Debug.Log("Can not play card because does not belong to this Deck Holder.");
                return false;
            }

            bool hasEnoughPoints = CardGamePoints.HasEnoughPoints(card.CardActionCost);
            bool legal = CurrentlyOurTurn && hasEnoughPoints && card.HasAnyLegalPlay(CardGameManager.Instance);
            return legal;
        }

        public bool TryPlayCard(CardBase card, ICardPlayContext context)
        {
            if (!_cardGamePoints.HasEnoughPoints(card.CardActionCost))
                return false;

            if (!hand.TryPlayCard(card, context))
                return false;

            _cardGamePoints.ModifyPoints(-card.CardActionCost);

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