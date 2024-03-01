using UnityEngine;

namespace Units
{
    public class SpeedInNumbers : UnitSpecial
    {
        public float BaseSpeedBuff = 0.1f;
        public float SpeedLevelIncrement = 0.1f;
        public float Range = 2f;

        [SerializeField]
        private int numOfCloseUnitsNeededToGetBuff;
        [SerializeField]
        private LayerMask mask;

        private Movement movement;

        Rarity rarity;
        int level;

        public float SpeedBuff => (int)rarity * (BaseSpeedBuff + SpeedLevelIncrement * level);

        public override void Init(Rarity rarity, int level)
        {
            this.rarity = rarity;
            this.level = level;
        }

        private void Awake()
        {
            movement = GetComponent<Movement>();
        }

        private void Update()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Range, mask);
        
            if(colliders.Length >= numOfCloseUnitsNeededToGetBuff)
            {
                movement.speed.AddPercentBuff("SpeedInNumbersBuff", SpeedBuff);
            }
        }
    }

    public abstract class UnitSpecial : MonoBehaviour
    {
        public abstract void Init(Rarity rarity, int level);
    }
}

