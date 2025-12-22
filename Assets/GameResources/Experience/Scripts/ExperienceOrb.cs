using UnityEngine;
namespace LevelingSystem
{

    [RequireComponent(typeof(Collider2D))]
    public class ExperienceOrb : MonoBehaviour
    {
        private bool isActive;

        public int ExperienceAmount { get; private set; }

        private ExperienceOrbPool owningPool;
        private Bounce bounce;

        public void Initialize(int experienceAmount, ExperienceOrbPool pool)
        {
            ExperienceAmount = experienceAmount;
            owningPool = pool;
            gameObject.SetActive(true);
            isActive = true;
            if (TryGetComponent(out bounce))
            {
                bounce.ResetBounces();
            }
        }

        public void Release()
        {
            if (!isActive)
                return;

            isActive = false;
            owningPool.Release(this);
        }
    }

}