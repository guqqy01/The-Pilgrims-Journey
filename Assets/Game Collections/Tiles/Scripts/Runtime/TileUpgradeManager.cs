using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Units;

public class TileUpgradeManager : MonoBehaviour
{
    public static TileUpgradeManager Instance { get; private set; }

    public List<Upgrade> upgrades;

    public Buff GetBuff(StatTypes stat, int level)
    {
        bool isBuffInt = stat == StatTypes.Health || stat == StatTypes.Shield;
        Buff buff = new Buff(BuffType.Additional | BuffType.Multiplier | BuffType.Base, isBuffInt);

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
