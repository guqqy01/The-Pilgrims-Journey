using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    public List<Upgrade> upgrades;

    public Buff GetBuff(UnitStatsEnum stat, int level)
    {
        Buff buff = new Buff(stat);

        foreach (var upgrade in upgrades)
        {
            if (upgrade.stat == stat)
            {
                Buff addBuff = upgrade.GetBuffAtLevel(level);

                try
                {
                    buff += addBuff;
                }
                catch(Exception e)
                {
                    Debug.Log(buff.Stat);
                    Debug.Log(buff.BaseBuff.IsInt);
                    Debug.Log(buff.AdditionalBuff.IsInt);

                    Debug.Log(addBuff.Stat);
                    Debug.Log(addBuff.BaseBuff.IsInt);
                    Debug.Log(addBuff.AdditionalBuff.IsInt);
                    throw e.InnerException;
                }
            }
        }

        return buff;
    }

    private void Awake()
    {
        Instance = this;
    }
}
