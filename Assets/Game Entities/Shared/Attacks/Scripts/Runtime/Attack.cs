using Heros;
using UnityEngine;
namespace Attacks
{
    public abstract class Attack<TAttackSO, TAttackSpawn> : BaseAttack 
        where TAttackSO : BaseAttackSO
        where TAttackSpawn : AttackSpawnBase
    {
        private AttackSpawnPool attackPool;

        public TAttackSO AttackSO { get; private set; }
        private TAttackSpawn _attackPrefab;

        public override BaseAttackSO BaseAttackSO => AttackSO;

        protected abstract void InheritSetup(TAttackSO heroAttackSO);

        protected virtual TAttackSpawn GetAttackSpawnFromPool(Vector3 position, Quaternion rotation, Transform parent = null)
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

            AttackSpawnBase attackSpawn = attackPool.Get(position, rotation);

            if (attackSpawn == null)
                return null;

            if (parent != null)
                attackSpawn.transform.SetParent(parent);
            return (TAttackSpawn)attackSpawn;
        }

        public override void Setup<TBaseAttackSO>(TBaseAttackSO attackSO, LayerMask hitLayerMask)
        {
            AttackSO = attackSO as TAttackSO;
            _attackPrefab = (TAttackSpawn)attackSO.AttackSpawn;
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
