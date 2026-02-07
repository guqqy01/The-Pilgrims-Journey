using System;
using Helper.Wait;
using UnityEngine;
using Heros.Attacks;
using GameManagerCalls;

namespace Heros
{
    // Combined hero save/load and load coordination service.
    // Responsibilities:
    // - Save/Load hero base data (HeroSO reference + stats)
    // - Trigger HeroStatBuffsManager and HeroEquipmentManager to load/bind/apply in a deterministic order
    // - Keep logs concise and readable for debugging
    public class HeroSaveLoadService : MonoBehaviour, ISaveLoad, IOnSave, IOnLoad, IOnClear
    {
        private HeroManager heroManager;

        public string SaveFileName => "HeroManager";

        [Serializable]
        public class SaveData : SaveProfileData
        {
            // Store a direct reference to the HeroSO when possible (Unity will serialize the reference inside the same project/editor).
            public HeroSO heroSO;
            // Keep the name as a fallback for cases where the reference can't be resolved from JSON.
            public string heroSOName;
            public HeroStatContainer.SaveData stats;
        }

        private void Awake()
        {
            heroManager = HeroManager.Instance;
        }

        public void Save()
        {
            if (heroManager == null) heroManager = HeroManager.Instance;

            Debug.Log("[HeroPersistence] Save starting");

            var data = new SaveData();
            data.heroSO = heroManager?.HeroSO;
            data.heroSOName = heroManager?.HeroSO != null ? heroManager.HeroSO.name : string.Empty;
            data.stats = heroManager?.Stats != null ? heroManager.Stats.GetSave() : null;

            // Write base file
            var profile = new SaveProfile<SaveData>(SaveFileName, data);
            SaveManager.SaveOver(profile);
            Debug.Log("[HeroPersistence] Saved hero base data");

            // Save related managers
            HeroStatBuffsManager.Instance?.Save();
            HeroAttacksManager.Instance?.Save();
            Debug.Log("[HeroPersistence] Saved buff and attack manager data");
        }

        public void Load()
        {
            if (heroManager == null) heroManager = HeroManager.Instance;

            Debug.Log("[HeroPersistence] Load starting");

            if (!SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outProfile))
            {
                Debug.LogWarning("[HeroPersistence] No hero save found");
                return;
            }

            var saveData = outProfile.saveData;
            if (saveData == null)
            {
                Debug.LogWarning("[HeroPersistence] Hero save payload null");
                return;
            }

            // Resolve HeroSO: prefer direct reference, fall back to name-based lookup
            HeroSO resolved = saveData.heroSO;
            if (resolved == null && !string.IsNullOrEmpty(saveData.heroSOName))
            {
#if UNITY_EDITOR
                try
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets($"{saveData.heroSOName} t:HeroSO");
                    foreach (var guid in guids)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var candidate = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroSO>(path);
                        if (candidate != null && candidate.name == saveData.heroSOName)
                        {
                            resolved = candidate;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[HeroPersistence] AssetDatabase lookup failed: {ex}");
                }
#endif

                if (resolved == null)
                {
                    var all = Resources.FindObjectsOfTypeAll<HeroSO>();
                    foreach (var h in all)
                    {
                        if (h != null && h.name == saveData.heroSOName)
                        {
                            resolved = h;
                            break;
                        }
                    }
                }
            }

            if (resolved != null)
            {
                heroManager.SelectHero(resolved);
                Debug.Log($"[HeroPersistence] Resolved and selected HeroSO '{resolved.name}'");
            }
            else
            {
                Debug.LogWarning("[HeroPersistence] Could not resolve HeroSO from save. Hero selection skipped.");
            }

            // Apply saved stats to the container if present
            if (saveData.stats != null)
            {
                if (heroManager.HeroSO == null)
                {
                    Debug.LogWarning("[HeroPersistence] Saved stats present but HeroSO not resolved; skipping stats load");
                }
                else
                {
                    heroManager.EnsureStatsFromHeroSO();
                    heroManager.Stats.LoadFromSave(saveData.stats);
                    heroManager.Stats.EnsureDefaults(heroManager.HeroSO);
                    Debug.Log("[HeroPersistence] Applied saved stats to HeroStatContainer");
                }
            }

            // Now ensure buff manager and equipment manager exist, then load/bind/apply in order
            WaitForHelper.WaitForConditionThenPerformAction(() => HeroStatBuffsManager.Instance != null && HeroEquipmentManager.Instance != null,
                () =>
                {
                    // Load buff save so manager has entries
                    HeroStatBuffsManager.Instance.Load();

                    // Bind the container so buffs apply to runtime stats
                    if (heroManager.Stats != null)
                    {
                        HeroStatBuffsManager.Instance.Bind(heroManager.Stats);
                        Debug.Log("[HeroPersistence] Bound HeroStatBuffsManager to HeroStatContainer");
                    }

                    // Reapply equipment-based buffs
                    HeroEquipmentManager.Instance.ApplyEquipmentBuffs();
                    Debug.Log("[HeroPersistence] Applied equipment buffs");

                    // Load attacks (they will reapply when hero exists)
                    HeroAttacksManager.Instance?.Load();
                    Debug.Log("[HeroPersistence] Invoked HeroAttacksManager.Load");
                });
        }

        public void Clear()
        {
            if (heroManager == null) heroManager = HeroManager.Instance;

            Debug.Log("[HeroPersistence] Clear called");

            heroManager?.ClearHeroState();
            HeroStatBuffsManager.Instance?.Clear();
            HeroAttacksManager.Instance?.Clear();

            Debug.Log("[HeroPersistence] Cleared hero and related manager state");
        }
    }
}
