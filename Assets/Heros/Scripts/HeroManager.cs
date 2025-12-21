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
        public HeroStatContainer Stats { get; private set; }

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
            Stats = new HeroStatContainer(heroSO);
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, SubscribeToHeroStatsManager);
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, SubscribeToHeroEquipmentManager);
        }

        private void SubscribeToHeroEquipmentManager()
        {
            HeroEquipmentManager.Instance.ApplyEquipmentBuffs();
        }

        private void SubscribeToHeroStatsManager()
        {
            HeroStatBuffsManager.Instance.Bind(Stats);
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

            ActiveHero.Setup(heroSO, Stats);

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
