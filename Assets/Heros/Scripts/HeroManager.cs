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
using static Heros.HeroManager;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Heros
{
    public class HeroManager : Singleton<HeroManager>, IOnAfterCreateLocalMap, IOnClear
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

        public delegate void OnStatsSetup(HeroStatContainer stats);
        public OnStatsSetup onStatsSetup;

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

        public void SelectHero(HeroSO heroSO)
        {
            this.HeroSO = heroSO;

            // reset binding flags when selecting a new hero
            statsBound = false;
            equipmentApplied = false;

            CreateNewStats();

            WaitForHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, SubscribeToHeroStatsManager);
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, SubscribeToHeroEquipmentManager);
        }

        public void AfterCreateLocalMap()
        {
            if (ActiveHero == null)
            {
                // If we've already selected a hero, ensure stats and equipment subscriptions are applied before spawning
                if (HeroSO != null)
                {
                    // Ensure Stats exists
                    if (Stats == null)
                    {
                        CreateNewStats();
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
                    WaitForHelper.WaitForObjectThenPerformAction(() => HeroSO, () => HandleSpawnHero());
                }
            }
            else
            {
                ResetHero();
            }
        }

        private void CreateNewStats()
        {
            Stats = new HeroStatContainer(HeroSO);
            Stats.EnsureDefaults(HeroSO);
            onStatsSetup?.Invoke(Stats);
        }

        private void HandleSpawnHero()
        {
            SpawnHero();

            // Wait for full readiness: hero instance, stats bound and attack manager available
            WaitForHelper.WaitForConditionThenPerformAction(() => 
                HeroAttacksManager.Instance != null &&
                ActiveHero != null &&
                Stats != null &&
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
            if (Stats == null)
            {
                WaitForHelper.WaitForConditionThenPerformAction(() =>
                    Stats != null,
                    SpawnHero);
                return;
            }

            if (HeroSO == null || HeroSO.prefab == null)
            {
                Debug.LogError("HeroManager.SpawnHero aborted: HeroSO or prefab is null.");
                return;
            }

            Vector2 position = new Vector2(LocalMapGenerator.Instance.MapSizeX / 2f, LocalMapGenerator.Instance.MapSizeY / 2f);

            if (!LocalMapGenerator.Instance.IsPositionWalkable(position))
            {
                if (LocalMapGenerator.Instance.TryFindNearestWalkable(position, out Vector2 walkablePositon))
                {
                    position = walkablePositon;
                    Debug.Log($"found new walkable position {position}");
                }
                else
                {
                    Debug.LogError("Could not find walkable spot");
                    return;
                }
            }
            else
            {
                Debug.Log("Is walkable");
            }

                ActiveHero = Instantiate(HeroSO.prefab, position, Quaternion.identity, transform);

            ActiveHero.Setup(HeroSO, Stats);

            ActiveHero.HealthAndShieldManager.Health.onDie += OnHeroDie;

            onHeroSpawned?.Invoke(ActiveHero);
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

        public void Clear()
        {
            RemoveHero();
            Stats = null;
            HeroSO = null;
            statsBound = false;
            equipmentApplied = false;
        }
    }
}
