using UnityEngine;
using System;
using Helper.Wait;
using Equipment;
using System.Collections.Generic;
using GameManagerCalls;

namespace Heros
{
    public class HeroStatBuffsManager : Singleton<HeroStatBuffsManager>, IOnClear
    {
        private HeroStatContainer container;

        public delegate void StatsChanged();
        public StatsChanged statsChanged;

        public enum BuffSourceType
        {
            Manual,
            Equipment,
            Relic
        }

        // Track buffs added through this manager so they can be saved/loaded.
        [Serializable]
        public class BuffSaveEntry
        {
            public string reference; // runtime reference string (e.g. "Sword: 0")
            public StatTypes stat;

            // If sourceType == Manual, buff is serialized directly
            public Buff buff;

            // Otherwise use source type/id to resolve on load
            public BuffSourceType sourceType = BuffSourceType.Manual;
            public string sourceId; // equipmentSO.name or relicSO.name
            public int equipmentContainerIndex = -1; // optional container index

            public BuffSaveEntry() { }

            public static BuffSaveEntry ForManual(string reference, StatTypes stat, Buff buff)
            {
                return new BuffSaveEntry { reference = reference, stat = stat, buff = buff, sourceType = BuffSourceType.Manual };
            }

            public static BuffSaveEntry ForEquipment(string sourceId, int containerIndex, StatTypes stat)
            {
                return new BuffSaveEntry { reference = $"{sourceId}: {containerIndex}", stat = stat, sourceType = BuffSourceType.Equipment, sourceId = sourceId, equipmentContainerIndex = containerIndex };
            }

            public static BuffSaveEntry ForRelic(string sourceId, StatTypes stat)
            {
                return new BuffSaveEntry { reference = sourceId, stat = stat, sourceType = BuffSourceType.Relic, sourceId = sourceId };
            }
        }

        [Serializable]
        private class SaveData : SaveProfileData
        {
            public List<BuffSaveEntry> entries = new List<BuffSaveEntry>();

            public SaveData() { }

            public SaveData(List<BuffSaveEntry> entries)
            {
                this.entries = entries ?? new List<BuffSaveEntry>();
            }
        }

        private readonly List<BuffSaveEntry> activeBuffSaves = new List<BuffSaveEntry>();

        // If load happens before Bind, store incoming save to apply when bound.
        private SaveData loadedSaveData = null;

        public string SaveFileName => "HeroStatBuffsManagerSave";

        public void Bind(HeroStatContainer container)
        {
            this.container = container;

            // If there is a loaded save, apply it now.
            if (loadedSaveData != null)
            {
                ApplySaveData(loadedSaveData);
                loadedSaveData = null;
            }

            // Also apply any active entries that were added before bind
            if (activeBuffSaves.Count > 0)
            {
                var copy = new List<BuffSaveEntry>(activeBuffSaves);
                var pending = new SaveData(copy);
                ApplySaveData(pending);
            }
        }

        // New helper for equipment-sourced buffs so we can save by equipment id instead of embedding Buff
        public void AddBuffFromEquipment(EquipmentSO equipment, int containerIndex, StatTypes stat)
        {
            if (equipment == null)
                return;

            string sourceId = equipment.name;
            string reference = $"{sourceId}: {containerIndex}";

            Buff buffToApply;
            if (!equipment.TryGetHeroBuff(stat, out buffToApply))
                return;

            if (container != null)
            {
                var target = container.Get(stat);
                if (target != null)
                {
                    target.AddBuff(reference, buffToApply);
                }
            }

            // Save equipment reference (do not serialize Buff object)
            AddToActiveSaves(BuffSaveEntry.ForEquipment(sourceId, containerIndex, stat));

            statsChanged?.Invoke();
        }

        public void RemoveBuffFromEquipment(EquipmentSO equipment, int containerIndex, StatTypes stat)
        {
            if (equipment == null)
                return;

            string sourceId = equipment.name;
            string reference = $"{sourceId}: {containerIndex}";

            if (container != null)
            {
                var target = container.Get(stat);
                if (target != null)
                {
                    target.RemoveBuff(reference);
                }
            }

            activeBuffSaves.RemoveAll(x => x.sourceType == BuffSourceType.Equipment && x.sourceId == sourceId && x.equipmentContainerIndex == containerIndex && x.stat == stat);

            statsChanged?.Invoke();
        }

        // Fallback AddBuff for manual cases
        public void AddBuff(string reference, StatTypes stat, Buff buff)
        {
            if (container == null)
            {
                Debug.LogWarning("HeroStatBuffsManager: container not bound yet. Buff will be stored and applied when bound.");
                // store buff so it will apply later once bound
                AddToActiveSaves(BuffSaveEntry.ForManual(reference, stat, buff));
                return;
            }

            var target = container.Get(stat);
            if (target == null)
            {
                Debug.LogWarning($"HeroStatBuffsManager: attempted to add buff for missing stat {stat}");
                return;
            }

            target.AddBuff(reference, buff);
            AddToActiveSaves(BuffSaveEntry.ForManual(reference, stat, buff));

            statsChanged?.Invoke();
        }

