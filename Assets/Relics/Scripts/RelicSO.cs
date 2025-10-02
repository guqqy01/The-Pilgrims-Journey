using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
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
    [CreateAssetMenu()]
    public class RelicSO : ScriptableObject
    {
        [TextArea()]
        public string summary;
        [TextArea()]
        public string description;
        public Sprite sprite;
        public int baseValue;

        public List<SpellSO> spells;
        [SerializeReference]
        [AbstractSelector]
        public List<RelicEffect> relicEffects;
    }
}

