using System;
using UnityEngine;
using static UnityEngine.UI.Image;

[Serializable]
public class Buff
{
    [SerializeField]
    private UnitStatsEnum stat;
    public UnitStatsEnum Stat => stat;
    /// <summary>
    /// Multiplies the base buff: BaseBuff * Multiplier + AdditionalBuff
    /// </summary>
    public float Multiplier = 1f;

    /// <summary>
    /// Is the base buffs, get multiplied by the Multiplier: BaseBuff * Multiplier + AdditionalBuff
    /// </summary>
    public IntOrFloatObject BaseBuff;

    /// <summary>
    /// Additional buff is unaffected by multiplier: BaseBuff * Multiplier + AdditionalBuff
    /// </summary>
    public IntOrFloatObject AdditionalBuff;

    public static Buff operator +(Buff buff1, Buff buff2)
    {
        if (buff1.Stat != buff2.Stat)
            throw new ArgumentException("Can only add same buffs, Buff1: " + buff1.Stat + ", Buff2: " + buff2.Stat);

        return new Buff(buff1.Stat, buff1.Multiplier * buff2.Multiplier, buff1.BaseBuff + buff2.BaseBuff, buff1.AdditionalBuff + buff2.AdditionalBuff);
    }

    public Buff(UnitStatsEnum stat)
    {
        this.stat = stat;
        Multiplier = 1f;

        if (stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
        {
            BaseBuff = new IntOrFloatObject(0);
            AdditionalBuff = new IntOrFloatObject(0);
        }
        else
        {
            BaseBuff = new IntOrFloatObject(0f);
            AdditionalBuff = new IntOrFloatObject(0f);
        }
    }

    public Buff(UnitStatsEnum stat, float multiplier, IntOrFloatObject baseBuff, IntOrFloatObject additionalBuff)
    {
        this.stat = stat;
        Multiplier = multiplier;
        BaseBuff = baseBuff;
        AdditionalBuff = additionalBuff;
    }

    public Buff(UnitStatsEnum stat, float multiplier, object baseBuff, object additionalBuff)
    {
        this.stat = stat;
        Multiplier = multiplier;

        if(stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield)
        {
            BaseBuff = new IntOrFloatObject((int)baseBuff);
            AdditionalBuff = new IntOrFloatObject((int)additionalBuff);
        }
        else
        {
            BaseBuff = new IntOrFloatObject((float)baseBuff);
            AdditionalBuff = new IntOrFloatObject((float)additionalBuff);
        }

        
    }
    public Buff(UnitStatsEnum stat, Buff buff)
    {
        this.stat = stat;
        this.Multiplier = buff.Multiplier;
        this.BaseBuff = buff.BaseBuff;
        this.AdditionalBuff = buff.AdditionalBuff;
    }


    public virtual IntOrFloatObject Value(IntOrFloatObject baseValue)
    {
        return ((baseValue + BaseBuff) * Multiplier) + AdditionalBuff;
    }
}
