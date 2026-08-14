using Heros;
using UnityEngine;
namespace Attacks
{
    public abstract class Attack<TAttackSO, TAttackInit, TAttackStats> : BaseAttack 
        where TAttackSO : BaseAttackSO
        where TAttackInit : AttackInit
        where TAttackStats : AttackStats
    {
        private AttackSpawnPool attackPool;

        public TAttackSO AttackSO { get; private set; }
        private AttackSpawnBase _attackPrefab;

        public override BaseAttackSO BaseAttackSO => AttackSO;

        protected abstract void InheritSetup(TAttackSO heroAttackSO);

        protected virtual TAttackSpawn GetAttackSpawnFromPoolAndSetup<TAttackSpawn>(TAttackInit attackInit)
            where TAttackSpawn : AttackSpawnBase
        {
            if (attackPool == null)
            {
                attackPool = AttackSpawnPoolManager.GetPool(_attackPrefab, GetOrCreatePoolParent());
            }

            if(attackPool == null)
            {
                Debug.LogError("Attack pool is null. Make sure to call Setup() before trying to get an attack spawn from the pool.");
                return null;
            }

            AttackSpawnBase attackSpawn = attackPool.Get(attackInit.Position, attackInit.Rotation);
            if (attackSpawn == null)
                return null;

            if (attackInit.Parent != null)
                attackSpawn.transform.SetParent(attackInit.Parent);

            if (attackSpawn is TAttackSpawn typedAttackSpawn)
            {
                TAttackStats attackStats = GetAttackStats();
                typedAttackSpawn.Init(attackInit, attackStats);

                string reference = "baseSize";

                size.AddBuff(reference, new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, 0f, attackSpawn.transform.localScale.x, 0));
                float newSizeX = size;
                size.RemoveBuff(reference);

                size.AddBuff(reference, new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, 0f, attackSpawn.transform.localScale.y, 0));
                float newSizeY = size;
                size.RemoveBuff(reference);

                attackSpawn.transform.localScale = new Vector3(newSizeX, newSizeY, attackSpawn.transform.localScale.z);

                return typedAttackSpawn;
            }
            else
            {
                Debug.LogError($"Attack spawn is not of type {typeof(TAttackSpawn).Name}. Make sure the attack prefab is of the correct type.");
                attackPool.Release(attackSpawn);
                return null;
            }
        }

        protected abstract TAttackStats GetAttackStats();
        public override void Setup<TBaseAttackSO>(TBaseAttackSO attackSO, LayerMask hitLayerMask)
        {
            AttackSO = attackSO as TAttackSO;
            _attackPrefab = attackSO.AttackSpawn;
            Level = 1;

            SetupBuffableStat(ref amountOfHits, StatTypes.AttackDurability);
            SetupBuffableStat(ref damage, StatTypes.Damage);
            SetupBuffableStat(ref attackSpeed, StatTypes.AttackSpeed);
            SetupBuffableStat(ref criticalChance, StatTypes.CriticalChance);
            SetupBuffableStat(ref criticalDamageMultiplier, StatTypes.CriticalDamageMultiplier);
            SetupBuffableStat(ref knockback, StatTypes.Knockback);
            SetupBuffableStat(ref size, StatTypes.AttackSize);

            findTargetDistance = attackSO.findTargetDistance;

            this.hitLayerMask = hitLayerMask;

            InheritSetup(AttackSO);

            if (attackPool == null)
            {
                attackPool = AttackSpawnPoolManager.GetPool(_attackPrefab, GetOrCreatePoolParent());
            }
        }

        protected void SetupBuffableStat(ref BuffableStat stat, StatTypes heroStats, AddBuffParameters addBuffParameters = AddBuffParameters.All)
        {
            stat = new BuffableStat(heroStats.IsIntStat() ? 0 : 0f);
            stat.AddBuff("Weapon", AttackSO.GetBuff(heroStats, Level), addBuffParameters);
            stat.AddBuff("Hero", HeroManager.Instance.Stats.Get(heroStats), addBuffParameters);
        }

        private Transform GetOrCreatePoolParent()
        {
            GameObject poolParent = GameObject.Find("Pools/" + _attackPrefab.name + " Pool");
            if (poolParent != null) return poolParent.transform;

            GameObject poolsParent = GameObject.Find("Pools");
            if (poolsParent == null)
            {
                poolsParent = new GameObject("Pools");
            }

            poolParent = GameObject.Find("Pools/" + _attackPrefab.name + "Pool");
            if (poolParent == null)
            {
                poolParent = new GameObject(_attackPrefab.name + "Pool");
                poolParent.transform.SetParent(poolsParent.transform);
            }

            return poolParent.transform;
        }
    }
}
