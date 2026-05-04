using System.Collections.Generic;

public interface IDescriptableUI : IDescriptable
{
    public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams { get; }

    public virtual void Select() { }
    public virtual void Deselect() { }

}