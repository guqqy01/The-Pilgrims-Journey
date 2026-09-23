using TPJ.Gameplay;
using UnityEngine;
using System.Collections.Generic;
namespace TPJ.UI
{
    public class CardTypeUIHolder : MonoBehaviour
    {
        [SerializeField]
        private CardTypeUI cardTypeUIPrefab;

        private readonly List<CardTypeUI> _cardTypeUIs = new List<CardTypeUI>();

        public void Setup(TileCardType cardType)
        {
            _cardTypeUIs.DestroyAndClearList();

            List<TileCardType> collapseFlags = cardType.CollapseFlags();

            foreach (TileCardType flag in collapseFlags)
            {
                CardTypeUI spawned = Instantiate(cardTypeUIPrefab, transform);
                spawned.Setup(flag);
                _cardTypeUIs.Add(spawned);
            }
        }
    }
}
