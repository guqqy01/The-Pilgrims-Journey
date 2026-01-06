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

        private void SubscribeToHeroEquipmentManager()
        {
            HeroEquipmentManager.Instance.ApplyEquipmentBuffs();
        }

        private void SubscribeToHeroStatsManager()
        {
            HeroStatBuffsManager.Instance.Bind(Stats);
        }

        private void SetupHeroAttack()
        {
            HeroAttacksManager.Instance.AddHeroAttack(HeroSO.startingAttack);
        }

        public void SelectHero(HeroSO heroSO)
        {
            this.HeroSO = heroSO;
            
            Stats = new HeroStatContainer(HeroSO);
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, SubscribeToHeroStatsManager);
            WaitForHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, SubscribeToHeroEquipmentManager);
        }

        public void AfterCreateLocalMap()
        {   
            if(ActiveHero == null)
            {
                SpawnHero();
                WaitForHelper.WaitForObjectThenPerformAction(() => HeroAttacksManager.Instance, SetupHeroAttack);
            }
            else 
            {
                ResetHero();
            }
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

        public void HeroLeftMap()
        {
            LocalMapLevelCompletedManager.Instance.CompleteLevel();
        }
    }

}
