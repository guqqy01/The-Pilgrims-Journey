using UnityEngine;
using UnityEngine.UI;
public class RewardUIItem : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private float itemSize = 44f;

    public void Setup(Sprite sprite)
    {
        itemImage.color = itemImage.color.SetAlpha(sprite == null ? 0f : 1f);

        if (sprite == null)
            return;

        itemImage.sprite = sprite;
        itemImage.NativeRatioScaleToSize(itemSize);
    }
}
