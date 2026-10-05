using GlobalMap;
using Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace TPJ.Gameplay
{
    [Serializable]
    public class EnemyDeckHolderAI : DeckHolder
    {
        [Header("AI Weights — tune in the Inspector")]
        [SerializeField] private float tileSizeWeight = 1f;

        [Header("Column control (win condition: most tiles in a column)")]
        [SerializeField] private float columnFlipBonus = 6f;    // takes a column from losing/tied to winning
        [SerializeField] private float columnHoldBonus = 1.5f;  // extends a column already winning
        [SerializeField] private float columnContestBonus = 0.75f; // still behind, but narrows the gap

        [Header("Raid / defense synergy")]
        [SerializeField] private float banditRaidBonus = 4f;
        [SerializeField] private float spyRaidBonus = 4f;
        [SerializeField] private float guardProtectBonus = 2f;
        [SerializeField] private float exposurePenalty = 3f;
        [SerializeField] private float guardedBonus = 1f;
        [SerializeField] private float brigadeThreatBonus = 2f;

        [SerializeField, Range(0f, 5f)]
        private float randomJitter = 0.5f; // small noise so the AI isn't perfectly predictable

        [SerializeField]
        private int amountOfMonsterCards = 20;

        public void SetupCards()
        {
            MonsterSpawnSettings monsterSpawnSettings = GlobalMapGenerator.Instance.CurrentMapPoint.GenerationSettings.MonsterSpawnSettings;

            List<MonsterTileBase> monsterTileBases = MonsterTileManager.Instance.GetMonsterTiles(monsterSpawnSettings, TileManager.Instance.MonsterTiles, amountOfMonsterCards);
            DeckBuilder.BuildDeck(this, monsterTileBases);
        }

        // Column index = cell.x (i.e. columns run along the grid's X axis, rows along Y).
        // Swap to cell.y below in BuildColumnTally/ScoreColumnControl if your lanes run the other way.

        public override void BeginTurn(int baseAmountOfCardToDraw, int baseAmountOfActionPoints)
        {
            base.BeginTurn(baseAmountOfCardToDraw, baseAmountOfActionPoints);

            int breaker = 20;
            while (CardGamePoints.CurrentPoints > 0 && breaker > 0)
            {
                EnemyMove best = FindBestMove();
                if (best == null)
                    break;

                CardGameManager.Instance.TryPlayCard(best.Card, best.Context);
                breaker--;
            }

            CardGameManager.IfReady(x => x.EndTurn());
        }

        private class EnemyMove
        {
            public readonly CardBase Card;
            public readonly ICardPlayContext Context;

            public EnemyMove(CardBase card, ICardPlayContext context)
            {
                Card = card;
                Context = context;
            }
        }

        private EnemyMove FindBestMove()
        {
            TileGridBase grid = CardGameManager.Instance.Grid;
            Dictionary<int, (int ally, int enemy)> columnTally = BuildColumnTally(grid);

            EnemyMove best = null;
            float bestScore = float.NegativeInfinity;

            foreach (CardBase card in AbleToPlayCards)
            {
                switch (card)
                {
                    case MonsterCard monsterCard:
                        TryImprovePlacement(monsterCard, grid, columnTally, ref best, ref bestScore);
                        break;

                    case EquipmentCard equipmentCard:
                        TryImproveEquipmentPlay(equipmentCard, ref best, ref bestScore);
                        break;
                }
            }

            return best;
        }

        private void TryImprovePlacement(
            MonsterCard card, TileGridBase grid,
            IReadOnlyDictionary<int, (int ally, int enemy)> columnTally,
            ref EnemyMove best, ref float bestScore)
        {
            MonsterTileBase tile = card.CardableRef;

            foreach (Vector2Int position in grid.GetFreeTilePositions())
            {
                for (float rotation = 0f; rotation < 360f; rotation += 90f)
                {
                    if (!grid.CanPlaceTile(tile, position, rotation))
                        continue;

                    float score = ScorePlacement(grid, tile, position, rotation, columnTally)
                                  + UnityEngine.Random.Range(0f, randomJitter);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = new EnemyMove(card, new TileCardPlayContext(grid, position, rotation));
                    }
                }
            }
        }

        private void TryImproveEquipmentPlay(EquipmentCard card, ref EnemyMove best, ref float bestScore)
        {
            var provider = (ICardLegalityProvider)CardGameManager.Instance;

            foreach (TileBase target in provider.GetTargetableTiles(this))
            {
                if (target is not IEquipmentCardTarget) continue;
                if (TileOwnership.Get(target) != this) continue;
                if (!target.HasRoomForEquipment) continue;

                float score = ScoreEquipmentPlay(card, target) + UnityEngine.Random.Range(0f, randomJitter);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = new EnemyMove(card, new EquipmentCardPlayContext(target));
                }
            }
        }

        private float ScorePlacement(
            TileGridBase grid, MonsterTileBase tile, Vector2Int position, float rotation,
            IReadOnlyDictionary<int, (int ally, int enemy)> columnTally)
        {
            float score = tile.Shape.Count * tileSizeWeight;

            score += ScoreColumnControl(tile, position, rotation, columnTally);

            List<TileBase> neighbors = GridFootprintMath
                .GetHypotheticalAdjacentTiles(grid, tile, position, rotation)
                .ToList();

            score += ScoreTypeSynergy(tile.TileCardType, neighbors);

            return score;
        }

        private Dictionary<int, (int ally, int enemy)> BuildColumnTally(TileGridBase grid)
        {
            var tally = new Dictionary<int, (int ally, int enemy)>();

            foreach (TileBase placed in grid.ListOfTiles)
            {
                DeckHolder owner = TileOwnership.Get(placed);
                if (owner == null) continue;

                foreach (Vector2Int cell in placed.RotatedPlacedShape)
                {
                    var entry = tally.TryGetValue(cell.x, out var v) ? v : (0, 0);
                    if (owner == this) v.ally++; else v.enemy++;
                    tally[cell.x] = entry;
                }
            }

            return tally;
        }

        private float ScoreColumnControl(
            MonsterTileBase tile, Vector2Int position, float rotation,
            IReadOnlyDictionary<int, (int ally, int enemy)> columnTally)
        {
            float score = 0f;
            List<Vector2Int> newCells = GridFootprintMath.GetHypotheticalFootprint(tile, position, rotation);

            foreach (int column in newCells.Select(c => c.x).Distinct())
            {
                var (ally, enemy) = columnTally.TryGetValue(column, out var v) ? v : (0, 0);
                int cellsAdded = newCells.Count(c => c.x == column);

                bool wasWinning = ally > enemy;
                bool willBeWinning = (ally + cellsAdded) > enemy;

                if (!wasWinning && willBeWinning)
                    score += columnFlipBonus;
                else if (willBeWinning)
                    score += columnHoldBonus;
                else
                    score += columnContestBonus * cellsAdded;
            }

            return score;
        }

        private float ScoreTypeSynergy(TileCardType type, List<TileBase> neighbors)
        {
            float score = 0f;

            foreach (TileCardType flag in type.CollapseFlags())
            {
                switch (flag)
                {
                    case TileCardType.Bandit:
                        foreach (TileBase n in neighbors)
                            if (n.TileCardType.HasFlag(TileCardType.Farmer) && TileOwnership.Get(n) != this && !TileCardTypeAdjacency.IsDefended(n))
                                score += banditRaidBonus * n.Shape.Count;
                        break;

                    case TileCardType.Spy:
                        foreach (TileBase n in neighbors)
                            if (n.TileCardType.HasFlag(TileCardType.Scout) && TileOwnership.Get(n) != this && !TileCardTypeAdjacency.IsDefended(n))
                                score += spyRaidBonus * n.Shape.Count;
                        break;

                    case TileCardType.Guard:
                        foreach (TileBase n in neighbors)
                            if ((n.TileCardType.HasFlag(TileCardType.Farmer) || n.TileCardType.HasFlag(TileCardType.Scout)) && TileOwnership.Get(n) == this)
                                score += guardProtectBonus;
                        break;

                    case TileCardType.Farmer:
                    case TileCardType.Scout:
                        TileCardType threatType = flag == TileCardType.Farmer ? TileCardType.Bandit : TileCardType.Spy;
                        bool threatened = neighbors.Any(n => n.TileCardType.HasFlag(threatType) && TileOwnership.Get(n) != this);
                        bool defended = neighbors.Any(n => n.TileCardType.HasFlag(TileCardType.Guard) && TileOwnership.Get(n) == this);

                        if (threatened && !defended) score -= exposurePenalty;
                        else if (defended) score += guardedBonus;
                        break;

                    case TileCardType.Brigade:
                        if (neighbors.Any(n => TileOwnership.Get(n) != this))
                            score += brigadeThreatBonus;
                        break;
                }
            }

            return score;
        }

        private float ScoreEquipmentPlay(EquipmentCard card, TileBase target)
        {
            float score = target.Shape.Count * tileSizeWeight * 0.5f;
            TileCardType grantedType = card.TileCardType;

            if (grantedType.HasFlag(TileCardType.Guard))
            {
                bool exposed = TileCardTypeAdjacency.TryFindOpposingAdjacent(target, TileCardType.Bandit, out _)
                            || TileCardTypeAdjacency.TryFindOpposingAdjacent(target, TileCardType.Spy, out _);
                if (exposed) score += guardProtectBonus * 2f;
            }

            if (grantedType.HasFlag(TileCardType.Bandit)
                && TileCardTypeAdjacency.TryFindOpposingAdjacent(target, TileCardType.Farmer, out TileBase farmer)
                && !TileCardTypeAdjacency.IsDefended(farmer))
            {
                score += banditRaidBonus;
            }

            if (grantedType.HasFlag(TileCardType.Spy)
                && TileCardTypeAdjacency.TryFindOpposingAdjacent(target, TileCardType.Scout, out TileBase scout)
                && !TileCardTypeAdjacency.IsDefended(scout))
            {
                score += spyRaidBonus;
            }

            // TODO once secret missions exist: add a ScoreMissionProgress(...) term here and in
            // ScorePlacement, so the AI also weighs moves that advance its hidden objective.

            return score;
        }
    }
}