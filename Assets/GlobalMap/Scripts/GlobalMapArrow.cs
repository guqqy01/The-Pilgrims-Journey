using UnityEngine;

public class GlobalMapArrow : MonoBehaviour
{
    [SerializeField]
    private RectTransform arrow;

    public float Size
    {
        get
        {
            return arrow.localScale.x;
        }
        set
        {
            Vector3 sizeVector = new Vector3(value, value, value);
            arrow.localScale = sizeVector;

            arrow.anchoredPosition = new Vector2(-value * 5f ,value / 2f);
        }
    }
}
