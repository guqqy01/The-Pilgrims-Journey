using Equipment;
using UnityEngine;
using UnityEngine.U2D.Animation;
namespace Heros
{
    [RequireComponent(typeof(HeroMovement), typeof(HealthAndShieldManager), typeof(SpriteLibrary))]
    public class Hero : MonoBehaviour
    {
        public HeroSO HeroSO {  get; private set; }

        public HeroMovement HeroMovement { private set; get; }
        public HealthAndShieldManager HealthAndShieldManager {  private set; get; }

        public SpriteLibrary SpriteLibrary { private set; get; }

        private void Awake()
        {
            HeroMovement = GetComponent<HeroMovement>();
            HealthAndShieldManager = GetComponent<HealthAndShieldManager>();
            SpriteLibrary = GetComponent<SpriteLibrary>();
        }

        public void Setup(HeroSO heroSO, StatContainer heroStats)
        {
            HeroSO = heroSO;

            HeroMovement.Setup(heroSO);

            HealthAndShieldManager.Setup(
                heroStats.Get(StatTypes.Health), 
                heroStats.Get(StatTypes.Shield), 
                heroStats.Get(StatTypes.ShieldRegeneration), 
                heroStats.Get(StatTypes.ShieldCooldown));

            SpriteLibrary.spriteLibraryAsset = heroSO.spriteLibrary;
        }

        public void ResetHero()
        {
            HealthAndShieldManager.Health.ModifyHealth(HealthAndShieldManager.Health.MaxHealth - HealthAndShieldManager.Health.CurrentHealth);
            HeroMovement.ResetMovement();
        }

    }
}

