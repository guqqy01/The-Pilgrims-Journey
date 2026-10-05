using Equipment;
using GameManagerCalls;
using System;
using System.Collections.Generic;
using TPJ.Gameplay;
using Units;
using UnityEngine;
namespace Tiles
{
    /// <summary>Cards the player has earned this run. Merged into the deck at the start of every card game.</summary>
    public class PlayerCollection : Singleton<PlayerCollection>, ISaveLoad, IOnSave, IOnLoad, IOnClear
    {
        public string SaveFileName => "PlayerCollectionSave";

        [Serializable]
        public class UnitEntry
        {
            public TileSO tile;
            public UnitSO unit;
        }

        private readonly List<UnitEntry> units = new();
        private readonly List<EquipmentSO> equipment = new();

        public IReadOnlyList<UnitEntry> Units => units;
        public IReadOnlyList<EquipmentSO> Equipment => equipment;

        public void AddUnit(TileSO tile, UnitSO unit)
        {
            if (tile == null || unit == null)
            {
                Debug.LogWarning("PlayerCollection.AddUnit: missing tile or unit.");
                return;
            }

            units.Add(new UnitEntry { tile = tile, unit = unit });
            Save();
        }

        public void AddEquipment(EquipmentSO item)
        {
            if (item == null)
                return;

            equipment.Add(item);
            Save();
        }

        /// <summary>Fresh card sources every call. Never reuse tiles from a previous game,
        /// because the card-game grid destroys its tiles when it resets.</summary>
        public List<ICardable> CreateCardables()
        {
            var result = new List<ICardable>();

            foreach (UnitEntry entry in units)
                result.Add(new UnitTile(entry.tile, entry.unit));

            foreach (EquipmentSO item in equipment)
            {
                if ((object)item is ICardable cardable)
                    result.Add(cardable);
                else
                    Debug.LogWarning($"{item.name} is not ICardable, so no card was created for it.");
            }

            return result;
        }

        public void Clear()
        {
            units.Clear();
            equipment.Clear();
        }

        public void Save()
        {
            var saveData = new SaveData
            {
                units = new List<UnitEntry>(units),
                equipment = new List<EquipmentSO>(equipment)
            };
            SaveManager.SaveOver(new SaveProfile<SaveData>(SaveFileName, saveData));
        }

        public void Load()
        {
            Clear();

            if (!SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> profile))
                return;

            foreach (UnitEntry entry in profile.saveData.units ?? new List<UnitEntry>())
            {
                if (entry.tile != null && entry.unit != null)
                    units.Add(entry);
            }

            foreach (EquipmentSO item in profile.saveData.equipment ?? new List<EquipmentSO>())
            {
                if (item != null)
                    equipment.Add(item);
            }
        }

        [Serializable]
        private class SaveData : SaveProfileData
        {
            public List<UnitEntry> units;
            public List<EquipmentSO> equipment;
        }
    }
}