using UnityEngine;
namespace LevelingSystem
{

    [RequireComponent(typeof(Collider2D))]
    public class ExperienceOrb : MonoBehaviour
    {
        private bool isActive;
        private float spawnTime;
        private bool isSettled;

        public int ExperienceAmount { get; private set; }
        public bool CanMerge => gameObject.activeInHierarchy && isActive && isSettled && Time.time - spawnTime >= minTimeBeforeMerge;

        private ExperienceOrbPool owningPool;
        private Bounce bounce;

        private const float minTimeBeforeMerge = 0.75f;

        public System.Action<ExperienceOrb> OnReleased;

        public Collider2D Collider2D { get; private set; }

        private void Awake()
        {
            Collider2D = GetComponent<Collider2D>();
        }

        public void Initialize(int experienceAmount, ExperienceOrbPool pool)
        {
            OnReleased = null;
            ExperienceAmount = experienceAmount;
            owningPool = pool;
            spawnTime = Time.time;
            isSettled = false;
            isActive = true;

            gameObject.SetActive(true);

            if (TryGetComponent(out bounce))
            {
                bounce.ResetBounces();
                bounce.OnSettled = OnSettled;
            }
            else
            {
                isSettled = true;
            }
        }

        private void OnSettled()
        {
            isSettled = true;
        }

        public void Release()
        {
            if (!isActive)
                return;

            isActive = false;
            OnReleased?.Invoke(this);
            OnReleased = null;

            owningPool.Release(this);
        }
    }

}