using System.Collections.Generic;
using UnityEngine;

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

        public List<ISubDescriptionData> GetSubDescriptionData => new List<ISubDescriptionData>();

        private string GetDescriptionString()
        {
            string returnString = description;

            foreach (RelicEffect relicEffect in relicEffects)
            {
                if (returnString != "")
                    returnString += "\r\n";
                returnString += relicEffect.Description(this);
            }

            return returnString;
        }

        public Sprite Sprite => sprite;

        public string ShortDescription => description;
    }
}