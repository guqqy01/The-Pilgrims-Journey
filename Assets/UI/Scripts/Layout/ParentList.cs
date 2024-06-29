using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

[Serializable]
public class ParentList<T> : HashSet<T> where T : IParentListItem<T>
{
    private Dictionary<T, int> _depthList = new Dictionary<T, int>();

    public void Add(T item, T parent)
    {
        base.Add(item);

        if(Contains(parent))
            _depthList.Add(item, GetDepth(item));

        item.Parent = parent;

        if (parent.Children == null)
            parent.Children = new List<T>();

        if(!parent.Children.Contains(item))
            parent.Children.Add(item);
    }

    public new void Remove(T item)
    {
        base.Remove(item);

        _depthList.Remove(item);
    }

    public new void Add(T item)
    {
        base.Add(item);

        foreach(T child in item.Children)
        {
            if (Contains(child))
            {
                _depthList[child] = GetDepth(child);
            }
        }

        if(item.Parent != null && Contains(item.Parent))
        {
            _depthList.Add(item, GetDepth(item.Parent));
        }
        else
        {
            _depthList.Add(item, 0);
        }

    }

    public new void Clear()
    {
        base.Clear();

        _depthList.Clear();
    }

    public List<T> GetSiblings(T item)
    {
        List<T> returnList = new List<T>();

        if(item.Parent == null)
            return returnList;

        foreach (T t in this)
        {
            if (t.Parent == null)
                continue;

            if (t.Parent.Equals(item.Parent))
            { returnList.Add(t); }
        }

        return returnList;
    }

    public ParentList<T> GetSubParentList(T newParent, IEnumerable<T> ignoreList, IEnumerable<T> addList)
    {
        if(newParent == null) return null;

        if(!Contains(newParent)) return null;

        ParentList<T> returnParentList = new ParentList<T>() { newParent };

        returnParentList.AddRange(GetChildren(newParent));

        foreach (T item in addList)
        {
            returnParentList.AddRange(GetChildren(item));
        }

        List<T> GetChildren(T parent)
        {
            List<T> returnList = new List<T>();

            foreach (T child in parent.Children)
            {
                if (ignoreList.Contains(child))
                    continue;

                if(!returnParentList.Contains(child))
                    returnList.Add(child);

                returnList.AddRange(GetChildren(child));
            }

            return returnList;
        }

        return returnParentList;
    }

    public void AddRange(IEnumerable<T> array)
    {
        foreach (T item in array)
        {
            Add(item);
        }
    }

    public void RemoveRange(IEnumerable<T> array)
    {
        foreach (T item in array)
        {
            Remove(item);
        }
    }

    public List<T> GetDepthList()
    {
        return _depthList.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
    }

    //Depth of 0 means has no parent
    //how many stacked parents
    public int GetDepth(T item)
    {
        T current = item;
        int depth = 0;
        while(current.Parent != null && Contains(current.Parent))
        {
            depth++;
            current = current.Parent;
        }
        return depth;
    }
}
