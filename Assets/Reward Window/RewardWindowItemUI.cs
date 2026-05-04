using UnityEngine;
using UnityEngine.UI;

public class RewardWindowItemUI : MonoBehaviour
{
    [SerializeField]
    private Image frameImage;
    [SerializeField]
    private Image objectImage;
    [SerializeField]
    private float spriteSize = 40f;

    [SerializeField]
    private Sprite unselectedSprite;
    [SerializeField]
    private Sprite selectedSprite;

    public IDescriptable descriptableObject;

    public delegate void OnSelectItem(IDescriptable descriptableObject);
    public OnSelectItem onSelectItem;

    public void Setup(IDescriptable descriptableObject)
    {
        this.descriptableObject = descriptableObject;
        objectImage.sprite = descriptableObject.Sprite;
        objectImage.NativeRatioScaleToSize(spriteSize);
    }

    public void OnClickButton()
    {
        onSelectItem?.Invoke(descriptableObject);
    }

    private void OnDestroy()
    {
        onSelectItem = null;
    }

    public void Select(bool selected)
    {
        frameImage.sprite = selected ? selectedSprite : unselectedSprite;
    }
}
