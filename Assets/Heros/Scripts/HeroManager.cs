using Equipment;
using GameManagerCalls;
using GlobalMap;
using Helper.Wait;
using Heros.Attacks;
using LevelingSystem;
using LocalMap;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Heros
{
    public class HeroManager : Singleton<HeroManager>, IOnAfterCreateLocalMap
    {
        public HeroStatContainer Stats { get; private set; }

        public HeroSO HeroSO { private set; get; }
        public Hero ActiveHero { private set; get; }

        private HeroStatBuffsManager heroStatsManager;
        public HeroStatBuffsManager HeroStatsManager
        {
            get
            {
                if (heroStatsManager == null)
                    heroStatsManager = HeroStatBuffsManager.Instance;

                return heroStatsManager;
            }
        }

        private HeroEquipmentManager heroEquipmentManager;
        public HeroEquipmentManager HeroEquipmentManager
        {
            get
            {
                if (heroEquipmentManager == null)
                    heroEquipmentManager = HeroEquipmentManager.Instance;

                return heroEquipmentManager;
            }
        }

        public delegate void OnHeroSpawned(Hero hero);
        public OnHeroSpawned onHeroSpawned;

        // Flags to indicate that post-load subscriptions have been applied
        private bool statsBound = false;
        private bool equipmentApplied = false;

        private void SubscribeToHeroEquipmentManager()
        {
            HeroEquipmentManager.Instance.ApplyEquipmentBuffs();
            equipmentApplied = true;
        }

        private void SubscribeToHeroStatsManager()
        {
            HeroStatBuffsManager.Instance.Bind(Stats);
            statsBound = true;
        }

        private void SetupHeroAttack()
        {
            // Ensure all required pieces are ready before adding the attack
            if (HeroManager.Instance == null || HeroManager.Instance.Stats == null || !statsBound || !equipmentApplied)
            {
                // Re-schedule setup until stats and managers are bound
                WaitForHelper.WaitForConditionThenPerformAction(() => HeroManager.Instance != null && HeroManager.Instance.Stats != null &&
                                                                     statsBound && equipmentApplied &&
                                                                     HeroStatBuffsManager.Instance != null && HeroEquipmentManager.Instance != null &&
                                                                     HeroAttacksManager.Instance != null && HeroManager.Instance.ActiveHero != null,
                                                                     SetupHeroAttack);
                return;
            }

            HeroAttacksManager.Instance.AddHeroAttack(HeroSO.startingAttack);
        }

        public void EnsureStatsFromHeroSO()
        {
            if (HeroSO == null)
                return;

            if (Stats == null)
            {
                Stats = new HeroStatContainer(HeroSO);
                Stats.EnsureDefaults(HeroSO);
            }
        }

        public void SetStatsBound(bool bound)
        {
            statsBound = bound;
        }

        public void SetEquipmentApplied(bool applied)
        {
            equipmentApplied = applied;
        }

        // Public API used by HeroLoadCoordinator to spawn and start attack setup
        public void SpawnAndStartAttackSetup()
        {
            HandleSpawnHero();
        }

        public void SelectHero(HeroSO heroSO)
        {
            Debug.Log($"HeroManager.SelectHero called with: {heroSO?.name}");

            this.HeroSO = heroSO;

            // reset binding flags when selecting a new hero
            statsBound = false;
            equipmentApplied = false;

            Stats = new HeroStatContainer(HeroSO);
            // Ensure defaults are present so subsequent bind/spawn sees all stats
            Stats.EnsureDefaults(HeroSO);

            WaitForHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, SubscribeToHeroStatsManager);
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, SubscribeToHeroEquipmentManager);
        }

        public void AfterCreateLocalMap()
        {
            Debug.Log($"HeroManager.AfterCreateLocalMap called. HeroSO currently: {(HeroSO != null ? HeroSO.name : "<null>")} ");

            if (ActiveHero == null)
            {
                // If we've already selected a hero, ensure stats and equipment subscriptions are applied before spawning
                if (HeroSO != null)
                {
                    Debug.Log("HeroManager.AfterCreateLocalMap: HeroSO present, ensuring Stats and subscriptions before spawning.");

                    // Ensure Stats exists
                    if (Stats == null)
                    {
                        Stats = new HeroStatContainer(HeroSO);
                        Stats.EnsureDefaults(HeroSO);
                    }

                    // Wait for the buff managers to be available and subscribe, then spawn
                    WaitForHelper.WaitForConditionThenPerformAction(() => HeroStatBuffsManager.Instance != null && HeroEquipmentManager.Instance != null,
                        () =>
                        {
                            if (!statsBound) SubscribeToHeroStatsManager();
                            if (!equipmentApplied) SubscribeToHeroEquipmentManager();

                            HandleSpawnHero();
                        });
                }
                else
                {
                    Debug.Log("HeroManager.AfterCreateLocalMap: HeroSO not present, waiting for selection...");
                    WaitForHelper.WaitForObjectThenPerformAction(() => HeroSO, () => HandleSpawnHero());
                }
            }
            else
            {
                ResetHero();
            }
        }

        private void HandleSpawnHero()
        {
            Debug.Log($"HeroManager.HandleSpawnHero invoked. HeroSO: {(HeroSO != null ? HeroSO.name : "<null>")}");
            SpawnHero();

            // Wait for full readiness: hero instance, stats bound and attack manager available
            WaitForHelper.WaitForConditionThenPerformAction(() => HeroAttacksManager.Instance != null &&
                                                               HeroManager.Instance != null &&
                                                               HeroManager.Instance.ActiveHero != null &&
                                                               HeroManager.Instance.Stats != null &&
                                                               statsBound && equipmentApplied &&
                                                               HeroStatBuffsManager.Instance != null &&
                                                               HeroEquipmentManager.Instance != null,
                                                               SetupHeroAttack);
        }

        private void RemoveHero()
        {
            if (ActiveHero != null)
            {
                ActiveHero.HealthAndShieldManager.Health.onDie -= OnHeroDie;
                Destroy(ActiveHero.gameObject);
            }
        }

        private void SpawnHero()
        {
            // Ensure all required stat entries are present before instantiating the hero.
            if (Stats == null || Stats.Get(HeroStats.Health) == null || Stats.Get(HeroStats.Shield) == null || Stats.Get(HeroStats.ShieldRegeneration) == null || Stats.Get(HeroStats.ShieldCooldown) == null)
            {
                Debug.Log("HeroManager.SpawnHero: required stats not ready, waiting...");
                WaitForHelper.WaitForConditionThenPerformAction(() =>
                    HeroManager.Instance != null && HeroManager.Instance.Stats != null &&
                    HeroManager.Instance.Stats.Get(HeroStats.Health) != null &&
                    HeroManager.Instance.Stats.Get(HeroStats.Shield) != null &&
                    HeroManager.Instance.Stats.Get(HeroStats.ShieldRegeneration) != null &&
                    HeroManager.Instance.Stats.Get(HeroStats.ShieldCooldown) != null,
                    SpawnHero);
                return;
            }

            Debug.Log($"HeroManager.SpawnHero called. HeroSO: {(HeroSO != null ? HeroSO.name : "<null>" )}, prefab: {(HeroSO != null ? (HeroSO.prefab != null ? HeroSO.prefab.name : "<prefab null>") : "<no so>")}");

            if (HeroSO == null || HeroSO.prefab == null)
            {
                Debug.LogError("HeroManager.SpawnHero aborted: HeroSO or prefab is null.");
                return;
            }

            Vector2 position = new Vector2(LocalMapGenerator.Instance.MapSizeX / 2f, LocalMapGenerator.Instance.MapSizeY / 2f);

            ActiveHero = Instantiate(HeroSO.prefab, position, Quaternion.identity, transform);

            ActiveHero.Setup(HeroSO, Stats);

            ActiveHero.HealthAndShieldManager.Health.onDie += OnHeroDie;

            onHeroSpawned?.Invoke(ActiveHero);

            Debug.Log($"Hero spawned: {ActiveHero?.name} at {position}");
        }

        private void ResetHero()
        {
            ActiveHero.transform.position = new Vector2(LocalMapGenerator.Instance.MapSizeX / 2f, LocalMapGenerator.Instance.MapSizeY / 2f);

            ActiveHero.ResetHero();
        }

        public void OnHeroDie(Health health)
        {
            GameManager.Instance.EndGame();
        }

        public void HeroLeftMap()
        {
            LocalMapLevelCompletedManager.Instance.CompleteLevel();
        }

        // Public method for persistence manager to clear hero state
        public void ClearHeroState()
        {
            Debug.Log($"HeroManager.ClearHeroState called. Current HeroSO: {(HeroSO != null ? HeroSO.name : "<null>")}. Caller stack:\n{Environment.StackTrace}");

            RemoveHero();
            Stats = null;
            HeroSO = null;
            statsBound = false;
            equipmentApplied = false;
        }

        [ContextMenu("Debug Spawn Hero")]
        private void DebugSpawnHero()
        {
            if (HeroSO == null || HeroSO.prefab == null)
            {
                Debug.LogError("DebugSpawnHero aborted: HeroSO or prefab is null.");
                return;
            }

            ActiveHero = Instantiate(HeroSO.prefab, new Vector2(0, 0), Quaternion.identity);

            ActiveHero.Setup(HeroSO, Stats);

            Debug.Log($"DebugSpawnHero: Hero spawned: {ActiveHero?.name}");
        }

        private void OnValidate()
        {
            // Ensure that HeroSO can be assigned in the inspector
            if (HeroSO == null)
            {
                Debug.LogWarning($"HeroManager.OnValidate: HeroSO is not assigned.", this);
            }
        }
    }
}
