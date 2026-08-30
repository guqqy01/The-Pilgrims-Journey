using System.Collections.Generic;

public interface IParentListItem<T> where T : IParentListItem<T>
{
    public T Parent { get; set; }

    public List<T> Children { get; set; }

    public List<T> GetAllChildren()
    {
        List<T> returnList = new List<T>();

        returnList.AddRange(Children);

        foreach(var child in Children)
        {
            returnList.AddRange(child.GetAllChildren());
        }

        return returnList;
    }

}
