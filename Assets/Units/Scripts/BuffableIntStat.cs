using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public struct  BuffableIntStat
    {
        private int baseStat;
        public int Value { private set; get; }

        private Dictionary<string, int> BaseStatBuff;
        private Dictionary<string, float> MultiplierStatBuff;

        public BuffableIntStat(int baseStat)
        {
            this.baseStat = baseStat;
            BaseStatBuff = new Dictionary<string, int>();
            MultiplierStatBuff = new Dictionary<string, float>();
            Value = baseStat;
        }

        /// <summary>
        /// Adds buff to stat with reference to what the buff is
        /// If the buff already exists it will use the large value
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="buff"></param>
        /// <returns>The change of the stat value</returns>
        public int AddBaseBuff(string reference, int buff)
        {
            int previous = Value;

            if (BaseStatBuff.ContainsKey(reference))
            {
                if (buff > BaseStatBuff[reference])
                    BaseStatBuff[reference] = buff;
            }
            else
            {
                BaseStatBuff.Add(reference, buff);
            }

            RefreshValue();

            return Value - previous;
        }

        /// <summary>
        /// Adds debuff to stat with reference to what the buff is
        /// If the debuff already exists it will use the smaller value
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="buff"></param>
        /// <returns>The change of the stat value</returns>
        public int AddBaseDebuff(string reference, int buff)
        {
            int previous = Value;

            if (BaseStatBuff.ContainsKey(reference))
            {
                if (buff < BaseStatBuff[reference])
                    BaseStatBuff[reference] = buff;
            }
            else
            {
                BaseStatBuff.Add(reference, buff);
            }

            RefreshValue();

            return Value - previous;
        }

        /// <summary>
        /// Removes buff
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>The change of the stat value</returns>
        public int RemoveBaseBuff(string reference)
        {
            int previous = Value;
            if (BaseStatBuff.ContainsKey(reference))
            {
                BaseStatBuff.Remove(reference);
                RefreshValue();
            }
            return Value - previous;
        }

        /// <summary>
        /// Adds buff to stat with reference to what the buff is
        /// If the buff already exists it will use the large value
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="buff"></param>
        /// <returns>The change of the stat value</returns>
        public int AddMultiplierBuff(string reference, float buff)
        {
            int previous = Value;
            if (MultiplierStatBuff.ContainsKey(reference))
            {
                MultiplierStatBuff[reference] = buff;
            }
            else
            {
                MultiplierStatBuff.Add(reference, buff);
            }

            RefreshValue();
            return Value - previous;
        }

        /// <summary>
        /// Removes buff
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>The change of the stat value</returns>
        public int RemoveMultiplierBuff(string reference)
        {
            int previous = Value;
            if (MultiplierStatBuff.ContainsKey(reference))
            {
                MultiplierStatBuff.Remove(reference);
                RefreshValue();
            }
            return Value - previous;
        }

        private void RefreshValue()
        {
            float percentBuff = 1;
            int baseBuff = 0;

            foreach (KeyValuePair<string, float> kvp in MultiplierStatBuff)
            {
                percentBuff *= kvp.Value;
            }

            foreach (KeyValuePair<string, int> kvp in BaseStatBuff)
            {
                baseBuff += kvp.Value;
            }

            Value = Mathf.Max(Mathf.CeilToInt((baseStat + baseBuff) * percentBuff), 0);
        }
    }
}

