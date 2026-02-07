using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heros
{
    [Serializable]
    public class HeroStatContainer
    {
        [Serializable]
        public struct StatEntry
        {
            public HeroStats stat;
            public BuffableStat value;
        }

        [SerializeField]
        private List<StatEntry> serializedStats = new List<StatEntry>();

        private readonly Dictionary<HeroStats, BuffableStat> stats = new Dictionary<HeroStats, BuffableStat>();

        public BuffableStat Get(HeroStats stat)
        {
            if(stats.TryGetValue(stat, out var buff))
            {
                return buff;
            }
            else
            {
                return null;
            }
        }

        public delegate void OnStatChanged();
        public OnStatChanged statChanged;

        public HeroStatContainer(HeroSO heroSO)
        {
            foreach(HeroStats stat in Enum.GetValues(typeof(HeroStats)))
            {
                Buff baseBuff = heroSO.GetBuff(stat);
                if (baseBuff == null) continue;

                var buffable = new BuffableStat(stat.IsIntStat() ? 0 : 0f);

                buffable.onValueChange += () => statChanged?.Invoke();

                buffable.AddBuff("Hero Base", baseBuff);
                stats.Add(stat, buffable);
            }

            // update serialized list for inspector / saving
            SyncToSerialized();
        }

        // Ensure any stats missing from serialized or initial setup are created from the HeroSO defaults
        public void EnsureDefaults(HeroSO heroSO)
        {
            foreach(HeroStats stat in Enum.GetValues(typeof(HeroStats)))
            {
                if (stats.ContainsKey(stat))
                    continue;

                Buff baseBuff = null;
                try { baseBuff = heroSO.GetBuff(stat); } catch { baseBuff = null; }

                var buffable = new BuffableStat(stat.IsIntStat() ? 0 : 0f);
                buffable.onValueChange += () => statChanged?.Invoke();

                if (baseBuff != null)
                {
                    buffable.AddBuff("Hero Base", baseBuff);
                }

                stats[stat] = buffable;
            }

            SyncToSerialized();
        }

        // Sync the runtime dictionary to the serialized list
        private void SyncToSerialized()
        {
            serializedStats.Clear();
            foreach(var kvp in stats)
            {
                serializedStats.Add(new StatEntry { stat = kvp.Key, value = kvp.Value });
            }
        }

        // Rebuild runtime dictionary from serialized list (used on load)
        private void RebuildFromSerialized()
        {
            stats.Clear();
            foreach(var entry in serializedStats)
            {
                if (entry.value == null)
                    continue;

                stats[entry.stat] = entry.value;
                entry.value.onValueChange += () => statChanged?.Invoke();
            }
        }

        // Provide a savable payload
        [Serializable]
        public class SaveData : SaveProfileData
        {
            public List<StatEntry> entries = new List<StatEntry>();

            public SaveData(List<StatEntry> entries) 
            {
                this.entries.Clear();
                this.entries.AddRange(entries);
            }
        }

        public SaveData GetSave()
        {
            SyncToSerialized();
            var save = new SaveData(serializedStats);
            return save;
        }

        public void LoadFromSave(SaveData saveData)
        {
            serializedStats.Clear();
            if (saveData != null && saveData.entries != null)
                serializedStats.AddRange(saveData.entries);

            RebuildFromSerialized();
        }
    }
}

