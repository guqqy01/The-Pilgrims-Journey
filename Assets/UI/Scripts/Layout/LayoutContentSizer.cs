using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Metadata;

//If Both controlling children with layoutGroup and getting controlled by parent layoutGroup

//size      : layoutGroup padding + layout spacing + children size's addedtogether
//position  : look at siblings size's from the GetRect() function and figure out position based on parents layoutgroup settings

//If controlling children with layoutGroup

//size      : layoutGroup padding + layout spacing + children size's addedtogether
//position  : current position

//If own position controlled by layoutGroup

//size      : padding + minPosition of all children to maxPosition of all children
//position  : look at siblings size's from the GetRect() function and figure out position based on parents layoutgroup settings


//If neither

//size      : padding + minPosition of all children to maxPosition of all children
//position  : centred to surround children


public class LayoutContentSizer : MonoBehaviour
{
    private LayoutContentSizerBaseParent layoutSizerParent;
    public LayoutContentSizerBaseParent LayoutSizerParent
    {
        get
        {
            if (layoutSizerParent == null)
                GetLayoutParent();

            if (layoutSizerParent == null)
            {

                return null;
            }

            return layoutSizerParent;
        }
        set
        {
            layoutSizerParent = value;
        }
    }

    protected RectTransform rectTransform;

    private ParentList<LayoutContent> content = new ParentList<LayoutContent>();
    internal ParentList<LayoutContent> Content
    {
        get
        {
            return content;
        }
        set
        {
            content = value;
        }
    }

    [SerializeField]
    private List<RectTransform> childrenToIgnore;

    public List<RectTransform> ChildrenToIgnore => childrenToIgnore;

    [SerializeField]
    private List<RectTransform> includeContent;

    public List<RectTransform> IncludeContent => includeContent;

    [SerializeField]
    [Header("Pixel Padding")]
    private float left;
    [SerializeField]
    private float right;
    [SerializeField]
    private float top;
    [SerializeField]
    private float bottom;

    public bool childrenAreContent = true;

    [SerializeField]
    private bool sizeWidth = true;
    public bool SizeWidth => sizeWidth;
    [SerializeField]
    private bool sizeHeight = true;
    public bool SizeHeight => sizeHeight;
    [SerializeField]
    private bool moveToCenterX = true;
    public bool MoveToCenterX => moveToCenterX; 
    [SerializeField]
    private bool moveToCenterY = true;
    public bool MoveToCenterY => moveToCenterY;

    private const float pixelSize = 3.125f;
    private float Top => top * pixelSize;
    private float Left => left * pixelSize;
    private float Right => right * pixelSize;
    private float Bottom => bottom * pixelSize;

    private Dictionary<RectTransform, Vector2> rectAndPosition = new Dictionary<RectTransform, Vector2>();

    [HideInInspector]
    public Rect pendingRect;

    private void Start()
    {
        GetLayoutParent();
    }

    private void GetLayoutParent()
    {
        Transform t = transform;

        while (layoutSizerParent == null && t.parent != null)
        {
            if (t.parent.TryGetComponent(out LayoutContentSizerBaseParent parent))
            {
                layoutSizerParent = parent;
            }
            t = t.parent;
        }
    }

    

    public void MeasureLayout()
    {
        foreach(var c in content)
        {
            if(c.contentSizer != null)
                c.contentSizer.MeasureLayout();
        }

        pendingRect = GetRect();
    }

    public void ApplyLayout()
    {
        if(rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        Dictionary<RectTransform, Vector2> originalPositions = new();
        foreach (var layoutContent in content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            if (!originalPositions.ContainsKey(rTransform))
                originalPositions[rTransform] = rTransform.position;
        }

        rectTransform.position = pendingRect.position;
        rectTransform.sizeDelta = pendingRect.size;

        foreach (var kvp in originalPositions)
        {
            kvp.Key.position = kvp.Value;
        }
    }

    public void TestApplyLayout()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        Dictionary<RectTransform, Vector2> originalPositions = new();
        foreach (var layoutContent in content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            if (!originalPositions.ContainsKey(rTransform))
                originalPositions[rTransform] = rTransform.position;
        }

        Rect rect = LayoutSizerParent.GetOwnContent(rectTransform).GetRect();

        rectTransform.position = rect.position;
        rectTransform.sizeDelta = rect.size;

        foreach (var kvp in originalPositions)
        {
            kvp.Key.position = kvp.Value;
        }
    }

