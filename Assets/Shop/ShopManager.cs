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
    private Dictionary<EquipmentSO, float> equipmentAbleToBePurchasedAndChanceToAppear;

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


    public delegate void OnSetupShop(Dictionary<EquipmentSO, int> equipmentAndStock);
    public OnSetupShop onSetupShop;

    public void SetupShop()
    {
        Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);


    }
}
