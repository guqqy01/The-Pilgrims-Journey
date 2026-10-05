using System;
using System.Collections.Generic;
using System.Linq;
using Attacks;
using Tiles;
using UnityEngine;

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

        public TileGridBase Grid => grid;

        private bool _gameEnded;
        private bool _inHeroSetupTurn;
        private bool _heroTilePlaced;
        private bool _attackPlacedOnHero;


        public IEnumerable<TileBase> GetTargetableTiles => Grid.ListOfTiles;

        private void Start()
        {
            grid.onTileAdded += InvokeOnBoardChanged;
            grid.onTileRemoved += InvokeOnBoardChanged;
            grid.onTileSectionDestroyed += InvokeOnBoardChanged;
            grid.onTileRemoved += TileOwnership.Remove;

            CardGameEventsBus.OnTileCardPlayed += HandleTileCardPlayed;
            CardGameEventsBus.OnAttackCardPlayed += HandleAttackCardPlayed;
        }

        private void InvokeOnBoardChanged(TileBase _)
        {
            OnBoardChanged?.Invoke();
        }

        public void ResetForNewGame()
        {
            _gameEnded = false;
            _currentAmountOfTurnsTaken = 0;
            _currentHolder = null;
            _inHeroSetupTurn = false;
            _heroTilePlaced = false;
            _attackPlacedOnHero = false;

            grid.Clear();           // fires onTileRemoved -> TileOwnership.Remove
            TileOwnership.Clear();  // safety net

            _player.ResetForNewGame();
            _enemyAI.ResetForNewGame();
        }

        protected override void CleanUp()
        {
            _player.CleanUp();
            _enemyAI.CleanUp();

            grid.onTileAdded -= InvokeOnBoardChanged;
            grid.onTileRemoved -= InvokeOnBoardChanged;
            grid.onTileSectionDestroyed -= InvokeOnBoardChanged;
            grid.onTileRemoved -= TileOwnership.Remove;

            CardGameEventsBus.OnTileCardPlayed -= HandleTileCardPlayed;
            CardGameEventsBus.OnAttackCardPlayed -= HandleAttackCardPlayed;

            OnBoardChanged = null;

            CardGameEventsBus.CleanUp();
        }

        private void HandleTileCardPlayed(DeckHolder holder, TileBase tile)
        {
            if (holder == _player && tile is HeroTile)
                _heroTilePlaced = true;
        }

        private void HandleAttackCardPlayed(DeckHolder holder, BaseAttackSO attack)
        {
            if (holder == _player)
                _attackPlacedOnHero = true;
        }

        /// <summary>Whether the player may end the current turn. During the hero setup turn
        /// this requires the hero tile and at least one attack card to have been placed.</summary>
        public bool CanEndTurn
        {
            get
            {
                if (_gameEnded || _currentHolder == null)
                    return false;

                if (_inHeroSetupTurn)
                    return _heroTilePlaced && _attackPlacedOnHero;

                return true;
            }
        }

        public bool TryPlayCard(CardBase card, ICardPlayContext context)
        {
            if (_currentHolder == null || !_currentHolder.CurrentlyOurTurn)
                return false;

            if (card == null)
                return false;

            if (card.Owner != _currentHolder)
                return false;

            if (_inHeroSetupTurn && !IsAllowedDuringHeroSetup(card))
                return false;

            if (!_currentHolder.TryPlayCard(card, context))
                return false;

            return true;
        }

        /// <summary>During the hero setup turn only the hero card and attack cards may be played,
        /// and an attack card requires the hero tile to already be on the board.</summary>
        private bool IsAllowedDuringHeroSetup(CardBase card)
        {
            if (card is HeroCard)
                return true;

            if (card is AttackCard)
                return _heroTilePlaced;

            return false;
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

            if (_inHeroSetupTurn)
            {
                CompleteHeroSetupTurn();
                return;
            }

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

            List<MonsterTileBase> monsterTileBases = new List<MonsterTileBase>();

            _enemyAI.SetupCards();

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
            _heroTilePlaced = false;
            _attackPlacedOnHero = false;
            _currentHolder = _player;
            _player.SetTurnActive(true);              // order matters: must be true before
            _player.GiveCardDirectlyToHand(_player.HeroCard);
            _player.DrawAllCardsOfType<AttackCard>();
        }

        private void CompleteHeroSetupTurn()
        {
            _inHeroSetupTurn = false;

            DiscardUnplayedAttackCardsFromHand();

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

        /// <summary>Moves any attack cards still in the player's hand (i.e. not placed on the
        /// hero tile during the setup turn) into the discard pile.</summary>
        private void DiscardUnplayedAttackCardsFromHand()
        {
            List<CardBase> unplayedAttacks = _player.Hand.Where(card => card is AttackCard).ToList();
            foreach (CardBase card in unplayedAttacks)
                _player.DiscardFromHand(card);
        }

        public void EndGame(CardGameEndReason reason = CardGameEndReason.TurnLimitReached)
        {
            if (_gameEnded)
                return;

            _gameEnded = true;
            _currentHolder = null;

            InputManager.Instance.Actions.UI_TilePlayer.Disable();

            CardGameEventsBus.RaiseOnGameEnd(reason);

            SceneLoader.Instance.LoadSceneFromType(SceneLoad.PhaseTwoLoad, AfterLoadPhaseTwo);
        }

        private void AfterLoadPhaseTwo()
        {
            TilePlayerManager.WhenReady(t => t.Timeline.LoadFromGrid(Grid));
            GameManager.Instance.CreateLocalMap();
        }

        public TileGridBase GetGridFor(DeckHolder holder)
        {
            return Grid;
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