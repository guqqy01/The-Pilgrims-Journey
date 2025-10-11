using Equipment;
using Map;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.ParticleSystem; 

public class ShopManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static ShopManager Instance;

    [SerializeField]
    private int minAmountOfDifferentItems = 3;

    [SerializeField]
    private List<ShopItemSetting> shopItemSettings;

    public void InitiateSingleton()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetupShop()
    {
        Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

        Dictionary<EquipmentSO, int> equipmentAndStock = new Dictionary<EquipmentSO, int>();

        while(equipmentAndStock.Count < minAmountOfDifferentItems)
        {
            equipmentAndStock.Clear();

            foreach (var item in shopItemSettings)
            {
                float evaluateTime = GlobalMapGenerator.Instance.CurrentPosition.x / GlobalMapGenerator.Instance.GenerationSettings.MapSize.x;
                float lerpFactor = Random.Range(0.0f, 1.0f);

                if (Random.Range(0f, 1f) <= item.ChanceOfBeginInShopAtXMapPosition.Evaluate(evaluateTime, lerpFactor))
                {
                    //Item Is in shop

                    int amountOfItems = item.IfInShopHowManyRange.RandomBetweenTwoValuesInclusive();

                    if(equipmentAndStock.ContainsKey(item.EquipmentSO))
                    {
                        equipmentAndStock[item.EquipmentSO] += amountOfItems;
                    }
                    else
                    {
                        equipmentAndStock.Add(item.EquipmentSO, amountOfItems);
                    }
                }
            }
        }

        ShopUI.Instance.Setup(equipmentAndStock);
    }
}

[Serializable]
public class ShopItemSetting
{
    public EquipmentSO EquipmentSO;

    // X position is normalized to 0-1 so end of the map will be at t = 1
    public MinMaxCurve ChanceOfBeginInShopAtXMapPosition;

    public Vector2Int IfInShopHowManyRange;
}
