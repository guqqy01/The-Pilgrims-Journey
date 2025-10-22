using System;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [Serializable]
    public class BuffableStat
    {
        private readonly IntOrFloatObject baseStat;
        public IntOrFloatObject Value { get; private set; }

        private readonly Dictionary<string, Buff> buffs = new();

        public bool IsIntStat => baseStat.IsInt;

        public BuffableStat(int baseStat)
        {
            this.baseStat = new IntOrFloatObject(baseStat);
            Value = this.baseStat;
        }

        public BuffableStat(float baseStat)
        {
            this.baseStat = new IntOrFloatObject(baseStat);
            Value = this.baseStat;
        }

        public BuffableStat(IntOrFloatObject baseStat)
        {
            this.baseStat = baseStat;
            Value = this.baseStat;
        }

        public IntOrFloatObject AddBuff(string reference, Buff buff)
        {
            var previous = Value;
            buffs[reference] = buff; // add or replace
            RefreshValue();
            return Value - previous;
        }

        public IntOrFloatObject RemoveBuff(string reference)
        {
            var previous = Value;
            buffs.Remove(reference);
            RefreshValue();
            return Value - previous;
        }

        private void RefreshValue()
        {
            if (buffs.Count == 0)
            {
                Value = baseStat;
                return;
            }

            // Combine all buffs
            Buff combined = null;
            foreach (var buff in buffs.Values)
            {
                combined = combined == null ? buff : combined + buff;
            }

            Value = combined.Value(baseStat);
        }

        private Buff ConvertToBuff()
        {
            IntOrFloatObject baseBuff = baseStat;
            IntOrFloatObject additionalBuff = 0;
            float multiplier = 0;

            foreach(var buff in buffs)
            {
                baseBuff += buff.Value.BaseBuff;
                multiplier += buff.Value.Multiplier;
                additionalBuff += buff.Value.AdditionalBuff;
            }

            return new Buff(multiplier, baseBuff, additionalBuff);
        }

        public static implicit operator BuffableStat(int value) => new BuffableStat(value);
        public static implicit operator BuffableStat(float value) => new BuffableStat(value);

        public static implicit operator int(BuffableStat obj) => obj.Value.IntValue;
        public static implicit operator float(BuffableStat obj) => obj.Value.FloatValue;


        public static implicit operator Buff(BuffableStat obj) => obj.ConvertToBuff();
    }
}