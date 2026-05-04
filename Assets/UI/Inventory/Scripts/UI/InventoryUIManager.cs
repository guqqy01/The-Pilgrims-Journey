using GlobalMap;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;
using GameManagerCalls;

public class InventoryUIManager : Singleton<InventoryUIManager>, IOnSetupNewGame
{
    public InventoryToolbar inventoryToolbar;

    [SerializeField]
    private List<Button> optionalButtons;

    [SerializeField]
    private SerializedDictionary<GlobalMapPointType, List<Button>> globalMapPointAndRequiredButtons;

    public void SetupContinueGameAfterMapPointLoad()
    {
        
    }

    public void RemoveBlurOverride()
    {
        foreach(var button in optionalButtons)
        {
            List<Button> requiredButtons = new List<Button>();
        }
    }

}


