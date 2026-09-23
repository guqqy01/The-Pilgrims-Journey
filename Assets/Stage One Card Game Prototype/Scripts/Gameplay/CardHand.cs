using System;
using UnityEngine;

namespace TPJ.Gameplay
{
    [Serializable]
    public class CardHand : CardPile
    {
        public bool TryPlayCard(CardBase card, ICardPlayContext context)
        {
            if (!ProtectedCardPile.Contains(card))
            {
                Debug.LogWarning($"Tried playing the card: {card}, which is not part of the hand.");
                return false;
            }

            if (!card.TryPlayCard(context))
                return false;
            ProtectedCardPile.Remove(card);
            return true;
        }
    }
}