using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AltarUI : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private Image paymentImage;
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

    private void Start()
    {
        if (altar != null)
        {
            altar.UpdateUI += UpdateUI;
            altar.HeroRangeChange += HeroInRangeChange;
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
