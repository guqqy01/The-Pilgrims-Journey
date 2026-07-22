using UnityEngine;

[CreateAssetMenu(menuName = "Altar/Item")]
public class AltarSO : ScriptableObject
{
    public AltarPayment AltarPayment;
    public Sprite altarSprite;
    public Sprite brokenAltarSprite;
    public IntRangeSlider costRange;
}
