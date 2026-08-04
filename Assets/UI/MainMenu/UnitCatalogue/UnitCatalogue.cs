using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Units.UI
{
    public class UnitCatalogue : MonoBehaviour
    {
        [SerializeField]
        private Button leftButton;
        [SerializeField]
        private Button rightButton;

        [SerializeField]
        private UnitCatalogueItem catalogueItem;

        private UnitManager unitManager;

        private void Start()
        {
            unitManager = UnitManager.Instance;

            List<UnitSO> units = unitManager.UnitStates.Keys.ToList();

            catalogueItem.Setup(units[0]);

            leftButton.interactable = false;
            rightButton.interactable = 0 >= 0 && 0 < units.Count - 1;
        }

        public void RightButtonPress()
        {
            List<UnitSO> units = unitManager.UnitStates.Keys.ToList();
            int index = units.IndexOf(catalogueItem.Unit);

            if (index >= 0 && index < units.Count - 1)
                catalogueItem.Setup(units[index + 1]);

            leftButton.interactable = index + 1 > 0 && index + 1 <= units.Count - 1;
            rightButton.interactable = index + 1 >= 0 && index + 1 < units.Count - 1;
        }

        public void LeftButtonPress()
        {
            List<UnitSO> units = unitManager.UnitStates.Keys.ToList();
            int index = units.IndexOf(catalogueItem.Unit);

            if (index > 0 && index <= units.Count - 1)
                catalogueItem.Setup(units[index - 1]);

            leftButton.interactable = index - 1 > 0 && index - 1 <= units.Count - 1;
            rightButton.interactable = index - 1 >= 0 && index - 1 < units.Count - 1;
        }
    }

}
