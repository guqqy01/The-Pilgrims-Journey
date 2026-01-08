using Heros;
using UnityEngine;
namespace Monsters
{
    public class SkullSlime : Monster
    {
        string hasShieldString = "HasShield";

        protected override void Start()
        {
            HealthAndShieldManager.Shield.onDestroy += OnShieldDestroyed;
            HealthAndShieldManager.Shield.onShieldRestored += OnShieldRestored;

            ani.SetBool(hasShieldString, true);
        }

        private void OnDestroy()
        {
            if (HealthAndShieldManager != null && HealthAndShieldManager.Shield != null)
            {
                HealthAndShieldManager.Shield.onDestroy -= OnShieldDestroyed;
                HealthAndShieldManager.Shield.onShieldRestored -= OnShieldRestored;
            }
        }

        protected override void Update()
        {
            base.Update();

            ani.SetBool("Moving", MonsterMovement.HeroMovement != null);
        }

        private void OnShieldDestroyed()
        {
            ani.SetBool(hasShieldString, false);

            MonsterMovement.Speed.AddBuff("Exposed", new Buff(-100f, 0f, 0f));
        }

        private void OnShieldRestored()
        {
            ani.SetBool(hasShieldString, true);

            MonsterMovement.Speed.RemoveBuff("Exposed");
        }

        protected override void ShockEffect(float shockTime)
        {
            throw new System.NotImplementedException();
        }
    }
}

