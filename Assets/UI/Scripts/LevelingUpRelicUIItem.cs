using Relics;
using UnityEngine;
using UnityEngine.UI;

public class LevelingUpRelicUIItem : MonoBehaviour
{
    private RelicSO relicSO;

    [SerializeField]
    private Image relicImage;
    [SerializeField]
    private Text relicName;
    [SerializeField]
    private Text LevelIncrease;
    [SerializeField]
    private Text StatIncrease;
    [SerializeField]
    private float spriteSize = 50f;

    public delegate void OnButtonClick(RelicSO relic);
    public OnButtonClick onButtonClick;

    public void ButtonClick()
    {
        onButtonClick?.Invoke(relicSO);
    }

    private void OnDestroy()
    {
        onButtonClick = null;
    }

    public void Setup(RelicSO relicSO, int levelsIncrease)
    {
        this.relicSO = relicSO;

        relicImage.sprite = relicSO.sprite;
        relicImage.NativeRatioScaleToSize(spriteSize);

        relicName.text = relicSO.name;
        

        if (RelicManager.Instance.TryGetRelic(relicSO, out Relic relic))
        {
            LevelIncrease.text = $"Level: {relic.Level} -> {relic.Level + levelsIncrease}";

            string statIncreaseString = "";

            foreach(RelicEffect effect in relic.relicEffects)
            {
                if(effect is UnitStatIncreaseRelicEffect statEffect)
                {

                }
            }
            StatIncrease.text = "";
        }
        else
        {
            LevelIncrease.text = $"Level: {levelsIncrease}";
            StatIncrease.text = "";
        }
    }

    
}