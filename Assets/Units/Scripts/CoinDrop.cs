using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(Health))]
    public class CoinDrop : MonoBehaviour
    {
        private ResourceManager resourceManager => ResourceManager.Instance;

        public BuffableStat coin1Percent;
        public BuffableStat coin5Percent;
        public BuffableStat coint10Percent;

        public void Init(UnitSO unitSO)
        {
            coin1Percent = new BuffableStat(unitSO.Coin1Percent);
            coin5Percent = new BuffableStat(unitSO.Coin5Percent);
            coint10Percent = new BuffableStat(unitSO.Coin10Percent);

            GetComponent<Health>().onDie += OnDie;
        }

        public void OnDie(Health health)
        {
            float value = Random.value;

            if(value <= (float)coint10Percent.Value.Value)
            {
                resourceManager.coins.Add(Instantiate(resourceManager.coin10, transform.position, Quaternion.identity, resourceManager.transform));
            }
            else if (value <= (float)coin5Percent.Value.Value)
            {
                resourceManager.coins.Add(Instantiate(resourceManager.coin5, transform.position, Quaternion.identity, resourceManager.transform));
            }
            else if (value <= (float)coin1Percent.Value.Value)
            {
                resourceManager.coins.Add(Instantiate(resourceManager.coin1, transform.position, Quaternion.identity, resourceManager.transform));
            }
        }
    }
}
