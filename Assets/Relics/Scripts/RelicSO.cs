using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

using UnityEditor;
namespace Relics
{
    [CreateAssetMenu()]
    public class RelicSO : ScriptableObject
    {
        [TextArea()]
        public string description;
        public Sprite sprite;
        public int baseValue;

        public List<RelicSpell> spells;

        [SerializeReference]
        [AbstractSelector]
        public List<RelicEffect> relicEffects;

        private Sprite previousSprite;
    }

    [Serializable]
    public class RelicSpell
    {
        public SpellSO spell;
        public int amountOfUses = 1;
    }
}