using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor.Rendering.LookDev;

public class LayoutContentSizerBaseParent : LayoutContentSizer, ILayoutGroup
{
    [SerializeField]
    private bool _setFinalPosition;
    [SerializeField]
    private Vector3 _finalAnchoredPosition;

    public ParentList<LayoutContent> LayoutContentParentList = new ParentList<LayoutContent>();
    public Dictionary<RectTransform, LayoutContent> RectAndLayoutContent = new Dictionary<RectTransform, LayoutContent>();

    public void UpdateAllLayoutsDeepestFirst()
    {
        SetupParentList();

        var allSizers = GetComponentsInChildren<LayoutContentSizer>(includeInactive: false)
            .OrderByDescending(s => GetHierarchyDepth(s.transform))
            .ToList();

        foreach (var sizer in allSizers)
        {
            ParentList<LayoutContent> content = GetContent(sizer);

            sizer.Content = GetContent(sizer);
        }

        GetOwnContent(rectTransform).GetRect();

        foreach (var sizer in allSizers)
        {
            sizer.TestApplyLayout();
        }

        if (_setFinalPosition)
            rectTransform.anchoredPosition = _finalAnchoredPosition;
    }

    public LayoutContent GetOwnContent(RectTransform rectTransform)
    {
        foreach (var item in LayoutContentParentList)
        {
            if(item.rectTransform == rectTransform)
                return item;
        }

        return null;
    }

    public List<LayoutContent> GetSiblings(RectTransform rectTransform)
    {
        foreach(var child in LayoutContentParentList)
        {
            if (child.rectTransform == rectTransform)
                return LayoutContentParentList.GetSiblings(child);
        }

        return null;
    }

    public void SetupParentList()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        ParentList<LayoutContent> returnContent = new ParentList<LayoutContent>();

        List<RectTransform> rectTransformContent = new List<RectTransform>();

        rectTransformContent = GetComponentsInChildren<RectTransform>(false).ToList();

        RectAndLayoutContent = new Dictionary<RectTransform, LayoutContent>();

        foreach (RectTransform rect in rectTransformContent)
        {
            LayoutContent layoutContent = new LayoutContent();

            layoutContent.rectTransform = rect;

            if(rect.TryGetComponent(out LayoutContentSizer contentSizer))
            {
                layoutContent.contentSizer = contentSizer;
                contentSizer.LayoutSizerParent = this;
            }

            layoutContent.layoutGroup = rect.GetComponent<LayoutGroup>();

            returnContent.Add(layoutContent);

            RectAndLayoutContent.Add(rect, layoutContent);
        }

        foreach(LayoutContent layoutContent in returnContent)
        {
            RectTransform rectParent = (RectTransform)layoutContent.rectTransform.parent;

            if(rectParent != null && RectAndLayoutContent.ContainsKey(rectParent))
                layoutContent.Parent = RectAndLayoutContent[rectParent];
        }

        LayoutContentParentList = returnContent;
    }

    public ParentList<LayoutContent> GetContent(LayoutContentSizer sizer)
    {
        ParentList<LayoutContent> addContent = RectAndLayoutContent.Where(x => sizer.IncludeContent.Contains(x.Key) && x.Key.gameObject.activeInHierarchy).Select(x => x.Value).ToParentList();

        if (!sizer.childrenAreContent)
            return addContent;

        List<LayoutContent> ignoreContent = RectAndLayoutContent.Where(x => sizer.ChildrenToIgnore.Contains(x.Key)).Select(x => x.Value).ToList();

        LayoutContent content = RectAndLayoutContent[(RectTransform)sizer.transform];

        ParentList<LayoutContent> subList = LayoutContentParentList.GetSubParentList(content, ignoreContent, addContent);

        subList.Remove(content);

        ParentList<LayoutContent> copyOfSubList = new ParentList<LayoutContent>();
        copyOfSubList.AddRange(subList);

        foreach(LayoutContent c in subList)
        {
            List<LayoutContent> children = ((IParentListItem<LayoutContent>)c).GetAllChildren();

            if (c.contentSizer != null)
                copyOfSubList.RemoveRange(children);
        }

        subList = new ParentList<LayoutContent>();
        subList.AddRange(copyOfSubList);

        foreach (LayoutContent c in addContent)
        {
            if (!subList.Contains(c))
            {
                subList.Add(c);
            }
        }

        return copyOfSubList;
    }

    private int GetHierarchyDepth(Transform t)
    {
        int depth = 0;
        while (t.parent != null || t == transform)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }

    public void SetLayoutHorizontal()
    {
        //UpdateAllLayoutsDeepestFirst();
    }

    public void SetLayoutVertical()
    {
        //UpdateAllLayoutsDeepestFirst();
    }
}
