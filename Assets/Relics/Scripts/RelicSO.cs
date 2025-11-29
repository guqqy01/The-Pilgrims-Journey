using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor;
using Spells.UI;

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

        public string MainDescription => GetDescriptionString();

        private string GetDescriptionString()
        {
            string returnString = description;

            foreach (RelicEffect relicEffect in relicEffects)
            {
                if (returnString != "")
                    returnString += "\r\n";
                returnString += relicEffect.Description;
            }

            return returnString;
        }

        public Sprite Sprite => sprite;

        [SerializeField]
        private SpellSubDescriptionUI relicSpellUIItemPrefab;

        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams
        {
            get
            {
                List<SubDescriptionUIAndParams> returnList = new List<SubDescriptionUIAndParams>();

                foreach (SpellAndAmountOfUses spellAndAmountOfUses in spells)
                {
                    object[] param = new object[2];
                    param[0] = spellAndAmountOfUses.spell;
                    param[1] = spellAndAmountOfUses.amountOfUses;

                    returnList.Add(new SubDescriptionUIAndParams(relicSpellUIItemPrefab, param));
                }

                return new List<SubDescriptionUIAndParams>();
            }
        }
    }
}