        public void RemoveBuff(string reference, StatTypes stat)
        {
            if (container != null)
            {
                var target = container.Get(stat);
                if (target != null)
                {
                    target.RemoveBuff(reference);
                }
            }

            // Remove from saved list (manual or any matching reference)
            activeBuffSaves.RemoveAll(x => x.reference == reference && x.stat == stat);

            statsChanged?.Invoke();
        }

        private void AddToActiveSaves(BuffSaveEntry entry)
        {
            if (entry == null) return;
            // Replace existing entry with same reference+stat
            activeBuffSaves.RemoveAll(x => x.reference == entry.reference && x.stat == entry.stat);
            activeBuffSaves.Add(entry);
        }

        private void ApplySaveData(SaveData saveData)
        {
            activeBuffSaves.Clear();

            if (saveData == null || saveData.entries == null)
                return;

            foreach (var entry in saveData.entries)
            {
                if (entry == null)
                    continue;

                // Resolve entry based on source type
                switch (entry.sourceType)
                {
                    case BuffSourceType.Manual:
                        if (entry.buff != null && container != null)
                        {
                            var target = container.Get(entry.stat);
                            if (target != null)
                                target.AddBuff(entry.reference, entry.buff);
                        }
                        activeBuffSaves.Add(entry);
                        break;
                    case BuffSourceType.Equipment:
                        // Try to find the equipment by name across all inventories and reapply
                        EquipmentSO foundEquip = null;
                        int foundIndex = -1;

                        if (Equipment.EquipmentManager.Instance != null)
                        {
                            foreach (var inv in Equipment.EquipmentManager.Instance.EquipmentInventories)
                            {
                                if (inv == null) continue;
                                for (int i = 0; i < inv.Containers.Length; i++)
                                {
                                    var c = inv.Containers[i];
                                    if (c?.EquipmentSO != null && c.EquipmentSO.name == entry.sourceId)
                                    {
                                        foundEquip = c.EquipmentSO;
                                        foundIndex = i;
                                        break;
                                    }
                                }
                                if (foundEquip != null) break;
                            }
                        }

                        if (foundEquip != null)
                        {
                            // Recreate reference and apply buff from equipment definition
                            var reference = $"{entry.sourceId}: {foundIndex}";
                            if (container != null)
                            {
                                var target = container.Get(entry.stat);
                                if (target != null && foundEquip.TryGetHeroBuff(entry.stat, out var buffToApply))
                                {
                                    target.AddBuff(reference, buffToApply);
                                }
                            }

                            // store resolved entry so we can remove later
                            activeBuffSaves.Add(BuffSaveEntry.ForEquipment(entry.sourceId, foundIndex, entry.stat));
                        }
                        else
                        {
                            // Couldn't resolve now; keep raw entry so it will be added later when equipment apply runs
                            activeBuffSaves.Add(entry);
                            Debug.LogWarning($"[HeroLoad] Could not resolve equipment '{entry.sourceId}' to reapply buff; will keep entry for later.");
                        }
                        break;
                    case BuffSourceType.Relic:
                        // For relics, we store the sourceId and wait for relics to reapply their own buffs (Relic.OnGainRelic usually does that).
                        // However, if we can find a buff directly on RelicManager, we could reapply here. Keep simple: store entry to active list.
                        activeBuffSaves.Add(entry);
                        break;
                }
            }

            statsChanged?.Invoke();
        }

        public void Save()
        {
            // Normalize entries: for equipment entries, make sure sourceId is set and buff is null
            var normalized = new List<BuffSaveEntry>();
            foreach (var buffSaveEntry in activeBuffSaves)
            {
                if (buffSaveEntry == null) continue;
                if (buffSaveEntry.sourceType == BuffSourceType.Equipment && string.IsNullOrEmpty(buffSaveEntry.sourceId))
                {
                    // try to extract equipment name from reference
                    if (!string.IsNullOrEmpty(buffSaveEntry.reference))
                    {
                        var parts = buffSaveEntry.reference.Split(':');
                        if (parts.Length >= 1)
                            buffSaveEntry.sourceId = parts[0].Trim();
                    }
                }
                // Do not serialize heavy Buff object for equipment entries
                if (buffSaveEntry.sourceType == BuffSourceType.Equipment)
                    buffSaveEntry.buff = null;

                normalized.Add(buffSaveEntry);
            }

            SaveManager.IfFileExistsDelete(SaveFileName);
            SaveData saveData = new SaveData(normalized);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                loadedSaveData = outSaveProfile.saveData;

                // If already bound, apply immediately
                if (container != null && loadedSaveData != null)
                {
                    ApplySaveData(loadedSaveData);
                    loadedSaveData = null;
                }
            }
            else
            {
                // no save present -- clear any internal state
                activeBuffSaves.Clear();
            }
        }

        public void Clear()
        {
            // Remove buffs applied to container
            if (container != null)
            {
                foreach (var entry in activeBuffSaves)
                {
                    if (entry == null) continue;
                    var target = container.Get(entry.stat);
                    if (target != null)
                        target.RemoveBuff(entry.reference);
                }
            }

            activeBuffSaves.Clear();
            loadedSaveData = null;
        }
    }
}


