using Attacks;
using Heros;
using System;
using System.Collections.Generic;
using Tiles;
using Units;
using UnityEngine;
namespace TPJ.Gameplay
{
    [Serializable]
    public class CardPlayerManager : DeckHolder
    {
        private static readonly SingletonHelper<CardPlayerManager> _singleton = new();
        public static CardPlayerManager Instance => _singleton.Instance;
        public static bool HasInstance => _singleton.HasInstance;
        public static void WhenReady(Action<CardPlayerManager> onReady) => _singleton.WhenReady(onReady);
        public static void IfReady(Action<CardPlayerManager> ifReady) => _singleton.IfReady(ifReady);

        [SerializeField]
        private List<UnitSO> units;

        [SerializeField]
        private int amountOfCards = 15;

        public void InitiateFromRootAtAwake()
        {
            if (!_singleton.TryClaim(this, () => Destroy(gameObject)))
                return;
        }

        public HeroCard HeroCard { private set; get; }

        public void SetHeroCard(HeroSO heroSO)
        {
            HeroTile heroTile = new HeroTile(heroSO, TileManager.Instance.HeroTiles.GetRandom());
            HeroCard = new HeroCard(heroTile);

            SetupDeck();
        }

        public override void ResetForNewGame()
        {
            base.ResetForNewGame();
            HeroCard = null;
        }

        private void SetupDeck()
        {
            List<ICardable> unitTiles = new List<ICardable>();

            for (int i = 0; i < amountOfCards; i++)
            {
                UnitSO unitSO = units.GetRandom();
                UnitTile unitTile = new UnitTile(TileManager.Instance.UnitTiles.GetRandom(), unitSO);
                unitTiles.Add(unitTile);
            }

            DeckBuilder.BuildDeck(this, unitTiles);
        }
    }
}
