using UnityEngine;

public static class LayerExtensions
{
    public static bool ContainsLayer(this LayerMask layerMask, int layer)
    {
        return (layerMask & (1 << layer)) != 0;
    }
}
