using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(Health))]
    public class CoinDrop : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private ResourceManager resourceManager;

        public BuffableFloatStat coin1Percent;
        public BuffableFloatStat coin5Percent;
        public BuffableFloatStat coint10Percent;

        public void Init(UnitSO unitSO)
        {
            coin1Percent = new BuffableFloatStat(unitSO.coin1Percent);
            coin5Percent = new BuffableFloatStat(unitSO.coin5Percent);
            coint10Percent = new BuffableFloatStat(unitSO.coint10Percent);

            GetComponent<Health>().onDie += OnDie;
        }

        private void Start()
        {
            resourceManager = ResourceManager.Instance;
        }

        public void OnDie()
        {
            float value = Random.value;

            Debug.Log("Die");

            if(value <= coint10Percent.Value)
            {
                Instantiate(resourceManager.coin10, transform.position, Quaternion.identity, resourceManager.transform);
            }
            else if (value <= coin5Percent.Value)
            {
                Instantiate(resourceManager.coin5, transform.position, Quaternion.identity, resourceManager.transform);
            }
            else if (value <= coin1Percent.Value)
            {
                Instantiate(resourceManager.coin1, transform.position, Quaternion.identity, resourceManager.transform);
            }
        }
    }
}
