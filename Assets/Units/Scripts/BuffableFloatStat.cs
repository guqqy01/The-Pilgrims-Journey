using System.Collections.Generic;
using UnityEngine;

namespace Units
{

    public struct BuffableFloatStat
    {
        private float baseStat;
        public float Value { private set; get; }

        private Dictionary<string, float> BaseStatBuff;
        private Dictionary<string, float> PercentStatBuff;

        public BuffableFloatStat(float baseStat)
        {
            this.baseStat = baseStat;
            BaseStatBuff = new Dictionary<string, float>();
            PercentStatBuff = new Dictionary<string, float>();
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
        public float AddPercentBuff(string reference, float buff)
        {
            float previous = Value;
            if (PercentStatBuff.ContainsKey(reference))
            {
                PercentStatBuff[reference] = buff;
            }
            else
            {
                PercentStatBuff.Add(reference, buff);
            }

            RefreshValue();
            return Value - previous;
        }

        /// <summary>
        /// Removes buff
        /// </summary>
        /// <param name="reference"></param>
        /// <returns>The change of the stat value</returns>
        public float RemovePercentBuff(string reference)
        {
            float previous = Value;
            if (PercentStatBuff.ContainsKey(reference))
            {
                PercentStatBuff.Remove(reference);
                RefreshValue();
            }
            return Value - previous;
        }

        private void RefreshValue()
        {
            float percentBuff = 1;
            float baseBuff = 0;

            foreach (KeyValuePair<string, float> kvp in PercentStatBuff)
            {
                percentBuff *= kvp.Value;
            }

            foreach (KeyValuePair<string, float> kvp in BaseStatBuff)
            {
                baseBuff += kvp.Value;
            }

            Value = Mathf.Max(Mathf.CeilToInt(baseStat * percentBuff) + baseBuff, 0);
        }
    }
}

