using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
namespace TPJ.Gameplay
{
    [Serializable]
    public class EnemyDeckHolderAI : DeckHolder
    {
        public override void BeginTurn(int baseAmountOfCardToDraw, int baseAmountOfActionPoints)
        {
            base.BeginTurn(baseAmountOfCardToDraw, baseAmountOfActionPoints);

            int breaker = 20;
            while(CardGamePoints.CurrentPoints > 0 && breaker > 0)
            {
                List<CardBase> cards = AbleToPlayCards;

                if (cards.Count == 0)
                    break;

                CardBase card = cards.GetRandom();

                if(card is MonsterCard monsterCard)
                {
                    MonsterTileBase tileBase = monsterCard.CardableRef;

                    TileGridBase gridBase = CardGameManager.Instance.GetGrid;

                    if (gridBase.TryGetRandomFreePosition(tileBase, out Vector2Int position, out float rotation))
                    {
                        TileCardPlayContext playContext = new TileCardPlayContext(gridBase, position, rotation);
                        CardGameManager.Instance.TryPlayCard(card, playContext);
                    }
                }

                breaker--;
            }

            CardGameManager.IfReady(x => x.EndTurn());
        }
    }
}
