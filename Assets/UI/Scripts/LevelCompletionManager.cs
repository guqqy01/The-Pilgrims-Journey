using Tiles;
using UnityEngine;
using Map;
using UnityEngine.UI;
using UnityEditor.UIElements;
public class LevelCompletionManager : MonoBehaviour
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

    public void Awake()
    {
        GameManager.onCompleteLocalMapLevel += OnCompleteLevel;
        GameManager.onCreateLocalMap += OnCreateLocalMap;
    }

    private void OnCompleteLevel()
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

    private void OnCreateLocalMap()
    {
        LevelCompleteScreen.SetActive(false);
    }

    public void OnButtonClick()
    {
        if (continueToNextLevel)
        {
            SceneLoader.Instance.LoadGlobalMapScene();
            GlobalMapUI.FirstSeen = true;

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
