using UnityEngine;
using UnityEngine.UI;
public class DiscountText : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private Text discountText;

    [Header("Size")]
    [SerializeField]
    private Vector2 minAndMaxSize;
    [SerializeField]
    private float sizeChangeFrequency;
    private float sizeChangeTimer;

    [Header("Angle")]
    [SerializeField]
    private Vector2 angleRanges;
    [SerializeField]
    private float angleChangeFrequency;
    private float angleChangeTimer;

    public void SetDiscount(float discount)
    {
        if (discount <= 0)
        {
            discountText.text = "";
            gameObject.SetActive(false);
            return;
        }
        discountText.text = $"-{(int)(discount * 100)}%";
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float minSize = minAndMaxSize.Min();
        float maxSize = minAndMaxSize.Max();

        float minAngle = angleRanges.Min();
        float maxAngle = angleRanges.Max();

        sizeChangeTimer += Time.deltaTime;
        angleChangeTimer += Time.deltaTime;

        float size = ((maxSize - minSize) * (Mathf.Sin(sizeChangeTimer * sizeChangeFrequency) + 1f) / 2f) + minSize;
        float angle = ((maxAngle - minAngle) * (Mathf.Sin(angleChangeTimer * angleChangeFrequency) + 1f) / 2f) + minAngle;

        rectTransform.localScale = new Vector3(size, size, 1f);
        rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
