using System.Collections;
using System.Collections.Generic;
using Units;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { private set; get; }

    public List<Upgrade> upgrades;

    public int UpgradePoints {  get; private set; }

    public delegate void OnUpgradePointsChange();
    public OnUpgradePointsChange onUpgradePointsChange;

    private void Awake()
    {
        Instance = this;
    }

    public void ModifyUpgradePoints(int amount)
    {
        UpgradePoints += amount;

        onUpgradePointsChange?.Invoke();
    }

    public Buff GetBuff(UnitStatsEnum stat)
    {
        Buff buff = new Buff(stat);  

        foreach (var upgrade in upgrades)
        {
            if(upgrade.stat == stat)
            {
                Buff addBuff = upgrade.GetBuff();

                buff += addBuff;
            }
        }

        return buff;
    }
}

public enum BuffType
{
    Multiplier,
    Base,
    Additional,
}