using UnityEngine;

namespace Heros
{
    [RequireComponent(typeof(HeroMovement))]
    public class Hero : MonoBehaviour
    {
        public static Hero Instance { private set; get; }

        private HeroMovement hMovement;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            hMovement = GetComponent<HeroMovement>();
        }

    }
}

