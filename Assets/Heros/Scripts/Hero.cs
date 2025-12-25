using Equipment;
using UnityEngine;

namespace Heros
{
    [RequireComponent(typeof(HeroMovement), typeof(HealthAndShieldManager))]
    public class Hero : MonoBehaviour
    {
        public HeroSO HeroSO {  get; private set; }

        public HeroMovement HeroMovement { private set; get; }
        public HealthAndShieldManager HealthAndShieldManager {  private set; get; }

        private void Awake()
        {
            HeroMovement = GetComponent<HeroMovement>();
            HealthAndShieldManager = GetComponent<HealthAndShieldManager>();
        }

        public void Setup(HeroSO heroSO, HeroStatContainer stats)
        {
            HeroSO = heroSO;

            HeroMovement.Setup(heroSO);

            HealthAndShieldManager.Setup(stats.Get(HeroStats.Health), 
                stats.Get(HeroStats.Shield), 
                stats.Get(HeroStats.ShieldRegeneration), 
                stats.Get(HeroStats.ShieldCooldown));
        }

        public void ResetHero()
        {
            HealthAndShieldManager.Health.ModifyHealth(HealthAndShieldManager.Health.MaxHealth - HealthAndShieldManager.Health.CurrentHealth);
            HeroMovement.ResetMovement();
        }

    }
}

