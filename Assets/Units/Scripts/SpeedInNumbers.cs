using UnityEngine;

namespace Units
{
    public class SpeedInNumbers : MonoBehaviour
    {
        public float SpeedBuff = 0.1f;
        public float Range = 2f;

        [SerializeField]
        private int numOfCloseUnitsNeededToGetBuff;
        [SerializeField]
        private LayerMask mask;

        private Movement movement;

        private void Awake()
        {
            movement = GetComponent<Movement>();
        }

        private void Update()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Range, mask);
        
            if(colliders.Length >= numOfCloseUnitsNeededToGetBuff)
            {
                movement.speed.AddMultiplierBuff("SpeedInNumbersBuff", SpeedBuff);
            }
            else
            {
                movement.speed.RemoveMultiplierBuff("SpeedInNumbersBuff");
            }
        }
    }
}

