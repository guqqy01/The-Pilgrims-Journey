using UnityEngine;

public static class ComponentExtensions
{
    public static bool TryGetComponentInParent<T>(this Component component, out T outComponent)
    {
        outComponent = component.GetComponentInParent<T>();
        return outComponent != null;
    }

    public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T outComponent)
    {
        outComponent = gameObject.GetComponentInParent<T>();
        return outComponent != null;
    }
}
