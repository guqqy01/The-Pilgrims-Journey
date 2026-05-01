using System.Collections.Generic;
using UnityEngine;
using Units.UI;
namespace Equipment.UI
{
    public class EquipmentUISO : ScriptableObject, IDescriptableUI
    {
        public EquipmentSO EquipmentSO;

        [SerializeField]
        private UnitStatSubDescriptionUI descriptionStatUIPrefab;


        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams
        {
            get
            {
                List<SubDescriptionUIAndParams> returnList = new List<SubDescriptionUIAndParams>();

                foreach (var kvp in EquipmentSO.EquipmentStatsDictionary)
                {
                    object[] paramObjects = new object[2];
                    paramObjects[0] = kvp.Value;
                    paramObjects[1] = kvp.Key;
                    returnList.Add(new SubDescriptionUIAndParams(descriptionStatUIPrefab, paramObjects));
                }

                return returnList;
            }
        }

        public string Title => EquipmentSO.Title;

        public string MainDescription => EquipmentSO.MainDescription;

        public Sprite Sprite => EquipmentSO.Sprite;
    }
}