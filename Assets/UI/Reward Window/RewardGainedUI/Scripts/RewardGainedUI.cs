using Equipment;
using Relics;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RewardGainedUI : Singleton<RewardGainedUI>
{
    [SerializeField] private Image itemImage;

    [SerializeField] private Animator glowAnimator;
    [SerializeField] private Animator itemAnimator;
    [SerializeField] private Animator textAnimator;


    [SerializeField] private TextMeshProUGUI itemText;

    [SerializeField] private float itemShowtime = 1f;

    private const string blurRequestReference = "Reward Gained UI";

    public void Setup(IDescriptable descriptable)
    {
        gameObject.SetActive(true);

        if(descriptable is EquipmentSO equipment)
        {
            Setup(equipment);
        }
        else if (descriptable is RelicSO relic)
        {
            Setup(relic);
        }
    }

    public void Setup(EquipmentSO equipmentSO)
    {
        Setup(equipmentSO.sprite, equipmentSO.name, true);
    }

    public void Setup(RelicSO relicSO)
    {
        Setup(relicSO.sprite, relicSO.name, false);
    }

    private void Setup(Sprite sprite, string itemName, bool isEquipment)
    {
        gameObject.SetActive(true);

        itemImage.sprite = sprite;
        itemText.text = itemName;

        BlurBackgroundController.Instance.EnableBlur(blurRequestReference);

        glowAnimator.SetTrigger("StartGlow");
        itemAnimator.SetTrigger("ShowItem");
        textAnimator.SetTrigger("Show");

        glowAnimator.ResetTrigger("FadeGlow");
        itemAnimator.ResetTrigger("GiveItemEquipment");
        itemAnimator.ResetTrigger("GiveItemRelics");
        textAnimator.ResetTrigger("Hide");

        StartCoroutine(WaitThenGiveItem(isEquipment));
    }

    private IEnumerator WaitThenGiveItem(bool isEquipment)
    {
        yield return new WaitForSeconds(itemShowtime);

        BlurBackgroundController.Instance.RemoveBlurRequest(blurRequestReference);

        glowAnimator.SetTrigger("FadeGlow");

        if(isEquipment)
            itemAnimator.SetTrigger("GiveItemEquipment");
        else
            itemAnimator.SetTrigger("GiveItemRelics");

        textAnimator.SetTrigger("Hide");

        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


}
