using Equipment;
using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using Units;
using System.Linq;
namespace TPJ.Gameplay
{
    public class CardGameManager : Singleton<CardGameManager>, ICardLegalityProvider
    {
        [SerializeField]
        private CardPlayerManager _player;
        [SerializeField]
        private EnemyDeckHolderAI _enemyAI;

        [SerializeField] private TileGridBase grid;

        public event Action<DeckHolder> OnTurnChanged;
        public event Action OnBoardChanged;

        [SerializeField] private int startDrawToHand = 5;
        [SerializeField] private int drawToHandEachTurn = 1;

        private DeckHolder _currentHolder;
        public bool IsPlayerTurn => _currentHolder == (DeckHolder)_player;

        [SerializeField]
        private List<EquipmentSO> testStartEquipment;

        [SerializeField]
        private int testAmountOfTilesToCreate;

        [SerializeField]
        private List<TileSO> testStartTiles;
        [SerializeField]
        private List<UnitSO> testStartUnits;

        public TileGridBase GetGrid => grid;

        public IEnumerable<TileBase> GetTargetableTiles =>
            GetGrid.ListOfTiles;

        private void Start()
        {
            grid.onTileAdded += _ => OnBoardChanged?.Invoke();
            grid.onTileRemoved += _ => OnBoardChanged?.Invoke();

            grid.onTileRemoved -= TileOwnership.Remove;
            grid.onTileRemoved += TileOwnership.Remove;

            BeginGame();
        }

        protected override void CleanUp()
        {
            _player.CleanUp();
            _enemyAI.CleanUp();

            grid.onTileRemoved -= TileOwnership.Remove;

            OnTurnChanged = null;
            OnBoardChanged = null;
        }

        public bool TryPlayCard(BaseCard card, CardPlayContext context)
        {
            if (_currentHolder == null || !_currentHolder.CurrentlyOurTurn)
                return false;

            if (card == null)
                return false;

            if (card.Owner != _currentHolder)
                return false;

            return _currentHolder.PlayCard(card, context);
        }

        public void MoveNextTurn()
        {
            if (_currentHolder == null)
                SelectBeginningPlayer();
            else if (_currentHolder == _player)
                _currentHolder = _enemyAI;
            else if (_currentHolder == _enemyAI)
                _currentHolder = _player;
            else
            {
                Debug.LogWarning("current holder not set to either player/enemy or null.");
                SelectBeginningPlayer();
            }

            BeginTurn(_currentHolder);
        }

        private void SelectBeginningPlayer()
        {
            _currentHolder = _player;
        }

        public void BeginTurn(DeckHolder holder)
        {
            holder.CurrentlyOurTurn = true;
            holder.DrawToHand(drawToHandEachTurn);
            OnTurnChanged?.Invoke(holder);
        }

        public void EndTurn()
        {
            _currentHolder.CurrentlyOurTurn = false;
            MoveNextTurn();
        }

        public void BeginGame()
        {
            List<ICardable> cardables = new List<ICardable>();

            for (int i = 0; i < testAmountOfTilesToCreate; i++)
            {
                UnitTile unitTile = new UnitTile(testStartTiles.GetRandom(), testStartUnits.GetRandom());
                cardables.Add(unitTile);
            }
            cardables.AddRange(testStartEquipment);
            DeckBuilder.BuildDeck(_player, cardables);
            //DeckBuilder.BuildDeck(_enemyAI, testStartEquipment);

            _player.Init();
            _enemyAI.Init();

            _player.DrawToHand(startDrawToHand);
            _enemyAI.DrawToHand(startDrawToHand);

            InputManager.Instance.Actions.UI_TilePlayer.Enable();

            MoveNextTurn();
        }

        public void EndGame()
        {
            InputManager.Instance.Actions.UI_TilePlayer.Disable();
        }

        public TileGridBase GetGridFor(DeckHolder holder)
        {
            return GetGrid;
        }

        IEnumerable<TileBase> ICardLegalityProvider.GetTargetableTiles(DeckHolder holder)
        {
            if(holder == _player)
            {
                return GetTargetableTiles.Where((t) => t is UnitTile || t is HeroTile);
            }
            else if (holder == _enemyAI)
            {
                return GetTargetableTiles.Where((t) => t is MonsterTileBase);
            }
            else
            {
                return new List<TileBase>();
            }

        }
    }
}