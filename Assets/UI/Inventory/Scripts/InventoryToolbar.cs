using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using Map;
public class InventoryToolbar : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static InventoryToolbar Instance;

    [SerializeField]
    private LayoutContentSizerBaseParent layoutContentSizer;

    public Sprite selectedBackground;
    public Sprite unselectedBackground;

    [HideInInspector]
    public Button selectedButton;

    [SerializeField]
    private List<ToolbarButtonSetup> toolbarButtons;

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

    private void Awake()
    {
        Setup();
    }

    private void Start()
    {
        GameManager.onCreateCurrentMapPoint += SetupLevelUI;
    }

    public void SetupLevelUI()
    {
        GlobalMapPointType currentType = GlobalMapGenerator.Instance.CurrentMapPoint.Type;

        foreach (ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            if (toolbarButton.MapPointTypesButtonActiveFor.Contains(currentType))
            {
                ActivateButton(toolbarButton.Button);
            }
            else
            {
                DeactivateButton(toolbarButton.Button);
            }
        }
    }

    private void Update()
    {
        HandleKeyboardShortcuts();
    }

    public void OpenByReference(string reference)
    {
        foreach(ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            if(toolbarButton.reference == reference)
            {
                SelectButton(toolbarButton.Button);
                return;
            }
        }
    }

    private void HandleKeyboardShortcuts()
    {
        foreach (ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            if (toolbarButton.Active && Input.GetKeyDown(toolbarButton.KeyCode))
            {
                toolbarButton.Button.onClick.Invoke();
            }
        }
    }

    private void Setup()
    {
        foreach(ToolbarButtonSetup button in toolbarButtons)
        {
            button.Button.onClick.AddListener(() => SelectButton(button.Button));
        }
    }

    public void DeactivateButton(Button button)
    {
        foreach (ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            if (toolbarButton.Button == button)
            {
                toolbarButton.Active = false;
                break;
            }
        }

        if(button == selectedButton)
        {
            Close();
        }
    }

    public void ActivateButton(Button button)
    {
        foreach (ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            if (toolbarButton.Button == button)
            {
                toolbarButton.Active = true;
                break;
            }
        }
    }

    public void SelectButton(Button selectedButton)
    {
        if (this.selectedButton == selectedButton)
        {
            Close();
            return;
        }

        this.selectedButton = selectedButton;

        GameObject selectedTab = null;

        foreach (ToolbarButtonSetup iterationButton in toolbarButtons)
        {
            if (iterationButton.Button.targetGraphic is Image image)
            {
                if (iterationButton.Button == selectedButton)
                {
                    image.sprite = selectedBackground;
                    if(iterationButton.Tab != null)
                    {
                        iterationButton.Tab.Open();
                        selectedTab = iterationButton.Tab.tab.gameObject;
                    }
                }
                else
                {
                    image.sprite = unselectedBackground;
                    if (iterationButton.Tab != null)
                        iterationButton.Tab.Close();
                }
            }
        }

        if (selectedTab != null && !selectedTab.activeInHierarchy)
        {
            selectedTab.SetActive(true);
        }

        if (layoutContentSizer != null)
            layoutContentSizer.UpdateAllLayoutsDeepestFirst();
    }

    public void Close()
    {
        selectedButton = null;
        foreach (ToolbarButtonSetup iterationButton in toolbarButtons)
        {
            if (iterationButton.Button.targetGraphic is Image image)
            {
                image.sprite = unselectedBackground;
                if (iterationButton.Tab != null)
                    iterationButton.Tab.Close();
            }
        }
    }

    [Serializable]
    public class ToolbarButtonSetup
    {
        public string reference;
        public bool Active;
        public TabReference Tab;
        public Button Button;
        public KeyCode KeyCode;
        public List<GlobalMapPointType> MapPointTypesButtonActiveFor;
    }

}
