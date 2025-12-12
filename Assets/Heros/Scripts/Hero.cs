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

        public void Setup(HeroSO heroSO)
        {
            HeroSO = heroSO;

            HeroMovement.Setup(heroSO);

            HealthAndShieldManager.Health.Init(HeroStatBase.GetBuffableStat(heroSO, HeroStats.Health));

            BuffableStat shieldStat = HeroStatBase.GetBuffableStat(heroSO, HeroStats.Shield);
            BuffableStat shieldCooldownStat = HeroStatBase.GetBuffableStat(heroSO, HeroStats.ShieldCooldown);
            BuffableStat shieldRegenerationStat = HeroStatBase.GetBuffableStat(heroSO, HeroStats.ShieldRegeneration);

            HealthAndShieldManager.Shield.Init(shieldStat, shieldRegenerationStat, shieldCooldownStat);
        }
    }
}

