using System.Collections.Generic;
using UnityEngine;
using Equipment;
namespace Equipment.UI
{
    public class EquipmentUISO : ScriptableObject, IDescriptableUI
    {
        public EquipmentSO equipmentSO;

        [SerializeField]
        private BuffSubDescriptionUI descriptionStatUIPrefab;

        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams
        {
            get
            {
                List<SubDescriptionUIAndParams> returnList = new List<SubDescriptionUIAndParams>();

                foreach (var kvp in equipmentSO.EquipmentStatsDictionary)
                {
                    object[] paramObjects = new object[2];
                    paramObjects[0] = kvp.Value;
                    paramObjects[1] = kvp.Key;
                    returnList.Add(new SubDescriptionUIAndParams(descriptionStatUIPrefab, paramObjects));
                }

                return returnList;
            }
        }

        public string Title => equipmentSO.Title;

        public string MainDescription => equipmentSO.MainDescription;

        public Sprite Sprite => equipmentSO.Sprite;
    }
}