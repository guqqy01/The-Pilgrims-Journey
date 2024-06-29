
using System;
using System.Collections.Generic;
using UnityEngine;
/*
Incease gold generation
Start units get a certain buff
Certain tiles get a boost
Spells
Buff certain stats, Health, Healing, Shield, etc
Stun monsters for a short period of time for the beginning of the game
 */


namespace Relics
{
    [Serializable]
    public class Relic
    {
        public RelicSO relicSO;

        public bool Active;

        public int baseValue;

        public string BottomText
        { 
            get
            {
                foreach(RelicEffect effect in relicEffects)
                {
                    if(effect.GetBottomText() != "")
                    {
                        return effect.GetBottomText();
                    }
                }

                return "";
            } 
        }

        public string TopText
        {
            get
            {
                foreach (RelicEffect effect in relicEffects)
                {
                    if (effect.GetTopText() != "")
                    {
                        return effect.GetTopText();
                    }
                }

                return "";
            }
        }

        [SerializeReference]
        public List<RelicEffect> relicEffects = new List<RelicEffect>();

        public Relic(RelicSO relicSO)
        {
            this.relicSO = relicSO;
            baseValue = relicSO.baseValue;

            foreach(RelicEffect effect in relicSO.relicEffects)
            {
                RelicEffect clonedEffect = effect.Clone;
                clonedEffect.Init(this);
                relicEffects.Add(clonedEffect);
            }

            foreach(SpellSO spell in relicSO.spells)
            {
                SpellManager.Instance.AddSpell(spell);
            }
        }
    }
}

