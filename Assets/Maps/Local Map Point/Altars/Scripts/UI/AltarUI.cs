using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AltarUI : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private RectTransform itemTransform;
    [SerializeField]
    private Image paymentImage;
    [SerializeField]
    private RectTransform paymentTransform;
    [SerializeField]
    private TextMeshProUGUI costText;

    [Header("Key Press Hint UI")]
    [SerializeField]
    private CanvasGroup keyPressHintCanvasGroup;
    [SerializeField]
    private float timeToShowAndHide = 0.15f;

    private Coroutine _showHideCoroutine;

    [SerializeField]
    private Altar altar;

    [Header("Payment Sprites")]
    [SerializeField]
    private Sprite coinSprite;
    [SerializeField]
    private Sprite healthSprite;
    [SerializeField]
    private Sprite combatSprite;

    [SerializeField]
    private float topUIPadding = 0.3f;
    [SerializeField]
    private float bottomUIPadding = 0.1f;
    [SerializeField]
    private AltarSpawnController spawnController;


    private void Start()
    {
        if (altar != null)
        {
            altar.UpdateUI += UpdateUI;
            altar.HeroRangeChange += HeroInRangeChange;
        }

        if(spawnController != null)
        {
            spawnController.SetupUI += SetupUIPlacement;
        }

        UpdateUI();
    }


    private void OnDestroy()
    {
        if(altar != null)
        {
            altar.UpdateUI -= UpdateUI;
            altar.HeroRangeChange -= HeroInRangeChange;
        }

        if (spawnController != null)
        {
            spawnController.SetupUI -= SetupUIPlacement;
        }
    }

    public void SetupUIPlacement(float altarHeight)
    {
        Vector2 itemAnchorMaxPrevious = itemTransform.anchorMax;
        Vector2 itemAnchorMinPrevious = itemTransform.anchorMin;
        Vector2 paymentAnchorMaxPrevious = paymentTransform.anchorMax;
        Vector2 paymentAnchorMinPrevious = paymentTransform.anchorMin;

        itemTransform.anchorMax = new Vector2(0.5f, 0.5f);
        itemTransform.anchorMin = new Vector2(0.5f, 0.5f);

        paymentTransform.anchorMax = new Vector2(0.5f, 0.5f);
        paymentTransform.anchorMin = new Vector2(0.5f, 0.5f);

        float itemHalfHeight = itemTransform.rect.height / 2f;
        float paymentHalfHeight = paymentTransform.rect.height / 2f;

        itemTransform.anchoredPosition = new Vector2(0f, altarHeight / 2f + itemHalfHeight + topUIPadding);
        paymentTransform.anchoredPosition = new Vector2(0f, -(altarHeight / 2f + paymentHalfHeight + bottomUIPadding));

        itemTransform.anchorMax = itemAnchorMaxPrevious;
        itemTransform.anchorMin = itemAnchorMinPrevious;
        paymentTransform.anchorMax = paymentAnchorMaxPrevious;
        paymentTransform.anchorMin = paymentAnchorMinPrevious;
    }

    private void UpdateUI()
    {
        if (altar.InteractedWith)
        {
            itemImage.sprite = null;
            paymentImage.sprite = null;
            costText.text = "";
            gameObject.SetActive(false);
            return;
        }

        itemImage.sprite = altar.Reward.sprite;

        paymentImage.sprite = altar.AltarSO.AltarPayment switch
        {
            AltarPayment.Gold => coinSprite,
            AltarPayment.Health => healthSprite,
            AltarPayment.HealthPercentage => healthSprite,
            AltarPayment.Combat => combatSprite,
            _ => coinSprite,
        };

        costText.text = altar.Cost.ToString();

        if (altar.AltarSO.AltarPayment == AltarPayment.HealthPercentage)
            costText.text += "%";
    }

    private void HeroInRangeChange(bool inRange)
    {
        if(_showHideCoroutine != null)
            StopCoroutine(_showHideCoroutine);

        _showHideCoroutine = StartCoroutine(ShowOrHideKeyPressHint(inRange));
    }

    private IEnumerator ShowOrHideKeyPressHint(bool show)
    {
        while (true)
        {
            if (show)
            {
                if(keyPressHintCanvasGroup.alpha == 1f)
                    break;
            }
            else if(keyPressHintCanvasGroup.alpha == 0f)
            {
                break;
            }

            float sign = (show ? 1 : -1);
            float amountToChange = sign * Time.deltaTime / timeToShowAndHide;

            keyPressHintCanvasGroup.alpha = Mathf.Clamp01(keyPressHintCanvasGroup.alpha + amountToChange);

            yield return null;
        }
    }
}