    public Rect GetRect()
    {
        return new Rect(GetPosition(), GetSize());
    }

    public void GetWidthAndXPosition(out float width, out float xPosition)
    {
        width = GetWidth();
        xPosition = GetXPosition();
    }

    public void GetHeightAndYPosition(out float height, out float yPosition)
    {
        height = GetHeight();
        yPosition = GetYPosition();
    }

    public Vector2 GetSize()
    {
        return new Vector2(GetWidth(), GetHeight());
    }

    public Vector2 GetPosition()
    {
        return new Vector2(GetXPosition(), GetYPosition());
    }

    public float GetWidth()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (!sizeWidth)
            return rectTransform.sizeDelta.x;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (content.Count <= 0)
        {
            return rectTransform.rect.width;
        }

        if(TryGetComponent(out LayoutGroup layoutGroup))
        {
            if (layoutGroup is  HorizontalLayoutGroup horizontalLayoutGroup)
            {
                return GetWidthIfHasHorizontalLayoutGroup(horizontalLayoutGroup);
            }
            else if (layoutGroup is VerticalLayoutGroup)
            {
                return GetWidthIfHasVerticalLayoutGroup();
            }
        }
       

        return GetShrinkWrappedWidth();
    }

    public float GetHeight()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (!sizeHeight)
            return rectTransform.sizeDelta.y;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (content.Count <= 0)
        {
            return rectTransform.rect.height;
        }

        if (TryGetComponent(out LayoutGroup layoutGroup))
        {
            if (layoutGroup is HorizontalLayoutGroup)
            {
                return GetHeightIfHasHorizontalLayoutGroup();
            }
            else if (layoutGroup is VerticalLayoutGroup verticalLayoutGroup)
            {
                return GetHeightIfHasVerticalLayoutGroup(verticalLayoutGroup);
            }
        }


        return GetShrinkWrappedHeight();
    }

    public float GetXPosition()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if(!moveToCenterX)
            return rectTransform.position.x;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (content.Count <= 0)
        {
            return rectTransform.position.x;
        }

        if (transform.parent.TryGetComponent(out LayoutGroup parentLayoutGroup) && parentLayoutGroup is HorizontalLayoutGroup or VerticalLayoutGroup)
        {
            //return GetXPositionIfControlledByLayoutGroup(parentLayoutGroup, (RectTransform)transform.parent);
        }
        
        if(TryGetComponent(out LayoutGroup layoutGroup))
        {
            if (layoutGroup is HorizontalLayoutGroup or VerticalLayoutGroup)
            {
                return rectTransform.position.x;
            }
        }

        return GetShrinkWrappedXPosition();
    }

    public float GetYPosition()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (!moveToCenterY)
            return rectTransform.position.y;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (content.Count <= 0)
        {
            return rectTransform.position.y;
        }

        if (transform.parent.TryGetComponent(out LayoutGroup parentLayoutGroup) && parentLayoutGroup is HorizontalLayoutGroup or VerticalLayoutGroup)
        {
            //return GetYPositionIfControlledByLayoutGroup(parentLayoutGroup, (RectTransform)transform.parent);
        }

        if (TryGetComponent(out LayoutGroup layoutGroup))
        {
            if (layoutGroup is HorizontalLayoutGroup or VerticalLayoutGroup)
            {
                return rectTransform.position.y;
            }
        }

        return GetShrinkWrappedYPosition();
    }

    public float GetXPositionIfControlledByLayoutGroup(LayoutGroup layoutGroup, RectTransform parentRect)
    {
        if(layoutGroup is HorizontalLayoutGroup horizontalLayoutGroup)
        {
            return HLayoutGroupSetUpChildrenNoCFENoCCSXPos(horizontalLayoutGroup, parentRect);
        }
        else if(layoutGroup is VerticalLayoutGroup verticalLayoutGroup)
        {
            return VLayoutGroupSetUpChildrenNoCFENoCCSXPos(verticalLayoutGroup, parentRect);
        }

        throw new NotImplementedException();
    }

    public float GetYPositionIfControlledByLayoutGroup(LayoutGroup layoutGroup, RectTransform parentRect)
    {
        if (layoutGroup is HorizontalLayoutGroup horizontalLayoutGroup)
        {
            return HLayoutGroupSetUpChildrenNoCFENoCCSYPos(horizontalLayoutGroup, parentRect);
        }
        else if (layoutGroup is VerticalLayoutGroup verticalLayoutGroup)
        {
            return VLayoutGroupSetUpChildrenNoCFENoCCSYPos(verticalLayoutGroup, parentRect);
        }

        throw new NotImplementedException();
    }

    private float HLayoutGroupSetUpChildrenNoCFENoCCSXPos(HorizontalLayoutGroup hLayoutGroup, RectTransform parentRect)
    {
        LayoutContent parentContent = LayoutSizerParent.GetOwnContent(parentRect);

        List<LayoutContent> orderedChildren = parentContent.Children.OrderBy(x => x.rectTransform.GetSiblingIndex()).ToList();

        float xPos = hLayoutGroup.padding.left + parentRect.position.x - parentRect.rect.size.x / 2f;

        foreach (var child in orderedChildren)
        {
            float childWidth = child.GetSize().x;

            float childXPosition = xPos + childWidth / 2f;

            if (child.contentSizer == this)
                return childXPosition;

            xPos += childWidth + hLayoutGroup.spacing;
        }

        return parentRect.position.x;
    }

    private float VLayoutGroupSetUpChildrenNoCFENoCCSYPos(VerticalLayoutGroup vLayoutGroup, RectTransform parentRect)
    {
        LayoutContent parentContent = LayoutSizerParent.GetOwnContent(parentRect);

        List<LayoutContent> orderedChildren = parentContent.Children.OrderBy(x => x.rectTransform.GetSiblingIndex()).ToList();

        float yPos = vLayoutGroup.padding.bottom + parentRect.position.y - parentRect.rect.size.y / 2f;

        foreach (var child in orderedChildren)
        {
            float childHeight = child.GetSize().y;

            float childYPosition = yPos + childHeight / 2f;

            if (child.contentSizer == this)
                return childYPosition;

            yPos += childHeight + vLayoutGroup.spacing;
        }

        return parentRect.position.y;
    }

    private float VLayoutGroupSetUpChildrenNoCFENoCCSXPos(VerticalLayoutGroup vLayoutGroup, RectTransform parentRect)
    {
        LayoutContent parentContent = LayoutSizerParent.GetOwnContent(parentRect);

        float leftPos = parentRect.position.x - parentRect.rect.size.x / 2f;
        float rightPos = parentRect.position.x + parentRect.rect.size.x / 2f;

        float childWidth = LayoutSizerParent.GetOwnContent(rectTransform).GetSize().x;

        float childXPosition = vLayoutGroup.childAlignment switch
        {
            TextAnchor.UpperLeft or TextAnchor.MiddleLeft or TextAnchor.LowerLeft =>
                leftPos + vLayoutGroup.padding.left + childWidth / 2f,

            TextAnchor.LowerCenter or TextAnchor.MiddleCenter or TextAnchor.UpperCenter =>
                parentRect.position.x,

            TextAnchor.LowerRight or TextAnchor.MiddleRight or TextAnchor.UpperRight =>
                rightPos - vLayoutGroup.padding.right - childWidth / 2f,

            _ => parentRect.position.y
        };

        return childXPosition;
    }

    private float HLayoutGroupSetUpChildrenNoCFENoCCSYPos(HorizontalLayoutGroup hLayoutGroup, RectTransform parentRect)
    {
        LayoutContent parentContent = LayoutSizerParent.GetOwnContent(parentRect);

        float bottomPos = parentRect.position.y - parentRect.rect.size.y / 2f;
        float topPos = parentRect.position.y + parentRect.rect.size.y / 2f;

        float childHeight = LayoutSizerParent.GetOwnContent(rectTransform).GetSize().y;

        float childYPosition = hLayoutGroup.childAlignment switch
        {
            TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight =>
                topPos - hLayoutGroup.padding.top - childHeight / 2f,

            TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight =>
                parentRect.position.x,

            TextAnchor.LowerLeft or TextAnchor.LowerCenter or TextAnchor.LowerRight =>
                bottomPos + hLayoutGroup.padding.bottom + childHeight / 2f,

            _ => parentRect.position.y
        };

        return childYPosition;
        
    }

    

    

    private float GetShrinkWrappedWidth()
    {
        float currentXMin = float.MaxValue;
        float currentXMax = float.MinValue;

        foreach (LayoutContent layoutContent in content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            float xMin;
            float xMax;

            if (layoutContent.contentSizer != null)
            {
                LayoutContentSizer sizer = layoutContent.contentSizer;
                sizer.GetWidthAndXPosition(out float w, out float xPos);

                xMin = xPos - w * rectTransform.lossyScale.x / 2f;
                xMax = xPos + w * rectTransform.lossyScale.x / 2f;
            }
            else
            {
                xMin = rTransform.position.x - rTransform.rect.size.x * rectTransform.lossyScale.x / 2f;
                xMax = rTransform.position.x + rTransform.rect.size.x * rectTransform.lossyScale.x / 2f;
            }

            if (currentXMin > xMin) currentXMin = xMin;
            if (currentXMax < xMax) currentXMax = xMax;
        }

        float rectWidth = currentXMax - currentXMin;

        return  rectWidth / rectTransform.lossyScale.x + Left + Right;
    }

    private float GetShrinkWrappedHeight()
    {
        float currentYMin = float.MaxValue;
        float currentYMax = float.MinValue;

        foreach (LayoutContent layoutContent in content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            float yMin;
            float yMax;

            if (layoutContent.contentSizer != null)
            {
                LayoutContentSizer sizer = layoutContent.contentSizer;
                sizer.GetHeightAndYPosition(out float h, out float yPos);

                yMin = yPos - h * rectTransform.lossyScale.y / 2f;
                yMax = yPos + h * rectTransform.lossyScale.y / 2f;
            }
            else
            {
                yMin = rTransform.position.y - rTransform.rect.size.y * rectTransform.lossyScale.y / 2f;
                yMax = rTransform.position.y + rTransform.rect.size.y * rectTransform.lossyScale.y / 2f;
            }

            if (currentYMin > yMin) currentYMin = yMin;
            if (currentYMax < yMax) currentYMax = yMax;
        }

        float rectHeight = currentYMax - currentYMin;

        return rectHeight / rectTransform.lossyScale.y + Top + Bottom;
    }

    private float GetShrinkWrappedXPosition()
    {
        float currentXMin = float.MaxValue;
        float currentXMax = float.MinValue;

        foreach (LayoutContent layoutContent in content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            float xMin;
            float xMax;

            if (layoutContent.contentSizer != null)
            {
                LayoutContentSizer sizer = layoutContent.contentSizer;
                sizer.GetWidthAndXPosition(out float w, out float xPos);

                xMin = xPos - w * rectTransform.lossyScale.x / 2f;
                xMax = xPos + w * rectTransform.lossyScale.x / 2f;
            }
            else
            {
                xMin = rTransform.position.x - rTransform.rect.size.x * rectTransform.lossyScale.x / 2f;
                xMax = rTransform.position.x + rTransform.rect.size.x * rectTransform.lossyScale.x / 2f;
            }



            if (currentXMin > xMin) currentXMin = xMin;
            if (currentXMax < xMax) currentXMax = xMax;
        }

        float rectWidth = currentXMax - currentXMin;

        return currentXMin + rectWidth / 2f + (Right - Left) * rectTransform.lossyScale.x / 2f;
    }

    private float GetShrinkWrappedYPosition()
    {
        float currentYMin = float.MaxValue;
        float currentYMax = float.MinValue;

        foreach (LayoutContent layoutContent in content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            float yMin;
            float yMax;

            if (layoutContent.contentSizer != null)
            {
                LayoutContentSizer sizer = layoutContent.contentSizer;
                sizer.GetHeightAndYPosition(out float h, out float yPos);

                yMin = yPos - h * rectTransform.lossyScale.y / 2f;
                yMax = yPos + h * rectTransform.lossyScale.y / 2f;
            }
            else
            {
                yMin = rTransform.position.y - rTransform.rect.size.y * rectTransform.lossyScale.y / 2f;
                yMax = rTransform.position.y + rTransform.rect.size.y * rectTransform.lossyScale.y / 2f;
            }

            if (currentYMin > yMin) currentYMin = yMin;
            if (currentYMax < yMax) currentYMax = yMax;
        }

        float rectHeight = currentYMax - currentYMin;

        return currentYMin + rectHeight / 2f + (Top - Bottom) * rectTransform.lossyScale.y / 2f;
    }

    private float GetWidthIfHasHorizontalLayoutGroup(HorizontalLayoutGroup horizontalLayoutGroup)
    {
        List<RectTransform> children = rectTransform.GetChildrenAtDepth(1).Where(x => x.gameObject.activeInHierarchy).ToList();

        float paddingAndSpacingWidth = horizontalLayoutGroup.padding.horizontal + (children.Count - 1) * horizontalLayoutGroup.spacing;

        float childrenWidth = paddingAndSpacingWidth;

        foreach (RectTransform child in children)
        {
            if (!child.gameObject.activeInHierarchy)
                continue;

            if (child.TryGetComponent(out LayoutContentSizer sizer))
            {
                childrenWidth += sizer.GetWidth();
            }
            else
            {
                childrenWidth += child.rect.width;
            }
        }

        return childrenWidth;
    }

    private float GetHeightIfHasVerticalLayoutGroup(VerticalLayoutGroup verticalLayoutGroup)
    {
        List<RectTransform> children = rectTransform.GetChildrenAtDepth(1).Where(x => x.gameObject.activeInHierarchy).ToList();

        float paddingAndSpacingHeight = verticalLayoutGroup.padding.vertical + (children.Count - 1) * verticalLayoutGroup.spacing;

        float childrenHeight = paddingAndSpacingHeight;

        foreach (RectTransform child in children)
        {
            if (!child.gameObject.activeInHierarchy)
                continue;

            if (child.TryGetComponent(out LayoutContentSizer sizer))
            {
                childrenHeight += sizer.GetHeight();
            }
            else
            {
                childrenHeight += child.rect.height;
            }
        }

        return childrenHeight;
    }

    private float GetWidthIfHasVerticalLayoutGroup()
    {
        float width = 0;

        foreach (LayoutContent layoutContent in Content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            float currentWidth;

            if (layoutContent.contentSizer != null)
            {
                currentWidth = layoutContent.contentSizer.GetWidth();
            }
            else
            {
                currentWidth = rTransform.rect.width;
            }

            if (currentWidth > width)
                width = currentWidth;
        }

        return width;
    }

    private float GetHeightIfHasHorizontalLayoutGroup()
    {
        float height = 0;

        foreach (LayoutContent layoutContent in Content)
        {
            RectTransform rTransform = layoutContent.rectTransform;

            float currentHeight;

            if (layoutContent.contentSizer != null)
            {
                currentHeight = layoutContent.contentSizer.GetHeight();
            }
            else
            {
                currentHeight = rTransform.rect.height;
            }

            if (currentHeight > height)
                height = currentHeight;
        }

        return height;
    }

}
