using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ParentList<T> : IEnumerable<T> where T : IParentListItem<T>
{
    private readonly HashSet<T> _items = new();
    private readonly Dictionary<T, int> _depthList = new();

    public int Count => _items.Count;

    public bool Add(T item)
    {
        if (item == null || !_items.Add(item))
            return false;

        _depthList[item] = GetDepth(item);
        return true;
    }

    public void Add(T item, T parent)
    {
        if (item == null) return;

        _items.Add(item);
        item.Parent = parent;

        if (parent.Children == null)
            parent.Children = new List<T>();

        if (!parent.Children.Contains(item))
            parent.Children.Add(item);

        _depthList[item] = GetDepth(item);
    }

    public bool Remove(T item)
    {
        if (!_items.Remove(item))
            return false;

        _depthList.Remove(item);
        item.Parent?.Children?.Remove(item);
        return true;
    }

    public void Clear()
    {
        _items.Clear();
        _depthList.Clear();
    }

    public bool Contains(T item) => _items.Contains(item);

    public List<T> GetSiblings(T item)
    {
        if (item != null && item.Parent == null) return new List<T>();
        return item.Parent.Children
            .Where(child => !ReferenceEquals(child, item))
            .ToList();
    }

    public List<T> GetDepthList() =>
        _depthList.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

    public int GetDepth(T item)
    {
        if (item == null || !_items.Contains(item)) return 0;
        if (_depthList.TryGetValue(item, out var depth))
            return depth;

        var computed = ComputeDepth(item);
        _depthList[item] = computed;
        return computed;
    }

    private int ComputeDepth(T item)
    {
        var visited = new HashSet<T>();
        var depth = 0;
        var current = item;

        while (current != null && current.Parent != null && _items.Contains(current.Parent))
        {
            if (!visited.Add(current))
            {
                UnityEngine.Debug.LogWarning($"Circular parent reference detected for {current}");
                break;
            }
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    public ParentList<T> GetSubParentList(T newParent, IEnumerable<T> ignoreList, IEnumerable<T> addList)
    {
        if (newParent == null || !_items.Contains(newParent))
            return null;

        var subList = new ParentList<T>();
        var ignored = new HashSet<T>(ignoreList ?? Enumerable.Empty<T>());
        var added = new HashSet<T>(addList ?? Enumerable.Empty<T>());

        void Collect(T parent)
        {
            if (parent?.Children == null) return;

            foreach (var child in parent.Children)
            {
                if (ignored.Contains(child)) continue;
                if (subList.Contains(child)) continue;

                subList.Add(child, parent);
                Collect(child);
            }
        }

        subList.Add(newParent);
        Collect(newParent);
        foreach (var extra in added)
            Collect(extra);

        return subList;
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item);
    }

    public void RemoveRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Remove(item);
    }

    // IEnumerable implementation
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
