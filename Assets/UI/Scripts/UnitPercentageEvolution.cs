using UnityEngine;
using Units;
using UnityEngine.UI;

public class UnitPercentageEvolution : MonoBehaviour
{
    [SerializeField]
    private Image unitImage;

    [SerializeField]
    private GameObject questionMark;

    [SerializeField]
    private Text percentageText;

    public void Init(UnitSO unit, string percentage)
    {
        unitImage.sprite = unit.sprite;
        bool unitUnlocked = UnitManager.Instance.UnlockedUnits[unit];
        unitImage.color = unitUnlocked ? Color.white : Color.black;
        questionMark.SetActive(!unitUnlocked);

        percentageText.text = percentage + "%";
    }

    public void ChangePercentage(string percentage)
    {
        percentageText.text = percentage + "%";
    }
}
