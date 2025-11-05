using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor.Rendering.LookDev;
using System.Collections;

public class LayoutContentSizerBaseParent : LayoutContentElement
{
    [SerializeField]
    private bool _setFinalPosition;
    [SerializeField]
    private Vector3 _finalAnchoredPosition;

    public ParentList<LayoutElementData> LayoutContentParentList = new ParentList<LayoutElementData>();
    public Dictionary<RectTransform, LayoutElementData> RectAndLayoutContent = new Dictionary<RectTransform, LayoutElementData>();

    public void WaitOneFrameThenUpdateLayout()
    {
        StartCoroutine(WaitThenUpdate());
    }

    private IEnumerator WaitThenUpdate()
    {
        yield return null;
        UpdateAllLayoutsDeepestFirst();
    }

    public void UpdateAllLayoutsDeepestFirst()
    {
        SetupParentList();

        var allSizers = GetComponentsInChildren<LayoutContentElement>(includeInactive: false)
            .OrderByDescending(s => GetHierarchyDepth(s.transform))
            .ToList();

        foreach (var sizer in allSizers)
        {
            ParentList<LayoutElementData> content = GetContent(sizer);

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

    public LayoutElementData GetOwnContent(RectTransform rectTransform)
    {
        foreach (var item in LayoutContentParentList)
        {
            if(item.rectTransform == rectTransform)
                return item;
        }

        return null;
    }

    public List<LayoutElementData> GetSiblings(RectTransform rectTransform)
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

        ParentList<LayoutElementData> returnContent = new ParentList<LayoutElementData>();

        List<RectTransform> rectTransformContent = new List<RectTransform>();

        rectTransformContent = GetComponentsInChildren<RectTransform>(false).ToList();

        RectAndLayoutContent = new Dictionary<RectTransform, LayoutElementData>();

        foreach (RectTransform rect in rectTransformContent)
        {
            LayoutElementData layoutContent = new LayoutElementData();

            layoutContent.rectTransform = rect;

            if(rect.TryGetComponent(out LayoutContentElement contentSizer))
            {
                layoutContent.contentSizer = contentSizer;
                contentSizer.LayoutSizerParent = this;
            }

            layoutContent.layoutGroup = rect.GetComponent<LayoutGroup>();

            returnContent.Add(layoutContent);

            RectAndLayoutContent.Add(rect, layoutContent);
        }

        foreach(LayoutElementData layoutContent in returnContent)
        {
            RectTransform rectParent = (RectTransform)layoutContent.rectTransform.parent;

            if(rectParent != null && RectAndLayoutContent.ContainsKey(rectParent))
                layoutContent.Parent = RectAndLayoutContent[rectParent];
        }

        LayoutContentParentList = returnContent;
    }

    public ParentList<LayoutElementData> GetContent(LayoutContentElement sizer)
    {
        ParentList<LayoutElementData> addContent = RectAndLayoutContent.Where(x => x.Key != null && sizer.IncludeContent.Contains(x.Key) && x.Key.gameObject.activeInHierarchy).Select(x => x.Value).ToParentList();

        if (!sizer.childrenAreContent)
            return addContent;

        List<LayoutElementData> ignoreContent = RectAndLayoutContent.Where(x => sizer.ChildrenToIgnore.Contains(x.Key)).Select(x => x.Value).ToList();

        LayoutElementData content = RectAndLayoutContent[(RectTransform)sizer.transform];

        ParentList<LayoutElementData> subList = LayoutContentParentList.GetSubParentList(content, ignoreContent, addContent);

        subList.Remove(content);

        ParentList<LayoutElementData> copyOfSubList = new ParentList<LayoutElementData>();
        copyOfSubList.AddRange(subList);

        foreach(LayoutElementData c in subList)
        {
            List<LayoutElementData> children = ((IParentListItem<LayoutElementData>)c).GetAllChildren();

            if (c.contentSizer != null)
                copyOfSubList.RemoveRange(children);
        }

        subList = new ParentList<LayoutElementData>();
        subList.AddRange(copyOfSubList);

        foreach (LayoutElementData c in addContent)
        {
            if (!subList.Contains(c))
            {
                subList.Add(c);
            }
        }

        return copyOfSubList;
    }

    /*private int GetHierarchyDepth(Transform t)
    {
        int depth = 0;
        while (t.parent != null || t == transform)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }*/

    private int GetHierarchyDepth(Transform t)
    {
        int depth = 0;
        while (t != null && t != transform)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }
}
