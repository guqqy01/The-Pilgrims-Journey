using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class Upgrade
{
    public int Level { private set; get; } = 0;

    public delegate void OnLevelUp(int level);
    public OnLevelUp onLevelUp;

    public UnitStatsEnum stat;
    public BuffType buffType;

    [SerializeField]
    private int[] intValues = new int[5];
    [SerializeField]
    private float[] floatValues = new float[5];

    public Buff GetBuffAtLevel(int Level)
    {
        object value = GetValueAtLevel(Level);

        return GetBuff(value);
    }

    public Buff GetBuff()
    {
        return GetBuff(Value);
    }

    public Buff GetBuff(object value)
    {
        switch (buffType)
        {
            case BuffType.Multiplier:
                if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
                {
                    return new Buff(stat, (float)value, 0, 0);
                }
                else
                {
                    return new Buff(stat, (float)value, 0f, 0f);
                }
            case BuffType.Base:
                if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
                {
                    return new Buff(stat, 1f, (int)value, 0);
                }
                else
                {
                    return new Buff(stat, 1f, (float)value, 0f);
                }
            case BuffType.Additional:
                if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
                {
                    return new Buff(stat, 1f, 0, (int)value);
                }
                else
                {
                    return new Buff(stat, 1f, 0f, (float)value);
                }
            default:
                break;
        }


        throw new NotImplementedException();
    }

    public object[] Values
    {
        get
        {
            object[] objs = new object[5];

            if (buffType != BuffType.Multiplier && (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield))
            {
                for (int i = 0; i < intValues.Length; i++)
                {
                    objs[i] = intValues[i];
                }
            }
            else
            {
                for (int i = 0; i < intValues.Length; i++)
                {
                    objs[i] = floatValues[i];
                }
            }

            return objs;
        }
    }

    public object Value
    {
        get
        {
            if(Level == 0)
            {
                if(buffType == BuffType.Multiplier)
                {
                    return 1f;
                }
                else
                {
                    if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
                        return 0;

                    return 0f;
                }
            }

            if(buffType != BuffType.Multiplier && (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield))
                return intValues[Level - 1];

            return floatValues[Level - 1];
        }
    }

    public object NextValue
    {
        get
        {
            if (buffType != BuffType.Multiplier && (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield))
            {
                if(Level == 5)
                    return intValues[Level - 1];

                return intValues[Level];
            }

            if (Level == 5)
                return floatValues[Level - 1];

            return floatValues[Level];
        } 
    }

    public object GetValueAtLevel(int level)
    {
        if (level == 0)
        {
            if (buffType == BuffType.Multiplier)
            {
                return 1f;
            }
            else
            {
                if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
                    return 0;

                return 0f;
            }
        }

        if (buffType != BuffType.Multiplier && (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield))
            return intValues[level - 1];

        return floatValues[level - 1];
    }

    public void TryLevelUp()
    {
        Level = Mathf.Clamp(Level + 1, 0, intValues.Length);

        onLevelUp?.Invoke(Level);
    }
}
