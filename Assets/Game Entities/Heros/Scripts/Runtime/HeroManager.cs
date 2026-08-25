using GameManagerCalls;
using Helper.Wait;
using Attacks;
using LocalMap;
using UnityEngine;

namespace Heros
{
    public class HeroManager : Singleton<HeroManager>, IOnAfterCreateLocalMap, IOnClear
    {
        [SerializeField]
        private StatContainer stats;
        public StatContainer Stats => stats;

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

        public delegate void OnStatsSetup(StatContainer stats);
        public OnStatsSetup onStatsSetup;

        private enum HeroInitState { NotStarted, Initializing, Ready }
        private HeroInitState initState = HeroInitState.NotStarted;
        private bool statsBound;
        private bool equipmentApplied;

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

        public void SelectHero(HeroSO heroSO)
        {
            HeroSO = heroSO;
            initState = HeroInitState.Initializing;
            statsBound = false;
            equipmentApplied = false;

            SetupStats();

            HeroStatBuffsManager.WhenReady(_ => { SubscribeToHeroStatsManager(); statsBound = true; TryAdvance(); });
            HeroEquipmentManager.WhenReady(_ => { SubscribeToHeroEquipmentManager(); equipmentApplied = true; TryAdvance(); });
        }

        private void TryAdvance()
        {
            if (initState != HeroInitState.Initializing || !statsBound || !equipmentApplied)
                return;

            initState = HeroInitState.Ready;
        }

        public void AfterCreateLocalMap()
        {
            if (ActiveHero != null) { ResetHero(); return; }
            if (initState == HeroInitState.Ready) HandleSpawnHero();
            else WaitForHelper.WaitForConditionThenPerformAction(() => initState == HeroInitState.Ready, HandleSpawnHero);
        }

        private void SetupStats()
        {
            Stats.Setup(HeroSO);
            onStatsSetup?.Invoke(Stats);
        }

        private void HandleSpawnHero()
        {
            SpawnHero();
            HeroAttacksManager.WhenReady(_ => HeroAttacksManager.Instance.AddAttack(HeroSO.startingAttack));
        }

        private Buff GetBuff(StatTypes statType)
        {
            return Stats.Get(statType);
        }

        private void RemoveHero()
        {
            if (ActiveHero != null)
            {
                ActiveHero.HealthAndShieldManager.Health.onDie -= OnHeroDie;
                Destroy(ActiveHero.gameObject);
            }

            ActiveHero = null;
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

            if (LocalMapGenerator.Instance.TryFindNearestWalkable(position, out Vector2 walkablePositon))
            {
                position = walkablePositon;
            }
            else
            {
                Debug.LogError("Could not find walkable spot");
                return;
            }

            ActiveHero = Instantiate(HeroSO.prefab, position, Quaternion.identity, transform);

            ActiveHero.Setup(HeroSO, Stats);

            ActiveHero.HealthAndShieldManager.Health.onDie += OnHeroDie;

            onHeroSpawned?.Invoke(ActiveHero);
        }

        private void ResetHero()
        {
            Vector2 position = new Vector2(LocalMapGenerator.Instance.MapSizeX / 2f, LocalMapGenerator.Instance.MapSizeY / 2f);

            if (LocalMapGenerator.Instance.TryFindNearestWalkable(position, out Vector2 walkablePositon))
            {
                position = walkablePositon;
            }

            ActiveHero.transform.position = position;

            ActiveHero.ResetHero();
        }

        public void OnHeroDie(Health health)
        {
            GameManager.Instance.EndGame("Your Hero has perished");
        }

        public void Clear()
        {
            RemoveHero();
            HeroSO = null;
            statsBound = false;
            equipmentApplied = false;
        }
    }
}
