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
        bool isBuffInt = stat == UnitStatsEnum.Health || stat == UnitStatsEnum.Shield;
        Buff buff = new Buff(isBuffInt);

        foreach (var upgrade in upgrades)
        {
            if (upgrade.stat == stat)
            {
                buff += upgrade.GetBuffAtLevel(level);
            }
        }

        return buff;
    }

    private void Awake()
    {
        Instance = this;
    }
}
