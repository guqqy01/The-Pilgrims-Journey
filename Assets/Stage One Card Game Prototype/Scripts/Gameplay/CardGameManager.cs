using Equipment;
using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using Units;
using System.Linq;
using Monsters;
using System.Xml.Serialization;
namespace TPJ.Gameplay
{

    public class CardGameManager : Singleton<CardGameManager>, ICardLegalityProvider
    {
        [SerializeField]
        private CardPlayerManager _player;
        [SerializeField]
        private EnemyDeckHolderAI _enemyAI;

        [SerializeField] private TileGridBase grid;

        public event Action OnBoardChanged;
        [SerializeField] private int startDrawToHand = 5;
        [SerializeField] private int drawToHandEachTurn = 1;
        [SerializeField] private int startActionPoints = 2;
        [SerializeField] private int actionPointsEachTurn = 2;
        private DeckHolder _currentHolder;
        public bool IsPlayerTurn => _currentHolder == _player;

        [SerializeField]
        private List<EquipmentSO> testStartEquipment;

        [SerializeField]
        private int testAmountOfUnitTilesToCreate;

        [SerializeField]
        private MonsterSpawnSettings testMonsterSpawnSettings;

        [SerializeField]
        private int testAmountOfMonsterTilesToCreate;

        [SerializeField]
        private List<TileSO> testStartTiles;
        [SerializeField]
        private List<UnitSO> testStartUnits;

        public TileGridBase GetGrid => grid;

        public IEnumerable<TileBase> GetTargetableTiles =>
            GetGrid.ListOfTiles;

        private void Start()
        {
            grid.onTileAdded += InvokeOnBoardChanged;
            grid.onTileRemoved += InvokeOnBoardChanged;

            grid.onTileRemoved -= TileOwnership.Remove;
            grid.onTileRemoved += TileOwnership.Remove;

            BeginGame();
        }

        private void InvokeOnBoardChanged(TileBase _)
        {
            OnBoardChanged?.Invoke();
        }

        protected override void CleanUp()
        {
            _player.CleanUp();
            _enemyAI.CleanUp();

            grid.onTileAdded -= InvokeOnBoardChanged;
            grid.onTileRemoved -= InvokeOnBoardChanged;
            grid.onTileRemoved -= TileOwnership.Remove;

            OnBoardChanged = null;

            CardGameEventsBus.CleanUp();
        }

        public bool TryPlayCard(CardBase card, ICardPlayContext context)
        {
            if (_currentHolder == null || !_currentHolder.CurrentlyOurTurn)
                return false;

            if (card == null)
                return false;

            if (card.Owner != _currentHolder)
                return false;

            if (!_currentHolder.TryPlayCard(card, context)) 
                return false;

            return true;
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
            _currentHolder.BeginTurn(drawToHandEachTurn, actionPointsEachTurn);
        }

        private void LockAllTiles()
        {
            foreach(TileBase tile in grid.ListOfTiles)
            {
                tile.Locked = true;
            }
        }

        public void EndTurn()
        {
            _currentHolder.EndTurn();

            LockAllTiles();
            MoveNextTurn();
        }

        public void BeginGame()
        {
            List<ICardable> playerCards = new List<ICardable>();

            for (int i = 0; i < testAmountOfUnitTilesToCreate; i++)
            {
                UnitTile unitTile = new UnitTile(testStartTiles.GetRandom(), testStartUnits.GetRandom());
                playerCards.Add(unitTile);
            }
            playerCards.AddRange(testStartEquipment);
            DeckBuilder.BuildDeck(_player, playerCards);

            List<MonsterTileBase> monsterTileBases = new List<MonsterTileBase>();
            if (MonsterTileManager.HasInstance)
            {
                monsterTileBases = MonsterTileManager.Instance.GetMonsterTiles(testMonsterSpawnSettings, testStartTiles, testAmountOfMonsterTilesToCreate);
            }
            else
            {
                Debug.LogWarning("Need to setup Monster Tile Manager");
            }
            
            DeckBuilder.BuildDeck(_enemyAI, monsterTileBases);

            _player.Init();
            _enemyAI.Init();

            _player.DrawToHand(startDrawToHand);
            _enemyAI.DrawToHand(startDrawToHand);

            _player.CardGamePoints.ModifyPoints(startActionPoints);
            _enemyAI.CardGamePoints.ModifyPoints(startActionPoints);

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