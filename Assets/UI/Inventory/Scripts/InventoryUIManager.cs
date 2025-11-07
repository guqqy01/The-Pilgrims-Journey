using GlobalMap;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;
using GameManagerCalls;

public class InventoryUIManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot, IOnSetup
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

    public void SetupContinueGameAfterMapPointLoad()
    {
        
    }

    public void SetupNewGame()
    {
        foreach(var button in optionalButtons)
        {
            List<Button> requiredButtons = new List<Button>();
        }
    }

}


