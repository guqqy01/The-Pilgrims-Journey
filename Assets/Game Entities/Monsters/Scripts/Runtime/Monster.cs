using GlobalMap;
using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.Serialization;


namespace Monsters
{
    [RequireComponent(typeof(MonsterMovement), typeof(HealthAndShieldManager))]
    public abstract class Monster : MonoBehaviour
    {
        private BeastaryManager beastaryManager;

        public MonsterSO MonsterSO { private set; get; }
        public Material MonsterMat {  get; private set; }

        [SerializeField]
        [FormerlySerializedAs("range")]
        protected float rangeToFindTarget = 3f;
        [FormerlySerializedAs("targetMask")]
        public LayerMask findTargetMask;
        [SerializeField]
        [FormerlySerializedAs("ani")]
        protected Animator animator;

        public bool Shocked { private set; get; }
        private const float percentageOfRemainingTimeToStartFlickering = 0.3f;
        private const float percentageOfRemainingTimeToChangeFlickerState = 0.2f;
        private static Color shockedColor = Color.yellow;
        protected float removeShockTime;
        private float timeToStartFlickering;
        private float timeToChangeFlickerColor;
        private bool flickerOnOrOff;


        private MonsterMovement monsterMovement;
        public MonsterMovement MonsterMovement
        {
            get
            {
                monsterMovement ??= GetComponent<MonsterMovement>();

                return monsterMovement;
            }
        }

        private HealthAndShieldManager healthAndShieldManager;
        public HealthAndShieldManager HealthAndShieldManager
        {
            get
            {
                healthAndShieldManager ??= GetComponent<HealthAndShieldManager>();

                return healthAndShieldManager;
            }
        }

        protected virtual void Start()
        {
            beastaryManager = BeastaryManager.Instance;
            MonsterMat = GetComponentInChildren<Renderer>().material;
        }

        public virtual void Init(MonsterSO monster, float difficulty = 1f)
        {
            MonsterSO = monster;

            MonsterMovement.Setup(monster);

            int scaledHealth = Mathf.RoundToInt(monster.health * (1f + (monster.healthDifficultyScaling - 1f) * (difficulty - 1f)));
            int scaledShield = Mathf.RoundToInt(monster.shield * (1f + (monster.shieldDifficultyScaling - 1f) * (difficulty - 1f)));
            int scaledShieldRegeneration = Mathf.RoundToInt(monster.shieldRegeneration * (1f + (monster.shieldRegenerationDifficultyScaling - 1f) * (difficulty - 1f)));
            int scaledShieldCooldown = Mathf.RoundToInt(monster.shieldCooldown * (1f + (monster.shieldCooldownDifficultyScaling - 1f) * (difficulty - 1f)));

            HealthAndShieldManager.Setup(new BuffableStat(monster.health), new BuffableStat(monster.shield), new BuffableStat(monster.shieldRegeneration), new BuffableStat(monster.shieldCooldown));
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
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, rangeToFindTarget, findTargetMask);

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
            float closestDistance = rangeToFindTarget;
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

            animator.speed = 0f;
        }

        private void RemoveShock()
        {
            Shocked = false;
            if(animator != null)
                animator.speed = 1f;

            if(MonsterMat != null)
            {
                MonsterMat.SetFloat("_EffectAmount", 0);
                MonsterMat.SetColor("_Tint", Color.white);
            }
        }

        protected abstract void ShockEffect(float shockTime);
    }
}