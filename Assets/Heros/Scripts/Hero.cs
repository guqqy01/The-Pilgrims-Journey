using Equipment;
using UnityEngine;

namespace Heros
{
    [RequireComponent(typeof(HeroMovement), typeof(HealthAndShieldManager), typeof(HeroAttacksManager))]
    public class Hero : MonoBehaviour
    {
        public HeroSO HeroSO {  get; private set; }

        public HeroMovement HeroMovement { private set; get; }
        public HealthAndShieldManager HealthAndShieldManager {  private set; get; }
        public HeroAttacksManager HeroAttacksManager { private set; get; }

        private void Awake()
        {
            HeroMovement = GetComponent<HeroMovement>();
            HealthAndShieldManager = GetComponent<HealthAndShieldManager>();
            HeroAttacksManager = GetComponent<HeroAttacksManager>();
        }

        public void Setup(HeroSO heroSO)
        {
            HeroSO = heroSO;

            HeroMovement.Setup(heroSO);

            HealthAndShieldManager.Setup(HeroStatBuffsManager.Instance.MaxHealth, HeroStatBuffsManager.Instance.Shield, HeroStatBuffsManager.Instance.ShieldRegeneration, HeroStatBuffsManager.Instance.ShieldCooldown);
        }

    }
}

