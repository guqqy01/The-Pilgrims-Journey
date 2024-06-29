using Map;
using System.Collections.Generic;
using Units;
using UnityEngine;


namespace Monsters
{
    public abstract class Monster : MonoBehaviour
    {
        private BeastaryManager beastaryManager;

        public MonsterSO MonsterSO { private set; get; }
        public Material MonsterMat {  get; private set; }

        [SerializeField]
        protected float range = 3f;
        public LayerMask targetMask;
        [SerializeField]
        protected Animator ani;

        public bool Shocked { private set; get; }
        private const float percentageOfRemainingTimeToStartFlickering = 0.3f;
        private const float percentageOfRemainingTimeToChangeFlickerState = 0.2f;
        private static Color shockedColor = Color.yellow;
        protected float removeShockTime;
        private float timeToStartFlickering;
        private float timeToChangeFlickerColor;
        private bool flickerOnOrOff;

        protected virtual void Start()
        {
            beastaryManager = BeastaryManager.Instance;
            MonsterMat = GetComponentInChildren<Renderer>().material;
        }

        public virtual void Init(MonsterSO monster)
        {
            MonsterSO = monster;
        }
        
        protected virtual void OnDamageUnit(OnHitInfo hitInfo)
        {
            beastaryManager.Unitdamaged(MonsterSO, hitInfo);
        }

        private void Flicker()
        {
            if (Time.time >= timeToChangeFlickerColor)
            {
                MonsterMat.SetColor("_Tint", flickerOnOrOff ? shockedColor : Color.white);
                timeToChangeFlickerColor = Time.time + (removeShockTime - Time.time) * percentageOfRemainingTimeToChangeFlickerState;
                flickerOnOrOff = !flickerOnOrOff;
            }
        }

        public Transform GetClosestTargetInRange()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

            float closestDistance = float.MaxValue;
            Transform closestTarget = null;

            foreach (Collider2D collider in colliders)
            {
                float distance = Vector2.Distance(collider.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }

            return closestTarget;
        }

        public Transform GetClosestTargetInRange(List<Transform> transforms)
        {
            float closestDistance = range;
            Transform closestTarget = null;

            foreach (Transform transf in transforms)
            {
                if (transf == null)
                    continue;

                float distance = Vector2.Distance(transf.position, this.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = transf;
                }
            }

            if(closestTarget == null)
                return GetClosestTargetInRange();

            return closestTarget;
        }

        protected virtual void Update()
        {
            if (Time.time >= removeShockTime)
            {
                RemoveShock();
            }
            else if (Time.time >= removeShockTime - timeToStartFlickering)
            {
                Flicker();
            }
        }

        public void ShockMonster(float shockTime)
        {
            ShockEffect(shockTime);

            Shocked = true;

            timeToStartFlickering = shockTime * percentageOfRemainingTimeToStartFlickering;

            removeShockTime = Time.time + shockTime;

            MonsterMat.SetFloat("_EffectAmount", 1);
            MonsterMat.SetColor("_Tint", shockedColor);

            ani.speed = 0f;
        }

        private void RemoveShock()
        {
            Shocked = false;
            if(ani != null)
                ani.speed = 1f;
            MonsterMat.SetFloat("_EffectAmount", 0);
            MonsterMat.SetColor("_Tint", Color.white);
        }

        protected abstract void ShockEffect(float shockTime);

    }
}