using TPJ.Gameplay;
using Tiles;
using UnityEngine;
using System.Collections.Generic;

namespace TPJ.UI
{
    public class CardTypeUIHolder : MonoBehaviour
    {
        [SerializeField]
        private CardTypeUI cardTypeUIPrefab;

        private readonly List<CardTypeUI> _cardTypeUIs = new List<CardTypeUI>();

        private TileBase _boundTile;

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

        /// <summary>
        /// Use for placed-tile UI instead of a raw Setup() call — keeps the icon list
        /// current if equipment later grants the tile a new TileCardType.
        /// </summary>
        public void Bind(TileBase tile)
        {
            if (_boundTile != null)
                _boundTile.OnTileCardTypeChanged -= Setup;

            _boundTile = tile;

            if (_boundTile != null)
            {
                _boundTile.OnTileCardTypeChanged += Setup;
                Setup(_boundTile.TileCardType);
            }
            else
            {
                Setup(default);
            }
        }

        private void OnDestroy()
        {
            if (_boundTile != null)
                _boundTile.OnTileCardTypeChanged -= Setup;
        }
    }
}