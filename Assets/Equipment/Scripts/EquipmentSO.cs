using System.Collections.Generic;
using UnityEngine;
using Units;
using System;
using UnityEditor;
[CreateAssetMenu()]
public class EquipmentSO : ScriptableObject
{
    public Sprite sprite;

    [TextArea()]
    public string description;

    public List<EquipmentIntBuff> IntBuffs;
    public List<EquipmentFloatBuff> FloatBuffs;

    public string equipmentSpecialClass;

    [SerializeField]
    private float[] floatParameters;

    [SerializeField]
    private int[] intParameters;

    [SerializeField]
    private bool[] boolParameters;

    public object[] Parameters
    {
        get
        {
            object[] returnArray = new object[floatParameters.Length + intParameters.Length + boolParameters.Length];

            int index = 0;

            foreach (var parameter in floatParameters)
            {
                returnArray[index] = parameter;
                index++;
            }

            foreach (var parameter in intParameters)
            {
                returnArray[index] = parameter;
                index++;
            }

            foreach (var parameter in boolParameters)
            {
                returnArray[index] = parameter;
                index++;
            }

            return returnArray;
        }
    }

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

        return 1;
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

        return 1f;
    }
}
