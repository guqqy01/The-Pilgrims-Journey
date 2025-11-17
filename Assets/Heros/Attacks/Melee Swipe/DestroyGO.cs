using UnityEngine;

public class DestroyGO : MonoBehaviour
{
    [SerializeField]
    private GameObject go;

    public void Destory()
    {
        Destroy(go);
    }
}
