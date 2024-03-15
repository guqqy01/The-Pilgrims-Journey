using System.Collections.Generic;
using UnityEngine;
using Units;

[CreateAssetMenu()]
public class EquipmentSO : ScriptableObject
{
    public Sprite sprite;

    public List<EquipmentIntBuff> IntBuffs;
    public List<EquipmentFloatBuff> FloatBuffs;

    public int IntBaseValue(UnitIntStats intStat)
    {
        foreach (EquipmentIntBuff intBuff in IntBuffs)
        {
            if (intBuff.stat == intStat)
                return intBuff.buffBaseAmount;
        }

        return 0;
    }

    public float IntMultiplierValue(UnitIntStats intStat)
    {
        foreach (EquipmentIntBuff intBuff in IntBuffs)
        {
            if (intBuff.stat == intStat)
                return intBuff.buffMultiplierAmount;
        }

        return 0;
    }

    public float FloatBaseValue(UnitFloatStats floatStat)
    {
        foreach (EquipmentFloatBuff floatBuff in FloatBuffs)
        {
            if (floatBuff.stat == floatStat)
                return floatBuff.buffBaseAmount;
        }

        return 0;
    }

    public float FloatMultiplierValue(UnitFloatStats floatStat)
    {
        foreach (EquipmentFloatBuff floatBuff in FloatBuffs)
        {
            if (floatBuff.stat == floatStat)
                return floatBuff.buffMultiplierAmount;
        }

        return 0;
    }
}