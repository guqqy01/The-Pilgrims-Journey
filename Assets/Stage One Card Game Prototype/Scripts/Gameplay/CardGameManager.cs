using Equipment;
using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using Units;
using System.Linq;
using Monsters;
using Heros;
using Attacks;
namespace TPJ.Gameplay
{
    public class CardGameManager : Singleton<CardGameManager>, ICardLegalityProvider
    {
        [SerializeField]
        private CardPlayerManager _player;
        public CardPlayerManager PlayerDeckHolder => _player;
        [SerializeField]
        private EnemyDeckHolderAI _enemyAI;
        public EnemyDeckHolderAI EnemyDeckHolder => _enemyAI;
        [SerializeField] private TileGridBase grid;

        public event Action OnBoardChanged;
        [SerializeField] private int startDrawToHand = 5;
        [SerializeField] private int drawToHandEachTurn = 1;
        [SerializeField] private int startActionPoints = 2;
        [SerializeField] private int actionPointsEachTurn = 2;

        [SerializeField]
        private int amountOfTurnsForEndGame = 10;
        public int AmountOfTurnsForEndGame => amountOfTurnsForEndGame;

        private int _currentAmountOfTurnsTaken;
        public int CurrentAmountOfTurnsTaken => _currentAmountOfTurnsTaken;

        public int AmountOfTurnsTillEndGame => AmountOfTurnsForEndGame - CurrentAmountOfTurnsTaken;
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

        [SerializeField]
        private List<BaseAttackSO> testAttacks;

        [SerializeField]
        private HeroSO testHero;
        [SerializeField]
        private TileSO testHeroTile;
        public TileGridBase GetGrid => grid;

        private bool _gameEnded;
        private bool _inHeroSetupTurn;


        public IEnumerable<TileBase> GetTargetableTiles =>
            GetGrid.ListOfTiles;

        private void Start()
        {
            grid.onTileAdded += InvokeOnBoardChanged;
            grid.onTileRemoved += InvokeOnBoardChanged;
            grid.onTileSectionDestroyed -= InvokeOnBoardChanged;
            grid.onTileSectionDestroyed += InvokeOnBoardChanged;

            grid.onTileRemoved -= TileOwnership.Remove;
            grid.onTileRemoved += TileOwnership.Remove;

            CardGameEventsBus.OnCardPlayed += OnAnyCardPlayed;   // add

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
            grid.onTileSectionDestroyed -= InvokeOnBoardChanged;
            grid.onTileRemoved -= TileOwnership.Remove;

            CardGameEventsBus.OnCardPlayed -= OnAnyCardPlayed;   // add

            OnBoardChanged = null;

            CardGameEventsBus.CleanUp();
        }
        public bool CanEndTurn => !_gameEnded && _currentHolder != null && !_inHeroSetupTurn;
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
            if (_gameEnded || _inHeroSetupTurn)
                return;

            if (_currentHolder == null)
            {
                SelectBeginningPlayer();
            }
            else if (_currentHolder == _player)
            {
                _currentHolder = _enemyAI;
            }
            else if (_currentHolder == _enemyAI)
            {
                _currentHolder = _player;
                _currentAmountOfTurnsTaken++;
            }
            else
            {
                Debug.LogWarning("current holder not set to either player/enemy or null.");
                SelectBeginningPlayer();
            }

            if (_currentAmountOfTurnsTaken >= amountOfTurnsForEndGame)
            {
                EndGame(CardGameEndReason.TurnLimitReached);
                return;
            }

            if (IsStalemate())
            {
                EndGame(CardGameEndReason.Stalemate);
                return;
            }

            BeginTurn(_currentHolder);
        }

        private bool IsStalemate()
        {
            bool playerStuck = !_player.HasCardsLeftToDraw && !_player.HasAnyStructurallyPlayableCard(this);
            bool enemyStuck = !_enemyAI.HasCardsLeftToDraw && !_enemyAI.HasAnyStructurallyPlayableCard(this);
            return playerStuck && enemyStuck;
        }

        private void SelectBeginningPlayer()
        {
            _currentHolder = _player;
        }

        public void BeginTurn(DeckHolder holder)
        {
            if (holder == null)
                return;

            holder.BeginTurn(drawToHandEachTurn, actionPointsEachTurn);
        }

        private void LockAllTiles()
        {
            foreach (TileBase tile in grid.ListOfTiles)
            {
                tile.Locked = true;
            }
        }

        public void EndTurn()
        {
            if (!CanEndTurn)
                return;

            _currentHolder.EndTurn();

            LockAllTiles();
            MoveNextTurn();
        }

        public void BeginGame()
        {
            _gameEnded = false;
            _currentAmountOfTurnsTaken = 0;
            _currentHolder = null;

            List<ICardable> playerCards = new List<ICardable>();

            for (int i = 0; i < testAmountOfUnitTilesToCreate; i++)
            {
                UnitTile unitTile = new UnitTile(testStartTiles.GetRandom(), testStartUnits.GetRandom());
                playerCards.Add(unitTile);
            }
            playerCards.AddRange(testStartEquipment);
            playerCards.AddRange(testAttacks);
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

            // Start-of-game draws/points are deferred to CompleteHeroSetupTurn now, so the
            // player's hand contains ONLY the hero card until it's placed.

            InputManager.Instance.Actions.UI_TilePlayer.Enable();

            CardGameEventsBus.RaiseOnGameBegin();

            BeginHeroSetupTurn();
        }

        private void BeginHeroSetupTurn()
        {
            _inHeroSetupTurn = true;

            HeroTile heroTile = new HeroTile(testHero, testHeroTile);
            HeroCard heroCard = new HeroCard(heroTile);

            _currentHolder = _player;
            _player.SetTurnActive(true);              // order matters: must be true before
            _player.GiveCardDirectlyToHand(heroCard);  // the card widget's legality check runs
        }

        private void OnAnyCardPlayed(DeckHolder holder, CardBase card)
        {
            if (_inHeroSetupTurn && card is HeroCard)
                CompleteHeroSetupTurn();
        }

        private void CompleteHeroSetupTurn()
        {
            _inHeroSetupTurn = false;

            _player.EndTurn();
            LockAllTiles();

            // Now grant the normal start-of-game draws/points, deferred from BeginGame.
            _player.DrawToHand(startDrawToHand);
            _enemyAI.DrawToHand(startDrawToHand);
            _player.CardGamePoints.ModifyPoints(startActionPoints);
            _enemyAI.CardGamePoints.ModifyPoints(startActionPoints);

            _currentHolder = null;
            MoveNextTurn();
        }

        public void EndGame(CardGameEndReason reason = CardGameEndReason.TurnLimitReached)
        {
            if (_gameEnded)
                return;

            _gameEnded = true;
            _currentHolder = null;

            InputManager.Instance.Actions.UI_TilePlayer.Disable();

            CardGameEventsBus.RaiseOnGameEnd(reason);
        }

        public TileGridBase GetGridFor(DeckHolder holder)
        {
            return GetGrid;
        }

        IEnumerable<TileBase> ICardLegalityProvider.GetTargetableTiles(DeckHolder holder)
        {
            if (holder == _player)
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