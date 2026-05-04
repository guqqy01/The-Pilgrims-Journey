using AYellowpaper.SerializedCollections;
using UnityEngine;

public class IDescriptableUIManager : Singleton<IDescriptableUIManager>
{
    [SerializeField]
    private SerializedDictionary<IDescriptable, IDescriptableUI> _descriptableUIs;

    public bool TryGetDescriptableUIs(IDescriptable key, out IDescriptableUI descriptableUI)
    {
        bool found = _descriptableUIs.TryGetValue(key, out descriptableUI);

        if (!found)
        {
            Debug.LogWarning($"Could not find IDescriptableUI for IDescriptable: {key}");
        }

        return found;
    }
}