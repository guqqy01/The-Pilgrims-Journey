using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Units
{
    public struct BuffableStat
    {
        private object baseStat;
        public IntOrFloatObject Value { private set; get; }

        private Dictionary<string, Buff> buffs;

        public BuffableStat(object baseStat)
        {
            if (baseStat is not int && baseStat is not float)
            {
                throw new System.Exception("Buffable stat needs to be either an int or float");
            }

            this.baseStat = baseStat;
            buffs = new Dictionary<string, Buff>();
            Value = new IntOrFloatObject(baseStat);
        }

        public object AddBuff(string reference, Buff intBuff)
        {
            IntOrFloatObject previous = Value;

            if (buffs.ContainsKey(reference))
            {
                buffs[reference] = intBuff;
            }
            else
            {
                buffs.Add(reference, intBuff);
            }

            RefreshValue();

            if(baseStat is int)
            {
                return (int)Value.Value - (int)previous.Value;
            }
            else
            {
                return (float)Value.Value - (float)previous.Value;
            }
        }

        public object RemoveBuff(string reference)
        {
            IntOrFloatObject previous = Value;

            if (buffs.ContainsKey(reference))
            {
                buffs.Remove(reference);
            }

            RefreshValue();

            if(baseStat is int)
            {
                return (int)Value.Value - (int)previous.Value;
            }
            else
            {
                return (float)Value.Value - (float)previous.Value;
            }
        }

        private void RefreshValue()
        {
            Buff buff = buffs.First().Value;

            foreach (KeyValuePair<string, Buff> kvp in buffs)
            {
                if (buff == kvp.Value) continue;

                buff += kvp.Value;
            }

            Value = buff.Value(new IntOrFloatObject(baseStat));
        }
    }
}

