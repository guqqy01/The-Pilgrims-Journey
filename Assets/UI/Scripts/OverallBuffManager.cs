using Units;
using UnityEngine;

public class OverallBuffManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static OverallBuffManager Instance { private set; get; }

    //Leveling Up
    public BuffableStat AmountOfRelicToChooseFrom = new BuffableStat(3);
    public BuffableStat Luck = new BuffableStat(0f);
    public BuffableStat ExperienceGain = new BuffableStat(1f);

    //Unit Stats
    public BuffableStat Health = new BuffableStat(0);
    public BuffableStat Shield = new BuffableStat(0);
    public BuffableStat MovementSpeed = new BuffableStat(0f);
    public BuffableStat SpawnRate = new BuffableStat(0f);
    public BuffableStat ShieldCooldown = new BuffableStat(0f);
    public BuffableStat ShieldRegeneration = new BuffableStat(0f);

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddStatBuff(string reference, UnitStatsEnum stat, Buff buff)
    {
        switch (stat)
        {
            case UnitStatsEnum.Health:
                Health.AddBuff(reference, buff);
                break;
            case UnitStatsEnum.Shield:
                Shield.AddBuff(reference, buff);
                break;
            case UnitStatsEnum.MovementSpeed:
                MovementSpeed.AddBuff(reference, buff);
                break;
            case UnitStatsEnum.SpawnRate:
                SpawnRate.AddBuff(reference, buff);
                break;
            case UnitStatsEnum.ShieldCooldown:
                ShieldCooldown.AddBuff(reference, buff);
                break;
            case UnitStatsEnum.ShieldRegeneration:
                ShieldRegeneration.AddBuff(reference, buff);
                break;
        }
    }

    public void RemoveStatBuff(string reference, UnitStatsEnum stat)
    {
        switch (stat)
        {
            case UnitStatsEnum.Health:
                Health.RemoveBuff(reference);
                break;
            case UnitStatsEnum.Shield:
                Shield.RemoveBuff(reference);
                break;
            case UnitStatsEnum.MovementSpeed:
                MovementSpeed.RemoveBuff(reference);
                break;
            case UnitStatsEnum.SpawnRate:
                SpawnRate.RemoveBuff(reference);
                break;
            case UnitStatsEnum.ShieldCooldown:
                ShieldCooldown.RemoveBuff(reference);
                break;
            case UnitStatsEnum.ShieldRegeneration:
                ShieldRegeneration.RemoveBuff(reference);
                break;
        }
    }
}
