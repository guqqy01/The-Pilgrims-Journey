using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using System.Collections;
namespace TPJ.Gameplay
{
    [Serializable]
    public class CardPile : IEnumerable<CardBase>
    {
        [SerializeReference]
        private List<CardBase> cardPile = new List<CardBase>();
        protected List<CardBase> ProtectedCardPile => cardPile;

        public IReadOnlyList<CardBase> Cards => cardPile;

        public void Clear()
        {
            cardPile.Clear();
        }

        /// <summary>
        /// Removes cards and returns the drawn CardPile. Draws from the top of the pile (Higher Index Values). 
        /// </summary>
        public CardPile Draw(int amountToDraw)
        {
            amountToDraw = Mathf.Min(amountToDraw, cardPile.Count);

            CardPile returnPile = new CardPile();

            for (int i = 0; i < amountToDraw; i++)
            {
                CardBase card = cardPile[cardPile.Count - 1];

                cardPile.Remove(card);
                returnPile.AddCard(card);
            }

            return returnPile;
        }

        public void Copy(CardPile other)
        {
            cardPile.Clear();
            cardPile.AddRange(other);
        }

        public void Copy(List<CardBase> other)
        {
            cardPile.Clear();
            cardPile.AddRange(other);
        }

        public IEnumerator<CardBase> GetEnumerator()
        {
            for (int i = 0; i < cardPile.Count; i++)
            {
                yield return cardPile[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Shuffle()
        {
            int n = cardPile.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);

                CardBase value = cardPile[k];
                cardPile[k] = cardPile[n];
                cardPile[n] = value;
            }
        }

        public void AddCard(CardBase card)
        {
            if (cardPile.Contains(card))
            {
                Debug.LogWarning("Tried adding card twice to pile");
                return;
            }

            cardPile.Add(card);
        }

        public void AddPile(CardPile pile)
        {
            foreach(CardBase card in pile)
            {
                AddCard(card);
            }
        }

        public bool RemoveCard(CardBase card)
        {
            return cardPile.Remove(card);
        }

        public void AddCardAndShuffle(CardBase card)
        {
            AddCard(card);
            Shuffle();
        }
    }
}
