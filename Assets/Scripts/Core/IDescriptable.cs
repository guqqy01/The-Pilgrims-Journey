using UnityEngine;
using System.Collections.Generic;
public interface IDescriptable
{
    public string Title { get; }
    public string MainDescription { get; }
    public string ShortDescription { get; }
    public Sprite Sprite { get; }
    public List<ISubDescriptionData> GetSubDescriptionData { get; }
}

public interface ISubDescriptionData { }

