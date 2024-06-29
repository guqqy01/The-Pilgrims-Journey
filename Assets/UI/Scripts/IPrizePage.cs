using UnityEditor.Rendering;
using UnityEngine;

public interface IPrizePage
{
    public void Open();

    public void Close();

    public virtual GameObject gameObject
    {
        get
        {
            if(this is MonoBehaviour monoBehaviour)
            {
                return monoBehaviour.gameObject;
            }
            return null;
        }
    }
}
