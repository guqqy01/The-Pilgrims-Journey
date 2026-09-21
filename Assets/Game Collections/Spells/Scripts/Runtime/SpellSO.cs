using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spells
{
    [CreateAssetMenu()]
    public class SpellSO : ScriptableObject, IDescriptable
    {
        [TextArea]
        public string description;

        public Sprite sprite;

        public Color highlightColor = Color.white;
        public Color invalidUseColor = Color.red;

        [SerializeReference]
        [AbstractSelector]
        public List<SpellEffect> spellEffects;

        public string Title => name;

        public string MainDescription => description;
        public string ShortDescription => description;

        public Sprite Sprite => sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => throw new System.NotImplementedException();


        public bool CanUseSpell(Vector2Int position)
        {
            foreach (SpellEffect effect in spellEffects)
            {
                if (!effect.CanUseEffect(position))
                    return false;
            }

            return true;
        }
    }
}

