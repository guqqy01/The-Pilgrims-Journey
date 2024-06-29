using System.Collections.Generic;
using System.Collections;
using System;

public class ParentListEnumerator<T> : IEnumerator where T : IParentListItem<T>
{
    private List<T> reversedDepthList = new List<T>();

    int position = -1;

    public T Current
    {
        get
        {
            if (position < 0 || position >= reversedDepthList.Count)
                throw new InvalidOperationException();

            return reversedDepthList[position];
        }
    }

    object IEnumerator.Current => Current;

    public ParentListEnumerator(ParentList<T> items)
    {
        reversedDepthList = items.GetDepthList();
    }

    public void Dispose() { }

    public bool MoveNext()
    {
        position++;
        return position < reversedDepthList.Count;
    }

    public void Reset()
    {
        position = -1;
    }
}