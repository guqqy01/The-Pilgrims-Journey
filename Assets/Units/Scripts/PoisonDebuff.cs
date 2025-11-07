using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using Monsters;
using UnityEditor.PackageManager;

namespace Units
{
    public class PoisonDebuff : MonoBehaviour
    {
        private Unit unit;

        private List<Poison> poisons = new List<Poison>();

        public delegate void OnPoisoned();
        public OnPoisoned onPoisoned;

        public delegate void OnCured();
        public OnCured onCured;

        private float damageAmount;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        private void Update()
        {
            DeleteFinishedPoisons();

            float damageOverTime = 0;

            foreach (Poison item in poisons)
            {
                damageOverTime += item.DamageOverTime;
                item.CheckDamage(unit);
            }

            damageAmount += damageOverTime * Time.deltaTime;

            int amountToDamage = Mathf.FloorToInt(damageAmount);

            if (amountToDamage >= 1)
            {
                unit.Health.ModifyHealth(-amountToDamage);
                damageAmount -= amountToDamage;

                if (unit.Health.CurrentHealth <= 0)
                {
                    MonsterSO returnMonster;
                    WeightedChanceList<MonsterSO> weightedChanceList = new WeightedChanceList<MonsterSO>();

                    foreach (Poison item in poisons)
                    {
                        weightedChanceList.Add(item.monster.MonsterSO, Mathf.RoundToInt(item.DamageOverTime * 10f));
                    }

                    returnMonster = weightedChanceList.GetRandom();
                    BeastaryManager.Instance.Unitdamaged(returnMonster, new OnHitInfo() { unit = unit, killed = true });
                }
            }
        }

        public void Add(Monster monster, int damage, float time)
        {
            poisons.Add(new Poison(monster, damage, time));

            if (poisons.Count == 1)
            {
                onPoisoned?.Invoke();
            }

            if (!unit.PoisonPS.isPlaying)
            {
                unit.PoisonPS.Play();
            }

            var emmision = unit.PoisonPS.emission;
            emmision.rateOverTime = poisons.Count;
        }

        private void DeleteFinishedPoisons()
        {
            List<Poison> toDelete = new List<Poison>();

            foreach (var item in poisons)
            {
                if (item == null || Time.time >= item.timeToRemove)
                    toDelete.Add(item);
            }

            if (toDelete.Count <= 0)
                return;

            foreach (var item in toDelete)
            {
                poisons.Remove(item);
            }

            if (poisons.Count <= 0)
            {
                unit.PoisonPS.Stop();
                onCured?.Invoke();
            }
            else
            {
                var emmision = unit.PoisonPS.emission;
                emmision.rateOverTime = poisons.Count;
            }
        }

        private class Poison
        {
            public int damage;
            public float time;
            public float timeToRemove;
            public Monster monster;

            private float currentDamageAmount;

            public float DamageOverTime => time != 0 ? damage / time : 0f;

            public Poison(Monster monster, int damage, float time)
            {
                this.monster = monster;
                this.damage = damage;
                this.time = time;
                timeToRemove = Time.time + time;
            }

            public void CheckDamage(Unit unit)
            {
                currentDamageAmount += DamageOverTime * Time.deltaTime;

                int amountToDamage = Mathf.FloorToInt(currentDamageAmount);

                if (amountToDamage >= 1)
                {
                    BeastaryManager.Instance.Unitdamaged(monster.MonsterSO, new OnHitInfo() { unit = unit, dmgToHealth = amountToDamage });
                    currentDamageAmount -= amountToDamage;
                }
            }
        }

    }
}

