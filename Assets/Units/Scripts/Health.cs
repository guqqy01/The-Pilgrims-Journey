using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    internal class Health : BuffableIntStat
    {
        public int currentHealth { private set; get; } = 5;

        public delegate void OnDie();
        public OnDie onDie;

        public delegate void OnHealthChange(int currentHealth);
        public OnHealthChange onHealthChange;

        public void Init(UnitSO unitSO, TileStats tileStats)
        {
            base.Init(unitSO.baseHealth);
            AddBaseBuff("Tile", tileStats.additionalHealth);
            AddPercentBuff("Tile", tileStats.healthMultiplier);

            currentHealth = Value;
            onHealthChange?.Invoke(currentHealth);
        }

        public void ModifyHealth(int amount)
        {
            currentHealth = Mathf.Clamp(amount + currentHealth, 0, Value);

            onHealthChange?.Invoke(currentHealth);

            if (currentHealth == 0)
            {
                onDie?.Invoke();
                Destroy(gameObject);
            }

        }
    }

    internal abstract class  BuffableIntStat : MonoBehaviour
    {
        private int baseStat;
        public int Value { private set; get; }

        public Dictionary<string, int> BaseStatBuff;
        public Dictionary<string, float> PercentStatBuff;

        public delegate void OnBuffChange();
        public OnBuffChange onBuffChange;

        protected virtual void Init(int baseStat)
        {
            this.baseStat = baseStat;
            onBuffChange += RefreshValue;
        }

        public void AddBaseBuff(string reference, int buff)
        {
            if (BaseStatBuff.ContainsKey(reference))
            {
                if (buff > BaseStatBuff[reference])
                    BaseStatBuff[reference] = buff;
            }
            else
            {
                BaseStatBuff.Add(reference, buff);
            }

            onBuffChange?.Invoke();
        }

        public void AddBaseDebuff(string reference, int buff)
        {
            if (BaseStatBuff.ContainsKey(reference))
            {
                if (buff < BaseStatBuff[reference])
                    BaseStatBuff[reference] = buff;
            }
            else
            {
                BaseStatBuff.Add(reference, buff);
            }

            onBuffChange?.Invoke();
        }

        public void RemoveBaseBuff(string reference)
        {
            if (BaseStatBuff.ContainsKey(reference))
            {
                BaseStatBuff.Remove(reference);
                onBuffChange?.Invoke();
            }
        }

        public void AddPercentBuff(string reference, float buff)
        {
            if (PercentStatBuff.ContainsKey(reference))
            {
                PercentStatBuff[reference] = buff;
            }
            else
            {
                PercentStatBuff.Add(reference, buff);
            }

            onBuffChange?.Invoke();
        }

        public void RemovePercentBuff(string reference)
        {
            if (PercentStatBuff.ContainsKey(reference))
            {
                PercentStatBuff.Remove(reference);
                onBuffChange?.Invoke();
            }
        }

        private void RefreshValue()
        {
            float percentBuff = 1;
            int baseBuff = 0;

            foreach (KeyValuePair<string, float> kvp in PercentStatBuff)
            {
                percentBuff *= kvp.Value;
            }

            foreach (KeyValuePair<string, int> kvp in BaseStatBuff)
            {
                baseBuff += kvp.Value;
            }

            Value = Mathf.Max(Mathf.CeilToInt(baseStat * percentBuff) + baseBuff, 0);
        }
    }
}

