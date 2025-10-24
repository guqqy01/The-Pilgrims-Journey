using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
using static UnityEditor.Searcher.SearcherWindow;

[Serializable]
public class LayoutElementData : IParentListItem<LayoutElementData>
{
    public RectTransform rectTransform;
    public LayoutContentElement contentSizer;
    public LayoutGroup layoutGroup;

    private Rect? rect;

    private LayoutElementData parent;
    public LayoutElementData Parent
    {
        get
        {
            return parent;
        }
        set
        {
            parent = value;

            if (parent == null)
                return;

            if(parent.Children == null)
                parent.Children = new List<LayoutElementData>();

            if (parent.Children.Contains(this))
                return;

            parent.Children.Add(this);
        }
    }
    public List<LayoutElementData> Children { get; set; } = new List<LayoutElementData>();

    public bool IsSubParent(LayoutElementData other)
    {
        LayoutElementData current = Parent;

        while (current != null && current != other)
        {
            current = current.Parent;
        }

        return current != null && current == other;
    }

    public bool IsSubChild(LayoutElementData other)
    {
        return other.IsSubParent(this);
    }

    public Rect GetRect()
    {
        if (rect != null)
            return (Rect)rect;

        if (layoutGroup != null)
        {
            if (layoutGroup is HorizontalLayoutGroup horizontalLayoutGroup)
            {
                HLayoutGroupSetUpChildrenNoCFENoCCS(horizontalLayoutGroup);
            }
            else if (layoutGroup is VerticalLayoutGroup verticalLayoutGroup)
            {
                VLayoutGroupSetUpChildrenNoCFENoCCS(verticalLayoutGroup);
            }
        }

        if (contentSizer != null)
        {
            foreach (var c in contentSizer.Content)
            {
                if (c.contentSizer != null)
                    c.contentSizer.MeasureLayout();
            }

            rect = contentSizer.GetRect();
            return (Rect)rect;
        }

        rect = new Rect()
        {
            position = rectTransform.position,
            size = rectTransform.sizeDelta,
        };

        return (Rect)rect;
    }

    public Vector2 GetSize()
    {
        if (rect != null)
            return ((Rect)rect).size;

        if (contentSizer != null)
        {
            return contentSizer.GetSize();
        }

        return rectTransform.sizeDelta;
    }

    /// <summary>
    /// Horizontal Layout Group
    /// No Child Force Expand
    /// No Control Child Size
    /// </summary>
    private void HLayoutGroupSetUpChildrenNoCFENoCCS(HorizontalLayoutGroup hLayoutGroup)
    {
        List<LayoutElementData> orderedChildren = Children.OrderBy(x => x.rectTransform.GetSiblingIndex()).ToList();

        float leftPos = rectTransform.position.x - rectTransform.sizeDelta.x / 2f;
        float bottomPos = rectTransform.position.y - rectTransform.sizeDelta.y / 2f;
        float topPos = rectTransform.position.y + rectTransform.sizeDelta.y / 2f;

        float xPos = hLayoutGroup.padding.left + leftPos;

        foreach (var child in orderedChildren)
        {
            Vector2 childSize = child.GetSize();
            float childHeight = childSize.y;

            float childXPosition = xPos + childSize.x / 2f;

            float childYPosition = hLayoutGroup.childAlignment switch
            {
                TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight =>
                    topPos -hLayoutGroup.padding.top - childHeight / 2f,

                TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight =>
                    rectTransform.position.x,

                TextAnchor.LowerLeft or TextAnchor.LowerCenter or TextAnchor.LowerRight =>
                    bottomPos + hLayoutGroup.padding.bottom + childHeight / 2f,

                _ => child.rectTransform.position.y
            };

            child.rect = new Rect(childSize, new Vector2(childXPosition, childYPosition));

            xPos += childSize.x + hLayoutGroup.spacing;
        }
    }

    /// <summary>
    /// Vertical Layout Group
    /// No Child Force Expand
    /// No Control Child Size
    /// </summary>
    private void VLayoutGroupSetUpChildrenNoCFENoCCS(VerticalLayoutGroup vLayoutGroup)
    {
        List<LayoutElementData> orderedChildren = Children.OrderBy(x => x.rectTransform.GetSiblingIndex()).ToList();

        float leftPos = rectTransform.position.x - GetSize().x / 2f;
        float rightPos = rectTransform.position.x + GetSize().x / 2f;
        float bottomPos = rectTransform.position.y - GetSize().y / 2f;

        float yPos = vLayoutGroup.padding.bottom + bottomPos;

        foreach (var child in orderedChildren)
        {
            Vector2 childSize = child.GetSize();
            float childWidth = childSize.x;

            float childYPosition = yPos + childSize.y / 2f;

            float childXPosition = vLayoutGroup.childAlignment switch
            {
                TextAnchor.UpperLeft or TextAnchor.MiddleLeft or TextAnchor.LowerLeft =>
                    leftPos + vLayoutGroup.padding.left + childWidth / 2f,

                TextAnchor.LowerCenter or TextAnchor.MiddleCenter or TextAnchor.UpperCenter =>
                    rectTransform.position.x,

                TextAnchor.LowerRight or TextAnchor.MiddleRight or TextAnchor.UpperRight =>
                    rightPos - vLayoutGroup.padding.right - childWidth / 2f,

                _ => child.rectTransform.position.y
            };

            child.rect = new Rect(childSize, new Vector2(childXPosition, childYPosition));

            yPos += childSize.y + vLayoutGroup.spacing;
        }
    }
}