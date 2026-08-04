
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics.UI
{
    public class RelicBottomBarUIManager : MonoBehaviour
    {
        private RelicManager manager;
        [SerializeField]
        private float relicImageMaxSize = 20f;
        public float RelicImageMaxSize => relicImageMaxSize;

        [SerializeField, HideInInspector]
        private List<RelicBottomBarUIItem> relicUIItems = new List<RelicBottomBarUIItem>();

        [SerializeField]
        private RelicBottomBarUIItem relicUIItemPrefab;

        public void Start()
        {
            manager = RelicManager.Instance;
            manager.updateUI += UpdateUI;
        }

        private void DestroyRelicUI()
        {
            foreach (RelicBottomBarUIItem relic in relicUIItems)
            {
                relic.Unsubscribe();
                Destroy(relic.gameObject);
            }

            relicUIItems.Clear();
        }

        public void UpdateUI()
        {
            DestroyRelicUI();

            foreach (Relic relic in manager.Relics)
            {
                RelicBottomBarUIItem relicUIItem = Instantiate(relicUIItemPrefab, transform);
                relicUIItem.Init(relic, this);
                relicUIItems.Add(relicUIItem);
            }
        }
    }

}
