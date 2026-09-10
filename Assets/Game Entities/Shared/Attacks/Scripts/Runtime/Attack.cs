using Heros;
using System;
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

        protected abstract void InheritSetup(TAttackSO heroAttackSO, Func<StatTypes, Buff> entityBuffCallback);

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

                size.AddBuff(reference, new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, 0f, _attackPrefab.transform.localScale.x, 0));
                float newSizeX = size;
                size.RemoveBuff(reference);

                size.AddBuff(reference, new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, 0f, _attackPrefab.transform.localScale.y, 0));
                float newSizeY = size;
                size.RemoveBuff(reference);

                attackSpawn.transform.localScale = new Vector3(newSizeX, newSizeY, _attackPrefab.transform.localScale.z);

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
        public override void Setup<TBaseAttackSO>(TBaseAttackSO attackSO, Func<StatTypes, Buff> entityBuffCallback, LayerMask hitLayerMask)
        {
            AttackSO = attackSO as TAttackSO;
            _attackPrefab = attackSO.AttackSpawn;
            Level = 1;

            Buff durabilityBuff = AttackSO.GetBuff(StatTypes.AttackDurability, Level);

            if (durabilityBuff != null)
                SetupBuffableStat(ref amountOfHits, entityBuffCallback, StatTypes.AttackDurability);

            SetupBuffableStat(ref damage, entityBuffCallback, StatTypes.Damage);
            SetupBuffableStat(ref attackSpeed, entityBuffCallback, StatTypes.AttackSpeed);
            SetupBuffableStat(ref criticalChance, entityBuffCallback, StatTypes.CriticalChance);
            SetupBuffableStat(ref criticalDamageMultiplier, entityBuffCallback, StatTypes.CriticalDamageMultiplier);
            SetupBuffableStat(ref knockback, entityBuffCallback, StatTypes.Knockback);
            SetupBuffableStat(ref size, entityBuffCallback, StatTypes.AttackSize);

            findTargetDistance = attackSO.findTargetDistance;

            this.hitLayerMask = hitLayerMask;

            InheritSetup(AttackSO, entityBuffCallback);

            if (attackPool == null)
            {
                attackPool = AttackSpawnPoolManager.GetPool(_attackPrefab, GetOrCreatePoolParent());
            }
        }

        protected void SetupBuffableStat(ref BuffableStat buffableStat, Func<StatTypes, Buff> EntityStatBuffCall, StatTypes statType, AddBuffParameters addBuffParameters = AddBuffParameters.All)
        {
            buffableStat = new BuffableStat(statType.IsIntStat() ? 0 : 0f);
            buffableStat.AddBuff("Weapon", AttackSO.GetBuff(statType, Level), addBuffParameters);
            buffableStat.AddBuff("Entity", EntityStatBuffCall?.Invoke(statType), addBuffParameters);
            //stat.AddBuff("Hero", HeroManager.Instance.Stats.Get(heroStats), addBuffParameters);
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
