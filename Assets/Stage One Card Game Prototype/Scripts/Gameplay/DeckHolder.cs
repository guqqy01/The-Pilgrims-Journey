using System;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
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
            int amountToDraw = Mathf.Min(amount, hand.handLimit - hand.Count());

            CardPile drawn = deck.DrawFromActive(amountToDraw);

            // Reset play state for all drawn cards so they can be played this turn
            foreach (CardBase card in drawn)
                card.ResetPlayState();

            hand.AddPile(drawn);

            OnDraw?.Invoke(drawn);
        }

        // DeckHolder.cs — add alongside AbleToPlayCards

        /// <summary>
        /// Whether any card in hand could structurally ever be played (ignores whose turn it is
        /// and ignores current points, since points regenerate — only placement/targeting legality
        /// makes a card permanently unplayable). Used to detect a stalemate.
        /// </summary>
        public bool HasAnyStructurallyPlayableCard(ICardLegalityProvider provider)
        {
            foreach (CardBase card in hand.Cards)
            {
                if (card.HasAnyLegalPlay(provider))
                    return true;
            }
            return false;
        }

        public void GiveCardDirectlyToHand(CardBase card)
        {
            card.Owner = this;
            card.ResetPlayState();

            CardPile singleCardPile = new CardPile();
            singleCardPile.AddCard(card);

            hand.AddPile(singleCardPile);
            OnDraw?.Invoke(singleCardPile);
        }

        public void DrawAllCardsOfType<TCard>() where TCard : CardBase
        {
            List<CardBase> cardsToDraw = deck.ActivePile.Cards.ToList();
            foreach (CardBase cardBase in cardsToDraw)
            {
                if (cardBase is TCard)
                {
                    deck.ActivePile.RemoveCard(cardBase);
                    GiveCardDirectlyToHand(cardBase);
                }
            }
        }

        public void SetTurnActive(bool active) => CurrentlyOurTurn = active;

        /// <summary>Whether this holder could still draw a new card this game.</summary>
        public bool HasCardsLeftToDraw => deck.ActivePile.Cards.Count > 0;

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

            OnPlay?.Invoke(card);

            // Tile cards stay on the board once played, so they don't go to the discard pile
            // (otherwise they could be redrawn). The OnDiscard event is still raised so the
            // hand UI removes the played card's widget.
            if (card.GoesToDiscardWhenPlayed)
                deck.DiscardCard(card);

            OnDiscard?.Invoke(card);
            return true;
        }

        public virtual void ResetForNewGame()
        {
            // Raise OnDiscard per card so any hand UI removes its widgets.
            foreach (CardBase card in hand.Cards.ToList())
                OnDiscard?.Invoke(card);

            hand.Clear();
            deck.ClearAll();
            _cardGamePoints.Reset();
            CurrentlyOurTurn = false;
        }

        /// <summary>Moves a card out of the hand into the discard pile without playing it.</summary>
        public void DiscardFromHand(CardBase card)
        {
            if (!hand.RemoveCard(card))
                return;

            deck.DiscardCard(card);
            OnDiscard?.Invoke(card);
        }

        public virtual void CleanUp()
        {
            OnDraw = null;
            OnPlay = null;
            OnDiscard = null;
        }
    }
}