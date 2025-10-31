using Equipment;
using Relics;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class RewardGainedUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;

    [SerializeField] private Animator glowAnimator;
    [SerializeField] private Animator itemAnimator;
    [SerializeField] private Animator textAnimator;


    [SerializeField] private Text itemText;

    [SerializeField] private float itemShowtime = 1f;


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
        itemImage.sprite = sprite;
        itemText.text = itemName;

        BlurBackgroundController.Instance.EnableBlur();

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

        BlurBackgroundController.Instance.DisableBlur();

        glowAnimator.SetTrigger("FadeGlow");

        if(isEquipment)
            itemAnimator.SetTrigger("GiveItemEquipment");
        else
            itemAnimator.SetTrigger("GiveItemRelics");

        textAnimator.SetTrigger("Hide");
    }
}
