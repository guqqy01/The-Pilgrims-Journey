
using System;
using System.Collections.Generic;
using UnityEngine;
using Spells;


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
    public class Relic : IDescriptable
    {
        public RelicSO relicSO;

        [SerializeField]
        private int level;
        public int Level
        {
            set
            {
                level = value;

                if (relicEffects == null)
                    return;

                foreach(RelicEffect relicEffect in relicEffects)
                {
                    EnsureRelicEffectSetup(relicEffect);

                    if (relicEffect != null)
                        relicEffect.OnLevelChangeRelic();
                }
            }
            get => level;
        }

        public bool Active;

        public int baseValue;

        public string BottomText
        { 
            get
            {
                foreach(RelicEffect relicEffect in relicEffects)
                {
                    EnsureRelicEffectSetup(relicEffect);

                    if (relicEffect.GetBottomText() != "")
                    {
                        return relicEffect.GetBottomText();
                    }
                }

                return "";
            } 
        }
        
        public string TopText
        {
            get
            {
                foreach (RelicEffect relicEffect in relicEffects)
                {
                    EnsureRelicEffectSetup(relicEffect);

                    if (relicEffect.GetTopText() != "")
                    {
                        return relicEffect.GetTopText();
                    }
                }

                return "";
            }
        }

        public string Title => relicSO.Title;

        public string MainDescription => relicSO.MainDescription;

        public Sprite Sprite => relicSO.Sprite;

        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams => relicSO.SubDescriptionPrefabsAndParams;

        [SerializeReference]
        public List<RelicEffect> relicEffects = new List<RelicEffect>();

        private void EnsureRelicEffectSetup(RelicEffect effect)
        {
            if(effect == null)
            {
                Debug.LogWarning($"Relic effect is null in Relic: {this}");
                return;
            }

            if (effect.Relic == null)
                return;

            effect.Init(this);
        }

        public Relic(RelicSO relicSO, int level = 1)
        {
            Init(relicSO, level);
        }

        public void Init(RelicSO relicSO, int level = 1)
        {
            this.relicSO = relicSO;
            baseValue = relicSO.baseValue;

            this.level = level;

            foreach (RelicEffect effect in relicSO.relicEffects)
            {
                if (effect == null)
                    continue;

                RelicEffect clonedEffect = effect.CreateInstance();
                clonedEffect.Init(this);
                relicEffects.Add(clonedEffect);
            }

            foreach (SpellAndAmountOfUses spell in relicSO.spells)
            {
                SpellManager.Instance.AddSpell(spell.spell, spell.amountOfUses);
            }
        }
    }
}

