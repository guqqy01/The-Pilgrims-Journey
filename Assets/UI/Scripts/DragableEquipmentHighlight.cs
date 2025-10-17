using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Equipment
{
    public class DragableEquipmentHighlight : MonoBehaviour
    {
        [SerializeField]
        private List<Image> highlightImages;

        public void SetActive(bool active, EquipmentSO equipmentSO = null)
        {
            if (!active)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            foreach(Image image in highlightImages)
            {
                image.sprite = equipmentSO.sprite;
                image.SetNativeSize();
            }
        }
    }
}


