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
    public class RelicSO : ScriptableObject, IDescriptable
    {
        [TextArea()]
        public string description;
        public Sprite sprite;
        public int baseValue;

        public List<SpellAndAmountOfUses> spells;

        [SerializeReference]
        [AbstractSelector]
        public List<RelicEffect> relicEffects;
        public string Title => name.AddSpacesBeforeCapsRegex();

        public string MainDescription => description;

        public Sprite Sprite => sprite;

        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams
        {
            get
            {
                return new List<SubDescriptionUIAndParams>();
            }
        }
    }
}