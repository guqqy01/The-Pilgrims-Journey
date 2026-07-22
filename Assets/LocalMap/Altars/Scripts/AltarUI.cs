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
    [SerializeField]
    private GameObject keyPressHintUI;


    [SerializeField]
    private Altar altar;

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
        keyPressHintUI.SetActive(inRange);
    }
}
