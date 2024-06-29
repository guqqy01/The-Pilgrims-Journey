using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EquipmentBuffItemUI : MonoBehaviour
{
    public Text text;

    public void Init(Buff equipmentBuff)
    {
        string textString = equipmentBuff.Stat.ToString() + ":";
        if(equipmentBuff.BaseBuff.IsInt && equipmentBuff.BaseBuff != 0)
        {
            textString += "\r\nBase: +" + equipmentBuff.BaseBuff;
        }
        else if (equipmentBuff.BaseBuff.IsFloat && equipmentBuff.BaseBuff != 0f)
        {
            textString += "\r\nBase: +" + equipmentBuff.BaseBuff;
        }
        if (equipmentBuff.Multiplier != 1f)
        {
            textString += "\r\nMultiplier: x" + equipmentBuff.Multiplier;
        }
        if(equipmentBuff.AdditionalBuff.IsInt && equipmentBuff.AdditionalBuff != 0f)
        {
            textString += "\r\nAdditional: +" + equipmentBuff.AdditionalBuff;
        }
        else if (equipmentBuff.AdditionalBuff.IsFloat && equipmentBuff.AdditionalBuff != 0f)
        {
            textString += "\r\nAdditional: +" + equipmentBuff.AdditionalBuff;
        }

        text.text = textString;
    }
}
