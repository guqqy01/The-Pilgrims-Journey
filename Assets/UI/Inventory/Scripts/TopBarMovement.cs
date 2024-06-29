using UnityEngine;
using UnityEngine.UI;
public class TopBarMovement : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static TopBarMovement Instance;

    [SerializeField]
    private VerticalLayoutGroup verticalLayoutGroup;

    [SerializeField]
    private RectTransform topBarLocationRect;

    [SerializeField]
    private float timeAwayFromBarToRemoveBar = 2f;
    private float timeWhenLeftBarLocation;

    private bool awayFromBar = false;

    [SerializeField]
    private float barSpeed = 35;

    private bool controlMovementOfBar = true;

    private float currentTopPadding = 0;

    private float currentPaddingAsFloat;

    [SerializeField]
    private int minPadding = -45;
    [SerializeField]
    private int maxPadding = 0;

    private bool beginReveal;

    public bool ControlMovementOfBar
    {
        get
        {
            return controlMovementOfBar;
        }
        set
        {
            controlMovementOfBar = value;
            if (!controlMovementOfBar)
            {
                ShowBar();
            }
        }
    }

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

    private void Update()
    {
        ControlBarMovement();
    }
    private void ShowBar()
    {
        verticalLayoutGroup.padding.top = maxPadding;
        currentPaddingAsFloat = maxPadding;

        verticalLayoutGroup.CalculateLayoutInputVertical();
        verticalLayoutGroup.SetLayoutVertical();
    }

    private void HideBar()
    {
        verticalLayoutGroup.padding.top = minPadding;
        currentPaddingAsFloat = minPadding;

        verticalLayoutGroup.CalculateLayoutInputVertical();
        verticalLayoutGroup.SetLayoutVertical();
    }

    private void ControlBarMovement() 
    {
        if (!controlMovementOfBar)
            return;

        if(RectTransformUtility.RectangleContainsScreenPoint(topBarLocationRect, Input.mousePosition) || beginReveal)
        {
            ReviealBar();
        }
        else
        {
            ConcealBar();
        }
    }

    private void ReviealBar()
    {
        awayFromBar = false;

        if (verticalLayoutGroup.padding.top == maxPadding)
        {
            beginReveal = false;
            return;
        }

        beginReveal = true;

        currentPaddingAsFloat += barSpeed * Time.deltaTime;
        currentPaddingAsFloat = Mathf.Clamp(currentPaddingAsFloat, minPadding, maxPadding);

        verticalLayoutGroup.padding.top = (int)currentPaddingAsFloat;

        verticalLayoutGroup.CalculateLayoutInputVertical();
        verticalLayoutGroup.SetLayoutVertical();
    }

    private void ConcealBar()
    {
        if (verticalLayoutGroup.padding.top == minPadding)
            return;

        if (!awayFromBar)
        {
            timeWhenLeftBarLocation = Time.time;
            awayFromBar = true;
        }

        if (Time.time < timeWhenLeftBarLocation + timeAwayFromBarToRemoveBar)
            return;

        currentPaddingAsFloat -= barSpeed * Time.deltaTime;
        currentPaddingAsFloat = Mathf.Clamp(currentPaddingAsFloat, minPadding, maxPadding);

        verticalLayoutGroup.padding.top = (int)currentPaddingAsFloat;

        verticalLayoutGroup.CalculateLayoutInputVertical();
        verticalLayoutGroup.SetLayoutVertical();
    }
}
