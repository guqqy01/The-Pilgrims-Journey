using Map;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;

public class InventoryUIManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static InventoryUIManager Instance;

    public InventoryToolbar inventoryToolbar;

    [SerializeField]
    private List<Button> optionalButtons;

    [SerializeField]
    private SerializedDictionary<GlobalMapPointType, List<Button>> globalMapPointAndRequiredButtons;

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        GameManager.onCreateCurrentMapPoint += Setup;
    }

    public void Setup()
    {
        foreach(var button in optionalButtons)
        {
            List<Button> requiredButtons = new List<Button>();


        }
    }

}


