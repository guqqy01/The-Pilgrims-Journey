using Helper.Wait;
using System;
using UnityEngine;

namespace Heros
{
    /// <summary>
    /// Coordinates hero-related load steps so that Stats, Buffs, Equipment, and Attacks are loaded and bound
    /// in a deterministic order before the hero GameObject is spawned.
    /// Attach to the same GameObject as HeroManager.
    /// </summary>
    public class HeroLoadCoordinator : MonoBehaviour
    {
        private HeroManager heroManager;

        public void StartLoad(HeroManager manager)
        {
            heroManager = manager ?? throw new ArgumentNullException(nameof(manager));

            // Ensure basic stats object exists
            heroManager.EnsureStatsFromHeroSO();

            // Step 1: Ensure buff manager is loaded and has applied its saved entries (if any)
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, OnBuffManagerReady);

            // Also ensure attacks manager load is invoked so it can reapply after hero spawn
            WaitForHelper.WaitForObjectThenPerformAction(() => Heros.Attacks.HeroAttacksManager.Instance, () => { try { Heros.Attacks.HeroAttacksManager.Instance.Load(); } catch { } });

            // Ensure equipment manager exists, we'll ask it to apply equipment buffs when ready
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, OnEquipmentManagerReady);
        }

        private void OnBuffManagerReady()
        {
            try
            {
                HeroStatBuffsManager.Instance.Load();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HeroLoadCoordinator: Failed to call HeroStatBuffsManager.Load: {ex}");
            }

            // If the Stats container already exists, bind now
            if (heroManager.Stats != null)
            {
                HeroStatBuffsManager.Instance.Bind(heroManager.Stats);
                heroManager.SetStatsBound(true);
            }
        }

        private void OnEquipmentManagerReady()
        {
            try
            {
                // Let the equipment manager reapply equipment buffs into the stat manager
                HeroEquipmentManager.Instance.ApplyEquipmentBuffs();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HeroLoadCoordinator: Failed to call HeroEquipmentManager.ApplyEquipmentBuffs: {ex}");
            }

            heroManager.SetEquipmentApplied(true);

            // Do not spawn here. Match StartNewJourney flow: spawning occurs in AfterCreateLocalMap.
            // Ensure bindings are final and flags set; AfterCreateLocalMap will handle the actual spawn when the local map is created.
            // However ensure final bind in case it wasn't done earlier.
            if (HeroStatBuffsManager.Instance != null && heroManager.Stats != null)
            {
                HeroStatBuffsManager.Instance.Bind(heroManager.Stats);
                heroManager.SetStatsBound(true);
            }
        }
    }
}
