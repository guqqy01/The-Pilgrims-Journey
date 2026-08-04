using UnityEngine;

public class DestroyAfterPlayHelper : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
}