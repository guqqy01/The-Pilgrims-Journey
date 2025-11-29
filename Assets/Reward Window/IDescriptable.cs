using UnityEngine;
using System.Collections.Generic;

public interface IDescriptable
{
    public string Title { get; }

    public string MainDescription { get; }

    public Sprite Sprite { get; }

    public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams { get; }

    public virtual void Select() { }
    public virtual void Deselect() { }
}
