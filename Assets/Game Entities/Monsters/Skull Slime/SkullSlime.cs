using Heros;
using UnityEngine;
namespace Monsters
{
    public class SkullSlime : Monster
    {
        private static readonly int HasShieldHash = Animator.StringToHash("HasShield");
        private static readonly int MovingHash = Animator.StringToHash("Moving");

        private bool isMoving;

        protected override void Start()
        {
            HealthAndShieldManager.Shield.onShieldDestroyed += OnShieldDestroyed;
            HealthAndShieldManager.Shield.onShieldRestored += OnShieldRestored;

            animator.SetBool(HasShieldHash, true);
        }

        private void OnDestroy()
        {
            if (HealthAndShieldManager != null && HealthAndShieldManager.Shield != null)
            {
                HealthAndShieldManager.Shield.onShieldDestroyed -= OnShieldDestroyed;
                HealthAndShieldManager.Shield.onShieldRestored -= OnShieldRestored;
            }
        }

        protected override void Update()
        {
            base.Update();

            bool shouldMove = HeroManager.Instance.ActiveHero != null;
            if (shouldMove != isMoving)
            {
                isMoving = shouldMove;
                animator.SetBool(MovingHash, isMoving);
            }
        }
        private void OnShieldDestroyed()
        {
            animator.SetBool(HasShieldHash, false);

            MonsterMovement.Speed.AddBuff("Exposed", new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, -100f, 0f, 0f));
        }

        private void OnShieldRestored()
        {
            animator.SetBool(HasShieldHash, true);

            MonsterMovement.Speed.RemoveBuff("Exposed");
        }

        protected override void ShockEffect(float shockTime)
        {
            throw new System.NotImplementedException();
        }
    }
}

