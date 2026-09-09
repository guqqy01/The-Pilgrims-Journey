using GameManagerCalls;
using GlobalMap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class InventoryToolbar : Singleton<InventoryToolbar>, IOnCreateCurrentMapPoint
{
    public Sprite selectedBackground;
    public Sprite unselectedBackground;

    [HideInInspector]
    public Button selectedButton;

    [SerializeField]
    private List<ToolbarButtonSetup> toolbarButtons;


    private void Awake()
    {
        Setup();

        Close();

        foreach (ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            toolbarButton.Subscribe();
        }
    }

    protected override void CleanUp()
    {
        foreach (ToolbarButtonSetup toolbarButton in toolbarButtons)
        {
            toolbarButton.UnSubscribe();
        }
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

    public void OnCreateCurrentMapPoint()
    {
        SetupLevelUI();
    }

    [Serializable]
    public class ToolbarButtonSetup
    {
        public string reference;
        public bool Active;
        public TabReference Tab;
        public Button Button;
        public InputActionReference NavigateAction;
        public List<GlobalMapPointType> MapPointTypesButtonActiveFor;

        public InputContext InputContext;

        private InputContextHandle _inputContextHandle;

        public void Subscribe()
        {
            if(NavigateAction != null)
            {
                NavigateAction.action.performed += SubscribeAction;
            }
        }

        public void UnSubscribe()
        {
            if (NavigateAction != null)
            {
                NavigateAction.action.performed -= SubscribeAction;
            }
        }

        public void PushInput()
        {
            _inputContextHandle = InputManager.Instance.PushContext(InputContext);
        }

        public void PopInput()
        {
            InputManager.Instance.PopContext(_inputContextHandle);
        }

        private void SubscribeAction(InputAction.CallbackContext context)
        {
            if (!Active)
                return;
            Button.onClick.Invoke();
        }
    }

}
