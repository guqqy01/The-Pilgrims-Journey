using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Put this on any Selectable (Button, etc). Scales/colors the object up when
/// hovered with a mouse OR selected via keyboard/gamepad, and keeps the two
/// input methods visually in sync (moving the mouse over a button also moves
/// the EventSystem's "selected" object, so gamepad nav and mouse hover never
/// disagree about what's highlighted).
/// </summary>
[RequireComponent(typeof(Selectable))]
public class MenuButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float highlightedScale = 1.1f;
    [SerializeField] private float animSpeed = 12f;

    [Header("Color (optional, leave targetGraphic empty to skip)")]
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.4f);
    [SerializeField]
    private TextMeshProUGUI text;

    private string baseText;

    private Vector3 _targetScale;
    private bool _hovered;
    private bool _selected;
    [SerializeField]
    private Button button;

    private void Awake()
    {
        _targetScale = Vector3.one * normalScale;
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
        if(button == null)
            button = GetComponent<Button>();
        if(text != null)
            baseText = text.text;
    }

    private void OnDisable()
    {
        // Reset state so a re-enabled button doesn't start "stuck" highlighted
        _hovered = false;
        _selected = false;
        transform.localScale = Vector3.one * normalScale;
        if (targetGraphic != null)
            targetGraphic.color = normalColor;

        if (text != null)
            text.text = baseText;
    }

    private void Update()
    {
        // Unscaled so the menu still animates if you pause gameplay with Time.timeScale = 0
        transform.localScale = Vector3.Lerp(
            transform.localScale, _targetScale, Time.unscaledDeltaTime * animSpeed);
    }

    private void RefreshState()
    {
        bool highlighted = _selected && button.interactable;
        _targetScale = Vector3.one * (highlighted ? highlightedScale : normalScale);
        if (targetGraphic != null)
            targetGraphic.color = highlighted ? selectedColor : normalColor;

        if (text != null)
            text.text = baseText + (highlighted ? "  <" : "") ;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        if (!_hovered && !_selected) SFXManager.Instance.PlayButtonHover();

        _hovered = true;
        // Sync gamepad/keyboard selection to whatever the mouse is over
        EventSystem.current.SetSelectedGameObject(gameObject);
        RefreshState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        RefreshState();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!button.interactable)
            return;

        if (!_hovered && !_selected) SFXManager.Instance.PlayButtonHover();
        _selected = true;
        RefreshState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _selected = false;

        RefreshState();
    }
}