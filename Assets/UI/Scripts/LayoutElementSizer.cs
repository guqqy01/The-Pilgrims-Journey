using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.LayoutElement))]
public class LayoutElementSizer : MonoBehaviour
{
    private UnityEngine.UI.LayoutElement layoutElement;

    [SerializeField]
    [Header("Padding")]
    private float leftPadding = 0;
    [SerializeField]
    private float rightPadding = 0;
    [SerializeField]
    private float upPadding = 0;
    [SerializeField]
    private float downPadding = 0;

    private void OnValidate()
    {
        if(layoutElement == null)
            layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
    }

    private void Awake()
    {
        layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
    }

    private void UpdateLayoutElementSize()
    {

    }
}
