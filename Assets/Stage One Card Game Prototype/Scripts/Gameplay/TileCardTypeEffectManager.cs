using System.Collections.Generic;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class TileCardTypeEffectManager : Singleton<TileCardTypeEffectManager>
    {
        private readonly Dictionary<TileCardType, TileCardTypeBehavior> _behaviors = new();
        private readonly Dictionary<TileBase, (TileBase target, Vector2Int cell)> _pendingBrigadeChoices = new();

        protected override void Awake()
        {
            base.Awake();
            Register(new FarmerBehavior());
            Register(new GuardBehavior());
            Register(new BanditBehavior());
            Register(new ScoutBehavior());
            Register(new SpyBehavior());
            Register(new BrigadeBehavior());
        }

        private void Register(TileCardTypeBehavior behavior) => _behaviors[behavior.HandledType] = behavior;

        private void OnEnable()
        {
            CardGameEventsBus.OnBeginTurn += OnBeginTurn;
            CardGameEventsBus.OnTileCardPlayed += OnTileCardPlayed;
            CardGameEventsBus.OnEndTurn += OnEndTurn;
        }

        protected override void CleanUp()
        {
            CardGameEventsBus.OnBeginTurn -= OnBeginTurn;
            CardGameEventsBus.OnTileCardPlayed -= OnTileCardPlayed;
            CardGameEventsBus.OnEndTurn -= OnEndTurn;
            _pendingBrigadeChoices.Clear();
        }

        private void OnBeginTurn(DeckHolder holder)
        {
            foreach (TileBase tile in CardGameManager.Instance.Grid.ListOfTiles)
            {
                if (TileOwnership.Get(tile) != holder) continue;

                foreach (TileCardType flag in tile.TileCardType.CollapseFlags())
                    if (_behaviors.TryGetValue(flag, out var behavior))
                        behavior.OnTurnBegin(tile);
            }
        }

        private void OnTileCardPlayed(DeckHolder holder, TileBase tile)
        {
            foreach (TileCardType flag in tile.TileCardType.CollapseFlags())
                if (_behaviors.TryGetValue(flag, out var behavior))
                    behavior.OnPlaced(holder, tile);
        }

        private void OnEndTurn(DeckHolder holder)
        {
            List<TileBase> resolved = new List<TileBase>();

            foreach (var entry in _pendingBrigadeChoices)
            {
                TileBase brigadeTile = entry.Key;
                if (TileOwnership.Get(brigadeTile) != holder)
                    continue;

                (TileBase target, Vector2Int cell) = entry.Value;

                if (target != null && target.TileGrid != null)
                    target.TileGrid.DestroyTileSection(target, cell);

                brigadeTile.Locked = false;
                resolved.Add(brigadeTile);
            }

            foreach (TileBase brigadeTile in resolved)
            {
                _pendingBrigadeChoices.Remove(brigadeTile);
                CardGameEventsBus.RaiseOnBrigadeResolved(brigadeTile);
            }
        }

        /// <summary>Records which cell the given Brigade tile will destroy at the end of this turn.</summary>
        public static void SetBrigadeTarget(TileBase brigadeTile, TileBase targetTile, Vector2Int targetCell) =>
            IfReady(m => m._pendingBrigadeChoices[brigadeTile] = (targetTile, targetCell));

        /// <summary>Cancels a pending Brigade choice (e.g. no legal targets were available). Caller
        /// is responsible for unlocking the tile afterward.</summary>
        public static void CancelBrigadeChoice(TileBase brigadeTile) =>
            IfReady(m => m._pendingBrigadeChoices.Remove(brigadeTile));
    }
}