using System.Collections.Generic;
using UnityEngine;
using Units;

[CreateAssetMenu()]
public class EquipmentSO : ScriptableObject
{
    public Sprite sprite;

    public List<EquipmentIntBuff> IntBuffs;
    public List<EquipmentFloatBuff> FloatBuffs;
}

public class SpecialEquipmentSO : EquipmentSO
{
    public List<FromToUnit> unitChanges;

    public UnitSO GetUnit(UnitSO baseUnit)
    {
        foreach (FromToUnit fromToUnit in unitChanges) 
        {
            if (fromToUnit.baseUnit == baseUnit)
                return fromToUnit.toUnit;
        }

        return baseUnit;
    } 
}

public struct FromToUnit
{
    public UnitSO baseUnit;
    public UnitSO toUnit;
}