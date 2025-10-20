using System;
using UnityEngine;

[Serializable]
public class Upgrade
{
    public int Level { get; private set; } = 0;

    public delegate void OnLevelUp(int level);
    public OnLevelUp onLevelUp;

    public UnitStatsEnum stat;
    public BuffType buffType;

    [SerializeField] private int[] intValues = new int[5];
    [SerializeField] private float[] floatValues = new float[5];

    private bool IsIntStat => stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield;

    public Buff GetBuffAtLevel(int level) => GetBuff(GetValueAtLevel(level));

    public Buff GetBuff() => GetBuff(Value);

    private Buff GetBuff(IntOrFloatObject value)
    {
        return buffType switch
        {
            BuffType.Multiplier => new Buff(stat, (float)value, 0, 0),
            BuffType.Base => new Buff(stat, 0f, value, 0),
            BuffType.Additional => new Buff(stat, 0f, 0, value),
            _ => throw new NotImplementedException()
        };
    }

    public IntOrFloatObject[] Values
    {
        get
        {
            if (IsIntStat && buffType != BuffType.Multiplier)
            {
                var objs = new IntOrFloatObject[intValues.Length];
                for (int i = 0; i < intValues.Length; i++)
                    objs[i] = new IntOrFloatObject(intValues[i]);
                return objs;
            }
            else
            {
                var objs = new IntOrFloatObject[floatValues.Length];
                for (int i = 0; i < floatValues.Length; i++)
                    objs[i] = new IntOrFloatObject(floatValues[i]);
                return objs;
            }
        }
    }

    public IntOrFloatObject Value => GetValueAtLevel(Level);

    public IntOrFloatObject NextValue
    {
        get
        {
            int max = IsIntStat && buffType != BuffType.Multiplier ? intValues.Length : floatValues.Length;
            int nextLevel = Mathf.Clamp(Level + 1, 0, max);
            return GetValueAtLevel(nextLevel);
        }
    }

    public IntOrFloatObject GetValueAtLevel(int level)
    {
        if (level == 0)
        {
            if (buffType == BuffType.Multiplier)
                return new IntOrFloatObject(0f);
            return IsIntStat ? new IntOrFloatObject(0) : new IntOrFloatObject(0f);
        }

        if (IsIntStat && buffType != BuffType.Multiplier)
            return new IntOrFloatObject(intValues[level - 1]);

        return new IntOrFloatObject(floatValues[level - 1]);
    }

    public void TryLevelUp()
    {
        int max = IsIntStat && buffType != BuffType.Multiplier ? intValues.Length : floatValues.Length;
        Level = Mathf.Clamp(Level + 1, 0, max);
        onLevelUp?.Invoke(Level);
    }
}