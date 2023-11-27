using UnityEngine;
using UnityEngine.UI;

public class TileUI : MonoBehaviour
{
    [SerializeField]
    private Image tileShapeImage;
    [SerializeField]
    private Image tileUnitImage;

    public RectTransform rectTransform { private set; get; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(Sprite tileShapeSprite, Sprite tileUnitSprite)
    {
        tileShapeImage.sprite = tileShapeSprite;
        tileUnitImage.sprite = tileUnitSprite;

        tileShapeImage.SetNativeSize();
        tileUnitImage.SetNativeSize();
    }

    public void SetColor(Color color)
    {
        tileShapeImage.color = color;
        tileUnitImage.color = color;
    }
}
