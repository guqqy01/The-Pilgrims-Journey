using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public struct BuffableFloatStat
    {
        private float baseStat;
        public float Value { private set; get; }

        private Dictionary<string, float> BaseStatBuff;
        private Dictionary<string, float> MultiplierStatBuff;

        public BuffableFloatStat(float baseStat)
        {
            this.baseStat = baseStat;
            BaseStatBuff = new Dictionary<string, float>();
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
        public float AddBaseBuff(string reference, float buff)
        {
            float previous = Value;

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
        public float AddBaseDebuff(string reference, float buff)
        {
            float previous = Value;

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
        public float RemoveBaseBuff(string reference)
        {
            float previous = Value;
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
        public float AddMultiplierBuff(string reference, float buff)
        {
            float previous = Value;
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
        public float RemoveMultiplierBuff(string reference)
        {
            float previous = Value;
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
            float baseBuff = 0;

            foreach (KeyValuePair<string, float> kvp in MultiplierStatBuff)
            {
                percentBuff *= kvp.Value;
            }

            foreach (KeyValuePair<string, float> kvp in BaseStatBuff)
            {
                baseBuff += kvp.Value;
            }

            Value = Mathf.Max((baseStat + baseBuff) * percentBuff , 0);
        }
    }
}

