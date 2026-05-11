using System.Collections.Generic;
using UnityEngine;
using Units;
using Equipment;
using System;
using LevelingSystem;

namespace Tiles
{
    public class UnitTile : Tile<UnitSO>
    {
        public EquipmentSO[] Equipment { get; private set; } = new EquipmentSO[4];

        public int Level { get; private set; } = 1;
        public bool IsMaxLevel => Level == 5;

        public int CurrentAmountOfUnitsInTile { get; private set; }
        public int MaxAmountOfUnitsInTile { get; private set; }

        public int AmountOfUnitsSpawned { get; private set; }
        public int AmountOfUnitsEscapedNeededToLevel { get; private set; }
        public int CurrentAmountOfUnitsEscaped { get; private set; }

        public event Action OnModelChanged;
        public event Action OnLevelHealthChanged;

        public OnUpdateUI onUpdateLevelUnitsAndHealthUI;

        public UnitTile(TileSO tileSO, UnitSO unitSO) : base(tileSO, unitSO)
        {
            RecalculateLevelValues();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnModelChanged = null;
            OnLevelHealthChanged = null;
            onUpdateLevelUnitsAndHealthUI = null;
        }

        public void SetupNewUnitSO(UnitSO unitSO)
        {
            Item = unitSO;

            RecalculateLevelValues();

            onUpdateUI?.Invoke();
        }

        #region Leveling

        private static float AdditionalUnitsPerLevel(int level)
        {
            return level * level * 0.2f + 1f;
        }

        private void RecalculateLevelValues()
        {
            AmountOfUnitsEscapedNeededToLevel =
                Mathf.RoundToInt(
                    AdditionalUnitsPerLevel(Level) *
                    Item.numberOfUnitsEscapedToLevelUp *
                    TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier *
                    TileSO.Shape.Count);

            MaxAmountOfUnitsInTile =
                Mathf.RoundToInt(
                    AdditionalUnitsPerLevel(Level) *
                    Item.numberOfUnitsAbleToSpawn *
                    TileSO.amountOfUnitsAbleToBeSpawnedAndRequiredToLevelPerTileSectionMultiplier *
                    TileSO.Shape.Count);

            CurrentAmountOfUnitsInTile = MaxAmountOfUnitsInTile;
        }

        public void OnUnitSpawn()
        {
            CurrentAmountOfUnitsInTile = Mathf.Clamp(CurrentAmountOfUnitsInTile - 1, 0, MaxAmountOfUnitsInTile);
            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned + 1);
            OnLevelHealthChanged?.Invoke();
        }

        public void OnUnitDie(Health _)
        {
            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned - 1);

            if (CurrentAmountOfUnitsInTile <= 0 && AmountOfUnitsSpawned <= 0)
            {
                DestroyTile();
            }
        }

        public void OnUnitEscape()
        {
            CurrentAmountOfUnitsEscaped =
                Mathf.Clamp(CurrentAmountOfUnitsEscaped + 1, 0, AmountOfUnitsEscapedNeededToLevel);

            if (CurrentAmountOfUnitsEscaped >= AmountOfUnitsEscapedNeededToLevel)
            {
                LevelUp();
            }

            AmountOfUnitsSpawned = Mathf.Max(0, AmountOfUnitsSpawned - 1);
            CurrentAmountOfUnitsInTile =
                Mathf.Clamp(CurrentAmountOfUnitsInTile + 1, 0, MaxAmountOfUnitsInTile);

            OnLevelHealthChanged?.Invoke();
        }

        private void LevelUp()
        {
            Level = Mathf.Clamp(Level + 1, 1, 5);
            CurrentAmountOfUnitsEscaped = 0;
            RecalculateLevelValues();
            OnModelChanged?.Invoke();
        }

        public void SetupFromSave(TileSave tileSave)
        {
            Level = tileSave.level;
            RecalculateLevelValues();
            CurrentAmountOfUnitsEscaped = tileSave.tileExp;
            CurrentAmountOfUnitsInTile = tileSave.tileHealth;
            OnModelChanged?.Invoke();
            Equipment = tileSave.equipment;
        }
        #endregion

        #region Stats

        public Buff GetTileBuff(UnitStatsEnum stat)
        {
            return TileUpgradeManager.Instance.GetBuff(stat, Level);
        }

        private IntOrFloatObject GetStat(UnitStatsEnum stat)
        {
            var baseValue = Item.GetStat(stat);

            List<Buff> buffs = new()
            {
                GetTileBuff(stat),
                UpgradeManager.Instance.GetBuff(stat),
                UnitBuffManager.Instance.GetBuff(stat)
            };

            foreach (var equipment in Equipment)
            {
                if (equipment != null && equipment.TryGetUnitBuff(stat, out Buff outBuff))
                {
                    buffs.Add(outBuff);
                }
            }

            if (buffs.Count == 0)
                return baseValue;

            Buff combined = buffs[0];
            for (int i = 1; i < buffs.Count; i++)
                combined += buffs[i];

            return combined.Value(baseValue);
        }

        public float SpawnRate => Mathf.Max(GetStat(UnitStatsEnum.SpawnRate), 0f);
        public float Movement => Mathf.Max(GetStat(UnitStatsEnum.MovementSpeed), 0f);
        public int Health => Mathf.Max(GetStat(UnitStatsEnum.Health), 0);
        public int Shield => Mathf.Max(GetStat(UnitStatsEnum.Shield), 0);
        public float ShieldRegen => Mathf.Max(GetStat(UnitStatsEnum.ShieldRegeneration), 0f);
        public float ShieldCooldown => Mathf.Max(GetStat(UnitStatsEnum.ShieldCooldown), 0f);

        #endregion
    }
 }

