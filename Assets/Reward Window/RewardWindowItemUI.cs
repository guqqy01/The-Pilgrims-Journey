using UnityEngine;
using UnityEngine.UI;

public class RewardWindowItemUI : MonoBehaviour
{
    [SerializeField]
    private Image objectImage;
    [SerializeField]
    private float spriteSize = 40f;


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
}
