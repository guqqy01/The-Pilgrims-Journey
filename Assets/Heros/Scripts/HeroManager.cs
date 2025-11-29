using UnityEngine;
using GameManagerCalls;
using GlobalMap;
using LocalMap;
 
namespace Heros
{
    public class HeroManager : Singleton<HeroManager>, IOnAfterCreateLocalMap
    {
        public HeroSO heroSO;

        public Hero ActiveHero { private set; get; }


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

            ActiveHero.Setup();

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
