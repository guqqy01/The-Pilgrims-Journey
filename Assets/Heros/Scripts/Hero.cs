using Equipment;
using UnityEngine;

namespace Heros
{
    [RequireComponent(typeof(HeroMovement), typeof(HealthAndShieldManager))]
    public class Hero : MonoBehaviour
    {
        public HeroMovement HeroMovement { private set; get; }
        public HealthAndShieldManager HealthAndShieldManager {  private set; get; }

        private void Awake()
        {
            HeroMovement = GetComponent<HeroMovement>();
            HealthAndShieldManager = GetComponent<HealthAndShieldManager>();
        }

        public void Setup()
        {

        }
    }
}

