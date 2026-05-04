using AYellowpaper.SerializedCollections;
using UnityEngine;

public class IDescriptableUIManager : Singleton<IDescriptableUIManager>
{
    [SerializeField]
    private SerializedDictionary<IDescriptable, IDescriptable> _descriptableUIs;

    public bool TryGetDescriptableUIs(IDescriptable key, out IDescriptable descriptableUI)
    {
        bool found = _descriptableUIs.TryGetValue(key, out descriptableUI);

        if (!found)
        {
            Debug.LogWarning($"Could not find IDescriptableUI for IDescriptable: {key}");
        }

        return found;
    }
}