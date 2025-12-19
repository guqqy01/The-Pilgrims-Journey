using UnityEngine;
using GameManagerCalls;
using GlobalMap;
using LocalMap;
using LevelingSystem;
using Helper.Wait;
using Equipment;
namespace Heros
{
    public class HeroManager : Singleton<HeroManager>, IOnAfterCreateLocalMap
    {
        public HeroSO heroSO;
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

        private void Awake()
        {
            WaitForObjectHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, SubscribeToHeroStatsManager);
            WaitForObjectHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, SubscribeToHeroEquipmentManager);
        }

        private void SubscribeToHeroEquipmentManager()
        {
            HeroEquipmentManager.Inventory.onEquipmentChanged += UpdateHeroStatsFromEquipmentChange;
            UpdateHeroStats();
        }

        private void SubscribeToHeroStatsManager()
        {
            HeroStatsManager.buffsChanged += UpdateHeroStats;
            UpdateHeroStats();
        }

        private void OnDestroy()
        {
            if (HeroStatsManager != null)
            {
                HeroStatsManager.buffsChanged -= UpdateHeroStats;
            }

            if (HeroEquipmentManager != null)
            {
                HeroEquipmentManager.Inventory.onEquipmentChanged -= UpdateHeroStatsFromEquipmentChange;
            }
        }

        private void UpdateHeroStatsFromEquipmentChange(EquipmentContainer container, EquipmentSO from, EquipmentSO to)
        {
            if (ActiveHero == null)
                return;

            ActiveHero.Setup(heroSO);
        }

        public void UpdateHeroStats()
        {
            if (ActiveHero == null)
                return;

            ActiveHero.Setup(heroSO);
        }

        public void AfterCreateLocalMap()
        {
            RemoveHero();

            SpawnHero();
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
            Vector2 position = new Vector2(LocalMapGenerator.Instance.MapSizeX / 2f, LocalMapGenerator.Instance.MapSizeY / 2f);

            ActiveHero = Instantiate(heroSO.prefab, position, Quaternion.identity, transform);

            ActiveHero.Setup(heroSO);

            ActiveHero.HealthAndShieldManager.Health.onDie += OnHeroDie;
        }

        public void OnHeroDie(Health health)
        {
            GameManager.Instance.EndGame();
        }

        public void HeroLeftMap()
        {
            LocalMapLevelCompletedManager.Instance.CompleteLevel();
        }
    }

}
