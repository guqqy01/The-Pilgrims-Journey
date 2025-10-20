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
    }
}