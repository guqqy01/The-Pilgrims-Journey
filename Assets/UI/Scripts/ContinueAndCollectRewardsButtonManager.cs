using Tiles;
using UnityEngine;
using Map;
using UnityEngine.UI;
using UnityEditor.UIElements;
using GameManagerCalls;
public class ContinueAndCollectRewardsButtonManager : MonoBehaviour, IOnCompleteLocalMapLevel, IOnCreateLocalMap
{
    [SerializeField]
    private GameObject LevelCompleteScreen;

    [SerializeField]
    private Text buttonText;

    private bool continueToNextLevel;

    [SerializeField]
    private Button prizeWindowButton;
    [SerializeField]
    private InventoryToolbar toolbar;


    public void OnCompleteLocalMapLevel()
    {
        LevelCompleteScreen.SetActive(true);

        if (RewardManager.Instance.TilesOrEquipmentAvailiable)
        {
            buttonText.text = "Collect Rewards";
            continueToNextLevel = false;
        }
        else
        {
            buttonText.text = "Continue";
            continueToNextLevel = true;
        }
    }

    public void OnCreateLocalMap()
    {
        LevelCompleteScreen.SetActive(false);
    }

    public void OnButtonClick()
    {
        if (continueToNextLevel)
        {
            GameManager.Instance.CompleteLevel();

            GlobalMapUI.FirstSeen = true;
            SceneLoader.Instance.LoadGlobalMapScene();
            

            LevelCompleteScreen.SetActive(false);
        }
        else
        {
            buttonText.text = "Continue";
            continueToNextLevel = true;

            if (toolbar.selectedButton != prizeWindowButton)
                prizeWindowButton.onClick.Invoke();
        }
    }
}
