
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relics
{
    public class RelicUIManager : MonoBehaviour
    {
        private RelicManager manager;
        [SerializeField]
        private float relicImageMaxSize = 20f;
        public float RelicImageMaxSize => relicImageMaxSize;

        [SerializeField, HideInInspector]
        private List<RelicUIItem> relicUIItems = new List<RelicUIItem>();

        [SerializeField]
        private RelicUIItem relicUIItemPrefab;

        public void Start()
        {
            manager = RelicManager.Instance;
            manager.updateUI += UpdateUI;
        }

        private void DestroyRelicUI()
        {
            foreach (RelicUIItem relic in relicUIItems)
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
                RelicUIItem relicUIItem = Instantiate(relicUIItemPrefab, transform);
                relicUIItem.Init(relic, this);
                relicUIItems.Add(relicUIItem);
            }
        }
    }

}
